// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Properties;

namespace System.Web.Http.Routing
{
    /// <remarks>
    /// Corresponds to the MVC implementation of attribute routing in System.Web.Mvc.Routing.AttributeRoutingMapper.
    /// </remarks>
    internal static class AttributeRoutingMapper
    {
        // Attribute routing will inject a single top-level route into the route table. 
        private const string AttributeRouteName = "MS_attributerouteWebApi";

        public static void MapAttributeRoutes(HttpConfiguration configuration,
            IInlineConstraintResolver constraintResolver)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (constraintResolver == null)
            {
                throw new ArgumentNullException("constraintResolver");
            }

            RouteCollectionRoute aggregateRoute = new RouteCollectionRoute();
            configuration.Routes.Add(AttributeRouteName, aggregateRoute);

            Action<HttpConfiguration> previousInitializer = configuration.Initializer;
            configuration.Initializer = config =>
                {
                    // Chain to the previous initializer hook. Do this before we access the config since
                    // initialization may make last minute changes to the configuration.
                    previousInitializer(config);

                    SubRouteCollection subRoutes = null;

                    // Add a single placeholder route that handles all of attribute routing.
                    // Add an initialize hook that initializes these routes after the config has been initialized.
                    Func<SubRouteCollection> initializer = () =>
                    {
                        subRoutes = new SubRouteCollection();
                        AddRouteEntries(subRoutes, configuration, constraintResolver);
                        return subRoutes;
                    };

                    // This won't change config. It wants to pick up the finalized config.
                    aggregateRoute.EnsureInitialized(initializer);

                    if (subRoutes != null)
                    {
                        AddGenerationHooksForSubRoutes(config.Routes, subRoutes.Entries);
                    }
                };
        }

        // Add generation hooks for the Attribute-routing subroutes. 
        // This lets us generate urls for routes supplied by attr-based routing.
        private static void AddGenerationHooksForSubRoutes(HttpRouteCollection routeTable,
            IEnumerable<RouteEntry> entries)
        {
            Contract.Assert(entries != null);
            foreach (RouteEntry entry in entries)
            {
                Contract.Assert(entry != null);
                string name = entry.Name;

                if (name != null)
                {
                    IHttpRoute route = entry.Route;
                    Contract.Assert(route != null);
                    IHttpRoute linkGenerationRoute = new LinkGenerationRoute(route);
                    routeTable.Add(name, linkGenerationRoute);
                }
            }
        }

        private static void AddRouteEntries(SubRouteCollection collector, HttpConfiguration configuration,
            IInlineConstraintResolver constraintResolver)
        {
            Contract.Assert(configuration != null);

            IHttpControllerSelector controllerSelector = configuration.Services.GetHttpControllerSelector();
            IDictionary<string, HttpControllerDescriptor> controllerMap = controllerSelector.GetControllerMapping();
            if (controllerMap != null)
            {
                foreach (HttpControllerDescriptor controllerDescriptor in controllerMap.Values)
                {
                    AddRouteEntries(collector, controllerDescriptor, constraintResolver);
                }
            }
        }

        private static void AddRouteEntries(SubRouteCollection collector, HttpControllerDescriptor controller,
            IInlineConstraintResolver constraintResolver)
        {
            IHttpActionSelector actionSelector = controller.Configuration.Services.GetActionSelector();
            ILookup<string, HttpActionDescriptor> actionMap = actionSelector.GetActionMapping(controller);
            if (actionMap == null)
            {
                return;
            }

            string prefix = GetRoutePrefix(controller);
            List<ReflectedHttpActionDescriptor> actionsWithoutRoutes = new List<ReflectedHttpActionDescriptor>();

            foreach (IGrouping<string, HttpActionDescriptor> actionGrouping in actionMap)
            {
                foreach (ReflectedHttpActionDescriptor action in actionGrouping.OfType<ReflectedHttpActionDescriptor>())
                {
                    IReadOnlyCollection<IDirectRouteFactory> factories = GetRouteFactories(action);

                    // Ignore the Route attributes from inherited actions.
                    if (action.MethodInfo != null &&
                        action.MethodInfo.DeclaringType != controller.ControllerType)
                    {
                        factories = null;
                    }

                    if (factories != null && factories.Count > 0)
                    {
                        AddRouteEntries(collector, prefix, factories,
                            new ReflectedHttpActionDescriptor[] { action }, constraintResolver, targetIsAction: true);
                    }
                    else
                    {
                        // IF there are no routes on the specific action, attach it to the controller routes (if any).
                        actionsWithoutRoutes.Add(action);
                    }
                }
            }

            IReadOnlyCollection<IDirectRouteFactory> controllerFactories = GetRouteFactories(controller);

            // If they exist and have not been overridden, create routes for controller-level route providers.
            if (controllerFactories.Count > 0 && actionsWithoutRoutes.Count > 0)
            {
                AddRouteEntries(collector, prefix, controllerFactories, actionsWithoutRoutes,
                    constraintResolver, targetIsAction: false);
            }
        }

