// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Collections.Generic;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
#else
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Moq;
#endif

namespace Microsoft.AspNet.OData.Test.Routing.Conventions
{
    /// <summary>
    /// Helper to create parameters for NavigationSourceRoutingConvention SelectAction().
    /// </summary>
    public static class SelectActionHelper
    {
#if NETCORE
        internal static IEnumerable<ControllerActionDescriptor> CreateActionMap()
        {
            List<ControllerActionDescriptor> actionMap = new List<ControllerActionDescriptor>();
            ControllerActionDescriptor descriptor = new ControllerActionDescriptor();
            actionMap.Add(descriptor);
            return actionMap;
        }

        internal static IEnumerable<ControllerActionDescriptor> CreateActionMap(params string[] actionNames)
        {
            List<ControllerActionDescriptor> actionMap = new List<ControllerActionDescriptor>();
            foreach (string actionName in actionNames)
            {
                ControllerActionDescriptor descriptor = new ControllerActionDescriptor();
                descriptor.ActionName = actionName;
                actionMap.Add(descriptor);
            }

            return actionMap;
        }

        internal static string SelectAction(NavigationSourceRoutingConvention convention, ODataPath odataPath, HttpRequest request, IEnumerable<ControllerActionDescriptor> actionMap, string controllerName = "ControllerName")
        {
            // COnstruct parameters.
            RouteContext routeContext = new RouteContext(request.HttpContext);
            routeContext.HttpContext.ODataFeature().Path = odataPath;

            SelectControllerResult controllerResult = new SelectControllerResult(controllerName, null);

            // Select the action.
            string result = convention.SelectAction(routeContext, controllerResult, actionMap);

            // Copy route data to the context. In the real pipeline, this occurs in
            // RouterMiddleware.cs after the request has been routed.
            request.HttpContext.Features[typeof(IRoutingFeature)] = new RoutingFeature()
            {
                RouteData = routeContext.RouteData,
            };

            return result;
        }

        internal static RouteData GetRouteData(HttpRequest request)
        {
            // Get route data from the context. In the real pipeline, RouterMiddleware.cs 
            // copied this from the route context after the request has been routed.
            return request.HttpContext.GetRouteData();
        }


        internal static IDictionary<string, object> GetRoutingConventionsStore(HttpRequest request)
        {
            return request.HttpContext.ODataFeature().RoutingConventionsStore;
        }
#else
        internal static ILookup<string, HttpActionDescriptor> CreateActionMap()
        {
            return new HttpActionDescriptor[0].ToLookup(desc => (string)null);
        }

        internal static ILookup<string, HttpActionDescriptor> CreateActionMap(params string[] actionNames)
        {
            List<HttpActionDescriptor> actionMap = new List<HttpActionDescriptor>();
            foreach (string actionName in actionNames)
            {
                Mock<HttpActionDescriptor> actionDescriptor = new Mock<HttpActionDescriptor> { CallBase = true };
                actionDescriptor.Setup(a => a.ActionName).Returns(actionName);
                actionMap.Add(actionDescriptor.Object);
            }

            return actionMap.ToLookup(a => a.ActionName);
        }

        internal static string SelectAction(NavigationSourceRoutingConvention convention, ODataPath odataPath, HttpRequestMessage request, ILookup<string, HttpActionDescriptor> actionMap, string controllerName = "ControllerName")
        {
            // Construct parameters.
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                Request = request,
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };
            controllerContext.Request.SetRequestContext(requestContext);

            // Select the action.
            string result = convention.SelectAction(odataPath, controllerContext, actionMap);
            return result;
        }

        internal static IHttpRouteData GetRouteData(HttpRequestMessage request)
        {
            return request.GetRouteData();
        }

        internal static IDictionary<string, object> GetRoutingConventionsStore(HttpRequestMessage request)
        {
            return request.ODataProperties().RoutingConventionsStore;
        }
#endif
    }
}
