// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using System.Web.Mvc.Routing;

namespace System.Web.Routing.Test
{
    internal static class DirectRouteTestHelpers
    {
        public static RouteCollectionRoute BuildDirectRouteFromMethod<T>(Expression<Action<T>> methodCall)
        {
            var route = new RouteCollectionRoute();
            AddDirectRouteFromMethod(route, methodCall);
            return route;
        }

        public static void AddDirectRouteFromMethod<T>(this RouteBase routeBase, Expression<Action<T>> methodCall)
        {
            RouteCollectionRoute route = (RouteCollectionRoute)routeBase;

            var method = ((MethodCallExpression)methodCall.Body).Method;
            var attributes = method.GetCustomAttributes(false).OfType<IRouteInfoProvider>();

            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(method.DeclaringType);
            var actionDescriptor = new ReflectedActionDescriptor(method, method.Name, controllerDescriptor);

            foreach (var attribute in attributes)
            {
                var subRoute = new Route(attribute.Template, routeHandler: null);
                subRoute.SetTargetActionDescriptors(new ActionDescriptor[] { actionDescriptor });
                subRoute.SetTargetControllerDescriptor(controllerDescriptor);
                route.SubRoutes.Add(subRoute);
            }
        }

        public static RouteCollectionRoute BuildDirectRouteFromController<T>()
        {
            RouteCollectionRoute route = new RouteCollectionRoute();
            AddDirectRouteFromController<T>(route);
            return route;
        }

        public static void AddDirectRouteFromController<T>(this RouteBase routeBase)
        {
            RouteCollectionRoute route = (RouteCollectionRoute)routeBase;

            var controllerType = typeof(T);
            var entries = new AttributeRoutingMapper(new RouteBuilder2()).MapMvcAttributeRoutes(new Type[] 
            { 
                controllerType,
            });

            foreach (var entry in entries)
            {
                route.SubRoutes.Add(entry.Route);
            }
        }

        public static void AddDirectRouteMatches(this RouteData routeData, Func<Route, RouteData, bool> selector = null)
        {
            RouteCollectionRoute route = (RouteCollectionRoute)routeData.Route;

            List<RouteData> matches = new List<RouteData>();
            foreach (var subRoute in route.SubRoutes)
            {
                RouteData match = new RouteData() { Route = subRoute };
                bool isMatch = selector == null ? true : selector(subRoute, match);
                if (isMatch)
                {
                    matches.Add(match);
                }
            }

            if (matches.Any())
            {
                routeData.SetDirectRouteMatches(matches);
            }
        }
    }
}