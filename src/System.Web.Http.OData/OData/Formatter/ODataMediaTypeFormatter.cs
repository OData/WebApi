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
        private const ODataVersion DefaultODataVersion = ODataVersion.V3;
        private const string ODataMaxServiceVersion = "MaxDataServiceVersion";
        private const string ODataServiceVersion = "DataServiceVersion";

        internal const string IsODataKey = "MS_IsOData";

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
                // Encoding's public surface area is mutable, so clone (and use separate instances) to prevent changes
                // to one instance from affecting the other.
                SupportedEncodings.Add((Encoding)supportedEncoding.Clone());
            }

            foreach (MediaTypeHeaderValue supportedMediaType in formatter.SupportedMediaTypes)
            {
                // MediaTypeHeaderValue's public surface area is mutable, so clone (and use separate instances) to
                // prevent changes to one instance from affecting the other.
                SupportedMediaTypes.Add((MediaTypeHeaderValue)((ICloneable)supportedMediaType).Clone());
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

        /// <summary>
        /// Gets or sets a value indicating whether to support serialization only (whether to disable deserialization
        /// support).
        /// </summary>
        internal bool WriteOnly { get; set; }

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
                // Adds information to allow callers to identify the ODataMediaTypeFormatter through the tracing wrapper
                // This is a workaround until tracing provides information about the wrapped inner formatter
                if (type == typeof(IEdmModel))
                {
                    request.Properties[IsODataKey] = true;
                }

                ODataVersion version = GetResponseODataVersion(request);
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
                    ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(model, type);
                    if (serializer != null)
                    {
                        return _payloadKinds.Contains(serializer.ODataPayloadKind);
                    }
                }
            }

            return false;
        }

        /// <inheritdoc/>
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
                    ODataMessageReaderSettings oDataReaderSettings = new ODataMessageReaderSettings { DisableMessageStreamDisposal = true, MessageQuotas = MessageReaderQuotas };
                    try
                    {
                        IODataRequestMessage oDataRequestMessage = new ODataMessageWrapper(readStream, contentHeaders);
                        oDataMessageReader = new ODataMessageReader(oDataRequestMessage, oDataReaderSettings, model);

                        ODataPath path = _request == null ? null : _request.GetODataPath();

                        ODataDeserializerContext readContext = new ODataDeserializerContext
                        {
                            IsPatchMode = isPatchMode,
                            Path = path,
                            Model = model
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
                    finally
                    {
                        if (oDataMessageReader != null)
                        {
                            oDataMessageReader.Dispose();
                        }
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
                // get the most appropriate serializer given that we support inheritance.
                IEdmModel model = _request.GetEdmModel();
                if (model == null)
                {
                    throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
                }

                type = value == null ? type : value.GetType();
                ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(model, type);
                if (serializer == null)
                {
                    string message = Error.Format(SRResources.TypeCannotBeSerialized, type.Name, typeof(ODataMediaTypeFormatter).Name);
                    throw new SerializationException(message);
                }

                ODataPath path = _request.GetODataPath();
                IEdmEntitySet targetEntitySet = path == null ? null : path.EntitySet;

                // serialize a response
                HttpConfiguration configuration = _request.GetConfiguration();
                if (configuration == null)
                {
                    throw Error.InvalidOperation(SRResources.RequestMustContainConfiguration);
                }

                Uri baseAddress = new Uri(_request.RequestUri, configuration.VirtualPathRoot);

                IODataResponseMessage responseMessage = new ODataMessageWrapper(writeStream);

                ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings()
                {
                    BaseUri = baseAddress,
                    Version = _version,
                    Indent = true,
                    DisableMessageStreamDisposal = true,
                    MessageQuotas = MessageWriterQuotas
                };

                UrlHelper urlHelper = _request.GetUrlHelper();
                Contract.Assert(urlHelper != null);

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
                    writerSettings.SetContentType(contentType.ToString(), contentType.CharSet);
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
                        NextPageLink = _request.GetNextPageLink(),
                        InlineCount = _request.GetInlineCount()
                    };

                    serializer.WriteObject(value, messageWriter, writeContext);
                }
            });
        }

        private static ODataVersion GetResponseODataVersion(HttpRequestMessage request)
        {
            // OData protocol requires that you send the minimum version that the client needs to know to understand the response.
            // There is no easy way we can figure out the minimum version that the client needs to understand our response. We send response headers much ahead
            // generating the response. So if the requestMessage has a MaxDataServiceVersion, tell the client that our response is of the same version; Else use
            // the DataServiceVersionHeader. Our response might require a higher version of the client and it might fail.
            // If the client doesn't send these headers respond with the default version (V3).
            return GetODataVersion(request.Headers, ODataMaxServiceVersion, ODataServiceVersion) ?? DefaultODataVersion;
        }

        private static ODataVersion? GetODataVersion(HttpHeaders headers, params string[] headerNames)
        {
            foreach (string headerName in headerNames)
            {
                IEnumerable<string> values;
                if (headers.TryGetValues(headerName, out values))
                {
                    string value = values.FirstOrDefault();
                    if (value != null)
                    {
                        string trimmedValue = value.Trim(' ', ';');
                        try
                        {
                            return ODataUtils.StringToODataVersion(trimmedValue);
                        }
                        catch (ODataException)
                        {
                            // Parsing ODataVersion failed, try next header
                        }
                    }
                }
            }

            return null;
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
    }
}
