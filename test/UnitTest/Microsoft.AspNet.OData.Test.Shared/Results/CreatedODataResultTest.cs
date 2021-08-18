//-----------------------------------------------------------------------------
// <copyright file="CreatedODataResultTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Net;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#else
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Results;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#endif

namespace Microsoft.AspNet.OData.Test.Results
{
    public class CreatedODataResultTest
    {
        private readonly TestEntity _entity = new TestEntity();
#if NETFX // Only needed for AspNet
        private readonly HttpRequestMessage _request = new HttpRequestMessage();
        private readonly IContentNegotiator _contentNegotiator = new Mock<IContentNegotiator>().Object;
        private readonly IEnumerable<MediaTypeFormatter> _formatters = new MediaTypeFormatter[0];
        private readonly Uri _locationHeader = new Uri("http://location_header");
        private readonly TestController _controller = new TestController();
#endif

#if NETCORE
        [Fact]
        public void Ctor_ControllerDependency_ThrowsArgumentNull_Entity()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new CreatedODataResult<TestEntity>(entity: null), "entity");
        }

        [Fact]
        public void GetEntity_ReturnsCorrect()
        {
            // Arrange
            Mock<CreatedODataResultTest> mock = new Mock<CreatedODataResultTest>();
            CreatedODataResult<CreatedODataResultTest> result =
                new CreatedODataResult<CreatedODataResultTest>(mock.Object);

            // Act & Assert
            Assert.Same(mock.Object, result.Entity);
        }
#else
        [Fact]
        public void Ctor_ControllerDependency_ThrowsArgumentNull_Entity()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new CreatedODataResult<TestEntity>(entity: null, controller: _controller), "entity");
        }

        [Fact]
        public void Ctor_ControllerDependency_ThrowsArgumentNull_Controller()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new CreatedODataResult<TestEntity>(_entity, controller: null), "controller");
        }

        [Fact]
        public void Ctor_DirectDependency_ThrowsArgumentNull_Entity()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new CreatedODataResult<TestEntity>(entity: null, contentNegotiator: _contentNegotiator,
                    request: _request, formatters: _formatters, locationHeader: _locationHeader), "entity");
        }

        [Fact]
        public void Ctor_DirectDependency_ThrowsArgumentNull_ContentNegotiator()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new CreatedODataResult<TestEntity>(_entity, contentNegotiator: null,
                    request: _request, formatters: _formatters, locationHeader: _locationHeader), "contentNegotiator");
        }

        [Fact]
        public void Ctor_DirectDependency_ThrowsArgumentNull_Request()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new CreatedODataResult<TestEntity>(_entity, _contentNegotiator,
                    request: null, formatters: _formatters, locationHeader: _locationHeader), "request");
        }

        [Fact]
        public void Ctor_DirectDependency_ThrowsArgumentNull_Formatters()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new CreatedODataResult<TestEntity>(_entity, _contentNegotiator, _request,
                    formatters: null, locationHeader: _locationHeader), "formatters");
        }

        [Fact]
        public void Ctor_DirectDependency_ThrowsArgumentNull_LocationHeader()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new CreatedODataResult<TestEntity>(_entity, _contentNegotiator, _request, _formatters,
                    locationHeader: null), "locationHeader");
        }

        [Fact]
        public void Property_Entity_DirectDependency()
        {
            CreatedODataResult<TestEntity> result = new CreatedODataResult<TestEntity>(
                _entity, _contentNegotiator, _request, _formatters, _locationHeader);

            Assert.Same(_entity, result.Entity);
        }

        [Fact]
        public void Property_ContentNegotiator_DirectDependency()
        {
            CreatedODataResult<TestEntity> result = new CreatedODataResult<TestEntity>(
                _entity, _contentNegotiator, _request, _formatters, _locationHeader);

            Assert.Same(_contentNegotiator, result.ContentNegotiator);
        }

        [Fact]
        public void Property_ContentNegotiator_Request()
        {
            CreatedODataResult<TestEntity> result = new CreatedODataResult<TestEntity>(
                _entity, _contentNegotiator, _request, _formatters, _locationHeader);

            Assert.Same(_request, result.Request);
        }

        [Fact]
        public void Property_ContentNegotiator_Formatters()
        {
            CreatedODataResult<TestEntity> result = new CreatedODataResult<TestEntity>(
                _entity, _contentNegotiator, _request, _formatters, _locationHeader);

            Assert.Same(_formatters, result.Formatters);
        }

        [Fact]
        public void Property_ContentNegotiator_LocationHeader()
        {
            CreatedODataResult<TestEntity> result = new CreatedODataResult<TestEntity>(
                _entity, _contentNegotiator, _request, _formatters, _locationHeader);

            Assert.Same(_locationHeader, result.LocationHeader);
        }
