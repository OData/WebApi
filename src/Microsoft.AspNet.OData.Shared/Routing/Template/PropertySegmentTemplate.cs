//-----------------------------------------------------------------------------
// <copyright file="PropertySegmentTemplate.cs" company=".NET Foundation">
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
