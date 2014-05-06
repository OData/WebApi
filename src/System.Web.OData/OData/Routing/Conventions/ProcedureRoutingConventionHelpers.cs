// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.Controllers;
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

            foreach (KeyValuePair<string, string> nameAndValue in functionSegment.Values)
            {
                string name = nameAndValue.Key;
                object value = functionSegment.GetParameterValue(name);

                ODataEnumValue enumValue = value as ODataEnumValue;
                if (enumValue != null)
                {
                    // Remove the type name of the ODataEnumValue and keep the value.
                    value = enumValue.Value;
                }

                controllerContext.RouteData.Values.Add(name, value);
            }
        }
    }
}
