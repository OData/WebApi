// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace System.Web.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that can match a <see cref="OpenPropertySegment"/>.
    /// </summary>
    public class OpenPropertySegmentTemplate : ODataPathSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenPropertySegmentTemplate"/> class.
        /// </summary>
        /// <param name="segment">The open property segment</param>
        public OpenPropertySegmentTemplate(OpenPropertySegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            Segment = segment;

            PropertyName = segment.PropertyName;
            TreatPropertyNameAsParameterName = false;

            if (RoutingConventionHelpers.IsRouteParameter(PropertyName))
            {
                PropertyName = PropertyName.Substring(1, PropertyName.Length - 2);
                TreatPropertyNameAsParameterName = true;

                if (String.IsNullOrEmpty(PropertyName))
                {
                    throw new ODataException(
                        Error.Format(SRResources.EmptyParameterAlias, PropertyName, segment.PropertyName));
                }
            }
        }

        /// <summary>
        /// Gets or sets the open property segment.
        /// </summary>
        public OpenPropertySegment Segment { get; private set; }

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
            OpenPropertySegment other = pathSegment as OpenPropertySegment;
            if (other == null)
            {
                return false;
            }

            // If we're treating the property name as a parameter store the provided name in our values collection
            // using the name from the template as the key.
            if (TreatPropertyNameAsParameterName)
            {
                values[PropertyName] = other.PropertyName;
                values[ODataParameterValue.ParameterValuePrefix + PropertyName] =
                    new ODataParameterValue(other.PropertyName,
                        EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string)));
                return true;
            }

            if (PropertyName == other.PropertyName)
            {
                return true;
            }

            return false;
        }
    }
}
