// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that can match a <see cref="PropertySegment"/>.
    /// </summary>
    public class PropertySegmentTemplate : ODataPathSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertySegmentTemplate"/> class.
        /// </summary>
        /// <param name="segment">The property segment</param>
        public PropertySegmentTemplate(PropertySegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            Segment = segment;
        }

        /// <summary>
        /// Gets or sets the property segment.
        /// </summary>
        public PropertySegment Segment { get; private set; }

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            PropertySegment other = pathSegment as PropertySegment;
            return other != null && other.Property == Segment.Property;
        }
    }
}
