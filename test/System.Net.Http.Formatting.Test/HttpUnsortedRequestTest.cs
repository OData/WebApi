// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

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
