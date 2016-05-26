// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Http.Controllers;
using System.Web.OData.Extensions;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing.Conventions
{
    internal static class ProcedureRoutingConventionHelpers
    {
        public static string SelectAction(this IEdmOperation operation, ILookup<string, HttpActionDescriptor> actionMap, bool isCollection)
        {
            Contract.Assert(actionMap != null);

            if (operation == null)
            {
                return null;
            }

            // The binding parameter is the first parameter by convention
            IEdmOperationParameter bindingParameter = operation.Parameters.FirstOrDefault();
            if (operation.IsBound && bindingParameter != null)
            {
                IEdmEntityType entityType = null;
                if (!isCollection)
                {
                    entityType = bindingParameter.Type.Definition as IEdmEntityType;
                }
                else
                {
                    IEdmCollectionType bindingParameterType = bindingParameter.Type.Definition as IEdmCollectionType;
                    if (bindingParameterType != null)
                    {
                        entityType = bindingParameterType.ElementType.Definition as IEdmEntityType;
                    }
                }

                if (entityType == null)
                {
                    return null;
                }

                string targetActionName = isCollection
                    ? operation.Name + "OnCollectionOf" + entityType.Name
                    : operation.Name + "On" + entityType.Name;
                return actionMap.FindMatchingAction(targetActionName, operation.Name);
            }

            return null;
        }

        public static void AddKeyValueToRouteData(this HttpControllerContext controllerContext, KeyValuePathSegment keySegment, IEdmEntityType entityType, string keyPrefix)
        {
            Contract.Assert(controllerContext != null);
            Contract.Assert(keySegment != null);

            IDictionary<string, object> routingConventionsStore = controllerContext.Request.ODataProperties().RoutingConventionsStore;

            if (entityType == null && keySegment.Segment != null)
            {
                entityType = keySegment.Segment.EdmType as IEdmEntityType;
            }
            Contract.Assert(entityType != null);

            int keyCount;
            if (keySegment.Segment != null)
            {
                keyCount = keySegment.Segment.Keys.Count();
                foreach (var keyValue in keySegment.Segment.Keys)
                {
                    IEdmTypeReference keyType;
                    string newKeyName = GetKeyInfos(keyCount, keyValue.Key, entityType, keyPrefix, out keyType);

                    ODataPathSegmentExtensions.AddKeyValues(newKeyName, keyValue.Value, keyType,
                        controllerContext.RouteData.Values,
                        routingConventionsStore);
                }
            }
            else
            {
                IEdmModel model = controllerContext.Request.ODataProperties().Model;
                IDictionary<string, string> keyValues = keySegment.Values;
                keyCount = keyValues.Count;
                foreach (var keyValue in keyValues)
                {
                    IEdmTypeReference keyType;
                    string newKeyName = GetKeyInfos(keyCount, keyValue.Key, entityType, keyPrefix, out keyType);

                    object value = ODataUriUtils.ConvertFromUriLiteral(keyValue.Value, ODataVersion.V4, model, keyType);
                    Contract.Assert(value != null);

                    ODataPathSegmentExtensions.AddKeyValues(newKeyName, value, keyType, controllerContext.RouteData.Values,
                        routingConventionsStore);
                }
            }
        }

        private static string GetKeyInfos(int keyCount, string keyName, IEdmEntityType entityType, string keyPrefix, out IEdmTypeReference keyType)
        {
            Contract.Assert(keyName != null);
            Contract.Assert(entityType != null);

            string newKeyName;
            IEdmStructuralProperty keyProperty;

            if (String.IsNullOrEmpty(keyName))
            {
                Contract.Assert(keyCount == 1);
                keyProperty = entityType.Key().First();
                newKeyName = keyPrefix;
            }
            else
            {
                bool alternateKey = false;
                keyProperty = entityType.Key().FirstOrDefault(k => k.Name == keyName);
                if (keyProperty == null)
                {
                    // If it's alternate key.
                    keyProperty =
                        entityType.Properties().OfType<IEdmStructuralProperty>().FirstOrDefault(p => p.Name == keyName);
                    alternateKey = true;
                }
                Contract.Assert(keyProperty != null);

                // if there's only one key, just use the given prefix name, for example: "key, relatedKey"  
                // otherwise, to append the key name after the given prefix name.  
                // so for multiple keys, the parameter name is "keyId1, keyId2..."  
                // for navigation property, the parameter name is "relatedKeyId1, relatedKeyId2 ..."  
                // for alternate key, to append the alternate key name after the given prefix name.  
                if (alternateKey || keyCount > 1)
                {
                    newKeyName = keyPrefix + keyName;
                }
                else
                {
                    newKeyName = keyPrefix;
                }
            }

            keyType = keyProperty.Type;
            return newKeyName;
        }

        public static void AddFunctionParameterToRouteData(this HttpControllerContext controllerContext, BoundFunctionPathSegment functionSegment)
        {
            Contract.Assert(controllerContext != null);
            Contract.Assert(functionSegment != null);

            IDictionary<string, object> routingConventionsStore = controllerContext.Request.ODataProperties().RoutingConventionsStore;

            foreach (KeyValuePair<string, string> nameAndValue in functionSegment.Values)
            {
                string name = nameAndValue.Key;
                object value = functionSegment.GetParameterValue(name);

                AddFunctionParameters(functionSegment.Function, name, value, controllerContext.RouteData.Values,
                    routingConventionsStore, null);
            }
        }

        public static void AddFunctionParameters(IEdmFunction function, string paramName, object paramValue, 
            IDictionary<string, object> routeData, IDictionary<string, object> values, IDictionary<string, string> paramMapping)
        {
            Contract.Assert(function != null);

            // using the following codes to support [FromODataUriAttribute]
            IEdmOperationParameter edmParam = function.FindParameter(paramName);
            Contract.Assert(edmParam != null);
            ODataParameterValue parameterValue = new ODataParameterValue(paramValue, edmParam.Type);

            string name = paramName;
            if (paramMapping != null)
            {
                Contract.Assert(paramMapping.ContainsKey(paramName));
                name = paramMapping[paramName];
            }

            string prefixName = ODataParameterValue.ParameterValuePrefix + name;
            values[prefixName] = parameterValue;

            // using the following codes to support [FromUriAttribute]
            if (!routeData.ContainsKey(name))
            {
                routeData.Add(name, paramValue);
            }

            ODataNullValue nullValue = paramValue as ODataNullValue;
            if (nullValue != null)
            {
                routeData[name] = null;
            }

            ODataEnumValue enumValue = paramValue as ODataEnumValue;
            if (enumValue != null)
            {
                // Remove the type name of the ODataEnumValue and keep the value.
                routeData[name] = enumValue.Value;
            }
        }
    }
}
