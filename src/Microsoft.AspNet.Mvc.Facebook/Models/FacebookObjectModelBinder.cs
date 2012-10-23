// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Microsoft.AspNet.Mvc.Facebook.Attributes;
using Microsoft.AspNet.Mvc.Facebook.Services;

namespace Microsoft.AspNet.Mvc.Facebook.Models
{
    public class FacebookObjectModelBinder : IModelBinder
    {
        private readonly IFacebookService facebookService;
        private readonly IFacebookUserStorageService facebookUserStorageService;
        private readonly IFacebookObjectStorageService facebookObjectStorageService;

        public FacebookObjectModelBinder()
            : this(DefaultFacebookService.Instance, FacebookSettings.DefaultUserStorageService, FacebookSettings.DefaultObjectStorageService)
        {
        }

        public FacebookObjectModelBinder(IFacebookService facebookService, IFacebookUserStorageService facebookUserStorageService, IFacebookObjectStorageService facebookObjectStorageService)
        {
            this.facebookService = facebookService;
            this.facebookUserStorageService = facebookUserStorageService;
            this.facebookObjectStorageService = facebookObjectStorageService;
        }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var client = facebookService.CreateClient();

            var requestParam = controllerContext.HttpContext.Request.Params["signed_request"];
            if (requestParam != null)
            {
                dynamic sr = client.ParseSignedRequest(FacebookSettings.AppSecret, requestParam);
                client.AccessToken = sr.oauth_token;

                return GetObjects(bindingContext.ModelType, (string)sr.user_id, client);
            }

            return null;
        }

        private object GetObjects(Type modelType, string userFacebookId, global::Facebook.FacebookClient client)
        {
            var typeName = GetTypeName(modelType);
            if (String.IsNullOrEmpty(typeName))
            {
                return null;
            }

            var objects = facebookObjectStorageService.GetObjects(userFacebookId);

            if (objects == null || objects.Count == 0/* || Some other time period has been met and we should sync again */)
            {
                return LoadObjects(client, modelType, userFacebookId, "me/" + typeName + facebookService.GetFields(modelType));
            }

            var castedObjects = (IList)Activator.CreateInstance(modelType);
            foreach (var obj in objects)
            {
                castedObjects.Add(obj);
            }
            return castedObjects;
        }

        private string GetTypeName(Type modelType)
        {
            if (modelType.IsGenericType)
            {
                var genericParameters = modelType.GetGenericArguments();
                if (genericParameters.Length > 0)
                {
                    var objectAttributes = (FacebookObjectAttribute[])genericParameters[0].GetCustomAttributes(typeof(FacebookObjectAttribute), true);
                    if (objectAttributes.Length > 0)
                    {
                        return objectAttributes[0].TypeName;
                    }
                }
            }
            return null;
        }

        private Type GetGenericType(Type modelType)
        {
            if (modelType.IsGenericType)
            {
                var genericArguments = modelType.GetGenericArguments();
                if (genericArguments.Length > 0)
                {
                    return genericArguments[0];
                }
            }
            return null;
        }

        private object LoadObjects(global::Facebook.FacebookClient client, Type modelType, string userFacebookId, string query)
        {
            dynamic objects = Activator.CreateInstance(modelType);
            dynamic objectList = client.Get(query);
            var genericType = GetGenericType(modelType);

            if (objects.Count == 0)
            {
                foreach (var obj in objectList.data)
                {
                    dynamic o = Activator.CreateInstance(genericType);
                    o.FacebookId = obj.Id;
                    o.FacebookUserId = userFacebookId;
                    o.Data = obj;
                    ApplyFields(o, obj);
                    objects.Add(o);
                    facebookObjectStorageService.AddObject((FacebookObject)o);
                }
            }
            else
            {
                // add, edit
                foreach (var obj in objectList.data)
                {
                    dynamic o = FindById(objects, obj.id);
                    if (o == null)
                    {
                        o = Activator.CreateInstance(genericType);
                        o.FacebookId = obj.Id;
                        o.FacebookUserId = userFacebookId;
                        o.Data = obj;
                        ApplyFields(o, obj);
                        objects.Add(o);
                        facebookObjectStorageService.AddObject((FacebookObject)o);
                        continue;
                    }
                    ApplyFields(o, obj);
                    facebookObjectStorageService.UpdateObject((FacebookObject)o);
                }

                // remove
                var removeCount = 0;
                for (var i = 0; i < (objects.Count - removeCount); i++)
                {
                    var obj = objects[i - removeCount];
                    var foundObject = false;
                    foreach (var newObject in objectList.data)
                    {
                        if (obj.FacebookId == newObject.id)
                        {
                            foundObject = true;
                            break;
                        }
                    }
                    if (foundObject)
                    {
                        objects.RemoveAt(i - removeCount);
                        i--;
                        removeCount++;
                    }
                }
            }

            return objects;
        }

        private dynamic FindById(dynamic obj, string id)
        {
            foreach (var o in obj)
            {
                if (o.FacebookId == id)
                {
                    return o;
                }
            }
            return null;
        }

        private void ApplyFields(FacebookObject obj, dynamic values)
        {
            var facebookFields = GetObjectFields(obj.GetType());
            PropertyInfo userProperty;
            string fieldName;
            object fieldValue;
            foreach (var field in facebookFields)
            {
                if (field.Value != null && field.Value.Ignore)
                {
                    continue;
                }
                userProperty = field.Key;
                fieldName = field.Value != null ? field.Value.JsonField : String.Empty;
                if (!String.IsNullOrEmpty(fieldName))
                {
                    fieldValue = GetFacebookFieldValue(values, fieldName.Split('.'));
                }
                else
                {
                    fieldValue = GetFacebookFieldValue(values, new[] { userProperty.Name });
                }
                if (fieldValue != null)
                {
                    userProperty.SetValue(obj, fieldValue, null);
                }
            }
        }

        private IDictionary<PropertyInfo, FacebookFieldAttribute> GetObjectFields(Type objectType)
        {
            var properties = objectType.GetProperties();
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

        private object GetFacebookFieldValue(dynamic facebookObject, IEnumerable<string> fieldNameParts)
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
            return GetFacebookFieldValue(subFacebookObject, fieldNameParts.Skip(1));
        }
    }
}
