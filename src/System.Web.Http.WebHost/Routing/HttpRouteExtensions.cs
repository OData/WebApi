// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Routing;
using System.Web.Routing;

namespace System.Web.Http.WebHost.Routing
{
    internal static class HttpRouteExtensions
    {
        public static Route ToRoute(this IHttpRoute httpRoute)
        {
            if (httpRoute == null)
            {
                throw Error.ArgumentNull("httpRoute");
            }

            HostedHttpRoute hostedHttpRoute = httpRoute as HostedHttpRoute;
            if (hostedHttpRoute != null)
            {
                return hostedHttpRoute.OriginalRoute;
            }

            // If the httpRoute is an IgnoreRoute, replace the HttpRoute with an IgnoreRouteInternal,
            // which is a Route instead of a HttpWebRoute.
            if (httpRoute.Handler is System.Web.Http.Routing.StopRoutingHandler)
            {
                return new IgnoreRouteInternal(
                    httpRoute.RouteTemplate,
                    MakeRouteValueDictionary(httpRoute.Defaults),
                    MakeRouteValueDictionary(httpRoute.Constraints),
                    MakeRouteValueDictionary(httpRoute.DataTokens),
                    new System.Web.Routing.StopRoutingHandler());
            }

            return new HttpWebRoute(
                httpRoute.RouteTemplate,
                MakeRouteValueDictionary(httpRoute.Defaults),
                MakeRouteValueDictionary(httpRoute.Constraints),
                MakeRouteValueDictionary(httpRoute.DataTokens),
                HttpControllerRouteHandler.Instance,
                httpRoute);
        }

        private static RouteValueDictionary MakeRouteValueDictionary(IDictionary<string, object> dictionary)
        {
            return dictionary == null
                ? new RouteValueDictionary()
                : new RouteValueDictionary(dictionary);
        }

        private sealed class IgnoreRouteInternal : Route
        {
            public IgnoreRouteInternal(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler)
                : base(url, defaults, constraints, dataTokens, routeHandler)
            {
            }

            public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary routeValues)
            {
                // Never match during route generation. This avoids the scenario where an IgnoreRoute with
                // fairly relaxed constraints ends up eagerly matching all generated URLs.
                return null;
            }
        }
    }
}
