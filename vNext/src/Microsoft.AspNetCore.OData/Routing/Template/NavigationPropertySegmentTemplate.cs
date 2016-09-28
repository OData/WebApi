// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Routing.Template
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
