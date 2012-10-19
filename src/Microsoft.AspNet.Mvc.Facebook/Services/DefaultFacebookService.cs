// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using Facebook;
using Microsoft.AspNet.Mvc.Facebook.Attributes;
using Microsoft.AspNet.Mvc.Facebook.Models;

namespace Microsoft.AspNet.Mvc.Facebook.Services
{
    public class DefaultFacebookService : IFacebookService
    {
        private readonly IFacebookUserStorageService facebookUserStorageService;
        private static DefaultFacebookService instance = new DefaultFacebookService();
        private string verificationToken;
        private static bool isRealtimeInitialized;
        private static string _accessTokenUrl = "https://graph.facebook.com/oauth/access_token?client_id={0}&client_secret={1}&grant_type=client_credentials";

        public DefaultFacebookService()
            : this(FacebookSettings.DefaultUserStorageService)
        {
        }

        public DefaultFacebookService(IFacebookUserStorageService facebookUserStorageService)
        {
            this.facebookUserStorageService = facebookUserStorageService;
        }

        public static DefaultFacebookService Instance
        {
            get
            {
                return instance;
            }
        }

        public IFacebookUserStorageService Storage
        {
            get
            {
                return facebookUserStorageService;
            }
        }

        public string VerificationToken
        {
            get
            {
                if (verificationToken == null)
                {
                    verificationToken = Guid.NewGuid().ToString("N");
                }

                return verificationToken;
            }
            set
            {
                verificationToken = value;
            }
        }

        //TODO: (ErikPo) Cache this for a while
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Should be auto-disposed")]
        public string GetAppAccessToken()
        {
            var client = new WebClient();
            var value = client.DownloadString(String.Format(_accessTokenUrl, FacebookSettings.AppId, FacebookSettings.AppSecret));
            if (value.StartsWith("access_token="))
            {
                return value.Substring("access_token=".Length);
            }
            return null;
        }

        private TUser RefreshUser<TUser>(TUser user, string accessToken) where TUser : FacebookUser, new()
        {
            var isNewUser = false;
            var changedUser = false;
            var userFields = GetActualFields(typeof(TUser)).Select(uf => uf.Value.FieldName).Distinct();

            if (userFields.Count() == 1 && userFields.ElementAt(0) == "id")
            {
                return user;
            }

            var userFieldsQuery = userFields.Count() > 0 ? "?fields=" + String.Join(",", userFields) : String.Empty;
            var client = CreateClient(accessToken);

            if (user == null)
            {
                user = new TUser();
                isNewUser = true;
            }

            if (isNewUser)
            {
                dynamic me = client.Get("me" + userFieldsQuery);
                RefreshUserFields(user, me, userFields.ToArray());
                changedUser = true;
            }

            //TODO: (ErikPo) Move this out of here and make it more generic
            var types = GetFBFieldTypes(((TUser)user).GetType());
            if (types.Count > 0)
            {
                foreach (var type in types)
                {
                    switch (type.Value.TypeName)
                    {
                        case "friends":
                            var changedFriends = LoadFriends<TUser>(client, user, type.Key, type.Value, userFields, userFieldsQuery);
                            if (changedFriends)
                            {
                                changedUser = true;
                            }
                            break;
                    }
                }
            }

            if (changedUser)
            {
                if (isNewUser)
                {
                    facebookUserStorageService.AddUser(user);
                }
                else
                {
                    facebookUserStorageService.UpdateUser(user);
                }
            }

            return user;
        }

        private IDictionary<PropertyInfo, FacebookObjectAttribute> GetFBFieldTypes(Type type)
        {
            var fields = new Dictionary<PropertyInfo, FacebookObjectAttribute>();
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var fbt = property.GetCustomAttributes(typeof(FacebookObjectAttribute), true);
                if (fbt != null && fbt.Length > 0 && fbt[0] is FacebookObjectAttribute)
                {
                    fields.Add(property, (FacebookObjectAttribute)fbt[0]);
                }
            }
            return fields;
        }

