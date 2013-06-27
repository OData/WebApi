// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Controllers
{
    public class AuthenticationFilterResultTests
    {
        [Fact]
        public void ExecuteAsync_DelegatesToInnerResult_WhenFiltersIsEmpty()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            using (HttpResponseMessage expectedResponse = CreateResponse())
            {
                HttpActionContext context = CreateContext();
                IAuthenticationFilter[] filters = new IAuthenticationFilter[0];
                IHostPrincipalService principalService = CreateStubPrincipalService();
                int calls = 0;
                CancellationToken cancellationToken;
                IHttpActionResult innerResult = CreateActionResult((t) =>
                {
                    calls++;
                    cancellationToken = t;
                    return Task.FromResult(expectedResponse);
                });
                IHttpActionResult product = CreateProductUnderTest(context, filters, principalService, request,
                    innerResult);
                CancellationToken expectedCancellationToken = CreateCancellationToken();

                // Act
                Task<HttpResponseMessage> task = product.ExecuteAsync(expectedCancellationToken);

                // Assert
                Assert.NotNull(task);
                HttpResponseMessage response = task.Result;
                Assert.Equal(1, calls);
                Assert.Equal(expectedCancellationToken, cancellationToken);
                Assert.Same(expectedResponse, response);
            }
        }

        [Fact]
        public void ExecuteAsync_CallsPrincipalServiceGetCurrentPrincipal()
        {
            // Arrange
            HttpActionContext context = CreateContext();
            IAuthenticationFilter filter = CreateStubFilter();
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter };
            int calls = 0;
            HttpRequestMessage request = null;
            IHostPrincipalService principalService = CreatePrincipalService((r) =>
            {
                calls++;
                request = r;
                return null;
            });
            IHttpActionResult innerResult = CreateStubActionResult();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IHttpActionResult product = CreateProductUnderTest(context, filters, principalService, expectedRequest,
                    innerResult);

                // Act
                Task<HttpResponseMessage> task = product.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);
                HttpResponseMessage response = task.Result;
                Assert.Equal(1, calls);
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public void ExecuteAsync_CallsFilterAuthenticateAsync()
        {
            // Arrange
            HttpActionContext expectedActionContext = CreateContext();
            int calls = 0;
            HttpAuthenticationContext authenticationContext = null;
            CancellationToken cancellationToken = default(CancellationToken);
            IAuthenticationFilter filter = CreateAuthenticationFilter((a, c) =>
            {
                calls++;
                authenticationContext = a;
                cancellationToken = c;
                IAuthenticationResult result = null;
                return Task.FromResult(result);
            });
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter };
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            IHostPrincipalService principalService = CreateStubPrincipalService(expectedPrincipal);
            IHttpActionResult innerResult = CreateStubActionResult();
            CancellationToken expectedCancellationToken = CreateCancellationToken();

            using (HttpRequestMessage request = CreateRequest())
            {
                IHttpActionResult product = CreateProductUnderTest(expectedActionContext, filters, principalService,
                    request, innerResult);

                // Act
                Task<HttpResponseMessage> task = product.ExecuteAsync(expectedCancellationToken);

                // Assert
                HttpResponseMessage ignore = task.Result;
            }

            Assert.Equal(1, calls);
            Assert.NotNull(authenticationContext);
            HttpActionContext actionContext = authenticationContext.ActionContext;
            Assert.Same(expectedActionContext, actionContext);
            IPrincipal principal = authenticationContext.Principal;
            Assert.Same(expectedPrincipal, principal);
            Assert.Equal(expectedCancellationToken, cancellationToken);
        }

        [Fact]
        public void ExecuteAsync_DelegatesToErrorResult_WhenFilterReturnsFailure()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            using (HttpResponseMessage expectedResponse = CreateResponse())
            {
                HttpActionContext context = CreateContext();
                int calls = 0;
                CancellationToken cancellationToken;
                IHttpActionResult errorResult = CreateActionResult((t) =>
                {
                    calls++;
                    cancellationToken = t;
                    return Task.FromResult(expectedResponse);
                });
                IAuthenticationFilter filter = CreateAuthenticationFilter(
                    (a, c) => Task.FromResult<IAuthenticationResult>(new FailedAuthenticationResult(errorResult)));
                IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter };
                IHostPrincipalService principalService = CreateStubPrincipalService();
                IHttpActionResult innerResult = CreateDummyActionResult();
                IHttpActionResult product = CreateProductUnderTest(context, filters, principalService, request,
                    innerResult);
                CancellationToken expectedCancellationToken = CreateCancellationToken();

                // Act
                Task<HttpResponseMessage> task = product.ExecuteAsync(expectedCancellationToken);

                // Assert
                Assert.NotNull(task);
                HttpResponseMessage response = task.Result;
                Assert.Equal(1, calls);
                Assert.Equal(expectedCancellationToken, cancellationToken);
                Assert.Same(expectedResponse, response);
            }
        }

        [Fact]
        public void ExecuteAsync_DoesNotCallSecondFilterAuthenticateOrInnerResult_WhenFirstFilterReturnsFailure()
        {
            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            using (HttpResponseMessage expectedResponse = CreateResponse())
            {
                HttpActionContext context = CreateContext();
                IHttpActionResult errorResult = CreateActionResult((t) => Task.FromResult(expectedResponse));
                IAuthenticationFilter firstFilter = CreateAuthenticationFilter(
                    (a, c) => Task.FromResult<IAuthenticationResult>(new FailedAuthenticationResult(errorResult)));
                int calls = 0;
                IAuthenticationFilter secondFilter = CreateAuthenticationFilter((a, c) =>
                    {
                        calls++;
                        return Task.FromResult<IAuthenticationResult>(null);
                    });
                IAuthenticationFilter[] filters = new IAuthenticationFilter[] { firstFilter, secondFilter };
                IHostPrincipalService principalService = CreateStubPrincipalService();
                IHttpActionResult innerResult = CreateDummyActionResult();
                IHttpActionResult product = CreateProductUnderTest(context, filters, principalService, request,
                    innerResult);
                CancellationToken expectedCancellationToken = CreateCancellationToken();

                // Act
                Task<HttpResponseMessage> task = product.ExecuteAsync(expectedCancellationToken);

                // Assert
                Assert.NotNull(task);
                HttpResponseMessage response = task.Result;
                Assert.Equal(0, calls);
            }
        }

        [Fact]
        public void ExecuteAsync_CallsPrincipalServiceSetCurrentPrincipal_WhenFilterReturnsSuccess()
        {
            // Arrange
            HttpActionContext context = CreateContext();
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            IAuthenticationResult authenticationResult = new SucceededAuthenticationResult(expectedPrincipal);
            IAuthenticationFilter filter = CreateAuthenticationFilter((a, c) => Task.FromResult(authenticationResult));
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter };
            int calls = 0;
            HttpRequestMessage request = null;
            IPrincipal principal = null;
            IHostPrincipalService principalService = CreatePrincipalService((p, r) =>
            {
                calls++;
                request = r;
                principal = p;
            });
            IHttpActionResult innerResult = CreateStubActionResult();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IHttpActionResult product = CreateProductUnderTest(context, filters, principalService, expectedRequest,
                    innerResult);

                // Act
                Task<HttpResponseMessage> task = product.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);
                HttpResponseMessage response = task.Result;
                Assert.Equal(1, calls);
                Assert.Same(expectedPrincipal, principal);
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public void ExecuteAsync_PassesPrincipalFromFirstFilterSuccessToSecondFilter()
        {
            // Arrange
            HttpActionContext context = CreateContext();
            IPrincipal expectedPrincipal = CreateDummyPrincipal();
            IAuthenticationResult authenticationResult = new SucceededAuthenticationResult(expectedPrincipal);
            IAuthenticationFilter firstFilter = CreateAuthenticationFilter(
                (a, c) => Task.FromResult(authenticationResult));
            IPrincipal principal = null;
            IAuthenticationFilter secondFilter = CreateAuthenticationFilter((a, c) =>
                {
                    if (a != null)
                    {
                        principal = a.Principal;
                    }

                    return Task.FromResult<IAuthenticationResult>(null);
                });
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { firstFilter, secondFilter };
            IHostPrincipalService principalService = CreateStubPrincipalService();
            IHttpActionResult innerResult = CreateStubActionResult();

            using (HttpRequestMessage request = CreateRequest())
            {
                IHttpActionResult product = CreateProductUnderTest(context, filters, principalService, request,
                    innerResult);

                // Act
                Task<HttpResponseMessage> task = product.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);
                HttpResponseMessage response = task.Result;
                Assert.Same(expectedPrincipal, principal);
            }
        }

        [Fact]
        public void ExecuteAsync_OnlySetsPrincipalOnce_WhenMultipleFiltersReturnSuccess()
        {
            // Arrange
            HttpActionContext context = CreateContext();
            IPrincipal principal = CreateDummyPrincipal();
            IAuthenticationResult authenticationResult = new SucceededAuthenticationResult(principal);
            IAuthenticationFilter filter = CreateAuthenticationFilter((a, c) => Task.FromResult(authenticationResult));
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter, filter };
            int calls = 0;
            IHostPrincipalService principalService = CreatePrincipalService((p, r) =>
            {
                calls++;
            });
            IHttpActionResult innerResult = CreateStubActionResult();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IHttpActionResult product = CreateProductUnderTest(context, filters, principalService, expectedRequest,
                    innerResult);

                // Act
                Task<HttpResponseMessage> task = product.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);
                HttpResponseMessage response = task.Result;
                Assert.Equal(1, calls);
            }
        }

        [Fact]
        public void ExecuteAsync_DoesNotSetPrincipal_WhenNoFilterReturnsSuccess()
        {
            // Arrange
            HttpActionContext context = CreateContext();
            IPrincipal principal = CreateDummyPrincipal();
            IAuthenticationResult authenticationResult = null;
            IAuthenticationFilter filter = CreateAuthenticationFilter((a, c) => Task.FromResult(authenticationResult));
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter, filter };
            int calls = 0;
            IHostPrincipalService principalService = CreatePrincipalService((p, r) =>
            {
                calls++;
            });
            IHttpActionResult innerResult = CreateStubActionResult();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                IHttpActionResult product = CreateProductUnderTest(context, filters, principalService, expectedRequest,
                    innerResult);

                // Act
                Task<HttpResponseMessage> task = product.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);
                HttpResponseMessage response = task.Result;
                Assert.Equal(0, calls);
            }
        }

        [Fact]
        public void ExecuteAsync_CallsFilterChallengeAsync_WithInnerResult_WhenNoFilterReturnsFailure()
        {
            // Arrange
            HttpActionContext expectedContext = CreateContext();
            int calls = 0;
            HttpActionContext context = null;
            IHttpActionResult innerResult = null;
            CancellationToken cancellationToken = default(CancellationToken);
            IAuthenticationFilter filter = CreateAuthenticationFilter((a, r, c) =>
            {
                calls++;
                context = a;
                innerResult = r;
                cancellationToken = c;
                return Task.FromResult(r);
            });
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter };
            IHostPrincipalService principalService = CreateStubPrincipalService();
            IHttpActionResult expectedInnerResult = CreateStubActionResult();
            CancellationToken expectedCancellationToken = CreateCancellationToken();

            using (HttpRequestMessage request = CreateRequest())
            {
                IHttpActionResult product = CreateProductUnderTest(expectedContext, filters, principalService, request,
                    expectedInnerResult);

                // Act
                Task<HttpResponseMessage> task = product.ExecuteAsync(expectedCancellationToken);

                // Assert
                Assert.NotNull(task);
                HttpResponseMessage response = task.Result;
                Assert.Equal(1, calls);
                Assert.Same(expectedContext, context);
                Assert.Same(expectedInnerResult, innerResult);
                Assert.Equal(expectedCancellationToken, cancellationToken);
            }
        }

        [Fact]
        public void ExecuteAsync_CallsFilterChallengeAsync_WithErrorResult_WhenFilterReturnsFailure()
        {
            // Arrange
            HttpActionContext expectedContext = CreateContext();
            IHttpActionResult expectedErrorResult = CreateDummyActionResult();
            int calls = 0;
            HttpActionContext context = null;
            IHttpActionResult innerResult = null;
            CancellationToken cancellationToken = default(CancellationToken);
            IAuthenticationFilter filter = CreateAuthenticationFilter(
                (a, c) =>
                {
                    IAuthenticationResult result = new FailedAuthenticationResult(expectedErrorResult);
                    return Task.FromResult(result);
                },
                (a, r, c) =>
                {
                    calls++;
                    context = a;
                    innerResult = r;
                    cancellationToken = c;
                    return Task.FromResult(CreateStubActionResult());
                });
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter };
            IHostPrincipalService principalService = CreateStubPrincipalService();
            IHttpActionResult originalInnerResult = CreateDummyActionResult();
            CancellationToken expectedCancellationToken = CreateCancellationToken();

            using (HttpRequestMessage request = CreateRequest())
            {
                IHttpActionResult product = CreateProductUnderTest(expectedContext, filters, principalService, request,
                    originalInnerResult);

                // Act
                Task<HttpResponseMessage> task = product.ExecuteAsync(expectedCancellationToken);

                // Assert
                Assert.NotNull(task);
                HttpResponseMessage response = task.Result;
                Assert.Equal(1, calls);
                Assert.Same(expectedContext, context);
                Assert.Same(expectedErrorResult, innerResult);
                Assert.Equal(expectedCancellationToken, cancellationToken);
            }
        }

        [Fact]
        public void ExecuteAsync_CallsSecondFilterChallengeAsync_WithFirstChallengeResult()
        {
            // Arrange
            HttpActionContext expectedContext = CreateContext();
            IHttpActionResult expectedInnerResult = CreateDummyActionResult();
            int calls = 0;
            HttpActionContext context = null;
            IHttpActionResult innerResult = null;
            CancellationToken cancellationToken = default(CancellationToken);
            IAuthenticationFilter firstFilter = CreateAuthenticationFilter((a, r, c) =>
            {
                return Task.FromResult(expectedInnerResult);
            });
            IAuthenticationFilter secondFilter = CreateAuthenticationFilter((a, r, c) =>
            {
                calls++;
                context = a;
                innerResult = r;
                cancellationToken = c;
                return Task.FromResult(CreateStubActionResult());
            });
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { firstFilter, secondFilter };
            IHostPrincipalService principalService = CreateStubPrincipalService();
            IHttpActionResult originalInnerResult = CreateDummyActionResult();
            CancellationToken expectedCancellationToken = CreateCancellationToken();

            using (HttpRequestMessage request = CreateRequest())
            {
                IHttpActionResult product = CreateProductUnderTest(expectedContext, filters, principalService, request,
                    originalInnerResult);

                // Act
                Task<HttpResponseMessage> task = product.ExecuteAsync(expectedCancellationToken);

                // Assert
                Assert.NotNull(task);
                HttpResponseMessage response = task.Result;
                Assert.Equal(1, calls);
                Assert.Same(expectedContext, context);
                Assert.Same(expectedInnerResult, innerResult);
                Assert.Equal(expectedCancellationToken, cancellationToken);
            }
        }

        [Fact]
        public void ExecuteAsync_CallsSecondFilterChallengeAsync_WithInnerResult_WhenFirstChallengeReturnsNull()
        {
            // Arrange
            HttpActionContext expectedContext = CreateContext();
            IHttpActionResult expectedInnerResult = CreateDummyActionResult();
            int calls = 0;
            HttpActionContext context = null;
            IHttpActionResult innerResult = null;
            CancellationToken cancellationToken = default(CancellationToken);
            IAuthenticationFilter firstFilter = CreateAuthenticationFilter((a, r, c) =>
            {
                return Task.FromResult<IHttpActionResult>(null);
            });
            IAuthenticationFilter secondFilter = CreateAuthenticationFilter((a, r, c) =>
            {
                calls++;
                context = a;
                innerResult = r;
                cancellationToken = c;
                return Task.FromResult(CreateStubActionResult());
            });
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { firstFilter, secondFilter };
            IHostPrincipalService principalService = CreateStubPrincipalService();
            CancellationToken expectedCancellationToken = CreateCancellationToken();

            using (HttpRequestMessage request = CreateRequest())
            {
                IHttpActionResult product = CreateProductUnderTest(expectedContext, filters, principalService, request,
                    expectedInnerResult);

                // Act
                Task<HttpResponseMessage> task = product.ExecuteAsync(expectedCancellationToken);

                // Assert
                Assert.NotNull(task);
                HttpResponseMessage response = task.Result;
                Assert.Equal(1, calls);
                Assert.Same(expectedContext, context);
                Assert.Same(expectedInnerResult, innerResult);
                Assert.Equal(expectedCancellationToken, cancellationToken);
            }
        }

        private static IHttpActionResult CreateActionResult(
            Func<CancellationToken, Task<HttpResponseMessage>> executeAsync)
        {
            Mock<IHttpActionResult> mock = new Mock<IHttpActionResult>(MockBehavior.Strict);
            CancellationToken cancellationToken;
            mock.Setup(r => r.ExecuteAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>((t) => { cancellationToken = t; })
                .Returns(() => executeAsync.Invoke(cancellationToken));
            return mock.Object;
        }

        private static IAuthenticationFilter CreateAuthenticationFilter(Func<HttpAuthenticationContext,
            CancellationToken, Task<IAuthenticationResult>> authenticateAsync)
        {
            Mock<IAuthenticationFilter> mock = new Mock<IAuthenticationFilter>();
            HttpAuthenticationContext authenticationContext = null;
            CancellationToken cancellationToken = default(CancellationToken);
            mock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(), It.IsAny<CancellationToken>()))
                .Callback<HttpAuthenticationContext, CancellationToken>((a, c) =>
                {
                    authenticationContext = a;
                    cancellationToken = c;
                })
                .Returns(() => authenticateAsync.Invoke(authenticationContext, cancellationToken));
            IHttpActionResult innerResult = null;
            mock.Setup(f => f.ChallengeAsync(It.IsAny<HttpActionContext>(), It.IsAny<IHttpActionResult>(),
                It.IsAny<CancellationToken>()))
                .Callback<HttpActionContext, IHttpActionResult, CancellationToken>((a, r, c) => { innerResult = r; })
                .Returns(() => Task.FromResult(innerResult));
            return mock.Object;
        }

        private static IAuthenticationFilter CreateAuthenticationFilter(Func<HttpActionContext, IHttpActionResult,
            CancellationToken, Task<IHttpActionResult>> challengeAsync)
        {
            Mock<IAuthenticationFilter> mock = new Mock<IAuthenticationFilter>();
            HttpActionContext actionContext = null;
            IHttpActionResult innerResult = null;
            CancellationToken cancellationToken = default(CancellationToken);
            mock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult<IAuthenticationResult>(null));
            mock.Setup(f => f.ChallengeAsync(It.IsAny<HttpActionContext>(), It.IsAny<IHttpActionResult>(),
                It.IsAny<CancellationToken>()))
                .Callback<HttpActionContext, IHttpActionResult, CancellationToken>((a, r, c) =>
                {
                    actionContext = a;
                    innerResult = r;
                    cancellationToken = c;
                })
                .Returns(() => challengeAsync.Invoke(actionContext, innerResult, cancellationToken));
            return mock.Object;
        }

        private static IAuthenticationFilter CreateAuthenticationFilter(Func<HttpAuthenticationContext,
            CancellationToken, Task<IAuthenticationResult>> authenticateAsync, Func<HttpActionContext,
            IHttpActionResult, CancellationToken, Task<IHttpActionResult>> challengeAsync)
        {
            Mock<IAuthenticationFilter> mock = new Mock<IAuthenticationFilter>();
            HttpAuthenticationContext authenticationContext = null;
            CancellationToken authenticateCancellationToken = default(CancellationToken);
            mock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(), It.IsAny<CancellationToken>()))
                .Callback<HttpAuthenticationContext, CancellationToken>((a, c) =>
                    {
                        authenticationContext = a;
                        authenticateCancellationToken = c;
                    })
                .Returns(() => authenticateAsync.Invoke(authenticationContext, authenticateCancellationToken));
            HttpActionContext actionContext = null;
            IHttpActionResult innerResult = null;
            CancellationToken challengeCancellationToken = default(CancellationToken);
            mock.Setup(f => f.ChallengeAsync(It.IsAny<HttpActionContext>(), It.IsAny<IHttpActionResult>(),
                It.IsAny<CancellationToken>()))
                .Callback<HttpActionContext, IHttpActionResult, CancellationToken>((a, r, c) =>
                    {
                        actionContext = a;
                        innerResult = r;
                        challengeCancellationToken = c;
                    })
                .Returns(() => challengeAsync.Invoke(actionContext, innerResult, challengeCancellationToken));
            return mock.Object;
        }

        private static CancellationToken CreateCancellationToken()
        {
            return new CancellationToken(canceled: true);
        }

        private static HttpActionContext CreateContext()
        {
            return new HttpActionContext();
        }

        private static IHttpActionResult CreateDummyActionResult()
        {
            return new Mock<IHttpActionResult>(MockBehavior.Strict).Object;
        }

        private static IPrincipal CreateDummyPrincipal()
        {
            return new Mock<IPrincipal>(MockBehavior.Strict).Object;
        }

        private static IHostPrincipalService CreatePrincipalService(
            Func<HttpRequestMessage, IPrincipal> getCurrentPrincipal)
        {
            Mock<IHostPrincipalService> mock = new Mock<IHostPrincipalService>();
            HttpRequestMessage request = null;
            mock.Setup(s => s.GetCurrentPrincipal(It.IsAny<HttpRequestMessage>()))
                .Callback<HttpRequestMessage>(r => { request = r; })
                .Returns(() => getCurrentPrincipal.Invoke(request));
            return mock.Object;
        }

        private static IHostPrincipalService CreatePrincipalService(
            Action<IPrincipal, HttpRequestMessage> setCurrentPrincipal)
        {
            Mock<IHostPrincipalService> mock = new Mock<IHostPrincipalService>();
            mock.Setup(s => s.SetCurrentPrincipal(It.IsAny<IPrincipal>(), It.IsAny<HttpRequestMessage>()))
                .Callback<IPrincipal, HttpRequestMessage>((p, r) => { setCurrentPrincipal.Invoke(p, r); });
            return mock.Object;
        }

        private static AuthenticationFilterResult CreateProductUnderTest(HttpActionContext context,
            IAuthenticationFilter[] filters, IHostPrincipalService principalService, HttpRequestMessage request,
            IHttpActionResult innerResult)
        {
            return new AuthenticationFilterResult(context, filters, principalService, request, innerResult);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static HttpResponseMessage CreateResponse()
        {
            return new HttpResponseMessage();
        }

        private static IHttpActionResult CreateStubActionResult()
        {
            Mock<IHttpActionResult> mock = new Mock<IHttpActionResult>(MockBehavior.Strict);
            HttpResponseMessage response = null;
            mock.Setup(r => r.ExecuteAsync(It.IsAny<CancellationToken>())).Returns(() => Task.FromResult(response));
            return mock.Object;
        }

        private static IAuthenticationFilter CreateStubFilter()
        {
            Mock<IAuthenticationFilter> mock = new Mock<IAuthenticationFilter>(MockBehavior.Strict);
            IAuthenticationResult authenticateResult = null;
            mock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(authenticateResult));
            IHttpActionResult innerResult = null;
            mock.Setup(f => f.ChallengeAsync(It.IsAny<HttpActionContext>(), It.IsAny<IHttpActionResult>(),
                It.IsAny<CancellationToken>()))
                .Callback<HttpActionContext, IHttpActionResult, CancellationToken>((a, i, c) => { innerResult = i; })
                .Returns(() => Task.FromResult(innerResult));
            return mock.Object;
        }

        private static IHostPrincipalService CreateStubPrincipalService()
        {
            return CreateStubPrincipalService(null);
        }

        private static IHostPrincipalService CreateStubPrincipalService(IPrincipal principal)
        {
            Mock<IHostPrincipalService> mock = new Mock<IHostPrincipalService>(MockBehavior.Strict);
            mock.Setup(s => s.GetCurrentPrincipal(It.IsAny<HttpRequestMessage>())).Returns(principal);
            mock.Setup(s => s.SetCurrentPrincipal(It.IsAny<IPrincipal>(), It.IsAny<HttpRequestMessage>()));
            return mock.Object;
        }
    }
}
