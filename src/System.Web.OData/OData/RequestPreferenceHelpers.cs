// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace System.Web.OData
{
    internal static class RequestPreferenceHelpers
    {
        private const string PreferHeaderName = "Prefer";
        private const string ReturnContentHeaderValue = "return-content";
        private const string ReturnNoContentHeaderValue = "return-no-content";

        internal static bool RequestPrefersReturnContent(HttpRequestMessage request)
        {
            IEnumerable<string> preferences = null;
            if (request.Headers.TryGetValues(PreferHeaderName, out preferences))
            {
                return preferences.Contains(ReturnContentHeaderValue);
            }
            return false;
        }

        internal static bool RequestPrefersReturnNoContent(HttpRequestMessage request)
        {
            IEnumerable<string> preferences = null;
            if (request.Headers.TryGetValues(PreferHeaderName, out preferences))
            {
                return preferences.Contains(ReturnNoContentHeaderValue);
            }
            return false;
        }
    }
}
