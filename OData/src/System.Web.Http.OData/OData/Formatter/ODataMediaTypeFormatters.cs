// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.OData.Formatter.Serialization;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter
{
    /// <summary>
    /// <see cref="MediaTypeFormatter"/> classes to handle OData.
    /// </summary>
    public static class ODataMediaTypeFormatters
    {
        /// <summary>
        /// Creates a list of media type formatters to handle OData.
        /// </summary>
        /// <returns>A list of media type formatters to handle OData.</returns>
        public static IList<ODataMediaTypeFormatter> Create()
        {
            return Create(DefaultODataSerializerProvider.Instance, DefaultODataDeserializerProvider.Instance);
        }

        /// <summary>
        /// Creates a list of media type formatters to handle OData with the given <paramref name="serializerProvider"/> and 
        /// <paramref name="deserializerProvider"/>.
        /// </summary>
        /// <param name="serializerProvider">The serializer provider to use.</param>
        /// <param name="deserializerProvider">The deserializer provider to use.</param>
        /// <returns>A list of media type formatters to handle OData.</returns>
        /// <remarks>The default serializer provider is <see cref="DefaultODataSerializerProvider"/> and the default deserializer provider is
        /// <see cref="DefaultODataDeserializerProvider"/>.</remarks>
        public static IList<ODataMediaTypeFormatter> Create(ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider)
        {
            return new List<ODataMediaTypeFormatter>()
            {
                // Create atomsvc+xml formatter first to handle service document requests without an Accept header in an XML format
                CreateApplicationAtomSvcXml(serializerProvider, deserializerProvider),
                // Create JSON formatter next so it gets used when the request doesn't ask for a specific content type
                CreateApplicationJson(serializerProvider, deserializerProvider),
                CreateApplicationAtomXmlTypeFeed(serializerProvider, deserializerProvider),
                CreateApplicationAtomXmlTypeEntry(serializerProvider, deserializerProvider),
                CreateApplicationXml(serializerProvider, deserializerProvider),
                CreateTextXml(serializerProvider, deserializerProvider),
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
            formatter.MediaTypeMappings.Add(new ODataBinaryValueMediaTypeMapping());
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateApplicationAtomSvcXml(ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider)
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(serializerProvider, deserializerProvider, ODataPayloadKind.ServiceDocument);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationAtomSvcXml);
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateApplicationAtomXmlTypeEntry(ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider)
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(serializerProvider, deserializerProvider, ODataPayloadKind.Entry);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationAtomXmlTypeEntry);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationAtomXml);
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateApplicationAtomXmlTypeFeed(ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider)
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(serializerProvider, deserializerProvider, ODataPayloadKind.Feed);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationAtomXmlTypeFeed);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationAtomXml);
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateApplicationJson(ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider)
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(
                serializerProvider,
                deserializerProvider,
                ODataPayloadKind.Feed,
                ODataPayloadKind.Entry,
                ODataPayloadKind.Property,
                ODataPayloadKind.EntityReferenceLink,
                ODataPayloadKind.EntityReferenceLinks,
                ODataPayloadKind.Collection,
                ODataPayloadKind.ServiceDocument,
                ODataPayloadKind.Error,
                ODataPayloadKind.Parameter);

            // Add minimal metadata as the first media type so it gets used when the request doesn't
            // ask for a specific content type
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingTrue);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingFalse);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingTrue);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingFalse);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataFullMetadata);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingTrue);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingFalse);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataNoMetadata);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataVerbose);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonStreamingTrue);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonStreamingFalse);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJson);
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateApplicationXml(ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider)
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(
                serializerProvider,
                deserializerProvider,
                ODataPayloadKind.Property,
                ODataPayloadKind.EntityReferenceLink,
                ODataPayloadKind.EntityReferenceLinks,
                ODataPayloadKind.Collection,
                ODataPayloadKind.ServiceDocument,
                ODataPayloadKind.MetadataDocument,
                ODataPayloadKind.Error);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationXml);
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateFormatterWithoutMediaTypes(ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider, params ODataPayloadKind[] payloadKinds)
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(deserializerProvider, serializerProvider, payloadKinds);
            AddSupportedEncodings(formatter);
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateTextXml(ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider)
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(
                serializerProvider,
                deserializerProvider,
                ODataPayloadKind.Property,
                ODataPayloadKind.EntityReferenceLink,
                ODataPayloadKind.EntityReferenceLinks,
                ODataPayloadKind.Collection);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.TextXml);
            return formatter;
        }
    }
}
