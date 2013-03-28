// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Batch;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class HttpBatchHandlerTest
    {
        [Fact]
        public void SendAsync_Throws_WhenRequestIsNull()
        {
            MockHttpBatchHandler mockHandler = new MockHttpBatchHandler(new HttpServer());

            Assert.ThrowsArgumentNull(() =>
                mockHandler.SendAsync(null).Wait(),
                "request");
        }

        [Fact]
        public void SendAsync_CallsProcessBatchAsync()
        {
            Mock<HttpBatchHandler> handler = new Mock<HttpBatchHandler>(new HttpServer());
            handler.Setup(h => h.ProcessBatchAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.Redirect)
                {
                    Content = new StringContent("ProcessBatchAsync called.")
                }));
            HttpMessageInvoker invoker = new HttpMessageInvoker(handler.Object);

            var response = invoker.SendAsync(new HttpRequestMessage(), CancellationToken.None).Result;

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("ProcessBatchAsync called.", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void SendAsync_ReturnsHttpResponseException()
        {
            Mock<HttpBatchHandler> handler = new Mock<HttpBatchHandler>(new HttpServer());
            handler.Setup(h => h.ProcessBatchAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .Returns(() =>
                {
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent("HttpResponseException Error.")
                    });
                });
            HttpMessageInvoker invoker = new HttpMessageInvoker(handler.Object);

            var response = invoker.SendAsync(new HttpRequestMessage(), CancellationToken.None).Result;

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("HttpResponseException Error.", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void SendAsync_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            Mock<HttpBatchHandler> handler = new Mock<HttpBatchHandler>(new HttpServer());
            handler.Setup(h => h.ProcessBatchAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .Returns(() =>
                {
                    throw new InvalidOperationException();
                });
            HttpMessageInvoker invoker = new HttpMessageInvoker(handler.Object);

            var response = invoker.SendAsync(new HttpRequestMessage(), CancellationToken.None).Result;

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        private class MockHttpBatchHandler : HttpBatchHandler
        {
            public MockHttpBatchHandler(HttpServer server)
                : base(server)
            {
            }

            public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
            {
                return SendAsync(request, CancellationToken.None);
            }

            public override Task<HttpResponseMessage> ProcessBatchAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage());
            }
        }
    }
}