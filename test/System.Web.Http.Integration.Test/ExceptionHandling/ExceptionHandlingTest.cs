using System.Json;
using System.Net;
using System.Net.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Properties;
using Xunit;
using Xunit.Extensions;

namespace System.Web.Http
{
    public class ExceptionHandlingTest
    {
        [Theory]
        [InlineData("Unavailable")]
        [InlineData("AsyncUnavailable")]
        [InlineData("AsyncUnavailableDelegate")]
        public void ThrowingHttpResponseException_FromAction_GetsReturnedToClient(string actionName)
        {
            string controllerName = "Exception";
            string requestUrl = String.Format("{0}/{1}/{2}", ScenarioHelper.BaseAddress, controllerName, actionName);

            ScenarioHelper.RunTest(
                controllerName,
                "/{action}",
                new HttpRequestMessage(HttpMethod.Get, requestUrl),
                (response) =>
                {
                    Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
                }
            );
        }

        [Theory]
        [InlineData("ArgumentNull")]
        [InlineData("AsyncArgumentNull")]
        public void ThrowingArgumentNullException_FromAction_GetsReturnedToClient(string actionName)
        {
            string controllerName = "Exception";
            string requestUrl = String.Format("{0}/{1}/{2}", ScenarioHelper.BaseAddress, controllerName, actionName);

            ScenarioHelper.RunTest(
                controllerName,
                "/{action}",
                new HttpRequestMessage(HttpMethod.Get, requestUrl),
                (response) =>
                {
                    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                    ExceptionSurrogate exception = response.Content.ReadAsAsync<ExceptionSurrogate>().Result;
                    Assert.Equal(typeof(ArgumentNullException).FullName, exception.ExceptionType.ToString());
                }
            );
        }

        [Theory]
        [InlineData("ArgumentNull")]
        [InlineData("AsyncArgumentNull")]
        public void ThrowingArgumentNullException_FromAction_GetsReturnedToClientParsedAsJson(string actionName)
        {
            string controllerName = "Exception";
            string requestUrl = String.Format("{0}/{1}/{2}", ScenarioHelper.BaseAddress, controllerName, actionName);

            ScenarioHelper.RunTest(
                controllerName,
                "/{action}",
                new HttpRequestMessage(HttpMethod.Get, requestUrl),
                (response) =>
                {
                    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                    dynamic json = JsonValue.Parse(response.Content.ReadAsStringAsync().Result);
                    string result = json.ExceptionType;
                    Assert.Equal(typeof(ArgumentNullException).FullName, result);
                }
            );
        }

        [Theory]
        [InlineData("AuthorizationFilter")]
        [InlineData("ActionFilter")]
        [InlineData("ExceptionFilter")]
        public void ThrowingArgumentException_FromFilter_GetsReturnedToClient(string actionName)
        {
            string controllerName = "Exception";
            string requestUrl = String.Format("{0}/{1}/{2}", ScenarioHelper.BaseAddress, controllerName, actionName);

            ScenarioHelper.RunTest(
                controllerName,
                "/{action}",
                new HttpRequestMessage(HttpMethod.Get, requestUrl),
                (response) =>
                {
                    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                    ExceptionSurrogate exception = response.Content.ReadAsAsync<ExceptionSurrogate>().Result;
                    Assert.Equal(typeof(ArgumentException).FullName, exception.ExceptionType.ToString());
                }
            );
        }

        [Theory]
        [InlineData("AuthorizationFilter", HttpStatusCode.Forbidden)]
        [InlineData("ActionFilter", HttpStatusCode.NotAcceptable)]
        [InlineData("ExceptionFilter", HttpStatusCode.NotImplemented)]
        public void ThrowingHttpResponseException_FromFilter_GetsReturnedToClient(string actionName, HttpStatusCode responseExceptionStatusCode)
        {
            string controllerName = "Exception";
            string requestUrl = String.Format("{0}/{1}/{2}", ScenarioHelper.BaseAddress, controllerName, actionName);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add(ExceptionController.ResponseExceptionHeaderKey, responseExceptionStatusCode.ToString());

            ScenarioHelper.RunTest(
                controllerName,
                "/{action}",
                request,
                (response) =>
                {
                    Assert.Equal(responseExceptionStatusCode, response.StatusCode);
                    Assert.Equal("HttpResponseExceptionMessage", response.Content.ReadAsAsync<string>().Result);
                }
            );
        }

