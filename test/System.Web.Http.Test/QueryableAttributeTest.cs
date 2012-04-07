// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using Moq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http
{
    public class QueryableAttributeTest
    {
        private readonly QueryableAttribute _filter = new QueryableAttribute();
        private readonly HttpRequestMessage _request = new HttpRequestMessage();
        private readonly HttpResponseMessage _response = new HttpResponseMessage();
        private readonly HttpActionExecutedContext _actionExecutedContext;
        private readonly HttpActionContext _actionContext;

        public QueryableAttributeTest()
        {
            _response.RequestMessage = _request;
            _actionContext = ContextUtil.CreateActionContext(ContextUtil.CreateControllerContext(request: _request));
            _actionContext.ControllerContext.ControllerDescriptor = new HttpControllerDescriptor
            {
                ControllerName = "TestControllerName"
            };
            Mock.Get(_actionContext.ActionDescriptor).Setup(ad => ad.ActionName).Returns("testActionName");
            _actionContext.Response = _response;
            _actionExecutedContext = ContextUtil.GetActionExecutedContext(_request, _response);
            _actionExecutedContext.ActionContext = _actionContext;
        }

        [Fact]
        public void OnActionExecuting_WhenContextParamterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => _filter.OnActionExecuting(actionContext: null), "actionContext");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void OnActionExecuting_WhenResultLimitValueIsInvalid_Throws(int resultLimit)
        {
            _filter.ResultLimit = resultLimit;

            Assert.Throws<InvalidOperationException>(() => _filter.OnActionExecuting(_actionContext),
                "The value of the ResultLimit property on the Queryable filter applied to action 'testActionName' on controller 'TestControllerName' must be greater than or equal to 0.");
        }

        [Fact]
        public void OnActionExecutedAppendsQueryToResponse()
        {
            // Arrange
            var content = new ObjectContent<IQueryable<int>>(Enumerable.Range(1, 1000).AsQueryable(), new JsonMediaTypeFormatter());
            _request.RequestUri = new Uri(String.Format("http://localhost/?{0}", "$top=100"));
            _response.Content = content;

            // Act
            _filter.OnActionExecuted(_actionExecutedContext);

            // Assert
            // TODO: we are depending on the correctness of QueryComposer here to test the filter which 
            // is sub-optimal. Reason being QueryComposer is a static class. cleanup with bug#325697 
            Assert.NotNull(_actionExecutedContext.Response);
            Assert.Same(content, _actionExecutedContext.Response.Content);
            Assert.Equal(100, ((IQueryable<int>)content.Value).Count());
        }

        [Fact]
        public void OnActionExecuted_WhenActionExecutedContextParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => _filter.OnActionExecuted(actionExecutedContext: null), "actionExecutedContext");
        }

        [Theory]
        [InlineData("$filter=error")]
        [InlineData("$top=error")]
        [InlineData("$skip=-100")]
        public void OnActionExecuted_WhenFilterQueryIsInvalid_SetsBadRequestResponseMessage(string query)
        {
            // Arrange
            _request.RequestUri = new Uri(String.Format("http://localhost/?{0}", query));
            _response.Content = new ObjectContent<IQueryable<string>>(Enumerable.Empty<string>().AsQueryable(), new JsonMediaTypeFormatter());
            _request.Properties[HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();

            // Act
            _filter.OnActionExecuted(_actionExecutedContext);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, _actionExecutedContext.Response.StatusCode);
            Assert.Contains("The query specified in the URI is not valid.", _actionExecutedContext.Response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void OnActionExecuted_ResponseDoesNotContainIQueryable_DoesNothing()
        {
            // Arrange
            var value = Enumerable.Empty<string>();
            var content = new ObjectContent<IEnumerable<string>>(value, new JsonMediaTypeFormatter());
            _response.Content = content;

            // Act
            _filter.OnActionExecuted(_actionExecutedContext);

            // Assert
            Assert.Same(_response, _actionExecutedContext.Response);
            Assert.Same(content, _response.Content);
            var resultContent = Assert.IsType<ObjectContent<IEnumerable<string>>>(_actionExecutedContext.Response.Content);
            Assert.Same(value, resultContent.Value);
        }

        [Fact]
        public void OnActionExecuted_ContextDoesNotHaveResponse_DoesNothing()
        {
            // Arrange
            _actionExecutedContext.Response = null;

            // Act
            _filter.OnActionExecuted(_actionExecutedContext);

            // Assert
            Assert.Null(_actionExecutedContext.Response);
        }

        [Fact]
        public void OnActionExecutedOnNullResponse()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            var actionContext = ContextUtil.GetActionExecutedContext(request, response: null);

            // Act & Assert
            Assert.DoesNotThrow(() => _filter.OnActionExecuted(actionContext));
            Assert.Null(actionContext.Response);
        }
    }
}
