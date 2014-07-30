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

            int startIndex = 0;

            while (true)
            {
                startIndex = url.IndexOf('$', startIndex);

                if (startIndex == -1)
                {
                    break;
                }

                int keyLength = 0;

                while (startIndex + keyLength < url.Length - 1 && IsContentIdCharacter(url[startIndex + keyLength + 1]))
                {
                    keyLength++;
                }

                if (keyLength > 0)
                {
                    // Might have matched a $<content-id> alias.
                    string locationKey = url.Substring(startIndex + 1, keyLength);
                    string locationValue;

                    if (contentIdToLocationMapping.TryGetValue(locationKey, out locationValue))
                    {
                        // As location headers MUST be absolute URL's, we can ignore everything 
                        // before the $content-id while resolving it.
                        return locationValue + url.Substring(startIndex + 1 + keyLength);
                    }
                }

                startIndex++;
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

        private static bool IsContentIdCharacter(char c)
        {
            // According to the OData ABNF grammar, Content-IDs follow the scheme.
            // content-id = "Content-ID" ":" OWS 1*unreserved
            // unreserved    = ALPHA / DIGIT / "-" / "." / "_" / "~"
            switch (c)
            {
                case '-':
                case '.':
                case '_':
                case '~':
                    return true;
                default:
                    return Char.IsLetterOrDigit(c);
            }
        }
    }
}