#endif

        [Fact]
        public void GetInnerActionResult_ReturnsNegotiatedContentResult_IfRequestHasNoPreferenceHeader()
        {
            // Arrange
            var request = CreateRequest();
            var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request);

            // Act
            var result = createdODataResult.GetInnerActionResult(request);

            // Assert
#if NETCORE
            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Same(typeof(TestEntity), objectResult.Value.GetType());
            Assert.Equal(HttpStatusCode.Created, (HttpStatusCode)objectResult.StatusCode);
#else
            NegotiatedContentResult<TestEntity> negotiatedResult = Assert.IsType<NegotiatedContentResult<TestEntity>>(result);
            Assert.Equal(HttpStatusCode.Created, negotiatedResult.StatusCode);
            Assert.Same(request, negotiatedResult.Request);
            Assert.Same(_contentNegotiator, negotiatedResult.ContentNegotiator);
            Assert.Same(_entity, negotiatedResult.Content);
            Assert.Same(_formatters, negotiatedResult.Formatters);
#endif
        }

        [Fact]
        public void GetInnerActionResult_ReturnsNoContentStatusCodeResult_IfRequestAsksForNoContent()
        {
            // Arrange
            var request = CreateRequest("return=minimal");
            var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request);

            // Act
            var result = createdODataResult.GetInnerActionResult(request);

            // Assert
            StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(HttpStatusCode.NoContent, (HttpStatusCode)statusCodeResult.StatusCode);
#if NETFX // Only needed for AspNet
            Assert.Same(request, statusCodeResult.Request);
#endif
        }

        [Fact]
        public void GetInnerActionResult_ReturnsNegotiatedContentResult_IfRequestAsksForContent()
        {
            // Arrange
            var request = CreateRequest("return=representation");
            var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request);

            // Act
            var result = createdODataResult.GetInnerActionResult(request);

            // Assert
#if NETCORE
            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Same(typeof(TestEntity), objectResult.Value.GetType());
            Assert.Equal(HttpStatusCode.Created, (HttpStatusCode)objectResult.StatusCode);
#else
            NegotiatedContentResult<TestEntity> negotiatedResult = Assert.IsType<NegotiatedContentResult<TestEntity>>(result);
            Assert.Equal(HttpStatusCode.Created, negotiatedResult.StatusCode);
            Assert.Same(request, negotiatedResult.Request);
            Assert.Same(_contentNegotiator, negotiatedResult.ContentNegotiator);
            Assert.Same(_entity, negotiatedResult.Content);
            Assert.Same(_formatters, negotiatedResult.Formatters);
