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
            string prefix = GetPrefixFrom(controllerDescriptor);
            ValidatePrefixTemplate(prefix, controllerDescriptor);            

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
                string actionName = GetActionName(method, actionSelector.AllowLegacyAsyncActions);
                IEnumerable<IDirectRouteInfoProvider> routeAttributes = GetRouteAttributes(method);

                foreach (var routeAttribute in routeAttributes)
                {
                    ValidateTemplate(routeAttribute.RouteTemplate, actionName, controllerDescriptor);

                    string template = CombinePrefixAndAreaWithTemplate(areaPrefix, prefix, routeAttribute.RouteTemplate);
                    Route route = _routeBuilder.BuildDirectRoute(template, routeAttribute.Verbs, controllerName,
                                                                    actionName, method, areaName);
                    RouteEntry entry = new RouteEntry
                    {
                        Name = routeAttribute.RouteName,
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

        private static void ValidatePrefixTemplate(string prefix, ControllerDescriptor controllerDescriptor)
        {
            if (prefix != null && (prefix.StartsWith("/", StringComparison.Ordinal) || prefix.EndsWith("/", StringComparison.Ordinal)))
            {
                string errorMessage = Error.Format(MvcResources.RoutePrefix_CannotStartOrEnd_WithForwardSlash,
                                                   prefix, controllerDescriptor.ControllerName);
                throw new InvalidOperationException(errorMessage);
            }
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
            object[] routePrefixAttributes = controllerDescriptor.GetCustomAttributes(typeof(RoutePrefixAttribute), inherit: false);
            if (routePrefixAttributes.Length > 0)
            {
                RoutePrefixAttribute routePrefixAttribute = routePrefixAttributes[0] as RoutePrefixAttribute;
                if (routePrefixAttribute != null)
                {
                    return routePrefixAttribute.Prefix;
                }
            }

            return null;
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

        private static string GetActionName(MethodInfo method, bool allowLegacyAsyncActions)
        {
            // Check for ActionName attribute
            object[] nameAttributes = method.GetCustomAttributes(typeof(ActionNameAttribute), inherit: true);
            if (nameAttributes.Length > 0)
            {
                ActionNameAttribute nameAttribute = nameAttributes[0] as ActionNameAttribute;
                if (nameAttribute != null)
                {
                    return nameAttribute.Name;
                }
            }

            const string AsyncMethodSuffix = "Async";
            if (allowLegacyAsyncActions && method.Name.EndsWith(AsyncMethodSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return method.Name.Substring(0, method.Name.Length - AsyncMethodSuffix.Length);
            }

            return method.Name;
        }
    }
}