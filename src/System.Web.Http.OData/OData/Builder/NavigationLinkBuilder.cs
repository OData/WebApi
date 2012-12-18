// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    public class NavigationLinkBuilder
    {
        public NavigationLinkBuilder(Func<EntityInstanceContext, IEdmNavigationProperty, Uri> navigationLinkFactory, bool followsConventions)
        {
            if (navigationLinkFactory == null)
            {
                throw Error.ArgumentNull("navigationLinkFactory");
            }

            Factory = navigationLinkFactory;
            FollowsConventions = followsConventions;
        }

        public Func<EntityInstanceContext, IEdmNavigationProperty, Uri> Factory { get; private set; }

        public bool FollowsConventions { get; private set; }
    }
}
