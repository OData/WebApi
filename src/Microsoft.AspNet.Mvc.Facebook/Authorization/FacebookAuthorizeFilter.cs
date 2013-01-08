// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            IEnumerable<object> authorizeAttributes = filterContext.ActionDescriptor.GetCustomAttributes(typeof(FacebookAuthorizeAttribute), inherit: true)
                .Union(filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(FacebookAuthorizeAttribute), inherit: true));
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

            if (signedRequest == null || String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(accessToken))
            {
                // Cannot obtain user information from signed_request, redirect to Facebook OAuth dialog.
                string redirectUrl = GetRedirectUrl(request);
                Uri loginUrl = client.GetLoginUrl(redirectUrl, _config.AppId, null);
                filterContext.Result = CreateRedirectResult(loginUrl);
            }
            else
            {
                HashSet<string> requiredPermissions = GetRequiredPermissions(authorizeAttributes);
                if (requiredPermissions.Count > 0)
                {
                    IEnumerable<string> currentPermissions = _config.PermissionService.GetUserPermissions(userId, accessToken);

                    // If the current permissions doesn't cover all required permissions,
                    // redirect to the specified redirect path or to the Facebook OAuth dialog.
                    if (currentPermissions == null || !requiredPermissions.IsSubsetOf(currentPermissions))
                    {
                        NameValueCollection parsedQueries = HttpUtility.ParseQueryString(request.Url.Query);
                        bool hasError = !String.IsNullOrEmpty(parsedQueries["error"]);
                        string redirectUrl = GetRedirectUrl(request, parsedQueries);
                        string requiredPermissionString = String.Join(",", requiredPermissions);
                        Uri authorizationUrl;

                        if (!String.IsNullOrEmpty(_config.AuthorizationRedirectPath) && hasError)
                        {
                            UriBuilder authorizationUrlBuilder = new UriBuilder(_config.AppUrl);
                            authorizationUrlBuilder.Path += _config.AuthorizationRedirectPath.Substring(1);
                            authorizationUrlBuilder.Query = String.Format(CultureInfo.InvariantCulture,
                                "originUrl={0}&permissions={1}",
                                HttpUtility.UrlEncode(redirectUrl),
                                HttpUtility.UrlEncode(requiredPermissionString));
                            authorizationUrl = authorizationUrlBuilder.Uri;
                        }
                        else
                        {
                            authorizationUrl = client.GetLoginUrl(redirectUrl, _config.AppId, requiredPermissionString);
                        }
                        filterContext.Result = CreateRedirectResult(authorizationUrl);
                    }
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
                "<script>window.top.location = '{0}';</script>",
                HttpUtility.JavaScriptStringEncode(redirectUrl.AbsoluteUri));
            return facebookAuthResult;
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

        private static HashSet<string> GetRequiredPermissions(IEnumerable<object> facebookAuthorizeAttributes)
        {
            HashSet<string> requiredPermissions = new HashSet<string>();
            foreach (FacebookAuthorizeAttribute facebookAuthorize in facebookAuthorizeAttributes)
            {
                foreach (string permission in facebookAuthorize.Permissions)
                {
                    if (permission.Contains(','))
                    {
                        throw new ArgumentException(
                            String.Format(
                                CultureInfo.CurrentCulture,
                                Resources.PermissionStringShouldNotContainComma,
                                permission));
                    }

                    requiredPermissions.Add(permission);
                }
            }
            return requiredPermissions;
        }
    }
}