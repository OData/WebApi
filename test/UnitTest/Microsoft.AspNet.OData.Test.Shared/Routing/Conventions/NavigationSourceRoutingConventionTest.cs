//-----------------------------------------------------------------------------
// <copyright file="NavigationSourceRoutingConventionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System.Linq;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#else
using System.Net.Http;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#endif

namespace Microsoft.AspNet.OData.Test.Routing.Conventions
{
    public class NavigationSourceRoutingConventionTest
    {
#if NETCORE
        [Fact]
        public void SelectController_ThrowsArgmentNull_IfMissRouteContext()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new MockNavigationSourceRoutingConvention().SelectAction(null),
                "routeContext");
        }

        [Fact]
        public void SelectController_ThrowsArgmentNull_IfMissOdataPath()
        {
            // Arrange
            var request = RequestFactory.Create();
            var routeContext = new RouteContext(request.HttpContext);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new MockNavigationSourceRoutingConvention().SelectAction(routeContext),
                "odataPath");
        }
#else
        [Fact]
        public void SelectController_ThrowsArgmentNull_IfMissOdataPath()
        {
            // Arrange
            Mock<HttpRequestMessage> request = new Mock<HttpRequestMessage>();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new MockNavigationSourceRoutingConvention().SelectController(null, request.Object),
                "odataPath");
        }

        [Fact]
        public void SelectController_ThrowsArgmentNull_IfMissRequest()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new ODataPath(new EntitySetSegment(model.Customers));

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new MockNavigationSourceRoutingConvention().SelectController(odataPath, null),
                "request");
        }
#endif

        [Fact]
        public void SelectController_RetrunsNull_IfNotNavigationSourceRequest()
        {
            // Arrange
            var request = RequestFactory.Create();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var ordersProperty = model.Customer.FindProperty("Orders") as IEdmNavigationProperty;
            NavigationPropertyLinkSegment navigationLinkSegment = new NavigationPropertyLinkSegment(ordersProperty, model.Orders);
            ODataPath odataPath = new ODataPath(navigationLinkSegment);

            // Act
            string controller = SelectController(new MockNavigationSourceRoutingConvention(), odataPath, request);

            // Assert
            Assert.Null(controller);
        }

#if NETFX // AspNetCore version returns the action descriptor.
        [Fact]
        public void SelectController_RetrunsEntitySetName_ForEntitySetRequest()
        {
            // Arrange
            var request = RequestFactory.Create();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://any/", "Customers");

            // Act
            string controller = SelectController(new MockNavigationSourceRoutingConvention(), odataPath, request);

            // Assert
            Assert.Equal("Customers", controller);
        }

        [Fact]
        public void SelectController_ReturnsSingletonName_ForSingletonRequest()
        {
            // Arrange
            var request = RequestFactory.Create();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://any/", "VipCustomer");

            // Act
            string controller = SelectController(new MockNavigationSourceRoutingConvention(), odataPath, request);

            // Assert
            Assert.Equal("VipCustomer", controller);
        }
#endif

#if NETCORE
        private string SelectController(NavigationSourceRoutingConvention convention, ODataPath odataPath, HttpRequest request)
        {
            RouteContext routeContext = new RouteContext(request.HttpContext);
            routeContext.HttpContext.ODataFeature().Path = odataPath;

            ControllerActionDescriptor descriptor = convention.SelectAction(routeContext)?.FirstOrDefault();
            return descriptor?.ControllerName;
        }
#else
        private string SelectController(NavigationSourceRoutingConvention convention, ODataPath odataPath, HttpRequestMessage request)
        {
            return convention.SelectController(odataPath, request);
        }
#endif
    }
}