#endif
        }

        [Fact]
        public void GenerateLocationHeader_ThrowsODataPathMissing_IfRequestDoesNotHaveODataPath()
        {
            var request = RequestFactory.CreateFromModel(EdmCoreModel.Instance);
            var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => createdODataResult.GenerateLocationHeader(request),
                "The operation cannot be completed because no ODataPath is available for the request.");
        }

        [Fact]
        public void GenerateLocationHeader_ThrowsEntitySetMissingDuringSerialization_IfODataPathEntitySetIsNull()
        {
            // Arrange
            ODataPath path = new ODataPath();
            var request = RequestFactory.CreateFromModel(EdmCoreModel.Instance, path: path);
            var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => createdODataResult.GenerateLocationHeader(request),
                "The related entity set or singleton cannot be found from the OData path. The related entity set or singleton is required to serialize the payload.");
        }

        [Fact]
        public void GenerateLocationHeader_ThrowsEntityTypeNotInModel_IfContentTypeIsNotThereInModel()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath path = new ODataPath(new EntitySetSegment(model.Customers));
            var request = RequestFactory.CreateFromModel(model.Model, path: path);
            var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => createdODataResult.GenerateLocationHeader(request),
                "Cannot find the resource type 'Microsoft.AspNet.OData.Test.Results.CreatedODataResultTest+TestEntity' in the model.");
        }

        [Fact]
        public void GenerateLocationHeader_ThrowsTypeMustBeEntity_IfMappingTypeIsNotEntity()
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath path = new ODataPath(new EntitySetSegment(model.Customers));
            var request = RequestFactory.CreateFromModel(model.Model, path: path);
            model.Model.SetAnnotationValue(model.Address, new ClrTypeAnnotation(typeof(TestEntity)));
            var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => createdODataResult.GenerateLocationHeader(request),
                "NS.Address is not an entity type. Only entity types are supported.");
        }

        [Fact]
        public void GenerateLocationHeader_UsesEntitySetLinkBuilder_ToGenerateLocationHeader()
        {
            // Arrange
            Uri editLink = new Uri("http://id-link");
            Mock<NavigationSourceLinkBuilderAnnotation> linkBuilder = new Mock<NavigationSourceLinkBuilderAnnotation>();
            linkBuilder.CallBase = true;
            linkBuilder.Setup(
                b => b.BuildEditLink(It.IsAny<ResourceContext>(), ODataMetadataLevel.FullMetadata, null))
                .Returns(editLink);

            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(TestEntity)));
            model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder.Object);
            ODataPath path = new ODataPath(new EntitySetSegment(model.Customers));
            var request = RequestFactory.CreateFromModel(model.Model, path: path);
            var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request);

            // Act
            var locationHeader = createdODataResult.GenerateLocationHeader(request);

            // Assert
            Assert.Same(editLink, locationHeader);
        }

        [Fact]
        public void GenerateLocationHeader_ForContainment()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.OrderLine, new ClrTypeAnnotation(typeof(OrderLine)));
            var path = new DefaultODataPathHandler().Parse(
                model.Model,
                "http://localhost/",
                "MyOrders(1)/OrderLines");
            var request = RequestFactory.CreateFromModel(model.Model, path: path);
            var orderLine = new OrderLine { ID = 2 };
            var createdODataResult = GetCreatedODataResult<OrderLine>(orderLine, request);

            // Act
            var locationHeader = createdODataResult.GenerateLocationHeader(request);

            // Assert
            Assert.Equal("http://localhost/MyOrders(1)/OrderLines(2)", locationHeader.ToString());
        }

        [Fact]
        public void GenerateLocationHeader_ThrowsEditLinkNullForLocationHeader_IfEntitySetLinkBuilderReturnsNull()
        {
            // Arrange
            Mock<NavigationSourceLinkBuilderAnnotation> linkBuilder = new Mock<NavigationSourceLinkBuilderAnnotation>();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(TestEntity)));
            model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder.Object);
            ODataPath path = new ODataPath(new EntitySetSegment(model.Customers));
            var request = RequestFactory.CreateFromModel(model.Model, path: path);
            var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request);

            // Act
            ExceptionAssert.Throws<InvalidOperationException>(() => createdODataResult.GenerateLocationHeader(request),
                "The edit link builder for the entity set 'Customers' returned null. An edit link is required for the location header.");
        }

        [Fact]
        public void Property_LocationHeader_IsEvaluatedLazily()
        {
            // Arrange
            Uri editLink = new Uri("http://edit-link");
            Mock<NavigationSourceLinkBuilderAnnotation> linkBuilder = new Mock<NavigationSourceLinkBuilderAnnotation>();
            linkBuilder.Setup(b => b.BuildEditLink(It.IsAny<ResourceContext>(), ODataMetadataLevel.FullMetadata, null))
                .Returns(editLink);

            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(TestEntity)));
            model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder.Object);
            ODataPath path = new ODataPath(new EntitySetSegment(model.Customers));
            var request = RequestFactory.CreateFromModel(model.Model, path: path);
            TestController controller = CreateController(request);
            var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request, controller);

            // Act
            Uri locationHeader = createdODataResult.GenerateLocationHeader(request);

            // Assert
            Assert.Same(editLink, locationHeader);
        }

        [Fact]
        public void Property_LocationHeader_IsEvaluatedOnlyOnce()
        {
            // Arrange
            Uri editLink = new Uri("http://edit-link");
            Mock<NavigationSourceLinkBuilderAnnotation> linkBuilder = new Mock<NavigationSourceLinkBuilderAnnotation>();
            linkBuilder.Setup(b => b.BuildEditLink(It.IsAny<ResourceContext>(), ODataMetadataLevel.FullMetadata, null))
                .Returns(editLink);

            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(TestEntity)));
            model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder.Object);
            ODataPath path = new ODataPath(new EntitySetSegment(model.Customers));
            var request = RequestFactory.CreateFromModel(model.Model, path: path);
            TestController controller = CreateController(request);
            var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request, controller);

            // Act
            Uri locationHeader = createdODataResult.GenerateLocationHeader(request);

            // Assert
            linkBuilder.Verify(
                (b) => b.BuildEditLink(It.IsAny<ResourceContext>(), ODataMetadataLevel.FullMetadata, null),
                Times.Once());
        }

        [Fact]
        public void Property_EntityIdHeader_IsEvaluatedLazilyAndOnlyOnce()
        {
            // Arrange
            Uri idLink = new Uri("http://id-link");
            Mock<NavigationSourceLinkBuilderAnnotation> linkBuilder = new Mock<NavigationSourceLinkBuilderAnnotation>();
            linkBuilder.CallBase = true;
            linkBuilder.Setup(b => b.BuildIdLink(It.IsAny<ResourceContext>(), ODataMetadataLevel.FullMetadata))
                .Returns(idLink);

            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(TestEntity)));
            model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder.Object);
            ODataPath path = new ODataPath(new EntitySetSegment(model.Customers));
            var request = RequestFactory.CreateFromModel(model.Model, path: path);
            TestController controller = CreateController(request);
            var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request, controller);

            // Act
            Uri entityIdHeader = createdODataResult.GenerateEntityId(request);

            // Assert
            Assert.Same(idLink, entityIdHeader);
            linkBuilder.Verify(
                b => b.BuildIdLink(It.IsAny<ResourceContext>(), ODataMetadataLevel.FullMetadata),
                Times.Once());
        }

        private class TestEntity
        {
        }

        private class TestController : ODataController
        {
        }

