//-----------------------------------------------------------------------------
// <copyright file="ODataBatchReaderExtensionsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Batch
{
    public class ODataBatchReaderExtensionsTest
    {
        [Fact]
        public async Task ReadChangeSetRequest_NullReader_Throws()
        {
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchReaderExtensions.ReadChangeSetRequestAsync(null, Guid.NewGuid()),
                "reader");
        }

        [Fact]
        public async Task ReadChangeSetRequest_InvalidState_Throws()
        {
            var httpContent = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            var reader = await httpContent.GetODataMessageReaderAsync(new ODataMessageReaderSettings(), CancellationToken.None);
            await ExceptionAssert.ThrowsAsync<InvalidOperationException>(
                () => ODataBatchReaderExtensions.ReadChangeSetRequestAsync(reader.CreateODataBatchReader(), Guid.NewGuid(),
                    CancellationToken.None),
                "The current batch reader state 'Initial' is invalid. The expected state is 'ChangesetStart'.");
        }

        [Fact]
        public async Task ReadOperationRequest_NullReader_Throws()
        {
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchReaderExtensions.ReadOperationRequestAsync(null, Guid.NewGuid(), false),
                "reader");
        }

        [Fact]
        public async Task ReadOperationRequest_InvalidState_Throws()
        {
            var httpContent = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            var reader = await httpContent.GetODataMessageReaderAsync(new ODataMessageReaderSettings(), CancellationToken.None);
            ExceptionAssert.Throws<InvalidOperationException>(
                () => ODataBatchReaderExtensions.ReadOperationRequestAsync(reader.CreateODataBatchReader(), Guid.NewGuid(),
                    false, CancellationToken.None),
                "The current batch reader state 'Initial' is invalid. The expected state is 'Operation'.");
        }

        [Fact]
        public async Task ReadChangeSetOperationRequest_NullReader_Throws()
        {
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchReaderExtensions.ReadChangeSetOperationRequestAsync(null, Guid.NewGuid(), Guid.NewGuid(), false),
                "reader");
        }

        [Fact]
        public async Task ReadChangeSetOperationRequest_InvalidState_Throws()
        {
            var httpContent = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            var reader = await httpContent.GetODataMessageReaderAsync(new ODataMessageReaderSettings(), CancellationToken.None);
            ExceptionAssert.Throws<InvalidOperationException>(
                () => ODataBatchReaderExtensions.ReadChangeSetOperationRequestAsync(reader.CreateODataBatchReader(),
                    Guid.NewGuid(), Guid.NewGuid(), false, CancellationToken.None),
                "The current batch reader state 'Initial' is invalid. The expected state is 'Operation'.");
        }

        [Fact]
        public async Task ParseBatchRequestsAsync_DoesNotCopyBlockedHeaders()
        {
            string batchBoundary = "batch_" + Guid.NewGuid();
            string batchContent = $@"
--{batchBoundary}
Content-Type: application/http
Content-Transfer-Encoding: binary

GET http://localhost/odata/Customers HTTP/1.1
Host: other.example.com
Authorization: Bearer test-token
X-Forwarded-For: 10.0.0.1
X-MS-Client-Principal-Id: test-user-id
X-Custom-Header: retained


--{batchBoundary}--
";
            HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost/$batch");
            batchRequest.Content = new StringContent(batchContent);
            batchRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/mixed;boundary={batchBoundary}");
            batchRequest.EnableHttpDependencyInjectionSupport();

            DefaultODataBatchHandler handler = new DefaultODataBatchHandler(new System.Web.Http.HttpServer());
            IList<ODataBatchRequestItem> requests = await handler.ParseBatchRequestsAsync(batchRequest, CancellationToken.None);

            HttpRequestMessage subRequest = Assert.IsType<OperationRequestItem>(Assert.Single(requests)).Request;
            Assert.Null(subRequest.Headers.Host);
            Assert.False(subRequest.Headers.Contains("Authorization"));
            Assert.False(subRequest.Headers.Contains("X-Forwarded-For"));
            Assert.False(subRequest.Headers.Contains("X-MS-Client-Principal-Id"));
            Assert.Equal("retained", Assert.Single(subRequest.Headers.GetValues("X-Custom-Header")));
        }

        [Fact]
        public void ValidateRequestUri_Throws_WhenAuthorityDiffers()
        {
            ExceptionAssert.Throws<InvalidOperationException>(
                () => ODataBatchReaderExtensions.ValidateRequestUri(
                    new Uri("https://other.example.com/odata/Customers"),
                    new Uri("http://localhost/odata/")),
                "The batch sub-request URI 'https://other.example.com/odata/Customers' has a different authority",
                partialMatch: true);
        }

        [Fact]
        public void ValidateRequestUri_Throws_WhenPathIsOutsideServiceRoot()
        {
            ExceptionAssert.Throws<InvalidOperationException>(
                () => ODataBatchReaderExtensions.ValidateRequestUri(
                    new Uri("http://localhost/odata-admin/Customers"),
                    new Uri("http://localhost/odata/")),
                "The batch sub-request URI 'http://localhost/odata-admin/Customers' targets a path that is not within the OData service root",
                partialMatch: true);
        }

        [Fact]
        public async Task SendMessageAsync_Throws_WhenContentIdResolutionEscapesServiceRoot()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "$1/../../../admin/control");
            request.SetODataBatchServiceRoot(new Uri("http://localhost/odata/"));
            IDictionary<string, string> contentIdMapping = new Dictionary<string, string>
            {
                { "1", "http://localhost/odata/Orders(1)" }
            };

            using (HttpMessageInvoker invoker = new HttpMessageInvoker(new HttpClientHandler()))
            {
                await ExceptionAssert.ThrowsAsync<InvalidOperationException>(
                    () => ODataBatchRequestItem.SendMessageAsync(invoker, request, CancellationToken.None, contentIdMapping),
                    "targets a path that is not within the OData service root",
                    partialMatch: true);
            }
        }

        private static ODataMessageQuotas _odataMessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = ODataMessageSizeOptions.DefaultMaxReceivedMessageSize };

        [Theory]
        // if no accept header, return multipart/mixed
        [InlineData(null, "multipart/mixed")]

        // if accept is multipart/mixed, return multipart/mixed
        [InlineData(new[] { "multipart/mixed" }, "multipart/mixed")]

        // if accept is application/json, return application/json
        [InlineData(new[] { "application/json" }, "application/json")]

        // if accept is application/json with charset, return application/json
        [InlineData(new[] { "application/json; charset=utf-8" }, "application/json")]

        // if multipart/mixed is high proprity, return multipart/mixed
        [InlineData(new[] { "multipart/mixed;q=0.9", "application/json;q=0.5" }, "multipart/mixed")]
        [InlineData(new[] { "application/json;q=0.5", "multipart/mixed;q=0.9" }, "multipart/mixed")]

        // if application/json is high proprity, return application/json
        [InlineData(new[] { "application/json;q=0.9", "multipart/mixed;q=0.5" }, "application/json")]
        [InlineData(new[] { "multipart/mixed;q=0.5", "application/json;q=0.9" }, "application/json")]

        // if priorities are same, return first
        [InlineData(new[] { "multipart/mixed;q=0.9", "application/json;q=0.9" }, "multipart/mixed")]
        [InlineData(new[] { "multipart/mixed", "application/json" }, "multipart/mixed")]

        // if priorities are same, return first
        [InlineData(new[] { "application/json;q=0.9", "multipart/mixed;q=0.9" }, "application/json")]
        [InlineData(new[] { "application/json", "multipart/mixed" }, "application/json")]

        // no priority has q=1.0
        [InlineData(new[] { "application/json", "multipart/mixed;q=0.9" }, "application/json")]
        [InlineData(new[] { "application/json;q=0.9", "multipart/mixed" }, "multipart/mixed")]

        public async Task CreateODataBatchResponseAsync(string[] accept, string expected)
        {
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/$batch");
            var responses = new[] { new ChangeSetResponseItem(Enumerable.Empty<HttpResponseMessage>()) };

            if (accept != null)
            {
                request.Headers.Add("Accept", accept);
            }

            var response = await request.CreateODataBatchResponseAsync(responses, _odataMessageQuotas);

            Assert.StartsWith(expected, response.Content.Headers.ContentType.MediaType);
        }

        [Theory]
        // if no contentType, return multipart/mixed
        [InlineData(null, "multipart/mixed")]
        // if contentType is application/json, return application/json
        [InlineData("application/json", "application/json")]
        [InlineData("application/json; charset=utf-8", "application/json")]
        // if contentType is multipart/mixed, return multipart/mixed
        [InlineData("multipart/mixed", "multipart/mixed")]
        public async Task CreateODataBatchResponseAsyncWhenNoAcceptHeader(string contentType, string expected)
        {
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/$batch");
            var responses = new[] { new ChangeSetResponseItem(Enumerable.Empty<HttpResponseMessage>()) };

            if (contentType != null)
            {
                request.Content = new ByteArrayContent(new byte[] { });
                request.Content.Headers.Add("Content-Type", contentType);
            }

            var response = await request.CreateODataBatchResponseAsync(responses, _odataMessageQuotas);

            Assert.False(request.Headers.Contains("Accept")); // check no accept header
            Assert.StartsWith(expected, response.Content.Headers.ContentType.MediaType);
        }
    }
}
#endif
