// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Web;
using Facebook;
using Microsoft.AspNet.Mvc.Facebook.Models;

namespace Microsoft.AspNet.Mvc.Facebook.Services
{
    public interface IFacebookService
    {
        TUser LoadUser<TUser>(HttpContextBase httpContext) where TUser : FacebookUser, new();
        string VerificationToken { get; set; }
        string GetAppAccessToken();
        string GetFields(Type modelType);
        void RefreshUserFields(FacebookUser user, params string[] fields);
        void RefreshUserFields(FacebookUser user, dynamic userFields, params string[] fields);
        FacebookClient CreateClient(string accessToken = null);
        void InitializeRealtime(Type userType, string callbackUrl = null);
        string GetRealtimeFields(Type userType);
        FacebookAuthorizationInfo Authorize(HttpContextBase httpContext);
        FacebookAuthorizationInfo Authorize(HttpContextBase httpContext, string[] permissions);
    }
}
