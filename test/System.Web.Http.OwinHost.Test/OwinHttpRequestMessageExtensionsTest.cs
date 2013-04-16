// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.TestCommon;

namespace System.Web.Http.OwinHost
{
    public class OwinHttpRequestMessageExtensionsTest
    {
        [Fact]
        public void GetOwinEnvironment_ReturnsNull_WhenNotSet()
        {
            Assert.Null(new HttpRequestMessage().GetOwinEnvironment());
        }

        [Fact]
        public void GetOwinEnvironment_ReturnsSetEnvironment()
        {
            var request = new HttpRequestMessage();
            var environment = new Dictionary<string, object>();
            request.SetOwinEnvironment(environment);

            Assert.Equal(environment, request.GetOwinEnvironment());
        }
    }
}
