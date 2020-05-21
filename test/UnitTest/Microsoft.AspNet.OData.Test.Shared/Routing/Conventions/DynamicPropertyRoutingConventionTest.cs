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
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Moq;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Routing.Conventions
{
    public class DynamicPropertyRoutingConventionTest
    {
        private DynamicPropertyRoutingConvention _routingConvention = new DynamicPropertyRoutingConvention();

        #region Negative Cases
#if NETCORE
        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissRouteContext()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => _routingConvention.SelectAction(null),
                "routeContext");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissOdataPath()
        {
            // Arrange
            var request = RequestFactory.Create();
            var routeContext = new RouteContext(request.HttpContext);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => _routingConvention.SelectAction(routeContext),
                "odataPath");
        }
#else
        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissOdataPath()
        {
            // Arrange
            Mock<HttpControllerContext> controllerContext = new Mock<HttpControllerContext>();
            ILookup<string, HttpActionDescriptor> emptyMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
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
            ExceptionAssert.ThrowsArgumentNull(
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
            ExceptionAssert.ThrowsArgumentNull(
                () => _routingConvention.SelectAction(odataPath, controllerContext.Object, null),
                "actionMap");
        }
#endif

        [Fact]
        public void SelectAction_ReturnsNull_IfActionIsMissing()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://localhost/", "Customers(10)/Account/Tax");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var emptyActionMap = SelectActionHelper.CreateActionMap();

            // Act
            string selectedAction = SelectActionHelper.SelectAction(_routingConvention, odataPath, request, emptyActionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
        }

        [Theory]
        [InlineData("Post")]
        [InlineData("Patch")]
        [InlineData("Put")]
        [InlineData("Delete")]
        public void SelectAction_ReturnsNull_IfNotCorrectMethod(string methodName)
        {
            // Arrange
            HttpMethod method = new HttpMethod(methodName);
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://localhost/", "Orders(7)/DynamicPropertyA");
            var request = RequestFactory.Create(method, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("GetDynamicProperty");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(_routingConvention, odataPath, request, actionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
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
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("GetDynamicPropertyFromAccount");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(_routingConvention, odataPath, request, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal("GetDynamicPropertyFromAccount", selectedAction);

            var routeData = SelectActionHelper.GetRouteData(request);
            Assert.Equal(5, routeData.Values.Count);
            Assert.Equal(7, routeData.Values["key"]);
            Assert.Equal(7, routeData.Values["keyID"]);
            Assert.Equal(1, routeData.Values[ODataRouteConstants.KeyCount]);
            Assert.Equal("Amount", routeData.Values["dynamicProperty"]);
            Assert.Equal("Amount", (routeData.Values[ODataParameterValue.ParameterValuePrefix + "dynamicProperty"] as ODataParameterValue).Value);
        }

        [Theory]
        [InlineData("VipCustomer/Account/Amount")]
        [InlineData("VipCustomer/NS.SpecialCustomer/Account/Amount")]
        public void SelectAction_OnSingletonPath_OpenComplexType_ReturnsTheActionName(string url)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://localhost/", url);
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("GetDynamicPropertyFromAccount");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(_routingConvention, odataPath, request, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal("GetDynamicPropertyFromAccount", selectedAction);

            var routeData = SelectActionHelper.GetRouteData(request);
            Assert.Equal(2, routeData.Values.Count);
            Assert.Equal("Amount", routeData.Values["dynamicProperty"]);
            Assert.Equal("Amount", (routeData.Values[ODataParameterValue.ParameterValuePrefix + "dynamicProperty"] as ODataParameterValue).Value);
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
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("GetDynamicProperty");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(_routingConvention, odataPath, request, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal("GetDynamicProperty", selectedAction);

            var routeData = SelectActionHelper.GetRouteData(request);
            Assert.Equal(5, routeData.Values.Count);
            Assert.Equal(7, routeData.Values["key"]);
            Assert.Equal(7, routeData.Values["keyID"]);
            Assert.Equal(1, routeData.Values[ODataRouteConstants.KeyCount]);
            Assert.Equal("DynamicPropertyA", routeData.Values["dynamicProperty"]);
            Assert.Equal("DynamicPropertyA", (routeData.Values[ODataParameterValue.ParameterValuePrefix + "dynamicProperty"] as ODataParameterValue).Value);
        }

        [Theory]
        [InlineData("RootOrder/DynamicPropertyA")]
        [InlineData("RootOrder/NS.SpecialOrder/DynamicPropertyA")]
        public void SelectAction_OnSingltonPath_OpenEntityType_ReturnsTheActionName(string url)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://localhost/", url);
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("GetDynamicProperty");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(_routingConvention, odataPath, request, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal("GetDynamicProperty", selectedAction);

            var routeData = SelectActionHelper.GetRouteData(request);
            Assert.Equal(2, routeData.Values.Count);
            Assert.Equal("DynamicPropertyA", routeData.Values["dynamicProperty"]);
            Assert.Equal("DynamicPropertyA", (routeData.Values[ODataParameterValue.ParameterValuePrefix + "dynamicProperty"] as ODataParameterValue).Value);
        }
        #endregion
    }
}