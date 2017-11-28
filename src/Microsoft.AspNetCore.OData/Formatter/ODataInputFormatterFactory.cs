// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Factory for <see cref="ODataInputFormatter"/> classes to handle OData.
    /// </summary>
    public static class ODataInputFormatterFactory
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
        public static IList<ODataInputFormatter> Create()
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
        public static IList<ODataInputFormatter> Create(ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider)
        {
            return new List<ODataInputFormatter>()
            {
                // Place JSON formatter first so it gets used when the request doesn't ask for a specific content type
                CreateApplicationJson(serializerProvider, deserializerProvider),
                CreateApplicationXml(serializerProvider, deserializerProvider),
                CreateRawValue(serializerProvider, deserializerProvider)
            };
        }

        private static void AddSupportedEncodings(ODataInputFormatter formatter)
        {
            formatter.SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true));
            formatter.SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true,
                throwOnInvalidBytes: true));
        }

        private static ODataInputFormatter CreateRawValue(ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider)
        {
            ODataInputFormatter formatter = CreateFormatterWithoutMediaTypes(serializerProvider, deserializerProvider, ODataPayloadKind.Value);
            //formatter.MediaTypeMappings.Add(new ODataPrimitiveValueMediaTypeMapping());
            //formatter.MediaTypeMappings.Add(new ODataEnumValueMediaTypeMapping());
            //formatter.MediaTypeMappings.Add(new ODataBinaryValueMediaTypeMapping());
            //formatter.MediaTypeMappings.Add(new ODataCountMediaTypeMapping());
            return formatter;
        }

        private static ODataInputFormatter CreateApplicationJson(ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider)
        {
            ODataInputFormatter formatter = CreateFormatterWithoutMediaTypes(
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

            //formatter.AddDollarFormatQueryStringMappings();
            //formatter.AddQueryStringMapping(DollarFormat, JsonFormat, ODataMediaTypes.ApplicationJson);

            return formatter;
        }

        private static ODataInputFormatter CreateApplicationXml(ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider)
        {
            ODataInputFormatter formatter = CreateFormatterWithoutMediaTypes(
                serializerProvider,
                deserializerProvider,
                ODataPayloadKind.MetadataDocument);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationXml);

            //formatter.AddDollarFormatQueryStringMappings();
            //formatter.AddQueryStringMapping(DollarFormat, XmlFormat, ODataMediaTypes.ApplicationXml);

            return formatter;
        }

        private static ODataInputFormatter CreateFormatterWithoutMediaTypes(ODataSerializerProvider serializerProvider, ODataDeserializerProvider deserializerProvider, params ODataPayloadKind[] payloadKinds)
        {
            ODataInputFormatter formatter = new ODataInputFormatter(deserializerProvider, payloadKinds);
            AddSupportedEncodings(formatter);
            return formatter;
        }

        private static void AddDollarFormatQueryStringMappings(this ODataInputFormatter formatter)
        {
            MediaTypeCollection supportedMediaTypes = formatter.SupportedMediaTypes;
            foreach (string supportedMediaType in supportedMediaTypes)
            {
                //QueryStringMediaTypeMapping mapping = new QueryStringMediaTypeMapping(DollarFormat, supportedMediaType);
                //formatter.MediaTypeMappings.Add(mapping);
            }
        }
    }
}
