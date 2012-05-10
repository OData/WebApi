// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Hosting;
using Moq;
using Moq.Protected;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Dispatcher
{
    public class HttpRoutingDispatcherTest
    {
        [Fact]
        public void Constructor_GuardClauses()
        {
            Assert.ThrowsArgumentNull(
                () => new HttpRoutingDispatcher(configuration: null),
                "configuration");
            Assert.ThrowsArgumentNull(
                () => new HttpRoutingDispatcher(configuration: null, defaultHandler: new Mock<HttpMessageHandler>().Object),
                "configuration");
            Assert.ThrowsArgumentNull(
                () => new HttpRoutingDispatcher(new HttpConfiguration(), defaultHandler: null),
                "defaultHandler");
        }

        [Fact]
        public void SendAsync_PopulatesRouteDataWhenNotPresentInRequest()
        {
            var config = new HttpConfiguration();
            var request = CreateRequest(config, "http://localhost/api/controllerName/42");
            var dispatcher = new HttpRoutingDispatcher(config);
            var invoker = new HttpMessageInvoker(dispatcher);

            invoker.SendAsync(request, CancellationToken.None).WaitUntilCompleted();

            var routeData = request.GetRouteData();
            Assert.NotNull(routeData);
            Assert.Same(config.Routes.Single(), routeData.Route);
            Assert.Equal("controllerName", routeData.Values["controller"]);
            Assert.Equal("42", routeData.Values["id"]);
        }

        [Fact]
        public void SendAsync_RemovesRouteParameterOptionalValuesThatAreNotPresent()
        {
            var config = new HttpConfiguration();
            var request = CreateRequest(config, "http://localhost/api/controllerName");
            var dispatcher = new HttpRoutingDispatcher(config);
            var invoker = new HttpMessageInvoker(dispatcher);

            invoker.SendAsync(request, CancellationToken.None).WaitUntilCompleted();

            Assert.False(request.GetRouteData().Values.ContainsKey("id"));
        }

        [Fact]
        public void SendAsync_Returns404WhenNoMatchingRoute()
        {
            var config = new HttpConfiguration();
            var request = CreateRequest(config, "http://localhost/noMatch");
            var dispatcher = new HttpRoutingDispatcher(config);
            var invoker = new HttpMessageInvoker(dispatcher);

            var responseTask = invoker.SendAsync(request, CancellationToken.None);
            responseTask.WaitUntilCompleted();

            Assert.Equal(HttpStatusCode.NotFound, responseTask.Result.StatusCode);
        }

        [Fact]
        public void SendAsync_CallsDefaultHandlerWhenRouteHandlerIsNull()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            var config = new HttpConfiguration();
            var request = CreateRequest(config, "http://localhost/api/controllerName", routeHandler: null);
            var dispatcher = new HttpRoutingDispatcher(config, defaultHandler: mockHandler.Object);
            var invoker = new HttpMessageInvoker(dispatcher);

            invoker.SendAsync(request, CancellationToken.None);

            mockHandler.Protected().Verify("SendAsync", Times.Once(), request, CancellationToken.None);
        }

        [Fact]
        public void SendAsync_CallsRouterHandlerWhenRouteHandlerIsNotNull()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            var config = new HttpConfiguration();
            var request = CreateRequest(config, "http://localhost/api/controllerName", routeHandler: mockHandler.Object);
            var dispatcher = new HttpRoutingDispatcher(config);
            var invoker = new HttpMessageInvoker(dispatcher);

            invoker.SendAsync(request, CancellationToken.None);

            mockHandler.Protected().Verify("SendAsync", Times.Once(), request, CancellationToken.None);
        }

        private static HttpRequestMessage CreateRequest(HttpConfiguration config, string requestUri)
        {
            return CreateRequest(config, requestUri, routeHandler: new EmptyResponseHandler());
        }

        private static HttpRequestMessage CreateRequest(HttpConfiguration config, string requestUri, HttpMessageHandler routeHandler)
        {
            var route = config.Routes.CreateRoute("api/{controller}/{id}",
                                                  defaults: new Dictionary<string, object> { { "id", RouteParameter.Optional } },
                                                  constraints: null,
                                                  dataTokens: null,
                                                  handler: routeHandler);
            config.Routes.Add("default", route);

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            return request;
        }

        private class EmptyResponseHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return TaskHelpers.FromResult(request.CreateResponse(HttpStatusCode.OK));
            }
        }
    }
}
