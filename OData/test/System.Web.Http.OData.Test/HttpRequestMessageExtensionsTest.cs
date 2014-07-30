// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.TestCommon;
using Moq;

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
                MessageLanguage = "en-US",
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
            Assert.Equal("en-US", contentError.MessageLanguage);
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

        private SelectExpandClause GetMockSelectExpandClause()
        {
            return new SelectExpandClause(new SelectItem[0], allSelected: true);
        }
    }
}