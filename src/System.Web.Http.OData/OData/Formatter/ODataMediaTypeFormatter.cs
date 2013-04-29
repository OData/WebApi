// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
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
using System.Text;
using System.Threading.Tasks;
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
        internal const ODataVersion DefaultODataVersion = ODataVersion.V3;
        internal const string ODataServiceVersion = "DataServiceVersion";
        private readonly ODataDeserializerProvider _deserializerProvider;
        private readonly ODataVersion _version;

        /// <summary>
        /// The set of payload kinds this formatter will accept (in CanReadType and CanWriteType).
        /// </summary>
        private readonly IEnumerable<ODataPayloadKind> _payloadKinds;

        private readonly HttpRequestMessage _request;
        private readonly ODataSerializerProvider _serializerProvider;

        internal ODataMediaTypeFormatter(IEnumerable<ODataPayloadKind> payloadKinds)
            : this(payloadKinds, request: null)
        {
        }

        internal ODataMediaTypeFormatter(ODataDeserializerProvider deserializerProvider,
            ODataSerializerProvider serializerProvider, IEnumerable<ODataPayloadKind> payloadKinds)
            : this(deserializerProvider, serializerProvider, payloadKinds, DefaultODataVersion, request: null)
        {
        }

        internal ODataMediaTypeFormatter(IEnumerable<ODataPayloadKind> payloadKinds,
            HttpRequestMessage request)
            : this(new DefaultODataDeserializerProvider(), new DefaultODataSerializerProvider(),
                payloadKinds, DefaultODataVersion, request)
        {
        }

        internal ODataMediaTypeFormatter(ODataDeserializerProvider deserializerProvider,
            ODataSerializerProvider serializerProvider,
            IEnumerable<ODataPayloadKind> payloadKinds,
            ODataVersion version,
            HttpRequestMessage request)
        {
            Contract.Assert(deserializerProvider != null);
            Contract.Assert(serializerProvider != null);
            Contract.Assert(payloadKinds != null);

            _deserializerProvider = deserializerProvider;
            _serializerProvider = serializerProvider;
            _payloadKinds = payloadKinds;
            _version = version;
            _request = request;

            // Maxing out the received message size as we depend on the hosting layer to enforce this limit.
            MessageReaderQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue };
            MessageWriterQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue };
        }

        private ODataMediaTypeFormatter(ODataMediaTypeFormatter formatter, ODataVersion version,
            HttpRequestMessage request)
        {
            Contract.Assert(formatter._serializerProvider != null);
            Contract.Assert(formatter._deserializerProvider != null);
            Contract.Assert(formatter._payloadKinds != null);
            Contract.Assert(request != null);

            // Parameter 1: formatter

            // Execept for the other two parameters, this constructor is a copy constructor, and we need to copy
            // everything on the other instance.

            // Parameter 1A: Copy this class's private fields and internal properties.
            _serializerProvider = formatter._serializerProvider;
            _deserializerProvider = formatter._deserializerProvider;
            _payloadKinds = formatter._payloadKinds;
            MessageReaderQuotas = formatter.MessageReaderQuotas;
            MessageWriterQuotas = formatter.MessageWriterQuotas;

            // Parameter 1B: Copy the base class's properties.
            foreach (MediaTypeMapping mediaTypeMapping in formatter.MediaTypeMappings)
            {
                // MediaTypeMapping doesn't support clone, and its public surface area is immutable anyway.
                MediaTypeMappings.Add(mediaTypeMapping);
            }

            RequiredMemberSelector = formatter.RequiredMemberSelector;

            foreach (Encoding supportedEncoding in formatter.SupportedEncodings)
            {
                // Per-request formatters share the encoding instances with the parent formatter
                SupportedEncodings.Add(supportedEncoding);
            }

            foreach (MediaTypeHeaderValue supportedMediaType in formatter.SupportedMediaTypes)
            {
                // Per-request formatters share the media type instances with the parent formatter
                SupportedMediaTypes.Add(supportedMediaType);
            }

            // Parameter 2: version
            _version = version;

            // Parameter 3: request
            _request = request;
        }

        /// <summary>
        /// Gets or sets the <see cref="ODataMessageQuotas"/> that this formatter uses on the read side.
        /// </summary>
        public ODataMessageQuotas MessageReaderQuotas { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ODataMessageQuotas"/> that this formatter uses on the write side.
        /// </summary>
        public ODataMessageQuotas MessageWriterQuotas { get; private set; }

        /// <inheritdoc/>
        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
        {
            // call base to validate parameters
            base.GetPerRequestFormatterInstance(type, request, mediaType);

            if (_request != null && _request == request)
            {
                // If the request is already set on this formatter, return itself.
                return this;
            }
            else
            {
                ODataVersion version = request.GetODataVersion();
                return new ODataMediaTypeFormatter(this, version, request);
            }
        }

        /// <inheritdoc/>
        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            // call base to validate parameters and set Content-Type header based on mediaType parameter.
            base.SetDefaultContentHeaders(type, headers, mediaType);

            headers.TryAddWithoutValidation(ODataServiceVersion, ODataUtils.ODataVersionToString(_version));
        }

        /// <inheritdoc/>
        public override bool CanReadType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (_request != null)
            {
                IEdmModel model = _request.GetEdmModel();
                if (model != null)
                {
                    TryGetInnerTypeForDelta(ref type);
                    ODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(model, type);

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

            if (_request != null)
            {
                IEdmModel model = _request.GetEdmModel();
                if (model != null)
                {
                    ODataPayloadKind payloadKind;

                    // TODO: We need the actual instance of the IEdmObject to figure out its EDM type which is needed to figure out the 
                    // payload kind. We cannot get access to the instance in CanWriteType unless we change the content-negotiator. This works 
                    // for now as we support writing IEdmObject's representing only entity or feed.This will have to change once we support type-less.
                    if (typeof(IEdmObject).IsAssignableFrom(type))
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(type))
                        {
                            // feed
                            payloadKind = ODataPayloadKind.Feed;
                        }
                        else
                        {
                            // entry
                            payloadKind = ODataPayloadKind.Entry;
                        }
                    }
                    else
                    {
                        // SingleResult<T> should be serialized as T.
                        if (type.IsGenericType && type.GetGenericTypeDefinition().IsEquivalentTo(typeof(SingleResult<>)))
                        {
                            type = type.GetGenericArguments()[0];
                        }

                        ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(model, type);
                        if (serializer != null)
                        {
                            payloadKind = serializer.ODataPayloadKind;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    return _payloadKinds.Contains(payloadKind);
                }
            }

            return false;
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "ODataMessageReader disposed later with request.")]
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

            if (_request == null)
            {
                throw Error.InvalidOperation(SRResources.ReadFromStreamAsyncMustHaveRequest);
            }

            return TaskHelpers.RunSynchronously<object>(() =>
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
                    IEdmModel model = _request.GetEdmModel();
                    if (model == null)
                    {
                        throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
                    }

                    Type originalType = type;
                    bool isPatchMode = TryGetInnerTypeForDelta(ref type);
                    ODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(model, type);
                    if (deserializer == null)
                    {
                        throw Error.Argument("type", SRResources.FormatterReadIsNotSupportedForType, type.FullName, GetType().FullName);
                    }

                    ODataMessageReader oDataMessageReader = null;
                    ODataMessageReaderSettings oDataReaderSettings = new ODataMessageReaderSettings
                    {
                        DisableMessageStreamDisposal = true,
                        MessageQuotas = MessageReaderQuotas,
                        BaseUri = GetBaseAddress(_request)
                    };

                    try
                    {
                        IODataRequestMessage oDataRequestMessage = new ODataMessageWrapper(readStream, contentHeaders);
                        oDataMessageReader = new ODataMessageReader(oDataRequestMessage, oDataReaderSettings, model);

                        _request.RegisterForDispose(oDataMessageReader);
                        ODataPath path = _request.GetODataPath();
                        ODataDeserializerContext readContext = new ODataDeserializerContext
                        {
                            IsPatchMode = isPatchMode,
                            Path = path,
                            Model = model,
                            Request = _request
                        };

                        if (isPatchMode)
                        {
                            readContext.PatchEntityType = originalType;
                        }

                        result = deserializer.Read(oDataMessageReader, readContext);
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
            });
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling acceptable")]
        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (writeStream == null)
            {
                throw Error.ArgumentNull("writeStream");
            }

            if (_request == null)
            {
                throw Error.InvalidOperation(SRResources.WriteToStreamAsyncMustHaveRequest);
            }

            HttpContentHeaders contentHeaders = content == null ? null : content.Headers;
            return TaskHelpers.RunSynchronously(() =>
            {
                IEdmModel model = _request.GetEdmModel();
                if (model == null)
                {
                    throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
                }

                ODataSerializer serializer = GetSerializer(type, value, model, _serializerProvider);

                UrlHelper urlHelper = _request.GetUrlHelper();
                Contract.Assert(urlHelper != null);

                ODataPath path = _request.GetODataPath();
                IEdmEntitySet targetEntitySet = path == null ? null : path.EntitySet;

                // serialize a response
                HttpConfiguration configuration = _request.GetConfiguration();
                if (configuration == null)
                {
                    throw Error.InvalidOperation(SRResources.RequestMustContainConfiguration);
                }

                IODataResponseMessage responseMessage = new ODataMessageWrapper(writeStream, content.Headers);

                ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings()
                {
                    BaseUri = GetBaseAddress(_request),
                    Version = _version,
                    Indent = true,
                    DisableMessageStreamDisposal = true,
                    MessageQuotas = MessageWriterQuotas
                };

                // The MetadataDocumentUri is never required for errors. Additionally, it sometimes won't be available
                // for errors, such as when routing itself fails. In that case, the route data property is not
                // available on the request, and due to a bug with HttpRoute.GetVirtualPath (bug #669) we won't be able
                // to generate a metadata link.
                if (serializer.ODataPayloadKind != ODataPayloadKind.Error)
                {
                    string metadataLink = urlHelper.ODataLink(new MetadataPathSegment());

                    if (metadataLink == null)
                    {
                        throw new SerializationException(SRResources.UnableToDetermineMetadataUrl);
                    }

                    writerSettings.SetMetadataDocumentUri(new Uri(metadataLink));
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
                        Request = _request,
                        Url = urlHelper,
                        EntitySet = targetEntitySet,
                        Model = model,
                        RootElementName = GetRootElementName(path) ?? "root",
                        SkipExpensiveAvailabilityChecks = serializer.ODataPayloadKind == ODataPayloadKind.Feed,
                        Path = path,
                        MetadataLevel = ODataMediaTypes.GetMetadataLevel(contentType),
                        SelectExpandClause = _request.GetSelectExpandClause()
                    };

                    serializer.WriteObject(value, messageWriter, writeContext);
                }
            });
        }

        private static ODataSerializer GetSerializer(Type type, object value, IEdmModel model, ODataSerializerProvider serializerProvider)
        {
            ODataSerializer serializer;

            IEdmObject edmObject = value as IEdmObject;
            if (edmObject != null)
            {
                IEdmTypeReference edmType = edmObject.GetEdmType();
                if (edmType == null)
                {
                    throw new SerializationException(
                        Error.Format(SRResources.EdmTypeCannotBeNull, type.FullName, typeof(IEdmObject).Name));
                }

                // we support IEdmObject for entities and feeds only right now. validate and throw.
                if (!IsEntityOrFeed(edmType))
                {
                    string message = Error.Format(
                        SRResources.EdmObjectCannotBeSerialized, typeof(ODataMediaTypeFormatter).Name, typeof(IEdmObject).Name, edmType.ToTraceString());
                    throw new SerializationException(message);
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
                serializer = serializerProvider.GetODataPayloadSerializer(model, type);
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

        private static bool IsEntityOrFeed(IEdmTypeReference type)
        {
            Contract.Assert(type != null);
            return type.IsEntity() ||
                (type.IsCollection() && type.AsCollection().ElementType().IsEntity());
        }

        private static Uri GetBaseAddress(HttpRequestMessage request)
        {
            UrlHelper urlHelper = request.GetUrlHelper();
            Contract.Assert(urlHelper != null);

            string baseAddress = urlHelper.ODataLink();
            if (baseAddress == null)
            {
                throw new SerializationException(SRResources.UnableToDetermineBaseUrl);
            }

            return new Uri(baseAddress);
        }
    }
}
