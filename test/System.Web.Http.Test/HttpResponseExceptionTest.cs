// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

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
        }
    }
}
