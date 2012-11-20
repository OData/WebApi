// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
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
        /// <param name="entitySet">The entity set being accessed.</param>
        public EntitySetPathSegment(IEdmEntitySet entitySet)
        {
            if (entitySet == null)
            {
                throw Error.ArgumentNull("entitySet");
            }

            EntitySet = entitySet;
            EntitySetName = entitySet.Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetPathSegment" /> class.
        /// </summary>
        /// <param name="entitySetName">Name of the entity set.</param>
        public EntitySetPathSegment(string entitySetName)
        {
            if (entitySetName == null)
            {
                throw Error.ArgumentNull("entitySetName");
            }

            EntitySetName = entitySetName;
        }

        /// <summary>
        /// Gets the entity set represented by this segment.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Refers to the EntitySet for this segment.")]
        public IEdmEntitySet EntitySet
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the entity set.
        /// </summary>
        public string EntitySetName
        {
            get;
            private set;
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
        /// Gets the EDM type for this segment.
        /// </summary>
        /// <param name="previousEdmType">The EDM type of the previous path segment.</param>
        /// <returns>
        /// The EDM type for this segment.
        /// </returns>
        public override IEdmType GetEdmType(IEdmType previousEdmType)
        {
            if (EntitySet != null)
            {
                return EntitySet.ElementType.GetCollection();
            }
            return null;
        }

        /// <summary>
        /// Gets the entity set for this segment.
        /// </summary>
        /// <param name="previousEntitySet">The entity set of the previous path segment.</param>
        /// <returns>
        /// The entity set for this segment.
        /// </returns>
        public override IEdmEntitySet GetEntitySet(IEdmEntitySet previousEntitySet)
        {
            if (EntitySet != null)
            {
                return EntitySet;
            }
            return null;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return EntitySetName;
        }
    }
}
