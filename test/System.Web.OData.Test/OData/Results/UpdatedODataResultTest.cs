// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Results
{
    public class UpdatedODataResultTest
    {
        private readonly TestEntity _entity = new TestEntity();
        private readonly HttpRequestMessage _request = new HttpRequestMessage();
        private readonly IContentNegotiator _contentNegotiator = new Mock<IContentNegotiator>().Object;
        private readonly IEnumerable<MediaTypeFormatter> _formatters = new MediaTypeFormatter[0];
        private readonly TestController _controller = new TestController();

        [Fact]
        public void Ctor_ControllerDependency_ThrowsArgumentNull_Entity()
        {
            Assert.ThrowsArgumentNull(
                () => new UpdatedODataResult<TestEntity>(entity: null, controller: _controller), "entity");
        }

        [Fact]
        public void Ctor_ControllerDependency_ThrowsArgumentNull_Controller()
        {
            Assert.ThrowsArgumentNull(
                () => new UpdatedODataResult<TestEntity>(_entity, controller: null), "controller");
        }

        [Fact]
        public void Ctor_DirectDependency_ThrowsArgumentNull_Entity()
        {
            Assert.ThrowsArgumentNull(
                () => new UpdatedODataResult<TestEntity>(entity: null, contentNegotiator: _contentNegotiator,
                    request: _request, formatters: _formatters), "entity");
        }

        [Fact]
        public void Ctor_DirectDependency_ThrowsArgumentNull_ContentNegotiator()
        {
            Assert.ThrowsArgumentNull(
                () => new UpdatedODataResult<TestEntity>(_entity, contentNegotiator: null,
                    request: _request, formatters: _formatters), "contentNegotiator");
        }

        [Fact]
        public void Ctor_DirectDependency_ThrowsArgumentNull_Request()
        {
            Assert.ThrowsArgumentNull(
                () => new UpdatedODataResult<TestEntity>(_entity, _contentNegotiator,
                    request: null, formatters: _formatters), "request");
        }

        [Fact]
        public void Ctor_DirectDependency_ThrowsArgumentNull_Formatters()
        {
            Assert.ThrowsArgumentNull(
                () => new UpdatedODataResult<TestEntity>(_entity, _contentNegotiator,
                    _request, formatters: null), "formatters");
        }

        [Fact]
        public void Property_Entity_DirectDependency()
        {
            UpdatedODataResult<TestEntity> result = new UpdatedODataResult<TestEntity>(
                _entity, _contentNegotiator, _request, _formatters);

            Assert.Same(_entity, result.Entity);
        }

        [Fact]
        public void Property_ContentNegotiator_DirectDependency()
        {
            UpdatedODataResult<TestEntity> result = new UpdatedODataResult<TestEntity>(
                _entity, _contentNegotiator, _request, _formatters);

            Assert.Same(_contentNegotiator, result.ContentNegotiator);
        }

        [Fact]
        public void Property_ContentNegotiator_Request()
        {
            UpdatedODataResult<TestEntity> result = new UpdatedODataResult<TestEntity>(
                _entity, _contentNegotiator, _request, _formatters);

            Assert.Same(_request, result.Request);
        }

        [Fact]
        public void Property_ContentNegotiator_Formatters()
        {
            UpdatedODataResult<TestEntity> result = new UpdatedODataResult<TestEntity>(
                _entity, _contentNegotiator, _request, _formatters);

            Assert.Same(_formatters, result.Formatters);
        }

        [Fact]
        public void GetActionResult_ReturnsNoContentStatusCodeResult_IfRequestHasNoPreferenceHeader()
        {
            // Arrange
            UpdatedODataResult<TestEntity> updatedODataResult = new UpdatedODataResult<TestEntity>(
                _entity, _contentNegotiator, _request, _formatters);

            // Act
            IHttpActionResult result = updatedODataResult.GetInnerActionResult();

            // Assert
            StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(HttpStatusCode.NoContent, statusCodeResult.StatusCode);
            Assert.Same(_request, statusCodeResult.Request);
        }

        [Fact]
        public void GetActionResult_ReturnsNoContentStatusCodeResult_IfRequestAsksForNoContent()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.TryAddWithoutValidation("Prefer", "return-no-content");
            UpdatedODataResult<TestEntity> updatedODataResult = new UpdatedODataResult<TestEntity>(_entity,
                _contentNegotiator, request, _formatters);

            // Act
            IHttpActionResult result = updatedODataResult.GetInnerActionResult();

            // Assert
            StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(HttpStatusCode.NoContent, statusCodeResult.StatusCode);
            Assert.Same(request, statusCodeResult.Request);
        }

        [Fact]
        public void GetActionResult_ReturnsNegotiatedContentResult_IfRequestAsksForContent()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.TryAddWithoutValidation("Prefer", "return-content");
            UpdatedODataResult<TestEntity> updatedODataResult = new UpdatedODataResult<TestEntity>(_entity,
                _contentNegotiator, request, _formatters);

            // Act
            IHttpActionResult result = updatedODataResult.GetInnerActionResult();

            // Assert
            NegotiatedContentResult<TestEntity> negotiatedResult = Assert.IsType<NegotiatedContentResult<TestEntity>>(result);
            Assert.Equal(HttpStatusCode.OK, negotiatedResult.StatusCode);
            Assert.Same(request, negotiatedResult.Request);
            Assert.Same(_contentNegotiator, negotiatedResult.ContentNegotiator);
            Assert.Same(_entity, negotiatedResult.Content);
            Assert.Same(_formatters, negotiatedResult.Formatters);
        }

        private class TestEntity
        {
        }

        private class TestController : ApiController
        {
        }
    }
}
