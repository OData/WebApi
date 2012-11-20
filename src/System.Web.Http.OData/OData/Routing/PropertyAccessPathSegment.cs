// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Routing
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

        /// <summary>
        /// Gets the EDM type for this segment.
        /// </summary>
        /// <param name="previousEdmType">The EDM type of the previous path segment.</param>
        /// <returns>
        /// The EDM type for this segment.
        /// </returns>
        public override IEdmType GetEdmType(IEdmType previousEdmType)
        {
            if (Property != null)
            {
                return Property.Type.Definition;
            }
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
            return PropertyName;
        }
    }
}
