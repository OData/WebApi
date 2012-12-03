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
        private const string ElementNameDefault = "root";
        internal const string EdmModelKey = "MS_EdmModel";

        private readonly ODataDeserializerProvider _deserializerProvider;
        private readonly IEdmModel _model;
        private readonly ODataVersion _version;

        /// <summary>
        /// The set of payload kinds this formatter will accept (in CanReadType and CanWriteType).
        /// </summary>
        private readonly IEnumerable<ODataPayloadKind> _payloadKinds;

        private readonly HttpRequestMessage _request;
        private readonly ODataSerializerProvider _serializerProvider;

        internal ODataMediaTypeFormatter(IEdmModel model, IEnumerable<ODataPayloadKind> payloadKinds)
            : this(model, payloadKinds, request: null)
        {
        }

        internal ODataMediaTypeFormatter(IEdmModel model, IEnumerable<ODataPayloadKind> payloadKinds,
            HttpRequestMessage request)
            : this(new DefaultODataDeserializerProvider(model), new DefaultODataSerializerProvider(model),
                payloadKinds, ODataFormatterConstants.DefaultODataVersion, request)
        {
        }

        private ODataMediaTypeFormatter(ODataDeserializerProvider deserializerProvider,
            ODataSerializerProvider serializerProvider,
            IEnumerable<ODataPayloadKind> payloadKinds,
            ODataVersion version,
            HttpRequestMessage request)
        {
            Contract.Assert(deserializerProvider != null);
            Contract.Assert(deserializerProvider.EdmModel != null);
            Contract.Assert(serializerProvider != null);
            Contract.Assert(payloadKinds != null);

            _deserializerProvider = deserializerProvider;
            _model = deserializerProvider.EdmModel;
            _serializerProvider = serializerProvider;
            _payloadKinds = payloadKinds;
            _version = version;
            _request = request;
        }

        private ODataMediaTypeFormatter(ODataMediaTypeFormatter formatter, ODataVersion version,
            HttpRequestMessage request)
        {
            Contract.Assert(formatter._serializerProvider != null);
            Contract.Assert(formatter._model != null);
            Contract.Assert(formatter._deserializerProvider != null);
            Contract.Assert(formatter._payloadKinds != null);
            Contract.Assert(request != null);

            // Parameter 1: formatter

            // Execept for the other two parameters, this constructor is a copy constructor, and we need to copy
            // everything on the other instance.

            // Parameter 1A: Copy this class's private fields and internal properties.
            _serializerProvider = formatter._serializerProvider;
            _model = formatter._model;
            _deserializerProvider = formatter._deserializerProvider;
            _payloadKinds = formatter._payloadKinds;
            WriteOnly = formatter.WriteOnly;

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
        /// The <see cref="IEdmModel"/> used by this formatter.
        /// </summary>
        public IEdmModel Model
        {
            get
            {
                return _model;
            }
        }

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

            // Adds model information to allow callers to identify the ODataMediaTypeFormatter through the tracing wrapper
            // This is a workaround until tracing provides information about the wrapped inner formatter
            if (type == typeof(IEdmModel))
            {
                request.Properties.Add(EdmModelKey, _model);
            }

            ODataVersion version = GetResponseODataVersion(request);
            return new ODataMediaTypeFormatter(this, version, request);
        }

        /// <inheritdoc/>
        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            // call base to validate parameters and set Content-Type header based on mediaType parameter.
            base.SetDefaultContentHeaders(type, headers, mediaType);

            headers.TryAddWithoutValidation(ODataFormatterConstants.ODataServiceVersion,
                ODataUtils.ODataVersionToString(_version) + ";");
        }

        /// <inheritdoc/>
        public override bool CanReadType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            // TODO: Feature #664 - Remove this logic (and property) once JSON light read support is available.
            if (WriteOnly)
            {
                return false;
            }

            TryGetInnerTypeForDelta(ref type);
            ODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(type);

            if (deserializer == null)
            {
                return false;
            }

            return _payloadKinds.Contains(deserializer.ODataPayloadKind);
        }

        /// <inheritdoc/>
        public override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(type);

            if (serializer == null)
            {
                return false;
            }

            return _payloadKinds.Contains(serializer.ODataPayloadKind);
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

            return TaskHelpers.RunSynchronously<object>(() =>
            {
                object result;

                HttpContentHeaders contentHeaders = content == null ? null : content.Headers;
                // If content length is 0 then return default value for this type
                if (contentHeaders != null && contentHeaders.ContentLength == 0)
                {
                    result = GetDefaultValueForType(type);
                }
                else
                {
                    Type originalType = type;
                    bool isPatchMode = TryGetInnerTypeForDelta(ref type);
                    ODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(type);
                    if (deserializer == null)
                    {
                        throw Error.Argument("type", SRResources.FormatterReadIsNotSupportedForType, type.FullName, GetType().FullName);
                    }

                    ODataMessageReader oDataMessageReader = null;
                    ODataMessageReaderSettings oDataReaderSettings = new ODataMessageReaderSettings { DisableMessageStreamDisposal = true };
                    try
                    {
                        IODataRequestMessage oDataRequestMessage = new ODataMessageWrapper(readStream, contentHeaders);
                        oDataMessageReader = new ODataMessageReader(oDataRequestMessage, oDataReaderSettings, _deserializerProvider.EdmModel);
                        ODataDeserializerContext readContext = new ODataDeserializerContext { IsPatchMode = isPatchMode, Request = _request, Model = _model };
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
                throw Error.NotSupported(SRResources.WriteToStreamAsyncMustHaveRequest);
            }

            HttpContentHeaders contentHeaders = content == null ? null : content.Headers;
            return TaskHelpers.RunSynchronously(() =>
            {
                // get the most appropriate serializer given that we support inheritance.
                type = value == null ? type : value.GetType();
                ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(type);
                if (serializer == null)
                {
                    throw Error.InvalidOperation(SRResources.TypeCannotBeSerialized, type.Name, typeof(ODataMediaTypeFormatter).Name);
                }

                UrlHelper urlHelper = _request.GetUrlHelper();

                ODataPath path = _request.GetODataPath();
                IEdmEntitySet targetEntitySet = path == null ? null : path.EntitySet;

                // serialize a response
                HttpConfiguration configuration = _request.GetConfiguration();
                Uri baseAddress = new Uri(_request.RequestUri, configuration.VirtualPathRoot);

                IODataResponseMessage responseMessage = new ODataMessageWrapper(writeStream);

                ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings()
                {
                    BaseUri = baseAddress,
                    Version = _version,
                    Indent = true,
                    DisableMessageStreamDisposal = true
                };

                IODataPathHandler pathHandler = configuration.GetODataPathHandler() ??
                    new DefaultODataPathHandler(_model);

                // The MetadataDocumentUri is never required for errors. Additionally, it sometimes won't be available
                // for errors, such as when routing itself fails. In that case, the route data property is not
                // available on the request, and due to a bug with HttpRoute.GetVirtualPath (bug #669) we won't be able
                // to generate a metadata link.
                if (serializer.ODataPayloadKind != ODataPayloadKind.Error)
                {
                    string metadataLink = urlHelper.ODataLink(pathHandler, new MetadataPathSegment());

                    if (metadataLink == null)
                    {
                        throw Error.InvalidOperation(SRResources.UnableToDetermineMetadataUrl);
                    }

                    writerSettings.SetMetadataDocumentUri(new Uri(metadataLink));
                }

                if (contentHeaders != null && contentHeaders.ContentType != null)
                {
                    MediaTypeHeaderValue contentType = contentHeaders.ContentType;
                    writerSettings.SetContentType(contentType.ToString(), contentType.CharSet);
                }

                using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings, _deserializerProvider.EdmModel))
                {
                    ODataSerializerContext writeContext = new ODataSerializerContext()
                    {
                        EntitySet = targetEntitySet,
                        UrlHelper = urlHelper,
                        PathHandler = pathHandler,
                        RootElementName = GetRootElementName(path) ?? ElementNameDefault,
                        SkipExpensiveAvailabilityChecks = serializer.ODataPayloadKind == ODataPayloadKind.Feed,
                        Request = _request
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
            return GetODataVersion(request.Headers, ODataFormatterConstants.ODataMaxServiceVersion, ODataFormatterConstants.ODataServiceVersion) ??
                ODataFormatterConstants.DefaultODataVersion;
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
