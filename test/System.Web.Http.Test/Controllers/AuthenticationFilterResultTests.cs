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
                return CreateDummyPrincipal();
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
            IAuthenticationFilter filter = CreateAuthenticationFilter((c, t) =>
            {
                calls++;
                authenticationContext = c;
                cancellationToken = t;
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
                IAuthenticationFilter filter = CreateAuthenticationFilter((c, t) =>
                {
                    c.ErrorResult = errorResult;
                });
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
                IAuthenticationFilter firstFilter = CreateAuthenticationFilter((c, t) =>
                {
                    c.ErrorResult = errorResult;
                });
                int calls = 0;
                IAuthenticationFilter secondFilter = CreateAuthenticationFilter((c, t) =>
                    {
                        calls++;
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
            IAuthenticationFilter filter = CreateAuthenticationFilter((c, t) =>
            {
                c.Principal = expectedPrincipal;
            });
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
            IAuthenticationFilter firstFilter = CreateAuthenticationFilter((c, t) =>
            {
                c.Principal = expectedPrincipal;
            });
            IPrincipal principal = null;
            IAuthenticationFilter secondFilter = CreateAuthenticationFilter((c, t) =>
            {
                if (c != null)
                {
                    principal = c.Principal;
                }
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
            IAuthenticationFilter filter = CreateAuthenticationFilter((c, t) =>
            {
                c.Principal = principal;
            });
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
            IAuthenticationFilter filter = CreateAuthenticationFilter((c, t) => Task.FromResult<object>(null));
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
            IAuthenticationFilter filter = CreateAuthenticationFilterChallenge((c, t) =>
            {
                calls++;
                context = c.ActionContext;
                innerResult = c.Result;
                cancellationToken = t;
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
                (c, t) =>
                {
                    c.ErrorResult = expectedErrorResult;
                },
                (c, t) =>
                {
                    calls++;
                    context = c.ActionContext;
                    innerResult = c.Result;
                    cancellationToken = t;

                    c.Result = CreateStubActionResult();
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
            IHttpActionResult result = null;
            CancellationToken cancellationToken = default(CancellationToken);
            IAuthenticationFilter firstFilter = CreateAuthenticationFilterChallenge((c, t) =>
            {
                c.Result = expectedInnerResult;
            });
            IAuthenticationFilter secondFilter = CreateAuthenticationFilterChallenge((c, t) =>
            {
                calls++;
                context = c.ActionContext;
                result = c.Result;
                cancellationToken = t;

                c.Result = CreateStubActionResult();
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
                Assert.Same(expectedInnerResult, result);
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

        private static IAuthenticationFilter CreateAuthenticationFilter(
            Action<HttpAuthenticationContext, CancellationToken> authenticate)
        {
            Mock<IAuthenticationFilter> mock = new Mock<IAuthenticationFilter>();
            mock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(), It.IsAny<CancellationToken>()))
                .Callback<HttpAuthenticationContext, CancellationToken>((c, t) =>
                {
                    authenticate.Invoke(c, t);
                })
                .Returns(() => Task.FromResult<object>(null));
            mock.Setup(f => f.ChallengeAsync(It.IsAny<HttpAuthenticationChallengeContext>(),
                It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult<object>(null));
            return mock.Object;
        }

        private static IAuthenticationFilter CreateAuthenticationFilter(
            Action<HttpAuthenticationContext, CancellationToken> authenticate,
            Action<HttpAuthenticationChallengeContext, CancellationToken> challenge)
        {
            Mock<IAuthenticationFilter> mock = new Mock<IAuthenticationFilter>();
            mock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(), It.IsAny<CancellationToken>()))
                .Callback<HttpAuthenticationContext, CancellationToken>((c, t) =>
                    {
                        authenticate.Invoke(c, t);
                    })
                .Returns(() => Task.FromResult<object>(null));
            mock.Setup(f => f.ChallengeAsync(It.IsAny<HttpAuthenticationChallengeContext>(),
                It.IsAny<CancellationToken>()))
                .Callback<HttpAuthenticationChallengeContext, CancellationToken>((c, t) =>
                    {
                        challenge.Invoke(c, t);
                    })
                .Returns(() => Task.FromResult<object>(null));
            return mock.Object;
        }

        private static IAuthenticationFilter CreateAuthenticationFilterChallenge(
            Action<HttpAuthenticationChallengeContext, CancellationToken> challenge)
        {
            Mock<IAuthenticationFilter> mock = new Mock<IAuthenticationFilter>();
            mock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult<object>(null));
            mock.Setup(f => f.ChallengeAsync(It.IsAny<HttpAuthenticationChallengeContext>(),
                It.IsAny<CancellationToken>()))
                .Callback<HttpAuthenticationChallengeContext, CancellationToken>((c, t) =>
                {
                    challenge.Invoke(c, t);
                })
                .Returns(() => Task.FromResult<object>(null));
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
            mock.Setup(s => s.GetCurrentPrincipal(It.IsAny<HttpRequestMessage>())).Returns(CreateDummyPrincipal());
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
            mock.Setup(f => f.AuthenticateAsync(It.IsAny<HttpAuthenticationContext>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult<object>(null));
            mock.Setup(f => f.ChallengeAsync(It.IsAny<HttpAuthenticationChallengeContext>(),
                It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult<object>(null));
            return mock.Object;
        }

        private static IHostPrincipalService CreateStubPrincipalService()
        {
            return CreateStubPrincipalService(CreateDummyPrincipal());
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
