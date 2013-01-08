// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="UrlHelper"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ODataUrlHelperExtensions
    {
        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string ODataLink(this UrlHelper urlHelper, params ODataPathSegment[] segments)
        {
            return urlHelper.ODataLink(segments as IList<ODataPathSegment>);
        }

        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string ODataLink(this UrlHelper urlHelper, IList<ODataPathSegment> segments)
        {
            if (urlHelper == null)
            {
                throw Error.ArgumentNull("urlHelper");
            }

            HttpRequestMessage request = urlHelper.Request;
            Contract.Assert(request != null);

            string routeName = request.GetODataRouteName() ?? ODataRouteConstants.DefaultRouteName;
            IODataPathHandler pathHandler = request.GetODataPathHandler();
            return urlHelper.ODataLink(routeName, pathHandler, segments);
        }

        /// <summary>
        /// Generates an OData link using the given OData route name and path handler.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="routeName">The name of the OData route.</param>
        /// <param name="pathHandler">The path handler to use for generating the link.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string ODataLink(this UrlHelper urlHelper, string routeName, IODataPathHandler pathHandler, IList<ODataPathSegment> segments)
        {
            if (urlHelper == null)
            {
                throw Error.ArgumentNull("urlHelper");
            }

            if (pathHandler == null)
            {
                throw Error.ArgumentNull("pathHandler");
            }

            string odataPath = pathHandler.Link(new ODataPath(segments));

            string directLink = urlHelper.GenerateLinkDirectly(routeName, odataPath);
            if (directLink != null)
            {
                return directLink;
            }

            // Slow path : use urlHelper.Link because the fast path failed
            return urlHelper.Link(
                routeName,
                new HttpRouteValueDictionary() { { ODataRouteConstants.ODataPath, odataPath } });
        }

        // Fast path link generation where we recognize an OData route of the form "prefix/{*odataPath}".
        // Link generation using HttpRoute.GetVirtualPath can consume up to 30% of processor time
        // This is incredibly brittle code and should be removed whenever OData is upgraded to the next version of the runtime
        // The right long-term fix is to remove this code and introduce an ODataRoute class that computes GetVirtualPath much faster
        // But the long-term fix cannot be implemented because of a critical bug in WebAPI routing that affects custom routes in WebHost
        // Removing this fast path is tracked by Issue 713
        internal static string GenerateLinkDirectly(this UrlHelper urlHelper, string linkRouteName, string odataPath)
        {
            HttpRequestMessage request = urlHelper.Request;
            HttpConfiguration config = request.GetConfiguration();
            if (config != null)
            {
                IHttpRoute odataRoute;
                if (config.Routes.TryGetValue(linkRouteName, out odataRoute))
                {
                    string routeTemplate = odataRoute.RouteTemplate;
                    if (routeTemplate.EndsWith(ODataRouteConstants.ODataPathTemplate, StringComparison.Ordinal))
                    {
                        int odataPathTemplateIndex = routeTemplate.Length - ODataRouteConstants.ODataPathTemplate.Length;
                        int indexOfFirstOpenBracket = routeTemplate.IndexOf('{');

                        // We can only fast-path if there are no open brackets in the route prefix that need to be replaced
                        // If there are, fall back to the slow path.
                        if (indexOfFirstOpenBracket == odataPathTemplateIndex)
                        {
                            string virtualPathRoot = config.VirtualPathRoot;
                            if (!virtualPathRoot.EndsWith("/", StringComparison.Ordinal))
                            {
                                virtualPathRoot += "/";
                            }

                            string routePrefix = routeTemplate.Substring(0, odataPathTemplateIndex);
                            string link = virtualPathRoot + routePrefix + odataPath;
                            return new Uri(request.RequestUri, link).AbsoluteUri;
                        }
                    }
                }
            }
            return null;
        }
    }
}
