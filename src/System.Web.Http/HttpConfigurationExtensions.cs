// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationExtensions
    {
        /// <summary>
        /// Register that the given parameter type on an Action is to be bound using the model binder.
        /// </summary>
        /// <param name="configuration">configuration to be updated.</param>
        /// <param name="type">parameter type that binder is applied to</param>
        /// <param name="binder">a model binder</param>
        public static void BindParameter(this HttpConfiguration configuration, Type type, IModelBinder binder)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (binder == null)
            {
                throw Error.ArgumentNull("binder");
            }

            // Add a provider so that we can use this type recursively
            // Be sure to insert at position 0 to preempt any eager binders (eg, MutableObjectBinder) that 
            // may eagerly claim all types.
            configuration.Services.Insert(typeof(ModelBinderProvider), 0, new SimpleModelBinderProvider(type, binder));

            // Add the binder to the list of rules. 
            // This ensures that the parameter binding will actually use model binding instead of Formatters.            
            // Without this, the parameter binding system may see the parameter type is complex and choose
            // to use formatters instead, in which case it would ignore the registered model binders. 
            configuration.ParameterBindingRules.Insert(0, type, param => param.BindWithModelBinding(binder));
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        public static void MapHttpAttributeRoutes(this HttpConfiguration configuration)
        {
            configuration.MapHttpAttributeRoutes(new HttpRouteBuilder());
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="routeBuilder">The <see cref="HttpRouteBuilder"/> to use for generating attribute routes.</param>
        public static void MapHttpAttributeRoutes(this HttpConfiguration configuration, HttpRouteBuilder routeBuilder)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (routeBuilder == null)
            {
                throw Error.ArgumentNull("routeBuilder");
            }

            List<HttpRouteEntry> attributeRoutes = new List<HttpRouteEntry>();

            IHttpControllerSelector controllerSelector = configuration.Services.GetHttpControllerSelector();
            IHttpActionSelector actionSelector = configuration.Services.GetActionSelector();
            foreach (HttpControllerDescriptor controllerDescriptor in controllerSelector.GetControllerMapping().Values)
            {
                Collection<RoutePrefixAttribute> routePrefixes = controllerDescriptor.GetCustomAttributes<RoutePrefixAttribute>(inherit: false);

                foreach (IGrouping<string, HttpActionDescriptor> actionGrouping in actionSelector.GetActionMapping(controllerDescriptor))
                {
                    string controllerName = controllerDescriptor.ControllerName;
                    attributeRoutes.AddRange(CreateAttributeRoutes(routeBuilder, controllerName, routePrefixes, actionGrouping));
                }
            }

            attributeRoutes.Sort(CompareRoutes);

            foreach (HttpRouteEntry attributeRoute in attributeRoutes)
            {
                configuration.Routes.Add(attributeRoute.Name, attributeRoute.Route);
            }
        }

        private static List<HttpRouteEntry> CreateAttributeRoutes(HttpRouteBuilder routeBuilder, string controllerName,
            Collection<RoutePrefixAttribute> routePrefixes, IGrouping<string, HttpActionDescriptor> actionGrouping)
        {
            List<HttpRouteEntry> routes = new List<HttpRouteEntry>();
            string actionName = actionGrouping.Key;

            foreach (HttpActionDescriptor actionDescriptor in actionGrouping)
            {
                IEnumerable<IHttpRouteInfoProvider> routeInfoProviders = actionDescriptor.GetCustomAttributes<IHttpRouteInfoProvider>(inherit: false);

                // DefaultIfEmpty below is required to add routes when there is a route prefix but no
                // route provider or when there is a route provider with a template but no route prefix
                foreach (RoutePrefixAttribute routePrefix in routePrefixes.DefaultIfEmpty())
                {
                    foreach (IHttpRouteInfoProvider routeProvider in routeInfoProviders.DefaultIfEmpty())
                    {
                        string prefixTemplate = routePrefix == null ? null : routePrefix.Prefix;
                        string providerTemplate = routeProvider == null ? null : routeProvider.RouteTemplate;
                        if (prefixTemplate == null && providerTemplate == null)
                        {
                            continue;
                        }

                        string routeTemplate;
                        if (String.IsNullOrWhiteSpace(prefixTemplate))
                        {
                            routeTemplate = providerTemplate ?? String.Empty;
                        }
                        else if (String.IsNullOrWhiteSpace(providerTemplate))
                        {
                            routeTemplate = prefixTemplate;
                        }
                        else
                        {
                            // template and prefix both not null - combine them
                            routeTemplate = prefixTemplate.TrimEnd('/') + '/' + providerTemplate;
                        }

                        Collection<HttpMethod> httpMethods = actionDescriptor.SupportedHttpMethods;
                        IHttpRoute route = routeBuilder.BuildHttpRoute(routeTemplate, httpMethods, controllerName, actionName);
                        HttpRouteEntry entry = new HttpRouteEntry() { Route = route };
                        if (routeProvider != null)
                        {
                            entry.Name = routeProvider.RouteName;
                            entry.Order = routeProvider.RouteOrder;
                        }
                        if (routePrefix != null)
                        {
                            entry.PrefixOrder = routePrefix.Order;
                        }
                        routes.Add(entry);
                    }
                }
            }

            // Only use a route suffix to disambiguate between multiple routes without a specified route name
            HttpRouteEntry[] namelessRoutes = routes.Where(entry => entry.Name == null).ToArray();
            if (namelessRoutes.Length == 1)
            {
                namelessRoutes[0].Name = controllerName + "." + actionName;
            }
            else if (namelessRoutes.Length > 1)
            {
                int routeSuffix = 1;
                foreach (HttpRouteEntry namelessRoute in namelessRoutes)
                {
                    namelessRoute.Name = controllerName + "." + actionName + routeSuffix;
                    routeSuffix++;
                }
            }

            return routes;
        }

        private static int CompareRoutes(HttpRouteEntry entryA, HttpRouteEntry entryB)
        {
            Contract.Assert(entryA != null);
            Contract.Assert(entryB != null);

            // Order by prefixes first
            if (entryA.PrefixOrder > entryB.PrefixOrder)
            {
                return 1;
            }
            else if (entryA.PrefixOrder < entryB.PrefixOrder)
            {
                return -1;
            }

            // Then order by the attribute order
            if (entryA.Order > entryB.Order)
            {
                return 1;
            }
            else if (entryA.Order < entryB.Order)
            {
                return -1;
            }

            return 0;
        }

        private class HttpRouteEntry
        {
            public IHttpRoute Route { get; set; }
            public string Name { get; set; }
            public int PrefixOrder { get; set; }
            public int Order { get; set; }
        }
    }
}
