// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Mvc.Facebook.Authorization;
using Microsoft.AspNet.Mvc.Facebook.Test.Helpers;
using Microsoft.TestCommon;

namespace Microsoft.AspNet.Mvc.Facebook.Test
{
    public class FacebookAuthorizeFilterHookTest
    {
        [Theory]
        [InlineData("http://example.com", "email", true)]
        [InlineData("http://example.com?error=access_denied", "email", false)]
        [InlineData("http://example.com?error=access_denied", null, false)]
        public void OnAuthorization_TriggersPreHookPriorToPermissionsDialog(string requestUrl,
                                                                            string permission,
                                                                            bool expectedTrigger)
        {
            // Arrange
            var config = BuildConfiguration("~/home/permissions");
            var authorizeFilter = new CustomDefaultAuthorizeFilter(config);
            var context = BuildSignedAuthorizationContext(requestUrl, permission);

            // Act
            authorizeFilter.OnAuthorization(context);

            // Assert
            Assert.Equal(expectedTrigger, authorizeFilter.PermissionPromptHookTriggered);
        }

        [Theory]
        [InlineData("http://example.com", "email", true)]
        [InlineData("http://example.com?error=access_denied", "email", false)]
        [InlineData("http://example.com?error=access_denied", null, false)]
        public void OnAuthorization_TriggersDeniedHook(string requestUrl, string permission, bool expectedTrigger)
        {
            // Arrange
            var config = BuildConfiguration("~/home/permissions");
            var authorizeFilter = new CustomDefaultAuthorizeFilter(config);
            var persistedCookies = new HttpCookieCollection();
            persistedCookies.Add(
                new HttpCookie(
                    PermissionHelper.RequestedPermissionCookieName, permission ?? string.Empty));
            var context = BuildSignedAuthorizationContext(requestUrl, permission, persistedCookies);

            // Act
            authorizeFilter.OnAuthorization(context);

            // Assert
            Assert.Equal(expectedTrigger, authorizeFilter.DeniedPermissionPromptHookTriggered);
        }

        [Theory]
        [InlineData("http://example.com", "email", "email", true)]
        [InlineData("http://example.com", "email", "foo", false)]
        [InlineData("http://example.com?error=access_denied", "email", "email", false)]
        [InlineData("http://example.com?error=access_denied", "email", "foo", false)]
        [InlineData("http://example.com?error=access_denied", null, "foo", false)]
        public void OnAuthorization_TriggersDeniedHookWithRevokedPermissions(string requestUrl,
                                                                             string permission,
                                                                             string permissionInStatus,
                                                                             bool expectedTrigger)
        {
            var rawPermissionsStatus = new Dictionary<string, string>
            {
                { "permission", permissionInStatus },
                { "status", "declined" },
            };

            var data = new List<IDictionary<string, string>>(new[] { rawPermissionsStatus });

            // Arrange
            var config = BuildConfiguration("~/home/permissions", userPermissionsStatus:
                new PermissionsStatus(data));
            var authorizeFilter = new CustomDefaultAuthorizeFilter(config);
            var context = BuildSignedAuthorizationContext(requestUrl, permission);

            // Act
            authorizeFilter.OnAuthorization(context);

            // Assert
            Assert.Equal(expectedTrigger, authorizeFilter.DeniedPermissionPromptHookTriggered);
        }

        [Theory]
        [InlineData("http://example.com", "email", true)]
        [InlineData("http://example.com?error=access_denied", "email", false)]
        [InlineData("http://example.com?error=access_denied", null, false)]
        public void OnAuthorization_TriggersDeniedHookAfterPersistingRequestedPermissions(string requestUrl,
                                                                                          string permission,
                                                                                          bool expectedTrigger)
        {
            // Arrange
            var config = BuildConfiguration("~/home/permissions");
            var authorizeFilter = new CustomDefaultAuthorizeFilter(config);
            var context = BuildSignedAuthorizationContext(requestUrl, permission);

            // Act
            authorizeFilter.OnAuthorization(context);

            // Here we're acting like a browser and adding the responses cookies to the "next" request's cookies
            var responseCookies = context.HttpContext.Response.Cookies;
            foreach (var cookieName in responseCookies.AllKeys)
            {
                context.HttpContext.Request.Cookies.Add(responseCookies[cookieName]);
            }

            // Assert
            Assert.Equal(false, authorizeFilter.DeniedPermissionPromptHookTriggered);

            // Act 2
            // We're making a "second" request essentially
            authorizeFilter.OnAuthorization(context);

            // Assert 2
            Assert.Equal(expectedTrigger, authorizeFilter.DeniedPermissionPromptHookTriggered);
        }

        [Fact]
        public void OnAuthorization_PreHookMustReturnActionResult()
        {
            // Arrange
            var config = BuildConfiguration("~/home/permissions");
            var authorizeFilter = new CustomInvalidAuthorizeFilter(config);
            var context = BuildSignedAuthorizationContext("http://www.example.com", "email");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => authorizeFilter.OnAuthorization(context));
        }

