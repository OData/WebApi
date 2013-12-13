// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.Http.TestCommon;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Routing.Conventions
{
    public class FunctionRoutingConventionTests
    {
        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfODataPathIsNull()
        {
            // Arrange
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => functionConvention.SelectAction(odataPath: null, controllerContext: null, actionMap: null),
                "odataPath");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfControllerContextIsNull()
        {
            // Arrange
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();
            ODataPath odataPath = new ODataPath();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => functionConvention.SelectAction(odataPath, controllerContext: null, actionMap: null),
                "controllerContext");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfActionMapIsNull()
        {
            // Arrange
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();
            ODataPath odataPath = new ODataPath();
            HttpControllerContext controllerContext = new HttpControllerContext();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => functionConvention.SelectAction(odataPath, controllerContext, actionMap: null),
                "actionMap");
        }

        [Theory]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("MERGE")]
        [InlineData("PATCH")]
        public void SelectAction_ReturnsNull_RequestMethodIsNotGet(string requestMethod)
        {
            // Arrange
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();
            ODataPath odataPath = new ODataPath();
            HttpControllerContext controllerContext = new HttpControllerContext();
            controllerContext.Request = new HttpRequestMessage(new HttpMethod(requestMethod), "http://localhost/");
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);

            // Act
            string selectedAction = functionConvention.SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Null(selectedAction);
        }

        [Fact]
        public void SelectAction_ReturnsFunctionName_ForFunctionOnEntity()
        {
            // Arrange
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "Customers(1)/IsUpgraded");
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
                {
                    Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/"),
                    RequestContext = requestContext,
                    RouteData = new HttpRouteData(new HttpRoute())
                };
            controllerContext.Request.SetRequestContext(requestContext);
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => "IsUpgraded");

            // Act
            string function = functionConvention.SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Equal("IsUpgraded", function);
            Assert.Equal(1, controllerContext.Request.GetRouteData().Values.Count);
            Assert.Equal("1", controllerContext.Request.GetRouteData().Values["key"]);
        }

        [Fact]
        public void SelectAction_ReturnsFunctionName_ForFunctionOnEntityCollection()
        {
            // Arrange
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "Customers/IsAnyUpgraded");
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
                {
                    Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/"),
                    RequestContext = requestContext,
                    RouteData = new HttpRouteData(new HttpRoute())
                };
            controllerContext.Request.SetRequestContext(requestContext);
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => "IsAnyUpgraded");

            // Act
            string function = functionConvention.SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Equal("IsAnyUpgraded", function);
            Assert.Empty(controllerContext.Request.GetRouteData().Values);
        }

        [Fact]
        public void SelectAction_UpdatesRouteData_ForFunctionWithParameters()
        {
            // Arrange
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "Customers(1)/IsUpgradedWithParam(city='any')");
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
                {
                    Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/"),
                    RequestContext = requestContext,
                    RouteData = new HttpRouteData(new HttpRoute())
                };
            controllerContext.Request.SetRequestContext(requestContext);
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => "IsUpgradedWithParam");

            // Act
            string function = functionConvention.SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Equal("IsUpgradedWithParam", function);
            Assert.Equal(2, controllerContext.Request.GetRouteData().Values.Count);
            Assert.Equal("1", controllerContext.Request.GetRouteData().Values["key"]);
            Assert.Equal("any", controllerContext.Request.GetRouteData().Values["city"]);
        }

        [Fact]
        public void SelectAction_ReturnsNull_IfActionIsMissing()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "Customers(1)/IsLocal");
            ILookup<string, HttpActionDescriptor> emptyActionMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);
            HttpControllerContext controllerContext = new HttpControllerContext
                {
                    Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/")
                };
            controllerContext.Request.SetRouteData(new HttpRouteData(new HttpRoute()));

            // Act
            string selectedAction = new FunctionRoutingConvention().SelectAction(odataPath, controllerContext, emptyActionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(controllerContext.Request.GetRouteData().Values);
        }
    }
}
