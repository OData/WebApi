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
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// <see cref="MediaTypeFormatter"/> class to handle OData.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling acceptable")]
    public class ODataMediaTypeFormatter : MediaTypeFormatter
    {
        private readonly ODataVersion _version;

        /// <summary>
        /// The set of payload kinds this formatter will accept (in CanReadType and CanWriteType).
        /// </summary>
        private readonly IEnumerable<ODataPayloadKind> _payloadKinds;

        private readonly ODataDeserializerProvider _deserializerProvider;
        private readonly ODataSerializerProvider _serializerProvider;

        private HttpRequestMessage _request;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataMediaTypeFormatter"/> class.
        /// </summary>
        /// <param name="payloadKinds">The kind of payloads this formatter supports.</param>
        public ODataMediaTypeFormatter(IEnumerable<ODataPayloadKind> payloadKinds)
            : this(ODataDeserializerProviderProxy.Instance, ODataSerializerProviderProxy.Instance, payloadKinds)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataMediaTypeFormatter"/> class.
        /// </summary>
        /// <param name="deserializerProvider">The <see cref="ODataDeserializerProvider"/> to use.</param>
        /// <param name="serializerProvider">The <see cref="ODataSerializerProvider"/> to use.</param>
        /// <param name="payloadKinds">The kind of payloads this formatter supports.</param>
        public ODataMediaTypeFormatter(ODataDeserializerProvider deserializerProvider, ODataSerializerProvider serializerProvider,
            IEnumerable<ODataPayloadKind> payloadKinds)
        {
            if (deserializerProvider == null)
            {
                throw Error.ArgumentNull("deserializerProvider");
            }
            if (serializerProvider == null)
            {
                throw Error.ArgumentNull("serializerProvider");
            }
            if (payloadKinds == null)
            {
                throw Error.ArgumentNull("payloadKinds");
            }

            _deserializerProvider = deserializerProvider;
            _serializerProvider = serializerProvider;
            _payloadKinds = payloadKinds;

            _version = HttpRequestMessageProperties.DefaultODataVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataMediaTypeFormatter"/> class.
        /// </summary>
        /// <param name="formatter">The <see cref="ODataMediaTypeFormatter"/> to copy settings from.</param>
        /// <param name="version">The OData version that this formatter supports.</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> for the per-request formatter instance.</param>
        /// <remarks>This is a copy constructor to be used in <see cref="GetPerRequestFormatterInstance"/>.</remarks>
        internal ODataMediaTypeFormatter(ODataMediaTypeFormatter formatter, ODataVersion version, HttpRequestMessage request)
            : base(formatter)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            Contract.Assert(formatter._serializerProvider != null);
            Contract.Assert(formatter._deserializerProvider != null);
            Contract.Assert(formatter._payloadKinds != null);

            // Parameter 1: formatter

            // Execept for the other two parameters, this constructor is a copy constructor, and we need to copy
            // everything on the other instance.

            // Copy this class's private fields and internal properties.
            _serializerProvider = formatter._serializerProvider;
            _deserializerProvider = formatter._deserializerProvider;
            _payloadKinds = formatter._payloadKinds;

            // Parameter 2: version
            _version = version;

            // Parameter 3: request
            Request = request;

            if (_serializerProvider.GetType() == typeof(ODataSerializerProviderProxy))
            {
                _serializerProvider = new ODataSerializerProviderProxy
                {
                    RequestContainer = request.GetRequestContainer()
                };
            }

            if (_deserializerProvider.GetType() == typeof(ODataDeserializerProviderProxy))
            {
                _deserializerProvider = new ODataDeserializerProviderProxy
                {
                    RequestContainer = request.GetRequestContainer()
                };
            }
        }

        /// <summary>
        /// Gets the <see cref="ODataSerializerProvider"/> that will be used by this formatter instance.
        /// </summary>
        public ODataSerializerProvider SerializerProvider
        {
            get
            {
                return _serializerProvider;
            }
        }

        /// <summary>
        /// Gets the <see cref="ODataDeserializerProvider"/> that will be used by this formatter instance.
        /// </summary>
        public ODataDeserializerProvider DeserializerProvider
        {
            get
            {
                return _deserializerProvider;
            }
        }

        /// <summary>
        /// Gets or sets a method that allows consumers to provide an alternate base
        /// address for OData Uri.
        /// </summary>
        public Func<HttpRequestMessage, Uri> BaseAddressFactory { get; set; }

        /// <summary>
        /// The request message associated with the per-request formatter instance.
        /// </summary>
        public HttpRequestMessage Request
        {
            get { return _request; }
            set
            {
                EnsureRequestContainer(value);
                _request = value;
            }
        }

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
                ODataVersion version = GetODataResponseVersion(request);
                return new ODataMediaTypeFormatter(this, version, request);
            }
        }

        /// <inheritdoc/>
        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (headers == null)
            {
                throw Error.ArgumentNull("headers");
            }

            // When the user asks for application/json we really need to set the content type to
            // application/json; odata.metadata=minimal. If the user provides the media type and is
            // application/json we are going to add automatically odata.metadata=minimal. Otherwise we are
            // going to fallback to the default implementation.

            // When calling this formatter as part of content negotiation the content negotiator will always
            // pick a non null media type. In case the user creates a new ObjectContent<T> and doesn't pass in a
            // media type, we delegate to the base class to rely on the default behavior. It's the user's
            // responsibility to pass in the right media type.

            if (mediaType != null)
            {
                if (mediaType.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) &&
                    !mediaType.Parameters.Any(p => p.Name.Equals("odata.metadata", StringComparison.OrdinalIgnoreCase)))
                {
                    mediaType.Parameters.Add(new NameValueHeaderValue("odata.metadata", "minimal"));
                }

                headers.ContentType = (MediaTypeHeaderValue)((ICloneable)mediaType).Clone();
            }
            else
            {
                // This is the case when a user creates a new ObjectContent<T> passing in a null mediaType
                base.SetDefaultContentHeaders(type, headers, mediaType);
            }

            // In general, in Web API we pick a default charset based on the supported character sets
            // of the formatter. However, according to the OData spec, the service shouldn't be sending
            // a character set unless explicitly specified, so if the client didn't send the charset we chose
            // we just clean it.
            if (headers.ContentType != null &&
                !Request.Headers.AcceptCharset
                    .Any(cs => cs.Value.Equals(headers.ContentType.CharSet, StringComparison.OrdinalIgnoreCase)))
            {
                headers.ContentType.CharSet = String.Empty;
            }

            headers.TryAddWithoutValidation(
                HttpRequestMessageProperties.ODataServiceVersionHeader,
                ODataUtils.ODataVersionToString(_version));
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
                IEdmModel model = Request.GetModel();
                IEdmTypeReference expectedPayloadType;
                ODataDeserializer deserializer = GetDeserializer(type, Request.ODataProperties().Path, model,
                    _deserializerProvider, out expectedPayloadType);
                if (deserializer != null)
                {
                    return _payloadKinds.Contains(deserializer.ODataPayloadKind);
                }
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
                ODataPayloadKind? payloadKind;

                Type elementType;
                if (typeof(IEdmObject).IsAssignableFrom(type) ||
                    (type.IsCollection(out elementType) && typeof(IEdmObject).IsAssignableFrom(elementType)))
                {
                    payloadKind = GetEdmObjectPayloadKind(type);
                }
                else
                {
                    payloadKind = GetClrObjectResponsePayloadKind(type);
                }

                return payloadKind == null ? false : _payloadKinds.Contains(payloadKind.Value);
            }

            return false;
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
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

            try
            {
                return Task.FromResult(ReadFromStream(type, readStream, content, formatterLogger));
            }
            catch (Exception ex)
            {
                return TaskHelpers.FromError<object>(ex);
            }
        }

        private ODataPayloadKind? GetClrObjectResponsePayloadKind(Type type)
        {
            // SingleResult<T> should be serialized as T.
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SingleResult<>))
            {
                type = type.GetGenericArguments()[0];
            }

            ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(type, Request);
            return serializer == null ? null : (ODataPayloadKind?)serializer.ODataPayloadKind;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "ODataMessageReader disposed later with request.")]
        private object ReadFromStream(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            object result;

            HttpContentHeaders contentHeaders = content == null ? null : content.Headers;

            // If content length is 0 then return default value for this type
            if (contentHeaders == null || contentHeaders.ContentLength == 0)
            {
                result = GetDefaultValueForType(type);
            }
            else
            {
                IEdmModel model = Request.GetModel();
                IEdmTypeReference expectedPayloadType;
                ODataDeserializer deserializer = GetDeserializer(type, Request.ODataProperties().Path, model, _deserializerProvider, out expectedPayloadType);
                if (deserializer == null)
                {
                    throw Error.Argument("type", SRResources.FormatterReadIsNotSupportedForType, type.FullName, GetType().FullName);
                }

                try
                {
                    ODataMessageReaderSettings oDataReaderSettings = Request.GetReaderSettings();
                    oDataReaderSettings.BaseUri = GetBaseAddressInternal(Request);
                    oDataReaderSettings.Validations = oDataReaderSettings.Validations & ~ValidationKinds.ThrowOnUndeclaredPropertyForNonOpenType;

                    IODataRequestMessage oDataRequestMessage = ODataMessageWrapperHelper.Create(readStream, contentHeaders, Request.GetODataContentIdMapping(), Request.GetRequestContainer());
                    ODataMessageReader oDataMessageReader = new ODataMessageReader(oDataRequestMessage, oDataReaderSettings, model);

                    Request.RegisterForDispose(oDataMessageReader);
                    ODataPath path = Request.ODataProperties().Path;
                    ODataDeserializerContext readContext = new ODataDeserializerContext
                    {
                        Path = path,
                        Model = model,
                        Request = Request,
                        ResourceType = type,
                        ResourceEdmType = expectedPayloadType,
                    };

                    result = deserializer.Read(oDataMessageReader, type, readContext);
                }
                catch (Exception e)
                {
                    if (formatterLogger == null)
                    {
                        throw;
                    }

                    formatterLogger.LogError(String.Empty, e);
                    result = GetDefaultValueForType(type);
                }
            }

            return result;
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
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

            HttpContentHeaders contentHeaders = content == null ? null : content.Headers;
            try
            {
                WriteToStream(type, value, writeStream, content, contentHeaders);
                return TaskHelpers.Completed();
            }
            catch (Exception ex)
            {
                return TaskHelpers.FromError(ex);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling acceptable")]
        private void WriteToStream(Type type, object value, Stream writeStream, HttpContent content, HttpContentHeaders contentHeaders)
        {
            IEdmModel model = Request.GetModel();
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
            }

            ODataSerializer serializer = GetSerializer(type, value, _serializerProvider);

            UrlHelper urlHelper = Request.GetUrlHelper() ?? new UrlHelper(Request);

            ODataPath path = Request.ODataProperties().Path;
            IEdmNavigationSource targetNavigationSource = path == null ? null : path.NavigationSource;

            // serialize a response
            HttpConfiguration configuration = Request.GetConfiguration();
            if (configuration == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustContainConfiguration);
            }

            string preferHeader = RequestPreferenceHelpers.GetRequestPreferHeader(new WebApiRequestHeaders(Request.Headers));
            string annotationFilter = null;
            if (!String.IsNullOrEmpty(preferHeader))
            {
                ODataMessageWrapper messageWrapper = ODataMessageWrapperHelper.Create(writeStream, content.Headers);
                messageWrapper.SetHeader(RequestPreferenceHelpers.PreferHeaderName, preferHeader);
                annotationFilter = messageWrapper.PreferHeader().AnnotationFilter;
            }

            ODataMessageWrapper responseMessageWrapper = ODataMessageWrapperHelper.Create(writeStream, content.Headers, Request.GetRequestContainer());
            IODataResponseMessage responseMessage = responseMessageWrapper;
            if (annotationFilter != null)
            {
                responseMessage.PreferenceAppliedHeader().AnnotationFilter = annotationFilter;
            }

            Uri baseAddress = GetBaseAddressInternal(Request);
            ODataMessageWriterSettings writerSettings = Request.GetWriterSettings();
            writerSettings.BaseUri = baseAddress;
            writerSettings.Version = _version;
            writerSettings.Validations = writerSettings.Validations & ~ValidationKinds.ThrowOnUndeclaredPropertyForNonOpenType;

            string metadataLink = urlHelper.CreateODataLink(MetadataSegment.Instance);

            if (metadataLink == null)
            {
                throw new SerializationException(SRResources.UnableToDetermineMetadataUrl);
            }

            writerSettings.ODataUri = new ODataUri
            {
                ServiceRoot = baseAddress,

                // TODO: 1604 Convert webapi.odata's ODataPath to ODL's ODataPath, or use ODL's ODataPath.
                SelectAndExpand = Request.ODataProperties().SelectExpandClause,
                Apply = Request.ODataProperties().ApplyClause,
                Path = (path == null || IsOperationPath(path)) ? null : path.ODLPath,
            };

            ODataMetadataLevel metadataLevel = ODataMetadataLevel.MinimalMetadata;
            if (contentHeaders != null && contentHeaders.ContentType != null)
            {
                MediaTypeHeaderValue contentType = contentHeaders.ContentType;
                IEnumerable<KeyValuePair<string, string>> parameters =
                    contentType.Parameters.Select(val => new KeyValuePair<string, string>(val.Name, val.Value));
                metadataLevel = ODataMediaTypes.GetMetadataLevel(contentType.MediaType, parameters);
            }

            using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings, model))
            {
                ODataSerializerContext writeContext = new ODataSerializerContext()
                {
                    Request = Request,
                    Url = urlHelper,
                    NavigationSource = targetNavigationSource,
                    Model = model,
                    RootElementName = GetRootElementName(path) ?? "root",
                    SkipExpensiveAvailabilityChecks = serializer.ODataPayloadKind == ODataPayloadKind.ResourceSet,
                    Path = path,
                    MetadataLevel = metadataLevel,
                    SelectExpandClause = Request.ODataProperties().SelectExpandClause
                };

                serializer.WriteObject(value, type, messageWriter, writeContext);
            }
        }

        /// <summary>
        /// This method is to get payload kind for untyped scenario.
        /// </summary>
        private ODataPayloadKind? GetEdmObjectPayloadKind(Type type)
        {
            if (ODataCountMediaTypeMapping.IsCountRequest(Request))
            {
                return ODataPayloadKind.Value;
            }

            Type elementType;
            if (type.IsCollection(out elementType))
            {
                if (typeof(IEdmComplexObject).IsAssignableFrom(elementType) || typeof(IEdmEnumObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Collection;
                }
                else if (typeof(IEdmEntityObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.ResourceSet;
                }
                else if (typeof(IEdmChangedObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Delta;
                }
            }
            else
            {
                if (typeof(IEdmComplexObject).IsAssignableFrom(elementType) || typeof(IEdmEnumObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Property;
                }
                else if (typeof(IEdmEntityObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Resource;
                }
            }

            return null;
        }

        private ODataDeserializer GetDeserializer(Type type, ODataPath path, IEdmModel model,
            ODataDeserializerProvider deserializerProvider, out IEdmTypeReference expectedPayloadType)
        {
            expectedPayloadType = EdmLibHelpers.GetExpectedPayloadType(type, path, model);

            // Get the deserializer using the CLR type first from the deserializer provider.
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(type, Request);
            if (deserializer == null && expectedPayloadType != null)
            {
                // we are in typeless mode, get the deserializer using the edm type from the path.
                deserializer = deserializerProvider.GetEdmTypeDeserializer(expectedPayloadType);
            }

            return deserializer;
        }

        private ODataSerializer GetSerializer(Type type, object value, ODataSerializerProvider serializerProvider)
        {
            ODataSerializer serializer;

            IEdmObject edmObject = value as IEdmObject;
            if (edmObject != null)
            {
                IEdmTypeReference edmType = edmObject.GetEdmType();
                if (edmType == null)
                {
                    throw new SerializationException(Error.Format(SRResources.EdmTypeCannotBeNull,
                        edmObject.GetType().FullName, typeof(IEdmObject).Name));
                }

                serializer = serializerProvider.GetEdmTypeSerializer(edmType);
                if (serializer == null)
                {
                    string message = Error.Format(SRResources.TypeCannotBeSerialized, edmType.ToTraceString(), typeof(ODataMediaTypeFormatter).Name);
                    throw new SerializationException(message);
                }
            }
            else
            {
                var applyClause = Request.ODataProperties().ApplyClause;
                // get the most appropriate serializer given that we support inheritance.
                if (applyClause == null)
                {
                    type = value == null ? type : value.GetType();
                }

                serializer = serializerProvider.GetODataPayloadSerializer(type, Request);
                if (serializer == null)
                {
                    string message = Error.Format(SRResources.TypeCannotBeSerialized, type.Name, typeof(ODataMediaTypeFormatter).Name);
                    throw new SerializationException(message);
                }
            }

            return serializer;
        }

        private static string GetRootElementName(ODataPath path)
        {
            if (path != null)
            {
                ODataPathSegment lastSegment = path.Segments.LastOrDefault();
                if (lastSegment != null)
                {
                    OperationSegment actionSegment = lastSegment as OperationSegment;
                    if (actionSegment != null)
                    {
                        IEdmAction action = actionSegment.Operations.Single() as IEdmAction;
                        if (action != null)
                        {
                            return action.Name;
                        }
                    }

                    PropertySegment propertyAccessSegment = lastSegment as PropertySegment;
                    if (propertyAccessSegment != null)
                    {
                        return propertyAccessSegment.Property.Name;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Internal method used for selecting the base address to be used with OData uris.
        /// If the consumer has provided a delegate for overriding our default implementation,
        /// we call that, otherwise we default to existing behavior below.
        /// </summary>
        /// <param name="request">The HttpRequestMessage object for the given request.</param>
        /// <returns>The base address to be used as part of the service root; must terminate with a trailing '/'.</returns>
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

        internal static ODataVersion GetODataResponseVersion(HttpRequestMessage request)
        {
            // OData protocol requires that you send the minimum version that the client needs to know to
            // understand the response. There is no easy way we can figure out the minimum version that the client
            // needs to understand our response. We send response headers much ahead generating the response. So if
            // the requestMessage has a OData-MaxVersion, tell the client that our response is of the same
            // version; else use the DataServiceVersionHeader. Our response might require a higher version of the
            // client and it might fail. If the client doesn't send these headers respond with the default version
            // (V4).
            HttpRequestMessageProperties properties = request.ODataProperties();
            return properties.ODataMaxServiceVersion ??
                properties.ODataServiceVersion ??
                HttpRequestMessageProperties.DefaultODataVersion;
        }

        // This function is used to determine whether an OData path includes operation (import) path segments.
        // We use this function to make sure the value of ODataUri.Path in ODataMessageWriterSettings is null
        // when any path segment is an operation. ODL will try to calculate the context URL if the ODataUri.Path
        // equals to null.
        private static bool IsOperationPath(ODataPath path)
        {
            if (path == null)
            {
                return false;
            }

            foreach (ODataPathSegment segment in path.Segments)
            {
                if (segment is OperationSegment ||
                    segment is OperationImportSegment)
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureRequestContainer(HttpRequestMessage request)
        {
            ODataSerializerProviderProxy serializerProviderProxy = _serializerProvider as ODataSerializerProviderProxy;
            if (serializerProviderProxy != null && serializerProviderProxy.RequestContainer == null)
            {
                serializerProviderProxy.RequestContainer = request.GetRequestContainer();
            }

            ODataDeserializerProviderProxy deserializerProviderProxy = _deserializerProvider as ODataDeserializerProviderProxy;
            if (deserializerProviderProxy != null && deserializerProviderProxy.RequestContainer == null)
            {
                deserializerProviderProxy.RequestContainer = request.GetRequestContainer();
            }
        }
    }
}