// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http
{
    public class ODataHttpRequestMessageExtensionTests
    {
        [Fact]
        public void GetEdmModelReturnsNullByDefault()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            IEdmModel model = request.GetEdmModel();

            Assert.Null(model);
        }

        [Fact]
        public void SetEdmModelThenGetReturnsWhatYouSet()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            IEdmModel model = new EdmModel();

            // Act
            request.SetEdmModel(model);
            IEdmModel newModel = request.GetEdmModel();

            // Assert
            Assert.Same(model, newModel);
        }

        [Fact]
        public void GetODataPathHandlerReturnsDefaultPathHandlerByDefault()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            IEdmModel model = new EdmModel();
            request.SetEdmModel(model);

            var pathHandler = request.GetODataPathHandler();

            Assert.NotNull(pathHandler);
            Assert.IsType<DefaultODataPathHandler>(pathHandler);
        }

        [Fact]
        public void SetODataPathHandlerThenGetReturnsWhatYouSet()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            IODataPathHandler parser = new Mock<IODataPathHandler>().Object;

            // Act
            request.SetODataPathHandler(parser);

            // Assert
            Assert.Same(parser, request.GetODataPathHandler());
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
        public void CreateODataError_Respects_IncludeErrorDetail(IncludeErrorDetailPolicy errorDetail, bool? isLocal, bool? includeErrorDetail, bool detailIncluded)
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

            HttpResponseMessage response = request.CreateODataErrorResponse(HttpStatusCode.BadRequest, error);

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
        public void GetSelectExpandCaluse_ThrowsArgumentNull_Request()
        {
            HttpRequestMessage request = null;
            Assert.ThrowsArgumentNull(
                () => request.GetSelectExpandClause(),
                "request");
        }

        [Fact]
        public void SetSelectExpandCaluse_ThrowsArgumentNull_Request()
        {
            HttpRequestMessage request = null;
            Assert.ThrowsArgumentNull(
                () => request.SetSelectExpandClause(GetMockSelectExpandClause()),
                "request");
        }

        [Fact]
        public void SetSelectExpandCaluse_ThrowsArgumentNull_SelectExpandClause()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            Assert.ThrowsArgumentNull(
                () => request.SetSelectExpandClause(selectExpandClause: null),
                "selectExpandClause");
        }

        [Fact]
        public void GetSelectExpandClause_ReturnsNullByDefault()
        {
            HttpRequestMessage request = new HttpRequestMessage();

            Assert.Null(request.GetSelectExpandClause());
        }

        [Fact]
        public void GetSelectExpandClause_Returns_SetSelectExpandClause()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            SelectExpandClause selectExpandCaluse = GetMockSelectExpandClause();

            // Act
            request.SetSelectExpandClause(selectExpandCaluse);
            var result = request.GetSelectExpandClause();

            // Assert
            Assert.Same(selectExpandCaluse, result);
        }

        private SelectExpandClause GetMockSelectExpandClause()
        {
            return new SelectExpandClause(new SelectItem[0], allSelected: true);
        }
    }
}