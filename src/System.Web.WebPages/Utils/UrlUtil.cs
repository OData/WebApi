// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;
using System.Text;
using System.Web.Routing;

namespace System.Web.WebPages
{
    internal static class UrlUtil
    {
        private static UrlRewriterHelper _urlRewriterHelper = new UrlRewriterHelper();

        // this method can accept an app-relative path or an absolute path for contentPath
        public static string GenerateClientUrl(HttpContextBase httpContext, string contentPath)
        {
            if (String.IsNullOrEmpty(contentPath))
            {
                return contentPath;
            }

            // many of the methods we call internally can't handle query strings properly, so just strip it out for
            // the time being
            string query;
            contentPath = StripQuery(contentPath, out query);

            return GenerateClientUrlInternal(httpContext, contentPath) + query;
        }

        public static string GenerateClientUrl(HttpContextBase httpContext, string basePath, string path, params object[] pathParts)
        {
            if (basePath != null)
            {
                path = VirtualPathUtility.Combine(basePath, path);
            }

            Tuple<string, string> contentPathAndQuery = BuildUrl(path, pathParts);

            // many of the methods we call internally can't handle query strings properly, so take it on after processing
            // the virtual app path and url rewrites
            return GenerateClientUrlInternal(httpContext, contentPathAndQuery.Item1) + contentPathAndQuery.Item2;
        }

        private static string GenerateClientUrlInternal(HttpContextBase httpContext, string contentPath)
        {
            if (String.IsNullOrEmpty(contentPath))
            {
                return contentPath;
            }

            // can't call VirtualPathUtility.IsAppRelative since it throws on some inputs
            bool isAppRelative = contentPath[0] == '~';
            if (isAppRelative)
            {
                string absoluteContentPath = VirtualPathUtility.ToAbsolute(contentPath, httpContext.Request.ApplicationPath);
                string modifiedAbsoluteContentPath = httpContext.Response.ApplyAppPathModifier(absoluteContentPath);
                return GenerateClientUrlInternal(httpContext, modifiedAbsoluteContentPath);
            }

            // we only want to manipulate the path if URL rewriting is active for this request, else we risk breaking the generated URL
            bool wasRequestRewritten = _urlRewriterHelper.WasRequestRewritten(httpContext);
            if (!wasRequestRewritten)
            {
                return contentPath;
            }

            // Since the rawUrl represents what the user sees in his browser, it is what we want to use as the base
            // of our absolute paths. For example, consider mysite.example.com/foo, which is internally
            // rewritten to content.example.com/mysite/foo. When we want to generate a link to ~/bar, we want to
            // base it from / instead of /foo, otherwise the user ends up seeing mysite.example.com/foo/bar,
            // which is incorrect.
            string relativeUrlToDestination = MakeRelative(httpContext.Request.Path, contentPath);
            string absoluteUrlToDestination = MakeAbsolute(httpContext.Request.RawUrl, relativeUrlToDestination);
            return absoluteUrlToDestination;
        }

        public static string MakeAbsolute(string basePath, string relativePath)
        {
            // The Combine() method can't handle query strings on the base path, so we trim it off.
            string query;
            basePath = StripQuery(basePath, out query);
            return VirtualPathUtility.Combine(basePath, relativePath);
        }

        public static string MakeRelative(string fromPath, string toPath)
        {
            string relativeUrl = VirtualPathUtility.MakeRelative(fromPath, toPath);
            if (String.IsNullOrEmpty(relativeUrl) || relativeUrl[0] == '?')
            {
                // Sometimes VirtualPathUtility.MakeRelative() will return an empty string when it meant to return '.',
                // but links to {empty string} are browser dependent. We replace it with an explicit path to force
                // consistency across browsers.
                relativeUrl = "./" + relativeUrl;
            }
            return relativeUrl;
        }

        private static string StripQuery(string path, out string query)
        {
            int queryIndex = path.IndexOf('?');
            if (queryIndex >= 0)
            {
                query = path.Substring(queryIndex);
                return path.Substring(0, queryIndex);
            }
            else
            {
                query = null;
                return path;
            }
        }

        internal static void ResetUrlRewriterHelper()
        {
            _urlRewriterHelper = new UrlRewriterHelper();
        }

        internal static Tuple<string, string> BuildUrl(string path, params object[] pathParts)
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
            return Tuple.Create(path, queryString.ToString());
        }

        private static bool IsDisplayableType(Type t)
        {
            // If it doesn't support any interfaces (e.g. IFormattable), we probably can't display it.  It's likely an anonymous type.
            // REVIEW: this is hacky!
            return t.GetInterfaces().Length > 0;
        }
    }
}
