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
            IDictionary<string, object> environment = CreateOwinEnvironment();

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

                using (HttpRequestMessage expectedRequest = CreateRequestWithOwinEnvironment(environment))
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

        [Fact]
        public void SendAsync_SuppressesAuthenticationChallenges_WhenNoChallengeIsSet()
        {
            // Arrange
            IHostPrincipalService principalService = CreateStubPrincipalService();
            HttpMessageHandler inner = CreateStubHandler();
            HttpMessageHandler handler = CreateProductUnderTest(principalService, inner);
            IDictionary<string, object> environment = CreateOwinEnvironment();

            using (HttpRequestMessage request = CreateRequestWithOwinEnvironment(environment))
            {
                // Act
                handler.SendAsync(request, CancellationToken.None);
            }

            // Assert
            OwinResponse owinResponse = new OwinResponse(environment);
            AuthenticationResponseChallenge challenge = owinResponse.AuthenticationResponseChallenge;
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
            IDictionary<string, object> environment = CreateOwinEnvironment();
            OwinResponse owinResponse = new OwinResponse(environment);
            AuthenticationExtra extraWrapper = new AuthenticationExtra();
            IDictionary<string, string> expectedExtra = extraWrapper.Properties;
            owinResponse.AuthenticationResponseChallenge = new AuthenticationResponseChallenge(null,
                extraWrapper);

            using (HttpRequestMessage request = CreateRequestWithOwinEnvironment(environment))
            {
                // Act
                handler.SendAsync(request, CancellationToken.None);
            }

            // Assert
            AuthenticationResponseChallenge challenge = owinResponse.AuthenticationResponseChallenge;
            Assert.NotNull(challenge);
            string[] authenticationTypes = challenge.AuthenticationTypes;
            Assert.NotNull(authenticationTypes);
            Assert.Equal(1, authenticationTypes.Length);
            string authenticationType = authenticationTypes[0];
            Assert.Null(authenticationType);
            AuthenticationExtra actualExtraWrapper = challenge.Extra;
            Assert.NotNull(actualExtraWrapper);
            Assert.Same(expectedExtra, actualExtraWrapper.Properties);
        }

        [Fact]
        public void SendAsync_SuppressesAuthenticationChallenges_WhenExistingAuthenticationTypesIsEmpty()
        {
            // Arrange
            IHostPrincipalService principalService = CreateStubPrincipalService();
            HttpMessageHandler inner = CreateStubHandler();
            HttpMessageHandler handler = CreateProductUnderTest(principalService, inner);
            IDictionary<string, object> environment = CreateOwinEnvironment();
            OwinResponse owinResponse = new OwinResponse(environment);
            AuthenticationExtra extraWrapper = new AuthenticationExtra();
            IDictionary<string, string> expectedExtra = extraWrapper.Properties;
            owinResponse.AuthenticationResponseChallenge = new AuthenticationResponseChallenge(new string[0],
                extraWrapper);

            using (HttpRequestMessage request = CreateRequestWithOwinEnvironment(environment))
            {
                // Act
                handler.SendAsync(request, CancellationToken.None);
            }

            // Assert
            AuthenticationResponseChallenge challenge = owinResponse.AuthenticationResponseChallenge;
            Assert.NotNull(challenge);
            string[] authenticationTypes = challenge.AuthenticationTypes;
            Assert.NotNull(authenticationTypes);
            Assert.Equal(1, authenticationTypes.Length);
            string authenticationType = authenticationTypes[0];
            Assert.Null(authenticationType);
            AuthenticationExtra actualExtraWrapper = challenge.Extra;
            Assert.NotNull(actualExtraWrapper);
            Assert.Same(expectedExtra, actualExtraWrapper.Properties);
        }

        [Fact]
        public void SendAsync_LeavesAuthenticationChallenges_WhenExistingAuthenticationTypesIsNonEmpty()
        {
            // Arrange
            IHostPrincipalService principalService = CreateStubPrincipalService();
            HttpMessageHandler inner = CreateStubHandler();
            HttpMessageHandler handler = CreateProductUnderTest(principalService, inner);
            IDictionary<string, object> environment = CreateOwinEnvironment();
            OwinResponse owinResponse = new OwinResponse(environment);
            AuthenticationExtra extraWrapper = new AuthenticationExtra();
            string[] expectedAuthenticationTypes = new string[] { "Existing" };
            IDictionary<string, string> expectedExtra = extraWrapper.Properties;
            owinResponse.AuthenticationResponseChallenge = new AuthenticationResponseChallenge(
                expectedAuthenticationTypes, extraWrapper);

            using (HttpRequestMessage request = CreateRequestWithOwinEnvironment(environment))
            {
                // Act
                handler.SendAsync(request, CancellationToken.None);
            }

            // Assert
            AuthenticationResponseChallenge challenge = owinResponse.AuthenticationResponseChallenge;
            Assert.NotNull(challenge);
            Assert.Same(expectedAuthenticationTypes, challenge.AuthenticationTypes);
            AuthenticationExtra actualExtraWrapper = challenge.Extra;
            Assert.NotNull(actualExtraWrapper);
            Assert.Same(expectedExtra, actualExtraWrapper.Properties);
        }

        private static HttpMessageHandler CreateDummyHandler()
        {
            return new DummyHttpMessageHandler();
        }

        private static IDictionary<string, object> CreateOwinEnvironment()
        {
            return new Dictionary<string, object>();
        }

        private static IHostPrincipalService CreateDummyPrincipalService()
        {
            return new Mock<IHostPrincipalService>(MockBehavior.Strict).Object;
        }

        private static PassiveAuthenticationMessageHandler CreateProductUnderTest(
            IHostPrincipalService principalService, HttpMessageHandler innerHandler)
        {
            PassiveAuthenticationMessageHandler handler = new PassiveAuthenticationMessageHandler(principalService);
            handler.InnerHandler = innerHandler;
            return handler;
        }

        private static HttpRequestMessage CreateRequestWithOwinEnvironment(IDictionary<string, object> environment)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetOwinEnvironment(environment);
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
