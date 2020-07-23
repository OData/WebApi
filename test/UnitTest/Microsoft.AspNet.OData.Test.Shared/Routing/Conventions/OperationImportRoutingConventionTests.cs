// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Net.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Xunit;
#else
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Routing.Conventions
{
    public class OperationImportRoutingConventionTests
    {
        private static IEdmModel EdmModel = GetEdmModel();

#if NETCORE
        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissRouteContext()
        {
            // Arrange
            OperationImportRoutingConvention importConvention = new OperationImportRoutingConvention();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => importConvention.SelectAction(null),
                "routeContext");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissOdataPath()
        {
            // Arrange
            var request = RequestFactory.Create();
            var routeContext = new RouteContext(request.HttpContext);
            OperationImportRoutingConvention importConvention = new OperationImportRoutingConvention();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => importConvention.SelectAction(routeContext),
                "odataPath");
        }
#else
        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfODataPathIsNull()
        {
            // Arrange
            OperationImportRoutingConvention importConvention = new OperationImportRoutingConvention();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => importConvention.SelectAction(odataPath: null, controllerContext: null, actionMap: null),
                "odataPath");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfControllerContextIsNull()
        {
            // Arrange
            OperationImportRoutingConvention importConvention = new OperationImportRoutingConvention();
            ODataPath odataPath = new ODataPath();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => importConvention.SelectAction(odataPath, controllerContext: null, actionMap: null),
                "controllerContext");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfActionMapIsNull()
        {
            // Arrange
            OperationImportRoutingConvention importConvention = new OperationImportRoutingConvention();
            ODataPath odataPath = new ODataPath();
            HttpControllerContext controllerContext = new HttpControllerContext();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => importConvention.SelectAction(odataPath, controllerContext, actionMap: null),
                "actionMap");
        }
#endif

        [Theory]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("MERGE")]
        [InlineData("PATCH")]
        public void SelectAction_ReturnsNull_RequestMethodIsNotGetOrPost(string requestMethod)
        {
            // Arrange
            OperationImportRoutingConvention importConvention = new OperationImportRoutingConvention();
            ODataPath odataPath = new ODataPath();
            var request = RequestFactory.Create(new HttpMethod(requestMethod), "http://localhost/");
            var emptyActionMap = SelectActionHelper.CreateActionMap();

            // Act
            string selectedAction = SelectActionHelper.SelectAction(importConvention, odataPath, request, emptyActionMap);

            // Assert
            Assert.Null(selectedAction);
        }

        [Fact]
        public void SelectAction_ReturnsFunctionImportName()
        {
            // Arrange
            OperationImportRoutingConvention importConvention = new OperationImportRoutingConvention();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(EdmModel, "http://localhost/", "RateByOrder(order=2)");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("RateByOrder");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(importConvention, odataPath, request, actionMap);

            // Assert
            Assert.Equal("RateByOrder", selectedAction);
            Assert.Single(SelectActionHelper.GetRouteData(request).Values);
            Assert.Equal(2, SelectActionHelper.GetRouteData(request).Values["order"]);
        }

        [Fact]
        public void SelectAction_ReturnsActionImportName()
        {
            // Arrange
            OperationImportRoutingConvention importConvention = new OperationImportRoutingConvention();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(EdmModel, "http://localhost/", "RateByName");
            var request = RequestFactory.Create(HttpMethod.Post, "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("RateByName");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(importConvention, odataPath, request, actionMap);

            // Assert
            Assert.Equal("RateByName", selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            var function = builder.Function("RateByOrder");
            function.Parameter<int>("order");
            function.Returns<string>();

            var action = builder.Action("RateByName");
            action.Parameter<string>("name");
            return builder.GetEdmModel();
        }
    }
}
