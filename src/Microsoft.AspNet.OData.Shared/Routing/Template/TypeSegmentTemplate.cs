//-----------------------------------------------------------------------------
// <copyright file="TypeSegmentTemplate.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing.Template
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
