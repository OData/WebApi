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
            IDictionary<string, HttpControllerDescriptor> controllerMap = controllerSelector.GetControllerMapping();
            if (controllerMap != null)
            {
                foreach (HttpControllerDescriptor controllerDescriptor in controllerMap.Values)
                {
                    IEnumerable<HttpRouteEntry> controllerRoutes = CreateRouteEntries(controllerDescriptor);

                    foreach (HttpRouteEntry route in controllerRoutes)
                    {
                        route.Route = routeBuilder.BuildHttpRoute(route.RouteTemplate, route.HttpMethods, route.Actions);
                    }

                    SetDefaultRouteNames(controllerRoutes, controllerDescriptor.ControllerName);
                    attributeRoutes.AddRange(controllerRoutes);
                }

                attributeRoutes.Sort();

                foreach (HttpRouteEntry attributeRoute in attributeRoutes)
                {
                    IHttpRoute route = attributeRoute.Route;
                    if (route != null)
                    {
                        configuration.Routes.Add(attributeRoute.Name, attributeRoute.Route);
                    }
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

        private static IEnumerable<HttpRouteEntry> CreateRouteEntries(HttpControllerDescriptor controllerDescriptor)
        {
            IHttpActionSelector actionSelector = controllerDescriptor.Configuration.Services.GetActionSelector();
            ILookup<string, HttpActionDescriptor> actionMap = actionSelector.GetActionMapping(controllerDescriptor);
            if (actionMap == null)
            {
                return Enumerable.Empty<HttpRouteEntry>();
            }

            List<HttpRouteEntry> routes = new List<HttpRouteEntry>();
            string routePrefix = GetRoutePrefix(controllerDescriptor);

            foreach (IGrouping<string, HttpActionDescriptor> actionGrouping in actionMap)
            {
                string actionName = actionGrouping.Key;

                foreach (ReflectedHttpActionDescriptor actionDescriptor in actionGrouping.OfType<ReflectedHttpActionDescriptor>())
                {
                    foreach (IHttpRouteInfoProvider routeProvider in actionDescriptor.GetCustomAttributes<IHttpRouteInfoProvider>(inherit: false))
                    {
                        string providerTemplate = routeProvider.RouteTemplate;
                        if (providerTemplate == null)
                        {
                            continue;
                        }

                        if (providerTemplate.StartsWith("/", StringComparison.Ordinal))
                        {
                            throw Error.InvalidOperation(SRResources.AttributeRoutes_InvalidTemplate, providerTemplate, actionName);
                        }

                        string routeTemplate = BuildRouteTemplate(routePrefix, providerTemplate);

                        // Try to find an entry with the same route template and the same HTTP verbs
                        HttpRouteEntry existingEntry = null;
                        foreach (HttpRouteEntry entry in routes)
                        {
                            if (String.Equals(routeTemplate, entry.RouteTemplate, StringComparison.OrdinalIgnoreCase) &&
                                    AreEqual(routeProvider.HttpMethods, entry.HttpMethods))
                            {
                                existingEntry = entry;
                                break;
                            }
                        }

                        if (existingEntry == null)
                        {
                            HttpRouteEntry entry = new HttpRouteEntry()
                            {
                                RouteTemplate = routeTemplate,
                                Actions = new HashSet<ReflectedHttpActionDescriptor>() { actionDescriptor }
                            };

                            entry.HttpMethods = routeProvider.HttpMethods;
                            entry.Name = routeProvider.RouteName;
                            entry.Order = routeProvider.RouteOrder;
                            routes.Add(entry);
                        }
                        else
                        {
                            existingEntry.Actions.Add(actionDescriptor);

                            // Take the minimum of the two orders as the order
                            int order = routeProvider.RouteOrder;
                            if (order < existingEntry.Order)
                            {
                                existingEntry.Order = order;
                            }

                            // Use the provider route name if the route hasn't already been named
                            if (existingEntry.Name == null)
                            {
                                existingEntry.Name = routeProvider.RouteName;
                            }
                        }
                    }
                }
            }

            return routes;
        }

        private static string GetRoutePrefix(HttpControllerDescriptor controllerDescriptor)
        {
            Collection<RoutePrefixAttribute> routePrefixAttributes = controllerDescriptor.GetCustomAttributes<RoutePrefixAttribute>(inherit: false);
            if (routePrefixAttributes.Count > 0)
            {
                string routePrefix = routePrefixAttributes[0].Prefix;
                if (routePrefix != null)
                {
                    if (routePrefix.EndsWith("/", StringComparison.Ordinal))
                    {
                        throw Error.InvalidOperation(SRResources.AttributeRoutes_InvalidPrefix, routePrefix, controllerDescriptor.ControllerName);
                    }

                    return routePrefix;
                }
            }
            return null;
        }

        private static string BuildRouteTemplate(string routePrefix, string routeTemplate)
        {
            Contract.Assert(routeTemplate != null);

            // If the provider's template starts with '~/', ignore the route prefix
            if (routeTemplate.StartsWith("~/", StringComparison.Ordinal))
            {
                return routeTemplate.Substring(2);
            }

            if (String.IsNullOrEmpty(routePrefix))
            {
                return routeTemplate;
            }
            else if (routeTemplate.Length == 0)
            {
                return routePrefix;
            }
            else
            {
                // template and prefix both not null - combine them
                return routePrefix + '/' + routeTemplate;
            }
        }

        private static bool AreEqual(IEnumerable<HttpMethod> routeProviderMethods, IEnumerable<HttpMethod> routeEntryMethods)
        {
            if (routeProviderMethods == null || routeEntryMethods == null)
            {
                return routeProviderMethods == routeEntryMethods;
            }

            // compare the collections by set equality
            return new HashSet<HttpMethod>(routeProviderMethods).SetEquals(new HashSet<HttpMethod>(routeEntryMethods));
        }

        private static void SetDefaultRouteNames(IEnumerable<HttpRouteEntry> routes, string controllerName)
        {
            // Only use a route suffix to disambiguate between routes without a specified route name
            int routeSuffix = 1;
            foreach (HttpRouteEntry namelessRoute in routes.Where(entry => entry.Name == null))
            {
                namelessRoute.Name = controllerName + routeSuffix;
                routeSuffix++;
            }
        }
    }
}
