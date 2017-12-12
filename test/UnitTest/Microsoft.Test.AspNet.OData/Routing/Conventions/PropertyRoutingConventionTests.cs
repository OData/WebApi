// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.TestCommon;
using Moq;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Routing.Conventions
{
    public class PropertyRoutingConventionTests
    {
        private const string _serviceRoot = "http://any/";

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissOdataPath()
        {
            // Arrange
            Mock<HttpControllerContext> controllerContext = new Mock<HttpControllerContext>();
            ILookup<string, HttpActionDescriptor> emptyMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new PropertyRoutingConvention().SelectAction(null, controllerContext.Object, emptyMap),
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
                () => new PropertyRoutingConvention().SelectAction(odataPath, null, emptyMap),
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
                () => new PropertyRoutingConvention().SelectAction(odataPath, controllerContext.Object, null),
                "actionMap");
        }

        [Theory]
        [InlineData("Get", "Get")]
        [InlineData("Put", "PutTo")]
        [InlineData("Patch", "PatchTo")]
        [InlineData("Delete", "DeleteTo")]
        public void SelectAction_OnEntitySetPath_ReturnsTheActionName(string httpMethod, string prefix)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "Customers(7)/Name");
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => prefix + "NameFromCustomer");
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                Request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/"),
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };
            controllerContext.Request.SetRequestContext(requestContext);

            // Act
            string selectedAction = new PropertyRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal(prefix + "NameFromCustomer", selectedAction);
            Assert.Single(controllerContext.Request.GetRouteData().Values);
            Assert.Equal(7, controllerContext.Request.GetRouteData().Values["key"]);
        }

        [Theory]
        [InlineData("Get", "Get")]
        [InlineData("Put", "PutTo")]
        [InlineData("Patch", "PatchTo")]
        public void SelectAction_OnEntitySetPath_ReturnsTheActionNameOfCast(string httpMethod, string prefix)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "Customers(7)/Account/NS.SpecialAccount");
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => prefix + "AccountOfSpecialAccountFromCustomer");
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                Request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/"),
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };
            controllerContext.Request.SetRequestContext(requestContext);

            // Act
            string selectedAction = new PropertyRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal(prefix + "AccountOfSpecialAccountFromCustomer", selectedAction);
            Assert.Single(controllerContext.Request.GetRouteData().Values);
            Assert.Equal(7, controllerContext.Request.GetRouteData().Values["key"]);
        }

        [Fact]
        public void SelectAction_ReturnsTheActionName_DollarCount()
        {
            // Arrange
            IEdmModel model = ODataCountTest.GetEdmModel();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(
                model, _serviceRoot, "DollarCountEntities(7)/EnumCollectionProp/$count");
            ILookup<string, HttpActionDescriptor> actionMap =
                new HttpActionDescriptor[1].ToLookup(desc => "GetEnumCollectionPropFromDollarCountEntity");
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                Request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/"),
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };
            controllerContext.Request.SetRequestContext(requestContext);

            // Act
            string selectedAction = new PropertyRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal("GetEnumCollectionPropFromDollarCountEntity", selectedAction);
            Assert.Single(controllerContext.Request.GetRouteData().Values);
            Assert.Equal(7, controllerContext.Request.GetRouteData().Values["key"]);
        }

        [Theory]
        [InlineData("Get", "Get")]
        [InlineData("Put", "PutTo")]
        [InlineData("Patch", "PatchTo")]
        [InlineData("Delete", "DeleteTo")]
        public void SelectAction_OnSingletonPath_ReturnsTheActionName(string httpMethod, string prefix)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "VipCustomer/Address");
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => prefix + "Address");
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                Request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/"),
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };
            controllerContext.Request.SetRequestContext(requestContext);

            // Act
            string selectedAction = new PropertyRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal(prefix + "Address", selectedAction);
            Assert.Empty(controllerContext.Request.GetRouteData().Values);
        }

        [Theory]
        [InlineData("Get", "Get")]
        [InlineData("Put", "PutTo")]
        [InlineData("Patch", "PatchTo")]
        public void SelectAction_OnSingletonPath_ReturnsTheActionNameWithCast(string httpMethod, string prefix)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "VipCustomer/Account/NS.SpecialAccount");
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => prefix + "AccountOfSpecialAccountFromCustomer");
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                Request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/"),
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };
            controllerContext.Request.SetRequestContext(requestContext);

            // Act
            string selectedAction = new PropertyRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal(prefix + "AccountOfSpecialAccountFromCustomer", selectedAction);
            Assert.Empty(controllerContext.Request.GetRouteData().Values);
        }

        [Fact]
        public void SelectAction_ReturnsNull_IfActionIsMissing()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "Customers(10)/Name");
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
            string selectedAction = new PropertyRoutingConvention().SelectAction(odataPath, controllerContext, emptyActionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(controllerContext.Request.GetRouteData().Values);
        }

        [Fact]
        public void SelectAction_ReturnsNull_IfPatchToCollectionProperty()
        {
            // Arrange
            IEdmModel model = ODataCountTest.GetEdmModel();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model, _serviceRoot, "DollarCountEntities(7)/EnumCollectionProp");
            ILookup<string, HttpActionDescriptor> emptyActionMap = new HttpActionDescriptor[0].ToLookup(desc => "PatchToEnumCollectionProp");
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                Request = new HttpRequestMessage(new HttpMethod("Patch"), "http://localhost/"),
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };
            controllerContext.Request.SetRequestContext(requestContext);

            // Act
            string selectedAction = new PropertyRoutingConvention().SelectAction(odataPath, controllerContext, emptyActionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(controllerContext.Request.GetRouteData().Values);
        }
    }
}
