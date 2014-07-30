// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// Provides an object representation for an OData path segment with additional information about the EDM type and entity set for the path.
    /// </summary>
    public abstract class ODataPathSegment : ODataPathSegmentTemplate
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
        /// Gets the navigation source for this segment.
        /// </summary>
        /// <param name="previousNavigationSource">The navigation source of the previous path segment.</param>
        /// <returns>The navigation source for this segment.</returns>
        public abstract IEdmNavigationSource GetNavigationSource(IEdmNavigationSource previousNavigationSource);
    }
}
