//-----------------------------------------------------------------------------
// <copyright file="ResultHelpersTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test.Results
{
    public class ResultHelpersTest
    {
        private readonly TestEntity _entity = new TestEntity();
        private readonly Uri _entityId = new Uri("http://entity_id");
        private readonly string _version = "4.0.1.101";

        [Fact]
        public void GenerateODataLink_ThrowsIdLinkNullForEntityIdHeader_IfEntitySetLinkBuilderReturnsNull()
        {
            // Arrange
            var linkBuilder = new Mock<NavigationSourceLinkBuilderAnnotation>();
            var model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(TestEntity)));
            model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder.Object);
            var path = new ODataPath(new EntitySetSegment(model.Customers));
            var request = RequestFactory.CreateFromModel(model.Model, path: path);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => ResultHelpers.GenerateODataLink(request, _entity, isEntityId: true),
                "The Id link builder for the entity set 'Customers' returned null. An Id link is required for the OData-EntityId header.");
        }

        [Fact]
        public void AddEntityId_AddsEntityId_IfResponseStatusCodeIsNoContent()
        {
            // Arrange
            var response = ResponseFactory.Create(HttpStatusCode.NoContent);

            // Act
            ResultHelpers.AddEntityId(response, () => _entityId);

            // Assert
#if NETCORE
            var entityIdHeaderValues = response.Headers[ResultHelpers.EntityIdHeaderName].ToList();
#else
            var entityIdHeaderValues = response.Headers.GetValues(ResultHelpers.EntityIdHeaderName).ToList();
#endif
            Assert.Single(entityIdHeaderValues);
            Assert.Equal(_entityId.ToString(), entityIdHeaderValues.Single());
        }

        [Fact]
        public void AddEntityId_DoesNotAddEntityId_IfResponseStatusCodeIsOtherThanNoContent()
        {
            // Arrange
            var response = ResponseFactory.Create(HttpStatusCode.OK);

            // Act
            ResultHelpers.AddEntityId(response, () => _entityId);

            // Assert
#if NETCORE
            Assert.False(response.Headers.ContainsKey(ResultHelpers.EntityIdHeaderName));
#else
            Assert.False(response.Headers.Contains(ResultHelpers.EntityIdHeaderName));
#endif
        }

        [Fact]
        public void AddServiceVersion_AddsODataVersion_IfResponseStatusCodeIsNoContent()
        {
            // Arrange
            var response = ResponseFactory.Create(HttpStatusCode.NoContent);

            // Act
            ResultHelpers.AddServiceVersion(response, () => _version);

            // Assert
#if NETCORE
            var versionHeaderValues = response.Headers[ODataVersionConstraint.ODataServiceVersionHeader].ToList();
#else
            var versionHeaderValues = response.Headers.GetValues(ODataVersionConstraint.ODataServiceVersionHeader).ToList();
#endif
            Assert.Single(versionHeaderValues);
            Assert.Equal(_version, versionHeaderValues.Single());
        }

        [Fact]
        public void AddServiceVersion_DoesNotAddServiceVersion_IfResponseStatusCodeIsOtherThanNoContent()
        {
            // Arrange
            var response = ResponseFactory.Create(HttpStatusCode.OK);

            // Act
            ResultHelpers.AddServiceVersion(response, () => _version);

            // Assert
#if NETCORE
            Assert.False(response.Headers.ContainsKey(ODataVersionConstraint.ODataServiceVersionHeader));
#else
            Assert.False(response.Headers.Contains(ODataVersionConstraint.ODataServiceVersionHeader));
#endif
        }

        private class TestEntity
        {
        }
    }
}
