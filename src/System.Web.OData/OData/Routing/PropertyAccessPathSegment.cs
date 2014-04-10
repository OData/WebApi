// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing a property access.
    /// </summary>
    public class PropertyAccessPathSegment : ODataPathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAccessPathSegment" /> class.
        /// </summary>
        /// <param name="property">The property being accessed by this segment.</param>
        public PropertyAccessPathSegment(IEdmProperty property)
        {
            if (property == null)
            {
                throw Error.ArgumentNull("property");
            }

            Property = property;
            PropertyName = property.Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAccessPathSegment" /> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public PropertyAccessPathSegment(string propertyName)
        {
            if (propertyName == null)
            {
                throw Error.ArgumentNull("propertyName");
            }

            PropertyName = propertyName;
        }

        /// <summary>
        /// Gets the segment kind for the current segment.
        /// </summary>
        public override string SegmentKind
        {
            get
            {
                return ODataSegmentKinds.Property;
            }
        }

        /// <summary>
        /// Gets the property property being accessed by this segment.
        /// </summary>
        public IEdmProperty Property
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string PropertyName
        {
            get;
            private set;
        }

        /// <inheritdoc/>
        public override IEdmType GetEdmType(IEdmType previousEdmType)
        {
            if (Property != null)
            {
                return Property.Type.Definition;
            }
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
            if (pathSegment.SegmentKind == ODataSegmentKinds.Property)
            {
                PropertyAccessPathSegment propertySegment = (PropertyAccessPathSegment)pathSegment;
                return propertySegment.Property == Property && propertySegment.PropertyName == PropertyName;
            }

            return false;
        }
    }
}
