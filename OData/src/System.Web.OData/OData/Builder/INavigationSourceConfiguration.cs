// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.OData.Properties;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Represents a navigation source
    /// </summary>
    public interface INavigationSourceConfiguration
    {
        /// <summary>
        /// Gets the navigation targets of <see cref=" NavigationSourceConfiguration"/>.
        /// </summary>
        IEnumerable<NavigationPropertyBindingConfiguration> Bindings { get; }

        /// <summary>
        /// Gets the entity type contained in this navigation source.
        /// </summary>
        EntityTypeConfiguration EntityType { get; }

        /// <summary>
        /// Gets the backing <see cref="Type"/> for the entity type contained in this navigation source.
        /// </summary>
        Type ClrType { get; }

        /// <summary>
        /// Gets the name of this navigation source.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Configures the navigation source URL.
        /// </summary>
        /// <param name="url">The navigation source URL.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            MessageId = "0#", Justification = "This Url property is not required to be a valid Uri")]
        INavigationSourceConfiguration HasUrl(string url);

        /// <summary>
        /// Configures the edit link for this navigation source.
        /// </summary>
        /// <param name="editLinkBuilder">The builder used to generate the edit link.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        INavigationSourceConfiguration HasEditLink(SelfLinkBuilder<Uri> editLinkBuilder);

        /// <summary>
        /// Configures the read link for this navigation source.
        /// </summary>
        /// <param name="readLinkBuilder">The builder used to generate the read link.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        INavigationSourceConfiguration HasReadLink(SelfLinkBuilder<Uri> readLinkBuilder);

        /// <summary>
        /// Configures the ID link for this navigation source.
        /// </summary>
        /// <param name="idLinkBuilder">The builder used to generate the ID.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        INavigationSourceConfiguration HasIdLink(SelfLinkBuilder<Uri> idLinkBuilder);

        /// <summary>
        /// Configures the navigation link for the given navigation property for this navigation source.
        /// </summary>
        /// <param name="navigationProperty">The navigation property for which the navigation link is being generated.</param>
        /// <param name="navigationLinkBuilder">The builder used to generate the navigation link.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        INavigationSourceConfiguration HasNavigationPropertyLink(NavigationPropertyConfiguration navigationProperty,
            NavigationLinkBuilder navigationLinkBuilder);

        /// <summary>
        /// Configures the navigation link for the given navigation properties for this navigation source.
        /// </summary>
        /// <param name="navigationProperties">The navigation properties for which the navigation link is being generated.</param>
        /// <param name="navigationLinkBuilder">The builder used to generate the navigation link.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        INavigationSourceConfiguration HasNavigationPropertiesLink(
            IEnumerable<NavigationPropertyConfiguration> navigationProperties, NavigationLinkBuilder navigationLinkBuilder);

        /// <summary>
        /// Binds the given navigation property to the target navigation source.
        /// </summary>
        /// <param name="navigationConfiguration">The navigation property.</param>
        /// <param name="targetNavigationSource">The target navigation source.</param>
        /// <returns>The <see cref="NavigationPropertyBindingConfiguration"/> so that it can be further configured.</returns>
        NavigationPropertyBindingConfiguration AddBinding(NavigationPropertyConfiguration navigationConfiguration,
            INavigationSourceConfiguration targetNavigationSource);

        /// <summary>
        /// Removes the binding for the given navigation property.
        /// </summary>
        /// <param name="navigationConfiguration">The navigation property</param>
        void RemoveBinding(NavigationPropertyConfiguration navigationConfiguration);

        /// <summary>
        /// Finds the binding for the given navigation property and tries to create it if it doesnot exist.
        /// </summary>
        /// <param name="navigationConfiguration">The navigation property.</param>
        /// <returns>The <see cref="NavigationPropertyBindingConfiguration"/> so that it can be further configured.</returns>
        NavigationPropertyBindingConfiguration FindBinding(NavigationPropertyConfiguration navigationConfiguration);

        /// <summary>
        /// Finds the binding for the given navigation property.
        /// </summary>
        /// <param name="navigationConfiguration">The navigation property.</param>
        /// <param name="autoCreate">Tells whether the binding should be auto created if it does not exist.</param>
        /// <returns>The <see cref="NavigationPropertyBindingConfiguration"/> so that it can be further configured.</returns>
        NavigationPropertyBindingConfiguration FindBinding(NavigationPropertyConfiguration navigationConfiguration,
            bool autoCreate);

        /// <summary>
        /// Gets the navigation source URL.
        /// </summary>
        /// <returns>The navigation source URL.</returns>
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "This Url property is not required to be a valid Uri")]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Consistent with EF Has/Get pattern")]
        string GetUrl();

        /// <summary>
        /// Gets the builder used to generate edit links for this navigation source.
        /// </summary>
        /// <returns>The link builder.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Consistent with EF Has/Get pattern")]
        SelfLinkBuilder<Uri> GetEditLink();

        /// <summary>
        /// Gets the builder used to generate read links for this navigation source.
        /// </summary>
        /// <returns>The link builder.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Consistent with EF Has/Get pattern")]
        SelfLinkBuilder<Uri> GetReadLink();

        /// <summary>
        /// Gets the builder used to generate ID for this navigation source.
        /// </summary>
        /// <returns>The builder.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Consistent with EF Has/Get pattern")]
        SelfLinkBuilder<Uri> GetIdLink();

        /// <summary>
        /// Gets the builder used to generate navigation link for the given navigation property for this navigation source.
        /// </summary>
        /// <param name="navigationProperty">The navigation property.</param>
        /// <returns>The link builder.</returns>
        NavigationLinkBuilder GetNavigationPropertyLink(NavigationPropertyConfiguration navigationProperty);

        /// <summary>
        /// Gets the <see cref="NavigationPropertyBindingConfiguration"/> for the navigation property with the given name.
        /// </summary>
        /// <param name="propertyName">The name of the navigation property.</param>
        /// <returns>The <see cref="NavigationPropertyBindingConfiguration" />.</returns>
        NavigationPropertyBindingConfiguration FindBinding(string propertyName);
    }
}
