// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

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

            return new HttpWebRoute(httpRoute.RouteTemplate, HttpControllerRouteHandler.Instance)
            {
                Defaults = new RouteValueDictionary(httpRoute.Defaults),
                Constraints = new RouteValueDictionary(httpRoute.Constraints),
                DataTokens = new RouteValueDictionary(httpRoute.DataTokens),
            };
        }
    }
}
