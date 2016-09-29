// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that can match a <see cref="TypeSegment"/>.
    /// </summary>
    public class TypeSegmentTemplate : ODataPathSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeSegmentTemplate"/> class.
        /// </summary>
        /// <param name="segment">The type cast segment.</param>
        public TypeSegmentTemplate(TypeSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull("segment");
            }

            Segment = segment;
        }

        /// <summary>
        /// Gets or sets the type cast segment.
        /// </summary>
        public TypeSegment Segment { get; private set; }

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            TypeSegment otherType = pathSegment as TypeSegment;
            return otherType != null && otherType.EdmType.FullTypeName() == Segment.EdmType.FullTypeName();
        }
    }
}
