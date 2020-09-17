// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        private static ODataMessageQuotas _odataMessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue };

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