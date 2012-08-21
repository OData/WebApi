// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
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
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter
{
    /// <summary>
    /// <see cref="MediaTypeFormatter"/> class to handle OData.
    /// </summary>
    public class ODataMediaTypeFormatter : MediaTypeFormatter
    {
        private readonly ODataVersion _defaultODataVersion = ODataFormatterConstants.DefaultODataVersion;

        /// <summary>
        /// This constructor is used for unit testing purposes only
        /// </summary>
        public ODataMediaTypeFormatter()
            : this(EdmCoreModel.Instance)
        {
        }

        public ODataMediaTypeFormatter(IEdmModel edmModel)
            : this(new DefaultODataDeserializerProvider(edmModel), new DefaultODataSerializerProvider(edmModel))
        {
            Model = edmModel;
        }

        internal ODataMediaTypeFormatter(ODataVersion oDataVersion, ODataDeserializerProvider oDataDeserializerProvider, ODataSerializerProvider oDataSerializerProvider)
            : this(oDataDeserializerProvider, oDataSerializerProvider)
        {
            _defaultODataVersion = oDataVersion;
        }

        internal ODataMediaTypeFormatter(ODataDeserializerProvider oDataDeserializerProvider, ODataSerializerProvider oDataSerializerProvider)
        {
            ODataDeserializerProvider = oDataDeserializerProvider;
            Model = oDataDeserializerProvider.EdmModel;
            ODataSerializerProvider = oDataSerializerProvider;

            SupportedMediaTypes.Add(ODataFormatterConstants.ApplicationAtomXmlMediaType);
            SupportedMediaTypes.Add(ODataFormatterConstants.ApplicationJsonMediaType);
            SupportedMediaTypes.Add(ODataFormatterConstants.ApplicationXmlMediaType);

            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
        }

        public bool IsClient { get; set; }

        public IEdmModel Model { get; private set; }

        /// <summary>
        /// The incoming <see cref="HttpRequestMessage" />.
        /// </summary>
        internal HttpRequestMessage Request { get; set; }

        internal ODataDeserializerProvider ODataDeserializerProvider { get; private set; }

        internal ODataSerializerProvider ODataSerializerProvider { get; private set; }

        /// <summary>
        /// Gets the default media type for atom, namely "application/atom+xml".
        /// </summary>
        /// <value>
        /// Because <see cref="MediaTypeHeaderValue"/> is mutable, the value
        /// returned will be a new instance every time.
        /// </value>
        public static MediaTypeHeaderValue DefaultMediaType
        {
            get { return ODataFormatterConstants.ApplicationAtomXmlMediaType; }
        }

        /// <inheritdoc/>
        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
        {
            // call base to validate parameters
            base.GetPerRequestFormatterInstance(type, request, mediaType);

            ODataVersion version = GetResponseODataVersion(request);
            return new ODataMediaTypeFormatter(version, ODataDeserializerProvider, ODataSerializerProvider) { IsClient = false, Request = request };
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
            return ODataDeserializerProvider.GetODataDeserializer(type) != null;
        }

        /// <inheritdoc/>
        public override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            ODataSerializer serializer = ODataSerializerProvider.GetODataPayloadSerializer(type);
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

            object result = null;

            HttpContentHeaders contentHeaders = content == null ? null : content.Headers;
            // If content length is 0 then return default value for this type
            if (contentHeaders != null && contentHeaders.ContentLength == 0)
            {
                result = GetDefaultValueForType(type);
            }
            else
            {
                bool isPatchMode = TryGetInnerTypeForDelta(ref type);
                ODataDeserializer deserializer = ODataDeserializerProvider.GetODataDeserializer(type);
                if (deserializer == null)
                {
                    throw Error.Argument("type", SRResources.FormatterReadIsNotSupportedForType, type.FullName, GetType().FullName);
                }

                ODataMessageReader oDataMessageReader = null;
                ODataMessageReaderSettings oDataReaderSettings = new ODataMessageReaderSettings { DisableMessageStreamDisposal = true };
                try
                {
                    if (IsClient)
                    {
                        IODataResponseMessage oDataResponseMessage = new ODataMessageWrapper(readStream, contentHeaders);
                        oDataMessageReader = new ODataMessageReader(oDataResponseMessage, oDataReaderSettings, ODataDeserializerProvider.EdmModel);
                    }
                    else
                    {
                        IODataRequestMessage oDataRequestMessage = new ODataMessageWrapper(readStream, contentHeaders);
                        oDataMessageReader = new ODataMessageReader(oDataRequestMessage, oDataReaderSettings, ODataDeserializerProvider.EdmModel);
                    }

                    ODataDeserializerReadContext readContext = new ODataDeserializerReadContext { IsPatchMode = isPatchMode };

                    result = deserializer.Read(oDataMessageReader, readContext);
                }
                finally
                {
                    if (oDataMessageReader != null)
                    {
                        oDataMessageReader.Dispose();
                    }
                }
            }

            return TaskHelpers.FromResult(result);
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

            HttpContentHeaders contentHeaders = content == null ? null : content.Headers;
            return TaskHelpers.RunSynchronously(() =>
            {
                // Get the format and version to use from the ODataServiceVersion content header or if not available use the
                // values configured for the specialized formatter instance.
                ODataVersion version;
                ODataFormat odataFormat;
                if (contentHeaders == null)
                {
                    version = _defaultODataVersion;
                    odataFormat = ODataFormatterConstants.DefaultODataFormat;
                }
                else
                {
                    version = GetODataVersion(contentHeaders, ODataFormatterConstants.ODataServiceVersion) ?? _defaultODataVersion;
                    odataFormat = GetODataFormat(contentHeaders);
                }

                ODataSerializer serializer = ODataSerializerProvider.GetODataPayloadSerializer(type);
                if (serializer == null)
                {
                    throw Error.InvalidOperation(SRResources.TypeCannotBeSerialized, type.Name, typeof(ODataMediaTypeFormatter).Name);
                }

                if (IsClient)
                {
                    // TODO: Bug 467617: figure out the story for the operation name on the client side and server side.
                    string operationName = (value != null ? value.GetType() : type).Name;

                    // serialize a request
                    IODataRequestMessage requestMessage = new ODataMessageWrapper(writeStream);
                    ODataResponseContext responseContext = new ODataResponseContext(requestMessage, odataFormat, version, new Uri(ODataFormatterConstants.DefaultNamespace), operationName);

                    ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings()
                    {
                        BaseUri = responseContext.BaseAddress,
                        Version = responseContext.ODataVersion,
                        Indent = responseContext.IsIndented,
                        DisableMessageStreamDisposal = true,
                    };

                    writerSettings.SetContentType(responseContext.ODataFormat);
                    using (ODataMessageWriter messageWriter = new ODataMessageWriter(requestMessage, writerSettings, Model))
                    {
                        ODataSerializerWriteContext writeContext = new ODataSerializerWriteContext(responseContext);
                        serializer.WriteObject(value, messageWriter, writeContext);
                    }
                }
                else
                {
                    UrlHelper urlHelper = Request.GetUrlHelper();
                    NameValueCollection queryStringValues = Request.RequestUri.ParseQueryString();

                    IEdmEntitySet targetEntitySet = null;
                    ODataUriHelpers.TryGetEntitySetAndEntityType(Request.RequestUri, Model, out targetEntitySet);

                    ODataQueryProjectionNode rootProjectionNode = null;
                    if (targetEntitySet != null)
                    {
                        // TODO: Bug 467621: Move to ODataUriParser once it is done.
                        rootProjectionNode = ODataUriHelpers.GetODataQueryProjectionNode(queryStringValues["$select"], queryStringValues["$expand"], targetEntitySet);
                    }

                    // serialize a response
                    Uri baseAddress = new Uri(Request.RequestUri, Request.GetConfiguration().VirtualPathRoot);

                    // TODO: Bug 467617: figure out the story for the operation name on the client side and server side.
                    // This is clearly a workaround. We are assuming that the operation name is the last segment in the request uri 
                    // which works for most cases and fall back to the type name of the object being written.
                    // We should rather use uri parser semantic tree to figure out the operation name from the request url.
                    string operationName = ODataUriHelpers.GetOperationName(Request.RequestUri, baseAddress);
                    operationName = operationName ?? type.Name;

                    IODataResponseMessage responseMessage = new ODataMessageWrapper(writeStream);
                    ODataResponseContext responseContext = new ODataResponseContext(responseMessage, odataFormat, version, baseAddress, operationName);

                    ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings()
                    {
                        BaseUri = responseContext.BaseAddress,
                        Version = responseContext.ODataVersion,
                        Indent = responseContext.IsIndented,
                        DisableMessageStreamDisposal = true,
                    };
                    if (contentHeaders != null && contentHeaders.ContentType != null)
                    {
                        writerSettings.SetContentType(contentHeaders.ContentType.ToString(), Encoding.UTF8.WebName);
                    }

                    using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings, ODataDeserializerProvider.EdmModel))
                    {
                        ODataSerializerWriteContext writeContext = new ODataSerializerWriteContext(responseContext)
                                                                       {
                                                                           EntitySet = targetEntitySet,
                                                                           UrlHelper = urlHelper,
                                                                           RootProjectionNode = rootProjectionNode,
                                                                           CurrentProjectionNode = rootProjectionNode
                                                                       };

                        serializer.WriteObject(value, messageWriter, writeContext);
                    }
                }
            });
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
            return GetODataVersion(request.Headers, ODataFormatterConstants.ODataMaxServiceVersion) ??
                GetODataVersion(request.Headers, ODataFormatterConstants.ODataServiceVersion) ??
                ODataFormatterConstants.DefaultODataVersion;
        }

        private static ODataVersion? GetODataVersion(HttpHeaders headers, string headerName)
        {
            ODataVersion? version = null;
            IEnumerable<string> values;

            if (headers.TryGetValues(headerName, out values))
            {
                foreach (string value in values)
                {
                    string trimmedValue = value.Trim(' ', ';');
                    version = GetODataVersion(trimmedValue);
                }
            }

            return version;
        }

        private static ODataVersion? GetODataVersion(string versionString)
        {
            try
            {
                return ODataUtils.StringToODataVersion(versionString);
            }
            catch (ODataException)
            {
                return null;
            }
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
                ODataSerializer serializer = ODataSerializerProvider.GetODataPayloadSerializer(graphType);

                // get the OData specific headers for the payloadkind
                ODataUtils.SetHeadersForPayload(messageWriter, serializer.ODataPayloadKind);
            }

            return responseMessage.Headers;
        }

        private static bool TryGetInnerTypeForDelta(ref Type type)
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
