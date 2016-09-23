// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Test.Models;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    public class ActionRoutingConventionTest
    {
        private const string _serviceRoot = "http://any/";

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfRouteContextIsNull()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("routeContext",
                () => new ActionRoutingConvention().SelectAction(routeContext: null, actionDescriptors: null));
        }

        [Theory]
        [InlineData("Get")]
        [InlineData("Put")]
        [InlineData("Delete")]
        [InlineData("Patch")]
        public void SelectAction_ReturnsNull_RequestMethodIsNotPost(string requestMethod)
        {
            // Arrange
            HttpContext context = CreateHttpContext(requestMethod);
            HttpRequest request = context.Request;
            request.ODataFeature().Path = new ODataPath();

            RouteContext routeContext = new RouteContext(context);
            IEnumerable<ControllerActionDescriptor> actionDescriptors = Enumerable.Empty<ControllerActionDescriptor>();

            // Act
            ActionDescriptor actionDescriptor = new ActionRoutingConvention().SelectAction(routeContext, actionDescriptors);

            // Assert
            Assert.Null(actionDescriptor);
        }

        [Fact]
        public void SelectAction_ReturnsTheActionDescriptor_ForEntitySetActionBoundToEntitySet()
        {
            // Arrange
            ActionRoutingConvention actionConvention = new ActionRoutingConvention();

            HttpContext context = CreateHttpContext("Post");
            HttpRequest request = context.Request;

            IEdmModel model = ODataRoutingModel.GetModel(null);
            IODataPathHandler pathHandler = context.RequestServices.GetRequiredService<IODataPathHandler>();
            ODataPath odataPath = pathHandler.Parse(model, _serviceRoot, "RoutingCustomers/Default.GetVIPs");
            context.ODataFeature().Path = odataPath;

            ControllerActionDescriptor descriptor = new ControllerActionDescriptor();
            descriptor.ControllerName = "RoutingCustomers";
            descriptor.ActionName = "GetVIPs";

            RouteContext routeContext = new RouteContext(context);
            IEnumerable<ControllerActionDescriptor> actionDescriptors = new [] { descriptor };

            // Act
            ActionDescriptor actionDescriptor = actionConvention.SelectAction(routeContext, actionDescriptors);

            // Assert
            Assert.Same(descriptor, actionDescriptor);
            Assert.Empty(routeContext.RouteData.Values);
        }

        [Fact]
        public void SelectAction_ReturnsTheActionName_ForSingletonActionBoundToEntity()
        {
            // Arrange
            ActionRoutingConvention actionConvention = new ActionRoutingConvention();
            HttpContext context = CreateHttpContext("Post");

            IEdmModel model = new CustomersModelWithInheritance().Model;
            //IODataPathHandler pathHandler = context.RequestServices.GetRequiredService<IODataPathHandler>();
            IODataPathHandler pathHandler = new DefaultODataPathHandler(context.RequestServices);
            ODataPath odataPath = pathHandler.Parse(model, _serviceRoot, "VipCustomer/NS.upgrade");
            context.ODataFeature().Path = odataPath;

            ControllerActionDescriptor descriptor = new ControllerActionDescriptor();
            descriptor.ControllerName = "VipCustomer";
            descriptor.ActionName = "upgrade";

            RouteContext routeContext = new RouteContext(context);
            IEnumerable<ControllerActionDescriptor> actionDescriptors = new[] { descriptor };

            // Act
            ActionDescriptor actionDescriptor = actionConvention.SelectAction(routeContext, actionDescriptors);

            // Assert
            Assert.Same(descriptor, actionDescriptor);
            Assert.Empty(routeContext.RouteData.Values);
        }

        [Fact]
        public void SelectAction_ReturnsNull_IfActionIsMissing()
        {
            string[] paths =
            {
                "RoutingCustomers(1)/Default.GetRelatedRoutingCustomers",
                "RoutingCustomers/Default.GetProducts"
            };

            foreach (string path in paths)
            {
                // Arrange
                HttpContext context = CreateHttpContext("Post");

                IEdmModel model = ODataRoutingModel.GetModel(null);
                IODataPathHandler pathHandler = context.RequestServices.GetRequiredService<IODataPathHandler>();
                ODataPath odataPath = pathHandler.Parse(model, _serviceRoot, path);
                context.ODataFeature().Path = odataPath;

                RouteContext routeContext = new RouteContext(context);
                IEnumerable<ControllerActionDescriptor> actionDescriptors = Enumerable.Empty<ControllerActionDescriptor>();

                // Act
                ActionDescriptor selectedAction = new ActionRoutingConvention().SelectAction(routeContext, actionDescriptors);

                // Assert
                Assert.Null(selectedAction);
                Assert.Empty(routeContext.RouteData.Values);
            }
        }

        private static HttpContext CreateHttpContext(string requestMethod)
        {
            HttpContext context = new DefaultHttpContext();

            IServiceCollection services = new ServiceCollection();
            services.AddOptions();
            services.AddOData();
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            context.RequestServices = serviceProvider;
            context.Features.Get<IHttpRequestFeature>().Method = requestMethod;

            return context;
        }
    }
}
