// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Net.Http;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Routing;
using Xunit;
#else
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Routing.Conventions
{
    public class FunctionRoutingConventionTests
    {
        private const string _serviceRoot = "http://any/";

#if NETCORE
        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissRouteContext()
        {
            // Arrange
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => functionConvention.SelectAction(null),
                "routeContext");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissOdataPath()
        {
            // Arrange
            var request = RequestFactory.Create();
            var routeContext = new RouteContext(request.HttpContext);
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => functionConvention.SelectAction(routeContext),
                "odataPath");
        }
#else
        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfODataPathIsNull()
        {
            // Arrange
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
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
            ExceptionAssert.ThrowsArgumentNull(
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
            ExceptionAssert.ThrowsArgumentNull(
                () => functionConvention.SelectAction(odataPath, controllerContext, actionMap: null),
                "actionMap");
        }
#endif

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
            var request = RequestFactory.Create(new HttpMethod(requestMethod), "http://localhost/");
            var emptyActionMap = SelectActionHelper.CreateActionMap();

            // Act
            string selectedAction = SelectActionHelper.SelectAction(functionConvention, odataPath, request, emptyActionMap);

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
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("IsUpgraded");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(functionConvention, odataPath, request, actionMap);

            // Assert
            Assert.Equal("IsUpgraded", selectedAction);
            Assert.Equal(3, SelectActionHelper.GetRouteData(request).Values.Count);
            Assert.Equal(1, SelectActionHelper.GetRouteData(request).Values["key"]);
            Assert.Equal(1, SelectActionHelper.GetRouteData(request).Values["keyID"]);
            Assert.Equal(1, SelectActionHelper.GetRouteData(request).Values[ODataRouteConstants.KeyCount]);
        }

        [Fact]
        public void SelectAction_ReturnsFunctionName_ForSingletonFunctionOnEntity()
        {
            // Arrange
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "VipCustomer/NS.IsUpgraded");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("IsUpgraded");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(functionConvention, odataPath, request, actionMap);

            // Assert
            Assert.Equal("IsUpgraded", selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
        }

        [Fact]
        public void SelectAction_ReturnsFunctionName_ForFunctionOnEntityCollection()
        {
            // Arrange
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "Customers/NS.IsAnyUpgraded");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("IsAnyUpgraded");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(functionConvention, odataPath, request, actionMap);

            // Assert
            Assert.Equal("IsAnyUpgraded", selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
        }

        [Fact]
        public void SelectAction_UpdatesRouteData_ForEntityFunctionWithParameters()
        {
            // Arrange
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "Customers(1)/NS.IsUpgradedWithParam(city='any')");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("IsUpgradedWithParam");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(functionConvention, odataPath, request, actionMap);

            // Assert
            Assert.Equal("IsUpgradedWithParam", selectedAction);
            Assert.Equal(4, SelectActionHelper.GetRouteData(request).Values.Count);
            Assert.Equal(1, SelectActionHelper.GetRouteData(request).Values["key"]);
            Assert.Equal(1, SelectActionHelper.GetRouteData(request).Values["keyID"]);
            Assert.Equal("any", SelectActionHelper.GetRouteData(request).Values["city"]);
            Assert.Equal(2, SelectActionHelper.GetRouteData(request).Values[ODataRouteConstants.KeyCount]);
        }

        [Fact]
        public void SelectAction_UpdatesRouteData_ForSingletonFunctionWithParameters()
        {
            // Arrange
            FunctionRoutingConvention functionConvention = new FunctionRoutingConvention();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "VipCustomer/NS.IsUpgradedWithParam(city='any')");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("IsUpgradedWithParam");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(functionConvention, odataPath, request, actionMap);

            // Assert
            Assert.Equal("IsUpgradedWithParam", selectedAction);
            Assert.Equal(2, SelectActionHelper.GetRouteData(request).Values.Count);
            Assert.Equal("any", SelectActionHelper.GetRouteData(request).Values["city"]);
            Assert.Equal(1, SelectActionHelper.GetRouteData(request).Values[ODataRouteConstants.KeyCount]);
        }

        [Fact]
        public void SelectAction_ReturnsNull_IfActionIsMissing()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "Customers(1)/NS.IsUpgraded");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var emptyActionMap = SelectActionHelper.CreateActionMap();

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new FunctionRoutingConvention(), odataPath, request, emptyActionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
        }

        [Fact]
        public void SelectAction_ReturnsFunctionName_DollarCount()
        {
            // Arrange
            var model = new CustomersModelWithInheritance();
            var handler = new DefaultODataPathHandler();
            ODataPath odataPath = handler.Parse(model.Model, _serviceRoot, "Customers(1)/NS.GetOrders(parameter=5)/$count");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("GetOrders");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new FunctionRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Equal("GetOrders", selectedAction);
            Assert.Equal(4, SelectActionHelper.GetRouteData(request).Values.Count);
            Assert.Equal(1, SelectActionHelper.GetRouteData(request).Values["key"]);
            Assert.Equal(1, SelectActionHelper.GetRouteData(request).Values["keyID"]);
            Assert.Equal(5, SelectActionHelper.GetRouteData(request).Values["parameter"]);
            Assert.Equal(2, SelectActionHelper.GetRouteData(request).Values[ODataRouteConstants.KeyCount]);
        }
    }
}
