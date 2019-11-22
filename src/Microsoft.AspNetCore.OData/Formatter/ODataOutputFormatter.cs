// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
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
                return false;
            }

            // We need the type in order to write it.
            Type type = context.ObjectType ?? context.Object?.GetType();
            if (type == null)
            {
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

            return ODataOutputFormatterHelper.CanWriteType(
                type,
                _payloadKinds,
                isSingleResult,
                new WebApiRequestMessage(request),
                (objectType) => serializerProvider.GetODataPayloadSerializer(objectType, request));
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
            response.Headers[ODataVersionConstraint.ODataServiceVersionHeader] = ODataUtils.ODataVersionToString(ResultHelpers.GetODataResponseVersion(request));
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
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

            try
            {
#if !NETCOREAPP2_0
                var body = request.HttpContext.Features.Get<AspNetCore.Http.Features.IHttpBodyControlFeature>();
                if (body != null)
                {
                    body.AllowSynchronousIO = true;
                }
#endif

                HttpResponse response = context.HttpContext.Response;
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

                ODataOutputFormatterHelper.WriteToStream(
                    type,
                    context.Object,
                    request.GetModel(),
                    ResultHelpers.GetODataResponseVersion(request),
                    baseAddress,
                    contentType,
                    new WebApiUrlHelper(request.GetUrlHelper()),
                    new WebApiRequestMessage(request),
                    new WebApiRequestHeaders(request.Headers),
                    (services) => ODataMessageWrapperHelper.Create(response.Body, response.Headers, services),
                    (edmType) => serializerProvider.GetEdmTypeSerializer(edmType),
                    (objectType) => serializerProvider.GetODataPayloadSerializer(objectType, request),
                    getODataSerializerContext);

                return TaskHelpers.Completed();
            }
            catch (Exception ex)
            {
                return TaskHelpers.FromError(ex);
            }
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
}