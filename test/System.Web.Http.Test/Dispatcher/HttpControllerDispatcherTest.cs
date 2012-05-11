// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Dispatcher
{
    public class HttpControllerDispatcherTest
    {
        [Fact]
        public void Constructor_GuardClauses()
        {
            Assert.ThrowsArgumentNull(
                () => new HttpControllerDispatcher(configuration: null),
                "configuration");
        }

        [Fact]
        public void SendAsync_CallsControllerSelectorToGetControllerDescriptor()
        {
            var mockSelector = new Mock<IHttpControllerSelector>();
            var config = new HttpConfiguration();
            config.Services.Replace(typeof(IHttpControllerSelector), mockSelector.Object);
            var request = CreateRequest(config, "http://localhost/api/foo");
            var dispatcher = new HttpControllerDispatcher(config);
            var invoker = new HttpMessageInvoker(dispatcher);

            invoker.SendAsync(request, CancellationToken.None).WaitUntilCompleted();

            mockSelector.Verify(s => s.SelectController(request), Times.Once());
        }

        [Fact]
        public void SendAsync_CallsControllerDescriptorToCreateController()
        {
            var mockSelector = new Mock<IHttpControllerSelector>();
            var mockDescriptor = new Mock<HttpControllerDescriptor>();
            var config = new HttpConfiguration();
            config.Services.Replace(typeof(IHttpControllerSelector), mockSelector.Object);
            var request = CreateRequest(config, "http://localhost/api/foo");
            mockSelector.Setup(s => s.SelectController(request))
                        .Returns(mockDescriptor.Object);
            mockDescriptor.Setup(d => d.CreateController(request))
                          .Returns(new PrivateController())
                          .Verifiable();
            var dispatcher = new HttpControllerDispatcher(config);
            var invoker = new HttpMessageInvoker(dispatcher);

            invoker.SendAsync(request, CancellationToken.None).WaitUntilCompleted();

            mockDescriptor.Verify();
        }

        [Fact]
        public void SendAsync_CallsControllerExecuteAsyncWithPopulatedControllerContext()
        {
            HttpControllerContext calledContext = null;
            var mockSelector = new Mock<IHttpControllerSelector>();
            var mockDescriptor = new Mock<HttpControllerDescriptor>();
            var mockController = new Mock<IHttpController>();
            var config = new HttpConfiguration();
            config.Services.Replace(typeof(IHttpControllerSelector), mockSelector.Object);
            var request = CreateRequest(config, "http://localhost/api/foo");
            mockSelector.Setup(s => s.SelectController(request))
                        .Returns(mockDescriptor.Object);
            mockDescriptor.Setup(d => d.CreateController(request))
                          .Returns(mockController.Object);
            mockDescriptor.Object.Initialize(config);
            mockController.Setup(c => c.ExecuteAsync(It.IsAny<HttpControllerContext>(), CancellationToken.None))
                          .Callback((HttpControllerContext ctxt, CancellationToken token) => { calledContext = ctxt; });
            var dispatcher = new HttpControllerDispatcher(config);
            var invoker = new HttpMessageInvoker(dispatcher);

            invoker.SendAsync(request, CancellationToken.None).WaitUntilCompleted();

            Assert.NotNull(calledContext);
            Assert.Same(mockController.Object, calledContext.Controller);
            Assert.Same(mockDescriptor.Object, calledContext.ControllerDescriptor);
            Assert.Same(config, calledContext.Configuration);
            Assert.Same(request, calledContext.Request);
            Assert.Same(request.GetRouteData(), calledContext.RouteData);
        }

        [Fact]
        public void SendAsync_Returns404WhenControllerSelectorReturnsNullControllerDescriptor()
        {
            var config = new HttpConfiguration();
            var request = CreateRequest(config, "http://localhost/api/foo");
            var dispatcher = new HttpControllerDispatcher(config);
            var invoker = new HttpMessageInvoker(dispatcher);

            Task<HttpResponseMessage> resultTask = invoker.SendAsync(request, CancellationToken.None);
            resultTask.WaitUntilCompleted();

            Assert.Equal(HttpStatusCode.NotFound, resultTask.Result.StatusCode);
        }

        [Fact]
        public void SendAsync_Returns500WithHttpErrorWhenControllerThrows()
        {
            var config = new HttpConfiguration() { IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always };
            var request = CreateRequest(config, "http://localhost/api/HttpControllerDispatcherThrowing");
            var dispatcher = new HttpControllerDispatcher(config);
            var invoker = new HttpMessageInvoker(dispatcher);

            Task<HttpResponseMessage> resultTask = invoker.SendAsync(request, CancellationToken.None);
            resultTask.WaitUntilCompleted();

            Assert.Equal(HttpStatusCode.InternalServerError, resultTask.Result.StatusCode);
            var objectContent = Assert.IsType<ObjectContent<HttpError>>(resultTask.Result.Content);
            var error = Assert.IsType<HttpError>(objectContent.Value);
            Assert.Equal("Hello from the throwing controller", error["ExceptionMessage"]);
        }

        private static HttpRequestMessage CreateRequest(HttpConfiguration config, string requestUri)
        {
            IHttpRoute route = config.Routes.MapHttpRoute("default", "api/{controller}/{id}", new { id = RouteParameter.Optional });
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = route.GetRouteData("/", request);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            return request;
        }

        private class PrivateController : ApiController
        {
            public void Get() { }

            public override Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
            {
                // Empty. Skip all the logic of execcuting a controller.
                HttpResponseMessage response = new HttpResponseMessage();
                return TaskHelpers.FromResult(response);
            }
        }
    }

    public class HttpControllerDispatcherThrowingController : ApiController
    {
        public void Get()
        {
            throw new Exception("Hello from the throwing controller");
        }
    }
}
