// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing a $ref segment.
    /// </summary>
    // TODO: 1681 enforce $ref and $value be the last segment.
    public class RefPathSegment : ODataPathSegment
    {
        /// <inheritdoc/>
        public override string SegmentKind
        {
            get
            {
                return ODataSegmentKinds.Ref;
            }
        }

        /// <inheritdoc/>
        public override IEdmType GetEdmType(IEdmType previousEdmType)
        {
            // $ref is the last path segment.  It uses previous segment's EDM type.
            return previousEdmType;
        }

        /// <inheritdoc/>
        public override IEdmNavigationSource GetNavigationSource(IEdmNavigationSource previousNavigationSource)
        {
            // $ref is the last path segment.  It uses previous segment's navigation source.
            return previousNavigationSource;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ODataSegmentKinds.Ref;
        }

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            return pathSegment.SegmentKind == ODataSegmentKinds.Ref;
        }
    }
}
