// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Factory for <see cref="ODataOutputFormatter"/> classes to handle OData.
    /// </summary>
    public static class ODataOutputFormatterFactory
    {
        private const string DollarFormat = "$format";

        private const string JsonFormat = "json";

        private const string XmlFormat = "xml";

        /// <summary>
        /// Creates a list of media type formatters to handle OData.
        /// The default serializer provider is <see cref="ODataSerializerProviderProxy"/>.
        /// </summary>
        /// <returns>A list of media type formatters to handle OData.</returns>
        public static IList<ODataOutputFormatter> Create()
        {
            return Create(new ODataSerializerProviderProxy());
        }

        /// <summary>
        /// Creates a list of media type formatters to handle OData with the given <paramref name="serializerProvider"/>.
        /// </summary>
        /// <param name="serializerProvider">The serializer provider to use.</param>
        /// <returns>A list of media type formatters to handle OData.</returns>
        public static IList<ODataOutputFormatter> Create(ODataSerializerProvider serializerProvider)
        {
            return new List<ODataOutputFormatter>()
            {
                // Place JSON formatter first so it gets used when the request doesn't ask for a specific content type
                CreateApplicationJson(serializerProvider),
                CreateApplicationXml(serializerProvider),
                CreateRawValue(serializerProvider)
            };
        }

        private static void AddSupportedEncodings(ODataOutputFormatter formatter)
        {
            formatter.SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true));
            formatter.SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true,
                throwOnInvalidBytes: true));
        }

        private static ODataOutputFormatter CreateRawValue(ODataSerializerProvider serializerProvider)
        {
            ODataOutputFormatter formatter = CreateFormatterWithoutMediaTypes(serializerProvider, ODataPayloadKind.Value);
            formatter.MediaTypeMappings.Add(new ODataPrimitiveValueMediaTypeMapping());
            formatter.MediaTypeMappings.Add(new ODataEnumValueMediaTypeMapping());
            formatter.MediaTypeMappings.Add(new ODataBinaryValueMediaTypeMapping());
            formatter.MediaTypeMappings.Add(new ODataCountMediaTypeMapping());
            return formatter;
        }

        private static ODataOutputFormatter CreateApplicationJson(ODataSerializerProvider serializerProvider)
        {
            ODataOutputFormatter formatter = CreateFormatterWithoutMediaTypes(
                serializerProvider,
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
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingTrue);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingFalse);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingTrue);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingFalse);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataFullMetadata);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingTrue);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingFalse);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataNoMetadata);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonStreamingTrue);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonStreamingFalse);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJson);

            formatter.AddDollarFormatQueryStringMappings();
            formatter.AddQueryStringMapping(DollarFormat, JsonFormat, ODataMediaTypes.ApplicationJson);

            return formatter;
        }

        private static ODataOutputFormatter CreateApplicationXml(ODataSerializerProvider serializerProvider)
        {
            ODataOutputFormatter formatter = CreateFormatterWithoutMediaTypes(
                serializerProvider,
                ODataPayloadKind.MetadataDocument);

            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationXml);

            formatter.AddDollarFormatQueryStringMappings();
            formatter.AddQueryStringMapping(DollarFormat, XmlFormat, ODataMediaTypes.ApplicationXml);

            return formatter;
        }

        private static ODataOutputFormatter CreateFormatterWithoutMediaTypes(ODataSerializerProvider serializerProvider, params ODataPayloadKind[] payloadKinds)
        {
            ODataOutputFormatter formatter = new ODataOutputFormatter(serializerProvider, payloadKinds);
            AddSupportedEncodings(formatter);
            return formatter;
        }

        private static void AddDollarFormatQueryStringMappings(this ODataOutputFormatter formatter)
        {
            MediaTypeCollection supportedMediaTypes = formatter.SupportedMediaTypes;
            foreach (string supportedMediaType in supportedMediaTypes)
            {
                QueryStringMediaTypeMapping mapping = new QueryStringMediaTypeMapping(DollarFormat, supportedMediaType);
                formatter.MediaTypeMappings.Add(mapping);
            }
        }

        private static void AddQueryStringMapping(this ODataOutputFormatter formatter, string queryStringParameterName,
            string queryStringParameterValue, string mediaType)
        {
            QueryStringMediaTypeMapping mapping = new QueryStringMediaTypeMapping(queryStringParameterName, queryStringParameterValue, mediaType);
            formatter.MediaTypeMappings.Add(mapping);
        }
    }
}
