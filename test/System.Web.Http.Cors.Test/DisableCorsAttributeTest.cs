// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Web.Cors;
using Microsoft.TestCommon;

namespace System.Web.Http.Cors.Test
{
    public class DisableCorsAttributeTest
    {
        [Fact]
        public void GetCorsPolicyAsync_ReturnsNull()
        {
            DisableCorsAttribute disableCors = new DisableCorsAttribute();
            CorsPolicy corsPolicy = disableCors.GetCorsPolicyAsync(new HttpRequestMessage(), CancellationToken.None).Result;

            Assert.Null(corsPolicy);
        }
    }
}