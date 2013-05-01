// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.WebHost.Properties;

namespace System.Web.Http.WebHost
{
    public class WebHostPrincipalService : IHostPrincipalService
    {
        public IPrincipal GetCurrentPrincipal(HttpRequestMessage request)
        {
            HttpContextBase context = GetHttpContext(request);
            return context.User;
        }

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
