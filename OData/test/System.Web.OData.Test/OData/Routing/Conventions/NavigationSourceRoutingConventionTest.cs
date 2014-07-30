// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.OData.TestCommon;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing.Conventions
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
            ODataPath odataPath = new ODataPath(new ODataPathSegment[] { new EntitySetPathSegment("Customers") });

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
            ODataPath odataPath = new ODataPath(new ODataPathSegment[] { new RefPathSegment() });

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
