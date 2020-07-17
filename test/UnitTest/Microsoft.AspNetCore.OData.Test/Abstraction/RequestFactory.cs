// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// A class to create HttpRequest[Message].
    /// </summary>
    public class RequestFactory
    {
        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static HttpRequest Create(IRouteBuilder routeBuilder = null, string routeName = null)
        {
            // Add the options services.
            string useRouteName = (routeName == null) ? "OData" : routeName;
            if (routeBuilder == null)
            {
                routeBuilder = RoutingConfigurationFactory.CreateWithRootContainer(useRouteName);
            }

            // Create a new context and assign the services.
            HttpContext context = new DefaultHttpContext();
            context.RequestServices = routeBuilder.ServiceProvider;

            // Ensure there is route data for the routing tests.
            var routeContext = new RouteContext(context);
            context.Features[typeof(IRoutingFeature)] = new RoutingFeature()
            {
                RouteData = routeContext.RouteData,
            };

            // Assign the route and get the request container, which will initialize
            // the request container if one does not exists.
            context.Request.ODataFeature().RouteName = useRouteName;
            IPerRouteContainer perRouteContainer = routeBuilder.ServiceProvider.GetRequiredService<IPerRouteContainer>();
            if (!perRouteContainer.HasODataRootContainer(useRouteName))
            {
                Action<IContainerBuilder> builderAction = ODataRouteBuilderExtensions.ConfigureDefaultServices(routeBuilder, null);
                IServiceProvider serviceProvider = perRouteContainer.CreateODataRootContainer(useRouteName, builderAction);
            }

            // Add some routing info
            IRouter defaultRoute = routeBuilder.Routes.FirstOrDefault();
            RouteData routeData = new RouteData();
            if (defaultRoute != null)
            {
                routeData.Routers.Add(defaultRoute);
            }
            else
            {
                var resolver = routeBuilder.ServiceProvider.GetRequiredService<IInlineConstraintResolver>();
                routeData.Routers.Add(new ODataRoute(routeBuilder.DefaultHandler, useRouteName, null, new ODataPathRouteConstraint(useRouteName), resolver));
            }

            var mockAction = new Mock<ActionDescriptor>();
            ActionDescriptor actionDescriptor = mockAction.Object;

            ActionContext actionContext = new ActionContext(context, routeData, actionDescriptor);

            IActionContextAccessor actionContextAccessor = context.RequestServices.GetRequiredService<IActionContextAccessor>();
            actionContextAccessor.ActionContext = actionContext;

            // Get request and return it.
            return context.Request;
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static HttpRequest Create(HttpMethod method, string uri, IRouteBuilder routeBuilder = null, string routeName = null, ODataPath path = null)
        {
            HttpRequest request = Create(routeBuilder, routeName);
            request.Method = method.ToString();

            Uri requestUri = new Uri(uri);
            request.Scheme = requestUri.Scheme;
            request.Host = requestUri.IsDefaultPort ?
                new HostString(requestUri.Host) :
                new HostString(requestUri.Host, requestUri.Port);
            request.QueryString = new QueryString(requestUri.Query);
            request.Path = new PathString(requestUri.AbsolutePath);

            if (path != null)
            {
                request.ODataFeature().Path = path;
            }

            return request;
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static HttpRequest CreateFromModel(IEdmModel model, string uri = "http://localhost", string routeName = "Route", ODataPath path = null)
        {
            var configuration = RoutingConfigurationFactory.CreateWithRootContainer(routeName);
            configuration.MapODataServiceRoute(routeName, null, model);

            var request = RequestFactory.Create(HttpMethod.Get, uri, configuration, routeName);

            if (path != null)
            {
                request.ODataFeature().Path = path;
            }

            return request;
        }
    }
}
