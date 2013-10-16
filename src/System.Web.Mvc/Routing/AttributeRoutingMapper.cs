// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly RouteBuilder2 _routeBuilder;

        public AttributeRoutingMapper(RouteBuilder2 routeBuilder)
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

            return routeEntries;
        }

        internal List<RouteEntry> MapMvcAttributeRoutes(ReflectedAsyncControllerDescriptor controllerDescriptor)
        {
            string prefix = controllerDescriptor.GetPrefixFrom();
            ValidatePrefixTemplate(prefix, controllerDescriptor);

            RouteAreaAttribute area = controllerDescriptor.GetAreaFrom();
            string areaName = controllerDescriptor.GetAreaName(area);
            string areaPrefix = area != null ? area.AreaPrefix ?? area.AreaName : null;

            ValidateAreaPrefixTemplate(areaPrefix, areaName, controllerDescriptor);

            AsyncActionMethodSelector actionSelector = controllerDescriptor.Selector;
            IEnumerable<MethodInfo> actionMethodsInfo = actionSelector.DirectRouteMethods;

            List<RouteEntry> routeEntries = new List<RouteEntry>();

            foreach (var method in actionMethodsInfo)
            {
                string actionName = actionSelector.GetActionName(method);
                ActionDescriptorCreator creator = actionSelector.GetActionDescriptorDelegate(method);
                Debug.Assert(creator != null);

                ActionDescriptor actionDescriptor = creator(actionName, controllerDescriptor);

                IEnumerable<IRouteInfoProvider> routeAttributes = GetRouteAttributes(method, controllerDescriptor.ControllerType);

                foreach (var routeAttribute in routeAttributes)
                {
                    ValidateTemplate(routeAttribute.Template, actionName, controllerDescriptor);

                    string template = CombinePrefixAndAreaWithTemplate(areaPrefix, prefix, routeAttribute.Template);
                    Route route = _routeBuilder.BuildDirectRoute(template, routeAttribute, controllerDescriptor, actionDescriptor);

                    RouteEntry entry = new RouteEntry
                    {
                        Name = routeAttribute.Name,
                        Route = route,
                        Template = template,
                    };

                    routeEntries.Add(entry);
                }
            }

            // Check for controller-level routes. 
            IEnumerable<IRouteInfoProvider> controllerRouteAttributes = controllerDescriptor.GetDirectRoutes();

            List<ActionDescriptor> actions = new List<ActionDescriptor>();
            foreach (var actionMethod in actionSelector.StandardRouteMethods)
            {
                string actionName = actionSelector.GetActionName(actionMethod);
                ActionDescriptorCreator creator = actionSelector.GetActionDescriptorDelegate(actionMethod);
                Debug.Assert(creator != null);

                ActionDescriptor actionDescriptor = creator(actionName, controllerDescriptor);
                actions.Add(actionDescriptor);
            }

            // Don't create a route for the controller-level attributes if no actions could possibly match
            if (actions.Any())
            {
                foreach (var routeAttribute in controllerRouteAttributes)
                {
                    string template = CombinePrefixAndAreaWithTemplate(areaPrefix, prefix, routeAttribute.Template);

                    Route route = _routeBuilder.BuildDirectRoute(template, routeAttribute, controllerDescriptor, actions);
                    RouteEntry entry = new RouteEntry
                    {
                        Name = routeAttribute.Name,
                        Route = route,
                        Template = template,
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

        private static IEnumerable<IRouteInfoProvider> GetRouteAttributes(MethodInfo methodInfo, Type controllerType)
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
    }
}