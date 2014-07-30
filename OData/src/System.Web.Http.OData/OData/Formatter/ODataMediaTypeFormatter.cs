// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
using System.Web.Http.OData.Batch;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.OData.Formatter.Serialization;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter
{
    /// <summary>
    /// <see cref="MediaTypeFormatter"/> class to handle OData.
    /// </summary>
    public class ODataMediaTypeFormatter : MediaTypeFormatter
    {
        private readonly ODataVersion _version;

        /// <summary>
        /// The set of payload kinds this formatter will accept (in CanReadType and CanWriteType).
        /// </summary>
        private readonly IEnumerable<ODataPayloadKind> _payloadKinds;

        private readonly ODataDeserializerProvider _deserializerProvider;
        private readonly ODataSerializerProvider _serializerProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataMediaTypeFormatter"/> class.
        /// </summary>
        /// <param name="payloadKinds">The kind of payloads this formatter supports.</param>
        public ODataMediaTypeFormatter(IEnumerable<ODataPayloadKind> payloadKinds)
            : this(DefaultODataDeserializerProvider.Instance, DefaultODataSerializerProvider.Instance, payloadKinds)
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

            // Maxing out the received message size as we depend on the hosting layer to enforce this limit.
            MessageWriterSettings = new ODataMessageWriterSettings
            {
                Indent = true,
                DisableMessageStreamDisposal = true,
                MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue }
            };
            MessageReaderSettings = new ODataMessageReaderSettings
            {
                DisableMessageStreamDisposal = true,
                MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue },
            };

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
            MessageWriterSettings = formatter.MessageWriterSettings;
            MessageReaderSettings = formatter.MessageReaderSettings;

            // Parameter 2: version
            _version = version;

            // Parameter 3: request
            Request = request;
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
        /// Gets the <see cref="ODataMessageQuotas"/> that this formatter uses on the read side.
        /// </summary>
        public ODataMessageQuotas MessageReaderQuotas
        {
            get
            {
                return MessageReaderSettings.MessageQuotas;
            }
        }

        /// <summary>
        /// Gets the <see cref="ODataMessageQuotas"/> that this formatter uses on the write side.
        /// </summary>
        public ODataMessageQuotas MessageWriterQuotas
        {
            get
            {
                return MessageWriterSettings.MessageQuotas;
            }
        }

        /// <summary>
        /// Gets the <see cref="ODataMessageWriterSettings"/> to be used while writing responses.
        /// </summary>
        public ODataMessageWriterSettings MessageWriterSettings { get; private set; }

        /// <summary>
        /// Gets the <see cref="ODataMessageReaderSettings"/> to be used while reading requests.
        /// </summary>
        public ODataMessageReaderSettings MessageReaderSettings { get; private set; }

        /// <summary>
        /// The request message associated with the per-request formatter instance.
        /// </summary>
        internal HttpRequestMessage Request { get; set; }

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
            // call base to validate parameters and set Content-Type header based on mediaType parameter.
            base.SetDefaultContentHeaders(type, headers, mediaType);

            headers.TryAddWithoutValidation(HttpRequestMessageProperties.ODataServiceVersionHeader, ODataUtils.ODataVersionToString(_version));
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
                IEdmModel model = Request.ODataProperties().Model;
                if (model != null)
                {
                    IEdmTypeReference expectedPayloadType;
                    ODataDeserializer deserializer = GetDeserializer(type, Request.ODataProperties().Path, model,
                        _deserializerProvider, out expectedPayloadType);
                    if (deserializer != null)
                    {
                        return _payloadKinds.Contains(deserializer.ODataPayloadKind);
                    }
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
                IEdmModel model = Request.ODataProperties().Model;
                if (model != null)
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
                        payloadKind = GetClrObjectResponsePayloadKind(type, model);
                    }

                    return payloadKind == null ? false : _payloadKinds.Contains(payloadKind.Value);
                }
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

        private ODataPayloadKind? GetClrObjectResponsePayloadKind(Type type, IEdmModel model)
        {
            // SingleResult<T> should be serialized as T.
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SingleResult<>))
            {
                type = type.GetGenericArguments()[0];
            }

            ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(model, type, Request);
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
                IEdmModel model = Request.ODataProperties().Model;
                if (model == null)
                {
                    throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
                }

                IEdmTypeReference expectedPayloadType;
                ODataDeserializer deserializer = GetDeserializer(type, Request.ODataProperties().Path, model, _deserializerProvider, out expectedPayloadType);
                if (deserializer == null)
                {
                    throw Error.Argument("type", SRResources.FormatterReadIsNotSupportedForType, type.FullName, GetType().FullName);
                }

                try
                {
                    ODataMessageReaderSettings oDataReaderSettings = new ODataMessageReaderSettings(MessageReaderSettings);
                    oDataReaderSettings.BaseUri = GetBaseAddress(Request);

                    IODataRequestMessage oDataRequestMessage = new ODataMessageWrapper(readStream, contentHeaders, Request.GetODataContentIdMapping());
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
                        RequestContext = Request.GetRequestContext(),
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
            IEdmModel model = Request.ODataProperties().Model;
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
            }

            ODataSerializer serializer = GetSerializer(type, value, model, _serializerProvider);

            UrlHelper urlHelper = Request.GetUrlHelper() ?? new UrlHelper(Request);

            ODataPath path = Request.ODataProperties().Path;
            IEdmEntitySet targetEntitySet = path == null ? null : path.EntitySet;

            // serialize a response
            HttpConfiguration configuration = Request.GetConfiguration();
            if (configuration == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustContainConfiguration);
            }

            IODataResponseMessage responseMessage = new ODataMessageWrapper(writeStream, content.Headers);

            ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings(MessageWriterSettings)
            {
                BaseUri = GetBaseAddress(Request),
                Version = _version,
            };

            // The MetadataDocumentUri is never required for errors. Additionally, it sometimes won't be available
            // for errors, such as when routing itself fails. In that case, the route data property is not
            // available on the request, and due to a bug with HttpRoute.GetVirtualPath (bug #669) we won't be able
            // to generate a metadata link.
            if (serializer.ODataPayloadKind != ODataPayloadKind.Error)
            {
                string metadataLink = urlHelper.CreateODataLink(new MetadataPathSegment());

                if (metadataLink == null)
                {
                    throw new SerializationException(SRResources.UnableToDetermineMetadataUrl);
                }

                string selectClause = GetSelectClause(Request);
                writerSettings.SetMetadataDocumentUri(new Uri(metadataLink), selectClause);
            }

            MediaTypeHeaderValue contentType = null;
            if (contentHeaders != null && contentHeaders.ContentType != null)
            {
                contentType = contentHeaders.ContentType;
            }

            using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings, model))
            {
                ODataSerializerContext writeContext = new ODataSerializerContext()
                {
                    Request = Request,
                    RequestContext = Request.GetRequestContext(),
                    Url = urlHelper,
                    EntitySet = targetEntitySet,
                    Model = model,
                    RootElementName = GetRootElementName(path) ?? "root",
                    SkipExpensiveAvailabilityChecks = serializer.ODataPayloadKind == ODataPayloadKind.Feed,
                    Path = path,
                    MetadataLevel = ODataMediaTypes.GetMetadataLevel(contentType),
                    SelectExpandClause = Request.ODataProperties().SelectExpandClause
                };

                serializer.WriteObject(value, type, messageWriter, writeContext);
            }
        }

        private static string GetSelectClause(HttpRequestMessage request)
        {
            Contract.Assert(request != null);

            if (request.ODataProperties().SelectExpandClause != null)
            {
                // Include the $select clause only if it has been applied.
                IEnumerable<KeyValuePair<string, string>> queryOptions = request.GetQueryNameValuePairs();
                return queryOptions.Where(kvp => kvp.Key == "$select").Select(kvp => kvp.Value).FirstOrDefault();
            }

            return null;
        }

        private static ODataPayloadKind? GetEdmObjectPayloadKind(Type type)
        {
            Type elementType;
            if (type.IsCollection(out elementType))
            {
                if (typeof(IEdmComplexObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Collection;
                }
                else if (typeof(IEdmEntityObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Feed;
                }
            }
            else
            {
                if (typeof(IEdmComplexObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Property;
                }
                else if (typeof(IEdmEntityObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Entry;
                }
            }

            return null;
        }

        private ODataDeserializer GetDeserializer(Type type, ODataPath path, IEdmModel model,
            ODataDeserializerProvider deserializerProvider, out IEdmTypeReference expectedPayloadType)
        {
            expectedPayloadType = GetExpectedPayloadType(type, path, model);

            // Get the deserializer using the CLR type first from the deserializer provider.
            ODataDeserializer deserializer = deserializerProvider.GetODataDeserializer(model, type, Request);
            if (deserializer == null && expectedPayloadType != null)
            {
                // we are in typeless mode, get the deserializer using the edm type from the path.
                deserializer = deserializerProvider.GetEdmTypeDeserializer(expectedPayloadType);
            }

            return deserializer;
        }

        private ODataSerializer GetSerializer(Type type, object value, IEdmModel model, ODataSerializerProvider serializerProvider)
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
                // get the most appropriate serializer given that we support inheritance.
                type = value == null ? type : value.GetType();
                serializer = serializerProvider.GetODataPayloadSerializer(model, type, Request);
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
                    ActionPathSegment actionSegment = lastSegment as ActionPathSegment;
                    if (actionSegment != null)
                    {
                        return actionSegment.Action.Name;
                    }

                    PropertyAccessPathSegment propertyAccessSegment = lastSegment as PropertyAccessPathSegment;
                    if (propertyAccessSegment != null)
                    {
                        return propertyAccessSegment.Property.Name;
                    }
                }
            }
            return null;
        }

        internal static bool TryGetInnerTypeForDelta(ref Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Delta<>))
            {
                type = type.GetGenericArguments()[0];
                return true;
            }

            return false;
        }

        internal static IEdmTypeReference GetExpectedPayloadType(Type type, ODataPath path, IEdmModel model)
        {
            IEdmTypeReference expectedPayloadType = null;

            if (typeof(IEdmObject).IsAssignableFrom(type))
            {
                // typeless mode. figure out the expected payload type from the OData Path.
                IEdmType edmType = path.EdmType;
                if (edmType != null)
                {
                    expectedPayloadType = EdmLibHelpers.ToEdmTypeReference(edmType, isNullable: false);
                    if (expectedPayloadType.TypeKind() == EdmTypeKind.Collection)
                    {
                        IEdmTypeReference elementType = expectedPayloadType.AsCollection().ElementType();
                        if (elementType.IsEntity())
                        {
                            // collection of entities cannot be CREATE/UPDATEd. Instead, the request would contain a single entry.
                            expectedPayloadType = elementType;
                        }
                    }
                }
            }
            else
            {
                TryGetInnerTypeForDelta(ref type);
                expectedPayloadType = model.GetEdmTypeReference(type);
            }

            return expectedPayloadType;
        }

        private static bool IsEntityOrFeed(IEdmTypeReference type)
        {
            Contract.Assert(type != null);
            return type.IsEntity() ||
                (type.IsCollection() && type.AsCollection().ElementType().IsEntity());
        }

        private static Uri GetBaseAddress(HttpRequestMessage request)
        {
            UrlHelper urlHelper = request.GetUrlHelper() ?? new UrlHelper(request);

            string baseAddress = urlHelper.CreateODataLink();
            if (baseAddress == null)
            {
                throw new SerializationException(SRResources.UnableToDetermineBaseUrl);
            }

            return new Uri(baseAddress);
        }

        internal static ODataVersion GetODataResponseVersion(HttpRequestMessage request)
        {
            // OData protocol requires that you send the minimum version that the client needs to know to
            // understand the response. There is no easy way we can figure out the minimum version that the client
            // needs to understand our response. We send response headers much ahead generating the response. So if
            // the requestMessage has a MaxDataServiceVersion, tell the client that our response is of the same
            // version; else use the DataServiceVersionHeader. Our response might require a higher version of the
            // client and it might fail. If the client doesn't send these headers respond with the default version
            // (V3).
            HttpRequestMessageProperties properties = request.ODataProperties();
            return properties.ODataMaxServiceVersion ??
                properties.ODataServiceVersion ??
                HttpRequestMessageProperties.DefaultODataVersion;
        }
    }
}