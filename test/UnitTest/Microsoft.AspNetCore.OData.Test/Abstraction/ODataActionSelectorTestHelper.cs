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
using System.IO;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;
using System.Text;
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
    /// <summary>
    /// Helper methods for working with ODataActionSelector tests
    /// </summary>
    public static class ODataActionSelectorTestHelper
    {
        /// <summary>
        /// Checks whether the specified method is a suitable match
        /// for the specified action
        /// </summary>
        /// <param name="action">Controller action to match</param>
        /// <param name="method">Candidate method to compare</param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates an action selector, descriptors and route builder based on
        /// the specified controller
        /// </summary>
        /// <param name="controllerType"></param>
        /// <param name="actionName">Action name of the action descriptors to return</param>
        /// <param name="routeBuilder"></param>
        /// <param name="actionSelector"></param>
        /// <param name="actionDescriptors"></param>
        public static void SetupActionSelector(System.Type controllerType,
           string actionName,
           out IRouteBuilder routeBuilder,
           out ODataActionSelector actionSelector,
           out IReadOnlyList<ControllerActionDescriptor> actionDescriptors)

        {
            routeBuilder = RoutingConfigurationFactory.Create();
            actionDescriptors = ControllerDescriptorFactory.Create(routeBuilder, controllerType.Name, controllerType)
                as IReadOnlyList<ControllerActionDescriptor>;
            actionDescriptors = actionDescriptors.Where(a => a.ActionName == actionName).ToList();
            var serviceProvider = routeBuilder.ServiceProvider;
#if NETCOREAPP2_0
            var actionsProvider = serviceProvider.GetRequiredService<IActionDescriptorCollectionProvider>();
            var actionConstraintsProvider = serviceProvider.GetRequiredService<ActionConstraintCache>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var modelBinderFactory = serviceProvider.GetRequiredService<IModelBinderFactory>();
            var modelMetadataProvider = serviceProvider.GetRequiredService<IModelMetadataProvider>();
            actionSelector = new ODataActionSelector(
                actionsProvider,
                actionConstraintsProvider,
                loggerFactory,
                modelBinderFactory,
                modelMetadataProvider);
        }
#else
        
            var innerActionSelectorMock = new Mock<IActionSelector>();
            var modelBinderFactory = (IModelBinderFactory)serviceProvider.GetService(typeof(IModelBinderFactory));
            var modelMetadataProvider = (IModelMetadataProvider)serviceProvider.GetService(typeof(IModelMetadataProvider));
            actionSelector = new ODataActionSelector(innerActionSelectorMock.Object, modelBinderFactory, modelMetadataProvider);
        }
#endif

        /// <summary>
        /// Create route context with the specified with the specified request data
        /// </summary>
        /// <param name="routeBuilder"></param>
        /// <param name="actionName">Name of the action being routed to</param>
        /// <param name="routeDataValues">Key-value pairs to add to the route data</param>
        /// <param name="method">HTTP request method</param>
        /// <param name="bodyContent">Request body content</param>
        /// <returns></returns>
        public static RouteContext SetupRouteContext(IRouteBuilder routeBuilder, string actionName,
            Dictionary<string, object> routeDataValues, string method, string bodyContent)
        {
            var request = RequestFactory.Create(routeBuilder);
            var routeContext = new RouteContext(request.HttpContext);
            var routeData = routeContext.RouteData;
            var routingConventionsStore = routeContext.HttpContext.ODataFeature().RoutingConventionsStore;
            var odataPath = new ODataPath();
            routeContext.HttpContext.ODataFeature().Path = odataPath;
            
            routeData.Values[ODataRouteConstants.ODataPath] = odataPath;
            routeData.Values[ODataRouteConstants.Action] = actionName;
            int keyCount = 0;
            foreach (var keyValuePair in routeDataValues)
            {
                routeData.Values[keyValuePair.Key] = keyValuePair.Value;
                keyCount++;
            }

            routingConventionsStore[ODataRouteConstants.KeyCount] = keyCount;

            request.Method = method;

            if (bodyContent != null)
            {
                request.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyContent));
                request.ContentLength = bodyContent.Length;
            }

            return routeContext;
        }
    }
}
