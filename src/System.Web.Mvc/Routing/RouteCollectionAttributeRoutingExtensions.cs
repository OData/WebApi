// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc.Routing;
using System.Web.Routing;

namespace System.Web.Mvc
{
    public static class RouteCollectionAttributeRoutingExtensions
    {
        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="routes"></param>
        public static void MapMvcAttributeRoutes(this RouteCollection routes)
        {
            MapMvcAttributeRoutes(routes, new DefaultInlineConstraintResolver());
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="constraintResolver">The <see cref="IInlineConstraintResolver"/> to use for resolving inline constraints in route templates.</param>
        public static void MapMvcAttributeRoutes(this RouteCollection routes, IInlineConstraintResolver constraintResolver)
        {
            DefaultControllerFactory typesLocator =
                DependencyResolver.Current.GetService<IControllerFactory>() as DefaultControllerFactory
                ?? ControllerBuilder.Current.GetControllerFactory() as DefaultControllerFactory
                ?? new DefaultControllerFactory();

            IReadOnlyList<Type> controllerTypes = typesLocator.GetControllerTypes();

            MapMvcAttributeRoutes(routes, controllerTypes, constraintResolver);
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="controllerTypes">The controller types to scan.</param>
        internal static void MapMvcAttributeRoutes(this RouteCollection routes, IEnumerable<Type> controllerTypes)
        {
            MapMvcAttributeRoutes(routes, controllerTypes, new DefaultInlineConstraintResolver());
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="controllerTypes">The controller types to scan.</param>
        /// <param name="constraintResolver">The <see cref="IInlineConstraintResolver"/> to use for resolving inline constraints in route templates.</param>
        internal static void MapMvcAttributeRoutes(this RouteCollection routes, IEnumerable<Type> controllerTypes, IInlineConstraintResolver constraintResolver)
        {
            List<RouteEntry> routeEntries = new AttributeRoutingMapper(new RouteBuilder2(constraintResolver)).MapMvcAttributeRoutes(controllerTypes);

            // This sort is here to enforce a static ordering for link generation using these routes. 
            // We don't apply dynamic criteria like ActionSelectors on link generation, but we can use the static ones.
            RouteEntry[] sorted = routeEntries.OrderBy(r => r.Route.GetOrder()).ThenBy(r => r.Route.GetPrecedence()).ToArray();

            RouteCollectionRoute aggregateRoute = new RouteCollectionRoute();
            if (sorted.Length > 0)
            {
                routes.Add(aggregateRoute);
            }
            
            foreach (var routeEntry in sorted)
            {
                aggregateRoute.SubRoutes.Add(routeEntry.Name, routeEntry.Route);

                if (routeEntry.Name == null)
                {
                    routes.Add(new GenerationRoute(routeEntry.Route));
                }
                else
                {
                    routes.Add(routeEntry.Name, new GenerationRoute(routeEntry.Route));
                }
            }
        }
    }
}
