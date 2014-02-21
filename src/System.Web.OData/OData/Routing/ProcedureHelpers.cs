// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
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
            if (actions.Count() == 0)
            {
                return null;
            }

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
                string message = String.Join(", ", matchesArray.Select(match => match.Name));
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

        // ODL Spec says:
        // To invoke a function bound to a resource, ... constructed by appending the namespace- or alias-qualified function name
        // to a URL that identifies a resource whose type is the same as, or derived from, the type of the binding parameter 
        // of the function. 
        private static IEnumerable<IEdmOperation> GetMatchedOperations(this IEnumerable<IEdmOperation> operations,
            string operationIdentifier, IEdmType bindingType)
        {
            Contract.Assert(operations != null);
            Contract.Assert(operationIdentifier != null);
            Contract.Assert(bindingType != null);

            string[] nameParts = operationIdentifier.Split('.');
            Contract.Assert(nameParts.Length != 0);

            if (nameParts.Length > 1)
            {
                // Namespace.Name
                string name = nameParts[nameParts.Length - 1];
                string nspace = String.Join(".", nameParts.Take(nameParts.Length - 1));
                return operations.Where(f => f.Name == name && f.Namespace == nspace && f.CanBindTo(bindingType));
            }
            else
            {
                return Enumerable.Empty<IEdmOperation>();
            }
        }

        // ODL Spec:
        // 11.5.3.1 Invoking a Function
        // To invoke a function through a function import the client issues a GET request to a URL identifying the function 
        // import and passing parameter values using inline parameter syntax. The canonical URL for a function import is 
        // the service root, followed by the name of the function import.
        // 11.5.4.1 Invoking an Action
        // To invoke an action through an action import, the client issues a POST request to a URL identifying the action import.
        // The canonical URL for an action import is the service root, followed by the name of the action import. 
        private static IEnumerable<IEdmOperationImport> GetMatchedOperationImports(this IEnumerable<IEdmOperationImport> operationImports,
            string operationIdentifier)
        {
            Contract.Assert(operationImports != null);
            Contract.Assert(operationIdentifier != null);

            string[] nameParts = operationIdentifier.Split('.');
            Contract.Assert(nameParts.Length != 0);

            if (nameParts.Length == 1)
            {
                // Procedure (Function/Action) Import Name
                string name = nameParts[0];
                return operationImports.Where(f => f.Name == name && !f.Operation.IsBound);
            }
            else
            {
                return Enumerable.Empty<IEdmOperationImport>();
            }
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
