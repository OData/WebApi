// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Microsoft.AspNet.Mvc.Facebook.Attributes;
using Microsoft.AspNet.Mvc.Facebook.Services;

namespace Microsoft.AspNet.Mvc.Facebook.Models
{
    public class FacebookUserModelBinder : IModelBinder
    {
        private readonly string fields;
        private readonly IFacebookService facebookService;
        private readonly IFacebookUserStorageService facebookUserStorageService;

        public FacebookUserModelBinder()
            : this(DefaultFacebookService.Instance, FacebookSettings.DefaultUserStorageService)
        {
        }

        public FacebookUserModelBinder(string fields)
            : this()
        {
            this.fields = fields;
        }

        public FacebookUserModelBinder(IFacebookService facebookService, IFacebookUserStorageService facebookUserStorageService)
        {
            this.facebookService = facebookService;
            this.facebookUserStorageService = facebookUserStorageService;
        }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var client = facebookService.CreateClient();

            var requestParam = controllerContext.HttpContext.Request.Params["signed_request"];
            if (requestParam != null)
            {
                dynamic sr = client.ParseSignedRequest(FacebookSettings.AppSecret, requestParam);
                client.AccessToken = sr.oauth_token;

                return GetUser(bindingContext.ModelType, (string)sr.user_id, client);
            }

            return null;
        }

        private object GetUser(Type modelType, string facebookId, global::Facebook.FacebookClient client)
        {
            var user = facebookUserStorageService.GetUser(facebookId);

            //TODO: (ErikPo) Fill in the other half of this condition
            if (user == null/* or this is the first time the app has started */)
            {
                object userFields = client.Get("me" + (!String.IsNullOrEmpty(fields) ? "?fields=" + fields.Replace(" ", String.Empty) : facebookService.GetFields(modelType)));

                if (modelType == typeof(FacebookUser))
                {
                    return new FacebookUser { Data = userFields };
                }
                else if (modelType == typeof(object))
                {
                    return (dynamic)userFields;
                }

                user = (FacebookUser)Activator.CreateInstance(modelType);
                user.Data = userFields;

                var facebookFields = GetUserFields(modelType);
                PropertyInfo userProperty;
                string facebookFieldName;
                object fieldValue;
                foreach (var field in facebookFields)
                {
                    if (field.Value != null && field.Value.Ignore)
                    {
                        continue;
                    }
                    userProperty = field.Key;
                    facebookFieldName = field.Value != null ? field.Value.JsonField : String.Empty;
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

                facebookUserStorageService.AddUser(user);
            }

            return user;
        }

        private IDictionary<PropertyInfo, FacebookFieldAttribute> GetUserFields(Type userType)
        {
            var properties = userType.GetProperties();
            var fields = new Dictionary<PropertyInfo, FacebookFieldAttribute>(properties.Length);
            foreach (var property in properties)
            {
                var fbuf = property.GetCustomAttributes(typeof(FacebookFieldAttribute), true);
                if (fbuf != null && fbuf.Length > 0 && fbuf[0] is FacebookFieldAttribute)
                {
                    fields.Add(property, (FacebookFieldAttribute)fbuf[0]);
                }
                else
                {
                    fields.Add(property, null);
                }
            }
            return fields;
        }

        private object GetFBFieldValue(dynamic facebookObject, IEnumerable<string> fieldNameParts)
        {
            dynamic subFacebookObject;
            try
            {
                subFacebookObject = facebookObject[fieldNameParts.ElementAt(0).ToLowerInvariant()];
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
    }
}
