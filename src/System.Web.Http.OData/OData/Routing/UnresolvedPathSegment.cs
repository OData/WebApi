// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing a segment that could not be resolved.
    /// </summary>
    public class UnresolvedPathSegment : ODataPathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnresolvedPathSegment" /> class.
        /// </summary>
        /// <param name="previous">The property being accessed by this segment.</param>
        /// <param name="segmentValue">The unresolved segment value.</param>
        public UnresolvedPathSegment(ODataPathSegment previous, string segmentValue)
            : base(previous)
        {
            if (segmentValue == null)
            {
                throw Error.ArgumentNull("segmentValue");
            }

            SegmentValue = segmentValue;
        }

        /// <summary>
        /// Gets the segment kind for the current segment.
        /// </summary>
        public override string SegmentKind
        {
            get
            {
                return ODataSegmentKinds.Unresolved;
            }
        }

        /// <summary>
        /// Gets the unresolved segment value.
        /// </summary>
        public string SegmentValue
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return SegmentValue;
        }
    }
}
