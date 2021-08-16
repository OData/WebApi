//-----------------------------------------------------------------------------
// <copyright file="ChangeSetRequestItemTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ChangeSetRequestItemTest
    {
        [Fact]
        public void Parameter_Constructor()
        {
            HttpRequestMessage[] requests = new HttpRequestMessage[0];
            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(requests);

            Assert.Same(requests, requestItem.Requests);
        }

        [Fact]
        public void Constructor_NullRequests_Throws()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new ChangeSetRequestItem(null),
                "requests");
        }

        [Fact]
        public async Task SendRequestAsync_NullInvoker_Throws()
        {
            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(new HttpRequestMessage[0]);

            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => requestItem.SendRequestAsync(null, CancellationToken.None),
                "invoker");
        }

        [Fact]
        public async Task SendRequestAsync_ReturnsChangeSetResponse()
        {
            HttpRequestMessage[] requests = new HttpRequestMessage[]
                {
                    new HttpRequestMessage(HttpMethod.Get, "http://example.com"),
                    new HttpRequestMessage(HttpMethod.Post, "http://example.com")
                };
            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(requests);

            Mock<HttpMessageInvoker> invoker = new Mock<HttpMessageInvoker>(new HttpServer());
            invoker.Setup(i => i.SendAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .Returns(() =>
                {
                    return Task.FromResult(new HttpResponseMessage());
                });

            var response = await requestItem.SendRequestAsync(invoker.Object, CancellationToken.None);

            var changesetResponse = Assert.IsType<ChangeSetResponseItem>(response);
            Assert.Equal(2, changesetResponse.Responses.Count());
        }

        [Fact]
        public async Task SendRequestAsync_ReturnsSingleErrorResponse()
        {
            HttpRequestMessage[] requests = new HttpRequestMessage[]
                {
                    new HttpRequestMessage(HttpMethod.Get, "http://example.com"),
                    new HttpRequestMessage(HttpMethod.Post, "http://example.com"),
                    new HttpRequestMessage(HttpMethod.Put, "http://example.com")
                };
            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(requests);

            Mock<HttpMessageInvoker> invoker = new Mock<HttpMessageInvoker>(new HttpServer());
            invoker.Setup(i => i.SendAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .Returns<HttpRequestMessage, CancellationToken>((req, c) =>
                {
                    if (req.Method == HttpMethod.Post)
                    {
                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
                    }
                    return Task.FromResult(new HttpResponseMessage());
                });

            var response = await requestItem.SendRequestAsync(invoker.Object, CancellationToken.None);

            var changesetResponse = Assert.IsType<ChangeSetResponseItem>(response);
            Assert.Single(changesetResponse.Responses);
            Assert.Equal(HttpStatusCode.BadRequest, changesetResponse.Responses.First().StatusCode);
        }

        [Fact]
        public async Task SendRequestAsync_DisposesResponseInCaseOfException()
        {
            List<MockHttpResponseMessage> responses = new List<MockHttpResponseMessage>();
            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(new HttpRequestMessage[]
            {
                new HttpRequestMessage(HttpMethod.Get, "http://example.com"),
                new HttpRequestMessage(HttpMethod.Post, "http://example.com"),
                new HttpRequestMessage(HttpMethod.Put, "http://example.com")
            });
            Mock<HttpMessageInvoker> invoker = new Mock<HttpMessageInvoker>(new HttpServer());
            invoker.Setup(i => i.SendAsync(It.IsAny<HttpRequestMessage>(), CancellationToken.None))
                .Returns<HttpRequestMessage, CancellationToken>((req, cancel) =>
                {
                    if (req.Method == HttpMethod.Put)
                    {
                        throw new InvalidOperationException();
                    }
                    var response = new MockHttpResponseMessage();
                    responses.Add(response);
                    return Task.FromResult<HttpResponseMessage>(response);
                });

            await ExceptionAssert.ThrowsAsync<InvalidOperationException>(
                () => requestItem.SendRequestAsync(invoker.Object, CancellationToken.None));

            Assert.Equal(2, responses.Count);
            foreach (var response in responses)
            {
                Assert.True(response.IsDisposed);
            }
        }

        [Fact]
        public void GetResourcesForDisposal_ReturnsResourcesRegisteredForDispose()
        {
            var disposeObject1 = new StringContent("foo");
            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            request1.RegisterForDispose(disposeObject1);
            var disposeObject2 = new StringContent("bar");
            var request2 = new HttpRequestMessage(HttpMethod.Post, "http://example.com");
            request2.RegisterForDispose(disposeObject2);

            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(new HttpRequestMessage[] { request1, request2 });

            var resourcesForDisposal = requestItem.GetResourcesForDisposal();

            Assert.Equal(2, resourcesForDisposal.Count());
            Assert.Contains(disposeObject1, resourcesForDisposal);
            Assert.Contains(disposeObject2, resourcesForDisposal);
        }

        [Fact]
        public void Dispose_DisposesAllHttpRequestMessages()
        {
            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(new MockHttpRequestMessage[]
            {
                new MockHttpRequestMessage(),
                new MockHttpRequestMessage(),
                new MockHttpRequestMessage()
            });

            requestItem.Dispose();

            Assert.Equal(3, requestItem.Requests.Count());
            foreach (var request in requestItem.Requests)
            {
                Assert.True(((MockHttpRequestMessage)request).IsDisposed);
            }
        }
    }
}
#endif
