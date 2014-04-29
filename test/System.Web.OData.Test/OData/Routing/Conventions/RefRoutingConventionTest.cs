// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing.Conventions
{
    public class RefRoutingConventionTest
    {
        [Theory]
        [InlineData("DELETE", new string[] { }, null)]
        [InlineData("DELETE", new[] { "UnrelatedAction" }, null)]
        [InlineData("DELETE", new[] { "DeleteRefToOrders" }, "DeleteRefToOrders")]
        [InlineData("DELETE", new[] { "DeleteRefToOrders", "DeleteRef" }, "DeleteRefToOrders")]
        [InlineData("DELETE", new[] { "DeleteRefToOrdersFromCustomer", "DeleteRefToOrders" }, "DeleteRefToOrdersFromCustomer")]
        [InlineData("POST", new string[] { }, null)]
        [InlineData("POST", new[] { "UnrelatedAction" }, null)]
        [InlineData("POST", new[] { "CreateRefToOrders" }, "CreateRefToOrders")]
        [InlineData("POST", new[] { "CreateRefToOrders", "CreateRef" }, "CreateRefToOrders")]
        [InlineData("POST", new[] { "CreateRefToOrders", "CreateRefToOrdersFromCustomer" }, "CreateRefToOrdersFromCustomer")]
        [InlineData("GET", new string[] { }, null)]
        [InlineData("GET", new[] { "UnrelatedAction" }, null)]
        [InlineData("GET", new[] { "GetRefToOrders" }, "GetRefToOrders")]
        [InlineData("GET", new[] { "GetRefToOrders", "GetRef" }, "GetRefToOrders")]
        [InlineData("GET", new[] { "GetRefToOrders", "GetRefToOrdersFromCustomer" }, "GetRefToOrdersFromCustomer")]
        public void SelectAction_Returns_ExpectedMethodOnBaseType(string method, string[] methodsInController,
            string expectedSelectedAction)
        {
            // Arrange
            string key = "42";
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var ordersProperty = model.Customer.FindProperty("Orders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(new EntitySetPathSegment(model.Customers), new KeyValuePathSegment(key),
                new NavigationPathSegment(ordersProperty), new RefPathSegment());

            HttpControllerContext controllerContext = CreateControllerContext(method);
            var actionMap = GetMockActionMap(methodsInController);

            // Act
            string selectedAction = new RefRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Equal(expectedSelectedAction, selectedAction);
            if (expectedSelectedAction == null)
            {
                Assert.Empty(controllerContext.RouteData.Values);
            }
            else
            {
                Assert.Equal(2, controllerContext.RouteData.Values.Count);
                Assert.Equal(key, controllerContext.RouteData.Values["key"]);
                Assert.Equal(ordersProperty.Name, controllerContext.RouteData.Values["navigationProperty"]);
            }
        }

        [Theory]
        [InlineData("DELETE", new string[] { }, null)]
        [InlineData("DELETE", new[] { "UnrelatedAction" }, null)]
        [InlineData("DELETE", new[] { "DeleteRefToSpecialOrders" }, "DeleteRefToSpecialOrders")]
        [InlineData("DELETE", new[] { "DeleteRefToSpecialOrders", "DeleteRefToOrders" }, "DeleteRefToSpecialOrders")]
        [InlineData("DELETE", new[] { "DeleteRefToSpecialOrders", "DeleteRefToSpecialOrdersFromSpecialCustomer" }, "DeleteRefToSpecialOrdersFromSpecialCustomer")]
        [InlineData("POST", new string[] { }, null)]
        [InlineData("POST", new[] { "UnrelatedAction" }, null)]
        [InlineData("POST", new[] { "CreateRefToSpecialOrders" }, "CreateRefToSpecialOrders")]
        [InlineData("POST", new[] { "CreateRefToSpecialOrders", "CreateRefToOrders" }, "CreateRefToSpecialOrders")]
        [InlineData("POST", new[] { "CreateRefToSpecialOrders", "CreateRefToSpecialOrdersFromSpecialCustomer" }, "CreateRefToSpecialOrdersFromSpecialCustomer")]
        [InlineData("GET", new string[] { }, null)]
        [InlineData("GET", new[] { "UnrelatedAction" }, null)]
        [InlineData("GET", new[] { "GetRefToSpecialOrders" }, "GetRefToSpecialOrders")]
        [InlineData("GET", new[] { "GetRefToSpecialOrders", "GetRefToOrders" }, "GetRefToSpecialOrders")]
        [InlineData("GET", new[] { "GetRefToSpecialOrders", "GetRefToSpecialOrdersFromSpecialCustomer" }, "GetRefToSpecialOrdersFromSpecialCustomer")]
        public void SelectAction_Returns_ExpectedMethodOnDerivedType(string method, string[] methodsInController,
            string expectedSelectedAction)
        {
            // Arrange
            string key = "42";
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var specialOrdersProperty = model.SpecialCustomer.FindProperty("SpecialOrders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(
                new EntitySetPathSegment(model.Customers),
                new KeyValuePathSegment(key),
                new CastPathSegment(model.SpecialCustomer),
                new NavigationPathSegment(specialOrdersProperty),
                new RefPathSegment());

            HttpControllerContext controllerContext = CreateControllerContext(method);
            var actionMap = GetMockActionMap(methodsInController);

            // Act
            string selectedAction = new RefRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Equal(expectedSelectedAction, selectedAction);
            if (expectedSelectedAction == null)
            {
                Assert.Empty(controllerContext.RouteData.Values);
            }
            else
            {
                Assert.Equal(2, controllerContext.RouteData.Values.Count);
                Assert.Equal(key, controllerContext.RouteData.Values["key"]);
                Assert.Equal(specialOrdersProperty.Name, controllerContext.RouteData.Values["navigationProperty"]);
            }
        }

        [Fact]
        public void SelectAction_SetsRelatedKey_ForDeleteRefRequests()
        {
            // Arrange
            string key = "42";
            string relatedKey = "24";
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var specialOrdersProperty = model.SpecialCustomer.FindProperty("SpecialOrders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(
                new EntitySetPathSegment(model.Customers),
                new KeyValuePathSegment(key),
                new CastPathSegment(model.SpecialCustomer),
                new NavigationPathSegment(specialOrdersProperty),
                new KeyValuePathSegment(relatedKey),
                new RefPathSegment());

            HttpControllerContext controllerContext = CreateControllerContext("DELETE");
            var actionMap = GetMockActionMap("DeleteRef");

            // Act
            new RefRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);
            var routeData = controllerContext.RouteData;

            // Assert
            Assert.Equal(key, routeData.Values["key"]);
            Assert.Equal("SpecialOrders", routeData.Values["navigationProperty"]);
            Assert.Equal(relatedKey, routeData.Values["relatedKey"]);
        }

        [Theory]
        [InlineData("http://any/Customers(42)/Orders/$ref?$id=http://any/Orders(24)")]
        [InlineData("http://any/Customers(42)/Orders/$ref?$id=../../Orders(24)")]
        public void SelectAction_SetsRelatedKey_ForDeleteRefRequestsWithDollarId(string uri)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://any/", uri);
            HttpControllerContext controllerContext = CreateControllerContext("DELETE");
            var actionMap = GetMockActionMap("DeleteRef");

            // Act
            var actionName = new RefRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);
            var routeData = controllerContext.RouteData;

            // Assert
            Assert.Equal("DeleteRef", actionName);
            Assert.Equal("42", routeData.Values["key"]);
            Assert.Equal("Orders", routeData.Values["navigationProperty"]);
            Assert.Equal("24", routeData.Values["relatedKey"]);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("PUT")]
        [InlineData("POST")]
        public void SelectAction_ReturnsNull_ForNonDeleteRequestWithDollarId(string method)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();

            ODataPath odataPath = new DefaultODataPathHandler().Parse(
                model.Model,
                "http://any/",
                "http://any/Customers(42)/Orders/$ref?$id=http://any/Orders(24)");

            HttpControllerContext controllerContext = CreateControllerContext(method);
            var actionMap = GetMockActionMap("DeleteRef", "CreateRef", "GetRef", "PutRef", "PostRef");

            // Act
            string actionName = new RefRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Null(actionName);
        }

        [InlineData("POST", "CreateRef")]
        [InlineData("GET", "GetRef")]
        public void SelectAction_SetsRouteData_ForGetOrCreateRefRequests(string method, string actionName)
        {
            // Arrange
            string key = "42";
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var specialOrdersProperty = model.SpecialCustomer.FindProperty("SpecialOrders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(
                new EntitySetPathSegment(model.Customers),
                new KeyValuePathSegment(key),
                new CastPathSegment(model.SpecialCustomer),
                new NavigationPathSegment(specialOrdersProperty),
                new RefPathSegment());

            HttpControllerContext controllerContext = CreateControllerContext(method);
            var actionMap = new[] { GetMockActionDescriptor(actionName) }.ToLookup(a => a.ActionName);

            // Act
            new RefRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);
            var routeData = controllerContext.RouteData;

            // Assert
            Assert.Equal(key, routeData.Values["key"]);
            Assert.Equal("SpecialOrders", routeData.Values["navigationProperty"]);
        }

        private static HttpControllerContext CreateControllerContext(string method)
        {
            HttpControllerContext controllerContext = new HttpControllerContext();
            controllerContext.Request = new HttpRequestMessage(new HttpMethod(method), "http://localhost/");
            controllerContext.RouteData = new HttpRouteData(new HttpRoute());
            return controllerContext;
        }

        private static HttpActionDescriptor GetMockActionDescriptor(string name)
        {
            Mock<HttpActionDescriptor> actionDescriptor = new Mock<HttpActionDescriptor> { CallBase = true };
            actionDescriptor.Setup(a => a.ActionName).Returns(name);
            return actionDescriptor.Object;
        }

        private static ILookup<string, HttpActionDescriptor> GetMockActionMap(params string[] actionNames)
        {
            return actionNames.Select(name => GetMockActionDescriptor(name)).ToLookup(a => a.ActionName);
        }
    }
}
