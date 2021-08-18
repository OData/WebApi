//-----------------------------------------------------------------------------
// <copyright file="ActionRoutingConventionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
    public class ActionRoutingConventionTest
    {
        private const string _serviceRoot = "http://any/";

#if NETCORE
        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissRouteContext()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new ActionRoutingConvention().SelectAction(null),
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
                () => new ActionRoutingConvention().SelectAction(routeContext),
                "odataPath");
        }
#else
        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfODataPathIsNull()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new ActionRoutingConvention().SelectAction(odataPath: null, controllerContext: null, actionMap: null),
                "odataPath");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfControllerContextIsNull()
        {
            // Arrange
            ODataPath odataPath = new ODataPath();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new ActionRoutingConvention().SelectAction(odataPath, controllerContext: null, actionMap: null),
                "controllerContext");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfActionMapIsNull()
        {
            // Arrange
            ODataPath odataPath = new ODataPath();
            var controllerContext = new Mock<HttpControllerContext>().Object;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new ActionRoutingConvention().SelectAction(odataPath, controllerContext, actionMap: null),
                "actionMap");
        }
#endif

        [Theory]
        [InlineData("GET")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("MERGE")]
        [InlineData("PATCH")]
        public void SelectAction_ReturnsNull_RequestMethodIsNotPost(string requestMethod)
        {
            // Arrange
            ODataPath odataPath = new ODataPath();
            var request = RequestFactory.Create(new HttpMethod(requestMethod), "http://localhost/");
            var emptyActionMap = SelectActionHelper.CreateActionMap();

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new ActionRoutingConvention(), odataPath, request, emptyActionMap);

            // Assert
            Assert.Null(selectedAction);
        }

        [Fact]
        public void SelectAction_ReturnsTheActionName_ForEntitySetActionBoundToEntitySet()
        {
            // Arrange
            ActionRoutingConvention actionConvention = new ActionRoutingConvention();
            IEdmModel model = ODataRoutingModel.GetModel();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model, _serviceRoot,"RoutingCustomers/Default.GetVIPs");
            var request = RequestFactory.Create(HttpMethod.Post, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("GetVIPs");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new ActionRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Equal("GetVIPs", selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
        }

        [Fact]
        public void SelectAction_ReturnsTheActionName_ForSingletonActionBoundToEntity()
        {
            // Arrange
            ActionRoutingConvention actionConvention = new ActionRoutingConvention();
            IEdmModel model = new CustomersModelWithInheritance().Model;
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model, _serviceRoot, "VipCustomer/NS.upgrade");
            var request = RequestFactory.Create(HttpMethod.Post, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("upgrade");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new ActionRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Equal("upgrade", selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
        }

        [Theory]
        [InlineData("RoutingCustomers(1)/Default.GetRelatedRoutingCustomers")]
        [InlineData("RoutingCustomers/Default.GetProducts")]
        public void SelectAction_ReturnsNull_IfActionIsMissing(string path)
        {
            // Arrange
            ODataPath odataPath = new DefaultODataPathHandler().Parse(ODataRoutingModel.GetModel(), _serviceRoot, path);
            var request = RequestFactory.Create(HttpMethod.Post, "http://localhost/");
            var emptyActionMap = SelectActionHelper.CreateActionMap();

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new ActionRoutingConvention(), odataPath, request, emptyActionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
        }
    }
}
