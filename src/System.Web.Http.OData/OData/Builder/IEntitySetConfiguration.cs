// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    public interface IEntitySetConfiguration
    {
        string Name { get; }

        IEntityTypeConfiguration EntityType { get; }

        IEnumerable<NavigationPropertyBinding> Bindings { get; }

        NavigationPropertyBinding AddBinding(NavigationPropertyConfiguration navigationConfiguration, IEntitySetConfiguration targetEntitySet);

        void RemoveBinding(NavigationPropertyConfiguration navigationConfiguration);

        NavigationPropertyBinding FindBinding(NavigationPropertyConfiguration navigationConfiguration);

        NavigationPropertyBinding FindBinding(NavigationPropertyConfiguration navigationConfiguration, bool autoCreate);

        NavigationPropertyBinding FindBinding(string propertyName);

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "This Url property is not required to be a valid Uri")]
        IEntitySetConfiguration HasUrl(string url);

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "This Url property is not required to be a valid Uri")]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Consistent with EF Has/Get pattern")]
        string GetUrl();

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Consistent with EF Has/Get pattern")]
        Func<FeedContext, Uri> GetFeedSelfLink();

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Consistent with EF Has/Get pattern")]
        Func<EntityInstanceContext, Uri> GetReadLink();

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Consistent with EF Has/Get pattern")]
        Func<EntityInstanceContext, Uri> GetEditLink();

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Consistent with EF Has/Get pattern")]
        Func<EntityInstanceContext, string> GetIdLink();

        Func<EntityInstanceContext, IEdmNavigationProperty, Uri> GetNavigationPropertyLink(NavigationPropertyConfiguration navigationProperty);

        /// <summary>
        /// Adds a self link to the feed.
        /// </summary>
        /// <param name="feedSelfLinkFactory">The builder used to generate the link URL.</param>
        /// <returns>The entity set configuration currently being configured.</returns>
        IEntitySetConfiguration HasFeedSelfLink(Func<FeedContext, Uri> feedSelfLinkFactory);

        IEntitySetConfiguration HasEditLink(Func<EntityInstanceContext, Uri> editLinkFactory);

        IEntitySetConfiguration HasReadLink(Func<EntityInstanceContext, Uri> readLinkFactory);

        IEntitySetConfiguration HasIdLink(Func<EntityInstanceContext, string> idLinkFactory);

        IEntitySetConfiguration HasNavigationPropertyLink(NavigationPropertyConfiguration navigationProperty, Func<EntityInstanceContext, IEdmNavigationProperty, Uri> navigationLinkFactory);

        IEntitySetConfiguration HasNavigationPropertiesLink(IEnumerable<NavigationPropertyConfiguration> navigationProperties, Func<EntityInstanceContext, IEdmNavigationProperty, Uri> navigationLinkFactory);
    }
}
