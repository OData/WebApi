// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test.Routing.Conventions
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
            var keys = new[] {new KeyValuePair<string, object>("ID", 42)};
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var ordersProperty = model.Customer.FindProperty("Orders") as IEdmNavigationProperty;
            ODataPath odataPath = new ODataPath(new EntitySetSegment(model.Customers), new KeySegment(keys, model.Customer, model.Customers),
                new NavigationPropertySegment(ordersProperty, model.Orders));
            var request = RequestFactory.Create(new HttpMethod(method), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(methodsInController);

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new NavigationRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Equal(expectedSelectedAction, selectedAction);
            if (expectedSelectedAction == null)
            {
                Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
            }
            else
            {
                Assert.Equal(2, SelectActionHelper.GetRouteData(request).Values.Count);
                Assert.Equal(42, SelectActionHelper.GetRouteData(request).Values["key"]);
                Assert.Equal(42, SelectActionHelper.GetRouteData(request).Values["keyID"]);
                Assert.Equal(1, SelectActionHelper.GetRoutingConventionsStore(request)[ODataRouteConstants.KeyCount]);
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
            ODataPath odataPath = new ODataPath(new SingletonSegment(model.VipCustomer),
                new NavigationPropertySegment(ordersProperty, model.Orders));
            var request = RequestFactory.Create(new HttpMethod(method), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(methodsInController);

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new NavigationRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Equal(expectedSelectedAction, selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
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
            var keys = new[] {new KeyValuePair<string, object>("ID", 42)};
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var specialOrdersProperty = model.SpecialCustomer.FindProperty("SpecialOrders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(new EntitySetSegment(model.Customers), new KeySegment(keys, model.Customer, model.Customers),
                new TypeSegment(model.SpecialCustomer, model.Customers), new NavigationPropertySegment(specialOrdersProperty, model.Orders));
            var request = RequestFactory.Create(new HttpMethod(method), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(methodsInController);

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new NavigationRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Equal(expectedSelectedAction, selectedAction);
            if (expectedSelectedAction == null)
            {
                Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
            }
            else
            {
                Assert.Equal(2, SelectActionHelper.GetRouteData(request).Values.Count);
                Assert.Equal(42, SelectActionHelper.GetRouteData(request).Values["key"]);
                Assert.Equal(42, SelectActionHelper.GetRouteData(request).Values["keyID"]);
                Assert.Equal(1, SelectActionHelper.GetRoutingConventionsStore(request)[ODataRouteConstants.KeyCount]);
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

            ODataPath odataPath = new ODataPath(new SingletonSegment(model.VipCustomer), new TypeSegment(model.SpecialCustomer, model.Customers),
                new NavigationPropertySegment(specialOrdersProperty, model.Orders));
            var request = RequestFactory.Create(new HttpMethod(method), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(methodsInController);

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new NavigationRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Equal(expectedSelectedAction, selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
        }

        [Theory]
        [InlineData("Companies(1)/CEO")]
        [InlineData("MyCompany/CEO")]
        [InlineData("Employees(1)/WorkCompany")]
        public void SelectAction_ReturnsNull_IfPostToNavigationPropertyBindingToNonCollectionValuedNavigationProperty(string path)
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Company>("Companies");
            builder.Singleton<Company>("MyCompany");
            builder.EntitySet<Employee>("Employees");
            builder.Singleton<Employee>("Tony");
            IEdmModel model = builder.GetEdmModel();

            ODataPath odataPath = new DefaultODataPathHandler().Parse(model, "http://any/", path);
            var request = RequestFactory.Create(HttpMethod.Post, "http://localhost/");
            var emptyActionMap = SelectActionHelper.CreateActionMap();

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new NavigationRoutingConvention(), odataPath, request, emptyActionMap);

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

            var keys = new[] { new KeyValuePair<string, object>("ID", 42) };
            ODataPath odataPath = new ODataPath(new EntitySetSegment(model.Customers), new KeySegment(keys, model.Customer, model.Customers),
                new NavigationPropertySegment(ordersProperty, model.Orders));

            var request = RequestFactory.Create(new HttpMethod(method), "http://localhost/");
            var emptyActionMap = SelectActionHelper.CreateActionMap();

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new NavigationRoutingConvention(), odataPath, request, emptyActionMap);

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

            var keys = new[] { new KeyValuePair<string, object>("ID", 1) };
            ODataPath odataPath = new ODataPath(new EntitySetSegment(model.Orders), new KeySegment(keys, model.Order, model.Orders),
                new NavigationPropertySegment(customerProperty, model.Customers));

            var request = RequestFactory.Create(new HttpMethod(method), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(methodsInController);

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new NavigationRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Equal(expectedSelectedAction, selectedAction);
            Assert.Equal(1, SelectActionHelper.GetRouteData(request).Values["key"]);
            Assert.Equal(1, SelectActionHelper.GetRouteData(request).Values["keyID"]);
        }

        [Theory]
        [InlineData("GET", new[] { "GetOrders" }, "GetOrders")]
        [InlineData("GET", new[] { "GetOrders", "Get" }, "GetOrders")]
        [InlineData("GET", new[] { "GetOrders", "GetOrdersFromCustomer" }, "GetOrdersFromCustomer")]
        public void SelectAction_Returns_DollarCount(string method, string[] methodsInController,
            string expectedSelectedAction)
        {
            // Arrange
            var keys = new[] { new KeyValuePair<string, object>("ID", 42) };
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var ordersProperty = model.Customer.FindProperty("Orders") as IEdmNavigationProperty;
            ODataPath odataPath = new ODataPath(new EntitySetSegment(model.Customers), new KeySegment(keys, model.Customer, model.Customers),
                new NavigationPropertySegment(ordersProperty, model.Orders), CountSegment.Instance);

            var request = RequestFactory.Create(new HttpMethod(method), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(methodsInController);

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new NavigationRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Equal(expectedSelectedAction, selectedAction);
            Assert.Equal(2, SelectActionHelper.GetRouteData(request).Values.Count);
            Assert.Equal(42, SelectActionHelper.GetRouteData(request).Values["key"]);
            Assert.Equal(42, SelectActionHelper.GetRouteData(request).Values["keyID"]);
            Assert.Equal(1, SelectActionHelper.GetRoutingConventionsStore(request)[ODataRouteConstants.KeyCount]);
        }

        [Theory]
        [InlineData("GET", new[] { "GetSpecialOrders" }, "GetSpecialOrders")]
        [InlineData("GET", new[] { "GetSpecialOrders", "GetOrders" }, "GetSpecialOrders")]
        [InlineData("GET", new[] { "GetSpecialOrders", "GetSpecialOrdersFromSpecialCustomer" }, "GetSpecialOrdersFromSpecialCustomer")]
        public void SelectAction_Returns_DerivedTypeWithDollarCount(string method, string[] methodsInController,
            string expectedSelectedAction)
        {
            // Arrange
            var keys = new[] { new KeyValuePair<string, object>("ID", 42) };
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var specialOrdersProperty = model.SpecialCustomer.FindProperty("SpecialOrders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(new EntitySetSegment(model.Customers),
                new KeySegment(keys, model.Customer, model.Customers),
                new TypeSegment(model.SpecialCustomer, model.Customers),
                new NavigationPropertySegment(specialOrdersProperty, model.Orders),
                CountSegment.Instance);

            var request = RequestFactory.Create(new HttpMethod(method), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(methodsInController);

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new NavigationRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Equal(expectedSelectedAction, selectedAction);
            Assert.Equal(2, SelectActionHelper.GetRouteData(request).Values.Count);
            Assert.Equal(42, SelectActionHelper.GetRouteData(request).Values["key"]);
            Assert.Equal(42, SelectActionHelper.GetRouteData(request).Values["keyID"]);
            Assert.Equal(1, SelectActionHelper.GetRoutingConventionsStore(request)[ODataRouteConstants.KeyCount]);
        }

        [Theory]
        [InlineData("POST", new[] { "GetSpecialOrders" })]
        [InlineData("POST", new[] { "GetSpecialOrders", "GetOrders" })]
        [InlineData("POST", new[] { "GetSpecialOrders", "GetSpecialOrdersFromSpecialCustomer" })]
        public void SelectAction_ReturnsNull_NotSupportedMethodForDollarCount(string method, string[] methodsInController)
        {
            // Arrange
            var keys = new[] { new KeyValuePair<string, object>("ID", 42) };
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var specialOrdersProperty = model.SpecialCustomer.FindProperty("SpecialOrders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(new EntitySetSegment(model.Customers), new KeySegment(keys, model.Customer, model.Customers),
                new TypeSegment(model.SpecialCustomer, model.Customers),
                new NavigationPropertySegment(specialOrdersProperty, model.Orders),
                CountSegment.Instance);

            var request = RequestFactory.Create(new HttpMethod(method), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(methodsInController);

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new NavigationRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Null(selectedAction);
        }
    }
}
