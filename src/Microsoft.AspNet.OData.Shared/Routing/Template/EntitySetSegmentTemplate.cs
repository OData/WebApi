//-----------------------------------------------------------------------------
// <copyright file="EntitySetSegmentTemplate.cs" company=".NET Foundation">
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
    /// Represents a template that can match a <see cref="EntitySetSegment"/>.
    /// </summary>
    public class EntitySetSegmentTemplate : ODataPathSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetSegmentTemplate"/> class.
        /// </summary>
        /// <param name="segment">The entity set segment</param>
        public EntitySetSegmentTemplate(EntitySetSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            Segment = segment;
        }

        /// <summary>
        /// Gets or sets the entity set segment.
        /// </summary>
        public EntitySetSegment Segment { get; private set; }

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            EntitySetSegment otherEntitySet = pathSegment as EntitySetSegment;
            return otherEntitySet != null && otherEntitySet.EntitySet == Segment.EntitySet;
        }
    }
}
