// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
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
        public static BoundFunctionPathSegment TryResolveBound(IEnumerable<IEdmFunction> functions, IEdmModel model, string nextSegment)
        {
            Dictionary<string, string> parameters = GetParameters(nextSegment);
            IEnumerable<string> parameterNames;

            if (parameters == null)
            {
                parameterNames = null;
            }
            else if (parameters.Keys.Contains(String.Empty))
            {
                // One of the function parameters has no name.
                return null;
            }
            else
            {
                parameterNames = parameters.Keys;
            }

            IEdmFunction function = FindBestBoundFunction(functions, parameterNames);
            if (function != null)
            {
                if (GetNonBindingParameters(function).Any())
                {
                    return new BoundFunctionPathSegment(function, model, parameters);
                }
                else
                {
                    return new BoundFunctionPathSegment(function, model, parameterValues: null);
                }
            }

            return null;
        }

        public static UnboundFunctionPathSegment TryResolveUnbound(IEnumerable<IEdmFunctionImport> functions, IEdmModel model, string nextSegment)
        {
            Dictionary<string, string> parameters = GetParameters(nextSegment);
            IEnumerable<string> parameterNames;

            if (parameters == null)
            {
                parameterNames = null;
            }
            else if (parameters.Keys.Contains(String.Empty))
            {
                // One of the function parameters has no name.
                return null;
            }
            else
            {
                parameterNames = parameters.Keys;
            }

            IEdmFunctionImport function = FindBestUnboundFunction(functions, parameterNames);
            if (function != null)
            {
                if (GetNonBindingParameters(function.Function).Any())
                {
                    return new UnboundFunctionPathSegment(function, model, parameters);
                }
                else
                {
                    return new UnboundFunctionPathSegment(function, model, parameterValues: null);
                }
            }

            return null;
        }

        private static IEdmFunction FindBestBoundFunction(IEnumerable<IEdmFunction> possibleFunctions, IEnumerable<string> parameterNames)
        {
            if (parameterNames != null)
            {
                // function call with parameters.
                HashSet<String> parametersNameSet = new HashSet<string>(parameterNames);
                IEnumerable<IEdmFunction> possibleFunctionsUsingParameters = possibleFunctions.Where(f => IsMatch(f, parametersNameSet));
                IEdmFunction[] matchedFunctions = possibleFunctionsUsingParameters.ToArray();
                if (matchedFunctions.Length == 1)
                {
                    return matchedFunctions[0];
                }
                else if (matchedFunctions.Length > 1)
                {
                    string identifier = matchedFunctions[0].Name;
                    throw new ODataException(Error.Format(SRResources.FunctionResolutionFailed, identifier, String.Join(",", parameterNames)));
                }
            }
            else
            {
                // function call without parameters.
                possibleFunctions = possibleFunctions.Where(f => GetNonBindingParameters(f).Count() == 0);
                IEdmFunction[] matchedFunctions = possibleFunctions.ToArray();
                if (matchedFunctions.Length == 1)
                {
                    return matchedFunctions[0];
                }
                else if (matchedFunctions.Length > 1)
                {
                    string identifier = matchedFunctions[0].Name;
                    throw new ODataException(Error.Format(SRResources.FunctionResolutionFailed, identifier, String.Empty));
                }
            }

            return null;
        }

        private static IEdmFunctionImport FindBestUnboundFunction(IEnumerable<IEdmFunctionImport> possibleFunctions, IEnumerable<string> parameterNames)
        {
            if (parameterNames != null)
            {
                // function call with parameters.
                HashSet<String> parametersNameSet = new HashSet<string>(parameterNames);
                IEnumerable<IEdmFunctionImport> possibleFunctionsUsingParameters = possibleFunctions.Where(f => IsMatch(f.Function, parametersNameSet));
                IEdmFunctionImport[] matchedFunctions = possibleFunctionsUsingParameters.ToArray();
                if (matchedFunctions.Length == 1)
                {
                    return matchedFunctions[0];
                }
                else if (matchedFunctions.Length > 1)
                {
                    string identifier = matchedFunctions[0].Name;
                    throw new ODataException(Error.Format(SRResources.FunctionResolutionFailed, identifier, String.Join(",", parameterNames)));
                }
            }
            else
            {
                // function call without parameters.
                possibleFunctions = possibleFunctions.Where(f => GetNonBindingParameters(f.Function).Count() == 0);
                IEdmFunctionImport[] matchedFunctions = possibleFunctions.ToArray();
                if (matchedFunctions.Length == 1)
                {
                    return matchedFunctions[0];
                }
                else if (matchedFunctions.Length > 1)
                {
                    string identifier = matchedFunctions[0].Name;
                    throw new ODataException(Error.Format(SRResources.FunctionResolutionFailed, identifier, String.Join(",", parameterNames)));
                }
            }

            return null;
        }

        private static bool IsMatch(IEdmFunction function, HashSet<string> parameterNamesSet)
        {
            IEnumerable<IEdmOperationParameter> nonBindingParameters = GetNonBindingParameters(function);
            return parameterNamesSet.SetEquals(nonBindingParameters.Select(p => p.Name));
        }

        private static IEnumerable<IEdmOperationParameter> GetNonBindingParameters(IEdmFunction function)
        {
            IEnumerable<IEdmOperationParameter> functionParameters = function.Parameters;
            if (function.IsBound)
            {
                // skip the binding parameter(first one by convention) for matching.
                functionParameters = functionParameters.Skip(1);
            }

            return functionParameters;
        }

        private static Dictionary<string, string> GetParameters(string nextSegment)
        {
            Dictionary<string, string> parameters = null;
            if (IsEnclosedInParentheses(nextSegment))
            {
                string value = nextSegment.Substring(1, nextSegment.Length - 2);
                parameters = KeyValueParser.ParseKeys(value);
            }

            return parameters;
        }

        internal static bool IsEnclosedInParentheses(string segment)
        {
            return segment != null &&
                segment.StartsWith("(", StringComparison.Ordinal) &&
                segment.EndsWith(")", StringComparison.Ordinal);
        }

        public static bool IsBoundTo(IEdmFunction function, IEdmType type)
        {
            Contract.Assert(function != null);
            Contract.Assert(type != null);

            // The binding parameter is the first parameter by convention
            IEdmOperationParameter bindingParameter = function.Parameters.FirstOrDefault();
            if (bindingParameter == null)
            {
                return false;
            }

            IEdmType fromType;
            if (bindingParameter.Type.Definition.TypeKind == EdmTypeKind.Collection)
            {
                fromType = ((IEdmCollectionType)bindingParameter.Type.Definition).ElementType.Definition;
            }
            else
            {
                fromType = bindingParameter.Type.Definition;
            }

            return fromType == type;
        }
    }
}
