// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
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
            List<RouteEntry> routeEntries = new AttributeRoutingMapper(new RouteBuilder(constraintResolver)).MapMvcAttributeRoutes(controllerTypes);

            foreach (var routeEntry in routeEntries)
            {
                string routeName = routeEntry.Name;
                if (routeName == null)
                {
                    routes.Add(routeEntry.Route);
                }
                else
                {
                    routes.Add(routeName, routeEntry.Route);
                }
            }
        }
    }
}