        [Fact]
        public void OnAuthorization_DeniedHookMustReturnActionResult()
        {
            // Arrange
            var config = BuildConfiguration("~/home/permissions");
            var authorizeFilter = new CustomInvalidAuthorizeFilter(config);
            var persistedCookies = new HttpCookieCollection();
            persistedCookies.Add(
                new HttpCookie(
                    PermissionHelper.RequestedPermissionCookieName, "email"));
            var context = BuildSignedAuthorizationContext("http://www.example.com", "email", persistedCookies);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => authorizeFilter.OnAuthorization(context));
        }

        [Fact]
        public void OnAuthorization_PreHookCustomActionResultIsContextsResult()
        {
            // Arrange
            var tempUrl = "http://www.example.com";
            var config = BuildConfiguration("~/home/permissions");
            var preHookResult = new RedirectResult(tempUrl);
            var authorizeFilter = new CustomReturningAuthorizeFilter(config,
                                                                     preHookResult,
                                                                     new RedirectResult(tempUrl));
            var context = BuildSignedAuthorizationContext(tempUrl, "email");

            // Act
            authorizeFilter.OnAuthorization(context);

            // Assert
            Assert.Equal(preHookResult, context.Result);
        }

        [Fact]
        public void OnAuthorization_DeniedHookCustomActionResultIsContextsResult()
        {
            // Arrange
            var tempUrl = "http://www.example.com";
            var config = BuildConfiguration("~/home/permissions");
            var deniedHookResult = new RedirectResult(tempUrl);
            var authorizeFilter = new CustomReturningAuthorizeFilter(config,
                                                                     new RedirectResult(tempUrl),
                                                                     deniedHookResult);
            var persistedCookies = new HttpCookieCollection();
            persistedCookies.Add(
                new HttpCookie(
                    PermissionHelper.RequestedPermissionCookieName, "email"));
            var context = BuildSignedAuthorizationContext(tempUrl, "email", persistedCookies);

            // Act
            authorizeFilter.OnAuthorization(context);

            // Assert
            Assert.Equal(deniedHookResult, context.Result);
        }

        // Helper methods and classes
        private FacebookConfiguration BuildConfiguration(string authorizationRedirectPath,
                                                         PermissionsStatus userPermissionsStatus = null)
        {
            var client = MockHelpers.CreateFacebookClient();
            var permissionService = MockHelpers.CreatePermissionService(new[] { "" }, userPermissionsStatus);
            var config = MockHelpers.CreateConfiguration(client, permissionService);
            config.AuthorizationRedirectPath = authorizationRedirectPath;

            return config;
        }

        private AuthorizationContext BuildSignedAuthorizationContext(string requestUrl,
                                                                     string permission,
                                                                     HttpCookieCollection requestCookies = null)
        {
            var permissions = permission == null ? new string[0] : new string[] { permission };

            var context = new AuthorizationContext(
                MockHelpers.CreateControllerContext(new NameValueCollection
                {
                    {"signed_request", "exampleSignedRequest"}
                },
                null,
                new Uri(requestUrl),
                requestCookies),
                MockHelpers.CreateActionDescriptor(new[] { new FacebookAuthorizeAttribute(permissions) }));

            return context;
        }

        private class CustomInvalidAuthorizeFilter : FacebookAuthorizeFilter
        {
            public CustomInvalidAuthorizeFilter(FacebookConfiguration config)
                : base(config)
            { }

            protected override ActionResult OnPermissionPrompt(PermissionsContext context)
            {
                return null;
            }

            protected override ActionResult OnDeniedPermissionPrompt(PermissionsContext context)
            {
                return null;
            }
        }

        private class CustomDefaultAuthorizeFilter : FacebookAuthorizeFilter
        {
            public CustomDefaultAuthorizeFilter(FacebookConfiguration config)
                : base(config)
            { }

            public bool PermissionPromptHookTriggered { get; private set; }
            public bool DeniedPermissionPromptHookTriggered { get; private set; }

            protected override ActionResult OnPermissionPrompt(PermissionsContext context)
            {
                PermissionPromptHookTriggered = true;

                return base.OnPermissionPrompt(context);
            }

            protected override ActionResult OnDeniedPermissionPrompt(PermissionsContext context)
            {
                DeniedPermissionPromptHookTriggered = true;

                return base.OnDeniedPermissionPrompt(context);
            }
        }

        private class CustomReturningAuthorizeFilter : FacebookAuthorizeFilter
        {
            private ActionResult _promptPermissionHookResult;
            private ActionResult _deniedPermissionPromptHookResult;

            public CustomReturningAuthorizeFilter(FacebookConfiguration config,
                                                  ActionResult promptPermissionHookResult,
                                                  ActionResult deniedPermissionPromptHookResult)
                : base(config)
            {
                _promptPermissionHookResult = promptPermissionHookResult;
                _deniedPermissionPromptHookResult = deniedPermissionPromptHookResult;
            }

            protected override ActionResult OnPermissionPrompt(PermissionsContext context)
            {
                return _promptPermissionHookResult;
            }

            protected override ActionResult OnDeniedPermissionPrompt(PermissionsContext context)
            {
                return _deniedPermissionPromptHookResult;
            }
        }
    }
}
