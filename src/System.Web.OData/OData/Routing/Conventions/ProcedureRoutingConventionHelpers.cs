// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.OData.Extensions;
using Microsoft.OData.Core;
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

        public static void AddKeyValueToRouteData(this HttpControllerContext controllerContext, ODataPath odataPath)
        {
            Contract.Assert(controllerContext != null);
            Contract.Assert(odataPath != null);

            KeyValuePathSegment keyValueSegment = odataPath.Segments[1] as KeyValuePathSegment;
            if (keyValueSegment != null)
            {
                controllerContext.RouteData.Values[ODataRouteConstants.Key] = keyValueSegment.Value;
            }
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
