// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
        public static readonly string ApplicationJsonIEEE754CompatibleTrue  = "application/json;IEEE754Compatible=true";
        public static readonly string ApplicationJsonIEEE754CompatibleFalse = "application/json;IEEE754Compatible=false";
        public static readonly string ApplicationJsonODataFullMetadata = "application/json;odata.metadata=full";
        public static readonly string ApplicationJsonODataFullMetadataIEEE754CompatibleTrue  = "application/json;odata.metadata=full;IEEE754Compatible=true";
        public static readonly string ApplicationJsonODataFullMetadataIEEE754CompatibleFalse = "application/json;odata.metadata=full;IEEE754Compatible=false";
        public static readonly string ApplicationJsonODataFullMetadataStreamingFalse = "application/json;odata.metadata=full;odata.streaming=false";
        public static readonly string ApplicationJsonODataFullMetadataStreamingFalseIEEE754CompatibleTrue  = "application/json;odata.metadata=full;odata.streaming=false;IEEE754Compatible=true";
        public static readonly string ApplicationJsonODataFullMetadataStreamingFalseIEEE754CompatibleFalse = "application/json;odata.metadata=full;odata.streaming=false;IEEE754Compatible=false";
        public static readonly string ApplicationJsonODataFullMetadataStreamingTrue = "application/json;odata.metadata=full;odata.streaming=true";
        public static readonly string ApplicationJsonODataFullMetadataStreamingTrueIEEE754CompatibleTrue  = "application/json;odata.metadata=full;odata.streaming=true;IEEE754Compatible=true";
        public static readonly string ApplicationJsonODataFullMetadataStreamingTrueIEEE754CompatibleFalse = "application/json;odata.metadata=full;odata.streaming=true;IEEE754Compatible=false";
        public static readonly string ApplicationJsonODataMinimalMetadata = "application/json;odata.metadata=minimal";
        public static readonly string ApplicationJsonODataMinimalMetadataIEEE754CompatibleTrue  = "application/json;odata.metadata=minimal;IEEE754Compatible=true";
        public static readonly string ApplicationJsonODataMinimalMetadataIEEE754CompatibleFalse = "application/json;odata.metadata=minimal;IEEE754Compatible=false";
        public static readonly string ApplicationJsonODataMinimalMetadataStreamingFalse = "application/json;odata.metadata=minimal;odata.streaming=false";
        public static readonly string ApplicationJsonODataMinimalMetadataStreamingFalseIEEE754CompatibleTrue  = "application/json;odata.metadata=minimal;odata.streaming=false;IEEE754Compatible=true";
        public static readonly string ApplicationJsonODataMinimalMetadataStreamingFalseIEEE754CompatibleFalse = "application/json;odata.metadata=minimal;odata.streaming=false;IEEE754Compatible=false";
        public static readonly string ApplicationJsonODataMinimalMetadataStreamingTrue = "application/json;odata.metadata=minimal;odata.streaming=true";
        public static readonly string ApplicationJsonODataMinimalMetadataStreamingTrueIEEE754CompatibleTrue  = "application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=true";
        public static readonly string ApplicationJsonODataMinimalMetadataStreamingTrueIEEE754CompatibleFalse = "application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false";
        public static readonly string ApplicationJsonODataNoMetadata = "application/json;odata.metadata=none";
        public static readonly string ApplicationJsonODataNoMetadataIEEE754CompatibleTrue  = "application/json;odata.metadata=none;IEEE754Compatible=true";
        public static readonly string ApplicationJsonODataNoMetadataIEEE754CompatibleFalse = "application/json;odata.metadata=none;IEEE754Compatible=false";
        public static readonly string ApplicationJsonODataNoMetadataStreamingFalse = "application/json;odata.metadata=none;odata.streaming=false";
        public static readonly string ApplicationJsonODataNoMetadataStreamingFalseIEEE754CompatibleTrue  = "application/json;odata.metadata=none;odata.streaming=false;IEEE754Compatible=true";
        public static readonly string ApplicationJsonODataNoMetadataStreamingFalseIEEE754CompatibleFalse = "application/json;odata.metadata=none;odata.streaming=false;IEEE754Compatible=false";
        public static readonly string ApplicationJsonODataNoMetadataStreamingTrue = "application/json;odata.metadata=none;odata.streaming=true";
        public static readonly string ApplicationJsonODataNoMetadataStreamingTrueIEEE754CompatibleTrue  = "application/json;odata.metadata=none;odata.streaming=true;IEEE754Compatible=true";
        public static readonly string ApplicationJsonODataNoMetadataStreamingTrueIEEE754CompatibleFalse = "application/json;odata.metadata=none;odata.streaming=true;IEEE754Compatible=false";
        public static readonly string ApplicationJsonStreamingFalse = "application/json;odata.streaming=false";
        public static readonly string ApplicationJsonStreamingFalseIEEE754CompatibleTrue  = "application/json;odata.streaming=false;IEEE754Compatible=true";
        public static readonly string ApplicationJsonStreamingFalseIEEE754CompatibleFalse = "application/json;odata.streaming=false;IEEE754Compatible=false";
        public static readonly string ApplicationJsonStreamingTrue = "application/json;odata.streaming=true";
        public static readonly string ApplicationJsonStreamingTrueIEEE754CompatibleTrue  = "application/json;odata.streaming=true;IEEE754Compatible=true";
        public static readonly string ApplicationJsonStreamingTrueIEEE754CompatibleFalse = "application/json;odata.streaming=true;IEEE754Compatible=false";

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
