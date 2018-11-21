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
            
            //If no value for skiptoken is provided; revert to using skip 
            bool useSkipToken = true;
            if(String.IsNullOrWhiteSpace(skipTokenValue))
            {
                useSkipToken = false;
            }

            foreach (KeyValuePair<string, string> kvp in queryParameters)
            {
                string key = kvp.Key.ToLowerInvariant();
                string value = kvp.Value;
                switch (key)
                {
                    case "$top":
                        int top;
                        if (Int32.TryParse(value, out top))
                        {
                            // There is no next page if the $top query option's value is less than or equal to the page size. You should not call this API if top <= pagesize.
                            Contract.Assert(top > pageSize);
                            // We decrease top by the pageSize because that's the number of results we're returning in the current page
                            if (top > pageSize)
                            {
                                value = (top - pageSize).ToString(CultureInfo.InvariantCulture);
                            }
                        }
                        break;
                    case "$skip":
                        //Need to increment skip only if we are not using skiptoken 
                        if (!useSkipToken) 
                        {
                            int skip = 0;
                            if (Int32.TryParse(value, out skip))
                            {
                                // We increase skip by the pageSize because that's the number of results we're returning in the current page
                                nextPageSkip += skip;
                            }

                        }
                        continue;
                    default:
                        break;
                }

                if ( (key=="$skip" && useSkipToken) || (key=="$skiptoken" && useSkipToken))
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

            if (useSkipToken)
            {
                queryBuilder.AppendFormat("$skiptoken={0}", skipTokenValue);
            }
            else
            {
                queryBuilder.AppendFormat("$skip={0}", nextPageSkip );
            }
            UriBuilder uriBuilder = new UriBuilder(requestUri)
            {
                Query = queryBuilder.ToString()
            };
            return uriBuilder.Uri;
        }
    }
}
