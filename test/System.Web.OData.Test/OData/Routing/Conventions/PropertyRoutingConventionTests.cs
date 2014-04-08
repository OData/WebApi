// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.OData.TestCommon;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing.Conventions
{
    public class PropertyRoutingConventionTests
    {
        private const string _serviceRoot = "http://any/";

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissOdataPath()
        {
            // Arrange
            Mock<HttpControllerContext> controllerContext = new Mock<HttpControllerContext>();
            ILookup<string, HttpActionDescriptor> emptyMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new PropertyRoutingConvention().SelectAction(null, controllerContext.Object, emptyMap),
                "odataPath");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissControllerContext()
        {
            // Arrange
            ODataPath odataPath = new ODataPath();
            ILookup<string, HttpActionDescriptor> emptyMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new PropertyRoutingConvention().SelectAction(odataPath, null, emptyMap),
                "controllerContext");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissActionMap()
        {
            // Arrange
            ODataPath odataPath = new ODataPath();
            Mock<HttpControllerContext> controllerContext = new Mock<HttpControllerContext>();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new PropertyRoutingConvention().SelectAction(odataPath, controllerContext.Object, null),
                "actionMap");
        }

        [Fact]
        public void SelectAction_OnEntitySetPath_ReturnsTheActionName()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "Customers(7)/Name");
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => "GetNameFromCustomer");
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/"),
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };
            controllerContext.Request.SetRequestContext(requestContext);

            // Act
            string selectedAction = new PropertyRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal("GetNameFromCustomer", selectedAction);
            Assert.Equal(1, controllerContext.Request.GetRouteData().Values.Count);
            Assert.Equal("7", controllerContext.Request.GetRouteData().Values["key"]);
        }

        [Fact]
        public void SelectAction_OnSingletonPath_ReturnsTheActionName()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "VipCustomer/Address");
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => "GetAddress");
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/"),
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };
            controllerContext.Request.SetRequestContext(requestContext);

            // Act
            string selectedAction = new PropertyRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal("GetAddress", selectedAction);
            Assert.Empty(controllerContext.Request.GetRouteData().Values);
        }

        [Fact]
        public void SelectAction_ReturnsNull_IfActionIsMissing()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "Customers(10)/Name");
            ILookup<string, HttpActionDescriptor> emptyActionMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/"),
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };
            controllerContext.Request.SetRequestContext(requestContext);

            // Act
            string selectedAction = new PropertyRoutingConvention().SelectAction(odataPath, controllerContext, emptyActionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(controllerContext.Request.GetRouteData().Values);
        }
    }
}
