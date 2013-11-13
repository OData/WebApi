// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc.Async;
using System.Web.Mvc.Properties;
using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    /// <remarks>
    /// Corresponds to the Web API implementation of attribute routing in
    /// System.Web.Http.Routing.AttributeRoutingMapper.
    /// </remarks>
    internal static class AttributeRoutingMapper
    {
        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="constraintResolver">
        /// The <see cref="IInlineConstraintResolver"/> to use for resolving inline constraints in route templates.
        /// </param>
        public static void MapAttributeRoutes(RouteCollection routes, IInlineConstraintResolver constraintResolver)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }

            if (constraintResolver == null)
            {
                throw new ArgumentNullException("constraintResolver");
            }

            DefaultControllerFactory typesLocator =
                DependencyResolver.Current.GetService<IControllerFactory>() as DefaultControllerFactory
                ?? ControllerBuilder.Current.GetControllerFactory() as DefaultControllerFactory
                ?? new DefaultControllerFactory();

            IReadOnlyList<Type> controllerTypes = typesLocator.GetControllerTypes();

            MapAttributeRoutes(routes, controllerTypes, constraintResolver);
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="controllerTypes">The controller types to scan.</param>
        public static void MapAttributeRoutes(RouteCollection routes, IEnumerable<Type> controllerTypes)
        {
            MapAttributeRoutes(routes, controllerTypes, new DefaultInlineConstraintResolver());
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="controllerTypes">The controller types to scan.</param>
        /// <param name="constraintResolver">
        /// The <see cref="IInlineConstraintResolver"/> to use for resolving inline constraints in route templates.
        /// </param>
        public static void MapAttributeRoutes(RouteCollection routes, IEnumerable<Type> controllerTypes,
            IInlineConstraintResolver constraintResolver)
        {
            SubRouteCollection subRoutes = new SubRouteCollection();
            AddRouteEntries(subRoutes, controllerTypes, constraintResolver);
            IReadOnlyCollection<RouteEntry> entries = subRoutes.Entries;

            if (entries.Count > 0)
            {
                RouteCollectionRoute aggregrateRoute = new RouteCollectionRoute(subRoutes);
                routes.Add(aggregrateRoute);

                // This sort is here to enforce a static ordering for link generation using these routes. 
                // We don't apply dynamic criteria like ActionSelectors on link generation, but we can use the static
                // ones.
                RouteEntry[] sorted = entries
                    .OrderBy(r => r.Route.GetOrder())
                    .ThenBy(r => r.Route.GetPrecedence())
                    .ToArray();

                AddGenerationHooksForSubRoutes(routes, sorted);
            }
        }

        // Add generation hooks for the Attribute-routing subroutes. 
        // This lets us generate urls for routes supplied by attr-based routing.
        private static void AddGenerationHooksForSubRoutes(RouteCollection routeTable, IList<RouteEntry> entries)
        {
            Contract.Assert(entries != null);

            foreach (RouteEntry entry in entries)
            {
                Contract.Assert(entry != null);
                Route route = entry.Route;
                Contract.Assert(route != null);
                RouteBase generationRoute = new GenerationRoute(route);
                string name = entry.Name;

                if (name == null)
                {
                    routeTable.Add(generationRoute);
                }
                else
                {
                    routeTable.Add(name, generationRoute);
                }
            }
        }

        internal static void AddRouteEntries(SubRouteCollection collector, IEnumerable<Type> controllerTypes,
            IInlineConstraintResolver constraintResolver)
        {
            ControllerDescriptorCache descriptorsCache = new AsyncControllerActionInvoker().DescriptorCache;
            IEnumerable<ReflectedAsyncControllerDescriptor> descriptors = controllerTypes
                .Select(
                    type =>
                    descriptorsCache.GetDescriptor(type, innerType => new ReflectedAsyncControllerDescriptor(innerType), type))
                .Cast<ReflectedAsyncControllerDescriptor>();

            foreach (ReflectedAsyncControllerDescriptor controllerDescriptor in descriptors)
            {
                AddRouteEntries(collector, controllerDescriptor, constraintResolver);
            }
        }

        internal static IReadOnlyCollection<RouteEntry> MapAttributeRoutes(
            ReflectedAsyncControllerDescriptor controller)
        {
            SubRouteCollection collector = new SubRouteCollection();
            AddRouteEntries(collector, controller, new DefaultInlineConstraintResolver());
            return collector.Entries;
        }

        internal static void AddRouteEntries(SubRouteCollection collector,
            ReflectedAsyncControllerDescriptor controller, IInlineConstraintResolver constraintResolver)
        {
            string prefix = GetRoutePrefix(controller);

            RouteAreaAttribute area = controller.GetAreaFrom();
            string areaName = controller.GetAreaName(area);
            string areaPrefix = area != null ? area.AreaPrefix ?? area.AreaName : null;
            ValidateAreaPrefixTemplate(areaPrefix, areaName, controller);

            AsyncActionMethodSelector actionSelector = controller.Selector;

            foreach (var method in actionSelector.DirectRouteMethods)
            {
                ActionDescriptor action = CreateActionDescriptor(controller, actionSelector, method);

                IEnumerable<IDirectRouteProvider> providers = GetRouteProviders(method, controller.ControllerType);

                AddRouteEntries(collector, areaPrefix, prefix, providers, new ActionDescriptor[] { action },
                    constraintResolver, targetIsAction: true);
            }

            // Check for controller-level routes. 
            List<ActionDescriptor> actionsWithoutRoutes = new List<ActionDescriptor>();

            foreach (var method in actionSelector.StandardRouteMethods)
            {
                ActionDescriptor action = CreateActionDescriptor(controller, actionSelector, method);

                actionsWithoutRoutes.Add(action);
            }

            IReadOnlyCollection<IDirectRouteProvider> controllerProviders = GetRouteProviders(controller);

            // If they exist and have not been overridden, create routes for controller-level route providers.
            if (controllerProviders.Count > 0 && actionsWithoutRoutes.Count > 0)
            {
                AddRouteEntries(collector, areaPrefix, prefix, controllerProviders, actionsWithoutRoutes,
                    constraintResolver, targetIsAction: false);
            }
        }

        private static ActionDescriptor CreateActionDescriptor(ControllerDescriptor controller,
            AsyncActionMethodSelector actionSelector, MethodInfo method)
        {
            string actionName = actionSelector.GetActionName(method);
            ActionDescriptorCreator creator = actionSelector.GetActionDescriptorDelegate(method);
            Debug.Assert(creator != null);

            return creator(actionName, controller);
        }

        private static void AddRouteEntries(SubRouteCollection collector, string areaPrefix, string prefix,
            IEnumerable<IDirectRouteProvider> providers, IReadOnlyCollection<ActionDescriptor> actions,
            IInlineConstraintResolver constraintResolver, bool targetIsAction)
        {
            foreach (IDirectRouteProvider provider in providers)
            {
                RouteEntry entry = CreateRouteEntry(areaPrefix, prefix, provider, actions, constraintResolver,
                    targetIsAction);
                collector.Add(entry);
            }
        }

        internal static RouteEntry CreateRouteEntry(string areaPrefix, string prefix, IDirectRouteProvider provider,
            IReadOnlyCollection<ActionDescriptor> actions, IInlineConstraintResolver constraintResolver, bool targetIsAction)
        {
            Contract.Assert(provider != null);

            DirectRouteProviderContext context = new DirectRouteProviderContext(areaPrefix, prefix, actions,
                constraintResolver, targetIsAction);
            RouteEntry entry = provider.CreateRoute(context);

            if (entry == null)
            {
                throw new InvalidOperationException(Error.Format(MvcResources.TypeMethodMustNotReturnNull,
                    typeof(IDirectRouteProvider).Name, "CreateRoute"));
            }

            Route route = entry.Route;
            Contract.Assert(route != null);

            ActionDescriptor[] targetActions = route.GetTargetActionDescriptors();

            if (targetActions == null || targetActions.Length == 0)
            {
                throw new InvalidOperationException(MvcResources.DirectRoute_MissingActionDescriptors);
            }

            return entry;
        }

        private static string GetRoutePrefix(ControllerDescriptor controllerDescriptor)
        {
            // this only happens once per controller type, for the lifetime of the application,
            // so we do not need to cache the results
            object[] attributes = controllerDescriptor.GetCustomAttributes(typeof(IRoutePrefix),
                inherit: false);

            if (attributes == null)
            {
                return null;
            }

            if (attributes.Length > 1)
            {
                string errorMessage = Error.Format(
                    MvcResources.RoutePrefix_CannotSupportMultiRoutePrefix,
                    controllerDescriptor.ControllerType.FullName);
                throw new InvalidOperationException(errorMessage);
            }

            if (attributes.Length == 1)
            {
                IRoutePrefix attribute = attributes[0] as IRoutePrefix;

                if (attribute != null)
                {
                    string prefix = attribute.Prefix;
                    if (prefix == null)
                    {
                        string errorMessage = Error.Format(
                            MvcResources.RoutePrefix_PrefixCannotBeNull,
                            controllerDescriptor.ControllerType.FullName);
                        throw new InvalidOperationException(errorMessage);
                    }

                    if (prefix.StartsWith("/", StringComparison.Ordinal)
                        || prefix.EndsWith("/", StringComparison.Ordinal))
                    {
                        string errorMessage = Error.Format(
                            MvcResources.RoutePrefix_CannotStartOrEnd_WithForwardSlash, prefix,
                            controllerDescriptor.ControllerName);
                        throw new InvalidOperationException(errorMessage);
                    }

                    return prefix;
                }
            }

            return null;
        }

        public static IReadOnlyCollection<IDirectRouteProvider> GetRouteProviders(ControllerDescriptor controller)
        {
            object[] attributes = controller.GetCustomAttributes(inherit: false);
            IEnumerable<IDirectRouteProvider> newProviders = attributes.OfType<IDirectRouteProvider>();
            IEnumerable<IRouteInfoProvider> oldProviders = attributes.OfType<IRouteInfoProvider>();

            List<IDirectRouteProvider> combined = new List<IDirectRouteProvider>();
            combined.AddRange(newProviders);

            foreach (IRouteInfoProvider oldProvider in oldProviders)
            {
                if (oldProvider is IDirectRouteProvider)
                {
                    continue;
                }

                combined.Add(new RouteInfoDirectRouteProvider(oldProvider));
            }

            return combined;
        }

        private static IEnumerable<IDirectRouteProvider> GetRouteProviders(MethodInfo methodInfo, Type controllerType)
        {
            // Skip Route attributes on inherited actions.
            if (methodInfo.DeclaringType != controllerType)
            {
                return Enumerable.Empty<IDirectRouteProvider>();
            }

            // We do not want to cache this as these attributes are only being looked up during
            // application's init time, so there will be no perf gain, and we will end up
            // storing that cache for no reason
            object[] attributes = methodInfo.GetCustomAttributes(inherit: false);

            IEnumerable<IDirectRouteProvider> newProviders = attributes.OfType<IDirectRouteProvider>();

            IEnumerable<IRouteInfoProvider> oldProviders = attributes
                .OfType<IRouteInfoProvider>()
                .Where(attr => attr.Template != null);

            List<IDirectRouteProvider> combined = new List<IDirectRouteProvider>();
            combined.AddRange(newProviders);

            foreach (IRouteInfoProvider oldProvider in oldProviders)
            {
                if (oldProvider is IDirectRouteProvider)
                {
                    continue;
                }

                combined.Add(new RouteInfoDirectRouteProvider(oldProvider));
            }

            return combined;
        }

        private static void ValidateAreaPrefixTemplate(string areaPrefix, string areaName, ControllerDescriptor controllerDescriptor)
        {
            if (areaPrefix != null && areaPrefix.EndsWith("/", StringComparison.Ordinal))
            {
                string errorMessage = Error.Format(MvcResources.RouteAreaPrefix_CannotEnd_WithForwardSlash,
                                                   areaPrefix, areaName, controllerDescriptor.ControllerName);
                throw new InvalidOperationException(errorMessage);
            }
        }
    }
}