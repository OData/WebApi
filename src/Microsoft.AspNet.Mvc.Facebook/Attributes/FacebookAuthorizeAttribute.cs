// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Mvc.Facebook.Services;

namespace Microsoft.AspNet.Mvc.Facebook.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class FacebookAuthorizeAttribute : FilterAttribute, IAuthorizationFilter, IActionFilter
    {
        private readonly IFacebookService _facebookService;

        public FacebookAuthorizeAttribute()
            : this(DefaultFacebookService.Instance)
        {
        }

        public FacebookAuthorizeAttribute(IFacebookService facebookService)
        {
            _facebookService = facebookService;
        }

        public string Permissions { get; set; }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            // TODO: (ntotten) - Handle scenario where user denies authorization
            // https://www.facebook.com/dialog/oauth?perms=email&redirect_uri=https://apps.facebook.com/mvctetmsadsf/Home/Test?error_reason=user_denied&error=access_denied&error_description=The+user+denied+your+request.&client_id=202821839850333

            // TODO: (ntotten) - Allow developer to specify to send user to url/view rather than automatic authorization

            // TODO: (ntotten) - Set the state parameter to protect against cross-site request forgery.
            // This will require session state to be used so we have to fall back if session is disabled.
            // https://developers.facebook.com/docs/reference/dialogs/oauth/#parameters

            FacebookAuthorizationInfo authInfo;
            if (!String.IsNullOrWhiteSpace(Permissions))
            {
                var permissions = Permissions.Split(',').Select(s => s.Trim()).ToArray();
                authInfo = _facebookService.Authorize(filterContext.HttpContext, permissions);
            }
            else
            {
                authInfo = _facebookService.Authorize(filterContext.HttpContext);
            }

            // Check if user has allowed app and has permissions
            // If authorized add access_token to ViewBag
            if (authInfo.IsAuthorized)
            {
                filterContext.Controller.ViewBag.FacebookAccessToken = authInfo.AccessToken;
            }
            else
            {
                var client = _facebookService.CreateClient();

                // NOTE: (ntotten) - Do we need to handle mobile in a iFrame app?

                var appPath = FacebookSettings.AppNamespace;
                if (String.IsNullOrWhiteSpace(appPath))
                {
                    appPath = FacebookSettings.AppId;
                }

                var redirectUri = String.Format(CultureInfo.InvariantCulture,
                    "{0}/{1}{2}",
                    FacebookSettings.FacebookAppUrl.TrimEnd('/'),
                    appPath,
                    filterContext.HttpContext.Request.Url.PathAndQuery);

                Dictionary<string, object> loginUrlParameters = new Dictionary<string, object>();
                loginUrlParameters["redirect_uri"] = redirectUri;
                loginUrlParameters["client_id"] = FacebookSettings.AppId;
                if (!String.IsNullOrWhiteSpace(Permissions))
                {
                    loginUrlParameters["scope"] = Permissions;
                }

                var loginUrl = client.GetLoginUrl(loginUrlParameters);

                var facebookAuthResult = new ContentResult();
                facebookAuthResult.ContentType = "text/html";
                facebookAuthResult.Content = String.Format("<script>window.top.location = '{0}';</script>", loginUrl.AbsoluteUri);
                filterContext.Result = facebookAuthResult;
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var redirectResult = filterContext.Result as RedirectResult;
            if (redirectResult != null)
            {
                var result = GetRedirectTopResult(filterContext.HttpContext.Request, redirectResult.Url);
                filterContext.Result = result;
                return;
            }

            var redirectToRouteResult = filterContext.Result as RedirectToRouteResult;
            if (redirectToRouteResult != null)
            {
                var url = new UrlHelper(filterContext.RequestContext).RouteUrl(redirectToRouteResult.RouteName, redirectToRouteResult.RouteValues);
                var result = GetRedirectTopResult(filterContext.HttpContext.Request, url);
                filterContext.Result = result;
                return;
            }
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        private ContentResult GetRedirectTopResult(HttpRequestBase request, string url)
        {
            url = String.Concat(
               request.Url.Scheme,
               "://apps.facebook.com/",
               FacebookSettings.AppNamespace,
               url);

            var facebookAuthResult = new ContentResult();
            facebookAuthResult.ContentType = "text/html";
            facebookAuthResult.Content = String.Format("<script>window.top.location = '{0}';</script>", url);
            return facebookAuthResult;
        }
    }
}
