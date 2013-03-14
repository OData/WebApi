// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Principal;
using System.Web.Http.Controllers;

namespace System.Web.Http.Filters
{
    public interface IAuthenticationResult
    {
        IPrincipal Principal { get; }

        IHttpActionResult ErrorResult { get; }
    }
}
