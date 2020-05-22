// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test.Routing.Conventions
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
            var keys = new[] { new KeyValuePair<string, object>("ID", 42) };
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var ordersProperty = model.Customer.FindProperty("Orders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(new EntitySetSegment(model.Customers), new KeySegment(keys, model.Customer, model.Customers),
                new NavigationPropertyLinkSegment(ordersProperty, model.Orders));

            var request = RequestFactory.Create(new HttpMethod(method), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(methodsInController);

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new RefRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Equal(expectedSelectedAction, selectedAction);
            if (expectedSelectedAction == null)
            {
                Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
            }
            else
            {
                Assert.Equal(4, SelectActionHelper.GetRouteData(request).Values.Count);
                Assert.Equal(42, SelectActionHelper.GetRouteData(request).Values["key"]);
                Assert.Equal(42, SelectActionHelper.GetRouteData(request).Values["keyID"]);
                Assert.Equal(ordersProperty.Name, SelectActionHelper.GetRouteData(request).Values["navigationProperty"]);
                Assert.Equal(2, SelectActionHelper.GetRouteData(request).Values[ODataRouteConstants.KeyCount]);
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
            var keys = new[] { new KeyValuePair<string, object>("ID", 42) };
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var specialOrdersProperty = model.SpecialCustomer.FindProperty("SpecialOrders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(
                new EntitySetSegment(model.Customers),
                new KeySegment(keys, model.Customer, model.Customers),
                new TypeSegment(model.SpecialCustomer, model.Customers),
                new NavigationPropertyLinkSegment(specialOrdersProperty, model.Orders));

            var request = RequestFactory.Create(new HttpMethod(method), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(methodsInController);

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new RefRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Equal(expectedSelectedAction, selectedAction);
            if (expectedSelectedAction == null)
            {
                Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
            }
            else
            {
                Assert.Equal(4, SelectActionHelper.GetRouteData(request).Values.Count);
                Assert.Equal(42, SelectActionHelper.GetRouteData(request).Values["key"]);
                Assert.Equal(42, SelectActionHelper.GetRouteData(request).Values["keyID"]);
                Assert.Equal(specialOrdersProperty.Name, SelectActionHelper.GetRouteData(request).Values["navigationProperty"]);
                Assert.Equal(2, SelectActionHelper.GetRouteData(request).Values[ODataRouteConstants.KeyCount]);
            }
        }

        [Fact]
        public void SelectAction_SetsRelatedKey_ForDeleteRefRequests()
        {
            // Arrange
            var keys = new[] { new KeyValuePair<string, object>("ID", 42) };
            var relatedKeys = new[] { new KeyValuePair<string, object>("ID", 24) };
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var specialOrdersProperty = model.SpecialCustomer.FindProperty("SpecialOrders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(
                new EntitySetSegment(model.Customers),
                new KeySegment(keys, model.Customer, model.Customers),
                new TypeSegment(model.SpecialCustomer, model.Customers),
                new NavigationPropertyLinkSegment(specialOrdersProperty, model.Orders),
                new KeySegment(relatedKeys, model.Order, model.Orders));

            var request = RequestFactory.Create(HttpMethod.Delete, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("DeleteRef");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new RefRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Equal(42, SelectActionHelper.GetRouteData(request).Values["key"]);
            Assert.Equal(42, SelectActionHelper.GetRouteData(request).Values["keyID"]);
            Assert.Equal("SpecialOrders", SelectActionHelper.GetRouteData(request).Values["navigationProperty"]);
            Assert.Equal(24, SelectActionHelper.GetRouteData(request).Values["relatedKey"]);
            Assert.Equal(24, SelectActionHelper.GetRouteData(request).Values["relatedKeyID"]);
        }

        [Theory]
        [InlineData("http://any/Customers(42)/Orders/$ref?$id=http://any/Orders(24)")]
        [InlineData("http://any/Customers(42)/Orders/$ref?$id=../../Orders(24)")]
        public void SelectAction_SetsRelatedKey_ForDeleteRefRequestsWithDollarId(string uri)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://any/", uri);
            var request = RequestFactory.Create(HttpMethod.Delete, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("DeleteRef");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new RefRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Equal("DeleteRef", selectedAction);
            Assert.Equal(42, SelectActionHelper.GetRouteData(request).Values["key"]);
            Assert.Equal(42, SelectActionHelper.GetRouteData(request).Values["keyID"]);
            Assert.Equal("Orders", SelectActionHelper.GetRouteData(request).Values["navigationProperty"]);
            Assert.Equal(24, SelectActionHelper.GetRouteData(request).Values["relatedKey"]);
            Assert.Equal(24, SelectActionHelper.GetRouteData(request).Values["relatedKeyID"]);
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

            var request = RequestFactory.Create(new HttpMethod(method), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("DeleteRef", "CreateRef", "GetRef", "PutRef", "PostRef");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new RefRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Null(selectedAction);
        }

        [Theory]
        [InlineData("POST", "CreateRef")]
        [InlineData("GET", "GetRef")]
        public void SelectAction_SetsRouteData_ForGetOrCreateRefRequests(string method, string actionName)
        {
            // Arrange
            var keys = new[] { new KeyValuePair<string, object>("ID", 42) };
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var specialOrdersProperty = model.SpecialCustomer.FindProperty("SpecialOrders") as IEdmNavigationProperty;

            ODataPath odataPath = new ODataPath(
                new EntitySetSegment(model.Customers),
                new KeySegment(keys, model.Customer, model.Customers),
                new TypeSegment(model.SpecialCustomer, model.Customers),
                new NavigationPropertyLinkSegment(specialOrdersProperty, model.Orders));

            var request = RequestFactory.Create(new HttpMethod(method), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(actionName);

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new RefRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Equal(42, SelectActionHelper.GetRouteData(request).Values["key"]);
            Assert.Equal(42, SelectActionHelper.GetRouteData(request).Values["keyID"]);
            Assert.Equal("SpecialOrders", SelectActionHelper.GetRouteData(request).Values["navigationProperty"]);
        }
    }
}
