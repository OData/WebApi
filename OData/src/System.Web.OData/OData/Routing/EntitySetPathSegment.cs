// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http;
using System.Web.OData.Formatter;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
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
        public EntitySetPathSegment(IEdmEntitySetBase entitySet)
        {
            if (entitySet == null)
            {
                throw Error.ArgumentNull("entitySet");
            }

            EntitySetBase = entitySet;
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
        public IEdmEntitySetBase EntitySetBase
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
            if (EntitySetBase != null)
            {
                return EntitySetBase.EntityType().GetCollection();
            }

            return null;
        }

        /// <inheritdoc/>
        public override IEdmNavigationSource GetNavigationSource(IEdmNavigationSource previousNavigationSource)
        {
            return EntitySetBase;
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

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            if (pathSegment.SegmentKind == ODataSegmentKinds.EntitySet)
            {
                EntitySetPathSegment entitySetSegment = (EntitySetPathSegment)pathSegment;
                return entitySetSegment.EntitySetBase == EntitySetBase && entitySetSegment.EntitySetName == EntitySetName;
            }

            return false;
        }
    }
}
