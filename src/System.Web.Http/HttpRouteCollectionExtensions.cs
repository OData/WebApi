// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    /// <summary>
    /// Extension methods for <see cref="HttpRouteCollection"/>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpRouteCollectionExtensions
    {
        /// <summary>
        /// Maps the specified route template.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="name">The name of the route to map.</param>
        /// <param name="routeTemplate">The route template for the route.</param>
        /// <returns>A reference to the mapped route.</returns>
        public static IHttpRoute MapHttpRoute(this HttpRouteCollection routes, string name, string routeTemplate)
        {
            return MapHttpRoute(routes, name, routeTemplate, defaults: null, constraints: null, handler: null);
        }

        /// <summary>
        /// Maps the specified route template and sets default constraints.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="name">The name of the route to map.</param>
        /// <param name="routeTemplate">The route template for the route.</param>
        /// <param name="defaults">An object that contains default route values.</param>
        /// <returns>A reference to the mapped route.</returns>
        public static IHttpRoute MapHttpRoute(this HttpRouteCollection routes, string name, string routeTemplate, object defaults)
        {
            return MapHttpRoute(routes, name, routeTemplate, defaults, constraints: null, handler: null);
        }

        /// <summary>
        /// Maps the specified route template and sets default route values and constraints.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="name">The name of the route to map.</param>
        /// <param name="routeTemplate">The route template for the route.</param>
        /// <param name="defaults">An object that contains default route values.</param>
        /// <param name="constraints">A set of expressions that specify values for <paramref name="routeTemplate"/>.</param>
        /// <returns>A reference to the mapped route.</returns>
        public static IHttpRoute MapHttpRoute(this HttpRouteCollection routes, string name, string routeTemplate, object defaults, object constraints)
        {
            return MapHttpRoute(routes, name, routeTemplate, defaults, constraints, handler: null);
        }

        /// <summary>
        /// Maps the specified route template and sets default route values, constraints, and end-point message handler.
        /// </summary>
        /// <param name="routes">A collection of routes for the application.</param>
        /// <param name="name">The name of the route to map.</param>
        /// <param name="routeTemplate">The route template for the route.</param>
        /// <param name="defaults">An object that contains default route values.</param>
        /// <param name="constraints">A set of expressions that specify values for <paramref name="routeTemplate"/>.</param>
        /// <param name="handler">The handler to which the request will be dispatched.</param>
        /// <returns>A reference to the mapped route.</returns>
        public static IHttpRoute MapHttpRoute(this HttpRouteCollection routes, string name, string routeTemplate, object defaults, object constraints, HttpMessageHandler handler)
        {
            if (routes == null)
            {
                throw Error.ArgumentNull("routes");
            }

            HttpRouteValueDictionary defaultsDictionary = new HttpRouteValueDictionary(defaults);
            HttpRouteValueDictionary constraintsDictionary = new HttpRouteValueDictionary(constraints);
            IHttpRoute route = routes.CreateRoute(routeTemplate, defaultsDictionary, constraintsDictionary, dataTokens: null, handler: handler);
            routes.Add(name, route);
            return route;
        }

        public static void MapHttpAttributeRoutes(this HttpRouteCollection routes)
        {
            // Configuration only used to retrieve the assembly resolver and controller type resolver
            using (HttpConfiguration config = new HttpConfiguration())
            {
                MapHttpAttributeRoutes(routes, new DefaultHttpControllerSelector(config), new ApiControllerActionSelector());
            }
        }

        public static void MapHttpAttributeRoutes(this HttpRouteCollection routes, IHttpControllerSelector controllerSelector, IHttpActionSelector actionSelector)
        {
            foreach (HttpControllerDescriptor controllerDescriptor in controllerSelector.GetControllerMapping().Values)
            {
                foreach (IGrouping<string, HttpActionDescriptor> actionGrouping in actionSelector.GetActionMapping(controllerDescriptor))
                {
                    MapHttpAttributeRoutes(routes, controllerDescriptor, actionGrouping);
                }
            }
        }

        private static void MapHttpAttributeRoutes(HttpRouteCollection routes, HttpControllerDescriptor controllerDescriptor, IGrouping<string, HttpActionDescriptor> actionGrouping)
        {
            int routeSuffix = 1;
            foreach (ReflectedHttpActionDescriptor actionDescriptor in actionGrouping.OfType<ReflectedHttpActionDescriptor>())
            {
                foreach (IHttpRouteProvider routeProvider in actionDescriptor.MethodInfo.GetCustomAttributes(false).OfType<IHttpRouteProvider>())
                {
                    if (routeProvider.RouteTemplate != null)
                    {
                        string controllerName = controllerDescriptor.ControllerName;
                        string actionName = actionDescriptor.ActionName;

                        // TODO: Improve default route name and make it configurable. AR was using a strategy pattern.
                        string routeName = routeProvider.RouteName ??
                            String.Format(CultureInfo.InvariantCulture, "{0}.{1}{2}", controllerName, actionName, routeSuffix);

                        IHttpRoute route = HttpRouteBuilder.BuildHttpRoute(routeProvider, controllerName, actionName);
                        routes.Add(routeName, route);

                        routeSuffix++;
                    }
                }
            }
        }
    }
}
