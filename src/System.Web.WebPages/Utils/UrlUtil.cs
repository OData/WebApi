// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;
using System.Text;
using System.Web.Routing;

namespace System.Web.WebPages
{
    internal static class UrlUtil
    {
        internal static string Url(string basePath, string path, params object[] pathParts)
        {
            if (basePath != null)
            {
                path = VirtualPathUtility.Combine(basePath, path);
            }

            // Make sure it's not a ~/ path, which the client couldn't handle
            path = VirtualPathUtility.ToAbsolute(path);

            return BuildUrl(path, pathParts);
        }

        internal static string BuildUrl(string path, params object[] pathParts)
        {
            path = HttpUtility.UrlPathEncode(path);
            StringBuilder queryString = new StringBuilder();

            foreach (var pathPart in pathParts)
            {
                Type partType = pathPart.GetType();
                if (IsDisplayableType(partType))
                {
                    var displayablePath = Convert.ToString(pathPart, CultureInfo.InvariantCulture);
                    path += "/" + HttpUtility.UrlPathEncode(displayablePath);
                }
                else
                {
                    // If it smells like an anonymous object, treat it as query string name/value pairs instead of path info parts
                    // REVIEW: this is hacky!
                    var dictionary = new RouteValueDictionary(pathPart);
                    foreach (var item in dictionary)
                    {
                        if (queryString.Length == 0)
                        {
                            queryString.Append('?');
                        }
                        else
                        {
                            queryString.Append('&');
                        }

                        string stringValue = Convert.ToString(item.Value, CultureInfo.InvariantCulture);

                        queryString.Append(HttpUtility.UrlEncode(item.Key))
                            .Append('=')
                            .Append(HttpUtility.UrlEncode(stringValue));
                    }
                }
            }
            return path + queryString;
        }

        private static bool IsDisplayableType(Type t)
        {
            // If it doesn't support any interfaces (e.g. IFormattable), we probably can't display it.  It's likely an anonymous type.
            // REVIEW: this is hacky!
            return t.GetInterfaces().Length > 0;
        }
    }
}
