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
        /// Creates a set of media type formatters to handle OData.
        /// </summary>
        /// <param name="model">The data model the formatter will support.</param>
        /// <returns>A set of media type formatters to handle OData.</returns>
        public static IEnumerable<ODataMediaTypeFormatter> Create(IEdmModel model)
        {
            return new ODataMediaTypeFormatter[]
            {
                CreateApplicationAtomXmlTypeFeed(model),
                CreateApplicationAtomXmlTypeEntry(model),
                CreateApplicationXml(model),
                CreateApplicationAtomSvcXml(model),
                CreateTextXml(model),
                CreateApplicationJsonODataVerbose(model),
                CreateApplicationJsonODataLight(model)
            };
        }

        private static void AddSupportedEncodings(MediaTypeFormatter formatter)
        {
            formatter.SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true));
            formatter.SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true,
                throwOnInvalidBytes: true));
        }

        private static ODataMediaTypeFormatter CreateApplicationAtomSvcXml(IEdmModel model)
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(
                model,
                ODataPayloadKind.ServiceDocument);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationAtomSvcXml);
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateApplicationAtomXmlTypeEntry(IEdmModel model)
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(
                model,
                ODataPayloadKind.Entry);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationAtomXmlTypeEntry);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationAtomXml);
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateApplicationAtomXmlTypeFeed(IEdmModel model)
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(
                model,
                ODataPayloadKind.Feed);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationAtomXmlTypeFeed);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationAtomXml);
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateApplicationJsonODataLight(IEdmModel model)
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(
                model,
                ODataPayloadKind.Feed,
                ODataPayloadKind.Entry,
                ODataPayloadKind.Property,
                ODataPayloadKind.EntityReferenceLink,
                ODataPayloadKind.EntityReferenceLinks,
                ODataPayloadKind.Collection,
                ODataPayloadKind.ServiceDocument,
                ODataPayloadKind.Error,
                ODataPayloadKind.Parameter);
            // TODO: Feature #664 - Support reading for JSON light.
            formatter.WriteOnly = true;
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingTrue);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingFalse);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataFullMetadata);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingTrue);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingFalse);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
            // TODO: Feature #664 - Support nometadata for JSON light.
            // TODO: Bug #671 - Don't silently take over application/json globally.
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonStreamingTrue);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonStreamingFalse);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJson);
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateApplicationJsonODataVerbose(IEdmModel model)
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(
                model,
                ODataPayloadKind.Feed,
                ODataPayloadKind.Entry,
                ODataPayloadKind.Property,
                ODataPayloadKind.EntityReferenceLink,
                ODataPayloadKind.EntityReferenceLinks,
                ODataPayloadKind.Collection,
                ODataPayloadKind.ServiceDocument,
                ODataPayloadKind.Error,
                ODataPayloadKind.Parameter);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataVerbose);
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateApplicationXml(IEdmModel model)
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(
                model,
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

        private static ODataMediaTypeFormatter CreateFormatterWithoutMediaTypes(IEdmModel model,
            params ODataPayloadKind[] payloadKinds)
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(model, payloadKinds);
            AddSupportedEncodings(formatter);
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateTextXml(IEdmModel model)
        {
            ODataMediaTypeFormatter formatter = CreateFormatterWithoutMediaTypes(
                model,
                ODataPayloadKind.Property,
                ODataPayloadKind.EntityReferenceLink,
                ODataPayloadKind.EntityReferenceLinks,
                ODataPayloadKind.Collection);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.TextXml);
            return formatter;
        }
    }
}
