// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Routing
{
    internal static class ProcedureHelpers
    {
        public static IEdmFunctionImport FindAction(this IEdmEntityContainer container,
            string actionIdentifier, IEdmType bindingParameterType)
        {
            Contract.Assert(container != null);
            Contract.Assert(actionIdentifier != null);

            IEnumerable<IEdmFunctionImport> matches =
                container.FunctionImports()
                .GetMatchingProcedures(actionIdentifier, bindingParameterType, isSideEffecting: true);

            if (bindingParameterType != null)
            {
                // bindable function.
                bool isCollection = false;
                if (bindingParameterType.TypeKind == EdmTypeKind.Collection)
                {
                    bindingParameterType = ((IEdmCollectionType)bindingParameterType).ElementType.Definition;
                    isCollection = true;
                }

                return FindBest(actionIdentifier, matches, (IEdmEntityType)bindingParameterType, isCollection);
            }
            else
            {
                IEdmFunctionImport[] matchesArray = matches.ToArray();
                if (matchesArray.Length > 1)
                {
                    string message = String.Join(", ", matchesArray.Select(match => match.Container.FullName() + "." + match.Name));
                    throw Error.Argument("actionIdentifier", SRResources.ActionResolutionFailed, actionIdentifier, message);
                }
                else if (matchesArray.Length == 1)
                {
                    return matchesArray[0];
                }
                else
                {
                    return null;
                }
            }
        }

        public static IEnumerable<IEdmFunctionImport> FindFunctions(this IEdmEntityContainer container,
            string functionIdentifier, IEdmType bindingParameterType)
        {
            Contract.Assert(container != null);
            Contract.Assert(functionIdentifier != null);

            return container.FunctionImports()
                .GetMatchingProcedures(functionIdentifier, bindingParameterType, isSideEffecting: false);
        }

        private static IEnumerable<IEdmFunctionImport> GetMatchingProcedures(this IEnumerable<IEdmFunctionImport> procedures,
            string procedureIdentifier, IEdmType bindingParameterType, bool isSideEffecting)
        {
            Contract.Assert(procedures != null);
            Contract.Assert(procedureIdentifier != null);

            procedures = procedures.Where(p => p.IsSideEffecting == isSideEffecting);

            string[] nameParts = procedureIdentifier.Split('.');
            Contract.Assert(nameParts.Length != 0);

            if (nameParts.Length == 1)
            {
                // Name
                string name = nameParts[0];
                procedures = procedures.Where(f => f.Name == name);
            }
            else if (nameParts.Length == 2)
            {
                // Container.Name
                string name = nameParts[nameParts.Length - 1];
                string container = nameParts[nameParts.Length - 2];
                procedures = procedures.Where(f => f.Name == name && f.Container.Name == container);
            }
            else
            {
                // Namespace.Container.Name
                string name = nameParts[nameParts.Length - 1];
                string container = nameParts[nameParts.Length - 2];
                string nspace = String.Join(".", nameParts.Take(nameParts.Length - 2));
                procedures = procedures.Where(f => f.Name == name && f.Container.Name == container && f.Container.Namespace == nspace);
            }

            if (bindingParameterType != null)
            {
                procedures = procedures.Where(procedure => procedure.CanBindTo(bindingParameterType));
            }
            else
            {
                procedures = procedures.Where(procedure => !procedure.IsBindable);
            }

            return procedures;
        }

        private static bool CanBindTo(this IEdmFunctionImport function, IEdmType type)
        {
            Contract.Assert(function != null);
            Contract.Assert(type != null);

            if (!function.IsBindable)
            {
                return false;
            }

            // The binding parameter is the first parameter by convention
            IEdmFunctionParameter bindingParameter = function.Parameters.FirstOrDefault();
            if (bindingParameter == null)
            {
                return false;
            }

            return IsAssignable(type, bindingParameter.Type.Definition);
        }

        private static bool IsAssignable(IEdmType from, IEdmType to)
        {
            if (from == null || to == null || from.TypeKind != to.TypeKind)
            {
                return false;
            }

            if (from.TypeKind == EdmTypeKind.Entity)
            {
                return (from as IEdmEntityType).IsOrInheritsFrom(to);
            }

            if (from.TypeKind == EdmTypeKind.Collection)
            {
                IEdmType fromElementType = (from as IEdmCollectionType).ElementType.Definition;
                IEdmType toElementType = (to as IEdmCollectionType).ElementType.Definition;
                return IsAssignable(fromElementType, toElementType);
            }

            return false;
        }

        // Performs overload resolution between a set of matching bindable actions. OData protocol ensures that there 
        // cannot be multiple bindable actions with same name and different sets of non-bindable paramters. 
        // The resolution logic is simple and is dependant only on the binding parameter and choses the action that is defined
        // closest to the binding parameter in the inheritance hierarchy.
        private static IEdmFunctionImport FindBest(string actionIdentifier, IEnumerable<IEdmFunctionImport> bindableActions,
            IEdmEntityType bindingParameterType, bool isCollection)
        {
            if (bindingParameterType == null)
            {
                return null;
            }

            List<IEdmFunctionImport> actionsBoundToThisType = new List<IEdmFunctionImport>();
            foreach (IEdmFunctionImport action in bindableActions)
            {
                IEdmType actionParameterType = action.Parameters.First().Type.Definition;
                if (isCollection)
                {
                    actionParameterType = ((IEdmCollectionType)actionParameterType).ElementType.Definition;
                }

                if (actionParameterType == bindingParameterType)
                {
                    actionsBoundToThisType.Add(action);
                }
            }

            if (actionsBoundToThisType.Count > 1)
            {
                throw Error.Argument(
                    "actionIdentifier",
                    SRResources.ActionResolutionFailed,
                    actionIdentifier,
                    String.Join(", ", actionsBoundToThisType.Select(match => match.Container.FullName() + "." + match.Name)));
            }
            else if (actionsBoundToThisType.Count == 1)
            {
                return actionsBoundToThisType[0];
            }
            else
            {
                return FindBest(actionIdentifier, bindableActions, bindingParameterType.BaseEntityType(), isCollection);
            }
        }
    }
}
