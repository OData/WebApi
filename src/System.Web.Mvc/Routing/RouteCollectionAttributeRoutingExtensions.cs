// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc.Async;
using System.Web.Mvc.Properties;
using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    /// <summary>
    /// Helper methods on <see cref="RouteCollection"/> for mapping MVC actions with routes attributes into Route instances on the RouteCollection.
    /// </summary>
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

            ControllerDescriptorCache descriptorsCache = new AsyncControllerActionInvoker().DescriptorCache;
            IEnumerable<ReflectedAsyncControllerDescriptor> descriptors = typesLocator.GetControllerTypes()
                .Select(
                    type =>
                    descriptorsCache.GetDescriptor(type, innerType => new ReflectedAsyncControllerDescriptor(innerType), type))
                .Cast<ReflectedAsyncControllerDescriptor>();

            foreach (ReflectedAsyncControllerDescriptor controllerDescriptor in descriptors)
            {
                MapAttributeRoutesFromController(routes, controllerDescriptor);
            }
        }

        internal static void MapAttributeRoutesFromController(this RouteCollection routes, ReflectedAsyncControllerDescriptor controllerDescriptor)
        {
            string prefix = GetPrefixFrom(controllerDescriptor);
            RouteAreaAttribute area = GetAreaFrom(controllerDescriptor);
            string areaName = GetAreaName(controllerDescriptor, area);
            string areaPrefix = area != null ? area.AreaPrefix ?? area.AreaName : null;

            string controllerName = controllerDescriptor.ControllerName;

            AsyncActionMethodSelector actionSelector = controllerDescriptor.Selector;
            IEnumerable<MethodInfo> actionMethodsInfo = actionSelector.AliasedMethods
                .Concat(actionSelector.NonAliasedMethods.SelectMany(x => x))
                .Where(m => m.DeclaringType == controllerDescriptor.ControllerType);

            if (actionSelector.AllowLegacyAsyncActions)
            {
                // if the ActionAsync / ActionCompleted pattern is used, we need to remove the "Completed" methods
                // and not look up routing attributes on them
                actionMethodsInfo =
                    actionMethodsInfo.Where(m => !m.Name.EndsWith("Completed", StringComparison.OrdinalIgnoreCase));
            }

            // todo: extensibility is coming up next
            RouteBuilder routeBuilder = new RouteBuilder();

            foreach (MethodInfo method in actionMethodsInfo)
            {
                string actionName = GetCanonicalActionName(method, actionSelector.AllowLegacyAsyncActions);
                IEnumerable<IDirectRouteInfoProvider> routeAttributes = GetRouteAttributes(method);

                foreach (IDirectRouteInfoProvider routeAttribute in routeAttributes)
                {
                    string template = CombinePrefixAndAreaWithTemplate(areaPrefix, prefix, routeAttribute.RouteTemplate);
                    Route route = routeBuilder.BuildDirectRoute(template, routeAttribute.Verbs, controllerName,
                                                            actionName, method, areaName);
                    routes.Add(routeAttribute.RouteName, route);
                }
            }
        }

        private static IEnumerable<IDirectRouteInfoProvider> GetRouteAttributes(MethodInfo methodInfo)
        {
            // We do not want to cache this as these attributes are only being looked up during
            // application's init time, so there will be no perf gain, and we will end up
            // storing that cache for no reason
            return methodInfo.GetCustomAttributes(inherit: false)
              .OfType<IDirectRouteInfoProvider>()
              .Where(attr => attr.RouteTemplate != null)
              .ToArray();
        }

        private static string GetAreaName(ReflectedAsyncControllerDescriptor controllerDescriptor, RouteAreaAttribute area)
        {
            if (area == null)
            {
                return null;
            }
            
            if (area.AreaName != null)
            {
                return area.AreaName;
            }
            if (controllerDescriptor.ControllerType.Namespace != null)
            {
                return controllerDescriptor.ControllerType.Namespace.Split('.').Last();
            }

            throw Error.InvalidOperation(MvcResources.AttributeRouting_CouldNotInferAreaNameFromMissingNamespace, controllerDescriptor.ControllerName);
        }

        private static RouteAreaAttribute GetAreaFrom(ReflectedAsyncControllerDescriptor controllerDescriptor)
        {
            RouteAreaAttribute areaAttribute =
                controllerDescriptor.GetCustomAttributes(typeof(RouteAreaAttribute), true)
                                    .Cast<RouteAreaAttribute>()
                                    .FirstOrDefault();
            return areaAttribute;
        }

        private static string GetPrefixFrom(ReflectedAsyncControllerDescriptor controllerDescriptor)
        {
            // this only happens once per controller type, for the lifetime of the application,
            // so we do not need to cache the results
            RoutePrefixAttribute prefixAttribute =
                controllerDescriptor.GetCustomAttributes(typeof(RoutePrefixAttribute), false)
                                    .Cast<RoutePrefixAttribute>()
                                    .FirstOrDefault();

            string prefix = null;
            if (prefixAttribute != null && !String.IsNullOrEmpty(prefixAttribute.Prefix))
            {
                prefix = prefixAttribute.Prefix;
            }
            return prefix;
        }

        internal static string CombinePrefixAndAreaWithTemplate(string areaPrefix, string prefix, string template)
        {
            Contract.Assert(template != null);

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

        private static string GetCanonicalActionName(MethodInfo method, bool allowLegacyAsyncActions)
        {
            const string AsyncMethodSuffix = "Async";
            if (allowLegacyAsyncActions && method.Name.EndsWith(AsyncMethodSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return method.Name.Substring(0, method.Name.Length - AsyncMethodSuffix.Length);
            }

            return method.Name;
        }
    }
}