// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Principal;
using System.Threading;

namespace System.Web.Http.Controllers
{
    public class ThreadPrincipalService : IHostPrincipalService
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
            }
        }
    }
}
