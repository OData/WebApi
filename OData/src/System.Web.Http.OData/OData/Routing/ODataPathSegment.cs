// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// Provides an object representation for an OData path segment with additional information about the EDM type and entity set for the path.
    /// </summary>
    public abstract class ODataPathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathSegment" /> class.
        /// </summary>
        protected ODataPathSegment()
        {
        }

        /// <summary>
        /// Gets the segment kind for the current segment.
        /// </summary>
        public abstract string SegmentKind
        {
            get;
        }

        /// <summary>
        /// Gets the EDM type for this segment.
        /// </summary>
        /// <param name="previousEdmType">The EDM type of the previous path segment.</param>
        /// <returns>The EDM type for this segment.</returns>
        public abstract IEdmType GetEdmType(IEdmType previousEdmType);

        /// <summary>
        /// Gets the entity set for this segment.
        /// </summary>
        /// <param name="previousEntitySet">The entity set of the previous path segment.</param>
        /// <returns>The entity set for this segment.</returns>
        public abstract IEdmEntitySet GetEntitySet(IEdmEntitySet previousEntitySet);
    }
}
