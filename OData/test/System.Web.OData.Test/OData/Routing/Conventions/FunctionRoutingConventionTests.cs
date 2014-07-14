// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.OData.TestCommon;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing.Conventions
{
    public class FunctionRoutingConventionTests
    {
        private const string _serviceRoot = "http://any/";

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
        public void SelectAction_ReturnsFunctionName_ForEntityFunctionOnEntity()
        {
            // Arrange
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "Customers(1)/NS.IsUpgraded");
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
        public void SelectAction_ReturnsFunctionName_ForSingletonFunctionOnEntity()
        {
            // Arrange
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "VipCustomer/NS.IsUpgraded");
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
            Assert.Equal(0, controllerContext.Request.GetRouteData().Values.Count);
        }

        [Fact]
        public void SelectAction_ReturnsFunctionName_ForFunctionOnEntityCollection()
        {
            // Arrange
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "Customers/NS.IsAnyUpgraded");
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
        public void SelectAction_UpdatesRouteData_ForEntityFunctionWithParameters()
        {
            // Arrange
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "Customers(1)/NS.IsUpgradedWithParam(city='any')");
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
        public void SelectAction_UpdatesRouteData_ForSingletonFunctionWithParameters()
        {
            // Arrange
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "VipCustomer/NS.IsUpgradedWithParam(city='any')");
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
            Assert.Equal(1, controllerContext.Request.GetRouteData().Values.Count);
            Assert.Equal("any", controllerContext.Request.GetRouteData().Values["city"]);
        }

        [Fact]
        public void SelectAction_ReturnsNull_IfActionIsMissing()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "Customers(1)/NS.IsUpgraded");
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

        [Fact]
        public void SelectAction_ReturnsFunctionName_DollarCount()
        {
            // Arrange
            var model = new CustomersModelWithInheritance();
            var handler = new DefaultODataPathHandler();
            ODataPath odataPath = handler.Parse(model.Model, _serviceRoot, "Customers(1)/NS.GetOrders(parameter=5)/$count");
            var requestContext = new HttpRequestContext();
            var controllerContext = new HttpControllerContext
            {
                Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/"),
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };
            controllerContext.Request.SetRequestContext(requestContext);
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => "GetOrders");

            // Act
            string selectedAction = new FunctionRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Equal("GetOrders", selectedAction);
            Assert.Equal(2, controllerContext.Request.GetRouteData().Values.Count);
            Assert.Equal("1", controllerContext.Request.GetRouteData().Values["key"]);
            Assert.Equal(5, controllerContext.Request.GetRouteData().Values["parameter"]);
        }
    }
}
