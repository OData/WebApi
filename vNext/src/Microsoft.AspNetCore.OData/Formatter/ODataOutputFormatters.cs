// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter
{
    /// <summary>
    /// <see cref="ODataOutputFormatter"/> classes to handle OData.
    /// </summary>
    public static class ODataOutputFormatters
    {
        private const string DollarFormat = "$format";

        private const string JsonFormat = "json";

        private const string XmlFormat = "xml";

        /// <summary>
        /// Creates a list of media type formatters to handle OData.
        /// </summary>
        /// <returns>A list of media type formatters to handle OData.</returns>
        public static IList<ODataOutputFormatter> Create()
        {
            return new List<ODataOutputFormatter>()
            {
                // Place JSON formatter first so it gets used when the request doesn't ask for a specific content type
                CreateApplicationJson(),
                CreateApplicationXml(),
                CreateRawValue()
            };
        }

        private static void AddSupportedEncodings(ODataOutputFormatter formatter)
        {
            formatter.SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true));
            formatter.SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true,
                throwOnInvalidBytes: true));
        }

        private static ODataOutputFormatter CreateRawValue()
        {
            ODataOutputFormatter formatter = CreateFormatterWithoutMediaTypes(ODataPayloadKind.Value);
            //formatter.MediaTypeMappings.Add(new ODataPrimitiveValueMediaTypeMapping());
            //formatter.MediaTypeMappings.Add(new ODataEnumValueMediaTypeMapping());
            //formatter.MediaTypeMappings.Add(new ODataBinaryValueMediaTypeMapping());
            //formatter.MediaTypeMappings.Add(new ODataCountMediaTypeMapping());
            return formatter;
        }

        private static ODataOutputFormatter CreateApplicationJson()
        {
            ODataOutputFormatter formatter = CreateFormatterWithoutMediaTypes(
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

        private static ODataOutputFormatter CreateApplicationXml()
        {
            ODataOutputFormatter formatter = CreateFormatterWithoutMediaTypes(
                ODataPayloadKind.MetadataDocument);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationXml);

            //formatter.AddDollarFormatQueryStringMappings();
            //formatter.AddQueryStringMapping(DollarFormat, XmlFormat, ODataMediaTypes.ApplicationXml);

            return formatter;
        }

        private static ODataOutputFormatter CreateFormatterWithoutMediaTypes(params ODataPayloadKind[] payloadKinds)
        {
            ODataOutputFormatter formatter = new ODataOutputFormatter(payloadKinds);
            AddSupportedEncodings(formatter);
            return formatter;
        }

        //private static void AddDollarFormatQueryStringMappings(this ODataOutputFormatter formatter)
        //{
        //    ICollection<MediaTypeHeaderValue> supportedMediaTypes = formatter.SupportedMediaTypes;
        //    foreach (MediaTypeHeaderValue supportedMediaType in supportedMediaTypes)
        //    {
        //        QueryStringMediaTypeMapping mapping = new QueryStringMediaTypeMapping(DollarFormat, supportedMediaType);
        //        formatter.MediaTypeMappings.Add(mapping);
        //    }
        //}
    }
}
