// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
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

        /// <inheritdoc/>
        public override IEdmType GetEdmType(IEdmType previousEdmType)
        {
            return null;
        }

        /// <inheritdoc/>
        public override IEdmNavigationSource GetNavigationSource(IEdmNavigationSource previousNavigationSource)
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

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            return false;
        }
    }
}
