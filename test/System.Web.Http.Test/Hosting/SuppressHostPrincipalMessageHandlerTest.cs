// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Hosting
{
    public class SuppressHostPrincipalMessageHandlerTest
    {
        [Fact]
        public void ConstructorWithConfiguration_SetsPrincipalService()
        {
            // Arrange
            IHostPrincipalService expectedPrincipalService = CreateDummyPrincipalService();

            IHostPrincipalService principalService;

            using (HttpConfiguration configuration = new HttpConfiguration())
            {
                configuration.Services.Replace(typeof(IHostPrincipalService), expectedPrincipalService);
                SuppressHostPrincipalMessageHandler handler = new SuppressHostPrincipalMessageHandler(configuration);

                // Act
                principalService = handler.HostPrincipalService;
            }

            // Assert
            Assert.Same(expectedPrincipalService, principalService);
        }

        [Fact]
        public void ConstructorWithConfiguration_Throws_WhenConfigurationIsNull()
        {
            // Arrange
            HttpConfiguration configuration = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { new SuppressHostPrincipalMessageHandler(configuration); },
                "configuration");
        }

        [Fact]
        public void ConstructorWithConfiguration_Throws_WhenPrincipalServiceIsNull()
        {
            // Arrange
            using (HttpConfiguration configuration = new HttpConfiguration())
            {
                configuration.Services.Replace(typeof(IHostPrincipalService), null);

                // Act & Assert
                InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                {
                    new SuppressHostPrincipalMessageHandler(configuration);
                });
                Assert.Equal("ServicesContainer must have an IHostPrincipalService.", exception.Message);
            }
        }

        [Fact]
        public void ConstructorWithPrincipalService_SetsPrincipalService()
        {
            // Arrange
            IHostPrincipalService expectedPrincipalService = CreateDummyPrincipalService();
            SuppressHostPrincipalMessageHandler handler = new SuppressHostPrincipalMessageHandler(
                expectedPrincipalService);

            // Act
            IHostPrincipalService principalService = handler.HostPrincipalService;

            // Assert
            Assert.Same(expectedPrincipalService, principalService);
        }

        [Fact]
        public void ConstructorWithPrincipalService_Throws_WhenPrincipalServiceIsNull()
        {
            // Arrange
            IHostPrincipalService principalService = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { new SuppressHostPrincipalMessageHandler(principalService); },
                "principalService");
        }

        [Fact]
        public void SendAsync_DelegatesToInnerHandler()
        {
            // Arrange
            IHostPrincipalService principalService = CreateStubPrincipalService();
            HttpRequestMessage request = null;
            CancellationToken cancellationToken = default(CancellationToken);
            Task<HttpResponseMessage> expectedResult = new Task<HttpResponseMessage>(() => null);
            HttpMessageHandler innerHandler = new LambdaHttpMessageHandler((r, c) =>
            {
                request = r;
                cancellationToken = c;
                return expectedResult;
            });
            HttpMessageHandler handler = CreateProductUnderTest(principalService, innerHandler);
            CancellationToken expectedCancellationToken = new CancellationToken(true);

            using (HttpRequestMessage expectedRequest = new HttpRequestMessage())
            {
                // Act
                Task<HttpResponseMessage> result = handler.SendAsync(expectedRequest, expectedCancellationToken);

                // Assert
                Assert.Same(expectedRequest, request);
                Assert.Equal(expectedCancellationToken, cancellationToken);
                Assert.Same(expectedResult, result);
            }
        }

        [Fact]
        public void SendAsync_SetsCurrentPrincipalToAnonymous_BeforeCallingInnerHandler()
        {
            // Arrange
            IPrincipal principalServiceCurrentPrincipal = null;
            IHostPrincipalService principalService = CreateSpyPrincipalService((p) =>
            {
                principalServiceCurrentPrincipal = p;
            });
            IPrincipal principalBeforeInnerHandler = null;
            HttpMessageHandler inner = new LambdaHttpMessageHandler((ignore1, ignore2) =>
            {
                principalBeforeInnerHandler = principalServiceCurrentPrincipal;
                return Task.FromResult<HttpResponseMessage>(null);
            });
            HttpMessageHandler handler = CreateProductUnderTest(principalService, inner);

            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                // Act
                handler.SendAsync(request, CancellationToken.None);
            }

            // Assert
            Assert.NotNull(principalBeforeInnerHandler);
            IIdentity identity = principalBeforeInnerHandler.Identity;
            Assert.NotNull(identity);
            Assert.False(identity.IsAuthenticated);
            Assert.Null(identity.Name);
            Assert.Null(identity.AuthenticationType);
        }

        private static HttpMessageHandler CreateDummyHandler()
        {
            return new DummyHttpMessageHandler();
        }

        private static IHostPrincipalService CreateDummyPrincipalService()
        {
            return new Mock<IHostPrincipalService>(MockBehavior.Strict).Object;
        }

        private static SuppressHostPrincipalMessageHandler CreateProductUnderTest(
            IHostPrincipalService principalService, HttpMessageHandler innerHandler)
        {
            SuppressHostPrincipalMessageHandler handler = new SuppressHostPrincipalMessageHandler(principalService);
            handler.InnerHandler = innerHandler;
            return handler;
        }

        private static IHostPrincipalService CreateSpyPrincipalService(Action<IPrincipal> setPrincipal)
        {
            Mock<IHostPrincipalService> mock = new Mock<IHostPrincipalService>(MockBehavior.Strict);
            mock.Setup(s => s.SetCurrentPrincipal(It.IsAny<IPrincipal>(),
                It.IsAny<HttpRequestMessage>())).Callback<IPrincipal, HttpRequestMessage>(
                (p, ignore) => { setPrincipal(p); });
            return mock.Object;
        }

        private static IHostPrincipalService CreateStubPrincipalService()
        {
            Mock<IHostPrincipalService> mock = new Mock<IHostPrincipalService>(MockBehavior.Strict);
            mock.Setup(s => s.SetCurrentPrincipal(It.IsAny<IPrincipal>(), It.IsAny<HttpRequestMessage>()));
            return mock.Object;
        }

        private class DummyHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private class LambdaHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

            public LambdaHttpMessageHandler(Func<HttpRequestMessage, CancellationToken,
                Task<HttpResponseMessage>> sendAsync)
            {
                if (sendAsync == null)
                {
                    throw new ArgumentNullException("sendAsync");
                }

                _sendAsync = sendAsync;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return _sendAsync.Invoke(request, cancellationToken);
            }
        }
    }
}
