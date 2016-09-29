// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Routing.Template
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
