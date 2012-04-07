// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    [CLSCompliant(false)]
    public class HttpRequestExtensionsTest
    {
        [Fact]
        public void GetHttpMethodOverrideWithNullRequestThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => HttpRequestExtensions.GetHttpMethodOverride(null),
                "request"
                );
        }

        [Theory]
        [InlineData("GET", "PUT", null, null, "GET")] // Cannot override GET with header PUT
        [InlineData("GET", null, "PUT", null, "GET")] // Cannot override GET with form PUT
        [InlineData("GET", null, null, "PUT", "GET")] // Cannot override GET with query string PUT
        [InlineData("PUT", "GET", null, null, "PUT")] // Cannot override PUT with GET
        [InlineData("PUT", "POST", null, null, "PUT")] // Cannot override PUT with POST
        [InlineData("POST", "GET", null, null, "POST")] // Cannot override POST with GET
        [InlineData("POST", "POST", null, null, "POST")] // Cannot override POST with POST
        [InlineData("POST", "PUT", null, null, "PUT")] // Can override POST with header PUT
        [InlineData("POST", null, "PUT", null, "PUT")] // Can override POST with form PUT
        [InlineData("POST", null, null, "PUT", "PUT")] // Can override POST with query string PUT
        [InlineData("POST", "PUT", "BOGUS", null, "PUT")] // Header override wins over form override
        [InlineData("POST", "PUT", null, "BOGUS", "PUT")] // Header override wins over query string override
        [InlineData("POST", null, "PUT", "BOGUS", "PUT")] // Form override wins over query string override
        public void TestHttpMethodOverride(string httpRequestVerb,
                                           string httpHeaderVerb,
                                           string httpFormVerb,
                                           string httpQueryStringVerb,
                                           string expectedMethod)
        {
            // Arrange
            ControllerContext context = AcceptVerbsAttributeTest.GetControllerContextWithHttpVerb(httpRequestVerb, httpHeaderVerb, httpFormVerb, httpQueryStringVerb);

            // Act
            string methodOverride = context.RequestContext.HttpContext.Request.GetHttpMethodOverride();

            // Assert
            Assert.Equal(expectedMethod, methodOverride);
        }
    }
}
