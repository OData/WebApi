// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    internal static class DirectRouteExtensions
    {
        /// <summary>
        /// Gets the precedence or this route.
        /// </summary>
        public static decimal GetPrecedence(this RouteData routeData)
        {
            return GetRouteDataTokenValue<decimal>(routeData, RouteDataTokenKeys.DirectRoutePrecedence);
        }

        /// <summary>
        /// Gets the precedence or this route.
        /// </summary>
        public static decimal GetPrecedence(this Route route)
        {
            return GetRouteDataTokenValue<decimal>(route, RouteDataTokenKeys.DirectRoutePrecedence);
        }

        /// <summary>
        /// Sets the precedence of this Route.
        /// </summary>
        public static void SetPrecedence(this Route route, decimal precedence)
        {
            SetRouteDataTokenValue(route, RouteDataTokenKeys.DirectRoutePrecedence, precedence);
        }

        /// <summary>
        /// Gets the order or this route.
        /// </summary>
        public static int GetOrder(this RouteData routeData)
        {
            return GetRouteDataTokenValue<int>(routeData, RouteDataTokenKeys.DirectRouteOrder);
        }

        /// <summary>
        /// Gets the order or this route.
        /// </summary>
        public static int GetOrder(this Route route)
        {
            return GetRouteDataTokenValue<int>(route, RouteDataTokenKeys.DirectRouteOrder);
        }

        /// <summary>
        /// Sets the order of this Route.
        /// </summary>
        public static void SetOrder(this Route route, int order)
        {
            SetRouteDataTokenValue(route, RouteDataTokenKeys.DirectRouteOrder, order);
        }

        /// <summary>
        /// Gets the ControllerDescriptor that matches this Route.
        /// </summary>
        public static ControllerDescriptor GetTargetControllerDescriptor(this Route route)
        {
            return GetRouteDataTokenValue<ControllerDescriptor>(route, RouteDataTokenKeys.DirectRouteController);
        }

        /// <summary>
        /// Gets the ControllerDescriptor that matches this Route.
        /// </summary>
        public static ControllerDescriptor GetTargetControllerDescriptor(this RouteData routeData)
        {
            return GetRouteDataTokenValue<ControllerDescriptor>(routeData, RouteDataTokenKeys.DirectRouteController);
        }

        public static Type GetTargetControllerType(this Route route)
        {
            ControllerDescriptor controllerDescriptor = GetRouteDataTokenValue<ControllerDescriptor>(route, RouteDataTokenKeys.DirectRouteController);
            return controllerDescriptor == null ? null : controllerDescriptor.ControllerType;
        }

        public static Type GetTargetControllerType(this RouteData routeData)
        {
            ControllerDescriptor controllerDescriptor = GetRouteDataTokenValue<ControllerDescriptor>(routeData, RouteDataTokenKeys.DirectRouteController);
            return controllerDescriptor == null ? null : controllerDescriptor.ControllerType;
        }

        /// <summary>
        /// Sets the ControllerDescriptor that matches this Route.
        /// </summary>
        public static void SetTargetControllerDescriptor(this Route route, ControllerDescriptor controllerDescriptor)
        {
            if (controllerDescriptor == null)
            {
                throw Error.ArgumentNull("controllerDescriptor");
            }

            SetRouteDataTokenValue(route, RouteDataTokenKeys.DirectRouteController, controllerDescriptor);
        }

        /// <summary>
        /// Gets the target actions that can be matched if this route is matched.
        /// </summary>
        public static IEnumerable<ActionDescriptor> GetTargetActionDescriptors(this RouteData routeData)
        {
            return GetRouteDataTokenValue<IEnumerable<ActionDescriptor>>(routeData, RouteDataTokenKeys.DirectRouteActions)
                ?? Enumerable.Empty<ActionDescriptor>();
        }

        /// <summary>
        /// Gets the target actions that can be matched if this route is matched.
        /// </summary>
        public static IEnumerable<ActionDescriptor> GetTargetActionDescriptors(this Route route)
        {
            return GetRouteDataTokenValue<IEnumerable<ActionDescriptor>>(route, RouteDataTokenKeys.DirectRouteActions)
                ?? Enumerable.Empty<ActionDescriptor>();
        }

        /// <summary>
        /// Sets the target actions that can be matched if this route is matched.
        /// </summary>
        public static void SetTargetActionDescriptors(this Route route, IEnumerable<ActionDescriptor> actionDescriptors)
        {
            if (actionDescriptors == null || !actionDescriptors.Any())
            {
                throw Error.ParameterCannotBeNullOrEmpty("actionDescriptors");
            }

            SetRouteDataTokenValue(route, RouteDataTokenKeys.DirectRouteActions, actionDescriptors);
        }

        public static bool HasDirectRouteMatch(this RouteData routeData)
        {
            if (routeData == null)
            {
                throw Error.ArgumentNull("routeData");
            }

            return routeData.Values.ContainsKey(RouteDataTokenKeys.DirectRouteMatches);
        }

        public static IEnumerable<RouteData> GetDirectRouteMatches(this RouteData routeData)
        {
            return GetRouteDataValue<IEnumerable<RouteData>>(routeData, RouteDataTokenKeys.DirectRouteMatches) ?? Enumerable.Empty<RouteData>();
        }

        public static void SetDirectRouteMatches(this RouteData routeData, IEnumerable<RouteData> matches)
        {
            if (matches == null || !matches.Any())
            {
                throw Error.ParameterCannotBeNullOrEmpty("matches");
            }

            SetRouteDataValue(routeData, RouteDataTokenKeys.DirectRouteMatches, matches);
        }

        public static bool IsDirectRoute(this RouteBase routeBase)
        {
            Route route = routeBase as Route;
            if (route == null)
            {
                return false;
            }
            else
            {
                // All direct routes need to have a controller associated.
                return route.GetTargetControllerDescriptor() != null;
            }
        }

        private static T GetRouteDataTokenValue<T>(this Route route, string key)
        {
            if (route == null)
            {
                throw Error.ArgumentNull("route");
            }

            if (key == null)
            {
                throw Error.ArgumentNull("key");
            }

            T value;
            if (route.DataTokens != null && route.DataTokens.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return default(T);
            }
        }

        private static T GetRouteDataTokenValue<T>(this RouteData routeData, string key)
        {
            if (routeData == null)
            {
                throw Error.ArgumentNull("route");
            }

            if (key == null)
            {
                throw Error.ArgumentNull("key");
            }

            return GetRouteDataTokenValue<T>(routeData.Route as Route, key);
        }

        private static void SetRouteDataTokenValue<T>(this Route route, string key, T value)
        {
            if (route == null)
            {
                throw Error.ArgumentNull("route");
            }

            if (key == null)
            {
                throw Error.ArgumentNull("key");
            }

            if (route.DataTokens == null)
            {
                route.DataTokens = new RouteValueDictionary();
            }

            route.DataTokens[key] = value;
        }

        private static T GetRouteDataValue<T>(this RouteData routeData, string key)
        {
            if (routeData == null)
            {
                throw Error.ArgumentNull("routeData");
            }

            if (key == null)
            {
                throw Error.ArgumentNull("key");
            }

            T value;
            if (routeData.Values.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return default(T);
            }
        }

        private static void SetRouteDataValue<T>(this RouteData routeData, string key, T value)
        {
            if (routeData == null)
            {
                throw Error.ArgumentNull("routeData");
            }

            if (key == null)
            {
                throw Error.ArgumentNull("key");
            }

            routeData.Values[key] = value;
        }
    }
}