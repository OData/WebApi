// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Common;
using System.Linq;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Represents a template that can match a <see cref="BoundFunctionPathSegment"/>.
    /// </summary>
    public class BoundFunctionPathSegmentTemplate : ODataPathSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoundFunctionPathSegmentTemplate"/> class.
        /// </summary>
        /// <param name="function">The function segment to be templatized</param>
        public BoundFunctionPathSegmentTemplate(BoundFunctionPathSegment function)
        {
            if (function == null)
            {
                throw Error.ArgumentNull("function");
            }

            FunctionName = function.FunctionName;
            ParameterMappings = KeyValuePathSegmentTemplate.BuildParameterMappings(function.Values, function.ToString());
        }

        /// <summary>
        /// Gets the dictionary representing the mappings from the parameter names in the current function segment to the 
        /// parameter names in route data.
        /// </summary>
        public IDictionary<string, string> ParameterMappings { get; private set; }

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public string FunctionName { get; private set; }

        /// <inheritdoc />
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            if (pathSegment.SegmentKind == ODataSegmentKinds.Function)
            {
                BoundFunctionPathSegment functionSegment = (BoundFunctionPathSegment)pathSegment;
                if (FunctionName == functionSegment.FunctionName)
                {
                    var enumNames = functionSegment.Function.Parameters.Where(p => p.Type.IsEnum()).Select(p => p.Name);
                    if (KeyValuePathSegmentTemplate.TryMatch(ParameterMappings, functionSegment.Values, values,
                        enumNames))
                    {
                        foreach (KeyValuePair<string, string> nameAndValue in functionSegment.Values)
                        {
                            string name = nameAndValue.Key;
                            object value = functionSegment.GetParameterValue(name);

                            //ProcedureRoutingConventionHelpers.AddFunctionParameters(functionSegment.Function, name,
                            //    value, values, values, ParameterMappings);
                            throw new NotImplementedException("ProcedureRoutingConventionHelpers.AddFunctionParameters");
                        }

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
