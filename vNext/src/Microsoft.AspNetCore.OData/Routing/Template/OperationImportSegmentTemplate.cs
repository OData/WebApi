// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that can match a <see cref="OperationImportSegment"/>.
    /// </summary>
    public class OperationImportSegmentTemplate : ODataPathSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationImportSegmentTemplate"/> class.
        /// </summary>
        /// <param name="segment">The operation import segment</param>
        public OperationImportSegmentTemplate(OperationImportSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            Segment = segment;

            IEdmOperationImport operation = Segment.OperationImports.First();
            if (operation.IsFunctionImport())
            {
                ParameterMappings = RoutingConventionHelpers.BuildParameterMappings(segment.Parameters, operation.Name);
            }
        }

        /// <summary>
        /// Gets and sets the operation import segment.
        /// </summary>
        public OperationImportSegment Segment { get; private set; }

        /// <summary>
        /// Gets the dictionary representing the mappings from the parameter names in the current function segment to the 
        /// parameter names in route data.
        /// </summary>
        public IDictionary<string, string> ParameterMappings { get; private set; }

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            OperationImportSegment other = pathSegment as OperationImportSegment;
            if (other == null)
            {
                return false;
            }

            IEdmOperationImport operationImport = Segment.OperationImports.First();
            IEdmOperationImport otherImport = other.OperationImports.First();

            // for unbound action, just compare the action import
            if (operationImport.IsActionImport() && otherImport.IsActionImport())
            {
                return operationImport == otherImport;
            }
            else if (operationImport.IsFunctionImport() && otherImport.IsFunctionImport())
            {
                // but for unbound function, we should compare the parameter names and 
                // process the parameter values into odata routes.
                if (operationImport.Name != otherImport.Name)
                {
                    return false;
                }

                IDictionary<string, object> parameterValues = new Dictionary<string, object>();
                foreach (var parameter in other.Parameters)
                {
                    object value = other.GetParameterValue(parameter.Name);
                    parameterValues[parameter.Name] = value;
                }

                if (RoutingConventionHelpers.TryMatch(ParameterMappings, parameterValues, values))
                {
                    foreach (var operationSegmentParameter in other.Parameters)
                    {
                        string name = operationSegmentParameter.Name;
                        object value = parameterValues[name];

                        RoutingConventionHelpers.AddFunctionParameters((IEdmFunction)otherImport.Operation, name,
                            value, values, values, ParameterMappings);
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
