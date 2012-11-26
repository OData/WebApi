// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Facebook;

namespace Microsoft.AspNet.Mvc.Facebook.Client
{
    public static class FacebookClientExtensions
    {
        public static Task<object> GetFacebookObjectAsync(this FacebookClient client, string objectPath)
        {
            return GetFacebookObjectAsync<object>(client, objectPath);
        }

        public static Task<TFacebookObject> GetFacebookObjectAsync<TFacebookObject>(this FacebookClient client, string objectPath) where TFacebookObject : class
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            if (objectPath == null)
            {
                throw new ArgumentNullException("objectPath");
            }

            string path = objectPath + FacebookQueryHelper.GetFields(typeof(TFacebookObject));
            return client.GetTaskAsync<TFacebookObject>(path);
        }

        public static Task<object> GetCurrentUserAsync(this FacebookClient client)
        {
            return GetCurrentUserAsync<object>(client);
        }

        public static Task<TUser> GetCurrentUserAsync<TUser>(this FacebookClient client) where TUser : class
        {
            return GetFacebookObjectAsync<TUser>(client, "me");
        }

        public static Task<IEnumerable<object>> GetCurrentUserFriendsAsync(this FacebookClient client)
        {
            return GetCurrentUserFriendsAsync<object>(client);
        }

        public static async Task<IEnumerable<TUserFriend>> GetCurrentUserFriendsAsync<TUserFriend>(this FacebookClient client) where TUserFriend : class
        {
            FacebookGroupConnection<TUserFriend> friends = await GetFacebookObjectAsync<FacebookGroupConnection<TUserFriend>>(client, "me/friends");
            return friends != null ?
                friends.Data :
                Enumerable.Empty<TUserFriend>();
        }

        public static async Task<IEnumerable<string>> GetCurrentUserPermissionsAsync(this FacebookClient client)
        {
            FacebookGroupConnection<IDictionary<string, int>> permissionResults = await client.GetTaskAsync<FacebookGroupConnection<IDictionary<string, int>>>("me/permissions");
            return ParsePermissions(permissionResults.Data);
        }

        public static async Task<IEnumerable<TStatus>> GetCurrentUserStatusesAsync<TStatus>(this FacebookClient client) where TStatus : class
        {
            FacebookGroupConnection<TStatus> statuses = await GetFacebookObjectAsync<FacebookGroupConnection<TStatus>>(client, "me/statuses");
            return statuses != null ?
                statuses.Data :
                Enumerable.Empty<TStatus>();
        }

        public static async Task<IEnumerable<TPhotos>> GetCurrentUserPhotosAsync<TPhotos>(this FacebookClient client) where TPhotos : class
        {
            FacebookGroupConnection<TPhotos> photos = await GetFacebookObjectAsync<FacebookGroupConnection<TPhotos>>(client, "me/photos");
            return photos != null ?
                photos.Data :
                Enumerable.Empty<TPhotos>();
        }

        internal static Uri GetLoginUrl(this FacebookClient client, string redirectUrl, string appId, string permissions)
        {
            if (String.IsNullOrEmpty(redirectUrl))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, "redirectUrl");
            }
            if (String.IsNullOrEmpty(appId))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, "appId");
            }

            Dictionary<string, object> loginUrlParameters = new Dictionary<string, object>();
            loginUrlParameters["redirect_uri"] = redirectUrl;
            loginUrlParameters["client_id"] = appId;
            if (!String.IsNullOrEmpty(permissions))
            {
                loginUrlParameters["scope"] = permissions;
            }

            return client.GetLoginUrl(loginUrlParameters);
        }

        internal static object ParseSignedRequest(this FacebookClient client, HttpRequestBase request)
        {
            string requestParam = request.Form[FacebookQueryHelper.SignedRequestKey] ?? request.QueryString[FacebookQueryHelper.SignedRequestKey];
            if (requestParam != null)
            {
                return client.ParseSignedRequest(requestParam);
            }
            return null;
        }

        internal static IEnumerable<string> GetCurrentUserPermissions(this FacebookClient client)
        {
            FacebookGroupConnection<IDictionary<string, int>> permissionResults = client.Get<FacebookGroupConnection<IDictionary<string, int>>>("me/permissions");
            return ParsePermissions(permissionResults.Data);
        }

        private static IEnumerable<string> ParsePermissions(IEnumerable<IDictionary<string, int>> permissionResults)
        {
            if (permissionResults != null)
            {
                IDictionary<string, int> permissionResult = permissionResults.FirstOrDefault();
                if (permissionResult != null)
                {
                    return permissionResult.Where(kvp => kvp.Value == 1).Select(kvp => kvp.Key);
                }
            }

            return Enumerable.Empty<string>();
        }
    }
}
