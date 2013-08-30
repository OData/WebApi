// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Web.Mvc.Properties;
using System.Web.Mvc.Routing;

namespace System.Web.Mvc
{
    internal static class ControllerDescriptorExtensions
    {
        public static IEnumerable<IRouteInfoProvider> GetDirectRoutes(this ControllerDescriptor controller)
        {
            return controller.GetCustomAttributes(inherit: false).OfType<IRouteInfoProvider>();
        }

        public static string GetAreaName(this ControllerDescriptor controllerDescriptor, RouteAreaAttribute area)
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

        public static RouteAreaAttribute GetAreaFrom(this ControllerDescriptor controllerDescriptor)
        {
            RouteAreaAttribute areaAttribute =
                controllerDescriptor.GetCustomAttributes(typeof(RouteAreaAttribute), inherit: true)
                                    .Cast<RouteAreaAttribute>()
                                    .FirstOrDefault();
            return areaAttribute;
        }

        public static string GetPrefixFrom(this ControllerDescriptor controllerDescriptor)
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
    }
}