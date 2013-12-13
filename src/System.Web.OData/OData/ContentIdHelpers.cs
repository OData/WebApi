// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;

namespace System.Web.Http.OData
{
    internal static class ContentIdHelpers
    {
        private const string ContentId = "Content-ID";

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

        public static void CopyContentIdToResponse(HttpRequestMessage request, HttpResponseMessage response)
        {
            Contract.Assert(request != null);
            Contract.Assert(response != null);

            IEnumerable<string> values;
            if (request.Headers.TryGetValues(ContentId, out values))
            {
                response.Headers.TryAddWithoutValidation(ContentId, values);
            }
        }

        public static void AddLocationHeaderToMapping(HttpResponseMessage response, IDictionary<string, string> contentIdToLocationMapping)
        {
            Contract.Assert(response != null);
            Contract.Assert(contentIdToLocationMapping != null);

            IEnumerable<string> values;
            if (response.Headers.TryGetValues(ContentId, out values))
            {
                if (response.Headers.Location != null)
                {
                    contentIdToLocationMapping.Add(values.First(), response.Headers.Location.AbsoluteUri);
                }
            }
        }
    }
}