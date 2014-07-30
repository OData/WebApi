// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
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

        /// <inheritdoc/>
        public override IEdmType GetEdmType(IEdmType previousEdmType)
        {
            if (NavigationProperty != null)
            {
                return NavigationProperty.Type.Definition;
            }

            return null;
        }

        /// <inheritdoc/>
        public override IEdmNavigationSource GetNavigationSource(IEdmNavigationSource previousNavigationSource)
        {
            if (NavigationProperty != null && previousNavigationSource != null)
            {
                return previousNavigationSource.FindNavigationTarget(NavigationProperty);
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

        /// <inheritdoc/>
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            if (pathSegment.SegmentKind == ODataSegmentKinds.Navigation)
            {
                NavigationPathSegment navigationSegment = (NavigationPathSegment)pathSegment;
                return navigationSegment.NavigationProperty == NavigationProperty
                    && navigationSegment.NavigationPropertyName == NavigationPropertyName;
            }

            return false;
        }
    }
}
