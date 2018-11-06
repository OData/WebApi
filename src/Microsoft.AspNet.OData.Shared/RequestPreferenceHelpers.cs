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
        public const string ODataMaxPageSize = "odata.maxpagesize";
        public const string MaxPageSize = "maxpagesize";

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

        internal static bool RequestPrefersMaxPageSize(IWebApiHeaders headers, out int pageSize)
        {
            pageSize = -1;
            IEnumerable<string> preferences = null;
            if (headers.TryGetValues(PreferHeaderName, out preferences))
            {
                pageSize = GetMaxPageSize(preferences, MaxPageSize);
                if (pageSize >= 0)
                {
                    return true;
                }
                //maxpagesize gets supersedes odata.maxpagesize
                pageSize = GetMaxPageSize(preferences, ODataMaxPageSize);
                if (pageSize >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static int GetMaxPageSize(IEnumerable<string> Preferences, string preferenceHeaderName)
        {
            const int failed = -1;
            string maxPageSize = Preferences.FirstOrDefault(s => s.Contains(preferenceHeaderName));
            if (string.IsNullOrEmpty(maxPageSize))
            {
                return failed;
            }
            else
            {
                maxPageSize = maxPageSize.ToLower();
                int index = maxPageSize.IndexOf(preferenceHeaderName) + preferenceHeaderName.Length;
                string value = "";
                if(maxPageSize[index++]=='=')
                {
                    while (index < maxPageSize.Length && char.IsDigit(maxPageSize[index]))
                    {
                        value += maxPageSize[index++];
                    }
                }
                if (int.TryParse(value, out int pageSize))
                {
                    return pageSize;
                }
            }
            return failed; 
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
