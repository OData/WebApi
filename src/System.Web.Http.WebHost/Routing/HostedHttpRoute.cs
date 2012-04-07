// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.Routing;

namespace System.Web.Http.WebHost.Routing
{
    internal class HostedHttpRoute : IHttpRoute
    {
        private readonly Route _route;

        public HostedHttpRoute(Route route)
        {
            if (route == null)
            {
                throw Error.ArgumentNull("route");
            }

            _route = route;
        }

        public string RouteTemplate
        {
            get { return _route.Url; }
        }

        public IDictionary<string, object> Defaults
        {
            get { return _route.Defaults; }
        }

        public IDictionary<string, object> Constraints
        {
            get { return _route.Constraints; }
        }

        public IDictionary<string, object> DataTokens
        {
            get { return _route.DataTokens; }
        }

        internal Route OriginalRoute
        {
            get { return _route; }
        }

        public IHttpRouteData GetRouteData(string rootVirtualPath, HttpRequestMessage request)
        {
            if (rootVirtualPath == null)
            {
                throw Error.ArgumentNull("rootVirtualPath");
            }

            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            HttpContextBase httpContextBase;
            if (request.Properties.TryGetValue(HttpControllerHandler.HttpContextBaseKey, out httpContextBase))
            {
                RouteData routeData = _route.GetRouteData(httpContextBase);
                if (routeData != null)
                {
                    return new HostedHttpRouteData(routeData);
                }
            }

            return null;
        }

        public IHttpVirtualPathData GetVirtualPath(HttpControllerContext controllerContext, IDictionary<string, object> values)
        {
            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            HttpContextBase httpContextBase;
            if (controllerContext.Request.Properties.TryGetValue(HttpControllerHandler.HttpContextBaseKey, out httpContextBase))
            {
                HostedHttpRouteData routeData = controllerContext.RouteData as HostedHttpRouteData;
                if (routeData != null)
                {
                    RequestContext requestContext = new RequestContext(httpContextBase, routeData.OriginalRouteData);
                    VirtualPathData virtualPathData = _route.GetVirtualPath(requestContext, new RouteValueDictionary(values));
                    if (virtualPathData != null)
                    {
                        return new HostedHttpVirtualPathData(virtualPathData);
                    }
                }
            }

            return null;
        }
    }
}
