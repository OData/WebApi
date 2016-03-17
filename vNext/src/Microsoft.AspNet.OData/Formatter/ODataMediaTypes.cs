// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.OData.Formatter
{
    /// <summary>
    /// Contains media types used by the OData formatter.
    /// </summary>
    internal static class ODataMediaTypes
    {
        private static readonly MediaTypeHeaderValue _applicationJson = new MediaTypeHeaderValue("application/json");
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
        private static readonly MediaTypeHeaderValue _applicationXml = new MediaTypeHeaderValue("application/xml");

        public static MediaTypeHeaderValue ApplicationJson
        {
            get { return Clone(_applicationJson); }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataFullMetadata
        {
            get { return Clone(_applicationJsonODataFullMetadata); }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataFullMetadataStreamingFalse
        {
            get { return Clone(_applicationJsonODataFullMetadataStreamingFalse); }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataFullMetadataStreamingTrue
        {
            get { return Clone(_applicationJsonODataFullMetadataStreamingTrue); }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataMinimalMetadata
        {
            get { return Clone(_applicationJsonODataMinimalMetadata); }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataMinimalMetadataStreamingFalse
        {
            get
            {
                return Clone(_applicationJsonODataMinimalMetadataStreamingFalse);
            }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataMinimalMetadataStreamingTrue
        {
            get
            {
                return Clone(_applicationJsonODataMinimalMetadataStreamingTrue);
            }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataNoMetadata
        {
            get { return Clone(_applicationJsonODataNoMetadata); }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataNoMetadataStreamingFalse
        {
            get { return Clone(_applicationJsonODataNoMetadataStreamingFalse); }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataNoMetadataStreamingTrue
        {
            get { return Clone(_applicationJsonODataNoMetadataStreamingTrue); }
        }

        public static MediaTypeHeaderValue ApplicationJsonStreamingFalse
        {
            get { return Clone(_applicationJsonStreamingFalse); }
        }

        public static MediaTypeHeaderValue ApplicationJsonStreamingTrue
        {
            get { return Clone(_applicationJsonStreamingTrue); }
        }

        public static MediaTypeHeaderValue ApplicationXml
        {
            get { return Clone(_applicationXml); }
        }

        public static ODataMetadataLevel GetMetadataLevel(MediaTypeHeaderValue contentType)
        {
            if (contentType == null)
            {
                return ODataMetadataLevel.MinimalMetadata;
            }

            if (!String.Equals(ODataMediaTypes.ApplicationJson.MediaType, contentType.MediaType,
                StringComparison.Ordinal))
            {
                return ODataMetadataLevel.MinimalMetadata;
            }

            Contract.Assert(contentType.Parameters != null);
            NameValueHeaderValue odataParameter =
                contentType.Parameters.FirstOrDefault(
                    (p) => String.Equals("odata.metadata", p.Name, StringComparison.OrdinalIgnoreCase));

            if (odataParameter != null)
            {
                if (String.Equals("full", odataParameter.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return ODataMetadataLevel.FullMetadata;
                }
                if (String.Equals("none", odataParameter.Value, StringComparison.OrdinalIgnoreCase))
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
