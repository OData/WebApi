// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Facebook;
using Microsoft.AspNet.Mvc.Facebook.Client;

namespace Microsoft.AspNet.Mvc.Facebook.Authorization
{
    /// <summary>
    /// Authorization filter that verifies the signed requests and permissions from Facebook.
    /// </summary>
    public class FacebookAuthorizeFilter : IAuthorizationFilter
    {
        private static readonly Uri DefaultAuthorizationRedirectUrl = new Uri("https://www.facebook.com/");
        private const string ExecuteMethodCannotBeCalledFormat = "The {0} execute method should not be called.";

        private FacebookConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookAuthorizeFilter" /> class.
        /// </summary>
        /// <param name="config">The <see cref="FacebookConfiguration"/>.</param>
        public FacebookAuthorizeFilter(FacebookConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            _config = config;
        }

        /// <summary>
        /// Called when authorization is required.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Type references are needed for authorization")]
        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            IEnumerable<FacebookAuthorizeAttribute> authorizeAttributes = filterContext.ActionDescriptor
                .GetCustomAttributes(typeof(FacebookAuthorizeAttribute), inherit: true)
                .Union(filterContext.ActionDescriptor.ControllerDescriptor
                    .GetCustomAttributes(typeof(FacebookAuthorizeAttribute), inherit: true))
                .OfType<FacebookAuthorizeAttribute>();
            if (!authorizeAttributes.Any())
            {
                return;
            }

            FacebookClient client = _config.ClientProvider.CreateClient();
            HttpRequestBase request = filterContext.HttpContext.Request;
            dynamic signedRequest = FacebookRequestHelpers.GetSignedRequest(
                filterContext.HttpContext,
                rawSignedRequest =>
                {
                    return client.ParseSignedRequest(rawSignedRequest);
                });
            string userId = null;
            string accessToken = null;
            if (signedRequest != null)
            {
                userId = signedRequest.user_id;
                accessToken = signedRequest.oauth_token;
            }

            NameValueCollection parsedQueries = HttpUtility.ParseQueryString(request.Url.Query);
            HashSet<string> requiredPermissions = PermissionHelper.GetRequiredPermissions(authorizeAttributes);
            bool handleError = !String.IsNullOrEmpty(parsedQueries["error"]);

            // This must occur AFTER the handleError calculation because it modifies the parsed queries.
            string redirectUrl = GetRedirectUrl(request, parsedQueries);

            // Check if there was an error and we should handle it.
            if (handleError)
            {
                Uri errorUrl;

                if (String.IsNullOrEmpty(_config.AuthorizationRedirectPath))
                {
                    errorUrl = DefaultAuthorizationRedirectUrl;
                }
                else
                {
                    errorUrl = GetErroredAuthorizeUri(redirectUrl, requiredPermissions);
                }

                filterContext.Result = CreateRedirectResult(errorUrl);

                // There was an error so short circuit
                return;
            }

            FacebookContext facebookContext = new FacebookContext
            {
                Client = client,
                SignedRequest = signedRequest,
                AccessToken = accessToken,
                UserId = userId,
                Configuration = _config
            };

            PermissionsContext permissionsContext = new PermissionsContext
            {
                FacebookContext = facebookContext,
                FilterContext = filterContext,
                RequiredPermissions = requiredPermissions,
            };

            // Check if we need to prompt for default permissions.
            if (signedRequest == null || String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(accessToken))
            {
                PromptDefaultPermissions(permissionsContext, redirectUrl);
            }
            else if (requiredPermissions.Any())
            {
                PermissionsStatus currentPermissionsStatus = _config.PermissionService.GetUserPermissionsStatus(userId, accessToken);
                // Instead of performing another request to gather "granted" permissions just parse the status
                IEnumerable<string> currentPermissions = PermissionHelper.GetGrantedPermissions(currentPermissionsStatus);
                IEnumerable<string> missingPermissions = requiredPermissions.Except(currentPermissions);

                // If we have missing permissions than we need to present a prompt or redirect to an error 
                // page if there's an error.
                if (missingPermissions.Any())
                {
                    permissionsContext.MissingPermissions = missingPermissions;
                    permissionsContext.DeclinedPermissions = PermissionHelper.GetDeclinedPermissions(currentPermissionsStatus);
                    permissionsContext.SkippedPermissions = PermissionHelper.GetSkippedPermissions(
                        filterContext.HttpContext.Request,
                        missingPermissions,
                        permissionsContext.DeclinedPermissions);

                    PromptMissingPermissions(permissionsContext, redirectUrl);
                }
            }
        }

