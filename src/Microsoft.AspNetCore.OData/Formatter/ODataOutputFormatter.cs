// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.OData;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// <see cref="TextOutputFormatter"/> class to handle OData.
    /// </summary>
    public class ODataOutputFormatter : TextOutputFormatter, IMediaTypeMappingCollection
    {
        /// <summary>
        /// The set of payload kinds this formatter will accept in CanWriteType.
        /// </summary>
        private readonly IEnumerable<ODataPayloadKind> _payloadKinds;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataOutputFormatter"/> class.
        /// </summary>
        /// <param name="payloadKinds">The kind of payloads this formatter supports.</param>
        public ODataOutputFormatter(IEnumerable<ODataPayloadKind> payloadKinds)
        {
            if (payloadKinds == null)
            {
                throw Error.ArgumentNull("payloadKinds");
            }

            _payloadKinds = payloadKinds;
        }

        /// <summary>
        /// Gets or sets a method that allows consumers to provide an alternate base
        /// address for OData Uri.
        /// </summary>
        public Func<HttpRequest, Uri> BaseAddressFactory { get; set; }

        /// <summary>
        /// Gets a collection of <see cref="MediaTypeMapping"/> objects.
        /// </summary>
        public ICollection<MediaTypeMapping> MediaTypeMappings { get; } = new List<MediaTypeMapping>();

        /// <inheritdoc/>
        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            // Ensure we have a valid request.
            HttpRequest request = context.HttpContext.Request;
            if (request == null)
            {
                throw Error.InvalidOperation(SRResources.ReadFromStreamAsyncMustHaveRequest);
            }

            // Ignore non-OData requests.
            if (request.ODataFeature().Path == null)
            {
                return false;
            }

            // The following base.CanWriteResult(context) will change the context.ContentType
            // If this formatter can't write the result, we should reset the context.ContentType to its original value.
            // So that, the other formatter can make a descison based on the original content type.
            // Be noted: in .NET 5, the context.ContentType is a new StringSegment everytime when goes into each formatter
            // formatterContext.ContentType = new StringSegment();
            // So, in .NET 5, we don't need to reset the contentType to backupContentType.
            StringSegment backupContentType = context.ContentType;

            // Allow the base class to make its determination, which includes
            // checks for SupportedMediaTypes.
            bool suportedMediaTypeFound = false;
            if (SupportedMediaTypes.Any())
            {
                suportedMediaTypeFound = base.CanWriteResult(context);
            }

            // See if the request satisfies any mappings.
            IEnumerable<MediaTypeMapping> matchedMappings = (MediaTypeMappings == null) ? null : MediaTypeMappings
                .Where(m => (m.TryMatchMediaType(request) > 0));

            // Now pick the best content type. If a media mapping was found, use that and override the
            // value specified by the controller, if any. Otherwise, let the base class decide.
            if (matchedMappings != null && matchedMappings.Any())
            {
                context.ContentType = matchedMappings.First().MediaType.ToString();
            }
            else if (!suportedMediaTypeFound)
            {
                context.ContentType = backupContentType;
                return false;
            }

            // We need the type in order to write it.
            Type type = context.ObjectType ?? context.Object?.GetType();
            if (type == null)
            {
                context.ContentType = backupContentType;
                return false;
            }
            type = TypeHelper.GetTaskInnerTypeOrSelf(type);

            ODataSerializerProvider serializerProvider = request.GetRequestContainer().GetRequiredService<ODataSerializerProvider>();

            // See if this type is a SingleResult or is derived from SingleResult.
            bool isSingleResult = false;
            if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();
                Type baseType = TypeHelper.GetBaseType(type);
                isSingleResult = (genericType == typeof(SingleResult<>) || baseType == typeof(SingleResult));
            }

            bool result = ODataOutputFormatterHelper.CanWriteType(
                type,
                _payloadKinds,
                isSingleResult,
                new WebApiRequestMessage(request),
                (objectType) => serializerProvider.GetODataPayloadSerializer(objectType, request));

            if (!result)
            {
                context.ContentType = backupContentType;
            }

            return result;
        }

        /// <inheritdoc/>
        public override void WriteResponseHeaders(OutputFormatterWriteContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            Type type = context.ObjectType;
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            type = TypeHelper.GetTaskInnerTypeOrSelf(type);

            HttpRequest request = context.HttpContext.Request;
            if (request == null)
            {
                throw Error.InvalidOperation(SRResources.WriteToStreamAsyncMustHaveRequest);
            }

            HttpResponse response = context.HttpContext.Response;
            response.ContentType = context.ContentType.Value;

            MediaTypeHeaderValue contentType = GetContentType(response.Headers[HeaderNames.ContentType].FirstOrDefault());

            // Determine the content type.
            MediaTypeHeaderValue newMediaType = null;
            if (ODataOutputFormatterHelper.TryGetContentHeader(type, contentType, out newMediaType))
            {
                response.Headers[HeaderNames.ContentType] = new StringValues(newMediaType.ToString());
            }

            // Set the character set.
            MediaTypeHeaderValue currentContentType = GetContentType(response.Headers[HeaderNames.ContentType].FirstOrDefault());
            RequestHeaders requestHeader = request.GetTypedHeaders();
            if (requestHeader != null && requestHeader.AcceptCharset != null)
            {
                IEnumerable<string> acceptCharsetValues = requestHeader.AcceptCharset.Select(cs => cs.Value.Value);

                string newCharSet = string.Empty;
                if (ODataOutputFormatterHelper.TryGetCharSet(currentContentType, acceptCharsetValues, out newCharSet))
                {
                    currentContentType.CharSet = newCharSet;
                    response.Headers[HeaderNames.ContentType] = new StringValues(currentContentType.ToString());
                }
            }

            // Add version header.
            response.Headers[ODataVersionConstraint.ODataServiceVersionHeader] = ODataUtils.ODataVersionToString(ResultHelpers.GetODataVersion(request));
        }

        /// <inheritdoc/>
        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            Type type = context.ObjectType;
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            type = TypeHelper.GetTaskInnerTypeOrSelf(type);

            HttpRequest request = context.HttpContext.Request;
            if (request == null)
            {
                throw Error.InvalidOperation(SRResources.WriteToStreamAsyncMustHaveRequest);
            }

            HttpResponse response = context.HttpContext.Response;
            if (typeof(Stream).IsAssignableFrom(type))
            {
                // Ideally, it should go into the "ODataRawValueSerializer",
                // However, OData lib doesn't provide the method to overwrite/copyto stream
                // So, Here's the workaround
                Stream objStream = context.Object as Stream;
                return CopyStreamAsync(objStream, response);
            }

            Uri baseAddress = GetBaseAddressInternal(request);
            MediaTypeHeaderValue contentType = GetContentType(response.Headers[HeaderNames.ContentType].FirstOrDefault());

            Func<ODataSerializerContext> getODataSerializerContext = () =>
            {
                return new ODataSerializerContext()
                {
                    Request = request,
                };
            };

            ODataSerializerProvider serializerProvider = request.GetRequestContainer().GetRequiredService<ODataSerializerProvider>();

            return ODataOutputFormatterHelper.WriteToStreamAsync(
                type,
                context.Object,
                request.GetModel(),
                ResultHelpers.GetODataVersion(request),
                baseAddress,
                contentType,
                new WebApiUrlHelper(request.GetUrlHelper()),
                new WebApiRequestMessage(request),
                new WebApiRequestHeaders(request.Headers),
                (services) => ODataMessageWrapperHelper.Create(new StreamWrapper(response.Body), response.Headers, services),
                (edmType) => serializerProvider.GetEdmTypeSerializer(edmType),
                (objectType) => serializerProvider.GetODataPayloadSerializer(objectType, request),
                getODataSerializerContext);
        }

        private static async Task CopyStreamAsync(Stream source, HttpResponse response)
        {
            if (source != null)
            {
                await source.CopyToAsync(response.Body);
            }

            await response.Body.FlushAsync();
        }

        /// <summary>
        /// Internal method used for selecting the base address to be used with OData uris.
        /// If the consumer has provided a delegate for overriding our default implementation,
        /// we call that, otherwise we default to existing behavior below.
        /// </summary>
        /// <param name="request">The HttpRequest object for the given request.</param>
        /// <returns>The base address to be used as part of the service root; must terminate with a trailing '/'.</returns>
        private Uri GetBaseAddressInternal(HttpRequest request)
        {
            if (BaseAddressFactory != null)
            {
                return BaseAddressFactory(request);
            }
            else
            {
                return ODataOutputFormatter.GetDefaultBaseAddress(request);
            }
        }

        /// <summary>
        /// Returns a base address to be used in the service root when reading or writing OData uris.
        /// </summary>
        /// <param name="request">The HttpRequest object for the given request.</param>
        /// <returns>The base address to be used as part of the service root in the OData uri; must terminate with a trailing '/'.</returns>
        public static Uri GetDefaultBaseAddress(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            string baseAddress = request.GetUrlHelper().CreateODataLink();
            if (baseAddress == null)
            {
                throw new SerializationException(SRResources.UnableToDetermineBaseUrl);
            }

            return baseAddress[baseAddress.Length - 1] != '/' ? new Uri(baseAddress + '/') : new Uri(baseAddress);
        }

        /// <inheritdoc />
        public override IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        {
            if (SupportedMediaTypes.Count == 0)
            {
                // note: this is parity with the base implementation when there are no matches
                return default;
            }

            return base.GetSupportedContentTypes(contentType, objectType);
        }

        private MediaTypeHeaderValue GetContentType(string contentTypeValue)
        {
            MediaTypeHeaderValue contentType = null;
            if (!string.IsNullOrEmpty(contentTypeValue))
            {
                MediaTypeHeaderValue.TryParse(contentTypeValue, out contentType);
            }

            return contentType;
        }
    }

    internal class StreamWrapper : Stream
    {
        private Stream stream;
        public StreamWrapper(Stream stream)
        {
            this.stream = stream;
        }

        public override bool CanRead => this.stream.CanRead;

        public override bool CanSeek => this.stream.CanSeek;

        public override bool CanWrite => this.stream.CanWrite;

        public override long Length => this.stream.Length;

        public override int ReadTimeout { get => this.stream.ReadTimeout; set => this.stream.ReadTimeout = value; }

        public override int WriteTimeout { get => this.stream.WriteTimeout; set => this.stream.WriteTimeout = value; }

        public override bool CanTimeout => this.stream.CanTimeout;

        public override void Close()
        {
            this.stream.Close();
        }

        public override long Position { get => this.stream.Position; set => this.stream.Position = value; }

        public override void Flush()
        {
            this.stream.FlushAsync().Wait();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return stream.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.stream.ReadAsync(buffer, offset, count).Result;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return this.stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override int ReadByte()
        {
            return this.stream.ReadByte();
        }

        public override void WriteByte(byte value)
        {
            this.stream.WriteByte(value);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return this.stream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.stream.WriteAsync(buffer, offset, count).Wait();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return this.stream.WriteAsync(buffer, offset, count, cancellationToken);
        }

#if !NETSTANDARD2_0
        public override void CopyTo(Stream destination, int bufferSize)
        {
            this.stream.CopyToAsync(destination, bufferSize).Wait();
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return this.stream.WriteAsync(buffer, cancellationToken);
        }

        public override int Read(Span<byte> buffer)
        {
            return this.stream.Read(buffer);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return this.stream.ReadAsync(buffer, cancellationToken);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            this.stream.Write(buffer);
        }

        public override ValueTask DisposeAsync()
        {
            return stream.DisposeAsync();
        }
#endif

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.stream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.stream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return this.stream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.stream.EndWrite(asyncResult);
        }

        public override string ToString()
        {
            return this.stream.ToString();
        }
    }
}