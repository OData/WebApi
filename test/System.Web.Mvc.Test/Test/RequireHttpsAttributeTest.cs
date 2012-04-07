// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    public class RequireHttpsAttributeTest
    {
        [Fact]
        public void HandleNonHttpsRequestExtensibility()
        {
            // Arrange
            Mock<AuthorizationContext> mockAuthContext = new Mock<AuthorizationContext>();
            mockAuthContext.Setup(c => c.HttpContext.Request.IsSecureConnection).Returns(false);
            AuthorizationContext authContext = mockAuthContext.Object;

            RequireHttpsAttribute attr = new MyRequireHttpsAttribute();

            // Act
            attr.OnAuthorization(authContext);
            ContentResult result = authContext.Result as ContentResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Custom HandleNonHttpsRequest", result.Content);
        }

        [Fact]
        public void OnAuthorizationDoesNothingIfRequestIsSecure()
        {
            // Arrange
            Mock<AuthorizationContext> mockAuthContext = new Mock<AuthorizationContext>();
            mockAuthContext.Setup(c => c.HttpContext.Request.IsSecureConnection).Returns(true);
            AuthorizationContext authContext = mockAuthContext.Object;

            ViewResult result = new ViewResult();
            authContext.Result = result;

            RequireHttpsAttribute attr = new RequireHttpsAttribute();

            // Act
            attr.OnAuthorization(authContext);

            // Assert
            Assert.Same(result, authContext.Result);
        }

        [Fact]
        public void OnAuthorizationRedirectsIfRequestIsNotSecureAndMethodIsGet()
        {
            // Arrange
            Mock<AuthorizationContext> mockAuthContext = new Mock<AuthorizationContext>();
            mockAuthContext.Setup(c => c.HttpContext.Request.HttpMethod).Returns("get");
            mockAuthContext.Setup(c => c.HttpContext.Request.IsSecureConnection).Returns(false);
            mockAuthContext.Setup(c => c.HttpContext.Request.RawUrl).Returns("/alpha/bravo/charlie?q=quux");
            mockAuthContext.Setup(c => c.HttpContext.Request.Url).Returns(new Uri("http://www.example.com:8080/foo/bar/baz"));
            AuthorizationContext authContext = mockAuthContext.Object;

            RequireHttpsAttribute attr = new RequireHttpsAttribute();

            // Act
            attr.OnAuthorization(authContext);
            RedirectResult result = authContext.Result as RedirectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://www.example.com/alpha/bravo/charlie?q=quux", result.Url);
        }

        [Fact]
        public void OnAuthorizationThrowsIfFilterContextIsNull()
        {
            // Arrange
            RequireHttpsAttribute attr = new RequireHttpsAttribute();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { attr.OnAuthorization(null); }, "filterContext");
        }

        [Fact]
        public void OnAuthorizationThrowsIfRequestIsNotSecureAndMethodIsNotGet()
        {
            // Arrange
            Mock<AuthorizationContext> mockAuthContext = new Mock<AuthorizationContext>();
            mockAuthContext.Setup(c => c.HttpContext.Request.HttpMethod).Returns("post");
            mockAuthContext.Setup(c => c.HttpContext.Request.IsSecureConnection).Returns(false);
            AuthorizationContext authContext = mockAuthContext.Object;

            RequireHttpsAttribute attr = new RequireHttpsAttribute();

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                delegate { attr.OnAuthorization(authContext); },
                @"The requested resource can only be accessed via SSL.");
        }

        private class MyRequireHttpsAttribute : RequireHttpsAttribute
        {
            protected override void HandleNonHttpsRequest(AuthorizationContext filterContext)
            {
                filterContext.Result = new ContentResult() { Content = "Custom HandleNonHttpsRequest" };
            }
        }
    }
}
