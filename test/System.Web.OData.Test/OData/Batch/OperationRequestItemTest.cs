// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Batch;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Test
{
    public class OperationRequestItemTest
    {
        [Fact]
        public void Parameter_Constructor()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            OperationRequestItem requestItem = new OperationRequestItem(request);

            Assert.Same(request, requestItem.Request);
        }

        [Fact]
        public void Constructor_NullRequests_Throws()
        {
            Assert.ThrowsArgumentNull(
                () => new OperationRequestItem(null),
                "request");
        }

        [Fact]
        public void SendRequestAsync_NullInvoker_Throws()
        {
            OperationRequestItem requestItem = new OperationRequestItem(new HttpRequestMessage());

            Assert.ThrowsArgumentNull(
                () => requestItem.SendRequestAsync(null, CancellationToken.None).Wait(),
                "invoker");
        }

        [Fact]
        public void SendRequestAsync_ReturnsOperationResponse()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            OperationRequestItem requestItem = new OperationRequestItem(request);

            Mock<HttpMessageInvoker> invoker = new Mock<HttpMessageInvoker>(new HttpServer());
            invoker.Setup(i => i.SendAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .Returns(() =>
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotModified));
                });

            var response = requestItem.SendRequestAsync(invoker.Object, CancellationToken.None).Result;

            var operationResponse = Assert.IsType<OperationResponseItem>(response);
            Assert.Equal(HttpStatusCode.NotModified, operationResponse.Response.StatusCode);
        }

        [Fact]
        public void GetResourcesForDisposal_ReturnsResourceRegisteredForDispose()
        {
            var disposeObject = new StringContent("foo");
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            request.RegisterForDispose(disposeObject);

            OperationRequestItem requestItem = new OperationRequestItem(request);

            var resourcesForDisposal = requestItem.GetResourcesForDisposal();

            Assert.Equal(1, resourcesForDisposal.Count());
            Assert.Contains(disposeObject, resourcesForDisposal);
        }

        [Fact]
        public void Dispose_DisposesHttpRequestMessage()
        {
            OperationRequestItem requestItem = new OperationRequestItem(new MockHttpRequestMessage());

            requestItem.Dispose();

            Assert.True(((MockHttpRequestMessage)requestItem.Request).IsDisposed);
        }
    }
}