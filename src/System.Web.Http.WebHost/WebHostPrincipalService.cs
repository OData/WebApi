// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web.Http.Hosting;
using System.Web.Http.WebHost.Properties;

namespace System.Web.Http.WebHost
{
    /// <summary>Represents a host principal of <see cref="HttpContextBase.User"/>.</summary>
    public class WebHostPrincipalService : IHostPrincipalService
    {
        /// <inheritdoc />
        public IPrincipal GetCurrentPrincipal(HttpRequestMessage request)
        {
            HttpContextBase context = GetHttpContext(request);
            return context.User;
        }

        /// <inheritdoc />
        public void SetCurrentPrincipal(IPrincipal principal, HttpRequestMessage request)
        {
            HttpContextBase context = GetHttpContext(request);
            context.User = principal;
            Thread.CurrentPrincipal = principal;
        }

        private static HttpContextBase GetHttpContext(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            HttpContextBase context = request.GetHttpContext();

            if (context == null)
            {
                throw new InvalidOperationException(SRResources.HttpContextPropertyMissing);
            }

            return context;
        }
    }
}
