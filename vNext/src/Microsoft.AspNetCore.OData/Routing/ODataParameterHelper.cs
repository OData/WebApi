// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Provides the extension method for odata parameter
    /// </summary>
    public static class ODataParameterHelper
    {
        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        /// <param name="segment">The function segment</param>
        /// <param name="parameterName">The parameter name</param>
        /// <param name="parameterValue">The parameter value</param>
        /// <returns>The <value>true</value> ok, <value>false</value> not find.</returns>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "Generics not appropriate here")]
        public static bool TryGetParameterValue(this OperationSegment segment, string parameterName, out object parameterValue)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            if (String.IsNullOrEmpty(parameterName))
            {
                throw Error.ArgumentNullOrEmpty("parameterName");
            }

            parameterValue = null;
            OperationSegmentParameter parameter = segment.Parameters.FirstOrDefault(p => p.Name == parameterName);
            if (parameter == null)
            {
                return false;
            }

            parameterValue = TranslateNode(parameter.Value);
            return true;
        }

        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        /// <param name="segment">The function segment</param>
        /// <param name="parameterName">The parameter name</param>
        /// <returns>The parameter value</returns>
        public static object GetParameterValue(this OperationSegment segment, string parameterName)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            if (String.IsNullOrEmpty(parameterName))
            {
                throw Error.ArgumentNullOrEmpty("parameterName");
            }

            if (!segment.Operations.Any() || !segment.Operations.First().IsFunction())
            {
                throw Error.Argument("segment", /*SRResources.OperationSegmentMustBeFunction*/"TODO: ");
            }

            OperationSegmentParameter parameter = segment.Parameters.FirstOrDefault(p => p.Name == parameterName);
            if (parameter == null)
            {
                throw Error.Argument("parameterName", SRResources.FunctionParameterNotFound, parameterName);
            }

            object value = TranslateNode(parameter.Value);

            if (value == null || value is ODataNullValue)
            {
                IEdmOperation operation = segment.Operations.First();
                IEdmOperationParameter operationParameter = operation.Parameters.First(p => p.Name == parameterName);
                Contract.Assert(operationParameter != null);

                if (!operationParameter.Type.IsNullable)
                {
                    throw new ODataException("TODO: "/*String.Format(CultureInfo.InvariantCulture,
                        SRResources.NullOnNonNullableFunctionParameter, operationParameter.Type.FullName())*/);
                }
            }

            return value;
        }

        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        /// <param name="segment">The function segment</param>
        /// <param name="parameterName">The parameter name</param>
        /// <param name="parameterValue">The parameter value</param>
        /// <returns>The <value>true</value> ok, <value>false</value> not find.</returns>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "Generics not appropriate here")]
        public static bool TryGetParameterValue(this OperationImportSegment segment, string parameterName, out object parameterValue)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            if (String.IsNullOrEmpty(parameterName))
            {
                throw Error.ArgumentNullOrEmpty("parameterName");
            }

            parameterValue = null;
            OperationSegmentParameter parameter = segment.Parameters.FirstOrDefault(p => p.Name == parameterName);
            if (parameter == null)
            {
                return false;
            }

            parameterValue = TranslateNode(parameter.Value);
            return true;
        }

        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        /// <param name="segment">The function import segment</param>
        /// <param name="parameterName">The parameter name</param>
        /// <returns>The parameter value</returns>
        public static object GetParameterValue(this OperationImportSegment segment, string parameterName)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            if (String.IsNullOrEmpty(parameterName))
            {
                throw Error.ArgumentNullOrEmpty("parameterName");
            }

            if (!segment.OperationImports.Any() || !segment.OperationImports.First().IsFunctionImport())
            {
                throw Error.Argument("segment", "TODO: "/*SRResources.OperationImportSegmentMustBeFunction*/);
            }

            OperationSegmentParameter parameter = segment.Parameters.FirstOrDefault(p => p.Name == parameterName);
            if (parameter == null)
            {
                throw Error.Argument("parameterName", SRResources.FunctionParameterNotFound, parameterName);
            }

            object value = TranslateNode(parameter.Value);

            if (value == null || value is ODataNullValue)
            {
                IEdmOperationImport operation = segment.OperationImports.First();
                IEdmOperationParameter operationParameter = operation.Operation.Parameters.First(p => p.Name == parameterName);
                Contract.Assert(operationParameter != null);

                if (!operationParameter.Type.IsNullable)
                {
                    throw new ODataException("TODO: "/*String.Format(CultureInfo.InvariantCulture,
                        SRResources.NullOnNonNullableFunctionParameter, operationParameter.Type.FullName())*/);
                }
            }

            return value;
        }

        internal static object TranslateNode(object value)
        {
            if (value == null)
            {
                return null;
            }

            ConstantNode node = value as ConstantNode;
            if (node != null)
            {
                return node.Value;
            }

            ConvertNode convertNode = value as ConvertNode;
            if (convertNode != null)
            {
                object source = TranslateNode(convertNode.Source);
                return source;
            }

            ParameterAliasNode parameterAliasNode = value as ParameterAliasNode;
            if (parameterAliasNode != null)
            {
                return parameterAliasNode.Alias;
            }

            throw Error.NotSupported(SRResources.CannotRecognizeNodeType, typeof(ODataParameterHelper), value.GetType().FullName);
        }
    }
}
