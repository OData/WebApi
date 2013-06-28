// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    public static class RouteCollectionAttributeRoutingExtensions
    {
        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="routes"></param>
        public static void MapMvcAttributeRoutes(this RouteCollection routes)
        {
            DefaultControllerFactory typesLocator =
                DependencyResolver.Current.GetService<IControllerFactory>() as DefaultControllerFactory
                ?? ControllerBuilder.Current.GetControllerFactory() as DefaultControllerFactory
                ?? new DefaultControllerFactory();

            IReadOnlyList<Type> controllerTypes = typesLocator.GetControllerTypes();

            List<RouteEntry> routeEntries = new AttributeRoutingMapper(new RouteBuilder()).MapMvcAttributeRoutes(controllerTypes);

            foreach (var routeEntry in routeEntries)
            {
                routes.Add(routeEntry.Name, routeEntry.Route);
            }
        }
    }
}
