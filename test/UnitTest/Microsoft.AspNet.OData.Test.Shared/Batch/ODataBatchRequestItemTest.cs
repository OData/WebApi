// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Test.Common;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Batch
{
    public class ODataBatchRequestItemTest
    {
        [Fact]
        public async Task SendMessageAsync_Throws_WhenInvokerIsNull()
        {
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchRequestItem.SendMessageAsync(null, new HttpRequestMessage(), CancellationToken.None, null),
                "invoker");
        }

        [Fact]
        public async Task SendMessageAsync_Throws_WhenRequestIsNull()
        {
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchRequestItem.SendMessageAsync(new HttpMessageInvoker(new HttpServer()), null, CancellationToken.None, null),
                "request");
        }

        [Fact]
        public async Task SendMessageAsync_CallsInvoker()
        {
            HttpResponseMessage response = new HttpResponseMessage();

            HttpMessageInvoker invoker = new HttpMessageInvoker(new MockHttpServer((request) =>
                {
                    return response;
                }));

            var result = await ODataBatchRequestItem.SendMessageAsync(invoker, new HttpRequestMessage(HttpMethod.Get, "http://example.com"), CancellationToken.None, new Dictionary<string, string>());

            Assert.Same(response, result);
        }
    }
}
#else
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Batch
{
    public class ODataBatchRequestItemTest
    {
        [Fact]
        public async Task SendMessageAsync_Throws_WhenInvokerIsNull()
        {
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchRequestItem.SendRequestAsync(null, new DefaultHttpContext(), null),
                "handler");
        }

        [Fact]
        public async Task SendMessageAsync_Throws_WhenRequestIsNull()
        {
            RequestDelegate handler = (c) => { return Task.CompletedTask; };
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchRequestItem.SendRequestAsync(handler, null, null),
                "context");
        }

        [Fact]
        public async Task SendMessageAsync_Resolves_Uri_From_ContentId()
        {
			// Arrange
			DefaultHttpContext context = new DefaultHttpContext();
            HttpResponseMessage response = new HttpResponseMessage();
            RequestDelegate handler = (c) => { return Task.FromResult(response); };
            Dictionary<string, string> contentIdLocationMappings = new Dictionary<string, string>();
            contentIdLocationMappings.Add("1", "http://localhost:12345/odata/Customers(42)");
            Uri unresolvedUri = new Uri("http://localhost:12345/odata/$1/Orders");
            context.Request.CopyAbsoluteUrl(unresolvedUri);
			
			// Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, contentIdLocationMappings);

            // Assert
            Assert.Equal("/odata/Customers(42)/Orders", context.Request.Path.ToString());
        }
    }
}
#endif