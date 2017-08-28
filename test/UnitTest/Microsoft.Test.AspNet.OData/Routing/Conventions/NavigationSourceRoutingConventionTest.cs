// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.AspNet.OData.TestCommon;
using Moq;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.Test.AspNet.OData.Routing.Conventions
{
    public class NavigationSourceRoutingConventionTest
    {
        [Fact]
        public void SelectController_ThrowsArgmentNull_IfMissOdataPath()
        {
            // Arrange
            Mock<HttpRequestMessage> request = new Mock<HttpRequestMessage>();

            // Act & Assert
            Assert.ThrowsArgumentNull(
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
            Assert.ThrowsArgumentNull(
                () => new MockNavigationSourceRoutingConvention().SelectController(odataPath, null),
                "request");
        }

        [Fact]
        public void SelectController_RetrunsNull_IfNotNavigationSourceRequest()
        {
            // Arrange
            Mock<HttpRequestMessage> request = new Mock<HttpRequestMessage>();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            var ordersProperty = model.Customer.FindProperty("Orders") as IEdmNavigationProperty;
            NavigationPropertyLinkSegment navigationLinkSegment = new NavigationPropertyLinkSegment(ordersProperty, model.Orders);
            ODataPath odataPath = new ODataPath(navigationLinkSegment);

            // Act
            string controller = new MockNavigationSourceRoutingConvention().SelectController(odataPath, request.Object);

            // Assert
            Assert.Null(controller);
        }

        [Fact]
        public void SelectController_RetrunsEntitySetName_ForEntitySetRequest()
        {
            // Arrange
            Mock<HttpRequestMessage> request = new Mock<HttpRequestMessage>();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://any/", "Customers");

            // Act
            string controller = new MockNavigationSourceRoutingConvention().SelectController(odataPath, request.Object);

            // Assert
            Assert.Equal("Customers", controller);
        }

        [Fact]
        public void SelectController_RetrunsSingletonName_ForSingletonRequest()
        {
            // Arrange
            Mock<HttpRequestMessage> request = new Mock<HttpRequestMessage>();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new DefaultODataPathHandler().Parse(model.Model, "http://any/", "VipCustomer");

            // Act
            string controller = new MockNavigationSourceRoutingConvention().SelectController(odataPath, request.Object);

            // Assert
            Assert.Equal("VipCustomer", controller);
        }
    }
}
