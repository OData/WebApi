// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;

namespace System.Web.OData.Routing
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
                    return KeyValuePathSegmentTemplate.TryMatch(ParameterMappings, functionSegment.Values, values);
                }
            }

            return false;
        }
    }
}
