// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
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
        // Attribute routing will inject a single top-level route into the route table. 
        private const string AttributeRouteName = "MS_attributerouteWebApi";

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
            MapHttpAttributeRoutes(configuration, new DefaultInlineConstraintResolver());
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="constraintResolver">The <see cref="IInlineConstraintResolver"/> to use for resolving inline constraints.</param>
        public static void MapHttpAttributeRoutes(this HttpConfiguration configuration, IInlineConstraintResolver constraintResolver)
        {
            HttpRouteBuilder routeBuilder = new HttpRouteBuilder(constraintResolver);
            var attrRoute = new RouteCollectionRoute();
            configuration.Routes.Add(AttributeRouteName, attrRoute);

            Action<HttpConfiguration> previousInitializer = configuration.Initializer;
            configuration.Initializer = config =>
                {
                    // Chain to the previous initializer hook. Do this before we access the config since
                    // initialization may make last minute changes to the configuration.
                    previousInitializer(config);

                    // Add a single placeholder route that handles all of attribute routing.
                    // Add an initialize hook that initializes these routes after the config has been initialized.
                    Func<HttpSubRouteCollection> initializer = () => MapHttpAttributeRoutesInternal(configuration, routeBuilder);

                    // This won't change config. It wants to pick up the finalized config.
                    HttpSubRouteCollection subRoutes = attrRoute.EnsureInitialized(initializer);
                    if (subRoutes != null)
                    {
                        AddGenerationHooksForSubRoutes(config.Routes, subRoutes, routeBuilder);
                    }
                };
        }

        // Add generation hooks for the Attribute-routing subroutes. 
        // This lets us generate urls for routes supplied by attr-based routing.
        private static void AddGenerationHooksForSubRoutes(HttpRouteCollection destRoutes, HttpSubRouteCollection sourceRoutes, HttpRouteBuilder routeBuilder)
        {
            foreach (KeyValuePair<string, IHttpRoute> kv in sourceRoutes.NamedRoutes)
            {
                string name = kv.Key;
                IHttpRoute route = kv.Value;
                var stubRoute = routeBuilder.BuildGenerationRoute(route);
                destRoutes.Add(name, stubRoute);
            }
        }

        // Test Hook for inspecting the route table generated by MapHttpAttributeRoutes. 
        // MapHttpAttributeRoutes doesn't return the route collection because it's an implementation detail
        // that attr routes even generate a meaningful route collection. 
        // Public APIs can get similar functionality by querying the IHttpRoute for IEnumerable<IHttpRoute>.
        internal static HttpSubRouteCollection GetAttributeRoutes(this HttpConfiguration configuration)
        {
            configuration.EnsureInitialized();

            HttpRouteCollection routes = configuration.Routes;
            foreach (IHttpRoute route in routes)
            {
                var attrRoute = route as RouteCollectionRoute;
                if (attrRoute != null)
                {
                    return attrRoute.SubRoutes;
                }
            }
            return null;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "HttpRouteCollection doesn't need to be disposed")]
        private static HttpSubRouteCollection MapHttpAttributeRoutesInternal(this HttpConfiguration configuration, HttpRouteBuilder routeBuilder)
        {
            HttpSubRouteCollection subRoutes = new HttpSubRouteCollection();

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
                        route.Route = routeBuilder.BuildParsingRoute(route.Template, route.Order, route.Actions);
                    }

                    attributeRoutes.AddRange(controllerRoutes);
                }

                foreach (HttpRouteEntry attributeRoute in attributeRoutes)
                {
                    IHttpRoute route = attributeRoute.Route;
                    if (route != null)
                    {
                        subRoutes.Add(attributeRoute.Name, attributeRoute.Route);
                    }
                }
            }

            return subRoutes;
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
            configuration.MessageHandlers.Insert(0, new SuppressHostPrincipalMessageHandler());
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
            List<ReflectedHttpActionDescriptor> actionsWithoutRoutes = new List<ReflectedHttpActionDescriptor>();

            foreach (IGrouping<string, HttpActionDescriptor> actionGrouping in actionMap)
            {
                string actionName = actionGrouping.Key;

                foreach (ReflectedHttpActionDescriptor actionDescriptor in actionGrouping.OfType<ReflectedHttpActionDescriptor>())
                {
                    Collection<IHttpRouteInfoProvider> routeProviders = actionDescriptor.GetCustomAttributes<IHttpRouteInfoProvider>(inherit: false);

                    // Ignore the Route attributes from inherited actions.
                    if (actionDescriptor.MethodInfo != null && 
                        actionDescriptor.MethodInfo.DeclaringType != controllerDescriptor.ControllerType)
                    {
                        routeProviders = null;
                    }

                    if (routeProviders != null && routeProviders.Count > 0)
                    {
                        AddRouteEntries(routes, actionName, routePrefix, routeProviders,
                            new ReflectedHttpActionDescriptor[] { actionDescriptor });
                    }
                    else
                    {
                        // IF there are no routes on the specific action, attach it to the controller routes (if any).
                        actionsWithoutRoutes.Add(actionDescriptor);
                    }
                }
            }

            Collection<IHttpRouteInfoProvider> controllerRouteProviders =
                controllerDescriptor.GetCustomAttributes<IHttpRouteInfoProvider>(inherit: false);

            // If they exist and have not been overridden, create routes for controller-level route providers.
            if (controllerRouteProviders != null && controllerRouteProviders.Count > 0
                && actionsWithoutRoutes.Count > 0)
            {
                AddRouteEntries(routes, actionsWithoutRoutes[0].ActionName, routePrefix, controllerRouteProviders,
                    actionsWithoutRoutes);
            }

            return routes;
        }

        private static void AddRouteEntries(List<HttpRouteEntry> routes, string actionName, string routePrefix,
            Collection<IHttpRouteInfoProvider> routeProviders,
            IEnumerable<ReflectedHttpActionDescriptor> actionDescriptors)
        {
            foreach (IHttpRouteInfoProvider routeProvider in routeProviders)
            {
                HttpRouteEntry entry = CreateRouteEntry(actionName, routePrefix, routeProvider, actionDescriptors);
                bool mergedWithExistingEntry = false;

                if (String.IsNullOrEmpty(entry.Name))
                {
                    // Merge unnamed entries with the exact same template and order.
                    HttpRouteEntry existingMatch = routes.SingleOrDefault(
                        e => String.IsNullOrEmpty(e.Name)
                            && String.Equals(e.Template, entry.Template, StringComparison.Ordinal)
                            && e.Order == entry.Order);

                    if (existingMatch != null)
                    {
                        mergedWithExistingEntry = true;

                        foreach (ReflectedHttpActionDescriptor descriptor in actionDescriptors)
                        {
                            existingMatch.Actions.Add(descriptor);
                        }
                    }
                }

                if (!mergedWithExistingEntry)
                {
                    routes.Add(entry);
                }
            }
        }

        private static HttpRouteEntry CreateRouteEntry(string actionName, string routePrefix,
            IHttpRouteInfoProvider routeProvider, IEnumerable<ReflectedHttpActionDescriptor> actionDescriptors)
        {
            string providerTemplate = routeProvider.Template;
            if (providerTemplate == null)
            {
                return null;
            }

            if (providerTemplate.StartsWith("/", StringComparison.Ordinal))
            {
                throw Error.InvalidOperation(SRResources.AttributeRoutes_InvalidTemplate, providerTemplate,
                    actionName);
            }

            string routeTemplate = BuildRouteTemplate(routePrefix, providerTemplate);

            return new HttpRouteEntry()
            {
                Name = routeProvider.Name,
                Template = routeTemplate,
                Order = routeProvider.Order,
                Actions = new HashSet<ReflectedHttpActionDescriptor>(actionDescriptors)
            };
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
    }
}
