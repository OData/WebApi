// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http
{
    public class HttpUnsortedRequestTest
    {
        [Fact]
        public void Constructor_InitializesHeaders()
        {
            HttpUnsortedRequest request = new HttpUnsortedRequest();
            Assert.IsType<HttpUnsortedHeaders>(request.HttpHeaders);
        }
    }
}
