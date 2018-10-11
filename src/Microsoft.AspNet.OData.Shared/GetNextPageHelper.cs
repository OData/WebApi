// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Helper to generate next page links.
    /// </summary>
    internal static partial class GetNextPageHelper
    {
        internal static Uri GetNextPageLink(Uri requestUri, IEnumerable<KeyValuePair<string, string>> queryParameters, int pageSize, string skipTokenValue="")
        {
            Contract.Assert(requestUri != null);
            Contract.Assert(queryParameters != null);
            Contract.Assert(requestUri.IsAbsoluteUri);

            StringBuilder queryBuilder = new StringBuilder();

            int nextPageSkip = pageSize;

            foreach (KeyValuePair<string, string> kvp in queryParameters)
            {
                string key = kvp.Key;
                string value = kvp.Value;
                switch (key.ToLower())
                {
                    case "$top":
                        int top;
                        if (Int32.TryParse(value, out top))
                        {
                            // There is no next page if the $top query option's value is less than or equal to the page size.
                            Contract.Assert(top > pageSize);
                            // We decrease top by the pageSize because that's the number of results we're returning in the current page
                            value = (top - pageSize).ToString(CultureInfo.InvariantCulture);
                        }
                        break;
                    //    case "$skip":
                    //        int skip;
                    //        if (Int32.TryParse(value, out skip))
                    //        {
                    //            // We increase skip by the pageSize because that's the number of results we're returning in the current page
                    //            nextPageSkip += skip;
                    //        }
                    //        continue;
                    default:
                        break;
                }

                if(key.ToLower()=="$skip" || key.ToLower()=="$skiptoken")
                {
                    continue;
                }

                if (key.Length > 0 && key[0] == '$')
                {
                    // $ is a legal first character in query keys
                    key = '$' + Uri.EscapeDataString(key.Substring(1));
                }
                else
                {
                    key = Uri.EscapeDataString(key);
                }
                value = Uri.EscapeDataString(value);

                queryBuilder.Append(key);
                queryBuilder.Append('=');
                queryBuilder.Append(value);
                queryBuilder.Append('&');
            }

            queryBuilder.AppendFormat("$skiptoken={0}", skipTokenValue);

            UriBuilder uriBuilder = new UriBuilder(requestUri)
            {
                Query = queryBuilder.ToString()
            };
            return uriBuilder.Uri;
        }
    }
}