        /// <summary>
        /// Called when authorization fails and need to create a redirect result.
        /// </summary>
        /// <param name="redirectUrl">The redirect URL.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public virtual ActionResult CreateRedirectResult(Uri redirectUrl)
        {
            if (redirectUrl == null)
            {
                throw new ArgumentNullException("redirectUrl");
            }

            ContentResult facebookAuthResult = new ContentResult();
            facebookAuthResult.ContentType = "text/html";

            // Even though we're only JavaScript encoding the redirectUrl, the result is guaranteed to be HTML-safe as well
            facebookAuthResult.Content = String.Format(
                CultureInfo.InvariantCulture,
                "<script>window.top.location = '{0}';</script>",
                HttpUtility.JavaScriptStringEncode(redirectUrl.AbsoluteUri));
            return facebookAuthResult;
        }

        /// <summary>
        /// Returns an <see cref="ActionResult"/> that indicates we want to ignore a permission prompt.  Should only be used as
        /// a return value within the <see cref="OnPermissionPrompt"/> and <see cref="OnDeniedPermissionPrompt"/> methods.
        /// </summary>
        /// <returns>An <see cref="ActionResult"/> that indicates that we want to ignore a permission prompt.</returns>
        protected static ActionResult IgnorePrompt()
        {
            return new IgnorePromptResult();
        }

        /// <summary>
        /// Returns an <see cref="ActionResult"/> that indicates we want to show a permission prompt.  Should only be used as a
        /// return value within the <see cref="OnPermissionPrompt"/> and <see cref="OnDeniedPermissionPrompt"/> methods.
        /// </summary>
        /// <returns>An <see cref="ActionResult"/> that indicates that we want to show a permission prompt.</returns>
        protected static ActionResult ShowPrompt()
        {
            return new ShowPromptResult();
        }

        /// <summary>
        /// Invoked during <see cref="OnAuthorization"/> prior to a permission prompt that requests permissions that were skipped 
        /// or revoked. Occurs before the <see cref="OnPermissionPrompt"/> and short circuits the pipeline by default via
        /// returning an <see cref="IgnorePrompt"/> result.
        /// </summary>
        /// <param name="context">Provides access to permission information associated with the user.</param>
        /// <returns>An <see cref="ActionResult"/> for how to handle the denied permissions. Defaults to ignoring the coming prompt 
        /// via the <see cref="IgnorePrompt"/> result.</returns>
        protected virtual ActionResult OnDeniedPermissionPrompt(PermissionsContext context)
        {
            return IgnorePrompt();
        }

        /// <summary>
        /// Invoked during <see cref="OnAuthorization"/> prior to a permission prompt.
        /// </summary>
        /// <param name="context">Provides access to permission information associated with the user.</param>
        /// <returns>An <see cref="ActionResult"/> for how to handle the coming permission prompt. Defaults to showing the prompt  
        /// via the <see cref="ShowPrompt"/> result.</returns>
        protected virtual ActionResult OnPermissionPrompt(PermissionsContext context)
        {
            return ShowPrompt();
        }

        private static PermissionPromptResult ConvertActionResult(ActionResult result)
        {
            if (result is ShowPromptResult)
            {
                return PermissionPromptResult.Default;
            }
            else if (result is IgnorePromptResult)
            {
                return PermissionPromptResult.Ignore;
            }
            else if (result is ActionResult)
            {
                return PermissionPromptResult.Custom;
            }

            throw new ArgumentNullException("result");
        }

        private static bool HandleParsedHookResult(AuthorizationContext context, ActionResult result)
        {
            PermissionPromptResult hookResult = ConvertActionResult(result);

            if (hookResult == PermissionPromptResult.Ignore)
            {
                return true;
            }
            else if (hookResult == PermissionPromptResult.Custom)
            {
                context.Result = result;
                return true;
            }

            return false;
        }

        private Uri GetErroredAuthorizeUri(string originUrl, HashSet<string> requiredPermissions)
        {
            if (requiredPermissions == null)
            {
                throw new ArgumentNullException("requiredPermissions");
            }

            string requiredPermissionString = String.Join(",", requiredPermissions);
            UriBuilder authorizationUrlBuilder = new UriBuilder(new Uri(_config.AppUrl));
            authorizationUrlBuilder.Path += _config.AuthorizationRedirectPath.Substring(1);
            authorizationUrlBuilder.Query = String.Format(CultureInfo.InvariantCulture,
                "originUrl={0}&permissions={1}",
                HttpUtility.UrlEncode(originUrl),
                HttpUtility.UrlEncode(requiredPermissionString));

            return authorizationUrlBuilder.Uri;
        }

