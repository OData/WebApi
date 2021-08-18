//-----------------------------------------------------------------------------
// <copyright file="NavigationPropertyLinkSegmentTemplate.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that can match a <see cref="NavigationPropertyLinkSegment"/>.
    /// </summary>
    public class NavigationPropertyLinkSegmentTemplate : ODataPathSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPropertyLinkSegmentTemplate"/> class.
        /// </summary>
        /// <param name="segment">The navigation property link segment</param>
        public NavigationPropertyLinkSegmentTemplate(NavigationPropertyLinkSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            Segment = segment;
        }

        /// <summary>
        /// Gets or sets the navigation property link segment.
        /// </summary>
        public NavigationPropertyLinkSegment Segment { get; private set; }

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            NavigationPropertyLinkSegment other = pathSegment as NavigationPropertyLinkSegment;
            return other != null && other.NavigationProperty == Segment.NavigationProperty;
        }
    }
}
