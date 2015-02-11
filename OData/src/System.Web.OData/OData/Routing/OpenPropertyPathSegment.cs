// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing an open property.
    /// </summary>
    public class OpenPropertyPathSegment : ODataPathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenPropertyPathSegment" /> class.
        /// </summary>
        /// <param name="propertyName">The name of the open property.</param>
        public OpenPropertyPathSegment(string propertyName)
        {
            if (propertyName == null)
            {
                throw Error.ArgumentNull("propertyName");
            }

            PropertyName = propertyName;
        }

        /// <summary>
        /// Gets the name of the open property.
        /// </summary>
        public string PropertyName
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
                return ODataSegmentKinds.OpenProperty;
            }
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
            return this.PropertyName;
        }

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            return
                pathSegment.SegmentKind == ODataSegmentKinds.OpenProperty &&
                ((OpenPropertyPathSegment)pathSegment).PropertyName == this.PropertyName;
        }
    }
}
