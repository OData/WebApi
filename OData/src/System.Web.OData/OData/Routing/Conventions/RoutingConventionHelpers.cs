// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Extensions;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing.Conventions
{
    internal static class RoutingConventionHelpers
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

        public static void AddKeyValueToRouteData(this HttpControllerContext controllerContext, KeySegment segment, string keyName = "key")
        {
            Contract.Assert(controllerContext != null);
            Contract.Assert(segment != null);

            IDictionary<string, object> routingConventionsStore = controllerContext.Request.ODataProperties().RoutingConventionsStore;

            foreach (var keyValuePair in segment.Keys)
            {
                object value = keyValuePair.Value;
                ConstantNode node = value as ConstantNode;
                if (node != null)
                {
                    ODataEnumValue enumValue = node.Value as ODataEnumValue;
                    if (enumValue != null)
                    {
                        // For FromODataUri
                        string prefixName = ODataParameterValue.ParameterValuePrefix + keyValuePair.Key;
                        routingConventionsStore[prefixName] = enumValue;

                        // for without FromODataUri
                        value = ODataUriUtils.ConvertToUriLiteral(enumValue, ODataVersion.V4);
                    }
                }

                if (segment.Keys.Count() == 1)
                {
                    controllerContext.RouteData.Values[keyName] = value;
                }
                else
                {
                    // TODO: maybe to use the low camel case
                    controllerContext.RouteData.Values[keyValuePair.Key] = value;
                }
            }
        }

        public static void AddFunctionParameterToRouteData(this HttpControllerContext controllerContext, OperationSegment functionSegment)
        {
            Contract.Assert(controllerContext != null);
            Contract.Assert(functionSegment != null);

            IDictionary<string, object> routingConventionsStore = controllerContext.Request.ODataProperties().RoutingConventionsStore;

            IEdmFunction function = functionSegment.Operations.First() as IEdmFunction;

            foreach (OperationSegmentParameter parameter in functionSegment.Parameters)
            {
                string name = parameter.Name;
                object value = functionSegment.GetParameterValue(name);

                AddFunctionParameters(function, name, value, controllerContext.RouteData.Values,
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
        /*
        public static object TranslateNode(object node)
        {
            Contract.Assert(node != null);

            ConstantNode constantNode = node as ConstantNode;
            if (constantNode != null)
            {
                UriTemplateExpression uriTemplateExpression = constantNode.Value as UriTemplateExpression;
                if (uriTemplateExpression != null)
                {
                    return uriTemplateExpression.LiteralText;
                }

                // Make the enum prefix free to work.
                ODataEnumValue enumValue = constantNode.Value as ODataEnumValue;
                if (enumValue != null)
                {
                    return ODataUriUtils.ConvertToUriLiteral(enumValue, ODataVersion.V4);
                }

                return constantNode.LiteralText;
            }

            ConvertNode convertNode = node as ConvertNode;
            if (convertNode != null)
            {
                return TranslateNode(convertNode.Source);
            }

            ParameterAliasNode parameterAliasNode = node as ParameterAliasNode;
            if (parameterAliasNode != null)
            {
                return parameterAliasNode.Alias;
            }

            //return node.ToString();
            throw Error.NotSupported(SRResources.CannotRecognizeNodeType, typeof(ODataPathSegmentHandler),
                node.GetType().FullName);
        }*/

        public static bool IsRouteParameter(string parameterName)
        {
            return parameterName.StartsWith("{", StringComparison.Ordinal) &&
                    parameterName.EndsWith("}", StringComparison.Ordinal);
        }
    }
}
