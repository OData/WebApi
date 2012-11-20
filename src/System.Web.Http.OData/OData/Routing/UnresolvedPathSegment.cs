// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;
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
        /// <param name="segmentValue">The unresolved segment value.</param>
        public UnresolvedPathSegment(string segmentValue)
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
        /// Gets the EDM type for this segment.
        /// </summary>
        /// <param name="previousEdmType">The EDM type of the previous path segment.</param>
        /// <returns>
        /// The EDM type for this segment.
        /// </returns>
        public override IEdmType GetEdmType(IEdmType previousEdmType)
        {
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
            return SegmentValue;
        }
    }
}
