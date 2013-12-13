// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// Represents a template that can match a <see cref="FunctionPathSegment"/>.
    /// </summary>
    public class FunctionPathSegmentTemplate : ODataPathSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionPathSegmentTemplate"/> class.
        /// </summary>
        /// <param name="function">The function segment to be templatized</param>
        public FunctionPathSegmentTemplate(FunctionPathSegment function)
        {
            if (function == null)
            {
                throw Error.ArgumentNull("function");
            }

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
            if (pathSegment.SegmentKind == ODataSegmentKinds.Function)
            {
                FunctionPathSegment functionSegment = (FunctionPathSegment)pathSegment;
                return KeyValuePathSegmentTemplate.TryMatch(ParameterMappings, functionSegment.Values, values);
            }

            return false;
        }
    }
}
