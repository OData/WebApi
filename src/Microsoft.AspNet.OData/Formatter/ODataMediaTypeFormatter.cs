// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// <see cref="MediaTypeFormatter"/> class to handle OData.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling acceptable")]
    public class ODataMediaTypeFormatter : MediaTypeFormatter
    {
        private readonly IEnumerable<ODataPayloadKind> _payloadKinds;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataMediaTypeFormatter"/> class.
        /// </summary>
        /// <param name="payloadKinds">The kind of payloads this formatter supports.</param>
        public ODataMediaTypeFormatter(IEnumerable<ODataPayloadKind> payloadKinds)
        {
            if (payloadKinds == null)
            {
                throw Error.ArgumentNull("payloadKinds");
            }

            _payloadKinds = payloadKinds;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataMediaTypeFormatter"/> class.
        /// </summary>
        /// <param name="formatter">The <see cref="ODataMediaTypeFormatter"/> to copy settings from.</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> for the per-request formatter instance.</param>
        /// <remarks>This is a copy constructor to be used in <see cref="GetPerRequestFormatterInstance"/>.</remarks>
        internal ODataMediaTypeFormatter(ODataMediaTypeFormatter formatter, HttpRequestMessage request)
            : base(formatter)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            Contract.Assert(formatter._payloadKinds != null);

            // Parameter 1: formatter
            // Except for the other two parameters, this constructor is a copy constructor, and we need to copy
            // everything on the other instance.
            _payloadKinds = formatter._payloadKinds;

            // Parameter 2: request
            Request = request;

            // BaseAddressFactory
            BaseAddressFactory = formatter.BaseAddressFactory;
        }

        /// <summary>
        /// Gets or sets a method that allows consumers to provide an alternate base
        /// address for OData Uri.
        /// </summary>
        public Func<HttpRequestMessage, Uri> BaseAddressFactory { get; set; }

        /// <summary>
        /// The request message associated with the per-request formatter instance.
        /// </summary>
        public HttpRequestMessage Request { get; set; }

        /// <inheritdoc/>
        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
        {
            // call base to validate parameters
            base.GetPerRequestFormatterInstance(type, request, mediaType);

            if (Request != null && Request == request)
            {
                // If the request is already set on this formatter, return itself.
                return this;
            }
            else
            {
                return new ODataMediaTypeFormatter(this, request);
            }
        }

        /// <inheritdoc/>
        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            // Determine the content type or let base class handle it.
            MediaTypeHeaderValue newMediaType = null;
            if (ODataOutputFormatterHelper.TryGetContentHeader(type, mediaType, out newMediaType))
            {
                headers.ContentType = newMediaType;
            }
            else
            {
                // This is the case when a user creates a new ObjectContent<T> passing in a null mediaType
                base.SetDefaultContentHeaders(type, headers, mediaType);
            }

            // Set the character set.
            IEnumerable<string> acceptCharsetValues = Request.Headers.AcceptCharset.Select(cs => cs.Value);

            string newCharSet = String.Empty;
            if (ODataOutputFormatterHelper.TryGetCharSet(headers.ContentType, acceptCharsetValues, out newCharSet))
            {
                headers.ContentType.CharSet = newCharSet;
            }

            // Add version header.
            headers.TryAddWithoutValidation(
                ODataVersionConstraint.ODataServiceVersionHeader,
                ODataUtils.ODataVersionToString(ResultHelpers.GetODataResponseVersion(Request)));
        }

        /// <inheritdoc/>
        public override bool CanReadType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (Request != null)
            {
                ODataDeserializerProvider deserializerProvider = Request.GetRequestContainer()
                    .GetRequiredService<ODataDeserializerProvider>();

                return ODataInputFormatterHelper.CanReadType(
                    type,
                    Request.GetModel(),
                    Request.ODataProperties().Path,
                    _payloadKinds,
                    (objectType) => deserializerProvider.GetEdmTypeDeserializer(objectType),
                    (objectType) => deserializerProvider.GetODataDeserializer(objectType, Request));
            }

            return false;
        }

        /// <inheritdoc/>
        public override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (Request != null)
            {
                ODataSerializerProvider serializerProvider = Request.GetRequestContainer()
                    .GetRequiredService<ODataSerializerProvider>();

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
                    new WebApiRequestMessage(Request),
                    (objectType) => serializerProvider.GetODataPayloadSerializer(objectType, Request));
            }

            return false;
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling results for making ReadFromStream platform-agnostic.")]
        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (readStream == null)
            {
                throw Error.ArgumentNull("readStream");
            }

            if (Request == null)
            {
                throw Error.InvalidOperation(SRResources.ReadFromStreamAsyncMustHaveRequest);
            }

            object defaultValue = GetDefaultValueForType(type);

            // If content length is 0 then return default value for this type
            HttpContentHeaders contentHeaders = (content == null) ? null : content.Headers;
            if (contentHeaders == null || contentHeaders.ContentLength == 0)
            {
                return Task.FromResult(defaultValue);
            }

            try
            {
                Func<ODataDeserializerContext> getODataDeserializerContext = () =>
                {
                    return new ODataDeserializerContext
                    {
                        Request = Request,
                    };
                };

                Action<Exception> logErrorAction = (ex) =>
                {
                    if (formatterLogger == null)
                    {
                        throw ex;
                    }

                    formatterLogger.LogError(String.Empty, ex);
                };

                ODataDeserializerProvider deserializerProvider = Request.GetRequestContainer()
                    .GetRequiredService<ODataDeserializerProvider>();

                return Task.FromResult(ODataInputFormatterHelper.ReadFromStream(
                    type,
                    defaultValue,
                    Request.GetModel(),
                    GetBaseAddressInternal(Request),
                    new WebApiRequestMessage(Request),
                    () => ODataMessageWrapperHelper.Create(readStream, contentHeaders, Request.GetODataContentIdMapping(), Request.GetRequestContainer()),
                    (objectType) => deserializerProvider.GetEdmTypeDeserializer(objectType),
                    (objectType) => deserializerProvider.GetODataDeserializer(objectType, Request),
                    getODataDeserializerContext,
                    (disposable) => Request.RegisterForDispose(disposable),
                    logErrorAction));
            }
            catch (Exception ex)
            {
                return TaskHelpers.FromError<object>(ex);
            }
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling results for making WriteToStream platform-agnostic.")]
        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
            TransportContext transportContext, CancellationToken cancellationToken)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (writeStream == null)
            {
                throw Error.ArgumentNull("writeStream");
            }
            if (Request == null)
            {
                throw Error.InvalidOperation(SRResources.WriteToStreamAsyncMustHaveRequest);
            }
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskHelpers.Canceled();
            }

            try
            {
                if (typeof(Stream).IsAssignableFrom(type))
                {
                    // Ideally, it should go into the "ODataRawValueSerializer",
                    // However, OData lib doesn't provide the method to overwrite/copyto stream
                    // So, Here's the workaround
                    Stream objStream = value as Stream;
                    if (objStream != null)
                    {
                        objStream.CopyToAsync(writeStream);
                        writeStream.FlushAsync();
                    }

                    return TaskHelpers.Completed();
                }

                HttpConfiguration configuration = Request.GetConfiguration();
                if (configuration == null)
                {
                    throw Error.InvalidOperation(SRResources.RequestMustContainConfiguration);
                }

                HttpContentHeaders contentHeaders = (content == null) ? null : content.Headers;
                UrlHelper urlHelper = Request.GetUrlHelper() ?? new UrlHelper(Request);

                Func<ODataSerializerContext> getODataSerializerContext = () =>
                {
                    return new ODataSerializerContext()
                    {
                        Request = Request,
                        Url = urlHelper,
                    };
                };

                ODataSerializerProvider serializerProvider = Request.GetRequestContainer()
                    .GetRequiredService<ODataSerializerProvider>();

                ODataOutputFormatterHelper.WriteToStream(
                    type,
                    value,
                    Request.GetModel(),
                    ResultHelpers.GetODataResponseVersion(Request),
                    GetBaseAddressInternal(Request),
                    contentHeaders == null ? null : contentHeaders.ContentType,
                    new WebApiUrlHelper(urlHelper),
                    new WebApiRequestMessage(Request),
                    new WebApiRequestHeaders(Request.Headers),
                    (services) => ODataMessageWrapperHelper.Create(writeStream, contentHeaders, services),
                    (edmType) => serializerProvider.GetEdmTypeSerializer(edmType),
                    (objectType) => serializerProvider.GetODataPayloadSerializer(objectType, Request),
                    getODataSerializerContext);

                return TaskHelpers.Completed();
            }
            catch (Exception ex)
            {
                return TaskHelpers.FromError(ex);
            }
        }

        // To factor out request, just pass in a function to get base address. We'd get rid of
        // BaseAddressFactory and request.
        private Uri GetBaseAddressInternal(HttpRequestMessage request)
        {
            if (BaseAddressFactory != null)
            {
                return BaseAddressFactory(request);
            }
            else
            {
                return ODataMediaTypeFormatter.GetDefaultBaseAddress(request);
            }
        }

        /// <summary>
        /// Returns a base address to be used in the service root when reading or writing OData uris.
        /// </summary>
        /// <param name="request">The HttpRequestMessage object for the given request.</param>
        /// <returns>The base address to be used as part of the service root in the OData uri; must terminate with a trailing '/'.</returns>
        public static Uri GetDefaultBaseAddress(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            UrlHelper urlHelper = request.GetUrlHelper() ?? new UrlHelper(request);

            string baseAddress = urlHelper.CreateODataLink();
            if (baseAddress == null)
            {
                throw new SerializationException(SRResources.UnableToDetermineBaseUrl);
            }

            return baseAddress[baseAddress.Length - 1] != '/' ? new Uri(baseAddress + '/') : new Uri(baseAddress);
        }
    }
}