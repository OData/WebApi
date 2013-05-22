// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using System.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Properties;
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
            IDictionary<string, HttpControllerDescriptor> controllerMapping = controllerSelector.GetControllerMapping();
            if (controllerMapping != null)
            {
                foreach (HttpControllerDescriptor controllerDescriptor in controllerMapping.Values)
                {
                    Collection<RoutePrefixAttribute> routePrefixes = controllerDescriptor.GetCustomAttributes<RoutePrefixAttribute>(inherit: false);
                    IHttpActionSelector actionSelector = controllerDescriptor.Configuration.Services.GetActionSelector();
                    ILookup<string, HttpActionDescriptor> actionMapping = actionSelector.GetActionMapping(controllerDescriptor);
                    if (actionMapping != null)
                    {
                        foreach (IGrouping<string, HttpActionDescriptor> actionGrouping in actionMapping)
                        {
                            string controllerName = controllerDescriptor.ControllerName;
                            attributeRoutes.AddRange(CreateAttributeRoutes(routeBuilder, controllerName, routePrefixes, actionGrouping));
                        }
                    }
                }

                attributeRoutes.Sort();

                foreach (HttpRouteEntry attributeRoute in attributeRoutes)
                {
                    configuration.Routes.Add(attributeRoute.Name, attributeRoute.Route);
                }
            }
        }

        /// <summary>Enables suppression of the host's principal.</summary>
        /// <param name="configuration">The server configuration.</param>
        /// <remarks>
        /// When the host's principal is suppressed, the current principal is set to anonymous upon entering the
        /// <see cref="HttpServer"/>'s first message handler. As a result, any authentication performed by the host is
        /// ignored. The remaining pipeline within the <see cref="HttpServer"/>, including
        /// <see cref="IAuthenticationFilter"/>s, is then the exclusive authority for authentication.
        /// </remarks>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Message handler should be disposed with parent configuration.")]
        public static void SuppressHostPrincipal(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            Contract.Assert(configuration.MessageHandlers != null);
            configuration.MessageHandlers.Insert(0, new SuppressHostPrincipalMessageHandler(configuration));
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

                        ValidateTemplates(prefixTemplate, providerTemplate, actionDescriptor);

                        string routeTemplate;
                        if (String.IsNullOrEmpty(prefixTemplate))
                        {
                            routeTemplate = providerTemplate ?? String.Empty;
                        }
                        else if (String.IsNullOrEmpty(providerTemplate))
                        {
                            routeTemplate = prefixTemplate;
                        }
                        else
                        {
                            // template and prefix both not null - combine them
                            routeTemplate = prefixTemplate + '/' + providerTemplate;
                        }

                        Collection<HttpMethod> httpMethods = actionDescriptor.SupportedHttpMethods;
                        IHttpRoute route = routeBuilder.BuildHttpRoute(routeTemplate, httpMethods, controllerName, actionName);
                        HttpRouteEntry entry = new HttpRouteEntry() { Route = route, RouteTemplate = routeTemplate };
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

            SetDefaultRouteNames(routes, controllerName, actionName);

            return routes;
        }

        private static void ValidateTemplates(string prefixTemplate, string providerTemplate, HttpActionDescriptor actionDescriptor)
        {
            if (prefixTemplate != null && prefixTemplate.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                throw Error.InvalidOperation(SRResources.AttributeRoutes_InvalidPrefix, prefixTemplate, actionDescriptor.ControllerDescriptor.ControllerName);
            }

            if (providerTemplate != null && providerTemplate.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                throw Error.InvalidOperation(SRResources.AttributeRoutes_InvalidTemplate, providerTemplate, actionDescriptor.ActionName);
            }
        }

        private static void SetDefaultRouteNames(List<HttpRouteEntry> routes, string controllerName, string actionName)
        {
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
        }
    }
}
