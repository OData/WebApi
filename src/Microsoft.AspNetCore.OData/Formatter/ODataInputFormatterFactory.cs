// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Factory for <see cref="ODataInputFormatter"/> classes to handle OData.
    /// </summary>
    public static class ODataInputFormatterFactory
    {
        /// <summary>
        /// Creates a list of media type formatters to handle OData.
        /// The default deserializer provider is <see cref="ODataDeserializerProviderProxy"/>.
        /// </summary>
        /// <returns>A list of media type formatters to handle OData.</returns>
        public static IList<ODataInputFormatter> Create()
        {
            return Create(new ODataDeserializerProviderProxy());
        }

        /// <summary>
        /// Creates a list of media type formatters to handle OData with the given <paramref name="deserializerProvider"/>.
        /// </summary>
        /// <param name="deserializerProvider">The deserializer provider to use.</param>
        /// <returns>A list of media type formatters to handle OData.</returns>
        public static IList<ODataInputFormatter> Create(ODataDeserializerProvider deserializerProvider)
        {
            return new List<ODataInputFormatter>()
            {
                // Place JSON formatter first so it gets used when the request doesn't ask for a specific content type
                CreateApplicationJson(deserializerProvider),
                CreateApplicationXml(deserializerProvider),
                CreateRawValue(deserializerProvider)
            };
        }

        private static void AddSupportedEncodings(ODataInputFormatter formatter)
        {
            formatter.SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true));
            formatter.SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true,
                throwOnInvalidBytes: true));
        }

        private static ODataInputFormatter CreateRawValue(ODataDeserializerProvider deserializerProvider)
        {
            return CreateFormatterWithoutMediaTypes(deserializerProvider, ODataPayloadKind.Value);
        }

        private static ODataInputFormatter CreateApplicationJson(ODataDeserializerProvider deserializerProvider)
        {
            ODataInputFormatter formatter = CreateFormatterWithoutMediaTypes(
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

            return formatter;
        }

        private static ODataInputFormatter CreateApplicationXml(ODataDeserializerProvider deserializerProvider)
        {
            ODataInputFormatter formatter = CreateFormatterWithoutMediaTypes(
                deserializerProvider,
                ODataPayloadKind.MetadataDocument);

            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationXml);

            return formatter;
        }

        private static ODataInputFormatter CreateFormatterWithoutMediaTypes(ODataDeserializerProvider deserializerProvider, params ODataPayloadKind[] payloadKinds)
        {
            ODataInputFormatter formatter = new ODataInputFormatter(deserializerProvider, payloadKinds);
            AddSupportedEncodings(formatter);
            return formatter;
        }
    }
}
