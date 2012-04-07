// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Xunit;
using Xunit.Extensions;

namespace System.Web.Http.ExceptionHandling
{
    public class HttpResponseExceptionTest
    {
        [Theory]
        [InlineData("DoNotThrow")]
        [InlineData("ActionMethod")]
        // TODO : 332683 - HttpResponseExceptions in message handlers
        //[InlineData("RequestMessageHandler")]
        //[InlineData("ResponseMessageHandler")]
        [InlineData("RequestAuthorization")]
        [InlineData("BeforeActionExecuted")]
        [InlineData("AfterActionExecuted")]
        [InlineData("ContentNegotiatorNegotiate")]
        [InlineData("ActionMethodAndExceptionFilter")]
        [InlineData("MediaTypeFormatterReadFromStreamAsync")]
        public void HttpResponseExceptionWithExplicitStatusCode(string throwAt)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(ScenarioHelper.BaseAddress + "/ExceptionTests/ReturnString");
            request.Method = HttpMethod.Post;
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent("\"" + throwAt + "\"", Encoding.UTF8, "application/json");

            ScenarioHelper.RunTest(
                "ExceptionTests",
                "/{action}",
                request,
                response =>
                {
                    Assert.NotNull(response.Content);
                    Assert.NotNull(response.Content.Headers.ContentType);
                    Assert.Equal(response.Content.Headers.ContentType.MediaType, "application/json");

                    if (throwAt == "DoNotThrow")
                    {
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        Assert.Equal("Hello World!", response.Content.ReadAsAsync<string>(new List<MediaTypeFormatter>() { new JsonMediaTypeFormatter() }).Result);
                    }
                    else
                    {
                        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                        Assert.Equal(String.Format("Error at {0}", throwAt),
                                        response.Content.ReadAsAsync<string>(new List<MediaTypeFormatter>() { new JsonMediaTypeFormatter() }).Result);
                    }
                },
                config =>
                {
                    config.Services.Replace(typeof(IContentNegotiator), new CustomContentNegotiator(throwAt));

                    config.MessageHandlers.Add(new CustomMessageHandler(throwAt));
                    config.Filters.Add(new CustomActionFilterAttribute(throwAt));
                    config.Filters.Add(new CustomAuthorizationFilterAttribute(throwAt));
                    config.Filters.Add(new CustomExceptionFilterAttribute(throwAt));
                    config.Formatters.Clear();
                    config.Formatters.Add(new CustomJsonMediaTypeFormatter(throwAt));
                }
            );
        }
    }

    public class CustomMessageHandler : DelegatingHandler
    {
        private string _throwAt;

        public CustomMessageHandler(string throwAt)
        {
            _throwAt = throwAt;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ExceptionTestsUtility.CheckForThrow(_throwAt, "RequestMessageHandler");

            return base.SendAsync(request, cancellationToken).ContinueWith<HttpResponseMessage>((tsk) =>
            {
                ExceptionTestsUtility.CheckForThrow(_throwAt, "ResponseMessageHandler");

                HttpResponseMessage response = tsk.Result;

                return response;
            });
        }
    }

    public class ExceptionTestsController : ApiController
    {
        [HttpPost]
        public string ReturnString([FromBody] string throwAt)
        {
            string message = "Hello World!";

            // check if the test wants to throw from here
            ExceptionTestsUtility.CheckForThrow(throwAt, "ActionMethod");

            // NOTE: this indicates that we want to throw from here & after this gets intercepted
            // by the ExceptionFilter, we want to throw from there too
            ExceptionTestsUtility.CheckForThrow(throwAt, "ActionMethodAndExceptionFilter");

            return message;
        }
    }

    public class CustomAuthorizationFilterAttribute : AuthorizationFilterAttribute
    {
        private string _throwAt;

        public CustomAuthorizationFilterAttribute(string throwAt)
        {
            _throwAt = throwAt;
        }

        public override void OnAuthorization(HttpActionContext context)
        {
            ExceptionTestsUtility.CheckForThrow(_throwAt, "RequestAuthorization");
        }
    }

    public class CustomActionFilterAttribute : ActionFilterAttribute
    {
        private string _throwAt;

        public CustomActionFilterAttribute(string throwAt)
        {
            _throwAt = throwAt;
        }

        public override void OnActionExecuting(HttpActionContext context)
        {
            ExceptionTestsUtility.CheckForThrow(_throwAt, "BeforeActionExecuted");
        }

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            ExceptionTestsUtility.CheckForThrow(_throwAt, "AfterActionExecuted");
        }
    }

    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private string _throwAt;

        public CustomExceptionFilterAttribute(string throwAt)
        {
            _throwAt = throwAt;
        }

        public override void OnException(HttpActionExecutedContext context)
        {
            ExceptionTestsUtility.CheckForThrow(_throwAt, "ActionMethodAndExceptionFilter");
        }
    }

    public class CustomContentNegotiator : System.Net.Http.Formatting.DefaultContentNegotiator
    {
        private string _throwAt;

        public CustomContentNegotiator(string throwAt)
        {
            _throwAt = throwAt;
        }

        public override ContentNegotiationResult Negotiate(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
        {
            ExceptionTestsUtility.CheckForThrow(_throwAt, "ContentNegotiatorNegotiate");

            return base.Negotiate(type, request, formatters);
        }
    }

    public class CustomJsonMediaTypeFormatter : JsonMediaTypeFormatter
    {
        private string _throwAt;

        public CustomJsonMediaTypeFormatter(string throwAt)
        {
            _throwAt = throwAt;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream stream, HttpContentHeaders contentHeaders, IFormatterLogger formatterLogger)
        {
            ExceptionTestsUtility.CheckForThrow(_throwAt, "MediaTypeFormatterReadFromStreamAsync");

            return base.ReadFromStreamAsync(type, stream, contentHeaders, formatterLogger);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContentHeaders contentHeaders, TransportContext transportContext)
        {
            ExceptionTestsUtility.CheckForThrow(_throwAt, "MediaTypeFormatterWriteToStreamAsync");

            return base.WriteToStreamAsync(type, value, stream, contentHeaders, transportContext);
        }
    }

    public static class ExceptionTestsUtility
    {
        public static void CheckForThrow(string throwAt, string stage)
        {
            if (throwAt == stage)
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<string>(String.Format("Error at {0}", stage), new JsonMediaTypeFormatter())
                };

                throw new HttpResponseException(response);
            }
        }
    }
}
