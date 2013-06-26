// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Owin
{
    public class HostAuthenticationFilterTest
    {
        private const string ChallengeKey = "security.Challenge";

        [Fact]
        public void Constructor_Throws_WhenAuthenticationTypeIsNull()
        {
            // Arrange
            string authenticationType = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { var ignore = CreateProductUnderTest(authenticationType); },
                "authenticationType");
        }

        [Fact]
        public void AuthenticationType_ReturnsSpecifiedInstance()
        {
            // Arrange
            string expectedAuthenticationType = "AuthenticationType";
            HostAuthenticationFilter filter = CreateProductUnderTest(expectedAuthenticationType);

            // Act
            string authenticationType = filter.AuthenticationType;

            // Assert
            Assert.Same(expectedAuthenticationType, authenticationType);
        }

        [Fact]
        public void AllowMultiple_ReturnsTrue()
        {
            // Arrange
            IAuthenticationFilter filter = CreateProductUnderTest();

            // Act
            bool allowMultiple = filter.AllowMultiple;

            // Assert
            Assert.True(allowMultiple);
        }

        [Fact]
        public void AuthenticateAsync_SetsClaimsPrincipal_WhenOwinAuthenticateReturnsIdentity()
        {
            // Arrange
            string authenticationType = "AuthenticationType";
            IAuthenticationFilter filter = CreateProductUnderTest(authenticationType);
            IIdentity expectedIdentity = CreateDummyIdentity();
            IDictionary<string, object> environment = CreateOwinEnvironment((authenticationTypes, callback, state) =>
                {
                    if (authenticationTypes != null && authenticationTypes.Contains(authenticationType))
                    {
                        callback(expectedIdentity, null, null, state);
                    }

                    return Task.FromResult<object>(null);
                });
            HttpAuthenticationContext context;

            using (HttpRequestMessage request = CreateRequest(environment))
            {
                context = CreateAuthenticationContext(request);

                // Act
                filter.AuthenticateAsync(context, CancellationToken.None).Wait();
            }

            // Assert
            Assert.Null(context.ErrorResult);
            IPrincipal principal = context.Principal;
            Assert.IsType<ClaimsPrincipal>(principal);
            ClaimsPrincipal claimsPrincipal = (ClaimsPrincipal)principal;
            IIdentity identity = claimsPrincipal.Identity;
            Assert.Same(expectedIdentity, identity);
        }

        [Fact]
        public void AuthenticateAsync_SetsNoPrincipalOrError_WhenOwinAuthenticateDoesNotReturnIdentity()
        {
            // Arrange
            string authenticationType = "AuthenticationType";
            IAuthenticationFilter filter = CreateProductUnderTest(authenticationType);
            IIdentity expectedIdentity = CreateDummyIdentity();
            IDictionary<string, object> environment = CreateOwinEnvironment((ignore1, ignore2, ignore3) =>
                Task.FromResult<object>(null));
            IPrincipal expectedPrincipal = CreateDummyPrincipal();

            HttpAuthenticationContext context;

            using (HttpRequestMessage request = CreateRequest(environment))
            {
                context = CreateAuthenticationContext(request, expectedPrincipal);

                // Act
                filter.AuthenticateAsync(context, CancellationToken.None).Wait();
            }

            // Assert
            Assert.Null(context.ErrorResult);
            Assert.Same(expectedPrincipal, context.Principal);
        }

        [Fact]
        public void AuthenticateAsync_Throws_WhenContextIsNull()
        {
            // Arrange
            IAuthenticationFilter filter = CreateProductUnderTest();

            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
            {
                filter.AuthenticateAsync(null, CancellationToken.None).ThrowIfFaulted();
            }, "context");
        }

        [Fact]
        public void AuthenticateAsync_Throws_WhenRequestIsNull()
        {
            // Arrange
            IAuthenticationFilter filter = CreateProductUnderTest();
            HttpAuthenticationContext context = CreateAuthenticationContext();
            Assert.Null(context.Request);

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                filter.AuthenticateAsync(context, CancellationToken.None).ThrowIfFaulted();
            });
            Assert.Equal("HttpAuthenticationContext.Request must not be null.", exception.Message);
        }

        [Fact]
        public void AuthenticateAsync_ReturnsCanceledTask_WhenCancellationIsRequested()
        {
            // Arrange
            IAuthenticationFilter filter = CreateProductUnderTest();
            IDictionary<string, object> environment = CreateOwinEnvironment();

            Task task;

            using (HttpRequestMessage request = CreateRequest(environment))
            {
                HttpAuthenticationContext context = CreateAuthenticationContext(request);
                CancellationToken cancellationToken = new CancellationToken(true);

                // Act
                task = filter.AuthenticateAsync(context, cancellationToken);

                // Assert
                task.WaitUntilCompleted();
            }

            Assert.Equal(TaskStatus.Canceled, task.Status);
        }

        [Fact]
        public void ChallengeAsync_AddsAuthenticationType_WhenOwinChallengeAlreadyExists()
        {
            // Arrange
            string expectedAuthenticationType = "AuthenticationType";
            IAuthenticationFilter filter = CreateProductUnderTest(expectedAuthenticationType);
            IHttpActionResult result = CreateDummyActionResult();
            string originalAuthenticationType = "FirstChallenge";
            IDictionary<string, string> originalExtra = CreateDummyExtra();
            Tuple<string[], IDictionary<string, string>> originalChallenge =
                new Tuple<string[], IDictionary<string, string>>(new string[] { originalAuthenticationType },
                    originalExtra);
            IDictionary<string, object> environment = CreateOwinEnvironment(originalChallenge);

            using (HttpRequestMessage request = CreateRequest(environment))
            {
                HttpAuthenticationChallengeContext context = CreateChallengeContext(request, result);

                // Act
                filter.ChallengeAsync(context, CancellationToken.None).Wait();
            }

            // Assert
            Tuple<string[], IDictionary<string, string>> challenge =
                environment.GetOwinValue<Tuple<string[], IDictionary<string, string>>>(ChallengeKey);
            Assert.NotNull(challenge);
            string[] authenticationTypes = challenge.Item1;
            Assert.NotNull(authenticationTypes);
            Assert.Equal(2, authenticationTypes.Length);
            Assert.Same(originalAuthenticationType, authenticationTypes[0]);
            Assert.Same(expectedAuthenticationType, authenticationTypes[1]);
            IDictionary<string, string> extra = challenge.Item2;
            Assert.Same(originalExtra, extra);
        }

        [Fact]
        public void ChallengeAsync_CreatesAuthenticationTypes_WhenOwinChallengeWithNullTypesAlreadyExists()
        {
            // Arrange
            string expectedAuthenticationType = "AuthenticationType";
            IAuthenticationFilter filter = CreateProductUnderTest(expectedAuthenticationType);
            IHttpActionResult result = CreateDummyActionResult();
            IDictionary<string, string> originalExtra = CreateDummyExtra();
            Tuple<string[], IDictionary<string, string>> originalChallenge =
                new Tuple<string[], IDictionary<string, string>>(null, originalExtra);
            IDictionary<string, object> environment = CreateOwinEnvironment(originalChallenge);

            using (HttpRequestMessage request = CreateRequest(environment))
            {
                HttpAuthenticationChallengeContext context = CreateChallengeContext(request, result);

                // Act
                filter.ChallengeAsync(context, CancellationToken.None).Wait();
            }

            // Assert
            Tuple<string[], IDictionary<string, string>> challenge =
                environment.GetOwinValue<Tuple<string[], IDictionary<string, string>>>(ChallengeKey);
            Assert.NotNull(challenge);
            string[] authenticationTypes = challenge.Item1;
            Assert.NotNull(authenticationTypes);
            Assert.Equal(1, authenticationTypes.Length);
            Assert.Same(expectedAuthenticationType, authenticationTypes[0]);
            IDictionary<string, string> extra = challenge.Item2;
            Assert.Same(originalExtra, extra);
        }

        [Fact]
        public void ChallengeAsync_CreatesOwinChallengeWithAuthenticationType_WhenNoChallengeExists()
        {
            // Arrange
            string expectedAuthenticationType = "AuthenticationType";
            IAuthenticationFilter filter = CreateProductUnderTest(expectedAuthenticationType);
            IHttpActionResult result = CreateDummyActionResult();
            IDictionary<string, object> environment = CreateOwinEnvironment();

            using (HttpRequestMessage request = CreateRequest(environment))
            {
                HttpAuthenticationChallengeContext context = CreateChallengeContext(request, result);

                // Act
                filter.ChallengeAsync(context, CancellationToken.None).Wait();
            }

            // Assert
            Tuple<string[], IDictionary<string, string>> challenge =
                environment.GetOwinValue<Tuple<string[], IDictionary<string, string>>>(ChallengeKey);
            Assert.NotNull(challenge);
            string[] authenticationTypes = challenge.Item1;
            Assert.NotNull(authenticationTypes);
            Assert.Equal(1, authenticationTypes.Length);
            Assert.Same(expectedAuthenticationType, authenticationTypes[0]);
            IDictionary<string, string> extra = challenge.Item2;
            Assert.NotNull(extra);
            Assert.Equal(0, extra.Count);
        }

        [Fact]
        public void ChallengeAsync_Throws_WhenContextIsNull()
        {
            // Arrange
            IAuthenticationFilter filter = CreateProductUnderTest();

            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
            {
                filter.ChallengeAsync(null, CancellationToken.None).ThrowIfFaulted();
            }, "context");
        }

        [Fact]
        public void ChallengeAsync_Throws_WhenRequestIsNull()
        {
            // Arrange
            IAuthenticationFilter filter = CreateProductUnderTest();
            IHttpActionResult result = CreateDummyActionResult();
            HttpAuthenticationChallengeContext context = new HttpAuthenticationChallengeContext(
                new HttpActionContext(), result);
            Assert.Null(context.Request);

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                filter.ChallengeAsync(context, CancellationToken.None).ThrowIfFaulted();
            });
            Assert.Equal("HttpAuthenticationContext.Request must not be null.", exception.Message);
        }

        private static HttpActionContext CreateActionContext(HttpRequestMessage request)
        {
            HttpControllerContext controllerContext = new HttpControllerContext();
            controllerContext.Request = request;
            HttpActionDescriptor descriptor = CreateDummyActionDescriptor();
            return new HttpActionContext(controllerContext, descriptor);
        }

        private static HttpAuthenticationContext CreateAuthenticationContext()
        {
            HttpActionContext actionContext = new HttpActionContext();
            IPrincipal principal = CreateDummyPrincipal();
            return new HttpAuthenticationContext(actionContext, principal);
        }

        private static HttpAuthenticationContext CreateAuthenticationContext(HttpRequestMessage request)
        {
            IPrincipal principal = CreateDummyPrincipal();
            return CreateAuthenticationContext(request, principal);
        }

        private static HttpAuthenticationContext CreateAuthenticationContext(HttpRequestMessage request,
            IPrincipal principal)
        {
            HttpActionContext actionContext = CreateActionContext(request);
            return new HttpAuthenticationContext(actionContext, principal);
        }

        private static HttpAuthenticationChallengeContext CreateChallengeContext(HttpRequestMessage request,
            IHttpActionResult result)
        {
            HttpActionContext actionContext = CreateActionContext(request);
            return new HttpAuthenticationChallengeContext(actionContext, result);
        }

        private static HttpActionDescriptor CreateDummyActionDescriptor()
        {
            return new Mock<HttpActionDescriptor>(MockBehavior.Strict).Object;
        }

        private static IHttpActionResult CreateDummyActionResult()
        {
            return new Mock<IHttpActionResult>(MockBehavior.Strict).Object;
        }

        private static IDictionary<string, string> CreateDummyExtra()
        {
            return new Mock<IDictionary<string, string>>(MockBehavior.Strict).Object;
        }

        private static ClaimsIdentity CreateDummyIdentity()
        {
            return new Mock<ClaimsIdentity>(MockBehavior.Strict).Object;
        }

        private static IPrincipal CreateDummyPrincipal()
        {
            return new Mock<IPrincipal>(MockBehavior.Strict).Object;
        }

        private static IDictionary<string, object> CreateOwinEnvironment()
        {
            return new Dictionary<string, object>();
        }

        private static IDictionary<string, object> CreateOwinEnvironment(Func<string[], Action<IIdentity,
            IDictionary<string, string>, IDictionary<string, object>, object>, object, Task> authenticate)
        {
            IDictionary<string, object> environment = CreateOwinEnvironment();
            environment.Add("security.Authenticate", authenticate);
            return environment;
        }

        private static IDictionary<string, object> CreateOwinEnvironment(Tuple<string[],
            IDictionary<string, string>> challenge)
        {
            IDictionary<string, object> environment = CreateOwinEnvironment();
            environment.Add(ChallengeKey, challenge);
            return environment;
        }

        private static HostAuthenticationFilter CreateProductUnderTest()
        {
            return CreateProductUnderTest("IgnoreAuthenticationType");
        }

        private static HostAuthenticationFilter CreateProductUnderTest(string authenticationType)
        {
            return new HostAuthenticationFilter(authenticationType);
        }

        private static HttpRequestMessage CreateRequest(IDictionary<string, object> owinEnvironment)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetOwinEnvironment(owinEnvironment);
            return request;
        }
    }
}
