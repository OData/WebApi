// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web.Http.WebHost;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Hosting
{
    public class WebHostPrincipalServiceTest
    {
        private readonly IHostPrincipalService _service;

        public WebHostPrincipalServiceTest()
        {
            _service = new WebHostPrincipalService();
        }

        [Fact]
        public void GetCurrentPrincipal_ReturnsRequestContextUser()
        {
            // Arrange
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            HttpContextBase context = CreateStubContextWithUser(expectedPrincipal);

            IPrincipal principal;

            using (HttpRequestMessage request = CreateRequestWithContext(context))
            {
                // Act
                principal = _service.GetCurrentPrincipal(request);
            }

            // Assert
            Assert.Same(expectedPrincipal, principal);
        }

        [Fact]
        public void GetCurrentPrincipal_Throws_WhenRequestIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(() => { _service.GetCurrentPrincipal(null); }, "request");
        }

        [Fact]
        public void GetCurrentPrincipal_Throws_WhenRequestContextIsNull()
        {
            // Arrange
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                // Act & Assert
                InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                {
                    _service.GetCurrentPrincipal(request);
                });
                Assert.Equal("No HttpContextBase is associated with the request.", exception.Message);
            }
        }

        [Fact]
        public void SetCurrentPrincipal_SetsRequestContextUser()
        {
            // Arrange
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            HttpContextBase context = CreateStubContextWithUser();

            IPrincipal principal;

            using (HttpRequestMessage request = CreateRequestWithContext(context))
            using (new ThreadCurrentPrincipalContext())
            {
                // Act
                _service.SetCurrentPrincipal(expectedPrincipal, request);

                principal = context.User;
            }

            // Assert
            Assert.Same(expectedPrincipal, principal);
        }

        [Fact]
        public void SetCurrentPrincipal_SetsThreadCurrentPrincipal()
        {
            // Arrange
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            HttpContextBase context = CreateStubContextWithUser();

            IPrincipal principal;

            using (HttpRequestMessage request = CreateRequestWithContext(context))
            using (new ThreadCurrentPrincipalContext())
            {
                // Act
                _service.SetCurrentPrincipal(expectedPrincipal, request);

                principal = Thread.CurrentPrincipal;
            }

            // Assert
            Assert.Same(expectedPrincipal, principal);
        }

        [Fact]
        public void SetCurrentPrincipal_Throws_WhenRequestIsNull()
        {
            // Arrange
            IPrincipal principal = CreateDummyPrincipal();

            using (new ThreadCurrentPrincipalContext())
            {
                // Act & Assert
                Assert.ThrowsArgumentNull(() => { _service.SetCurrentPrincipal(principal, null); }, "request");
            }
        }

        [Fact]
        public void SetCurrentPrincipal_Throws_WhenRequestContextIsNull()
        {
            // Arrange
            IPrincipal principal = CreateDummyPrincipal();

            // Arrange
            using (HttpRequestMessage request = new HttpRequestMessage())
            using (new ThreadCurrentPrincipalContext())
            {
                // Act & Assert
                InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                {
                    _service.GetCurrentPrincipal(request);
                });
                Assert.Equal("No HttpContextBase is associated with the request.", exception.Message);
            }
        }

        private IPrincipal CreateDummyPrincipal()
        {
            return new Mock<IPrincipal>(MockBehavior.Strict).Object;
        }

        private HttpRequestMessage CreateRequestWithContext(HttpContextBase context)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetHttpContext(context);
            return request;
        }

        private HttpContextBase CreateStubContextWithUser()
        {
            Mock<HttpContextBase> mock = new Mock<HttpContextBase>();
            mock.SetupProperty(c => c.User);
            return mock.Object;
        }

        private HttpContextBase CreateStubContextWithUser(IPrincipal user)
        {
            Mock<HttpContextBase> mock = new Mock<HttpContextBase>();
            mock.Setup(c => c.User).Returns(user);
            return mock.Object;
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