        // TODO: add tests that throws from custom model binders

        [Fact]
        public void Service_ReturnsNotFound_WhenControllerNameDoesNotExist()
        {
            string controllerName = "randomControllerThatCannotBeFound";
            string requestUrl = String.Format("{0}/{1}", ScenarioHelper.BaseAddress, controllerName);

            ScenarioHelper.RunTest(
                controllerName,
                "",
                new HttpRequestMessage(HttpMethod.Get, requestUrl),
                (response) =>
                {
                    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                    Assert.Equal(
                        String.Format(SRResources.DefaultControllerFactory_ControllerNameNotFound, controllerName),
                        response.Content.ReadAsAsync<string>().Result);
                }
            );
        }

        [Fact]
        public void Service_ReturnsNotFound_WhenActionNameDoesNotExist()
        {
            string controllerName = "Exception";
            string actionName = "actionNotFound";
            string requestUrl = String.Format("{0}/{1}/{2}", ScenarioHelper.BaseAddress, controllerName, actionName);

            ScenarioHelper.RunTest(
                controllerName,
                "/{action}",
                new HttpRequestMessage(HttpMethod.Get, requestUrl),
                (response) =>
                {
                    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                    Assert.Equal(
                        String.Format(SRResources.ApiControllerActionSelector_ActionNameNotFound, controllerName, actionName),
                        response.Content.ReadAsAsync<string>().Result);
                }
            );
        }

        [Fact]
        public void Service_ReturnsMethodNotAllowed_WhenActionsDoesNotSupportTheRequestHttpMethod()
        {
            string controllerName = "Exception";
            string actionName = "GetString";
            HttpMethod requestMethod = HttpMethod.Post;
            string requestUrl = String.Format("{0}/{1}/{2}", ScenarioHelper.BaseAddress, controllerName, actionName);
            ScenarioHelper.RunTest(
                controllerName,
                "/{action}",
                new HttpRequestMessage(requestMethod, requestUrl),
                (response) =>
                {
                    Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
                    Assert.Equal(
                        String.Format(SRResources.ApiControllerActionSelector_HttpMethodNotSupported, requestMethod.Method),
                        response.Content.ReadAsAsync<string>().Result);
                }
            );
        }

        [Fact]
        public void Service_ReturnsInternalServerError_WhenMultipleActionsAreFound()
        {
            string controllerName = "Exception";
            string requestUrl = String.Format("{0}/{1}", ScenarioHelper.BaseAddress, controllerName);

            ScenarioHelper.RunTest(
                controllerName,
                "",
                new HttpRequestMessage(HttpMethod.Get, requestUrl),
                (response) =>
                {
                    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                    Assert.Contains(
                        String.Format(SRResources.ApiControllerActionSelector_AmbiguousMatch, String.Empty),
                        response.Content.ReadAsAsync<string>().Result);
                }
            );
        }

        [Fact]
        public void Service_ReturnsInternalServerError_WhenMultipleControllersAreFound()
        {
            string controllerName = "Duplicate";
            string requestUrl = String.Format("{0}/{1}", ScenarioHelper.BaseAddress, controllerName);

            ScenarioHelper.RunTest(
                controllerName,
                "",
                new HttpRequestMessage(HttpMethod.Get, requestUrl),
                (response) =>
                {
                    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                    Assert.Contains(
                        String.Format(SRResources.DefaultControllerFactory_ControllerNameAmbiguous_WithRouteTemplate, controllerName, "{controller}", String.Empty),
                        response.Content.ReadAsAsync<string>().Result);
                }
            );
        }
    }
}
