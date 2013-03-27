// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

            IHttpControllerSelector controllerSelector = configuration.Services.GetHttpControllerSelector();
            IHttpActionSelector actionSelector = configuration.Services.GetActionSelector();
            foreach (HttpControllerDescriptor controllerDescriptor in controllerSelector.GetControllerMapping().Values)
            {
                Collection<RoutePrefixAttribute> prefixAttributes = controllerDescriptor.GetCustomAttributes<RoutePrefixAttribute>(inherit: false);
                string[] routePrefixes = prefixAttributes.Select(prefixAttribute => prefixAttribute.Prefix).ToArray();

                foreach (IGrouping<string, HttpActionDescriptor> actionGrouping in actionSelector.GetActionMapping(controllerDescriptor))
                {
                    HttpRouteCollection routes = configuration.Routes;
                    string controllerName = controllerDescriptor.ControllerName;
                    MapHttpAttributeRoutes(routes, routeBuilder, controllerName, routePrefixes, actionGrouping);
                }
            }
        }

        private static void MapHttpAttributeRoutes(HttpRouteCollection routes, HttpRouteBuilder routeBuilder, string controllerName,
            string[] routePrefixes, IGrouping<string, HttpActionDescriptor> actionGrouping)
        {
            List<IHttpRoute> namelessAttributeRoutes = new List<IHttpRoute>();
            string actionName = actionGrouping.Key;

            foreach (HttpActionDescriptor actionDescriptor in actionGrouping)
            {
                IEnumerable<IHttpRouteInfoProvider> routeInfoProviders = actionDescriptor.GetCustomAttributes<IHttpRouteInfoProvider>(inherit: false);

                // DefaultIfEmpty below is required to add routes when there is a route prefix but no
                // route provider or when there is a route provider with a template but no route prefix
                foreach (IHttpRouteInfoProvider routeProvider in routeInfoProviders.DefaultIfEmpty())
                {
                    foreach (string routePrefix in routePrefixes.DefaultIfEmpty())
                    {
                        string providerTemplate = routeProvider == null ? null : routeProvider.RouteTemplate;
                        if (routePrefix == null && providerTemplate == null)
                        {
                            continue;
                        }

                        string routeTemplate;
                        if (String.IsNullOrWhiteSpace(routePrefix))
                        {
                            routeTemplate = providerTemplate ?? String.Empty;
                        }
                        else if (String.IsNullOrWhiteSpace(providerTemplate))
                        {
                            routeTemplate = routePrefix;
                        }
                        else
                        {
                            // template and prefix both not null - combine them
                            routeTemplate = routePrefix.TrimEnd('/') + '/' + providerTemplate;
                        }

                        Collection<HttpMethod> httpMethods = actionDescriptor.SupportedHttpMethods;
                        IHttpRoute route = routeBuilder.BuildHttpRoute(routeTemplate, httpMethods, controllerName, actionName);
                        if (routeProvider == null || routeProvider.RouteName == null)
                        {
                            namelessAttributeRoutes.Add(route);
                        }
                        else
                        {
                            routes.Add(routeProvider.RouteName, route);
                        }
                    }
                }
            }

            // Only use a route suffix to disambiguate between multiple routes without a specified route name
            if (namelessAttributeRoutes.Count == 1)
            {
                routes.Add(controllerName + "." + actionName, namelessAttributeRoutes[0]);
            }
            else if (namelessAttributeRoutes.Count > 1)
            {
                int routeSuffix = 1;
                foreach (IHttpRoute namelessAttributeRoute in namelessAttributeRoutes)
                {
                    routes.Add(controllerName + "." + actionName + routeSuffix, namelessAttributeRoute);
                    routeSuffix++;
                }
            }
        }
    }
}
