// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing a navigation.
    /// </summary>
    public class NavigationPathSegment : ODataPathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPathSegment" /> class.
        /// </summary>
        /// <param name="navigationProperty">The navigation property being accessed by this segment.</param>
        public NavigationPathSegment(IEdmNavigationProperty navigationProperty)
        {
            if (navigationProperty == null)
            {
                throw Error.ArgumentNull("navigation");
            }

            NavigationProperty = navigationProperty;
            NavigationPropertyName = navigationProperty.Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPathSegment" /> class.
        /// </summary>
        /// <param name="navigationPropertyName">Name of the navigation property.</param>
        public NavigationPathSegment(string navigationPropertyName)
        {
            if (navigationPropertyName == null)
            {
                throw Error.ArgumentNull("navigationPropertyName");
            }

            NavigationPropertyName = navigationPropertyName;
        }

        /// <summary>
        /// Gets the segment kind for the current segment.
        /// </summary>
        public override string SegmentKind
        {
            get
            {
                return ODataSegmentKinds.Navigation;
            }
        }

        /// <summary>
        /// Gets the navigation property being accessed by this segment.
        /// </summary>
        public IEdmNavigationProperty NavigationProperty
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the navigation property.
        /// </summary>
        public string NavigationPropertyName
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
            if (NavigationProperty != null)
            {
                return NavigationProperty.Partner.Multiplicity() == EdmMultiplicity.Many ?
                    (IEdmType)NavigationProperty.ToEntityType().GetCollection() :
                    (IEdmType)NavigationProperty.ToEntityType();
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
            if (NavigationProperty != null && previousEntitySet != null)
            {
                return previousEntitySet.FindNavigationTarget(NavigationProperty);
            }
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
            return NavigationPropertyName;
        }
    }
}
