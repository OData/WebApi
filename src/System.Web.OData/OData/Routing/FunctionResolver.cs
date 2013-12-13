// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// Performs function overload resolution. OData protocol mandates that functions with the same name and same binding
    /// parameter (null in case of unbound functions) should have different sets of parameter names i.e two functions
    /// with the same name and same binding parameter cannot have the same set of paramter names.
    /// 
    /// The function overload resolution logic then is a simple matter of figuring out the paramter names and trying to find
    /// a function with that parameter names.
    /// </summary>
    internal static class FunctionResolver
    {
        public static FunctionPathSegment TryResolve(IEnumerable<IEdmFunctionImport> functions, IEdmModel model, string nextSegment)
        {
            Dictionary<string, string> parameters = null;
            IEnumerable<string> parameterNames = null;
            if (IsEnclosedInParentheses(nextSegment))
            {
                string value = nextSegment.Substring(1, nextSegment.Length - 2);
                parameters = KeyValueParser.ParseKeys(value);
                parameterNames = parameters.Keys;
            }

            IEdmFunctionImport function = FindBestFunction(functions, parameterNames);
            if (function != null)
            {
                if (GetNonBindingParameters(function).Any())
                {
                    return new FunctionPathSegment(function, model, parameters);
                }
                else
                {
                    return new FunctionPathSegment(function, model, parameterValues: null);
                }
            }

            return null;
        }

        private static IEdmFunctionImport FindBestFunction(IEnumerable<IEdmFunctionImport> possibleFunctions, IEnumerable<string> parameterNames)
        {
            if (parameterNames != null)
            {
                // function call with parameters.
                IEnumerable<IEdmFunctionImport> possibleFunctionsUsingParameters = possibleFunctions.Where(f => IsMatch(f, parameterNames));
                if (possibleFunctionsUsingParameters.Count() == 1)
                {
                    return possibleFunctionsUsingParameters.Single();
                }
            }
            else
            {
                // function call with no parameters.
                possibleFunctions = possibleFunctions.Where(f => GetNonBindingParameters(f).Count() == 0);
                if (possibleFunctions.Count() == 1)
                {
                    return possibleFunctions.Single();
                }
            }

            return null;
        }

        private static bool IsMatch(IEdmFunctionImport function, IEnumerable<string> parameterNames)
        {
            IEnumerable<IEdmOperationParameter> nonBindingParameters = GetNonBindingParameters(function);
            return new HashSet<string>(parameterNames).SetEquals(nonBindingParameters.Select(p => p.Name));
        }

        private static IEnumerable<IEdmOperationParameter> GetNonBindingParameters(IEdmOperationImport operation)
        {
            IEnumerable<IEdmOperationParameter> functionParameters = operation.Operation.Parameters;
            if (operation.Operation.IsBound)
            {
                // skip the binding parameter(first one by convention) for matching.
                functionParameters = functionParameters.Skip(1);
            }

            return functionParameters;
        }

        internal static bool IsEnclosedInParentheses(string segment)
        {
            return segment != null &&
                segment.StartsWith("(", StringComparison.Ordinal) &&
                segment.EndsWith(")", StringComparison.Ordinal);
        }
    }
}
