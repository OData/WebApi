//-----------------------------------------------------------------------------
// <copyright file="ODataMediaTypeFormatters.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
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
        /// </summary>
        /// <returns>A list of media type formatters to handle OData.</returns>
        public static IList<ODataMediaTypeFormatter> Create()
        {
            return new List<ODataMediaTypeFormatter>()
            {
                // Place JSON formatter first so it gets used when the request doesn't ask for a specific content type
                CreateApplicationJson(),
                CreateMetadataFormatter(),
                CreateRawValue()
            };
        }

        private static void AddSupportedEncodings(MediaTypeFormatter formatter)
        {
            formatter.SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true));
            formatter.SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true,
                throwOnInvalidBytes: true));
        }

        private static ODataMediaTypeFormatter CreateRawValue()
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(ODataPayloadKind.Value);
            formatter.MediaTypeMappings.Add(new ODataPrimitiveValueMediaTypeMapping());
            formatter.MediaTypeMappings.Add(new ODataEnumValueMediaTypeMapping());
            formatter.MediaTypeMappings.Add(new ODataBinaryValueMediaTypeMapping());
            formatter.MediaTypeMappings.Add(new ODataCountMediaTypeMapping());
            formatter.MediaTypeMappings.Add(new ODataStreamMediaTypeMapping());
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateApplicationJson()
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(
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

        private static ODataMediaTypeFormatter CreateMetadataFormatter()
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(
                ODataPayloadKind.MetadataDocument);
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationXml));

            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJson));
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

            formatter.AddDollarFormatQueryStringMappings();
            formatter.AddQueryStringMapping(DollarFormat, XmlFormat, ODataMediaTypes.ApplicationXml);
            formatter.AddQueryStringMapping(DollarFormat, JsonFormat, ODataMediaTypes.ApplicationJson);
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateFormatterWithoutMediaTypes(params ODataPayloadKind[] payloadKinds)
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(payloadKinds);
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
