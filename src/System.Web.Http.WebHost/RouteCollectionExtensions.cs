// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Web.Http.WebHost.Routing;
using System.Web.Routing;

namespace System.Web.Http
{
    /// <summary>
    /// Extension methods for <see cref="RouteCollection"/>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class RouteCollectionExtensions
    {
        /// <summary>
        /// Maps the specified route template.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="name">The name of the route to map.</param>
        /// <param name="routeTemplate">The route template for the route.</param>
        /// <returns>A reference to the mapped route.</returns>
        public static Route MapHttpRoute(this RouteCollection routes, string name, string routeTemplate)
        {
            return MapHttpRoute(routes, name, routeTemplate, defaults: null, constraints: null, handler: null);
        }

        /// <summary>
        /// Maps the specified route template and sets default constraints, and namespaces.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="name">The name of the route to map.</param>
        /// <param name="routeTemplate">The route template for the route.</param>
        /// <param name="defaults">An object that contains default route values.</param>
        /// <returns>A reference to the mapped route.</returns>
        public static Route MapHttpRoute(this RouteCollection routes, string name, string routeTemplate, object defaults)
        {
            return MapHttpRoute(routes, name, routeTemplate, defaults, constraints: null, handler: null);
        }

        /// <summary>
        /// Maps the specified route template and sets default route values, constraints, and namespaces.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="name">The name of the route to map.</param>
        /// <param name="routeTemplate">The route template for the route.</param>
        /// <param name="defaults">An object that contains default route values.</param>
        /// <param name="constraints">A set of expressions that specify values for <paramref name="routeTemplate"/>.</param>
        /// <returns>A reference to the mapped route.</returns>
        public static Route MapHttpRoute(this RouteCollection routes, string name, string routeTemplate, object defaults, object constraints)
        {
            return MapHttpRoute(routes, name, routeTemplate, defaults, constraints, handler: null);
        }

        /// <summary>
        /// Maps the specified route template and sets default route values, constraints, namespaces, and end-point message handler.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="name">The name of the route to map.</param>
        /// <param name="routeTemplate">The route template for the route.</param>
        /// <param name="defaults">An object that contains default route values.</param>
        /// <param name="constraints">A set of expressions that specify values for <paramref name="routeTemplate"/>.</param>
        /// <param name="handler">The handler to which the request will be dispatched.</param>
        /// <returns>A reference to the mapped route.</returns>
        public static Route MapHttpRoute(this RouteCollection routes, string name, string routeTemplate, object defaults, object constraints, HttpMessageHandler handler)
        {
            if (routes == null)
            {
                throw Error.ArgumentNull("routes");
            }

            RouteValueDictionary defaultsDictionary = CreateRouteValueDictionary(defaults);
            RouteValueDictionary constraintsDictionary = CreateRouteValueDictionary(constraints);
            HostedHttpRoute httpRoute = (HostedHttpRoute)GlobalConfiguration.Configuration.Routes.CreateRoute(routeTemplate, defaultsDictionary, constraintsDictionary, dataTokens: null, handler: handler);
            Route route = httpRoute.OriginalRoute;
            routes.Add(name, route);
            return route;
        }

        private static RouteValueDictionary CreateRouteValueDictionary(object values)
        {
            if (values == null)
            {
                return new RouteValueDictionary();
            }

            var dictionary = values as IDictionary<string, object>;
            if (dictionary != null)
            {
                return new RouteValueDictionary(dictionary);
            }

            return new RouteValueDictionary(values);
        }
    }
}
