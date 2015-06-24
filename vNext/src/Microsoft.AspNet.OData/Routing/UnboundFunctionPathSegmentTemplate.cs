// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// Represents a template that can match a <see cref="UnboundFunctionPathSegment"/>.
    /// </summary>
    public class UnboundFunctionPathSegmentTemplate : ODataPathSegmentTemplate
    {
        private string _functionName;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnboundFunctionPathSegmentTemplate"/> class.
        /// </summary>
        /// <param name="function">The function segment to be templatized.</param>
        public UnboundFunctionPathSegmentTemplate(UnboundFunctionPathSegment function)
        {
            if (function == null)
            {
                throw Error.ArgumentNull("function");
            }

            _functionName = function.FunctionName;
            ParameterMappings = KeyValuePathSegmentTemplate.BuildParameterMappings(function.Values, function.ToString());
        }

        /// <summary>
        /// Gets the dictionary representing the mappings from the parameter names in the current function segment to the 
        /// parameter names in route data.
        /// </summary>
        public IDictionary<string, string> ParameterMappings { get; private set; }

        /// <inheritdoc />
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            if (pathSegment.SegmentKind == ODataSegmentKinds.UnboundFunction)
            {
                UnboundFunctionPathSegment functionSegment = (UnboundFunctionPathSegment)pathSegment;
                if (_functionName == functionSegment.FunctionName)
                {
                    var enumNames = functionSegment.Function.Function.Parameters.Where(p => p.Type.IsEnum()).Select(p => p.Name);
                    if (KeyValuePathSegmentTemplate.TryMatch(ParameterMappings, functionSegment.Values, values,
                        enumNames))
                    {
                        foreach (KeyValuePair<string, string> nameAndValue in functionSegment.Values)
                        {
                            string name = nameAndValue.Key;
                            object value = functionSegment.GetParameterValue(name);

                            //ProcedureRoutingConventionHelpers.AddFunctionParameters(functionSegment.Function.Function, name,
                            //    value, values, values, ParameterMappings);
                            throw new NotImplementedException("UnboundFunctionPathSegmentTemplate");
                        }

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
