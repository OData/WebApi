// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing.Conventions
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
        public void SelectAction_OnEntitySetPath_Returns_ExpectedMethodOnBaseType(string method, string[] methodsInController,
            string expectedSelectedAction)
        {
            // Arrange
            const string key = "42";
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
        [InlineData("GET", new[] { "GetOrders" }, "GetOrders")]
        [InlineData("GET", new[] { "GetOrders", "Get" }, "GetOrders")]
        [InlineData("GET", new[] { "GetOrders", "GetOrdersFromCustomer" }, "GetOrdersFromCustomer")]
        [InlineData("POST", new string[] { }, null)]
        [InlineData("POST", new[] { "UnrelatedAction" }, null)]
        [InlineData("POST", new[] { "PostToOrders" }, "PostToOrders")]
        [InlineData("POST", new[] { "PostToOrders", "Post" }, "PostToOrders")]
        [InlineData("POST", new[] { "PostToOrders", "PostToOrdersFromCustomer" }, "PostToOrdersFromCustomer")]
        public void SelectAction_OnSingletonPath_Returns_ExpectedMethodOnBaseType(string method, string[] methodsInController,
            string expectedSelectedAction)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var ordersProperty = model.Customer.FindProperty("Orders") as IEdmNavigationProperty;
            ODataPath odataPath = new ODataPath(new SingletonPathSegment(model.VipCustomer),
                new NavigationPathSegment(ordersProperty));
            HttpControllerContext controllerContext = CreateControllerContext(method);
            var actionMap = GetMockActionMap(methodsInController);

            // Act
            string selectedAction = new NavigationRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Equal(expectedSelectedAction, selectedAction);
            Assert.Empty(controllerContext.RouteData.Values);
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
        public void SelectAction_OnEntitySetPath_Returns_ExpectedMethodOnDerivedType(string method, string[] methodsInController,
            string expectedSelectedAction)
        {
            // Arrange
            const string key = "42";
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
        public void SelectAction_OnSingletonPath_Returns_ExpectedMethodOnDerivedType(string method, string[] methodsInController,
            string expectedSelectedAction)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var specialOrdersProperty = model.SpecialCustomer.FindProperty("SpecialOrders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(new SingletonPathSegment(model.VipCustomer), new CastPathSegment(model.SpecialCustomer),
                new NavigationPathSegment(specialOrdersProperty));

            HttpControllerContext controllerContext = CreateControllerContext(method);
            var actionMap = GetMockActionMap(methodsInController);

            // Act
            string selectedAction = new NavigationRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Equal(expectedSelectedAction, selectedAction);
            Assert.Empty(controllerContext.RouteData.Values);
        }

        [Theory]
        [InlineData("Companies(1)/CEO")]
        [InlineData("MyCompany/CEO")]
        [InlineData("Employees(1)/WorkCompany")]
        public void SelectAction_ReturnsNull_IfPostToNavigationPropertyBindingToNonCollectionValuedNavigationProperty(string path)
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Company>("Companies");
            builder.Singleton<Company>("MyCompany");
            builder.EntitySet<Employee>("Employees");
            builder.Singleton<Employee>("Tony");
            IEdmModel model = builder.GetEdmModel();

            ODataPath odataPath = new DefaultODataPathHandler().Parse(model, "http://any/", path);
            HttpControllerContext controllerContext = CreateControllerContext("Post");
            var actionMap = GetMockActionMap();

            // Act
            string selectedAction = new NavigationRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Null(selectedAction);
        }

        [Theory]
        [InlineData("Put")]
        [InlineData("Patch")]
        public void SelectAction_ReturnsNull_IfToCollectionValuedNavigationProperty(string method)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var ordersProperty = model.Customer.FindProperty("Orders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(new EntitySetPathSegment(model.Customers), new KeyValuePathSegment("1"),
                new NavigationPathSegment(ordersProperty));

            HttpControllerContext controllerContext = CreateControllerContext(method);
            var actionMap = GetMockActionMap();

            // Act
            string selectedAction = new NavigationRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Null(selectedAction);
        }

        [Theory]
        [InlineData("Patch", new[] { "PatchToCustomer" }, "PatchToCustomer")]
        [InlineData("Put", new[] { "PutToCustomer" }, "PutToCustomer")]
        public void SelectAction_Returns_ExpectedMethod_OnNonCollectionValuedNavigationProperty(string method, string[] methodsInController,
            string expectedSelectedAction)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var customerProperty = model.Order.FindProperty("Customer") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(new EntitySetPathSegment(model.Orders), new KeyValuePathSegment("1"),
                new NavigationPathSegment(customerProperty));

            HttpControllerContext controllerContext = CreateControllerContext(method);
            var actionMap = GetMockActionMap(methodsInController);

            // Act
            string selectedAction = new NavigationRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Equal(expectedSelectedAction, selectedAction);
            Assert.Equal("1", controllerContext.RouteData.Values["key"]);
        }

        [Theory]
        [InlineData("GET", new[] { "GetOrders" }, "GetOrders")]
        [InlineData("GET", new[] { "GetOrders", "Get" }, "GetOrders")]
        [InlineData("GET", new[] { "GetOrders", "GetOrdersFromCustomer" }, "GetOrdersFromCustomer")]
        public void SelectAction_Returns_DollarCount(string method, string[] methodsInController,
            string expectedSelectedAction)
        {
            // Arrange
            const string key = "42";
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var ordersProperty = model.Customer.FindProperty("Orders") as IEdmNavigationProperty;
            ODataPath odataPath = new ODataPath(new EntitySetPathSegment(model.Customers), new KeyValuePathSegment(key),
                new NavigationPathSegment(ordersProperty), new CountPathSegment());
            HttpControllerContext controllerContext = CreateControllerContext(method);
            var actionMap = GetMockActionMap(methodsInController);

            // Act
            string selectedAction = new NavigationRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Equal(expectedSelectedAction, selectedAction);
            Assert.Equal(1, controllerContext.RouteData.Values.Count);
            Assert.Equal(key, controllerContext.RouteData.Values["key"]);
        }

        [Theory]
        [InlineData("GET", new[] { "GetSpecialOrders" }, "GetSpecialOrders")]
        [InlineData("GET", new[] { "GetSpecialOrders", "GetOrders" }, "GetSpecialOrders")]
        [InlineData("GET", new[] { "GetSpecialOrders", "GetSpecialOrdersFromSpecialCustomer" }, "GetSpecialOrdersFromSpecialCustomer")]
        public void SelectAction_Returns_DerivedTypeWithDollarCount(string method, string[] methodsInController,
            string expectedSelectedAction)
        {
            // Arrange
            const string key = "42";
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var specialOrdersProperty = model.SpecialCustomer.FindProperty("SpecialOrders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(new EntitySetPathSegment(model.Customers), new KeyValuePathSegment(key),
                new CastPathSegment(model.SpecialCustomer), new NavigationPathSegment(specialOrdersProperty), new CountPathSegment());

            HttpControllerContext controllerContext = CreateControllerContext(method);
            var actionMap = GetMockActionMap(methodsInController);

            // Act
            string selectedAction = new NavigationRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Equal(expectedSelectedAction, selectedAction);
            Assert.Equal(1, controllerContext.RouteData.Values.Count);
            Assert.Equal(key, controllerContext.RouteData.Values["key"]);
        }

        [Theory]
        [InlineData("POST", new[] { "GetSpecialOrders" })]
        [InlineData("POST", new[] { "GetSpecialOrders", "GetOrders" })]
        [InlineData("POST", new[] { "GetSpecialOrders", "GetSpecialOrdersFromSpecialCustomer" })]
        public void SelectAction_ReturnsNull_NotSupportedMethodForDollarCount(string method, string[] methodsInController)
        {
            // Arrange
            const string key = "42";
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var specialOrdersProperty = model.SpecialCustomer.FindProperty("SpecialOrders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(new EntitySetPathSegment(model.Customers), new KeyValuePathSegment(key),
                new CastPathSegment(model.SpecialCustomer), new NavigationPathSegment(specialOrdersProperty), new CountPathSegment());

            HttpControllerContext controllerContext = CreateControllerContext(method);
            var actionMap = GetMockActionMap(methodsInController);

            // Act
            string selectedAction = new NavigationRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Null(selectedAction);
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
