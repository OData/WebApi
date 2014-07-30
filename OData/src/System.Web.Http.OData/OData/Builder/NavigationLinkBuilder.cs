// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Encapsulates a navigation link factory and whether the link factory follows conventions or not.
    /// </summary>
    public class NavigationLinkBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationLinkBuilder"/> class.
        /// </summary>
        /// <param name="navigationLinkFactory">The navigation link factory for creating navigation links.</param>
        /// <param name="followsConventions">Represents whether this factory follows OData conventions or not.</param>
        public NavigationLinkBuilder(Func<EntityInstanceContext, IEdmNavigationProperty, Uri> navigationLinkFactory, bool followsConventions)
        {
            if (navigationLinkFactory == null)
            {
                throw Error.ArgumentNull("navigationLinkFactory");
            }

            Factory = navigationLinkFactory;
            FollowsConventions = followsConventions;
        }

        /// <summary>
        /// Gets the navigation link factory for creating navigation links.
        /// </summary>
        public Func<EntityInstanceContext, IEdmNavigationProperty, Uri> Factory { get; private set; }

        /// <summary>
        /// Gets a value representing whether this factory follows OData conventions or not.
        /// </summary>
        public bool FollowsConventions { get; private set; }
    }
}
