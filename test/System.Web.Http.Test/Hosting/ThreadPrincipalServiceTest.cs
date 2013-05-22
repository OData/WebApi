// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Hosting
{
    public class ThreadPrincipalServiceTest
    {
        private readonly IHostPrincipalService _service;

        public ThreadPrincipalServiceTest()
        {
            _service = new ThreadPrincipalService();
        }

        [Fact]
        public void GetCurrentPrincipal_ReturnsThreadCurrentPrincipal()
        {
            // Arrange
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            HttpRequestMessage ignored = null;

            IPrincipal principal;

            using (new ThreadCurrentPrincipalContext())
            {
                Thread.CurrentPrincipal = expectedPrincipal;

                // Act
                principal = _service.GetCurrentPrincipal(ignored);
            }

            // Assert
            Assert.Same(expectedPrincipal, principal);
        }

        [Fact]
        public void SetCurrentPrincipal_SetsThreadCurrentPrincipal()
        {
            // Arrange
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            HttpRequestMessage ignored = null;

            IPrincipal principal;

            using (new ThreadCurrentPrincipalContext())
            {
                // Act
                _service.SetCurrentPrincipal(expectedPrincipal, ignored);

                principal = Thread.CurrentPrincipal;
            }

            // Assert
            Assert.Same(expectedPrincipal, principal);
        }

        private IPrincipal CreateDummyPrincipal()
        {
            return new Mock<IPrincipal>(MockBehavior.Strict).Object;
        }

        private sealed class ThreadCurrentPrincipalContext : IDisposable
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
}
