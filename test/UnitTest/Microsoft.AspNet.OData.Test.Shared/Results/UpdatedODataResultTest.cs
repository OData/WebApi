//-----------------------------------------------------------------------------
// <copyright file="UpdatedODataResultTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System.Net;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;
#else
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Moq;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Results
{
    public class UpdatedODataResultTest
    {
        private readonly TestEntity _entity = new TestEntity();
#if NETFX // Only needed for AspNet
        private readonly HttpRequestMessage _request = new HttpRequestMessage();
        private readonly IContentNegotiator _contentNegotiator = new Mock<IContentNegotiator>().Object;
        private readonly IEnumerable<MediaTypeFormatter> _formatters = new MediaTypeFormatter[0];
        private readonly TestController _controller = new TestController();
#endif

#if NETCORE
        [Fact]
        public void Ctor_ControllerDependency_ThrowsArgumentNull_Entity()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new UpdatedODataResult<TestEntity>(entity: null), "entity");
        }

        [Fact]
        public void GetEntity_ReturnsCorrect()
        {
            // Arrange
            Mock<UpdatedODataResultTest> mock = new Mock<UpdatedODataResultTest>();
            UpdatedODataResult<UpdatedODataResultTest> updatedODataResult =
                new UpdatedODataResult<UpdatedODataResultTest>(mock.Object);

            // Act & Assert
            Assert.Same(mock.Object, updatedODataResult.Entity);
        }
#else
        [Fact]
        public void Ctor_ControllerDependency_ThrowsArgumentNull_Entity()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new UpdatedODataResult<TestEntity>(entity: null, controller: _controller), "entity");
        }

        [Fact]
        public void Ctor_ControllerDependency_ThrowsArgumentNull_Controller()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new UpdatedODataResult<TestEntity>(_entity, controller: null), "controller");
        }

        [Fact]
        public void Ctor_DirectDependency_ThrowsArgumentNull_Entity()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new UpdatedODataResult<TestEntity>(entity: null, contentNegotiator: _contentNegotiator,
                    request: _request, formatters: _formatters), "entity");
        }

        [Fact]
        public void Ctor_DirectDependency_ThrowsArgumentNull_ContentNegotiator()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new UpdatedODataResult<TestEntity>(_entity, contentNegotiator: null,
                    request: _request, formatters: _formatters), "contentNegotiator");
        }

        [Fact]
        public void Ctor_DirectDependency_ThrowsArgumentNull_Request()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new UpdatedODataResult<TestEntity>(_entity, _contentNegotiator,
                    request: null, formatters: _formatters), "request");
        }

        [Fact]
        public void Ctor_DirectDependency_ThrowsArgumentNull_Formatters()
        {
            ExceptionAssert.ThrowsArgumentNull(
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
#endif

        [Fact]
        public void GetActionResult_ReturnsNoContentStatusCodeResult_IfRequestHasNoPreferenceHeader()
        {
            // Arrange
            var request = CreateRequest();

            // Act
            var result = CreateActionResult(request);

            // Assert
            StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(HttpStatusCode.NoContent, (HttpStatusCode)statusCodeResult.StatusCode);
#if NETFX // Only needed for AspNet
            Assert.Same(request, statusCodeResult.Request);
#endif
        }

        [Fact]
        public void GetActionResult_ReturnsNoContentStatusCodeResult_IfRequestAsksForNoContent()
        {
            // Arrange
            var request = CreateRequest("return=minimal");

            // Act
            var result = CreateActionResult(request);

            // Assert
            StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(HttpStatusCode.NoContent, (HttpStatusCode)statusCodeResult.StatusCode);
#if NETFX // Only needed for AspNet
            Assert.Same(request, statusCodeResult.Request);
#endif
        }

        [Fact]
        public void GetActionResult_ReturnsNegotiatedContentResult_IfRequestAsksForContent()
        {
            // Arrange
            var request = CreateRequest("return=representation");

            // Act
            var result = CreateActionResult(request);

            // Assert
#if NETCORE
            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Same(typeof(TestEntity), objectResult.Value.GetType());
            Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)objectResult.StatusCode);
#else
            NegotiatedContentResult<TestEntity> negotiatedResult = Assert.IsType<NegotiatedContentResult<TestEntity>>(result);
            Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)negotiatedResult.StatusCode);
            Assert.Same(request, negotiatedResult.Request);
            Assert.Same(_contentNegotiator, negotiatedResult.ContentNegotiator);
            Assert.Same(_entity, negotiatedResult.Content);
            Assert.Same(_formatters, negotiatedResult.Formatters);
#endif
        }

        private class TestEntity
        {
        }

        private class TestController : ODataController
        {
        }

#if NETCORE
        private HttpRequest CreateRequest(string preferHeaderValue = null)
        {
            var request = RequestFactory.Create();
            if (!string.IsNullOrEmpty(preferHeaderValue))
            {
                request.Headers.Add("Prefer", new StringValues(preferHeaderValue));
            }

            return request;
        }

        private IActionResult CreateActionResult(AspNetCore.Http.HttpRequest request)
        {
            UpdatedODataResult<TestEntity> updatedODataResult = new UpdatedODataResult<TestEntity>(_entity);
            return updatedODataResult.GetInnerActionResult(request);
        }
#else
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

        private IHttpActionResult CreateActionResult(HttpRequestMessage request)
        {
            UpdatedODataResult<TestEntity> updatedODataResult = new UpdatedODataResult<TestEntity>(_entity,
                _contentNegotiator, request, _formatters);

            return updatedODataResult.GetInnerActionResult();
        }
#endif
    }
}
