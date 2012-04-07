// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Hosting;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.Http.WebHost.Properties;
using System.Web.Routing;

namespace System.Web.Http.WebHost.Routing
{
    internal class HostedHttpRouteCollection : HttpRouteCollection
    {
        private readonly RouteCollection _routeCollection;

        public HostedHttpRouteCollection(RouteCollection routeCollection)
        {
            if (routeCollection == null)
            {
                throw Error.ArgumentNull("routeCollection");
            }

            _routeCollection = routeCollection;
        }

        public override string VirtualPathRoot
        {
            get { return HostingEnvironment.ApplicationVirtualPath; }
        }

        public override int Count
        {
            get { return _routeCollection.Count; }
        }

        public override IHttpRoute this[string name]
        {
            get
            {
                Route route = _routeCollection[name] as Route;
                if (route != null)
                {
                    return new HostedHttpRoute(route);
                }

                throw Error.KeyNotFound();
            }
        }

        public override IHttpRoute this[int index]
        {
            get
            {
                Route route = _routeCollection[index] as Route;
                if (route != null)
                {
                    return new HostedHttpRoute(route);
                }

                throw Error.ArgumentOutOfRange("index", index, SRResources.RouteCollectionOutOfRange);
            }
        }

        public override IHttpRouteData GetRouteData(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            HttpContextBase httpContextBase;
            if (request.Properties.TryGetValue(HttpControllerHandler.HttpContextBaseKey, out httpContextBase))
            {
                RouteData routeData = _routeCollection.GetRouteData(httpContextBase);
                if (routeData != null)
                {
                    return new HostedHttpRouteData(routeData);
                }
            }

            return null;
        }

        public override IHttpVirtualPathData GetVirtualPath(HttpControllerContext controllerContext, string name, IDictionary<string, object> values)
        {
            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            HttpRequestMessage request = controllerContext.Request;
            HttpContextBase httpContextBase;
            if (request.Properties.TryGetValue(HttpControllerHandler.HttpContextBaseKey, out httpContextBase))
            {
                RequestContext requestContext = new RequestContext(httpContextBase, controllerContext.RouteData.ToRouteData());
                RouteValueDictionary routeValues = values != null ? new RouteValueDictionary(values) : new RouteValueDictionary();
                VirtualPathData virtualPathData = _routeCollection.GetVirtualPath(requestContext, name, routeValues);
                if (virtualPathData != null)
                {
                    return new HostedHttpVirtualPathData(virtualPathData);
                }
            }

            return null;
        }

        public override IHttpRoute CreateRoute(string uriTemplate, IDictionary<string, object> defaults, IDictionary<string, object> constraints, IDictionary<string, object> dataTokens, IDictionary<string, object> parameters)
        {
            RouteValueDictionary routeDefaults = defaults != null ? new RouteValueDictionary(defaults) : null;
            RouteValueDictionary routeConstraints = constraints != null ? new RouteValueDictionary(constraints) : null;
            RouteValueDictionary routeDataTokens = dataTokens != null ? new RouteValueDictionary(dataTokens) : null;
            HttpWebRoute route = new HttpWebRoute(uriTemplate, routeDefaults, routeConstraints, routeDataTokens, HttpControllerRouteHandler.Instance);
            return new HostedHttpRoute(route);
        }

        public override void Add(string name, IHttpRoute route)
        {
            _routeCollection.Add(name, route.ToRoute());
        }

        public override void Clear()
        {
            _routeCollection.Clear();
        }

        public override bool Contains(IHttpRoute item)
        {
            HostedHttpRoute hostedHttpRoute = item as HostedHttpRoute;
            if (hostedHttpRoute != null)
            {
                return _routeCollection.Contains(hostedHttpRoute.OriginalRoute);
            }

            return false;
        }

        public override bool ContainsKey(string name)
        {
            return _routeCollection[name] != null;
        }

        public override void CopyTo(IHttpRoute[] array, int arrayIndex)
        {
            throw NotSupportedByHostedRouteCollection();
        }

        public override void CopyTo(KeyValuePair<string, IHttpRoute>[] array, int arrayIndex)
        {
            throw NotSupportedByRouteCollection();
        }

        public override void Insert(int index, string name, IHttpRoute value)
        {
            throw NotSupportedByRouteCollection();
        }

        public override bool Remove(string name)
        {
            throw NotSupportedByRouteCollection();
        }

        public override IEnumerator<IHttpRoute> GetEnumerator()
        {
            // Here we only care about Web API routes.
            return _routeCollection
                .OfType<HttpWebRoute>()
                .Select(httpWebRoute => new HostedHttpRoute(httpWebRoute))
                .GetEnumerator();
        }

        public override bool TryGetValue(string name, out IHttpRoute route)
        {
            Route rt = _routeCollection[name] as Route;
            if (rt != null)
            {
                route = new HostedHttpRoute(rt);
                return true;
            }

            route = null;
            return false;
        }

        private static NotSupportedException NotSupportedByRouteCollection()
        {
            return Error.NotSupported(SRResources.RouteCollectionNotSupported, typeof(RouteCollection).Name);
        }

        private static NotSupportedException NotSupportedByHostedRouteCollection()
        {
            return Error.NotSupported(SRResources.RouteCollectionUseDirectly, typeof(RouteCollection).Name);
        }
    }
}
