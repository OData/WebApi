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
        internal const string EdmModelKey = "MS_EdmModel";

        private readonly ODataVersion _defaultODataVersion;
        private readonly ODataDeserializerProvider _deserializerProvider;
        private readonly IEdmModel _model;
        private readonly HttpRequestMessage _request;
        private readonly ODataSerializerProvider _serializerProvider;

        private PatchKeyMode _patchKeyMode;

        internal ODataMediaTypeFormatter(IEdmModel model)
            : this(model, request: null)
        {
        }

        internal ODataMediaTypeFormatter(IEdmModel model, HttpRequestMessage request)
            : this(new DefaultODataDeserializerProvider(model), new DefaultODataSerializerProvider(model),
                ODataFormatterConstants.DefaultODataVersion, request)
        {
        }

        internal ODataMediaTypeFormatter(ODataDeserializerProvider deserializerProvider,
            ODataSerializerProvider serializerProvider)
            : this(deserializerProvider, serializerProvider, ODataFormatterConstants.DefaultODataVersion, null)
        {
        }

        internal ODataMediaTypeFormatter(ODataDeserializerProvider deserializerProvider,
            ODataSerializerProvider serializerProvider,
            ODataVersion version,
            HttpRequestMessage request)
        {
            Contract.Assert(_deserializerProvider != null);
            _deserializerProvider = deserializerProvider;
            Contract.Assert(deserializerProvider.EdmModel != null);
            _model = deserializerProvider.EdmModel;
            Contract.Assert(_serializerProvider != null);
            _serializerProvider = serializerProvider;
            _defaultODataVersion = version;
            _request = request;

            SupportedMediaTypes.Add(ODataFormatterConstants.ApplicationAtomXmlMediaType);
            SupportedMediaTypes.Add(ODataFormatterConstants.ApplicationJsonMediaType);
            SupportedMediaTypes.Add(ODataFormatterConstants.ApplicationXmlMediaType);

            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
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
        /// The <see cref="PatchKeyMode"/> to be used during deserialization.
        /// </summary>
        public PatchKeyMode PatchKeyMode
        {
            get
            {
                return _patchKeyMode;
            }

            set
            {
                PatchKeyModeHelper.Validate(value, "value");
                _patchKeyMode = value;
            }
        }

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
            return new ODataMediaTypeFormatter(_deserializerProvider, _serializerProvider, version, request);
        }

        /// <inheritdoc/>
        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            if (headers == null)
            {
                throw Error.ArgumentNull("headers");
            }

            // call base to validate parameters and set Content-Type header based on mediaType parameter.
            base.SetDefaultContentHeaders(type, headers, mediaType);

            ODataFormat format = GetODataFormat(headers);
            IEnumerable<KeyValuePair<string, string>> oDataHeaders = GetResponseMessageHeaders(type, format, _defaultODataVersion);

            foreach (KeyValuePair<string, string> pair in oDataHeaders)
            {
                // Special case Content-Type header so that we don't end up with two values for it
                // since base.SetDefaultContentHeaders could also have set it.
                if (String.Equals("Content-Type", pair.Key, StringComparison.OrdinalIgnoreCase))
                {
                    headers.ContentType = MediaTypeHeaderValue.Parse(pair.Value);
                    headers.ContentType.CharSet = Encoding.UTF8.WebName;
                }
                else
                {
                    headers.TryAddWithoutValidation(pair.Key, pair.Value);
                }
            }
        }

        /// <inheritdoc/>
        public override bool CanReadType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            TryGetInnerTypeForDelta(ref type);
            return _deserializerProvider.GetODataDeserializer(type) != null;
        }

        /// <inheritdoc/>
        public override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(type);
            return serializer != null;
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
                        ODataDeserializerContext readContext = new ODataDeserializerContext { IsPatchMode = isPatchMode, PatchKeyMode = PatchKeyMode, Request = _request, Model = _model };
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
                // Get the format and version to use from the ODataServiceVersion content header or if not available use the
                // values configured for the specialized formatter instance.
                ODataVersion version;
                if (contentHeaders == null)
                {
                    version = _defaultODataVersion;
                }
                else
                {
                    version = GetODataVersion(contentHeaders, ODataFormatterConstants.ODataServiceVersion) ?? _defaultODataVersion;
                }

                // get the most appropriate serializer given that we support inheritance.
                type = value == null ? type : value.GetType();
                ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(type);
                if (serializer == null)
                {
                    throw Error.InvalidOperation(SRResources.TypeCannotBeSerialized, type.Name, typeof(ODataMediaTypeFormatter).Name);
                }

                UrlHelper urlHelper = _request.GetUrlHelper();

                IEdmEntitySet targetEntitySet = null;
                ODataUriHelpers.TryGetEntitySetAndEntityType(_request.RequestUri, _model, out targetEntitySet);

                // serialize a response
                Uri baseAddress = new Uri(_request.RequestUri, _request.GetConfiguration().VirtualPathRoot);

                // TODO: Bug 467617: figure out the story for the operation name on the client side and server side.
                // This is clearly a workaround. We are assuming that the operation name is the last segment in the request uri 
                // which works for most cases and fall back to the type name of the object being written.
                // We should rather use uri parser semantic tree to figure out the operation name from the request url.
                string operationName = ODataUriHelpers.GetOperationName(_request.RequestUri, baseAddress);
                operationName = operationName ?? type.Name;

                IODataResponseMessage responseMessage = new ODataMessageWrapper(writeStream);

                // TODO: Issue 483: http://aspnetwebstack.codeplex.com/workitem/483
                // We need to set the MetadataDocumentUri when this property is added to ODataMessageWriterSettings as 
                // part of the JSON Light work.
                // This is required so ODataLib can coerce AbsoluteUri's into RelativeUri's when appropriate in JSON Light.
                ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings()
                {
                    BaseUri = baseAddress,
                    Version = version,
                    Indent = true,
                    DisableMessageStreamDisposal = true
                };

                if (contentHeaders != null && contentHeaders.ContentType != null)
                {
                    writerSettings.SetContentType(contentHeaders.ContentType.ToString(), Encoding.UTF8.WebName);
                }

                using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings, _deserializerProvider.EdmModel))
                {
                    ODataSerializerContext writeContext = new ODataSerializerContext()
                    {
                        EntitySet = targetEntitySet,
                        UrlHelper = urlHelper,
                        ServiceOperationName = operationName,
                        SkipExpensiveAvailabilityChecks = serializer.ODataPayloadKind == ODataPayloadKind.Feed,
                        Request = _request
                    };

                    serializer.WriteObject(value, messageWriter, writeContext);
                }
            });
        }

        private IEnumerable<KeyValuePair<string, string>> GetResponseMessageHeaders(Type graphType, ODataFormat odataFormat, ODataVersion version)
        {
            IODataResponseMessage responseMessage = new ODataMessageWrapper();

            ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings()
            {
                BaseUri = new Uri(ODataFormatterConstants.DefaultNamespace),
                Version = version,
                Indent = false
            };
            writerSettings.SetContentType(odataFormat);
            using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings))
            {
                ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(graphType);

                // get the OData specific headers for the payloadkind
                ODataUtils.SetHeadersForPayload(messageWriter, serializer.ODataPayloadKind);
            }

            return responseMessage.Headers;
        }

        private static ODataFormat GetODataFormat(HttpContentHeaders contentHeaders)
        {
            Contract.Assert(contentHeaders != null);

            if (contentHeaders.ContentType == null)
            {
                return ODataFormatterConstants.DefaultODataFormat;
            }

            if (String.Equals(contentHeaders.ContentType.MediaType, ODataFormatterConstants.DefaultApplicationODataMediaType, StringComparison.OrdinalIgnoreCase))
            {
                return ODataFormat.Atom;
            }
            else if (String.Equals(contentHeaders.ContentType.MediaType, ODataFormatterConstants.ApplicationJsonMediaType.MediaType, StringComparison.OrdinalIgnoreCase))
            {
                return ODataFormat.VerboseJson;
            }
            else
            {
                return ODataFormatterConstants.DefaultODataFormat;
            }
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
