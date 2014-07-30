// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
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
                    return KeyValuePathSegmentTemplate.TryMatch(ParameterMappings, functionSegment.Values, values, enumNames);
                }
            }

            return false;
        }
    }
}
