// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.Http.TestCommon;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Routing.Conventions
{
    public class NavigationRoutingConventionTest
    {
        [Theory]
        [InlineData("GET", new string[] { }, null)]
        [InlineData("GET", new[] { "UnrelatedAction" }, null)]
        [InlineData("GET", new[] { "GetOrders" }, "GetOrders")]
        [InlineData("GET", new[] { "GetOrders", "Get" }, "GetOrders")]
        [InlineData("GET", new[] { "GetOrders", "GetOrdersFromCustomer" }, "GetOrdersFromCustomer")]
        [InlineData("POST", new string[] { }, null)]
        [InlineData("POST", new[] { "UnrelatedAction" }, null)]
        [InlineData("POST", new[] { "PostToOrders" }, "PostToOrders")]
        [InlineData("POST", new[] { "PostToOrders", "Post" }, "PostToOrders")]
        [InlineData("POST", new[] { "PostToOrders", "PostToOrdersFromCustomer" }, "PostToOrdersFromCustomer")]
        public void SelectAction_Returns_ExpectedMethodOnBaseType(string method, string[] methodsInController,
            string expectedSelectedAction)
        {
            // Arrange
            string key = "42";
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var ordersProperty = model.Customer.FindProperty("Orders") as IEdmNavigationProperty;
            ODataPath odataPath = new ODataPath(new EntitySetPathSegment(model.Customers), new KeyValuePathSegment(key),
                new NavigationPathSegment(ordersProperty));
            HttpControllerContext controllerContext = CreateControllerContext(method);
            var actionMap = GetMockActionMap(methodsInController);

            // Act
            string selectedAction = new NavigationRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Equal(expectedSelectedAction, selectedAction);
            if (expectedSelectedAction == null)
            {
                Assert.Empty(controllerContext.RouteData.Values);
            }
            else
            {
                Assert.Equal(1, controllerContext.RouteData.Values.Count);
                Assert.Equal(key, controllerContext.RouteData.Values["key"]);
            }
        }

        [Theory]
        [InlineData("GET", new string[] { }, null)]
        [InlineData("GET", new[] { "UnrelatedAction" }, null)]
        [InlineData("GET", new[] { "GetSpecialOrders" }, "GetSpecialOrders")]
        [InlineData("GET", new[] { "GetSpecialOrders", "GetOrders" }, "GetSpecialOrders")]
        [InlineData("GET", new[] { "GetSpecialOrders", "GetSpecialOrdersFromSpecialCustomer" }, "GetSpecialOrdersFromSpecialCustomer")]
        [InlineData("POST", new string[] { }, null)]
        [InlineData("POST", new[] { "UnrelatedAction" }, null)]
        [InlineData("POST", new[] { "PostToSpecialOrders" }, "PostToSpecialOrders")]
        [InlineData("POST", new[] { "PostToSpecialOrders", "PostToOrders" }, "PostToSpecialOrders")]
        [InlineData("POST", new[] { "PostToSpecialOrders", "PostToSpecialOrdersFromSpecialCustomer" }, "PostToSpecialOrdersFromSpecialCustomer")]
        public void SelectAction_Returns_ExpectedMethodOnDerivedType(string method, string[] methodsInController,
            string expectedSelectedAction)
        {
            // Arrange
            string key = "42";
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var specialOrdersProperty = model.SpecialCustomer.FindProperty("SpecialOrders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(new EntitySetPathSegment(model.Customers), new KeyValuePathSegment(key),
                new CastPathSegment(model.SpecialCustomer), new NavigationPathSegment(specialOrdersProperty));

            HttpControllerContext controllerContext = CreateControllerContext(method);
            var actionMap = GetMockActionMap(methodsInController);

            // Act
            string selectedAction = new NavigationRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Equal(expectedSelectedAction, selectedAction);
            if (expectedSelectedAction == null)
            {
                Assert.Empty(controllerContext.RouteData.Values);
            }
            else
            {
                Assert.Equal(1, controllerContext.RouteData.Values.Count);
                Assert.Equal(key, controllerContext.RouteData.Values["key"]);
            }
        }

        private static ILookup<string, HttpActionDescriptor> GetMockActionMap(params string[] actionNames)
        {
            return actionNames.Select(name => GetMockActionDescriptor(name)).ToLookup(a => a.ActionName);
        }

        private static HttpActionDescriptor GetMockActionDescriptor(string name)
        {
            Mock<HttpActionDescriptor> actionDescriptor = new Mock<HttpActionDescriptor> { CallBase = true };
            actionDescriptor.Setup(a => a.ActionName).Returns(name);
            return actionDescriptor.Object;
        }

        private static HttpControllerContext CreateControllerContext(string method)
        {
            HttpControllerContext controllerContext = new HttpControllerContext();
            controllerContext.Request = new HttpRequestMessage(new HttpMethod(method), "http://localhost/");
            controllerContext.RouteData = new HttpRouteData(new HttpRoute());
            return controllerContext;
        }
    }
}
