// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Headers;

namespace System.Web.OData.Formatter
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
            get { return (MediaTypeHeaderValue)((ICloneable)_applicationJson).Clone(); }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataFullMetadata
        {
            get { return (MediaTypeHeaderValue)((ICloneable)_applicationJsonODataFullMetadata).Clone(); }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataFullMetadataStreamingFalse
        {
            get { return (MediaTypeHeaderValue)((ICloneable)_applicationJsonODataFullMetadataStreamingFalse).Clone(); }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataFullMetadataStreamingTrue
        {
            get { return (MediaTypeHeaderValue)((ICloneable)_applicationJsonODataFullMetadataStreamingTrue).Clone(); }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataMinimalMetadata
        {
            get { return (MediaTypeHeaderValue)((ICloneable)_applicationJsonODataMinimalMetadata).Clone(); }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataMinimalMetadataStreamingFalse
        {
            get
            {
                return (MediaTypeHeaderValue)((ICloneable)_applicationJsonODataMinimalMetadataStreamingFalse).Clone();
            }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataMinimalMetadataStreamingTrue
        {
            get
            {
                return (MediaTypeHeaderValue)((ICloneable)_applicationJsonODataMinimalMetadataStreamingTrue).Clone();
            }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataNoMetadata
        {
            get { return (MediaTypeHeaderValue)((ICloneable)_applicationJsonODataNoMetadata).Clone(); }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataNoMetadataStreamingFalse
        {
            get { return (MediaTypeHeaderValue)((ICloneable)_applicationJsonODataNoMetadataStreamingFalse).Clone(); }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataNoMetadataStreamingTrue
        {
            get { return (MediaTypeHeaderValue)((ICloneable)_applicationJsonODataNoMetadataStreamingTrue).Clone(); }
        }

        public static MediaTypeHeaderValue ApplicationJsonStreamingFalse
        {
            get { return (MediaTypeHeaderValue)((ICloneable)_applicationJsonStreamingFalse).Clone(); }
        }

        public static MediaTypeHeaderValue ApplicationJsonStreamingTrue
        {
            get { return (MediaTypeHeaderValue)((ICloneable)_applicationJsonStreamingTrue).Clone(); }
        }

        public static MediaTypeHeaderValue ApplicationXml
        {
            get { return (MediaTypeHeaderValue)((ICloneable)_applicationXml).Clone(); }
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
    }
}
