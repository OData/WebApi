// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.OData.Batch;
using Microsoft.TestCommon;

namespace System.Web.OData.Test
{
    public class ODataBatchRequestItemTest
    {
        [Fact]
        public void SendMessageAsync_Throws_WhenInvokerIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => ODataBatchRequestItem.SendMessageAsync(null, new HttpRequestMessage(), CancellationToken.None, null).Wait(),
                "invoker");
        }

        [Fact]
        public void SendMessageAsync_Throws_WhenRequestIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => ODataBatchRequestItem.SendMessageAsync(new HttpMessageInvoker(new HttpServer()), null, CancellationToken.None, null).Wait(),
                "request");
        }

        [Fact]
        public void SendMessageAsync_CallsInvoker()
        {
            HttpResponseMessage response = new HttpResponseMessage();

            HttpMessageInvoker invoker = new HttpMessageInvoker(new MockHttpServer((request) =>
                {
                    return response;
                }));

            var result = ODataBatchRequestItem.SendMessageAsync(invoker, new HttpRequestMessage(HttpMethod.Get, "http://example.com"), CancellationToken.None, new Dictionary<string, string>()).Result;

            Assert.Same(response, result);
        }
    }
}