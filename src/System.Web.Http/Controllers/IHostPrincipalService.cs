// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Principal;

namespace System.Web.Http.Controllers
{
    public interface IHostPrincipalService
    {
        IPrincipal GetCurrentPrincipal(HttpRequestMessage request);

        void SetCurrentPrincipal(IPrincipal principal, HttpRequestMessage request);
    }
}
