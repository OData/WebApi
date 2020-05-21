// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Net.Http;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Moq;
using Xunit;
#else
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Moq;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Routing.Conventions
{
    public class PropertyRoutingConventionTests
    {
        private const string _serviceRoot = "http://any/";

#if NETCORE
        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissRouteContext()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new PropertyRoutingConvention().SelectAction(null),
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
                () => new PropertyRoutingConvention().SelectAction(routeContext),
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
#endif

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
            var request = RequestFactory.Create(new HttpMethod(httpMethod), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(prefix + "NameFromCustomer");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new PropertyRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal(prefix + "NameFromCustomer", selectedAction);
            Assert.Equal(3, SelectActionHelper.GetRouteData(request).Values.Count);
            Assert.Equal(7, SelectActionHelper.GetRouteData(request).Values["key"]);
            Assert.Equal(7, SelectActionHelper.GetRouteData(request).Values["keyID"]);
            Assert.Equal(1, SelectActionHelper.GetRouteData(request).Values[ODataRouteConstants.KeyCount]);
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
            var request = RequestFactory.Create(new HttpMethod(httpMethod), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(prefix + "AccountOfSpecialAccountFromCustomer");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new PropertyRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal(prefix + "AccountOfSpecialAccountFromCustomer", selectedAction);
            Assert.Equal(3, SelectActionHelper.GetRouteData(request).Values.Count);
            Assert.Equal(7, SelectActionHelper.GetRouteData(request).Values["key"]);
            Assert.Equal(7, SelectActionHelper.GetRouteData(request).Values["keyID"]);
            Assert.Equal(1, SelectActionHelper.GetRouteData(request).Values[ODataRouteConstants.KeyCount]);
        }

        [Fact]
        public void SelectAction_ReturnsTheActionName_DollarCount()
        {
            // Arrange
            IEdmModel model = ODataCountTest.GetEdmModel();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(
                model, _serviceRoot, "DollarCountEntities(7)/EnumCollectionProp/$count");

            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("GetEnumCollectionPropFromDollarCountEntity");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new PropertyRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal("GetEnumCollectionPropFromDollarCountEntity", selectedAction);
            Assert.Equal(3, SelectActionHelper.GetRouteData(request).Values.Count);
            Assert.Equal(7, SelectActionHelper.GetRouteData(request).Values["key"]);
            Assert.Equal(7, SelectActionHelper.GetRouteData(request).Values["keyID"]);
            Assert.Equal(1, SelectActionHelper.GetRouteData(request).Values[ODataRouteConstants.KeyCount]);
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
            var request = RequestFactory.Create(new HttpMethod(httpMethod), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(prefix + "Address");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new PropertyRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal(prefix + "Address", selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
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
            var request = RequestFactory.Create(new HttpMethod(httpMethod), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(prefix + "AccountOfSpecialAccountFromCustomer");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new PropertyRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal(prefix + "AccountOfSpecialAccountFromCustomer", selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
        }

        [Fact]
        public void SelectAction_ReturnsNull_IfActionIsMissing()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "Customers(10)/Name");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var emptyActionMap = SelectActionHelper.CreateActionMap();

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new PropertyRoutingConvention(), odataPath, request, emptyActionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
        }

        [Fact]
        public void SelectAction_ReturnsNull_IfPatchToCollectionProperty()
        {
            // Arrange
            IEdmModel model = ODataCountTest.GetEdmModel();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model, _serviceRoot, "DollarCountEntities(7)/EnumCollectionProp");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("PatchToEnumCollectionProp");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new PropertyRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
        }

        [Fact]
        public void SelectAction_ReturnsNull_IfPostToNonCollectionProperty()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "VipCustomer/Address");
            var request = RequestFactory.Create(HttpMethod.Post, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("PostToAddress");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new PropertyRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
        }

        [Theory]
        [InlineData("Get", "Get")]
        [InlineData("Put", "PutTo")]
        [InlineData("Delete", "DeleteTo")]
        [InlineData("Post", "PostTo")]
        public void SelectAction_OnComplexCollection_ReturnsTheActionName(string httpMethod, string prefix)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "Customers(7)/OtherAccounts");
            var request = RequestFactory.Create(new HttpMethod(httpMethod), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(prefix + "OtherAccountsFromCustomer");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new PropertyRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal(prefix + "OtherAccountsFromCustomer", selectedAction);
            Assert.Equal(3, SelectActionHelper.GetRouteData(request).Values.Count);
            Assert.Equal(7, SelectActionHelper.GetRouteData(request).Values["key"]);
            Assert.Equal(7, SelectActionHelper.GetRouteData(request).Values["keyID"]);
            Assert.Equal(1, SelectActionHelper.GetRouteData(request).Values[ODataRouteConstants.KeyCount]);
        }

        [Theory]
        [InlineData("Get", "Get")]
        [InlineData("Put", "PutTo")]
        [InlineData("Post", "PostTo")]
        public void SelectAction_OnEnumCollection_ReturnsTheActionName(string httpMethod, string prefix)
        {
            // Arrange
            IEdmModel model = ODataCountTest.GetEdmModel();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model, _serviceRoot, "DollarCountEntities(7)/EnumCollectionProp");
            var request = RequestFactory.Create(new HttpMethod(httpMethod), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(prefix + "EnumCollectionProp");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new PropertyRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal(prefix + "EnumCollectionProp", selectedAction);
            Assert.Equal(3, SelectActionHelper.GetRouteData(request).Values.Count);
            Assert.Equal(7, SelectActionHelper.GetRouteData(request).Values["key"]);
            Assert.Equal(7, SelectActionHelper.GetRouteData(request).Values["keyID"]);
            Assert.Equal(1, SelectActionHelper.GetRouteData(request).Values[ODataRouteConstants.KeyCount]);
        }
    }
}