#if NETCORE
        private CreatedODataResult<T> GetCreatedODataResult<T>(T entity, HttpRequest request, TestController controller = null)
        {
            return new CreatedODataResult<T>(entity);
        }

        private HttpRequest CreateRequest(string preferHeaderValue = null)
        {
            var request = RequestFactory.Create();
            if (!string.IsNullOrEmpty(preferHeaderValue))
            {
                request.Headers.Add("Prefer", new StringValues(preferHeaderValue));
            }

            return request;
        }

        private TestController CreateController(AspNetCore.Http.HttpRequest request)
        {
            TestController controller = new TestController();
            return controller;
        }
#else
        private CreatedODataResult<T> GetCreatedODataResult<T>(T entity, HttpRequestMessage request, TestController controller = null)
        {
            if (controller != null)
            {
                return new CreatedODataResult<T>(entity, controller);
            }
            else
            {
                return new CreatedODataResult<T>(entity, _contentNegotiator, request, _formatters, _locationHeader);
            }
        }

        private HttpRequestMessage CreateRequest(string preferHeaderValue = null)
        {
            var request = RequestFactory.Create();
            if (!string.IsNullOrEmpty(preferHeaderValue))
            {
                request = new HttpRequestMessage();
                request.Headers.TryAddWithoutValidation("Prefer", preferHeaderValue);
            }

            return request;
        }

        private TestController CreateController(HttpRequestMessage request)
        {
            TestController controller = new TestController();
            controller.Configuration = request.GetConfiguration();
            controller.Request = request;
            return controller;
        }
#endif
    }
}
