// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Interfaces;

namespace Microsoft.AspNet.OData
{
    internal static class RequestPreferenceHelpers
    {
        public const string PreferHeaderName = "Prefer";
        public const string ReturnContentHeaderValue = "return=representation";
        public const string ReturnNoContentHeaderValue = "return=minimal";

        internal static bool RequestPrefersReturnContent(IWebApiHeaders headers)
        {
            IEnumerable<string> preferences = null;
            if (headers.TryGetValues(PreferHeaderName, out preferences))
            {
                return preferences.Contains(ReturnContentHeaderValue);
            }
            return false;
        }

        internal static bool RequestPrefersReturnNoContent(IWebApiHeaders headers)
        {
            IEnumerable<string> preferences = null;
            if (headers.TryGetValues(PreferHeaderName, out preferences))
            {
                return preferences.Contains(ReturnNoContentHeaderValue);
            }
            return false;
        }

        internal static string GetRequestPreferHeader(IWebApiHeaders headers)
        {
            IEnumerable<string> values;
            if (headers.TryGetValues(PreferHeaderName, out values))
            {
                // If there are many "Prefer" headers, pick up the first one.
                return values.FirstOrDefault();
            }

            return null;
        }
    }
}
