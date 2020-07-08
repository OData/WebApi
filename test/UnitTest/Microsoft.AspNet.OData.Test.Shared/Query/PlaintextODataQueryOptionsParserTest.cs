// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class PlainTextODataQueryOptionsParserTest
    {
        private const string QueryOptionsString = "$filter=Id le 5";

        [Fact]
        public async Task ParseAsync_WithQueryOptionsInStream()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(QueryOptionsString));

            var result = await new PlainTextODataQueryOptionsParser().ParseAsync(memoryStream);

            Assert.Equal('?' + QueryOptionsString, result);
        }

        [Fact]
        public async Task ParseAsync_WithDisposedStream()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(QueryOptionsString));
            memoryStream.Dispose();

            await Assert.ThrowsAsync<ODataException>(async() => await new PlainTextODataQueryOptionsParser().ParseAsync(memoryStream));
        }

        [Fact]
        public async Task ParseAsync_WithEmptyStream()
        {
            var memoryStream = new MemoryStream();

            var result = await new PlainTextODataQueryOptionsParser().ParseAsync(memoryStream);

            Assert.Equal("", result);
        }

        [Fact]
        public async Task ParseAsync_WithQueryStringSeparatorIncludedInStream()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes('?' + QueryOptionsString));

            var result = await new PlainTextODataQueryOptionsParser().ParseAsync(memoryStream);

            Assert.Equal('?' + QueryOptionsString, result);
        }

        [Fact]
        public void PlainTextODataQueryOptionsParser_IsReturnedBy_ODataQueryOptionsParserFactory()
        {
            var parsers = ODataQueryOptionsParserFactory.Create();

            Assert.Contains(parsers, p => p.GetType().Equals(typeof(PlainTextODataQueryOptionsParser)));
        }

        [Fact]
        public void CanParse_MatchesRequest_WithMatchingContentType()
        {
            var request = RequestFactory.Create(HttpMethod.Post, "http://localhost/People/$query");
            var payload = "$filter=Name eq 'Foo'";
            var contentType = "text/plain";
#if NETCORE
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(payload));
            request.ContentType = contentType;
#else
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse(contentType);
#endif
            Assert.True(new PlainTextODataQueryOptionsParser().CanParse(request));
        }

        [Fact]
        public void CanParse_DoesNotMatchRequest_WithNonMatchingContentType()
        {
            var request = RequestFactory.Create(HttpMethod.Post, "http://localhost/People/$query");
            var payload = "{}";
            var contentType = "application/json";
#if NETCORE
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(payload));
            request.ContentType = contentType;
#else
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse(contentType);
#endif
            Assert.False(new PlainTextODataQueryOptionsParser().CanParse(request));
        }

        [Fact]
        public void CanParse_MatchesRequest_WithNonExactContentType()
        {
            var request = RequestFactory.Create(HttpMethod.Post, "http://localhost/People/$query");
            var payload = "$filter=Name eq 'Foo'";
            var contentType = "text/plain;charset=utf-8";
#if NETCORE
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(payload));
            request.ContentType = contentType;
#else
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse(contentType);
#endif
            Assert.True(new PlainTextODataQueryOptionsParser().CanParse(request));
        }
    }
}
