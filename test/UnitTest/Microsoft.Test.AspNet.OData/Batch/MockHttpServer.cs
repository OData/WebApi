// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Test.AspNet.OData.Factories;

namespace Microsoft.Test.AspNet.OData.Batch
{
    internal class MockHttpServer : HttpServer
    {
        private Func<HttpRequestMessage, Task<HttpResponseMessage>> _action;

        public MockHttpServer(Func<HttpRequestMessage, HttpResponseMessage> action)
            : base(RoutingConfigurationFactory.CreateWithRootContainer("OData"))
        {
            _action = request =>
            {
                return Task.FromResult(action(request));
            };
        }

        public MockHttpServer(Func<HttpRequestMessage, Task<HttpResponseMessage>> action)
            : base(RoutingConfigurationFactory.CreateWithRootContainer("OData"))
        {
            _action = action;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _action(request);
        }
    }
}
#endif