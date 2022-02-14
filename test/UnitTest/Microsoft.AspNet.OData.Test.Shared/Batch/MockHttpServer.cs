//-----------------------------------------------------------------------------
// <copyright file="MockHttpServer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Test.Abstraction;

namespace Microsoft.AspNet.OData.Test.Batch
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
