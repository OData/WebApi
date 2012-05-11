// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

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
    }
}
