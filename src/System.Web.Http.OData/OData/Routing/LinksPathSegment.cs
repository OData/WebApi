// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing a $links segment.
    /// </summary>
    public class LinksPathSegment : ODataPathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinksPathSegment" /> class.
        /// </summary>
        /// <param name="previous">The previous segment in the path.</param>
        public LinksPathSegment(ODataPathSegment previous)
            : base(previous)
        {
            EdmType = previous.EdmType;
            EntitySet = previous.EntitySet;
        }

        /// <summary>
        /// Gets the segment kind for the current segment.
        /// </summary>
        public override string SegmentKind
        {
            get
            {
                return ODataSegmentKinds.Links;
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
            return ODataSegmentKinds.Links;
        }
    }
}
