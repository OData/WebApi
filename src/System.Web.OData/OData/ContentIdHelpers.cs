// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;

namespace System.Web.OData
{
    internal static class ContentIdHelpers
    {
        public static string ResolveContentId(string url, IDictionary<string, string> contentIdToLocationMapping)
        {
            Contract.Assert(url != null);
            Contract.Assert(contentIdToLocationMapping != null);

            foreach (KeyValuePair<string, string> location in contentIdToLocationMapping)
            {
                int index = url.IndexOf("$" + location.Key, StringComparison.Ordinal);
                if (index != -1)
                {
                    // As location headers MUST be absolute URL's, we can ignore everything 
                    // before the $content-id while resolving it.
                    return location.Value + url.Substring(index + 1 + location.Key.Length);
                }
            }

            return url;
        }

        public static void AddLocationHeaderToMapping(
            HttpResponseMessage response,
            IDictionary<string, string> contentIdToLocationMapping,
            string contentId)
        {
            Contract.Assert(response != null);
            Contract.Assert(contentIdToLocationMapping != null);
            Contract.Assert(contentId != null);

            if (response.Headers.Location != null)
            {
                contentIdToLocationMapping.Add(contentId, response.Headers.Location.AbsoluteUri);
            }
        }
    }
}