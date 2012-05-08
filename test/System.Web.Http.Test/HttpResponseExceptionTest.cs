// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http
{
    public class HttpResponseExceptionTest
    {
        [Fact]
        public void Constructor_WhenResponseParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => new HttpResponseException(response: null), "response");
        }

        [Fact]
        public void Constructor_SetsResponseProperty()
        {
            var response = new HttpResponseMessage();

            var exception = new HttpResponseException(response);

            Assert.Same(response, exception.Response);
            if(Assert.CurrentCultureIsEnglish) {
                Assert.Equal("Processing of the HTTP request resulted in an exception. Please see the HTTP response returned by the 'Response' property of this exception for details.", exception.Message);
            }
        }

        [Fact]
        public void Constructor_SetsResponsePropertyWithGivenStatusCode()
        {
            var exception = new HttpResponseException(HttpStatusCode.BadGateway);

            Assert.Equal(HttpStatusCode.BadGateway, exception.Response.StatusCode);
            if (Assert.CurrentCultureIsEnglish)
            {
                Assert.Equal("Processing of the HTTP request resulted in an exception. Please see the HTTP response returned by the 'Response' property of this exception for details.", exception.Message);
            }
        }
    }
}
