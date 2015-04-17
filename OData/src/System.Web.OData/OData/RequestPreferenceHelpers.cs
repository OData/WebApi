// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace System.Web.OData
{
    internal static class RequestPreferenceHelpers
    {
        public const string PreferHeaderName = "Prefer";
        public const string ReturnContentHeaderValue = "return=representation";
        public const string ReturnNoContentHeaderValue = "return=minimal";

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

        internal static string GetRequestPreferHeader(HttpRequestMessage request)
        {
            IEnumerable<string> values;
            if (request.Headers.TryGetValues(PreferHeaderName, out values))
            {
                // If there are many "Prefer" headers, pick up the first one.
                return values.FirstOrDefault();
            }

            return null;
        }
    }
}
