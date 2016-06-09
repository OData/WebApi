// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.OData
{
    internal static class RequestPreferenceHelpers
    {
        public const string PreferHeaderName = "Prefer";
        public const string ReturnContentHeaderValue = "return=representation";
        public const string ReturnNoContentHeaderValue = "return=minimal";

        internal static bool RequestPrefersReturnContent(HttpRequest request)
        {
            StringValues preferences;
            if (request.Headers.TryGetValue(PreferHeaderName, out preferences))
            {
                return preferences.Contains(ReturnContentHeaderValue);
            }
            return false;
        }

        internal static bool RequestPrefersReturnNoContent(HttpRequest request)
        {
            StringValues preferences;
            if (request.Headers.TryGetValue(PreferHeaderName, out preferences))
            {
                return preferences.Contains(ReturnNoContentHeaderValue);
            }
            return false;
        }

        internal static string GetRequestPreferHeader(HttpRequest request)
        {
            StringValues values;
            if (request.Headers.TryGetValue(PreferHeaderName, out values))
            {
                // If there are many "Prefer" headers, pick up the first one.
                return values.FirstOrDefault();
            }

            return null;
        }
    }
}
