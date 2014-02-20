// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing.Conventions
{
    public class ActionRoutingConventionTest
    {
        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfODataPathIsNull()
        {
            // Arrange
            ActionRoutingConvention actionConvention = new ActionRoutingConvention();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => actionConvention.SelectAction(odataPath: null, controllerContext: null, actionMap: null),
                "odataPath");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfControllerContextIsNull()
        {
            // Arrange
            ActionRoutingConvention actionConvention = new ActionRoutingConvention();
            ODataPath odataPath = new ODataPath();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => actionConvention.SelectAction(odataPath, controllerContext: null, actionMap: null),
                "controllerContext");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfActionMapIsNull()
        {
            // Arrange
            ActionRoutingConvention actionConvention = new ActionRoutingConvention();
            ODataPath odataPath = new ODataPath();
            HttpControllerContext controllerContext = new HttpControllerContext();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => actionConvention.SelectAction(odataPath, controllerContext, actionMap: null),
                "actionMap");
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("MERGE")]
        [InlineData("PATCH")]
        public void SelectAction_ReturnsNull_RequestMethodIsNotPost(string requestMethod)
        {
            // Arrange
            ActionRoutingConvention actionConvention = new ActionRoutingConvention();
            ODataPath odataPath = new ODataPath();
            HttpControllerContext controllerContext = new HttpControllerContext();
            controllerContext.Request = new HttpRequestMessage(new HttpMethod(requestMethod), "http://localhost/");
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);

            // Act
            string selectedAction = actionConvention.SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Null(selectedAction);
        }

        [Fact]
        public void SelectAction_ReturnsTheActionName_ForActionBoundToEntitySet()
        {
            // Arrange
            ActionRoutingConvention actionConvention = new ActionRoutingConvention();
            IEdmModel model = ODataRoutingModel.GetModel();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model, "RoutingCustomers/Default.GetVIPs");
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/"),
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };
            controllerContext.Request.SetRequestContext(requestContext);
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => "GetVIPs");

            // Act
            string action = actionConvention.SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Equal("GetVIPs", action);
            Assert.Equal(0, controllerContext.Request.GetRouteData().Values.Count);
        }

        [Theory]
        [InlineData("RoutingCustomers(1)/Default.GetRelatedRoutingCustomers")]
        [InlineData("RoutingCustomers/Default.GetProducts")]
        public void SelectAction_ReturnsNull_IfActionIsMissing(string path)
        {
            ODataPath odataPath = new DefaultODataPathHandler().Parse(ODataRoutingModel.GetModel(), path);
            ILookup<string, HttpActionDescriptor> emptyActionMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);
            HttpControllerContext controllerContext = new HttpControllerContext();
            controllerContext.Request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/");
            controllerContext.Request.SetRouteData(new HttpRouteData(new HttpRoute()));

            string selectedAction = new ActionRoutingConvention().SelectAction(odataPath, controllerContext, emptyActionMap);

            Assert.Null(selectedAction);
            Assert.Empty(controllerContext.Request.GetRouteData().Values);
        }
    }
}
