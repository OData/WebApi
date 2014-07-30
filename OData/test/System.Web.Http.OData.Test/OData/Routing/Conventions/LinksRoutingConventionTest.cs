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
    public class LinksRoutingConventionTest
    {
        [Theory]
        [InlineData("DELETE", new string[] { }, null)]
        [InlineData("DELETE", new[] { "UnrelatedAction" }, null)]
        [InlineData("DELETE", new[] { "DeleteLinkToOrders" }, "DeleteLinkToOrders")]
        [InlineData("DELETE", new[] { "DeleteLinkToOrders", "DeleteLink" }, "DeleteLinkToOrders")]
        [InlineData("DELETE", new[] { "DeleteLinkToOrdersFromCustomer", "DeleteLinkToOrders" }, "DeleteLinkToOrdersFromCustomer")]
        [InlineData("POST", new string[] { }, null)]
        [InlineData("POST", new[] { "UnrelatedAction" }, null)]
        [InlineData("POST", new[] { "CreateLinkToOrders" }, "CreateLinkToOrders")]
        [InlineData("POST", new[] { "CreateLinkToOrders", "CreateLink" }, "CreateLinkToOrders")]
        [InlineData("POST", new[] { "CreateLinkToOrders", "CreateLinkToOrdersFromCustomer" }, "CreateLinkToOrdersFromCustomer")]
        public void SelectAction_Returns_ExpectedMethodOnBaseType(string method, string[] methodsInController,
            string expectedSelectedAction)
        {
            // Arrange
            string key = "42";
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var ordersProperty = model.Customer.FindProperty("Orders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(new EntitySetPathSegment(model.Customers), new KeyValuePathSegment(key),
                new LinksPathSegment(), new NavigationPathSegment(ordersProperty));

            HttpControllerContext controllerContext = CreateControllerContext(method);
            var actionMap = GetMockActionMap(methodsInController);

            // Act
            string selectedAction = new LinksRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

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
        [InlineData("DELETE", new[] { "DeleteLinkToSpecialOrders" }, "DeleteLinkToSpecialOrders")]
        [InlineData("DELETE", new[] { "DeleteLinkToSpecialOrders", "DeleteLinkToOrders" }, "DeleteLinkToSpecialOrders")]
        [InlineData("DELETE", new[] { "DeleteLinkToSpecialOrders", "DeleteLinkToSpecialOrdersFromSpecialCustomer" }, "DeleteLinkToSpecialOrdersFromSpecialCustomer")]
        [InlineData("POST", new string[] { }, null)]
        [InlineData("POST", new[] { "UnrelatedAction" }, null)]
        [InlineData("POST", new[] { "CreateLinkToSpecialOrders" }, "CreateLinkToSpecialOrders")]
        [InlineData("POST", new[] { "CreateLinkToSpecialOrders", "CreateLinkToOrders" }, "CreateLinkToSpecialOrders")]
        [InlineData("POST", new[] { "CreateLinkToSpecialOrders", "CreateLinkToSpecialOrdersFromSpecialCustomer" }, "CreateLinkToSpecialOrdersFromSpecialCustomer")]
        public void SelectAction_Returns_ExpectedMethodOnDerivedType(string method, string[] methodsInController,
            string expectedSelectedAction)
        {
            // Arrange
            string key = "42";
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var specialOrdersProperty = model.SpecialCustomer.FindProperty("SpecialOrders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(new EntitySetPathSegment(model.Customers), new KeyValuePathSegment(key),
                new CastPathSegment(model.SpecialCustomer), new LinksPathSegment(), new NavigationPathSegment(specialOrdersProperty));

            HttpControllerContext controllerContext = CreateControllerContext(method);
            var actionMap = GetMockActionMap(methodsInController);

            // Act
            string selectedAction = new LinksRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

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
        public void SelectAction_SetsRelatedKey_ForDeleteLinkRequests()
        {
            // Arrange
            string key = "42";
            string relatedKey = "24";
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var specialOrdersProperty = model.SpecialCustomer.FindProperty("SpecialOrders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(new EntitySetPathSegment(model.Customers), new KeyValuePathSegment(key),
                new CastPathSegment(model.SpecialCustomer), new LinksPathSegment(), new NavigationPathSegment(specialOrdersProperty),
                new KeyValuePathSegment(relatedKey));

            HttpControllerContext controllerContext = CreateControllerContext("DELETE");
            var actionMap = GetMockActionMap("DeleteLink");

            // Act
            new LinksRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);
            var routeData = controllerContext.RouteData;

            // Assert
            Assert.Equal(key, routeData.Values["key"]);
            Assert.Equal("SpecialOrders", routeData.Values["navigationProperty"]);
            Assert.Equal(relatedKey, routeData.Values["relatedKey"]);
        }

        [Fact]
        public void SelectAction_SetsRouteData_ForCreateLinkRequests()
        {
            // Arrange
            string key = "42";
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var specialOrdersProperty = model.SpecialCustomer.FindProperty("SpecialOrders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(new EntitySetPathSegment(model.Customers), new KeyValuePathSegment(key),
                new CastPathSegment(model.SpecialCustomer), new LinksPathSegment(), new NavigationPathSegment(specialOrdersProperty));

            HttpControllerContext controllerContext = CreateControllerContext("POST");
            var actionMap = new[] { GetMockActionDescriptor("CreateLink") }.ToLookup(a => a.ActionName);

            // Act
            new LinksRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);
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
