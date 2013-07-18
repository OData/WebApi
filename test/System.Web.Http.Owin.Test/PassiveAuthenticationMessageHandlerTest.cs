// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Hosting;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Owin
{
    public class PassiveAuthenticationMessageHandlerTest
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
                PassiveAuthenticationMessageHandler handler = new PassiveAuthenticationMessageHandler(configuration);

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
            Assert.ThrowsArgumentNull(() => { new PassiveAuthenticationMessageHandler(configuration); },
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
                    new PassiveAuthenticationMessageHandler(configuration);
                });
                Assert.Equal("ServicesContainer must have an IHostPrincipalService.", exception.Message);
            }
        }

        [Fact]
        public void ConstructorWithPrincipalService_SetsPrincipalService()
        {
            // Arrange
            IHostPrincipalService expectedPrincipalService = CreateDummyPrincipalService();
            PassiveAuthenticationMessageHandler handler = new PassiveAuthenticationMessageHandler(
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
            Assert.ThrowsArgumentNull(() => { new PassiveAuthenticationMessageHandler(principalService); },
                "principalService");
        }

        [Fact]
        public void SendAsync_DelegatesToInnerHandler()
        {
            // Arrange
            IHostPrincipalService principalService = CreateStubPrincipalService();
            HttpRequestMessage request = null;
            CancellationToken cancellationToken = default(CancellationToken);
            IOwinContext context = CreateOwinContext();

            using (HttpResponseMessage expectedResponse = new HttpResponseMessage())
            {
                HttpMessageHandler innerHandler = new LambdaHttpMessageHandler((r, c) =>
                {
                    request = r;
                    cancellationToken = c;
                    return Task.FromResult(expectedResponse);
                });
                HttpMessageHandler handler = CreateProductUnderTest(principalService, innerHandler);
                CancellationToken expectedCancellationToken = new CancellationToken(true);

                using (HttpRequestMessage expectedRequest = CreateRequestWithOwinContext(context))
                {
                    // Act
                    HttpResponseMessage response = handler.SendAsync(expectedRequest,
                        expectedCancellationToken).Result;

                    // Assert
                    Assert.Same(expectedRequest, request);
                    Assert.Equal(expectedCancellationToken, cancellationToken);
                    Assert.Same(expectedResponse, response);
                }
            }
        }

        [Fact]
        public void SendAsync_Throws_WhenOwinContextIsNull()
        {
            // Arrange
            IHostPrincipalService principalService = CreateStubPrincipalService();
            HttpMessageHandler innerHandler = CreateStubHandler();
            HttpMessageHandler handler = CreateProductUnderTest(principalService, innerHandler);

            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                // Act & Assert
                InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                {
                    HttpResponseMessage ignore = handler.SendAsync(request, CancellationToken.None).Result;
                });
                Assert.Equal("No OWIN authentication manager is associated with the request.", exception.Message);
            }
        }

        [Fact]
        public void SendAsync_Throws_WhenAuthenticationManagerIsNull()
        {
            // Arrange
            IHostPrincipalService principalService = CreateStubPrincipalService();
            HttpMessageHandler innerHandler = CreateStubHandler();
            HttpMessageHandler handler = CreateProductUnderTest(principalService, innerHandler);
            IOwinContext context = CreateOwinContext(null);

            using (HttpRequestMessage request = CreateRequestWithOwinContext(context))
            {
                // Act & Assert
                InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                {
                    HttpResponseMessage ignore = handler.SendAsync(request, CancellationToken.None).Result;
                });
                Assert.Equal("No OWIN authentication manager is associated with the request.", exception.Message);
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
            IOwinContext context = CreateOwinContext();

            using (HttpRequestMessage request = CreateRequestWithOwinContext(context))
            {
                // Act
                HttpResponseMessage ignore = handler.SendAsync(request, CancellationToken.None).Result;
            }

            // Assert
            Assert.NotNull(principalBeforeInnerHandler);
            IIdentity identity = principalBeforeInnerHandler.Identity;
            Assert.NotNull(identity);
            Assert.False(identity.IsAuthenticated);
            Assert.Null(identity.Name);
            Assert.Null(identity.AuthenticationType);
        }

        [Fact]
        public void SendAsync_SuppressesAuthenticationChallenges_WhenNoChallengeIsSet()
        {
            // Arrange
            IHostPrincipalService principalService = CreateStubPrincipalService();
            HttpMessageHandler inner = CreateStubHandler();
            HttpMessageHandler handler = CreateProductUnderTest(principalService, inner);
            IOwinContext context = CreateOwinContext();

            using (HttpRequestMessage request = CreateRequestWithOwinContext(context))
            {
                // Act
                HttpResponseMessage ignore = handler.SendAsync(request, CancellationToken.None).Result;
            }

            // Assert
            IAuthenticationManager authenticationManager = context.Authentication;
            AuthenticationResponseChallenge challenge = authenticationManager.AuthenticationResponseChallenge;
            Assert.NotNull(challenge);
            string[] authenticationTypes = challenge.AuthenticationTypes;
            Assert.NotNull(authenticationTypes);
            Assert.Equal(1, authenticationTypes.Length);
            string authenticationType = authenticationTypes[0];
            Assert.Null(authenticationType);
        }

        [Fact]
        public void SendAsync_SuppressesAuthenticationChallenges_WhenExistingAuthenticationTypesIsNull()
        {
            // Arrange
            IHostPrincipalService principalService = CreateStubPrincipalService();
            HttpMessageHandler inner = CreateStubHandler();
            HttpMessageHandler handler = CreateProductUnderTest(principalService, inner);
            IOwinContext context = CreateOwinContext();
            IAuthenticationManager authenticationManager = context.Authentication;
            IDictionary<string, string> expectedExtra = new Dictionary<string, string>();
            AuthenticationProperties extraWrapper = new AuthenticationProperties(expectedExtra);
            context.Authentication.AuthenticationResponseChallenge = new AuthenticationResponseChallenge(null,
                extraWrapper);

            using (HttpRequestMessage request = CreateRequestWithOwinContext(context))
            {
                // Act
                HttpResponseMessage ignore = handler.SendAsync(request, CancellationToken.None).Result;
            }

            // Assert
            AuthenticationResponseChallenge challenge = authenticationManager.AuthenticationResponseChallenge;
            Assert.NotNull(challenge);
            string[] authenticationTypes = challenge.AuthenticationTypes;
            Assert.NotNull(authenticationTypes);
            Assert.Equal(1, authenticationTypes.Length);
            string authenticationType = authenticationTypes[0];
            Assert.Null(authenticationType);
            AuthenticationProperties actualExtraWrapper = challenge.Properties;
            Assert.NotNull(actualExtraWrapper);
            Assert.Same(expectedExtra, actualExtraWrapper.Dictionary);
        }

        [Fact]
        public void SendAsync_SuppressesAuthenticationChallenges_WhenExistingAuthenticationTypesIsEmpty()
        {
            // Arrange
            IHostPrincipalService principalService = CreateStubPrincipalService();
            HttpMessageHandler inner = CreateStubHandler();
            HttpMessageHandler handler = CreateProductUnderTest(principalService, inner);
            IOwinContext context = CreateOwinContext();
            IAuthenticationManager authenticationManager = context.Authentication;
            AuthenticationProperties extraWrapper = new AuthenticationProperties();
            IDictionary<string, string> expectedExtra = extraWrapper.Dictionary;
            authenticationManager.AuthenticationResponseChallenge = new AuthenticationResponseChallenge(new string[0],
                extraWrapper);

            using (HttpRequestMessage request = CreateRequestWithOwinContext(context))
            {
                // Act
                HttpResponseMessage ignore = handler.SendAsync(request, CancellationToken.None).Result;
            }

            // Assert
            AuthenticationResponseChallenge challenge = authenticationManager.AuthenticationResponseChallenge;
            Assert.NotNull(challenge);
            string[] authenticationTypes = challenge.AuthenticationTypes;
            Assert.NotNull(authenticationTypes);
            Assert.Equal(1, authenticationTypes.Length);
            string authenticationType = authenticationTypes[0];
            Assert.Null(authenticationType);
            AuthenticationProperties actualExtraWrapper = challenge.Properties;
            Assert.NotNull(actualExtraWrapper);
            Assert.Same(expectedExtra, actualExtraWrapper.Dictionary);
        }

        [Fact]
        public void SendAsync_LeavesAuthenticationChallenges_WhenExistingAuthenticationTypesIsNonEmpty()
        {
            // Arrange
            IHostPrincipalService principalService = CreateStubPrincipalService();
            HttpMessageHandler inner = CreateStubHandler();
            HttpMessageHandler handler = CreateProductUnderTest(principalService, inner);
            IOwinContext context = CreateOwinContext();
            IAuthenticationManager authenticationManager = context.Authentication;
            AuthenticationProperties extraWrapper = new AuthenticationProperties();
            string[] expectedAuthenticationTypes = new string[] { "Existing" };
            IDictionary<string, string> expectedExtra = extraWrapper.Dictionary;
            authenticationManager.AuthenticationResponseChallenge = new AuthenticationResponseChallenge(
                expectedAuthenticationTypes, extraWrapper);

            using (HttpRequestMessage request = CreateRequestWithOwinContext(context))
            {
                // Act
                HttpResponseMessage ignore = handler.SendAsync(request, CancellationToken.None).Result;
            }

            // Assert
            AuthenticationResponseChallenge challenge = authenticationManager.AuthenticationResponseChallenge;
            Assert.NotNull(challenge);
            Assert.Same(expectedAuthenticationTypes, challenge.AuthenticationTypes);
            AuthenticationProperties actualExtraWrapper = challenge.Properties;
            Assert.NotNull(actualExtraWrapper);
            Assert.Same(expectedExtra, actualExtraWrapper.Dictionary);
        }

        private static HttpMessageHandler CreateDummyHandler()
        {
            return new DummyHttpMessageHandler();
        }

        private static IOwinContext CreateOwinContext()
        {
            return new OwinContext();
        }

        private static IOwinContext CreateOwinContext(IAuthenticationManager authenticationManager)
        {
            Mock<IOwinContext> mock = new Mock<IOwinContext>(MockBehavior.Strict);
            mock.SetupGet(m => m.Authentication).Returns(authenticationManager);
            return mock.Object;
        }

        private static IHostPrincipalService CreateDummyPrincipalService()
        {
            return new Mock<IHostPrincipalService>(MockBehavior.Strict).Object;
        }

        private static AuthenticationProperties CreateExtra()
        {
            return new AuthenticationProperties();
        }

        private static PassiveAuthenticationMessageHandler CreateProductUnderTest(
            IHostPrincipalService principalService, HttpMessageHandler innerHandler)
        {
            PassiveAuthenticationMessageHandler handler = new PassiveAuthenticationMessageHandler(principalService);
            handler.InnerHandler = innerHandler;
            return handler;
        }

        private static HttpRequestMessage CreateRequestWithOwinContext(IOwinContext context)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetOwinContext(context);
            return request;
        }

        private static IHostPrincipalService CreateSpyPrincipalService(Action<IPrincipal> setPrincipal)
        {
            Mock<IHostPrincipalService> mock = new Mock<IHostPrincipalService>(MockBehavior.Strict);
            mock.Setup(s => s.SetCurrentPrincipal(It.IsAny<IPrincipal>(),
                It.IsAny<HttpRequestMessage>())).Callback<IPrincipal, HttpRequestMessage>(
                (p, ignore) => { setPrincipal(p); });
            return mock.Object;
        }

        private static HttpMessageHandler CreateStubHandler()
        {
            return new LambdaHttpMessageHandler((ignore1, ignore2) =>
            {
                return Task.FromResult<HttpResponseMessage>(null);
            });
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
