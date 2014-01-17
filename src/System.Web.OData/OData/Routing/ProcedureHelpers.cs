// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.OData.Properties;
using Microsoft.OData.Edm;

namespace System.Web.Http.OData.Routing
{
    internal static class ProcedureHelpers
    {
        public static IEnumerable<IEdmOperation> FindMatchedOperations(this IEdmModel model, string identifier, IEdmType bindingType)
        {
            Contract.Assert(model != null);
            Contract.Assert(identifier != null);
            Contract.Assert(bindingType != null);

            IEnumerable<IEdmOperation> operations = model.SchemaElements.OfType<IEdmOperation>();
            return operations.GetMatchedOperations(identifier, bindingType);
        }

        public static IEnumerable<IEdmOperationImport> FindMatchedOperationImports(this IEdmEntityContainer container, string identifier)
        {
            Contract.Assert(container != null);
            Contract.Assert(identifier != null);

            return container.OperationImports().GetMatchedOperationImports(identifier);
        }

        public static IEdmAction FindAction(this IEdmModel model, string actionIdentifier, IEdmType bindingType)
        {
            Contract.Assert(model != null);
            Contract.Assert(actionIdentifier != null);
            Contract.Assert(bindingType != null);

            IEnumerable<IEdmOperation> matchedOperations = model.FindMatchedOperations(actionIdentifier, bindingType);
            IList<IEdmAction> actions = matchedOperations.OfType<IEdmAction>().ToList();

            bool isCollection = false;
            if (bindingType.TypeKind == EdmTypeKind.Collection)
            {
                bindingType = ((IEdmCollectionType)bindingType).ElementType.Definition;
                isCollection = true;
            }

            // filter by the paramters
            return FindBest(actionIdentifier, actions, (IEdmEntityType)bindingType, isCollection);
        }

        public static IEdmActionImport FindActionImport(this IEdmEntityContainer container, string actionIdentifier)
        {
            Contract.Assert(container != null);
            Contract.Assert(actionIdentifier != null);

            IEnumerable<IEdmOperationImport> matchedOperations = container.FindMatchedOperationImports(actionIdentifier);
            IEdmActionImport[] matchesArray = matchedOperations.OfType<IEdmActionImport>().ToArray();

            // Refer to the OData V4 Specification. Section 11.5.4.2 contains this requirement:
            // The same action name may be used multiple times within a schema provided there is at most one unbound overload, 
            // and each bound overload specifies a different binding parameter type.
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

        private static IEnumerable<IEdmOperation> GetMatchedOperations(this IEnumerable<IEdmOperation> operations,
            string operationIdentifier, IEdmType bindingType)
        {
            Contract.Assert(operations != null);
            Contract.Assert(operationIdentifier != null);
            Contract.Assert(bindingType != null);

            string[] nameParts = operationIdentifier.Split('.');
            Contract.Assert(nameParts.Length != 0);

            if (nameParts.Length == 1)
            {
                // Name
                string name = nameParts[0];
                operations = operations.Where(f => f.Name == name);
            }
            else
            {
                // Namespace.Name
                string name = nameParts[nameParts.Length - 1];
                string nspace = String.Join(".", nameParts.Take(nameParts.Length - 1));
                operations = operations.Where(f => f.Name == name && f.Namespace == nspace);
            }

            return operations.Where(procedure => procedure.CanBindTo(bindingType));
        }

        private static IEnumerable<IEdmOperationImport> GetMatchedOperationImports(this IEnumerable<IEdmOperationImport> operationImports,
            string operationIdentifier)
        {
            Contract.Assert(operationImports != null);
            Contract.Assert(operationIdentifier != null);

            string[] nameParts = operationIdentifier.Split('.');
            Contract.Assert(nameParts.Length != 0);

            if (nameParts.Length == 1)
            {
                // Name
                string name = nameParts[0];
                operationImports = operationImports.Where(f => f.Name == name);
            }
            else if (nameParts.Length == 2)
            {
                // Container.Name
                string name = nameParts[nameParts.Length - 1];
                string container = nameParts[nameParts.Length - 2];
                operationImports = operationImports.Where(f => f.Name == name && f.Container.Name == container);
            }
            else
            {
                // Namespace.Container.Name
                string name = nameParts[nameParts.Length - 1];
                string container = nameParts[nameParts.Length - 2];
                string nspace = String.Join(".", nameParts.Take(nameParts.Length - 2));
                operationImports = operationImports.Where(f => f.Name == name && f.Container.Name == container && f.Container.Namespace == nspace);
            }

            return operationImports.Where(p => !p.Operation.IsBound);
        }

        private static bool CanBindTo(this IEdmOperation operation, IEdmType type)
        {
            Contract.Assert(operation != null);
            Contract.Assert(type != null);

            // The binding parameter is the first parameter by convention
            IEdmOperationParameter bindingParameter = operation.Parameters.FirstOrDefault();
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
        private static IEdmAction FindBest(string actionIdentifier, IEnumerable<IEdmAction> bindableActions,
            IEdmEntityType bindingType, bool isCollection)
        {
            if (bindingType == null)
            {
                return null;
            }

            List<IEdmAction> actionsBoundToThisType = new List<IEdmAction>();
            foreach (IEdmAction action in bindableActions)
            {
                IEdmType actionParameterType = action.Parameters.First().Type.Definition;
                if (isCollection)
                {
                    actionParameterType = ((IEdmCollectionType)actionParameterType).ElementType.Definition;
                }

                if (actionParameterType == bindingType)
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
                    String.Join(", ", actionsBoundToThisType.Select(match => match.FullName())));
            }
            else if (actionsBoundToThisType.Count == 1)
            {
                return actionsBoundToThisType[0];
            }
            else
            {
                return FindBest(actionIdentifier, bindableActions, bindingType.BaseEntityType(), isCollection);
            }
        }
    }
}
