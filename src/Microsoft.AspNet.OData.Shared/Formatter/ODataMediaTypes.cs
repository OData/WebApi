//-----------------------------------------------------------------------------
// <copyright file="ODataMediaTypes.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Contains media types used by the OData formatter.
    /// </summary>
    internal static class ODataMediaTypes
    {
        public static readonly string ApplicationJson = "application/json";
        public static readonly string ApplicationJsonODataFullMetadata = "application/json;odata.metadata=full";
        public static readonly string ApplicationJsonODataFullMetadataStreamingFalse = "application/json;odata.metadata=full;odata.streaming=false";
        public static readonly string ApplicationJsonODataFullMetadataStreamingTrue = "application/json;odata.metadata=full;odata.streaming=true";
        public static readonly string ApplicationJsonODataMinimalMetadata = "application/json;odata.metadata=minimal";
        public static readonly string ApplicationJsonODataMinimalMetadataStreamingFalse = "application/json;odata.metadata=minimal;odata.streaming=false";
        public static readonly string ApplicationJsonODataMinimalMetadataStreamingTrue = "application/json;odata.metadata=minimal;odata.streaming=true";
        public static readonly string ApplicationJsonODataNoMetadata = "application/json;odata.metadata=none";
        public static readonly string ApplicationJsonODataNoMetadataStreamingFalse = "application/json;odata.metadata=none;odata.streaming=false";
        public static readonly string ApplicationJsonODataNoMetadataStreamingTrue = "application/json;odata.metadata=none;odata.streaming=true";
        public static readonly string ApplicationJsonStreamingFalse = "application/json;odata.streaming=false";
        public static readonly string ApplicationJsonStreamingTrue = "application/json;odata.streaming=true";
        public static readonly string ApplicationXml = "application/xml";

        public static ODataMetadataLevel GetMetadataLevel(string mediaType, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            if (mediaType == null)
            {
                return ODataMetadataLevel.MinimalMetadata;
            }

            if (!String.Equals(ODataMediaTypes.ApplicationJson, mediaType,
                StringComparison.Ordinal))
            {
                return ODataMetadataLevel.MinimalMetadata;
            }

            Contract.Assert(parameters != null);
            KeyValuePair<string, string> odataParameter =
                parameters.FirstOrDefault(
                    (p) => String.Equals("odata.metadata", p.Key, StringComparison.OrdinalIgnoreCase));

            if (!odataParameter.Equals(default(KeyValuePair<string, string>)))
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
