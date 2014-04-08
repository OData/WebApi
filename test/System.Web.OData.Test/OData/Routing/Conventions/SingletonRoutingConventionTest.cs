// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.OData.TestCommon;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing.Conventions
{
    public class SingletonRoutingConventionTest
    {
        private const string _serviceRoot = "http://any/";

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissOdataPath()
        {
            // Arrange
            Mock<HttpControllerContext> controllerContext = new Mock<HttpControllerContext>();
            ILookup<string, HttpActionDescriptor> emptyMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new SingletonRoutingConvention().SelectAction(null, controllerContext.Object, emptyMap),
                "odataPath");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissControllerContext()
        {
            // Arrange
            ODataPath odataPath = new ODataPath(new ODataPathSegment[] { new SingletonPathSegment("Boss") });
            ILookup<string, HttpActionDescriptor> emptyMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new SingletonRoutingConvention().SelectAction(odataPath, null, emptyMap),
                "controllerContext");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissActionMap()
        {
            // Arrange
            ODataPath odataPath = new ODataPath(new ODataPathSegment[] { new SingletonPathSegment("Boss") });
            Mock<HttpControllerContext> controllerContext = new Mock<HttpControllerContext>();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new SingletonRoutingConvention().SelectAction(odataPath, controllerContext.Object, null),
                "actionMap");
        }

        [Theory]
        [InlineData("GET", "Get")]
        [InlineData("PUT", "Put")]
        [InlineData("PATCH", "Patch")]
        [InlineData("MERGE", "Patch")]
        public void SelectAction_ReturnsTheActionName_ForValidHttpMethods(string httpMethod, string httpMethodNamePrefix)
        {
            // Arrange
            const string SingletonName = "VipCustomer";
            string actionName = httpMethodNamePrefix + SingletonName;
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, SingletonName);
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => actionName);
            HttpControllerContext controllerContext = new HttpControllerContext();
            controllerContext.Request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/");
            controllerContext.Request.SetRouteData(new HttpRouteData(new HttpRoute()));

            // Act
            string selectedAction = new SingletonRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal(actionName, selectedAction);
            Assert.Empty(controllerContext.Request.GetRouteData().Values);
        }

        [Theory]
        [InlineData("Post")]
        [InlineData("Delete")]
        public void SelectAction_ReturnsNull_ForInvalidHttpMethods(string httpMethod)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "VipCustomer");
            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);
            HttpControllerContext controllerContext = new HttpControllerContext();
            controllerContext.Request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/");
            controllerContext.Request.SetRouteData(new HttpRouteData(new HttpRoute()));

            // Act
            string actionName = new SingletonRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Act & Assert
            Assert.Null(actionName);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("PUT")]
        [InlineData("PATCH")]
        [InlineData("MERGE")]
        public void SelectAction_ReturnsNull_IfActionIsMissing(string httpMethod)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "VipCustomer");
            ILookup<string, HttpActionDescriptor> emptyActionMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);
            HttpControllerContext controllerContext = new HttpControllerContext();
            controllerContext.Request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/");
            controllerContext.Request.SetRouteData(new HttpRouteData(new HttpRoute()));

            // Act
            string selectedAction = new EntitySetRoutingConvention().SelectAction(odataPath, controllerContext, emptyActionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(controllerContext.Request.GetRouteData().Values);
        }
    }
}
