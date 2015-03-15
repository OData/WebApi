// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing a dynamic property.
    /// </summary>
    public class DynamicPropertyPathSegment : ODataPathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicPropertyPathSegment" /> class.
        /// </summary>
        /// <param name="propertyName">The name of the dynamic property.</param>
        public DynamicPropertyPathSegment(string propertyName)
        {
            if (propertyName == null)
            {
                throw Error.ArgumentNull("propertyName");
            }

            PropertyName = propertyName;
        }

        /// <summary>
        /// Gets the name of the dynamic property.
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
                return ODataSegmentKinds.DynamicProperty;
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
            return PropertyName;
        }

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            return
                pathSegment.SegmentKind == ODataSegmentKinds.DynamicProperty &&
                ((DynamicPropertyPathSegment)pathSegment).PropertyName == PropertyName;
        }
    }
}
