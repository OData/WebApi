// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Principal;
using System.Threading;

namespace System.Web.Http.Hosting
{
    /// <summary>Represents a host principal of <see cref="Thread.CurrentPrincipal"/>.</summary>
    public class ThreadPrincipalService : IHostPrincipalService
    {
        /// <inheritdoc />
        public IPrincipal GetCurrentPrincipal(HttpRequestMessage request)
        {
            return Thread.CurrentPrincipal;
        }

        /// <inheritdoc />
        public void SetCurrentPrincipal(IPrincipal principal, HttpRequestMessage request)
        {
            Thread.CurrentPrincipal = principal;
        }
    }
}
