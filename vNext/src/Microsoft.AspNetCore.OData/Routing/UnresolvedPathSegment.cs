// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing
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
        public virtual string SegmentKind
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

        /// <summary>
        /// Translate a <see cref="UnresolvedPathSegment" /> an implementation of <see cref="PathSegmentTranslator{T}" />
        /// </summary>
        /// <typeparam name="T">Type that the translator will return after visiting this token.</typeparam>
        /// <param name="translator">An implementation of the translator interface.</param>
        /// <returns>An object whose type is determined by the type parameter of the translator.</returns>
        public override T TranslateWith<T>(PathSegmentTranslator<T> translator)
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)SegmentValue;
            }

            return default(T);
        }

        /// <summary>
        /// Handle a <see cref="UnresolvedPathSegment" /> an implementation of <see cref="PathSegmentHandler" />
        /// </summary>
        /// <param name="handler">An implementation of the handler interface.</param>
        public override void HandleWith(PathSegmentHandler handler)
        {
        }

        /// <summary>
        /// Gets the <see cref="IEdmType" /> of this <see cref="UnresolvedPathSegment" />.
        /// </summary>
        public override IEdmType EdmType
        {
            get { return null; }
        }
    }
}
