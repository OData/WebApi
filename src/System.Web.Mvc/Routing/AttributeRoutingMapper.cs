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
    internal class AttributeRoutingMapper
    {
        private readonly RouteBuilder2 _routeBuilder;

        public AttributeRoutingMapper(RouteBuilder2 routeBuilder)
        {
            _routeBuilder = routeBuilder;
        }

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
            AttributeRoutingMapper mapper = new AttributeRoutingMapper(new RouteBuilder2(constraintResolver));

            SubRouteCollection subRoutes = new SubRouteCollection();
            mapper.AddRouteEntries(subRoutes, controllerTypes);
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

        internal void AddRouteEntries(SubRouteCollection collector, IEnumerable<Type> controllerTypes)
        {
            ControllerDescriptorCache descriptorsCache = new AsyncControllerActionInvoker().DescriptorCache;
            IEnumerable<ReflectedAsyncControllerDescriptor> descriptors = controllerTypes
                .Select(
                    type =>
                    descriptorsCache.GetDescriptor(type, innerType => new ReflectedAsyncControllerDescriptor(innerType), type))
                .Cast<ReflectedAsyncControllerDescriptor>();

            foreach (ReflectedAsyncControllerDescriptor controllerDescriptor in descriptors)
            {
                AddRouteEntries(collector, controllerDescriptor);
            }
        }

        internal IReadOnlyCollection<RouteEntry> MapAttributeRoutes(ReflectedAsyncControllerDescriptor controller)
        {
            SubRouteCollection collector = new SubRouteCollection();
            AddRouteEntries(collector, controller);
            return collector.Entries;
        }

        internal void AddRouteEntries(SubRouteCollection collector, ReflectedAsyncControllerDescriptor controller)
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

                IEnumerable<IRouteInfoProvider> providers = GetRouteProviders(method, controller.ControllerType);

                AddRouteEntries(collector, areaPrefix, prefix, providers, controller,
                    new ActionDescriptor[] { action }, routeIsForAction: true);
            }

            // Check for controller-level routes. 
            List<ActionDescriptor> actionsWithoutRoutes = new List<ActionDescriptor>();

            foreach (var method in actionSelector.StandardRouteMethods)
            {
                ActionDescriptor action = CreateActionDescriptor(controller, actionSelector, method);

                actionsWithoutRoutes.Add(action);
            }

            IReadOnlyCollection<IRouteInfoProvider> controllerProviders = GetRouteProviders(controller);

            // If they exist and have not been overridden, create routes for controller-level route providers.
            if (controllerProviders.Count > 0 && actionsWithoutRoutes.Count > 0)
            {
                AddRouteEntries(collector, areaPrefix, prefix, controllerProviders, controller, actionsWithoutRoutes,
                    routeIsForAction: false);
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

        private void AddRouteEntries(SubRouteCollection collector, string areaPrefix, string prefix,
            IEnumerable<IRouteInfoProvider> providers, ControllerDescriptor controller,
            IList<ActionDescriptor> actions, bool routeIsForAction)
        {
            foreach (IRouteInfoProvider provider in providers)
            {
                RouteEntry entry = CreateRouteEntry(areaPrefix, prefix, provider, controller, actions,
                    routeIsForAction);
                collector.Add(entry);
            }
        }

        private RouteEntry CreateRouteEntry(string areaPrefix, string prefix, IRouteInfoProvider provider,
            ControllerDescriptor controller, IList<ActionDescriptor> actions, bool routeIsForAction)
        {
            ValidateTemplate(provider.Template, actions[0].ActionName, controller);
            string template = CombinePrefixAndAreaWithTemplate(areaPrefix, prefix, provider.Template);
            Route route = _routeBuilder.BuildDirectRoute(template, provider, controller, actions, routeIsForAction);

            RouteEntry entry = new RouteEntry
            {
                Name = provider.Name,
                Route = route,
                Template = template,
            };

            return entry;
        }

        private static string GetRoutePrefix(ControllerDescriptor controllerDescriptor)
        {
            // this only happens once per controller type, for the lifetime of the application,
            // so we do not need to cache the results
            object[] attributes = controllerDescriptor.GetCustomAttributes(typeof(RoutePrefixAttribute),
                inherit: false);

            if (attributes.Length > 0)
            {
                RoutePrefixAttribute attribute = attributes[0] as RoutePrefixAttribute;

                if (attribute != null)
                {
                    string prefix = attribute.Prefix;

                    if (prefix != null)
                    {
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
            }

            return null;
        }

        public static IReadOnlyCollection<IRouteInfoProvider> GetRouteProviders(ControllerDescriptor controller)
        {
            return controller.GetCustomAttributes(inherit: false).OfType<IRouteInfoProvider>().AsArray();
        }

        private static IEnumerable<IRouteInfoProvider> GetRouteProviders(MethodInfo methodInfo, Type controllerType)
        {
            // Skip Route attributes on inherited actions.
            if (methodInfo.DeclaringType != controllerType)
            {
                return Enumerable.Empty<IRouteInfoProvider>();
            }

            // We do not want to cache this as these attributes are only being looked up during
            // application's init time, so there will be no perf gain, and we will end up
            // storing that cache for no reason
            return methodInfo.GetCustomAttributes(inherit: false)
              .OfType<IRouteInfoProvider>()
              .Where(attr => attr.Template != null);
        }

        internal static string CombinePrefixAndAreaWithTemplate(string areaPrefix, string prefix, string template)
        {
            Contract.Assert(template != null);

            // If the attribute's template starts with '~/', ignore the area and controller prefixes
            if (template.StartsWith("~/", StringComparison.Ordinal))
            {
                return template.Substring(2);
            }

            if (prefix == null && areaPrefix == null)
            {
                return template;
            }

            StringBuilder templateBuilder = new StringBuilder();

            if (areaPrefix != null)
            {
                templateBuilder.Append(areaPrefix);
            }

            if (!String.IsNullOrEmpty(prefix))
            {
                if (templateBuilder.Length > 0)
                {
                    templateBuilder.Append('/');
                }
                templateBuilder.Append(prefix);
            }

            if (!String.IsNullOrEmpty(template))
            {
                if (templateBuilder.Length > 0)
                {
                    templateBuilder.Append('/');
                }
                templateBuilder.Append(template);
            }

            return templateBuilder.ToString();
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

        private static void ValidateTemplate(string routeTemplate, string actionName, ControllerDescriptor controllerDescriptor)
        {
            if (routeTemplate.StartsWith("/", StringComparison.Ordinal))
            {
                string errorMessage = Error.Format(MvcResources.RouteTemplate_CannotStart_WithForwardSlash,
                                                   routeTemplate, actionName, controllerDescriptor.ControllerName);
                throw new InvalidOperationException(errorMessage);
            }
        }
    }
}