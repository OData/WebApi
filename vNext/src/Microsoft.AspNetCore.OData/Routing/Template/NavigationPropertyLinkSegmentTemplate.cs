// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Routing.Template
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
