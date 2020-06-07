// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Abstraction;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter
{
    public class ContentTypeMediaTypeMappingTests
    {
        [Fact]
        public void TryMatchMediaType_MatchesRequest_WithMatchingContentType()
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
            Assert.Equal(1, new ContentTypeMediaTypeMapping("text/plain").TryMatchMediaType(request));
        }

        [Fact]
        public void TryMatchMediaType_DoesNotMatchRequest_WithNonMatchingContentType()
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
            Assert.Equal(0, new ContentTypeMediaTypeMapping("text/plain").TryMatchMediaType(request));
        }

        [Fact]
        public void TryMatchMediaType_MatchesRequest_WithNonExactContentType()
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
            Assert.True(new ContentTypeMediaTypeMapping("text/plain").TryMatchMediaType(request) > 0);
        }
    }
}
