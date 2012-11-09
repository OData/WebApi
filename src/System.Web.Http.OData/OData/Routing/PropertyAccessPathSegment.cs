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
        /// <param name="previous">The previous segment in the path.</param>
        /// <param name="property">The property being accessed by this segment.</param>
        public PropertyAccessPathSegment(ODataPathSegment previous, IEdmProperty property)
            : base(previous)
        {
            if (property == null)
            {
                throw Error.ArgumentNull("property");
            }

            EdmType = property.Type.Definition;
            Property = property;
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Property.Name;
        }
    }
}
