// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Principal;
using System.Threading;
using System.Web.Http.Controllers;

namespace System.Web.Http.WebHost
{
    public class WebHostPrincipalProvider : IPrincipalProvider
    {
        public IPrincipal CurrentPrincipal
        {
            get
            {
                return Thread.CurrentPrincipal;
            }
            set
            {
                Thread.CurrentPrincipal = value;
                HttpContext.Current.User = value;
            }
        }
    }
}
