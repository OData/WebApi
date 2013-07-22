// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc.Async;
using System.Web.Mvc.Properties;
using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    internal class AttributeRoutingMapper
    {
        private readonly RouteBuilder _routeBuilder;

        public AttributeRoutingMapper(RouteBuilder routeBuilder)
        {
            _routeBuilder = routeBuilder;
        }

        internal List<RouteEntry> MapMvcAttributeRoutes(IEnumerable<Type> controllerTypes)
        {
            ControllerDescriptorCache descriptorsCache = new AsyncControllerActionInvoker().DescriptorCache;
            IEnumerable<ReflectedAsyncControllerDescriptor> descriptors = controllerTypes
                .Select(
                    type =>
                    descriptorsCache.GetDescriptor(type, innerType => new ReflectedAsyncControllerDescriptor(innerType), type))
                .Cast<ReflectedAsyncControllerDescriptor>();

            List<RouteEntry> routeEntries = new List<RouteEntry>();

            foreach (ReflectedAsyncControllerDescriptor controllerDescriptor in descriptors)
            {
                routeEntries.AddRange(MapMvcAttributeRoutes(controllerDescriptor));
            }

            routeEntries.Sort();

            return routeEntries;
        }

        internal List<RouteEntry> MapMvcAttributeRoutes(ReflectedAsyncControllerDescriptor controllerDescriptor)
        {
            RoutePrefixAttribute prefixAttribute = GetPrefixFrom(controllerDescriptor);
            ValidatePrefixTemplate(prefixAttribute, controllerDescriptor);            

            RouteAreaAttribute area = GetAreaFrom(controllerDescriptor);
            string areaName = GetAreaName(controllerDescriptor, area);
            string areaPrefix = area != null ? area.AreaPrefix ?? area.AreaName : null;
            ValidateAreaPrefixTemplate(areaPrefix, areaName, controllerDescriptor);

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

            List<RouteEntry> routeEntries = new List<RouteEntry>();

            foreach (var method in actionMethodsInfo)
            {
                string actionName = GetCanonicalActionName(method, actionSelector.AllowLegacyAsyncActions);
                IEnumerable<IDirectRouteInfoProvider> routeAttributes = GetRouteAttributes(method);

                foreach (var routeAttribute in routeAttributes)
                {
                    ValidateTemplate(routeAttribute, actionName, controllerDescriptor);

                    string prefix = prefixAttribute != null ? prefixAttribute.Prefix : null;

                    string template = CombinePrefixAndAreaWithTemplate(areaPrefix, prefix, routeAttribute.RouteTemplate);
                    Route route = _routeBuilder.BuildDirectRoute(template, routeAttribute.Verbs, controllerName,
                                                                    actionName, method, areaName);
                    RouteEntry entry = new RouteEntry
                    {
                        Name = routeAttribute.RouteName ?? template,
                        Route = route,
                        RouteTemplate = template,
                        ParsedRoute = RouteParser.Parse(route.Url),
                        Order = routeAttribute.RouteOrder                        
                    };
                    routeEntries.Add(entry);                    
                }
            }

            return routeEntries;
        }

        private static void ValidatePrefixTemplate(RoutePrefixAttribute prefixAttribute, ControllerDescriptor controllerDescriptor)
        {
            if (prefixAttribute != null && !IsValidTemplate(prefixAttribute.Prefix))
            {
                string errorMessage = Error.Format(MvcResources.RoutePrefix_CannotStartOrEnd_WithForwardSlash,
                                                   prefixAttribute.Prefix, controllerDescriptor.ControllerName);
                throw new InvalidOperationException(errorMessage);
            }
        }

        private static void ValidateAreaPrefixTemplate(string areaPrefix, string areaName, ControllerDescriptor controllerDescriptor)
        {
            if (areaPrefix != null && !IsValidTemplate(areaPrefix))
            {
                string errorMessage = Error.Format(MvcResources.RouteAreaPrefix_CannotStartOrEnd_WithForwardSlash,
                                                   areaPrefix, areaName, controllerDescriptor.ControllerName);
                throw new InvalidOperationException(errorMessage);
            }
        }

        private static void ValidateTemplate(IDirectRouteInfoProvider routeInfoProvider, string actionName, ControllerDescriptor controllerDescriptor)
        {
            if (!IsValidTemplate(routeInfoProvider.RouteTemplate))
            {
                string errorMessage = Error.Format(MvcResources.RouteTemplate_CannotStartOrEnd_WithForwardSlash,
                                                   routeInfoProvider.RouteTemplate, actionName,
                                                   controllerDescriptor.ControllerName);
                throw new InvalidOperationException(errorMessage);
            }
        }

        private static bool IsValidTemplate(string template)
        {
            return !template.StartsWith("/", StringComparison.Ordinal) &&
                   !template.EndsWith("/", StringComparison.Ordinal);
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

        private static RoutePrefixAttribute GetPrefixFrom(ReflectedAsyncControllerDescriptor controllerDescriptor)
        {
            // this only happens once per controller type, for the lifetime of the application,
            // so we do not need to cache the results
           return controllerDescriptor.GetCustomAttributes(typeof(RoutePrefixAttribute), inherit: false)
                                    .Cast<RoutePrefixAttribute>().SingleOrDefault();
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