//-----------------------------------------------------------------------------
// <copyright file="NavigationPropertySegmentTemplate.cs" company=".NET Foundation">
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
    /// Represents a template that can match a <see cref="NavigationPropertySegment"/>.
    /// </summary>
    public class NavigationPropertySegmentTemplate : ODataPathSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPropertySegmentTemplate"/> class.
        /// </summary>
        /// <param name="segment">The navigation property segment</param>
        public NavigationPropertySegmentTemplate(NavigationPropertySegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            Segment = segment;
        }

        /// <summary>
        /// Gets or sets the navigation property segment.
        /// </summary>
        public NavigationPropertySegment Segment { get; private set; }

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            NavigationPropertySegment otherNavPropSegment = pathSegment as NavigationPropertySegment;
            return otherNavPropSegment != null && otherNavPropSegment.NavigationProperty == Segment.NavigationProperty;
        }
    }
}
