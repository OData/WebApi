// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Core;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// Represents a template that can match a <see cref="OpenPropertyPathSegment"/>.
    /// </summary>
    public class OpenPropertyPathSegmentTemplate : ODataPathSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenPropertyPathSegmentTemplate"/> class.
        /// </summary>
        /// <param name="openPropertyPathSegment">The key value segment to be parsed as a template.</param>
        public OpenPropertyPathSegmentTemplate(OpenPropertyPathSegment openPropertyPathSegment)
        {
            if (openPropertyPathSegment == null)
            {
                throw Error.ArgumentNull("openPropertyPathSegment");
            }

            this.PropertyName = openPropertyPathSegment.PropertyName;
            this.TreatPropertyNameAsParameterName = false;

            if (OpenPropertyPathSegmentTemplate.IsRouteParameter(this.PropertyName))
            {
                this.PropertyName = this.PropertyName.Substring(1, this.PropertyName.Length - 2);
                this.TreatPropertyNameAsParameterName = true;

                if (String.IsNullOrEmpty(this.PropertyName))
                {
                    throw new ODataException(
                        Error.Format(SRResources.EmptyParameterAlias, this.PropertyName, openPropertyPathSegment));
                }
            }
        }

        /// <summary>
        /// The parameter name of the open property.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Indicates whether the template should match the name, or treat it as a parameter.
        /// </summary>
        public bool TreatPropertyNameAsParameterName { get; private set; }

        /// <inheritdoc />
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            if (pathSegment.SegmentKind == ODataSegmentKinds.OpenProperty)
            {
                var openPropertyPathSegment = (OpenPropertyPathSegment)pathSegment;

                // If we're treating the property name as a parameter store the provided name in our values collection
                // using the name from the template as the key.
                if (this.TreatPropertyNameAsParameterName)
                {
                    values[this.PropertyName] = openPropertyPathSegment.PropertyName;
                    return true;
                }

                if (this.PropertyName == openPropertyPathSegment.PropertyName)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsRouteParameter(string parameterName)
        {
            return parameterName.StartsWith("{", StringComparison.Ordinal) &&
                    parameterName.EndsWith("}", StringComparison.Ordinal);
        }
    }
}
