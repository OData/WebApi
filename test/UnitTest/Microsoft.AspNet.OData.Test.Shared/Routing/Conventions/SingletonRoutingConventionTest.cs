//-----------------------------------------------------------------------------
// <copyright file="SingletonRoutingConventionTest.cs" company=".NET Foundation">
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
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#else
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#endif

namespace Microsoft.AspNet.OData.Test.Routing.Conventions
{
    public class SingletonRoutingConventionTest
    {
        private const string _serviceRoot = "http://any/";

#if NETCORE
        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissRouteContext()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new SingletonRoutingConvention().SelectAction(null),
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
                () => new SingletonRoutingConvention().SelectAction(routeContext),
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
                () => new SingletonRoutingConvention().SelectAction(null, controllerContext.Object, emptyMap),
                "odataPath");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissControllerContext()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new ODataPath(new SingletonSegment(model.VipCustomer));
            ILookup<string, HttpActionDescriptor> emptyMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new SingletonRoutingConvention().SelectAction(odataPath, null, emptyMap),
                "controllerContext");
        }

        [Fact]
        public void SelectAction_ThrowsArgumentNull_IfMissActionMap()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new ODataPath(new SingletonSegment(model.VipCustomer));
            Mock<HttpControllerContext> controllerContext = new Mock<HttpControllerContext>();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new SingletonRoutingConvention().SelectAction(odataPath, controllerContext.Object, null),
                "actionMap");
        }
#endif

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
            var request = RequestFactory.Create(new HttpMethod(httpMethod), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(actionName);

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new SingletonRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.NotNull(selectedAction);
            Assert.Equal(actionName, selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
        }

        [Theory]
        [InlineData("Post")]
        [InlineData("Delete")]
        public void SelectAction_ReturnsNull_ForInvalidHttpMethods(string httpMethod)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, _serviceRoot, "VipCustomer");
            var request = RequestFactory.Create(new HttpMethod(httpMethod), "http://localhost/");
            var emptyActionMap = SelectActionHelper.CreateActionMap();

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new SingletonRoutingConvention(), odataPath, request, emptyActionMap);

            // Act & Assert
            Assert.Null(selectedAction);
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
            var request = RequestFactory.Create(new HttpMethod(httpMethod), "http://localhost/");
            var emptyActionMap = SelectActionHelper.CreateActionMap();

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new EntitySetRoutingConvention(), odataPath, request, emptyActionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
        }
    }
}
