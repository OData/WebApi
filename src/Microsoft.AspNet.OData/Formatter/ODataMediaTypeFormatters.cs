// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// <see cref="MediaTypeFormatter"/> classes to handle OData.
    /// </summary>
    public static class ODataMediaTypeFormatters
    {
        private const string DollarFormat = "$format";

        private const string JsonFormat = "json";

        private const string XmlFormat = "xml";

        /// <summary>
        /// Creates a list of media type formatters to handle OData.
        /// The default serializer provider is <see cref="ODataSerializerProviderProxy"/> and the default deserializer provider is
        /// <see cref="ODataDeserializerProviderProxy"/>.
        /// </summary>
        /// <returns>A list of media type formatters to handle OData.</returns>
        public static IList<ODataMediaTypeFormatter> Create()
        {
            return Create(ODataSerializerProviderProxy.Instance, ODataDeserializerProviderProxy.Instance);
        }

        /// <summary>
        /// Creates a list of media type formatters to handle OData with the given <paramref name="serializerProvider"/> and
        /// <paramref name="deserializerProvider"/>.
        /// </summary>
        /// <param name="serializerProvider">The serializer provider to use.</param>
        /// <param name="deserializerProvider">The deserializer provider to use.</param>
        /// <returns>A list of media type formatters to handle OData.</returns>
        public static IList<ODataMediaTypeFormatter> Create(ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider)
        {
            return new List<ODataMediaTypeFormatter>()
            {
                // Place JSON formatter first so it gets used when the request doesn't ask for a specific content type
                CreateApplicationJson(serializerProvider, deserializerProvider),
                CreateApplicationXml(serializerProvider, deserializerProvider),
                CreateRawValue(serializerProvider, deserializerProvider)
            };
        }

        private static void AddSupportedEncodings(MediaTypeFormatter formatter)
        {
            formatter.SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true));
            formatter.SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true,
                throwOnInvalidBytes: true));
        }

        private static ODataMediaTypeFormatter CreateRawValue(ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider)
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(serializerProvider, deserializerProvider, ODataPayloadKind.Value);
            formatter.MediaTypeMappings.Add(new ODataPrimitiveValueMediaTypeMapping());
            formatter.MediaTypeMappings.Add(new ODataEnumValueMediaTypeMapping());
            formatter.MediaTypeMappings.Add(new ODataBinaryValueMediaTypeMapping());
            formatter.MediaTypeMappings.Add(new ODataCountMediaTypeMapping());
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateApplicationJson(ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider)
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(
                serializerProvider,
                deserializerProvider,
                ODataPayloadKind.ResourceSet,
                ODataPayloadKind.Resource,
                ODataPayloadKind.Property,
                ODataPayloadKind.EntityReferenceLink,
                ODataPayloadKind.EntityReferenceLinks,
                ODataPayloadKind.Collection,
                ODataPayloadKind.ServiceDocument,
                ODataPayloadKind.Error,
                ODataPayloadKind.Parameter,
                ODataPayloadKind.Delta);

            // Add minimal metadata as the first media type so it gets used when the request doesn't
            // ask for a specific content type
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingTrue));
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingFalse));
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonODataMinimalMetadata));
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingTrue));
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingFalse));
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonODataFullMetadata));
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingTrue));
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingFalse));
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonODataNoMetadata));
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonStreamingTrue));
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonStreamingFalse));
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJson));

            formatter.AddDollarFormatQueryStringMappings();
            formatter.AddQueryStringMapping(DollarFormat, JsonFormat, ODataMediaTypes.ApplicationJson);

            return formatter;
        }

        private static ODataMediaTypeFormatter CreateApplicationXml(ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider)
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(
                serializerProvider,
                deserializerProvider,
                ODataPayloadKind.MetadataDocument);
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationXml));

            formatter.AddDollarFormatQueryStringMappings();
            formatter.AddQueryStringMapping(DollarFormat, XmlFormat, ODataMediaTypes.ApplicationXml);

            return formatter;
        }

        private static ODataMediaTypeFormatter CreateFormatterWithoutMediaTypes(ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider, params ODataPayloadKind[] payloadKinds)
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(deserializerProvider, serializerProvider, payloadKinds);
            AddSupportedEncodings(formatter);
            return formatter;
        }

        private static void AddDollarFormatQueryStringMappings(this ODataMediaTypeFormatter formatter)
        {
            ICollection<MediaTypeHeaderValue> supportedMediaTypes = formatter.SupportedMediaTypes;
            foreach (MediaTypeHeaderValue supportedMediaType in supportedMediaTypes)
            {
                QueryStringMediaTypeMapping mapping = new QueryStringMediaTypeMapping(DollarFormat, supportedMediaType);
                formatter.MediaTypeMappings.Add(mapping);
            }
        }
    }
}
