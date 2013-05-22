// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Principal;
using System.Threading;

namespace System.Web.Http.Hosting
{
    internal sealed class ThreadCurrentPrincipalContext : IDisposable
    {
        private readonly IPrincipal originalPrincipal;

        private bool disposed;

        public ThreadCurrentPrincipalContext()
        {
            originalPrincipal = Thread.CurrentPrincipal;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                Thread.CurrentPrincipal = originalPrincipal;
                disposed = true;
            }
        }
    }
}
