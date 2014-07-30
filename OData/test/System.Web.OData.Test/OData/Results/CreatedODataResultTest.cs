// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.Routing;
using System.Web.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Results
{
    public class CreatedODataResultTest
    {
        private readonly TestEntity _entity = new TestEntity();
        private readonly HttpRequestMessage _request = new HttpRequestMessage();
        private readonly IContentNegotiator _contentNegotiator = new Mock<IContentNegotiator>().Object;
        private readonly IEnumerable<MediaTypeFormatter> _formatters = new MediaTypeFormatter[0];
        private readonly Uri _locationHeader = new Uri("http://location_header");
        private readonly TestController _controller = new TestController();

        [Fact]
        public void Ctor_ControllerDependency_ThrowsArgumentNull_Entity()
        {
            Assert.ThrowsArgumentNull(
                () => new CreatedODataResult<TestEntity>(entity: null, controller: _controller), "entity");
        }

        [Fact]
        public void Ctor_ControllerDependency_ThrowsArgumentNull_Controller()
        {
            Assert.ThrowsArgumentNull(
                () => new CreatedODataResult<TestEntity>(_entity, controller: null), "controller");
        }

        [Fact]
        public void Ctor_DirectDependency_ThrowsArgumentNull_Entity()
        {
            Assert.ThrowsArgumentNull(
                () => new CreatedODataResult<TestEntity>(entity: null, contentNegotiator: _contentNegotiator,
                    request: _request, formatters: _formatters, locationHeader: _locationHeader), "entity");
        }

        [Fact]
        public void Ctor_DirectDependency_ThrowsArgumentNull_ContentNegotiator()
        {
            Assert.ThrowsArgumentNull(
                () => new CreatedODataResult<TestEntity>(_entity, contentNegotiator: null,
                    request: _request, formatters: _formatters, locationHeader: _locationHeader), "contentNegotiator");
        }

        [Fact]
        public void Ctor_DirectDependency_ThrowsArgumentNull_Request()
        {
            Assert.ThrowsArgumentNull(
                () => new CreatedODataResult<TestEntity>(_entity, _contentNegotiator,
                    request: null, formatters: _formatters, locationHeader: _locationHeader), "request");
        }

        [Fact]
        public void Ctor_DirectDependency_ThrowsArgumentNull_Formatters()
        {
            Assert.ThrowsArgumentNull(
                () => new CreatedODataResult<TestEntity>(_entity, _contentNegotiator, _request,
                    formatters: null, locationHeader: _locationHeader), "formatters");
        }

        [Fact]
        public void Ctor_DirectDependency_ThrowsArgumentNull_LocationHeader()
        {
            Assert.ThrowsArgumentNull(
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

        [Fact]
        public void GetInnerActionResult_ReturnsNegotiatedContentResult_IfRequestHasNoPreferenceHeader()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            CreatedODataResult<TestEntity> createdODataResult =
                new CreatedODataResult<TestEntity>(_entity, _contentNegotiator, request, _formatters, _locationHeader);

            // Act
            IHttpActionResult result = createdODataResult.GetInnerActionResult();

            // Assert
            NegotiatedContentResult<TestEntity> negotiatedResult = Assert.IsType<NegotiatedContentResult<TestEntity>>(result);
            Assert.Equal(HttpStatusCode.Created, negotiatedResult.StatusCode);
            Assert.Same(request, negotiatedResult.Request);
            Assert.Same(_contentNegotiator, negotiatedResult.ContentNegotiator);
            Assert.Same(_entity, negotiatedResult.Content);
            Assert.Same(_formatters, negotiatedResult.Formatters);
        }

        [Fact]
        public void GetInnerActionResult_ReturnsNoContentStatusCodeResult_IfRequestAsksForNoContent()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.TryAddWithoutValidation("Prefer", "return=minimal");
            CreatedODataResult<TestEntity> createdODataResult =
                new CreatedODataResult<TestEntity>(_entity, _contentNegotiator, request, _formatters, _locationHeader);

            // Act
            IHttpActionResult result = createdODataResult.GetInnerActionResult();

            // Assert
            StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(HttpStatusCode.NoContent, statusCodeResult.StatusCode);
            Assert.Same(request, statusCodeResult.Request);
        }

        [Fact]
        public void GetInnerActionResult_ReturnsNegotiatedContentResult_IfRequestAsksForContent()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.TryAddWithoutValidation("Prefer", "return=representation");
            CreatedODataResult<TestEntity> createdODataResult =
                new CreatedODataResult<TestEntity>(_entity, _contentNegotiator, request, _formatters, _locationHeader);

            // Act
            IHttpActionResult result = createdODataResult.GetInnerActionResult();

            // Assert
            NegotiatedContentResult<TestEntity> negotiatedResult = Assert.IsType<NegotiatedContentResult<TestEntity>>(result);
            Assert.Equal(HttpStatusCode.Created, negotiatedResult.StatusCode);
            Assert.Same(request, negotiatedResult.Request);
            Assert.Same(_contentNegotiator, negotiatedResult.ContentNegotiator);
            Assert.Same(_entity, negotiatedResult.Content);
            Assert.Same(_formatters, negotiatedResult.Formatters);
        }

        [Fact]
        public void GenerateLocationHeader_ThrowsRequestMustHaveModel_IfRequestDoesNotHaveModel()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            CreatedODataResult<TestEntity> createdODataResult = GetCreatedODataResult(request);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => createdODataResult.GenerateLocationHeader(),
                "The request must have an associated EDM model. Consider using the extension method " +
                "HttpConfiguration.Routes.MapODataServiceRoute to register a route that parses the OData URI and " +
                "attaches the model information.");
        }

        [Fact]
        public void GenerateLocationHeader_ThrowsODataPathMissing_IfRequestDoesNotHaveODataPath()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().Model = EdmCoreModel.Instance;
            CreatedODataResult<TestEntity> createdODataResult = GetCreatedODataResult(request);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => createdODataResult.GenerateLocationHeader(),
                "The operation cannot be completed because no ODataPath is available for the request.");
        }

        [Fact]
        public void GenerateLocationHeader_ThrowsEntitySetMissingDuringSerialization_IfODataPathEntitySetIsNull()
        {
            ODataPath path = new ODataPath();
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().Model = EdmCoreModel.Instance;
            request.ODataProperties().Path = path;
            CreatedODataResult<TestEntity> createdODataResult = GetCreatedODataResult(request);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => createdODataResult.GenerateLocationHeader(),
                "The related entity set or singleton cannot be found from the OData path. The related entity set or singleton is required to serialize the payload.");
        }

        [Fact]
        public void GenerateLocationHeader_ThrowsEntityTypeNotInModel_IfContentTypeIsNotThereInModel()
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath path = new ODataPath(new EntitySetPathSegment(model.Customers));
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().Model = model.Model;
            request.ODataProperties().Path = path;
            CreatedODataResult<TestEntity> createdODataResult = GetCreatedODataResult(request);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => createdODataResult.GenerateLocationHeader(),
                "Cannot find the entity type 'System.Web.OData.Results.CreatedODataResultTest+TestEntity' in the model.");
        }

        [Fact]
        public void GenerateLocationHeader_ThrowsTypeMustBeEntity_IfMappingTypeIsNotEntity()
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath path = new ODataPath(new EntitySetPathSegment(model.Customers));
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().Model = model.Model;
            request.ODataProperties().Path = path;
            model.Model.SetAnnotationValue(model.Address, new ClrTypeAnnotation(typeof(TestEntity)));
            CreatedODataResult<TestEntity> createdODataResult = GetCreatedODataResult(request);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => createdODataResult.GenerateLocationHeader(),
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
                b => b.BuildEditLink(It.IsAny<EntityInstanceContext>(), ODataMetadataLevel.FullMetadata, null))
                .Returns(editLink);

            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(TestEntity)));
            model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder.Object);
            ODataPath path = new ODataPath(new EntitySetPathSegment(model.Customers));
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().Model = model.Model;
            request.ODataProperties().Path = path;
            CreatedODataResult<TestEntity> createdODataResult = GetCreatedODataResult(request);

            // Act
            var locationHeader = createdODataResult.GenerateLocationHeader();

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
            var request = GetODataRequest(model.Model);
            request.ODataProperties().Model = model.Model;
            request.ODataProperties().Path = path;
            var orderLine = new OrderLine { ID = 2 };
            var createdODataResult = new CreatedODataResult<OrderLine>(
                orderLine,
                _contentNegotiator,
                request,
                _formatters,
                _locationHeader);

            // Act
            var locationHeader = createdODataResult.GenerateLocationHeader();

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
            ODataPath path = new ODataPath(new EntitySetPathSegment(model.Customers));
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().Model = model.Model;
            request.ODataProperties().Path = path;
            CreatedODataResult<TestEntity> createdODataResult = GetCreatedODataResult(request);

            // Act
            Assert.Throws<InvalidOperationException>(() => createdODataResult.GenerateLocationHeader(),
                "The edit link builder for the entity set 'Customers' returned null. An edit link is required for the location header.");
        }

        [Fact]
        public void Property_LocationHeader_IsEvaluatedLazily()
        {
            // Arrange
            Uri editLink = new Uri("http://edit-link");
            Mock<NavigationSourceLinkBuilderAnnotation> linkBuilder = new Mock<NavigationSourceLinkBuilderAnnotation>();
            linkBuilder.Setup(b => b.BuildEditLink(It.IsAny<EntityInstanceContext>(), ODataMetadataLevel.FullMetadata, null))
                .Returns(editLink);

            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(TestEntity)));
            model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder.Object);
            ODataPath path = new ODataPath(new EntitySetPathSegment(model.Customers));
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().Model = model.Model;
            request.ODataProperties().Path = path;
            TestController controller = new TestController();
            controller.Configuration = new HttpConfiguration();
            CreatedODataResult<TestEntity> createdODataResult = new CreatedODataResult<TestEntity>(_entity, controller);

            // Act
            controller.Request = request;
            Uri locationHeader = createdODataResult.LocationHeader;

            // Assert
            Assert.Same(editLink, locationHeader);
        }

        [Fact]
        public void Property_LocationHeader_IsEvaluatedOnlyOnce()
        {
            // Arrange
            Uri editLink = new Uri("http://edit-link");
            Mock<NavigationSourceLinkBuilderAnnotation> linkBuilder = new Mock<NavigationSourceLinkBuilderAnnotation>();
            linkBuilder.Setup(b => b.BuildEditLink(It.IsAny<EntityInstanceContext>(), ODataMetadataLevel.FullMetadata, null))
                .Returns(editLink);

            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(TestEntity)));
            model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder.Object);
            ODataPath path = new ODataPath(new EntitySetPathSegment(model.Customers));
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().Model = model.Model;
            request.ODataProperties().Path = path;
            TestController controller = new TestController { Request = request, Configuration = new HttpConfiguration() };
            CreatedODataResult<TestEntity> createdODataResult = new CreatedODataResult<TestEntity>(_entity, controller);

            // Act
            Uri locationHeader = createdODataResult.LocationHeader;

            // Assert
            linkBuilder.Verify(
                (b) => b.BuildEditLink(It.IsAny<EntityInstanceContext>(), ODataMetadataLevel.FullMetadata, null),
                Times.Once());
        }

        [Fact]
        public void Property_EntityIdHeader_IsEvaluatedLazilyAndOnlyOnce()
        {
            // Arrange
            Uri idLink = new Uri("http://id-link");
            Mock<NavigationSourceLinkBuilderAnnotation> linkBuilder = new Mock<NavigationSourceLinkBuilderAnnotation>();
            linkBuilder.CallBase = true;
            linkBuilder.Setup(b => b.BuildIdLink(It.IsAny<EntityInstanceContext>(), ODataMetadataLevel.FullMetadata))
                .Returns(idLink);

            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(TestEntity)));
            model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder.Object);
            ODataPath path = new ODataPath(new EntitySetPathSegment(model.Customers));
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().Model = model.Model;
            request.ODataProperties().Path = path;
            TestController controller = new TestController { Configuration = new HttpConfiguration() };
            CreatedODataResult<TestEntity> createdODataResult = new CreatedODataResult<TestEntity>(_entity, controller);

            // Act
            controller.Request = request;
            Uri entityIdHeader = createdODataResult.EntityId;

            // Assert
            Assert.Same(idLink, entityIdHeader);
            linkBuilder.Verify(
                b => b.BuildIdLink(It.IsAny<EntityInstanceContext>(), ODataMetadataLevel.FullMetadata),
                Times.Once());
        }

        private CreatedODataResult<TestEntity> GetCreatedODataResult(HttpRequestMessage request)
        {
            return new CreatedODataResult<TestEntity>(_entity, _contentNegotiator, request, _formatters, _locationHeader);
        }

        private static HttpRequestMessage GetODataRequest(IEdmModel model)
        {
            HttpConfiguration configuration = new HttpConfiguration();
            string routeName = "Route";
            configuration.MapODataServiceRoute(routeName, null, model);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            request.SetConfiguration(configuration);
            request.ODataProperties().RouteName = routeName;
            return request;
        }

        private class TestEntity
        {
        }

        private class TestController : ApiController
        {
        }
    }
}
