// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Principal;

namespace System.Web.Http.Controllers
{
    public interface IHostPrincipalService
    {
        IPrincipal CurrentPrincipal { get; set; }
    }
}
