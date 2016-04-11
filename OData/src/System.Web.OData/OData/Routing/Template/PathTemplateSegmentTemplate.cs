// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.OData.Routing.Template
{
    /// <summary>
    ///  Represents a template that can match a <see cref="PathTemplateSegment"/>.
    /// </summary>
    public class PathTemplateSegmentTemplate : ODataPathSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PathTemplateSegmentTemplate"/> class.
        /// </summary>
        /// <param name="segment">The path template segment to be parsed as a template.</param>
        public PathTemplateSegmentTemplate(PathTemplateSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            TemplateSegment = segment;

            string value;
            SegmentName = segment.TranslatePathTemplateSegment(out value);

            PropertyName = value;
            TreatPropertyNameAsParameterName = false;

            if (RoutingConventionHelpers.IsRouteParameter(PropertyName))
            {
                PropertyName = PropertyName.Substring(1, PropertyName.Length - 2);
                TreatPropertyNameAsParameterName = true;

                if (String.IsNullOrEmpty(PropertyName))
                {
                    Error.Format(SRResources.EmptyParameterAlias, PropertyName, segment.LiteralText);
                }
            }
        }

        /// <summary>
        /// The parameter name of the dynamic property.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Gets the segment name
        /// </summary>
        public string SegmentName { get; private set; }

        /// <summary>
        /// Indicates whether the template should match the name, or treat it as a parameter.
        /// </summary>
        private bool TreatPropertyNameAsParameterName { get; set; }

        /// <summary>
        /// The parameter name of the dynamic property.
        /// </summary>
        public PathTemplateSegment TemplateSegment { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathSegment"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            // So far, we only support the dynamic property segment template
            OpenPropertySegment openPropertySegment = pathSegment as OpenPropertySegment;
            if (openPropertySegment == null)
            {
                return false;
            }

            // If we're treating the property name as a parameter store the provided name in our values collection
            // using the name from the template as the key.
            if (TreatPropertyNameAsParameterName)
            {
                values[PropertyName] = openPropertySegment.PropertyName;
                values[ODataParameterValue.ParameterValuePrefix + PropertyName] =
                    new ODataParameterValue(openPropertySegment.PropertyName,
                        EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string)));
                return true;
            }

            if (PropertyName == openPropertySegment.PropertyName)
            {
                return true;
            }

            return false;
        }
    }
}