        private string GetRedirectUrl(HttpRequestBase request)
        {
            NameValueCollection queryNameValuePair = HttpUtility.ParseQueryString(request.Url.Query);
            return GetRedirectUrl(request, queryNameValuePair);
        }

        private string GetRedirectUrl(HttpRequestBase request, NameValueCollection queryNameValuePair)
        {
            // Don't propagate query strings added by Facebook OAuth Dialog
            queryNameValuePair.Remove("code");
            queryNameValuePair.Remove("error");
            queryNameValuePair.Remove("error_reason");
            queryNameValuePair.Remove("error_description");

            string redirectUrl = String.Format(
                CultureInfo.InvariantCulture,
                "{0}/{1}",
                _config.AppUrl.TrimEnd('/'),
                request.AppRelativeCurrentExecutionFilePath.TrimStart('~', '/'));

            string query = queryNameValuePair.ToString();
            if (!String.IsNullOrEmpty(query))
            {
                redirectUrl += "?" + query;
            }

            return redirectUrl;
        }

        private void PromptDefaultPermissions(PermissionsContext permissionsContext, string redirectUrl)
        {
            FacebookClient client = permissionsContext.FacebookContext.Client;
            // Cannot obtain user information from signed_request, redirect to Facebook OAuth dialog.
            Uri navigationUrl = client.GetLoginUrl(redirectUrl,
                                                   _config.AppId,
                                                   permissions: null);

            permissionsContext.FilterContext.Result = CreateRedirectResult(navigationUrl);
        }

        private void PromptMissingPermissions(PermissionsContext permissionsContext, string redirectUrl)
        {
            AuthorizationContext filterContext = permissionsContext.FilterContext;
            HashSet<string> requiredPermissions = permissionsContext.RequiredPermissions;

            // If there were no errors it means that we will be prompted with a permission prompt.
            // Therefore, invoke the permission prompt hooks and navigate to the prompt.

            IEnumerable<string> declinedPermissions = permissionsContext.DeclinedPermissions;
            IEnumerable<string> skippedPermissions = permissionsContext.SkippedPermissions;
            FacebookContext facebookContext = permissionsContext.FacebookContext;
            IEnumerable<string> missingPermissions = permissionsContext.MissingPermissions;

            // Declined permissions and skipped permissions can persist through multiple pages.  So we need to cross check
            // them against the current pages permissions, this will determine if we should invoke the denied permission hook.
            bool deniedPermissions = missingPermissions.Where(
                permission => declinedPermissions.Contains(permission) ||
                              skippedPermissions.Contains(permission)).Any();

            // The DeniedPermissionPromptHook will only be invoked if we detect there are denied permissions.
            // It is attempted PRIOR to the pre hook to allow app creators to handle situations when a user
            // skip's or revokes previously prompted permissions. Ex: redirect to a different page.
            if (deniedPermissions && InvokeDeniedPermissionPromptHook(permissionsContext))
            {
                return;
            }
            else if (InvokePrePermissionPromptHook(permissionsContext))
            {
                return;
            }

            // We persist the requested permissions in a cookie to know if a permission was denied in any way.
            // The persisted data allows us to detect skipping of permissions.
            PermissionHelper.PersistRequestedPermissions(filterContext, requiredPermissions);

            FacebookClient client = facebookContext.Client;
            Uri navigationUrl = client.GetLoginUrl(redirectUrl,
                                                   _config.AppId,
                                                   permissions: String.Join(",", requiredPermissions));

            filterContext.Result = CreateRedirectResult(navigationUrl);
        }

        private bool InvokeDeniedPermissionPromptHook(PermissionsContext context)
        {
            ActionResult postResult = OnDeniedPermissionPrompt(context);

            return HandleParsedHookResult(context.FilterContext, postResult);
        }

        private bool InvokePrePermissionPromptHook(PermissionsContext context)
        {
            ActionResult preResult = OnPermissionPrompt(context);

            return HandleParsedHookResult(context.FilterContext, preResult);
        }

        private enum PermissionPromptResult
        {
            Default,
            Ignore,
            Custom,
        }

        private class IgnorePromptResult : ActionResult
        {
            public override void ExecuteResult(ControllerContext context)
            {
                Debug.Fail(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        ExecuteMethodCannotBeCalledFormat,
                        typeof(IgnorePromptResult).Name));
            }
        }

        private class ShowPromptResult : ActionResult
        {
            public override void ExecuteResult(ControllerContext context)
            {
                Debug.Fail(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        ExecuteMethodCannotBeCalledFormat,
                        typeof(IgnorePromptResult).Name));
            }
        }
    }
}