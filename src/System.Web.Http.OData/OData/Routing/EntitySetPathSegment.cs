// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Formatter;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing an entity set.
    /// </summary>
    public class EntitySetPathSegment : ODataPathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetPathSegment" /> class.
        /// </summary>
        /// <param name="previous">The previous segment in the path.</param>
        /// <param name="entitySet">The entity set being accessed.</param>
        public EntitySetPathSegment(ODataPathSegment previous, IEdmEntitySet entitySet)
            : base(previous)
        {
            if (entitySet == null)
            {
                throw Error.ArgumentNull("entitySet");
            }

            EdmType = entitySet.ElementType.GetCollection();
            EntitySet = entitySet;
        }

        /// <summary>
        /// Gets the segment kind for the current segment.
        /// </summary>
        public override string SegmentKind
        {
            get
            {
                return ODataSegmentKinds.EntitySet;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return EntitySet.Name;
        }
    }
}
