// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Text;
using Microsoft.Data.Edm;
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
            return new List<ODataMediaTypeFormatter>()
            {
                // Create JSON formatter first so it gets used when the request doesn't
                // ask for a specific content type
                CreateApplicationJson(),
                CreateApplicationAtomXmlTypeFeed(),
                CreateApplicationAtomXmlTypeEntry(),
                CreateApplicationXml(),
                CreateApplicationAtomSvcXml(),
                CreateTextXml()
            };
        }

        private static void AddSupportedEncodings(MediaTypeFormatter formatter)
        {
            formatter.SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true));
            formatter.SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true,
                throwOnInvalidBytes: true));
        }

        private static ODataMediaTypeFormatter CreateApplicationAtomSvcXml()
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(ODataPayloadKind.ServiceDocument);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationAtomSvcXml);
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateApplicationAtomXmlTypeEntry()
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(ODataPayloadKind.Entry);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationAtomXmlTypeEntry);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationAtomXml);
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateApplicationAtomXmlTypeFeed()
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(ODataPayloadKind.Feed);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationAtomXmlTypeFeed);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationAtomXml);
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateApplicationJson()
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(
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

        private static ODataMediaTypeFormatter CreateApplicationXml()
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(
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

        private static ODataMediaTypeFormatter CreateFormatterWithoutMediaTypes(params ODataPayloadKind[] payloadKinds)
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(payloadKinds);
            AddSupportedEncodings(formatter);
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateTextXml()
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(
                ODataPayloadKind.Property,
                ODataPayloadKind.EntityReferenceLink,
                ODataPayloadKind.EntityReferenceLinks,
                ODataPayloadKind.Collection);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.TextXml);
            return formatter;
        }
    }
}
