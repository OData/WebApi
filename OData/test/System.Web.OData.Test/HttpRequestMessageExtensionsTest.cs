// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Serialization.Models;
using System.Web.OData.Routing;
using System.Web.OData.TestCommon;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;
using ODataPath = System.Web.OData.Routing.ODataPath;
using ODataPathSegment = System.Web.OData.Routing.ODataPathSegment;

namespace System.Net.Http
{
    public class HttpRequestMessageExtensionsTest
    {
        [Fact]
        public void ModelGetter_ReturnsNullByDefault()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            IEdmModel model = request.ODataProperties().Model;

            Assert.Null(model);
        }

        [Fact]
        public void ModelGetter_Returns_ModelSetter()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            IEdmModel model = new EdmModel();

            // Act
            request.ODataProperties().Model = model;
            IEdmModel newModel = request.ODataProperties().Model;

            // Assert
            Assert.Same(model, newModel);
        }

        [Fact]
        public void PathHandlerGetter_ReturnsDefaultPathHandlerByDefault()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            IEdmModel model = new EdmModel();
            request.ODataProperties().Model = model;

            var pathHandler = request.ODataProperties().PathHandler;

            Assert.NotNull(pathHandler);
            Assert.IsType<DefaultODataPathHandler>(pathHandler);
        }

        [Fact]
        public void PathHandlerGetter_Returns_PathHandlerSetter()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            IODataPathHandler parser = new Mock<IODataPathHandler>().Object;

            // Act
            request.ODataProperties().PathHandler = parser;

            // Assert
            Assert.Same(parser, request.ODataProperties().PathHandler);
        }

        [Theory]
        [InlineData(IncludeErrorDetailPolicy.Default, null, null, false)]
        [InlineData(IncludeErrorDetailPolicy.Default, null, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Default, null, false, false)]
        [InlineData(IncludeErrorDetailPolicy.Default, true, null, true)]
        [InlineData(IncludeErrorDetailPolicy.Default, true, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Default, true, false, false)]
        [InlineData(IncludeErrorDetailPolicy.Default, false, null, false)]
        [InlineData(IncludeErrorDetailPolicy.Default, false, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Default, false, false, false)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, null, null, false)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, true, false, true)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, false, true, false)]
        [InlineData(IncludeErrorDetailPolicy.Always, null, null, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, true, false, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, false, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Never, null, null, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, true, false, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, false, true, false)]
        public void CreateErrorResponse_Respects_IncludeErrorDetail(IncludeErrorDetailPolicy errorDetail, bool? isLocal, bool? includeErrorDetail, bool detailIncluded)
        {
            HttpConfiguration config = new HttpConfiguration() { IncludeErrorDetailPolicy = errorDetail };
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetConfiguration(config);
            if (isLocal.HasValue)
            {
                request.Properties.Add(HttpPropertyKeys.IsLocalKey, new Lazy<bool>(() => isLocal.Value));
            }
            if (includeErrorDetail.HasValue)
            {
                request.Properties.Add(HttpPropertyKeys.IncludeErrorDetailKey, new Lazy<bool>(() => includeErrorDetail.Value));
            }
            ODataError error = new ODataError()
            {
                ErrorCode = "36",
                Message = "Bad stuff",
                InnerError = new ODataInnerError()
                {
                    Message = "Exception message"
                }
            };

            HttpResponseMessage response = request.CreateErrorResponse(HttpStatusCode.BadRequest, error);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            ODataError contentError;
            Assert.True(response.TryGetContentValue<ODataError>(out contentError));
            Assert.Equal("36", contentError.ErrorCode);
            Assert.Equal("Bad stuff", contentError.Message);
            if (detailIncluded)
            {
                Assert.NotNull(contentError.InnerError);
                Assert.Equal("Exception message", contentError.InnerError.Message);
            }
            else
            {
                Assert.Null(contentError.InnerError);
            }
        }

        [Fact]
        public void ODataProperties_ThrowsArgumentNull_RequestNull()
        {
            HttpRequestMessage request = null;
            Assert.ThrowsArgumentNull(
                () => request.ODataProperties(),
                "request");
        }

        [Fact]
        public void SelectExpandClauseSetter_ThrowsArgumentNull()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            Assert.ThrowsArgumentNull(
                () => request.ODataProperties().SelectExpandClause = null,
                "value");
        }

        [Fact]
        public void SelectExpandClauseGetter_ReturnsNullByDefault()
        {
            HttpRequestMessage request = new HttpRequestMessage();

            Assert.Null(request.ODataProperties().SelectExpandClause);
        }

        [Fact]
        public void SelectExpandClauseGetter_Returns_SelectExpandClauseSetter()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            SelectExpandClause selectExpandClause = GetMockSelectExpandClause();

            // Act
            request.ODataProperties().SelectExpandClause = selectExpandClause;
            var result = request.ODataProperties().SelectExpandClause;

            // Assert
            Assert.Same(selectExpandClause, result);
        }

        [Fact]
        public void RoutingConventionsStoreGetter_ReturnsEmptyNonNullDictionary()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            IDictionary<string, object> result = request.ODataProperties().RoutingConventionsStore;

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void RoutingConventionsStoreGetter_ReturnsSameInstance_IfCalledMultipleTimes()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            IDictionary<string, object> instance1 = request.ODataProperties().RoutingConventionsStore;
            IDictionary<string, object> instance2 = request.ODataProperties().RoutingConventionsStore;

            // Assert
            Assert.NotNull(instance1);
            Assert.NotNull(instance2);
            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void GetETag_ThrowsArgumentNull_Request()
        {
            // Arrange
            HttpRequestMessage request = null;
            EntityTagHeaderValue headerValue = new EntityTagHeaderValue("\"any\"");

            // Act & Assert
            Assert.ThrowsArgumentNull(() => request.GetETag(headerValue), "request");
        }

        [Fact]
        public void GetETag_ThrowsInvalidOperation_EmptyRequest()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            EntityTagHeaderValue headerValue = new EntityTagHeaderValue("\"any\"");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => request.GetETag(headerValue),
                "Request message does not contain an HttpConfiguration object.");
        }

        [Fact]
        public void GetETag_Returns_ETagAny()
        {
            // Arrange
            var request = new HttpRequestMessage();
            var etagHeaderValue = EntityTagHeaderValue.Any;

            // Act
            var result = request.GetETag(etagHeaderValue);

            // Assert
            Assert.True(result.IsAny);
        }

        [Fact]
        public void GetETagTEntity_Returns_ETagAny()
        {
            // Arrange
            var request = new HttpRequestMessage();
            var etagHeaderValue = EntityTagHeaderValue.Any;

            // Act
            var result = request.GetETag<Customer>(etagHeaderValue);

            // Assert
            Assert.True(result.IsAny);
        }

        [Fact]
        public void GetETag_Returns_ETagInHeader()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            HttpConfiguration cofiguration = new HttpConfiguration();
            request.SetConfiguration(cofiguration);
            Dictionary<string, object> properties = new Dictionary<string, object> { { "City", "Foo" } };
            EntityTagHeaderValue etagHeaderValue = new DefaultODataETagHandler().CreateETag(properties);

            CustomersModelWithInheritance model = new CustomersModelWithInheritance();

            Mock<ODataPathSegment> mockSegment = new Mock<ODataPathSegment> { CallBase = true };
            mockSegment.Setup(s => s.GetEdmType(null)).Returns(model.Customer);
            mockSegment.Setup(s => s.GetNavigationSource(null)).Returns((IEdmNavigationSource)null);
            ODataPath odataPath = new ODataPath(new[] { mockSegment.Object });
            request.ODataProperties().Path = odataPath;

            // Act
            ETag result = request.GetETag(etagHeaderValue);
            dynamic dynamicResult = result;

            // Assert
            Assert.Equal("Foo", result["City"]);
            Assert.Equal("Foo", dynamicResult.City);
        }

        [Fact]
        public void GetETagTEntity_Returns_ETagInHeader()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            HttpConfiguration cofiguration = new HttpConfiguration();
            request.SetConfiguration(cofiguration);
            Dictionary<string, object> properties = new Dictionary<string, object> { { "City", "Foo" } };
            EntityTagHeaderValue etagHeaderValue = new DefaultODataETagHandler().CreateETag(properties);

            CustomersModelWithInheritance model = new CustomersModelWithInheritance();

            Mock<ODataPathSegment> mockSegment = new Mock<ODataPathSegment> { CallBase = true };
            mockSegment.Setup(s => s.GetEdmType(null)).Returns(model.Customer);
            mockSegment.Setup(s => s.GetNavigationSource(null)).Returns((IEdmNavigationSource)null);
            ODataPath odataPath = new ODataPath(new[] { mockSegment.Object });
            request.ODataProperties().Path = odataPath;

            // Act
            ETag<Customer> result = request.GetETag<Customer>(etagHeaderValue);
            dynamic dynamicResult = result;

            // Assert
            Assert.Equal("Foo", result["City"]);
            Assert.Equal("Foo", dynamicResult.City);
        }

        private SelectExpandClause GetMockSelectExpandClause()
        {
            return new SelectExpandClause(new SelectItem[0], allSelected: true);
        }
    }
}