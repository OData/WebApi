// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    internal static class DirectRouteExtensions
    {
        private const string TargetActionMethodKey = "TargetActionMethod";

        // For [Route] on a controller, get the controller descriptor. 
        // Null if this is not a route specified directly on a controller.
        public static ControllerDescriptor GetTargetControllerDescriptor(this RouteData routeData)
        {
            ControllerDescriptor descriptor = routeData.DataTokens[RouteDataTokenKeys.DirectRouteToController] as ControllerDescriptor;

            return descriptor;
        }

        /// <summary>
        /// Gets the target action method that will be invoked if this route is matched.
        /// </summary>
        public static MethodInfo GetTargetActionMethod(this RouteData routeData)
        {
            if (routeData == null)
            {
                throw Error.ArgumentNull("routeData");
            }

            Route route = routeData.Route as Route;
            if (route == null)
            {
                return null;
            }

            return GetTargetActionMethod(route);
        }

        /// <summary>
        /// Sets the target action method that will be invoked if this route is matched.
        /// </summary>
        public static void SetTargetActionMethod(this Route route, MethodInfo targetMethod)
        {
            if (route == null)
            {
                throw Error.ArgumentNull("route");
            }
            if (route.DataTokens == null)
            {
                route.DataTokens = new RouteValueDictionary();
            }
            route.DataTokens[TargetActionMethodKey] = targetMethod;
        }

        /// <summary>
        /// Gets the target action method that will be invoked if this route is matched.
        /// </summary>
        internal static MethodInfo GetTargetActionMethod(this Route route)
        {
            if (route == null)
            {
                throw Error.ArgumentNull("route");
            }
            if (route.DataTokens == null)
            {
                return null;
            }
            return route.DataTokens[TargetActionMethodKey] as MethodInfo;
        }
    }
}