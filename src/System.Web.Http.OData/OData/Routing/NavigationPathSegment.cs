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
        /// <param name="previous">The property being accessed by this segment.</param>
        /// <param name="navigationProperty">The navigation property being accessed by this segment.</param>
        public NavigationPathSegment(ODataPathSegment previous, IEdmNavigationProperty navigationProperty)
            : base(previous)
        {
            if (navigationProperty == null)
            {
                throw Error.ArgumentNull("navigation");
            }

            if (previous.EntitySet == null)
            {
                throw Error.Argument(SRResources.PreviousSegmentMustHaveEntitySet);
            }

            EdmType = navigationProperty.Partner.Multiplicity() == EdmMultiplicity.Many ?
                (IEdmType)navigationProperty.ToEntityType().GetCollection() :
                (IEdmType)navigationProperty.ToEntityType();
            EntitySet = previous.EntitySet.FindNavigationTarget(navigationProperty);
            NavigationProperty = navigationProperty;
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return NavigationProperty.Name;
        }
    }
}
