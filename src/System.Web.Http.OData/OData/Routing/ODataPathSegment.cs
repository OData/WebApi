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
        /// Initializes a new instance of the <see cref="ODataPathSegment" /> class for a segment at the root of the path.
        /// </summary>
        protected ODataPathSegment()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathSegment" /> class.
        /// </summary>
        /// <param name="previous">The previous segment in the path.</param>
        protected ODataPathSegment(ODataPathSegment previous)
        {
            if (previous == null)
            {
                throw Error.ArgumentNull("previous");
            }

            Previous = previous;
        }

        /// <summary>
        /// Gets or sets the EDM type of the path up to the current segment.
        /// </summary>
        public IEdmType EdmType
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the entity set of the path up to the current segment.
        /// </summary>
        public IEdmEntitySet EntitySet
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the previous segment in the path.
        /// </summary>
        public ODataPathSegment Previous
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the segment kind for the current segment.
        /// </summary>
        public abstract string SegmentKind
        {
            get;
        }
    }
}
