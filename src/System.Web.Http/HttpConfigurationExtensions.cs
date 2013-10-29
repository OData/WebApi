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
            if (constraintResolver == null)
            {
                throw new ArgumentNullException("constraintResolver");
            }

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
                    Func<HttpSubRouteCollection> initializer = () => MapHttpAttributeRoutesInternal(configuration, constraintResolver);

                    // This won't change config. It wants to pick up the finalized config.
                    HttpSubRouteCollection subRoutes = attrRoute.EnsureInitialized(initializer);
                    if (subRoutes != null)
                    {
                        AddGenerationHooksForSubRoutes(config.Routes, subRoutes);
                    }
                };
        }

        // Add generation hooks for the Attribute-routing subroutes. 
        // This lets us generate urls for routes supplied by attr-based routing.
        private static void AddGenerationHooksForSubRoutes(HttpRouteCollection destRoutes, HttpSubRouteCollection sourceRoutes)
        {
            foreach (KeyValuePair<string, IHttpRoute> kv in sourceRoutes.NamedRoutes)
            {
                string name = kv.Key;
                IHttpRoute route = kv.Value;
                var stubRoute = new GenerationRoute(route);
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
        private static HttpSubRouteCollection MapHttpAttributeRoutesInternal(this HttpConfiguration configuration,
            IInlineConstraintResolver constraintResolver)
        {
            HttpSubRouteCollection subRoutes = new HttpSubRouteCollection();

            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            List<HttpRouteEntry> attributeRoutes = new List<HttpRouteEntry>();

            IHttpControllerSelector controllerSelector = configuration.Services.GetHttpControllerSelector();
            IDictionary<string, HttpControllerDescriptor> controllerMap = controllerSelector.GetControllerMapping();
            if (controllerMap != null)
            {
                foreach (HttpControllerDescriptor controllerDescriptor in controllerMap.Values)
                {
                    AddRouteEntries(attributeRoutes, controllerDescriptor, constraintResolver);
                }

                foreach (HttpRouteEntry attributeRoute in attributeRoutes)
                {
                    IHttpRoute route = attributeRoute.Route;
                    if (route != null)
                    {
                        subRoutes.Add(attributeRoute.Name, route);
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

        private static void AddRouteEntries(List<HttpRouteEntry> routes, HttpControllerDescriptor controllerDescriptor,
            IInlineConstraintResolver constraintResolver)
        {
            IHttpActionSelector actionSelector = controllerDescriptor.Configuration.Services.GetActionSelector();
            ILookup<string, HttpActionDescriptor> actionMap = actionSelector.GetActionMapping(controllerDescriptor);
            if (actionMap == null)
            {
                return;
            }

            string routePrefix = GetRoutePrefix(controllerDescriptor);
            List<ReflectedHttpActionDescriptor> actionsWithoutRoutes = new List<ReflectedHttpActionDescriptor>();

            foreach (IGrouping<string, HttpActionDescriptor> actionGrouping in actionMap)
            {
                foreach (ReflectedHttpActionDescriptor actionDescriptor in actionGrouping.OfType<ReflectedHttpActionDescriptor>())
                {
                    IReadOnlyCollection<IDirectRouteProvider> routeProviders = GetActionRouteProviders(actionDescriptor);

                    // Ignore the Route attributes from inherited actions.
                    if (actionDescriptor.MethodInfo != null &&
                        actionDescriptor.MethodInfo.DeclaringType != controllerDescriptor.ControllerType)
                    {
                        routeProviders = null;
                    }

                    if (routeProviders != null && routeProviders.Count > 0)
                    {
                        AddRouteEntries(routes, routePrefix, routeProviders,
                            new ReflectedHttpActionDescriptor[] { actionDescriptor }, constraintResolver);
                    }
                    else
                    {
                        // IF there are no routes on the specific action, attach it to the controller routes (if any).
                        actionsWithoutRoutes.Add(actionDescriptor);
                    }
                }
            }

            IReadOnlyCollection<IDirectRouteProvider> controllerRouteProviders =
                GetControllerRouteProviders(controllerDescriptor);

            // If they exist and have not been overridden, create routes for controller-level route providers.
            if (controllerRouteProviders != null && controllerRouteProviders.Count > 0
                && actionsWithoutRoutes.Count > 0)
            {
                AddRouteEntries(routes, routePrefix, controllerRouteProviders, actionsWithoutRoutes,
                    constraintResolver);
            }
        }

        private static void AddRouteEntries(List<HttpRouteEntry> routes, string routePrefix,
            IReadOnlyCollection<IDirectRouteProvider> routeProviders,
            IEnumerable<ReflectedHttpActionDescriptor> actionDescriptors, IInlineConstraintResolver constraintResolver)
        {
            foreach (IDirectRouteProvider routeProvider in routeProviders)
            {
                HttpRouteEntry entry = CreateRouteEntry(routePrefix, routeProvider, actionDescriptors,
                    constraintResolver);
                routes.Add(entry);
            }
        }

        private static HttpRouteEntry CreateRouteEntry(string routePrefix, IDirectRouteProvider routeProvider,
            IEnumerable<ReflectedHttpActionDescriptor> actionDescriptors, IInlineConstraintResolver constraintResolver)
        {
            Contract.Assert(routeProvider != null);

            DirectRouteProviderContext context = new DirectRouteProviderContext(routePrefix, actionDescriptors,
                constraintResolver);
            HttpRouteEntry entry = routeProvider.CreateRoute(context);

            if (entry == null)
            {
                throw Error.InvalidOperation(SRResources.TypeMethodMustNotReturnNull,
                    typeof(IDirectRouteProvider).Name, "CreateRoute");
            }

            return entry;
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

        private static IReadOnlyCollection<IDirectRouteProvider> GetControllerRouteProviders(
            HttpControllerDescriptor controllerDescriptor)
        {
            Collection<IDirectRouteProvider> newProviders =
            controllerDescriptor.GetCustomAttributes<IDirectRouteProvider>(inherit: false);

            Collection<IHttpRouteInfoProvider> oldProviders =
                controllerDescriptor.GetCustomAttributes<IHttpRouteInfoProvider>(inherit: false);

            List<IDirectRouteProvider> combined = new List<IDirectRouteProvider>();

            combined.AddRange(newProviders);

            foreach (IHttpRouteInfoProvider oldProvider in oldProviders)
            {
                if (oldProvider is IDirectRouteProvider)
                {
                    continue;
                }

                combined.Add(new RouteInfoDirectRouteProvider(oldProvider));
            }

            return combined;
        }

        private static IReadOnlyCollection<IDirectRouteProvider> GetActionRouteProviders(
            HttpActionDescriptor actionDescriptor)
        {
            Collection<IDirectRouteProvider> newProviders =
                actionDescriptor.GetCustomAttributes<IDirectRouteProvider>(inherit: false);

            Collection<IHttpRouteInfoProvider> oldProviders =
                actionDescriptor.GetCustomAttributes<IHttpRouteInfoProvider>(inherit: false);

            List<IDirectRouteProvider> combined = new List<IDirectRouteProvider>();

            combined.AddRange(newProviders);

            foreach (IHttpRouteInfoProvider oldProvider in oldProviders)
            {
                if (oldProvider is IDirectRouteProvider)
                {
                    continue;
                }

                combined.Add(new RouteInfoDirectRouteProvider(oldProvider));
            }

            return combined;
        }
    }
}
