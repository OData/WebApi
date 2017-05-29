// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.OData.TestCommon;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing.Conventions
{
    public class DynamicPropertyRoutingConventionTest
    {
        private DynamicPropertyRoutingConvention _routingConvention = new DynamicPropertyRoutingConvention();

        #region Negative Cases
        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissOdataPath()
        {
            // Arrange
            Mock<HttpControllerContext> controllerContext = new Mock<HttpControllerContext>();
            ILookup<string, HttpActionDescriptor> emptyMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => _routingConvention.SelectAction(null, controllerContext.Object, emptyMap),
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
                () => _routingConvention.SelectAction(odataPath, null, emptyMap),
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
                () => _routingConvention.SelectAction(odataPath, controllerContext.Object, null),
                "actionMap");
        }

        [Fact]
        public void SelectAction_ReturnsNull_IfActionIsMissing()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://localhost/", "Customers(10)/Account/Tax");
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
            string selectedAction = _routingConvention.SelectAction(odataPath, controllerContext, emptyActionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(controllerContext.Request.GetRouteData().Values);
        }

        [Theory]
        [InlineData("Post")]
        [InlineData("Patch")]
        [InlineData("Put")]
        [InlineData("Delete")]
        public void SelectAction_ReturnsNull_IfNotCorrectMethod(string methodName)
        {
            HttpMethod method = new HttpMethod(methodName);
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://localhost/", "Orders(7)/DynamicPropertyA");
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => "GetDynamicProperty");
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                Request = new HttpRequestMessage(method, "http://localhost/"),
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };
            controllerContext.Request.SetRequestContext(requestContext);

            // Act
            string selectedAction = _routingConvention.SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(controllerContext.Request.GetRouteData().Values);
        }
        #endregion

        #region Cases for Open Complex Type
        [Theory]
        [InlineData("Customers(7)/Account/Amount")]
        [InlineData("Customers(7)/NS.SpecialCustomer/Account/Amount")]
        public void SelectAction_OnEntitySetPath_OpenComplexType_ReturnsTheActionName(string url)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://localhost/", url);
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => "GetDynamicPropertyFromAccount");
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/"),
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };
            controllerContext.Request.SetRequestContext(requestContext);

            // Act
            string selectedAction = _routingConvention.SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal("GetDynamicPropertyFromAccount", selectedAction);
            var test = controllerContext.Request.GetRouteData();
            Assert.Equal(3, controllerContext.Request.GetRouteData().Values.Count);
            Assert.Equal("7", controllerContext.Request.GetRouteData().Values["key"]);
            Assert.Equal("Amount", controllerContext.Request.GetRouteData().Values["dynamicProperty"]);
            Assert.Equal("Amount", (controllerContext.Request.GetRouteData().Values[ODataParameterValue.ParameterValuePrefix + "dynamicProperty"] as ODataParameterValue).Value);
        }

        [Theory]
        [InlineData("VipCustomer/Account/Amount")]
        [InlineData("VipCustomer/NS.SpecialCustomer/Account/Amount")]
        public void SelectAction_OnSingletonPath_OpenComplexType_ReturnsTheActionName(string url)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://localhost/", url);
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => "GetDynamicPropertyFromAccount");
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/"),
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };
            controllerContext.Request.SetRequestContext(requestContext);

            // Act
            string selectedAction = _routingConvention.SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal("GetDynamicPropertyFromAccount", selectedAction);
            Assert.Equal(2, controllerContext.Request.GetRouteData().Values.Count);
            Assert.Equal("Amount", controllerContext.Request.GetRouteData().Values["dynamicProperty"]);
            Assert.Equal("Amount", (controllerContext.Request.GetRouteData().Values[ODataParameterValue.ParameterValuePrefix + "dynamicProperty"] as ODataParameterValue).Value);
        }
        #endregion

        #region Cases for Open Entity Type
        [Theory]
        [InlineData("Orders(7)/DynamicPropertyA")]
        [InlineData("Orders(7)/NS.SpecialOrder/DynamicPropertyA")]
        public void SelectAction_OnEntitySetPath_OpenEntityType_ReturnsTheActionName(string url)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://localhost/", url);
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => "GetDynamicProperty");
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/"),
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };

            controllerContext.Request.SetRequestContext(requestContext);

            // Act
            string selectedAction = _routingConvention.SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal("GetDynamicProperty", selectedAction);
            Assert.Equal(3, controllerContext.Request.GetRouteData().Values.Count);
            Assert.Equal("7", controllerContext.Request.GetRouteData().Values["key"]);
            Assert.Equal("DynamicPropertyA", controllerContext.Request.GetRouteData().Values["dynamicProperty"]);
            Assert.Equal("DynamicPropertyA", (controllerContext.Request.GetRouteData().Values[ODataParameterValue.ParameterValuePrefix + "dynamicProperty"] as ODataParameterValue).Value);
        }

        [Theory]
        [InlineData("RootOrder/DynamicPropertyA")]
        [InlineData("RootOrder/NS.SpecialOrder/DynamicPropertyA")]
        public void SelectAction_OnSingltonPath_OpenEntityType_ReturnsTheActionName(string url)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://localhost/", url);
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => "GetDynamicProperty");
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/"),
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };

            controllerContext.Request.SetRequestContext(requestContext);

            // Act
            string selectedAction = _routingConvention.SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal("GetDynamicProperty", selectedAction);
            Assert.Equal(2, controllerContext.Request.GetRouteData().Values.Count);
            Assert.Equal("DynamicPropertyA", controllerContext.Request.GetRouteData().Values["dynamicProperty"]);
            Assert.Equal("DynamicPropertyA", (controllerContext.Request.GetRouteData().Values[ODataParameterValue.ParameterValuePrefix + "dynamicProperty"] as ODataParameterValue).Value);
        }
        #endregion
    }
}