        private static void AddRouteEntries(SubRouteCollection collector, string prefix,
            IReadOnlyCollection<IDirectRouteFactory> factories,
            IReadOnlyCollection<HttpActionDescriptor> actions, IInlineConstraintResolver constraintResolver, bool targetIsAction)
        {
            foreach (IDirectRouteFactory factory in factories)
            {
                RouteEntry entry = CreateRouteEntry(prefix, factory, actions, constraintResolver, targetIsAction);
                collector.Add(entry);
            }
        }

        internal static RouteEntry CreateRouteEntry(
            string prefix, 
            IDirectRouteFactory factory,
            IReadOnlyCollection<HttpActionDescriptor> actions, 
            IInlineConstraintResolver constraintResolver,
            bool targetIsAction)
        {
            Contract.Assert(factory != null);

            DirectRouteFactoryContext context = new DirectRouteFactoryContext(prefix, actions, constraintResolver, targetIsAction);
            RouteEntry entry = factory.CreateRoute(context);

            if (entry == null)
            {
                throw Error.InvalidOperation(SRResources.TypeMethodMustNotReturnNull,
                    typeof(IDirectRouteFactory).Name, "CreateRoute");
            }

            IHttpRoute route = entry.Route;
            Contract.Assert(route != null);

            HttpActionDescriptor[] targetActions = GetTargetActionDescriptors(route);

            if (targetActions == null || targetActions.Length == 0)
            {
                throw new InvalidOperationException(SRResources.DirectRoute_MissingActionDescriptors);
            }

            if (route.Handler != null)
            {
                throw new InvalidOperationException(SRResources.DirectRoute_HandlerNotSupported);
            }

            return entry;
        }

        private static HttpActionDescriptor[] GetTargetActionDescriptors(IHttpRoute route)
        {
            Contract.Assert(route != null);
            IDictionary<string, object> dataTokens = route.DataTokens;

            if (dataTokens == null)
            {
                return null;
            }

            HttpActionDescriptor[] actions;

            if (!dataTokens.TryGetValue<HttpActionDescriptor[]>(RouteDataTokenKeys.Actions, out actions))
            {
                return null;
            }

            return actions;
        }

        // Use `internal` instead of `private` here for unit tests.
        internal static string GetRoutePrefix(HttpControllerDescriptor controller)
        {
            Collection<IRoutePrefix> attributes =
                controller.GetCustomAttributes<IRoutePrefix>(inherit: false);

            if (attributes == null)
            {
                return null;
            }

            if (attributes.Count > 1)
            {
                string errorMessage = Error.Format(SRResources.RoutePrefix_CannotSupportMultiRoutePrefix, controller.ControllerType.FullName);
                throw new InvalidOperationException(errorMessage);
            }

            if (attributes.Count == 1)
            {
                IRoutePrefix attribute = attributes[0];

                if (attribute != null)
                {
                    string prefix = attribute.Prefix;
                    if (prefix == null)
                    {
                        string errorMessage = Error.Format(
                            SRResources.RoutePrefix_PrefixCannotBeNull,
                            controller.ControllerType.FullName);
                        throw new InvalidOperationException(errorMessage);
                    }

                    if (prefix.EndsWith("/", StringComparison.Ordinal))
                    {
                        throw Error.InvalidOperation(SRResources.AttributeRoutes_InvalidPrefix, prefix,
                            controller.ControllerName);
                    }

                    return prefix;
                }
            }

            return null;
        }

        private static IReadOnlyCollection<IDirectRouteFactory> GetRouteFactories(HttpControllerDescriptor controller)
        {
            Collection<IDirectRouteFactory> newFactories =
                controller.GetCustomAttributes<IDirectRouteFactory>(inherit: false);

            Collection<IHttpRouteInfoProvider> oldProviders =
                controller.GetCustomAttributes<IHttpRouteInfoProvider>(inherit: false);

            List<IDirectRouteFactory> combined = new List<IDirectRouteFactory>();
            combined.AddRange(newFactories);

            foreach (IHttpRouteInfoProvider oldProvider in oldProviders)
            {
                if (oldProvider is IDirectRouteFactory)
                {
                    continue;
                }

                combined.Add(new RouteInfoDirectRouteFactory(oldProvider));
            }

            return combined;
        }

        private static IReadOnlyCollection<IDirectRouteFactory> GetRouteFactories(HttpActionDescriptor action)
        {
            Collection<IDirectRouteFactory> newFactories =
                action.GetCustomAttributes<IDirectRouteFactory>(inherit: false);

            Collection<IHttpRouteInfoProvider> oldProviders =
                action.GetCustomAttributes<IHttpRouteInfoProvider>(inherit: false);

            List<IDirectRouteFactory> combined = new List<IDirectRouteFactory>();
            combined.AddRange(newFactories);

            foreach (IHttpRouteInfoProvider oldProvider in oldProviders)
            {
                if (oldProvider is IDirectRouteFactory)
                {
                    continue;
                }

                combined.Add(new RouteInfoDirectRouteFactory(oldProvider));
            }

            return combined;
        }
    }
}
