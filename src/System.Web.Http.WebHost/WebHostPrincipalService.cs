// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Principal;
using System.Threading;
using System.Web.Http.Controllers;

namespace System.Web.Http.WebHost
{
    public class WebHostPrincipalService : IHostPrincipalService
    {
        public IPrincipal CurrentPrincipal
        {
            get
            {
                return HttpContext.Current.User;
            }
            set
            {
                HttpContext.Current.User = value;
                Thread.CurrentPrincipal = value;
            }
        }
    }
}
