// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Abstractions;
#if NETCOREAPP2_0
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.OData;
    using Microsoft.AspNetCore.Mvc.Internal;
#else
    using Moq;
#endif


namespace Microsoft.AspNet.OData.Test.Abstraction
{
    internal class ODataActionSelectorTestHelper
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

#if NETCOREAPP2_0
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
#else
        public static void SetupActionSelector(System.Type controllerType,
            out IRouteBuilder routeBuilder,
            out ODataActionSelector actionSelector,
            out IReadOnlyList<ControllerActionDescriptor> actionDescriptors)
        {
            var innerActionSelectorMock = new Mock<IActionSelector>();
            actionSelector = new ODataActionSelector(innerActionSelectorMock.Object);
            routeBuilder = RoutingConfigurationFactory.Create();
            actionDescriptors = ControllerDescriptorFactory.Create(routeBuilder, controllerType.Name, controllerType)
                as IReadOnlyList<ControllerActionDescriptor>;
        }
#endif

        public static RouteContext SetupRouteContext(IRouteBuilder routeBuilder, string actionName, Dictionary<string, object> routeDataValues, string bodyContent)
        {
            var request = RequestFactory.Create(routeBuilder);
            var routeContext = new RouteContext(request.HttpContext);
            var routeData = routeContext.RouteData;
            var odataPath = new ODataPath();
            routeContext.HttpContext.ODataFeature().Path = odataPath;
            
            routeData.Values[ODataRouteConstants.ODataPath] = odataPath;
            routeData.Values[ODataRouteConstants.Action] = actionName;
            foreach (var keyValuePair in routeDataValues)
            {
                routeData.Values[keyValuePair.Key] = keyValuePair.Value;
            }

            if (bodyContent != null)
            {
                request.ContentLength = bodyContent.Length;
            }

            return routeContext;
        }
    }
}
