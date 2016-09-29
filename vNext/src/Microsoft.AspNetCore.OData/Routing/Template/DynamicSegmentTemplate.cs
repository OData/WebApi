// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Conventions;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that can match a <see cref="DynamicPathSegment"/>.
    /// </summary>
    public class DynamicSegmentTemplate : ODataPathSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSegmentTemplate"/> class.
        /// </summary>
        /// <param name="segment">The open property segment</param>
        public DynamicSegmentTemplate(DynamicPathSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            Segment = segment;

            PropertyName = segment.Identifier;
            TreatPropertyNameAsParameterName = false;

            if (RoutingConventionHelpers.IsRouteParameter(PropertyName))
            {
                PropertyName = PropertyName.Substring(1, PropertyName.Length - 2);
                TreatPropertyNameAsParameterName = true;

                if (String.IsNullOrEmpty(PropertyName))
                {
                    throw new ODataException(
                        Error.Format(SRResources.EmptyParameterAlias, PropertyName, segment.Identifier));
                }
            }
        }

        /// <summary>
        /// Gets or sets the open property segment.
        /// </summary>
        public DynamicPathSegment Segment { get; private set; }

        /// <summary>
        /// The parameter name of the dynamic property.
        /// </summary>
        private string PropertyName { get; set; }

        /// <summary>
        /// Indicates whether the template should match the name, or treat it as a parameter.
        /// </summary>
        private bool TreatPropertyNameAsParameterName { get; set; }

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            DynamicPathSegment other = pathSegment as DynamicPathSegment;
            if (other == null)
            {
                return false;
            }

            // If we're treating the property name as a parameter store the provided name in our values collection
            // using the name from the template as the key.
            if (TreatPropertyNameAsParameterName)
            {
                values[PropertyName] = other.Identifier;
                values[ODataParameterValue.ParameterValuePrefix + PropertyName] =
                    new ODataParameterValue(other.Identifier,
                        EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string)));
                return true;
            }

            if (PropertyName == other.Identifier)
            {
                return true;
            }

            return false;
        }
    }
}
