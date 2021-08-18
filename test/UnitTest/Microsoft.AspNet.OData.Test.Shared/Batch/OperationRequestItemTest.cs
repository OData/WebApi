//-----------------------------------------------------------------------------
// <copyright file="OperationRequestItemTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Test.Common;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Batch
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
            ExceptionAssert.ThrowsArgumentNull(
                () => new OperationRequestItem(null),
                "request");
        }

        [Fact]
        public async Task SendRequestAsync_NullInvoker_Throws()
        {
            OperationRequestItem requestItem = new OperationRequestItem(new HttpRequestMessage());

            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => requestItem.SendRequestAsync(null, CancellationToken.None),
                "invoker");
        }

        [Fact]
        public async Task SendRequestAsync_ReturnsOperationResponse()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            OperationRequestItem requestItem = new OperationRequestItem(request);

            Mock<HttpMessageInvoker> invoker = new Mock<HttpMessageInvoker>(new HttpServer());
            invoker.Setup(i => i.SendAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .Returns(() =>
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotModified));
                });

            var response = await requestItem.SendRequestAsync(invoker.Object, CancellationToken.None);

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

            Assert.Single(resourcesForDisposal);
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
#endif
