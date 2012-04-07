// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;
using FactAttribute = Microsoft.TestCommon.DefaultTimeoutFactAttribute;
using TheoryAttribute = Microsoft.TestCommon.DefaultTimeoutTheoryAttribute;

namespace System.Net.Http
{
    public class HttpRequestHeadersExtensionsTest
    {
        [Fact]
        public void GetCookies_ThrowsOnNull()
        {
            HttpRequestHeaders headers = CreateHttpRequestHeaders();

            Assert.ThrowsArgumentNull(() => HttpRequestHeadersExtensions.GetCookies(null), "headers");
        }

        [Fact]
        public void GetCookies_GetsCookiesReturnsEmptyCollection()
        {
            // Arrange
            HttpRequestHeaders headers = CreateHttpRequestHeaders();

            // Act
            IEnumerable<CookieHeaderValue> cookies = headers.GetCookies();

            // Assert
            Assert.Equal(0, cookies.Count());
        }

        [Theory]
        [InlineData("name1=n1=v1&n2=v2&n3=v3; expires=Sun, 06 Nov 1994 08:49:37 GMT; max-age=86400; domain=domain1; path=path1; secure; httponly")]
        public void GetCookies_GetsCookies(string expectedCookie)
        {
            // Arrange
            HttpRequestHeaders headers = CreateHttpRequestHeaders();
            headers.TryAddWithoutValidation("Cookie", expectedCookie);

            // Act
            IEnumerable<CookieHeaderValue> cookies = headers.GetCookies();

            // Assert
            Assert.Equal(1, cookies.Count());
            string actualCookie = cookies.ElementAt(0).ToString();
            Assert.Equal(expectedCookie, actualCookie);
        }

        private static HttpRequestHeaders CreateHttpRequestHeaders()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            return request.Headers;
        }
    }
}