        private bool LoadFriends<TUser>(FacebookClient client, TUser user, PropertyInfo propertyInfo, FacebookObjectAttribute objectType, IEnumerable<string> userFields, string userFieldsQuery) where TUser : FacebookUser, new()
        {
            //TODO: (ErikPo) The following use of reflection won't work properly with EF. Switch to using static Attribute.GetCustomAttribute instead
            var lastUpdated = user.GetType().GetProperties().FirstOrDefault(pi => pi.PropertyType == typeof(DateTime?) && pi.Name == "FriendsLastUpdated");
            if (lastUpdated != null)
            {
                var lastUpdatedValue = (DateTime?)lastUpdated.GetValue(user, null);
                //TODO: (ErikPo) Decide if this should be configurable
                if (lastUpdatedValue.HasValue && lastUpdatedValue.Value.AddHours(1) < DateTime.UtcNow)
                {
                    return false;
                }
            }

            var friends = (List<TUser>)propertyInfo.GetValue(user, null);
            if (friends == null)
            {
                friends = new List<TUser>();
            }

            dynamic fl = client.Get("me/friends" + userFieldsQuery);

            if (friends.Count == 0)
            {
                foreach (var friend in fl.data)
                {
                    string friendId = friend.id;
                    var f = new TUser() { FacebookId = friendId };
                    RefreshUserFields(f, friend, userFields.ToArray());
                    friends.Add(f);
                }
            }
            else
            {
                // add, edit
                foreach (var friend in fl.data)
                {
                    string friendId = friend.id;
                    var f = friends.FirstOrDefault(frnd => frnd.FacebookId == friendId);
                    if (f == null)
                    {
                        f = new TUser() { FacebookId = friendId };
                        friends.Add(f);
                    }
                    RefreshUserFields(f, friend, userFields.ToArray());
                }

                // remove
                var removeCount = 0;
                //TODO: (ErikPo) Verify that I'm removing things from the list of friends properly
                for (var i = 0; i < (friends.Count - removeCount); i++)
                {
                    var friend = friends[i - removeCount];
                    //TODO: (ErikPo) Find out if you can use LINQ against dynamic enumerables or not and get rid of below code if possible
                    var foundFriend = false;
                    foreach (var newFriend in fl.data)
                    {
                        if (friend.FacebookId == newFriend.id)
                        {
                            foundFriend = true;
                            break;
                        }
                    }
                    if (foundFriend)
                    {
                        friends.RemoveAt(i - removeCount);
                        i--;
                        removeCount++;
                    }
                }
            }

            propertyInfo.SetValue(user, friends, null);

            lastUpdated.SetValue(user, DateTime.UtcNow, null);

            return true;
        }

        public TUser LoadUser<TUser>(HttpContextBase httpContext) where TUser : FacebookUser, new()
        {
            dynamic signedRequest = GetSignedRequest(httpContext);
            if (signedRequest != null)
            {
                string userId = signedRequest.user_id;
                if (userId == null)
                {
                    return null;
                }

                var user = (TUser)facebookUserStorageService.GetUser(userId);
                if (user == null)
                {
                    user = new TUser() { FacebookId = userId };
                }
                return RefreshUser(user, signedRequest.oauth_token);
            }

            throw new ApplicationException("Invalid request. No signed_request parameter was found on the request.");
        }

        public FacebookAuthorizationInfo Authorize(HttpContextBase httpContext)
        {
            bool isAuthorized = false;
            string accessToken = null;
            dynamic signedRequest = GetSignedRequest(httpContext);
            if (signedRequest == null)
            {
                return new FacebookAuthorizationInfo()
                {
                    IsAuthorized = false
                };
            }

            accessToken = signedRequest.oauth_token;
            if (!String.IsNullOrWhiteSpace(accessToken))
            {
                isAuthorized = true;
            }
            return new FacebookAuthorizationInfo
            {
                IsAuthorized = isAuthorized,
                AccessToken = accessToken,
            };
        }

