// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    internal static class RoutingConventionHelpers
    {
        public static string SelectAction(this IEdmOperation operation, IWebApiActionMap actionMap, bool isCollection)
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

        public static bool TryMatch(
            IDictionary<string, string> templateParameters,
            IDictionary<string, object> parameters,
            IDictionary<string, object> matches)
        {
            Contract.Assert(templateParameters != null);
            Contract.Assert(parameters != null);
            Contract.Assert(matches != null);

            if (templateParameters.Count != parameters.Count)
            {
                return false;
            }

            Dictionary<string, object> routeData = new Dictionary<string, object>();
            foreach (KeyValuePair<string, string> templateParameter in templateParameters)
            {
                string nameInSegment = templateParameter.Key;

                object value;
                if (!parameters.TryGetValue(nameInSegment, out value))
                {
                    // parameter not found. not a match.
                    return false;
                }

                string nameInRouteData = templateParameter.Value;
                routeData.Add(nameInRouteData, value);
            }

            foreach (KeyValuePair<string, object> kvp in routeData)
            {
                matches[kvp.Key] = kvp.Value;
            }

            return true;
        }

        public static bool TryMatch(this KeySegment keySegment, IDictionary<string, string> mapping, IDictionary<string, object> values)
        {
            Contract.Assert(keySegment != null);
            Contract.Assert(mapping != null);
            Contract.Assert(values != null);

            if (keySegment.Keys.Count() != mapping.Count)
            {
                return false;
            }

            IEdmEntityType entityType = keySegment.EdmType as IEdmEntityType;

            Dictionary<string, object> routeData = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> key in keySegment.Keys)
            {
                string mappingName;
                if (!mapping.TryGetValue(key.Key, out mappingName))
                {
                    // mapping name is not found. it's not a match.
                    return false;
                }

                IEdmTypeReference typeReference;
                // get the key property from the entity type
                if (entityType != null)
                {
                    IEdmStructuralProperty keyProperty = entityType.Key().FirstOrDefault(k => k.Name == key.Key);
                    if (keyProperty == null)
                    {
                        // If it's an alternate key
                        keyProperty = entityType.Properties().OfType<IEdmStructuralProperty>().FirstOrDefault(p => p.Name == key.Key);
                    }

                    Contract.Assert(keyProperty != null);
                    typeReference = keyProperty.Type;
                }
                else
                {
                    typeReference = EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(key.Value.GetType());
                }

                AddKeyValues(mappingName, key.Value, typeReference, routeData, routeData);
            }

            foreach (KeyValuePair<string, object> kvp in routeData)
            {
                values[kvp.Key] = kvp.Value;
            }

            return true;
        }

        public static void AddKeyValueToRouteData(this IWebApiControllerContext controllerContext, KeySegment segment, string keyName = "key")
        {
            Contract.Assert(controllerContext != null);
            Contract.Assert(segment != null);

            IDictionary<string, object> routingConventionsStore = controllerContext.Request.Context.RoutingConventionsStore;

            IEdmEntityType entityType = segment.EdmType as IEdmEntityType;
            Contract.Assert(entityType != null);

            int keyCount = segment.Keys.Count();
            foreach (var keyValuePair in segment.Keys)
            {
                bool alternateKey = false; 
                // get the key property from the entity type
                IEdmStructuralProperty keyProperty = entityType.Key().FirstOrDefault(k => k.Name == keyValuePair.Key);
                if (keyProperty == null)
                {
                    // If it's alternate key.
                    keyProperty = entityType.Properties().OfType<IEdmStructuralProperty>().FirstOrDefault(p => p.Name == keyValuePair.Key);
                    alternateKey = true;
                }
                Contract.Assert(keyProperty != null);

                // if there's only one key, provide two paramters, one using the given key name, e.g., "key, relatedKey"
                // and the other appending the property name to the given key name: "keyId, relatedKeyId"
                // in other cases, just append the property names to the given key name
                // so for multiple keys, the parameter name is "keyId1, keyId2..."
                // for navigation property, the parameter name is "relatedKeyId1, relatedKeyId2 ..."
                if (alternateKey || keyCount > 1)
                {
                    var newKeyName = keyName + keyValuePair.Key;
                    AddKeyValues(newKeyName, keyValuePair.Value, keyProperty.Type, controllerContext.RouteData, routingConventionsStore);
                }
                else
                {
                    AddKeyValues(keyName, keyValuePair.Value, keyProperty.Type, controllerContext.RouteData, routingConventionsStore);
                    if (keyCount == 1)
                    {
                        var anotherKeyName = keyName + keyValuePair.Key;
                        AddKeyValues(anotherKeyName, keyValuePair.Value, keyProperty.Type, controllerContext.RouteData, routingConventionsStore);
                    }
                }

                IncrementKeyCount(controllerContext.RouteData);
            }
        }

        private static void AddKeyValues(string name, object value, IEdmTypeReference edmTypeReference, IDictionary<string, object> routeValues,
            IDictionary<string, object> odataValues)
        {
            Contract.Assert(routeValues != null);
            Contract.Assert(odataValues != null);

            object routeValue = null;
            object odataValue = null;
            ConstantNode node = value as ConstantNode;
            if (node != null)
            {
                ODataEnumValue enumValue = node.Value as ODataEnumValue;
                if (enumValue != null)
                {
                    odataValue = new ODataParameterValue(enumValue, edmTypeReference);
                    routeValue = enumValue.Value;
                }
            }
            else
            {
                odataValue = new ODataParameterValue(value, edmTypeReference);
                routeValue = value;
            }

            // for without FromODataUri
            routeValues[name] = routeValue;

            // For FromODataUri
            string prefixName = ODataParameterValue.ParameterValuePrefix + name;
            odataValues[prefixName] = odataValue;
        }

        private static void IncrementKeyCount(IDictionary<string, object> routeValues)
        {
            if (routeValues.TryGetValue(ODataRouteConstants.KeyCountKey, out object count))
            {
                routeValues[ODataRouteConstants.KeyCountKey] = ((int)count) + 1;
            }
            else
            {
                routeValues[ODataRouteConstants.KeyCountKey] = 1;
            }
        }

        public static void AddFunctionParameterToRouteData(this IWebApiControllerContext controllerContext, OperationSegment functionSegment)
        {
            Contract.Assert(controllerContext != null);
            Contract.Assert(functionSegment != null);

            IDictionary<string, object> routingConventionsStore = controllerContext.Request.Context.RoutingConventionsStore;

            IEdmFunction function = functionSegment.Operations.First() as IEdmFunction;
            if (function == null)
            {
                return;
            }

            foreach (OperationSegmentParameter parameter in functionSegment.Parameters)
            {
                string name = parameter.Name;
                object value = functionSegment.GetParameterValue(name);

                AddFunctionParameters(function, name, value, controllerContext.RouteData,
                    routingConventionsStore, null);
            }

            // Append the optional parameters into RouteData.
            ODataOptionalParameter optional = new ODataOptionalParameter();
            foreach (var optionalParameter in function.Parameters.OfType<IEdmOptionalParameter>())
            {
                if (!functionSegment.Parameters.Any(c => c.Name == optionalParameter.Name))
                {
                    optional.Add(optionalParameter);
                }
            }

            if (optional.OptionalParameters.Any())
            {
                controllerContext.RouteData.Add(ODataRouteConstants.OptionalParameters, optional);
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

        public static IDictionary<string, string> BuildParameterMappings(
            IEnumerable<OperationSegmentParameter> parameters, string segment)
        {
            Contract.Assert(parameters != null);

            Dictionary<string, string> parameterMappings = new Dictionary<string, string>();

            foreach (OperationSegmentParameter parameter in parameters)
            {
                string parameterName = parameter.Name;
                string nameInRouteData = null;

                ConstantNode node = parameter.Value as ConstantNode;
                if (node != null)
                {
                    UriTemplateExpression uriTemplateExpression = node.Value as UriTemplateExpression;
                    if (uriTemplateExpression != null)
                    {
                        nameInRouteData = uriTemplateExpression.LiteralText.Trim();
                    }
                }
                else
                {
                    // Just for easy constructor the function parameters
                    nameInRouteData = parameter.Value as string;
                }

                if (nameInRouteData == null || !IsRouteParameter(nameInRouteData))
                {
                    throw new ODataException(
                        Error.Format(SRResources.ParameterAliasMustBeInCurlyBraces, parameter.Value, segment));
                }

                nameInRouteData = nameInRouteData.Substring(1, nameInRouteData.Length - 2);
                if (String.IsNullOrEmpty(nameInRouteData))
                {
                    throw new ODataException(
                            Error.Format(SRResources.EmptyParameterAlias, parameter.Value, segment));
                }

                parameterMappings[parameterName] = nameInRouteData;
            }

            return parameterMappings;
        }

        public static bool IsRouteParameter(string parameterName)
        {
            return parameterName.StartsWith("{", StringComparison.Ordinal) &&
                    parameterName.EndsWith("}", StringComparison.Ordinal);
        }
    }
}
