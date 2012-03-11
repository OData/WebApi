using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Filters
{
    public class QueryCompositionFilterAttributeTest
    {
        private const string QueryKey = "MS_QueryKey";

        QueryCompositionFilterAttribute _filter = new QueryCompositionFilterAttribute(typeof(int), queryValidator: null);

        [Fact]
        public void ConstructorThrowsOnNullInput()
        {
            Assert.ThrowsArgumentNull(() => new QueryCompositionFilterAttribute(null, queryValidator: null), "queryElementType");
        }

        [Fact]
        public void OnActionExecutingSetsQueryPropertyOnRequestMessage()
        {
            // Arrange
            var actionContext = ContextUtil.GetHttpActionContext(new HttpRequestMessage(HttpMethod.Get, "http://localhost/?$top=100"));

            // Act
            _filter.OnActionExecuting(actionContext);
            var requestProperties = actionContext.ControllerContext.Request.Properties;

            // Assert
            Assert.True(requestProperties.ContainsKey(QueryKey));
            Assert.IsAssignableFrom<IQueryable<int>>(requestProperties[QueryKey]);
        }

        [Fact]
        public void OnActionExecutedAppendsQueryToResponse()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.Properties[QueryKey] = (new int[0]).AsQueryable().Take(100);
            HttpResponseMessage response = new HttpResponseMessage() { Content = new ObjectContent<IQueryable<int>>(Enumerable.Range(1, 1000).AsQueryable(), new JsonMediaTypeFormatter()) };

            var actionExecutedContext = ContextUtil.GetActionExecutedContext(request, response);

            // Act
            _filter.OnActionExecuted(actionExecutedContext);
            HttpResponseMessage result = actionExecutedContext.Result;

            // Assert
            // TODO: we are depending on the correctness of QueryComposer here to test the filter which 
            // is sub-optimal. Reason being QueryComposer is a static class. cleanup with bug#325697 
            Assert.NotNull(result);
            Assert.Equal(100, result.Content.ReadAsAsync<IQueryable<int>>().Result.Count());
        }

        [Theory]
        [InlineData("$top=error")]
        [InlineData("$filter=error")]
        [InlineData("$skip=-100")]
        public void OnActionExecutingThrowsForIncorrectTopQuery(string query)
        {
            // Arrange
            const string baseAddress = "http://localhost/?{0}";
            var request = new HttpRequestMessage(HttpMethod.Get, String.Format(baseAddress, query));
            var actionContext = ContextUtil.GetHttpActionContext(request);

            // Act & Assert
            Assert.Throws<HttpRequestException>(
                () => _filter.OnActionExecuting(actionContext),
                "The query specified in the URI is not valid.");
        }

        [Fact]
        public void OnActionExecutedOnNullResponse()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.Properties[QueryKey] = (new int[0]).AsQueryable().Take(100);
            var actionContext = ContextUtil.GetActionExecutedContext(request, response: null);

            // Act & Assert
            Assert.DoesNotThrow(() => _filter.OnActionExecuted(actionContext));
            Assert.Null(actionContext.Result);
        }
    }
}
