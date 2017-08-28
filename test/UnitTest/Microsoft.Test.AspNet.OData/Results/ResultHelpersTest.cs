// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Results;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.AspNet.OData.TestCommon;
using Moq;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.Test.AspNet.OData.Query.Results
{
    public class ResultHelpersTest
    {
        private readonly TestEntity _entity = new TestEntity();
        private readonly Uri _entityId = new Uri("http://entity_id");

        [Fact]
        public void GenerateODataLink_ThrowsIdLinkNullForEntityIdHeader_IfEntitySetLinkBuilderReturnsNull()
        {
            // Arrange
            var linkBuilder = new Mock<NavigationSourceLinkBuilderAnnotation>();
            var model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(TestEntity)));
            model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder.Object);
            var path = new ODataPath(new EntitySetSegment(model.Customers));
            var request = new HttpRequestMessage();
            request.ODataProperties().Path = path;
            request.EnableHttpDependencyInjectionSupport(model.Model);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => ResultHelpers.GenerateODataLink(request, _entity, isEntityId: true),
                "The Id link builder for the entity set 'Customers' returned null. An Id link is required for the OData-EntityId header.");
        }

        [Fact]
        public void AddEntityId_AddsEntityId_IfResponseStatusCodeIsNoContent()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.NoContent);

            // Act
            ResultHelpers.AddEntityId(response, () => _entityId);

            // Assert
            var entityIdHeaderValues = response.Headers.GetValues(ResultHelpers.EntityIdHeaderName).ToList();
            Assert.Single(entityIdHeaderValues);
            Assert.Equal(_entityId.ToString(), entityIdHeaderValues.Single());
        }

        [Fact]
        public void AddEntityId_DoesNotAddEntityId_IfResponseStatusCodeIsOtherThanNoContent()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            // Act
            ResultHelpers.AddEntityId(response, () => _entityId);

            // Assert
            Assert.False(response.Headers.Contains(ResultHelpers.EntityIdHeaderName));
        }

        private class TestEntity
        {
        }
    }
}