        public FacebookAuthorizationInfo Authorize(HttpContextBase httpContext, string[] permissions)
        {
            dynamic signedRequest = GetSignedRequest(httpContext);
            if (signedRequest == null)
            {
                return new FacebookAuthorizationInfo()
                {
                    IsAuthorized = false
                };
            }

            bool isAuthorized = false;
            string accessToken = signedRequest.oauth_token;
            string facebookId = signedRequest.user_id;
            string[] currentPermissions = new string[0];

            if (!String.IsNullOrWhiteSpace(accessToken) && !String.IsNullOrWhiteSpace(facebookId))
            {
                // Get permissions from storage service and check if user has all required permissions
                currentPermissions = this.facebookUserStorageService.GetPermissions(facebookId);
                isAuthorized = HasRequiredPermissions(currentPermissions, permissions);

                // If stored permissions don't authorize the user then call the
                // Graph API for most current permissions and check authorization.
                if (!isAuthorized)
                {
                    try
                    {
                        var fb = this.CreateClient(accessToken: accessToken);
                        var permsResult = fb.Get("me/permissions") as IDictionary<string, object>;
                        if (permsResult != null && permsResult.ContainsKey("data"))
                        {
                            var data = permsResult["data"] as IList<object>;

                            if (data != null && data.Count > 0)
                            {
                                var permsData = data[0] as IDictionary<string, object>;
                                if (permsData == null)
                                {
                                    currentPermissions = new string[0];
                                }
                                else
                                {
                                    currentPermissions = (from perm in permsData
                                                          where perm.Value.ToString() == "1"
                                                          select perm.Key).ToArray();
                                }
                            }
                        }

                        // Since the permissions have been retrieved from Graph API request
                        // update the storage service with the most current permissions.
                        this.facebookUserStorageService.SetPermissions(facebookId, currentPermissions);
                    }
                    catch (FacebookOAuthException)
                    {
                        // The OAuth token is no longer valid.
                        isAuthorized = false;
                    }

                    isAuthorized = HasRequiredPermissions(currentPermissions, permissions);
                }
            }
            return new FacebookAuthorizationInfo
            {
                IsAuthorized = isAuthorized,
                AccessToken = accessToken,
                FacebookId = facebookId,
                Permissions = currentPermissions,
            };
        }

        public void RefreshUserFields(FacebookUser user, params string[] fields)
        {
            var client = CreateClient();
            dynamic facebookUserFields = client.Get(user.FacebookId + (fields != null && fields.Length > 0 ? "?fields=" + String.Join(",", fields) : String.Empty));

            RefreshUserFields(user, facebookUserFields, fields);
#if Debug
            Utilities.Log(facebookUserFields.ToString());
#endif
        }

        public void RefreshUserFields(FacebookUser user, dynamic userFields, params string[] fields)
        {
            var facebookFields = GetActualFields(user.GetType());
            PropertyInfo userProperty;
            string facebookFieldName;
            object fieldValue;
            foreach (var facebookField in facebookFields)
            {
                userProperty = facebookField.Key;
                facebookFieldName = facebookField.Value != null ? facebookField.Value.JsonField : String.Empty;
                if (!String.IsNullOrEmpty(facebookFieldName))
                {
                    fieldValue = GetFBFieldValue(userFields, facebookFieldName.Split('.'));
                }
                else
                {
                    fieldValue = GetFBFieldValue(userFields, new[] { userProperty.Name });
                }
                if (fieldValue != null)
                {
                    userProperty.SetValue(user, fieldValue, null);
                }
            }
        }

        public string GetFields(Type modelType)
        {
            if (modelType != typeof(FacebookObject) && modelType != typeof(FacebookUser) && modelType != typeof(object))
            {
                var facebookFields = GetActualFields(modelType);
                var userFields = new List<string>();
                foreach (var facebookField in facebookFields)
                {
                    if (facebookField.Value == null)
                    {
                        userFields.Add(facebookField.Key.Name.ToLowerInvariant());
                    }
                    else if (!facebookField.Value.Ignore && !String.IsNullOrEmpty(facebookField.Value.FieldName))
                    {
                        userFields.Add(facebookField.Value.FieldName);
                    }
                }
                return "?fields=" + String.Join(",", userFields);
            }

            return String.Empty;
        }

        private IDictionary<PropertyInfo, FacebookFieldAttribute> GetActualFields(Type modelType)
        {
            var properties = GetActualModelType(modelType).GetProperties();
            var facebookFields = new Dictionary<PropertyInfo, FacebookFieldAttribute>(properties.Length);
            foreach (var property in properties)
            {
                var fbuf = property.GetCustomAttributes(typeof(FacebookFieldAttribute), true);
                if (fbuf != null && fbuf.Length > 0 && fbuf[0] is FacebookFieldAttribute)
                {
                    facebookFields.Add(property, (FacebookFieldAttribute)fbuf[0]);
                }
                else
                {
                    facebookFields.Add(property, null);
                }
            }
            return facebookFields;
        }

        private Type GetActualModelType(Type modelType)
        {
            var genericArguments = modelType.GetGenericArguments();
            if (genericArguments.Length > 0)
            {
                return genericArguments[0];
            }

            return modelType;
        }

        private object GetFBFieldValue(dynamic facebookObject, IEnumerable<string> fieldNameParts)
        {
            dynamic subFacebookObject;
            try
            {
                subFacebookObject = facebookObject[fieldNameParts.ElementAt(0)];
            }
            catch
            {
                subFacebookObject = null;
            }
            if (subFacebookObject == null)
            {
                return null;
            }
            if (fieldNameParts.Count() == 1)
            {
                return subFacebookObject;
            }
            return GetFBFieldValue(subFacebookObject, fieldNameParts.Skip(1));
        }

