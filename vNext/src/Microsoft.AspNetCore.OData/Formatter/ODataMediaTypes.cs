// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.OData.Formatter
{
    /// <summary>
    ///     Contains media types used by the OData formatter.
    /// </summary>
    internal static class ODataMediaTypes
    {
        private static readonly MediaTypeHeaderValue _applicationJson = 
            new MediaTypeHeaderValue("application/json");

        private static readonly MediaTypeHeaderValue _applicationJsonODataFullMetadata =
            MediaTypeHeaderValue.Parse("application/json;odata.metadata=full");

        private static readonly MediaTypeHeaderValue _applicationJsonODataFullMetadataStreamingFalse =
            MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=false");

        private static readonly MediaTypeHeaderValue _applicationJsonODataFullMetadataStreamingTrue =
            MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=true");

        private static readonly MediaTypeHeaderValue _applicationJsonODataMinimalMetadata =
            MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal");

        private static readonly MediaTypeHeaderValue _applicationJsonODataMinimalMetadataStreamingFalse =
            MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=false");

        private static readonly MediaTypeHeaderValue _applicationJsonODataMinimalMetadataStreamingTrue =
            MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=true");

        private static readonly MediaTypeHeaderValue _applicationJsonODataNoMetadata =
            MediaTypeHeaderValue.Parse("application/json;odata.metadata=none");

        private static readonly MediaTypeHeaderValue _applicationJsonODataNoMetadataStreamingFalse =
            MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=false");

        private static readonly MediaTypeHeaderValue _applicationJsonODataNoMetadataStreamingTrue =
            MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=true");

        private static readonly MediaTypeHeaderValue _applicationJsonStreamingFalse =
            MediaTypeHeaderValue.Parse("application/json;odata.streaming=false");

        private static readonly MediaTypeHeaderValue _applicationJsonStreamingTrue =
            MediaTypeHeaderValue.Parse("application/json;odata.streaming=true");

        private static readonly MediaTypeHeaderValue _applicationXml = 
            new MediaTypeHeaderValue("application/xml");

        //private static readonly MediaTypeHeaderValue _textPlain = 
        //    new MediaTypeHeaderValue("text/plain");

        public static MediaTypeHeaderValue ApplicationJson => 
            Clone(_applicationJson);

        public static MediaTypeHeaderValue ApplicationJsonODataFullMetadata => 
            Clone(_applicationJsonODataFullMetadata);

        public static MediaTypeHeaderValue ApplicationJsonODataFullMetadataStreamingFalse => 
            Clone(_applicationJsonODataFullMetadataStreamingFalse);

        public static MediaTypeHeaderValue ApplicationJsonODataFullMetadataStreamingTrue => 
            Clone(_applicationJsonODataFullMetadataStreamingTrue);

        public static MediaTypeHeaderValue ApplicationJsonODataMinimalMetadata =>
            Clone(_applicationJsonODataMinimalMetadata);

        public static MediaTypeHeaderValue ApplicationJsonODataMinimalMetadataStreamingFalse =>
            Clone(_applicationJsonODataMinimalMetadataStreamingFalse);

        public static MediaTypeHeaderValue ApplicationJsonODataMinimalMetadataStreamingTrue => 
            Clone(_applicationJsonODataMinimalMetadataStreamingTrue);

        public static MediaTypeHeaderValue ApplicationJsonODataNoMetadata => 
            Clone(_applicationJsonODataNoMetadata);

        public static MediaTypeHeaderValue ApplicationJsonODataNoMetadataStreamingFalse => 
            Clone(_applicationJsonODataNoMetadataStreamingFalse);

        public static MediaTypeHeaderValue ApplicationJsonODataNoMetadataStreamingTrue => 
            Clone(_applicationJsonODataNoMetadataStreamingTrue);

        public static MediaTypeHeaderValue ApplicationJsonStreamingFalse => 
            Clone(_applicationJsonStreamingFalse);

        public static MediaTypeHeaderValue ApplicationJsonStreamingTrue => 
            Clone(_applicationJsonStreamingTrue);

        public static MediaTypeHeaderValue ApplicationXml => 
            Clone(_applicationXml);

        //public static MediaTypeHeaderValue TextPlain => 
        //    Clone(_textPlain);

        public static ODataMetadataLevel GetMetadataLevel(MediaTypeHeaderValue contentType)
        {
            if (contentType == null)
            {
                return ODataMetadataLevel.MinimalMetadata;
            }

            if (!string.Equals(ApplicationJson.MediaType, contentType.MediaType,
                StringComparison.Ordinal))
            {
                return ODataMetadataLevel.MinimalMetadata;
            }

            Contract.Assert(contentType.Parameters != null);
            var odataParameter =
                contentType.Parameters.FirstOrDefault(
                    p => string.Equals("odata.metadata", p.Name, StringComparison.OrdinalIgnoreCase));

            if (odataParameter != null)
            {
                if (string.Equals("full", odataParameter.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return ODataMetadataLevel.FullMetadata;
                }
                if (string.Equals("none", odataParameter.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return ODataMetadataLevel.NoMetadata;
                }
            }

            // Minimal is the default metadata level
            return ODataMetadataLevel.MinimalMetadata;
        }

        private static MediaTypeHeaderValue Clone(MediaTypeHeaderValue contentType)
        {
            return MediaTypeHeaderValue.Parse(contentType.ToString());
        }
    }
}