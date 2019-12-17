// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    class ODataActionSelectorTestHelper
    {
        public static bool ActionMatchesMethod(ActionDescriptor action, MethodInfo method)
        {
            if (action.DisplayName != method.Name)
            {
                return false;
            }
            var parameters = method.GetParameters();
            if (parameters.Length != action.Parameters.Count)
            {
                return false;
            }
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].Name != action.Parameters[i].Name ||
                    parameters[i].ParameterType != action.Parameters[i].ParameterType)
                {
                    return false;
                }
            }
            return true;
        }

        public static void SetupActionSelector(System.Type controllerType,
            out IRouteBuilder routeBuilder,
            out ODataActionSelector actionSelector,
            out IReadOnlyList<ControllerActionDescriptor> actionDescriptors)
        {
            routeBuilder = RoutingConfigurationFactory.Create();
            actionDescriptors = ControllerDescriptorFactory.Create(routeBuilder, controllerType.Name, controllerType)
                as IReadOnlyList<ControllerActionDescriptor>;
            var serviceProvider = routeBuilder.ServiceProvider;
            var actionsProvider = serviceProvider.GetRequiredService<IActionDescriptorCollectionProvider>();
            var actionConstraintsProvider = serviceProvider.GetRequiredService<ActionConstraintCache>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            actionSelector = new ODataActionSelector(
                actionsProvider,
                actionConstraintsProvider,
                loggerFactory);
        }

        public static RouteContext SetupRouteContext(IRouteBuilder routeBuilder, string actionName, Dictionary<string, object> routeDataValues)
        {
            var request = RequestFactory.Create(routeBuilder);
            var routeContext = new RouteContext(request.HttpContext);
            var odataPath = new ODataPath();
            routeContext.HttpContext.ODataFeature().Path = odataPath;
            var routeData = routeContext.RouteData;
            routeData.Values[ODataRouteConstants.ODataPath] = odataPath;
            routeData.Values[ODataRouteConstants.Action] = actionName;
            foreach (var keyValuePair in routeDataValues)
            {
                routeData.Values[keyValuePair.Key] = keyValuePair.Value;
            }

            return routeContext;
        }
    }
}