        public FacebookClient CreateClient(string accessToken = null)
        {
            //TODO: (ErikPo) Make sure JSON.NET's serializer is used by default for consistency w/ Web API
            //global::Facebook.FacebookClient.SetDefaultJsonSerializers()

            var client = new FacebookClient();
            client.AppId = FacebookSettings.AppId;
            if (!String.IsNullOrEmpty(accessToken))
            {
                client.AccessToken = accessToken;
            }
            return client;
        }

        public string GetRealtimeFields(Type userType)
        {
            if (userType == null || userType == typeof(FacebookUser) || userType == typeof(object))
            {
                return String.Empty;
            }

            var fields = new List<string>();
            var properties = userType.GetProperties();
            foreach (var property in properties)
            {
                var fbf = property.GetCustomAttributes(typeof(FacebookFieldAttribute), true);
                var fbt = property.GetCustomAttributes(typeof(FacebookObjectAttribute), true);
                if (fbf != null && fbf.Length > 0 && fbf[0] is FacebookFieldAttribute)
                {
                    var field = ((FacebookFieldAttribute)fbf[0]);
                    if (!(field.Ignore || String.Equals(field.FieldName, "id", StringComparison.OrdinalIgnoreCase)))
                    {
                        fields.Add(field.FieldName);
                    }
                }
                else if (fbt != null && fbt.Length > 0 && fbt[0] is FacebookObjectAttribute)
                {
                    fields.Add(((FacebookObjectAttribute)fbt[0]).TypeName);
                }
                else
                {
                    if (!String.Equals(property.Name, "id", StringComparison.OrdinalIgnoreCase))
                    {
                        fields.Add(property.Name.ToLowerInvariant());
                    }
                }
            }

            return String.Join(",", fields);
        }

        private object GetSignedRequest(HttpContextBase httpContext)
        {
            var requestParam = httpContext.Request.Params["signed_request"];
            if (requestParam != null)
            {
                var client = CreateClient();
                object signedRequest;
                if (client.TryParseSignedRequest(FacebookSettings.AppSecret, requestParam, out signedRequest))
                {
                    return signedRequest;
                }
                throw new InvalidOperationException("Invalid signed request. Did you set the FacebookSettings.AppId and FacebookSettings.AppSecret?");
            }
            return null;
        }

        //TODO: (ErikPo) Make this async
        public void InitializeRealtime(Type userType = null, string callbackUrl = null)
        {
            if (!isRealtimeInitialized)
            {
                if (String.IsNullOrEmpty(callbackUrl))
                {
                    callbackUrl = FacebookSettings.RealtimeCallbackUrl;
                }

                if (callbackUrl.ToLowerInvariant().Contains("//localhost/") || callbackUrl.ToLowerInvariant().Contains("//localhost:"))
                {
                    isRealtimeInitialized = true;
                    return;
                }

                //TODO: (ErikPo) Replace all of this with something cleaner (HttpClient?) and make it async
                var appRealtimeFields = GetRealtimeFields(userType);
                var request = WebRequest.Create(String.Format("https://graph.facebook.com/{0}/subscriptions?access_token={1}", FacebookSettings.AppId, GetAppAccessToken()));
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                using (var requestStream = request.GetRequestStream())
                {
                    var contentValue = "object=user" + (!String.IsNullOrEmpty(appRealtimeFields) ? "&fields=" + HttpUtility.UrlEncode(appRealtimeFields) : String.Empty) + "&callback_url=" + HttpUtility.UrlEncode(callbackUrl) + "&verify_token=" + VerificationToken;
#if Debug
                    Utilities.Log(contentValue);
#endif
                    var content = System.Text.Encoding.UTF8.GetBytes(contentValue);
                    requestStream.Write(content, 0, content.Length);
                }

                try
                {
                    var response = request.GetResponse();
                    isRealtimeInitialized = true;
                }
                catch
                {
                    //TODO: decide what to do with the exception
                }
            }
        }

        private bool HasRequiredPermissions(string[] currentPermissions, string[] requiredPermissions)
        {
            for (int i = 0; i < requiredPermissions.Length; i++)
            {
                var hasPermission = currentPermissions.Contains(requiredPermissions[i]);
                if (!hasPermission)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
