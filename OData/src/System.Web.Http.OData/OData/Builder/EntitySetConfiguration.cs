// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http.OData.Properties;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Allows configuration to be performed for a entity set in a model.
    /// A <see cref="EntitySetConfiguration"/> can be obtained by using the method <see cref="ODataModelBuilder.EntitySet"/>.
    /// </summary>
    public class EntitySetConfiguration
    {
        private readonly ODataModelBuilder _modelBuilder;
        private readonly Dictionary<NavigationPropertyConfiguration, NavigationPropertyBindingConfiguration> _entitySetBindings;

        private string _url;
        private Func<FeedContext, Uri> _feedSelfLinkFactory;

        private SelfLinkBuilder<Uri> _editLinkBuilder;
        private SelfLinkBuilder<Uri> _readLinkBuilder;
        private SelfLinkBuilder<string> _idLinkBuilder;

        private readonly Dictionary<NavigationPropertyConfiguration, NavigationLinkBuilder> _navigationPropertyLinkBuilders;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetConfiguration"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public EntitySetConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetConfiguration"/> class.
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder"/>.</param>
        /// <param name="entityType">The CLR <see cref="Type"/> of the entity type contained in this entity set.</param>
        /// <param name="name">The name of the entity set.</param>
        /// </summary>
        public EntitySetConfiguration(ODataModelBuilder modelBuilder, Type entityType, string name)
            : this(modelBuilder, new EntityTypeConfiguration(modelBuilder, entityType), name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetConfiguration"/> class.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder"/>.</param>
        /// <param name="entityType">The entity type contained in this entity set.</param>
        /// <param name="name">The name of the entity set.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The property being set, ClrType, is on a different object.")]
        public EntitySetConfiguration(ODataModelBuilder modelBuilder, EntityTypeConfiguration entityType, string name)
        {
            if (modelBuilder == null)
            {
                throw Error.ArgumentNull("modelBuilder");
            }

            if (entityType == null)
            {
                throw Error.ArgumentNull("entityType");
            }

            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }

            _modelBuilder = modelBuilder;
            Name = name;
            EntityType = entityType;
            ClrType = entityType.ClrType;
            _url = Name;

            _editLinkBuilder = null;
            _readLinkBuilder = null;
            _navigationPropertyLinkBuilders = new Dictionary<NavigationPropertyConfiguration, NavigationLinkBuilder>();
            _entitySetBindings = new Dictionary<NavigationPropertyConfiguration, NavigationPropertyBindingConfiguration>();
        }

        /// <summary>
        /// Gets the navigation targets of this entity set.
        /// </summary>
        public virtual IEnumerable<NavigationPropertyBindingConfiguration> Bindings
        {
            get
            {
                return _entitySetBindings.Values;
            }
        }

        /// <summary>
        /// Gets the entity type contained in this entity set.
        /// </summary>
        public virtual EntityTypeConfiguration EntityType { get; private set; }

        /// <summary>
        /// Gets the backing clr type for the entity type contained in this entity set.
        /// </summary>
        public virtual Type ClrType { get; private set; }

        /// <summary>
        /// Gets the name of this entity set.
        /// </summary>
        public virtual string Name { get; private set; }

        /// <summary>
        /// Configures the entity set URL.
        /// </summary>
        /// <param name="url">The entity set URL.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "This Url property is not required to be a valid Uri")]
        public virtual EntitySetConfiguration HasUrl(string url)
        {
            _url = url;
            return this;
        }

        /// <summary>
        /// Adds a self link to the feed.
        /// </summary>
        /// <param name="feedSelfLinkFactory">The builder used to generate the link URL.</param>
        /// <returns>The entity set configuration currently being configured.</returns>
        public virtual EntitySetConfiguration HasFeedSelfLink(Func<FeedContext, Uri> feedSelfLinkFactory)
        {
            _feedSelfLinkFactory = feedSelfLinkFactory;
            return this;
        }

        /// <summary>
        /// Configures the edit link for the entities from this entity set.
        /// </summary>
        /// <param name="editLinkBuilder">The builder used to generate the edit link.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public virtual EntitySetConfiguration HasEditLink(SelfLinkBuilder<Uri> editLinkBuilder)
        {
            if (editLinkBuilder == null)
            {
                throw Error.ArgumentNull("editLinkBuilder");
            }

            _editLinkBuilder = editLinkBuilder;
            return this;
        }

        /// <summary>
        /// Configures the read link for the entities from this entity set.
        /// </summary>
        /// <param name="readLinkBuilder">The builder used to generate the read link.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public virtual EntitySetConfiguration HasReadLink(SelfLinkBuilder<Uri> readLinkBuilder)
        {
            if (readLinkBuilder == null)
            {
                throw Error.ArgumentNull("readLinkBuilder");
            }

            _readLinkBuilder = readLinkBuilder;
            return this;
        }

        /// <summary>
        /// Configures the ID for the entities from this entity set.
        /// </summary>
        /// <param name="idLinkBuilder">The builder used to generate the ID.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public virtual EntitySetConfiguration HasIdLink(SelfLinkBuilder<string> idLinkBuilder)
        {
            if (idLinkBuilder == null)
            {
                throw Error.ArgumentNull("idLinkBuilder");
            }

            _idLinkBuilder = idLinkBuilder;
            return this;
        }

        /// <summary>
        /// Configures the navigation link for the given navigation property for entities from this entity set.
        /// </summary>
        /// <param name="navigationProperty">The navigation property for which the navigation link is being generated.</param>
        /// <param name="navigationLinkBuilder">The builder used to generate the navigation link.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public virtual EntitySetConfiguration HasNavigationPropertyLink(NavigationPropertyConfiguration navigationProperty, NavigationLinkBuilder navigationLinkBuilder)
        {
            if (navigationProperty == null)
            {
                throw Error.ArgumentNull("navigationProperty");
            }

            if (navigationLinkBuilder == null)
            {
                throw Error.ArgumentNull("navigationLinkBuilder");
            }

            EntityTypeConfiguration declaringEntityType = navigationProperty.DeclaringEntityType;
            if (!(declaringEntityType.IsAssignableFrom(EntityType) || EntityType.IsAssignableFrom(declaringEntityType)))
            {
                throw Error.Argument("navigationProperty", SRResources.NavigationPropertyNotInHierarchy, declaringEntityType.FullName, EntityType.FullName, Name);
            }

            _navigationPropertyLinkBuilders[navigationProperty] = navigationLinkBuilder;
            return this;
        }

        /// <summary>
        /// Configures the navigation link for the given navigation properties for entities from this entity set.
        /// </summary>
        /// <param name="navigationProperties">The navigation properties for which the navigation link is being generated.</param>
        /// <param name="navigationLinkBuilder">The builder used to generate the navigation link.</param>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public virtual EntitySetConfiguration HasNavigationPropertiesLink(IEnumerable<NavigationPropertyConfiguration> navigationProperties, NavigationLinkBuilder navigationLinkBuilder)
        {
            if (navigationProperties == null)
            {
                throw Error.ArgumentNull("navigationProperties");
            }

            if (navigationLinkBuilder == null)
            {
                throw Error.ArgumentNull("navigationLinkBuilder");
            }

            foreach (NavigationPropertyConfiguration navigationProperty in navigationProperties)
            {
                HasNavigationPropertyLink(navigationProperty, navigationLinkBuilder);
            }

            return this;
        }

        /// <summary>
        /// Binds the given navigation property to the target entity set.
        /// </summary>
        /// <param name="navigationConfiguration">The navigation property.</param>
        /// <param name="targetEntitySet">The target entity set.</param>
        /// <returns>The <see cref="NavigationPropertyBindingConfiguration"/> so that it can be further configured.</returns>
        public virtual NavigationPropertyBindingConfiguration AddBinding(NavigationPropertyConfiguration navigationConfiguration, EntitySetConfiguration targetEntitySet)
        {
            if (navigationConfiguration == null)
            {
                throw Error.ArgumentNull("navigationConfiguration");
            }

            if (targetEntitySet == null)
            {
                throw Error.ArgumentNull("targetEntitySet");
            }

            EntityTypeConfiguration declaringEntityType = navigationConfiguration.DeclaringEntityType;
            if (!(declaringEntityType.IsAssignableFrom(EntityType) || EntityType.IsAssignableFrom(declaringEntityType)))
            {
                throw Error.Argument("navigationConfiguration", SRResources.NavigationPropertyNotInHierarchy, declaringEntityType.FullName, EntityType.FullName, Name);
            }

            NavigationPropertyBindingConfiguration navigationPropertyBinding = null;
            if (_entitySetBindings.ContainsKey(navigationConfiguration))
            {
                navigationPropertyBinding = _entitySetBindings[navigationConfiguration];
                if (navigationPropertyBinding.EntitySet != targetEntitySet)
                {
                    throw Error.NotSupported(SRResources.RebindingNotSupported);
                }
            }
            else
            {
                navigationPropertyBinding = new NavigationPropertyBindingConfiguration(navigationConfiguration, targetEntitySet);
                _entitySetBindings[navigationConfiguration] = navigationPropertyBinding;
            }
            return navigationPropertyBinding;
        }

        /// <summary>
        /// Removes the binding for the given navigation property.
        /// </summary>
        /// <param name="navigationConfiguration">The navigation property</param>
        public virtual void RemoveBinding(NavigationPropertyConfiguration navigationConfiguration)
        {
            if (_entitySetBindings.ContainsKey(navigationConfiguration))
            {
                _entitySetBindings.Remove(navigationConfiguration);
            }
        }

        /// <summary>
        /// Finds the binding for the given navigation property and tries to create it if it doesnot exist.
        /// </summary>
        /// <param name="navigationConfiguration">The navigation property.</param>
        /// <returns>The <see cref="NavigationPropertyBindingConfiguration"/> so that it can be further configured.</returns>
        public virtual NavigationPropertyBindingConfiguration FindBinding(NavigationPropertyConfiguration navigationConfiguration)
        {
            return FindBinding(navigationConfiguration, autoCreate: true);
        }

        /// <summary>
        /// Finds the binding for the given navigation property.
        /// </summary>
        /// <param name="autoCreate">Tells whether the binding should be auto created if it does not exist.</param>
        /// <param name="navigationConfiguration">The navigation property.</param>
        /// <returns>The <see cref="NavigationPropertyBindingConfiguration"/> so that it can be further configured.</returns>
        public virtual NavigationPropertyBindingConfiguration FindBinding(NavigationPropertyConfiguration navigationConfiguration, bool autoCreate)
        {
            if (navigationConfiguration == null)
            {
                throw Error.ArgumentNull("navigationConfiguration");
            }

            if (_entitySetBindings.ContainsKey(navigationConfiguration))
            {
                return _entitySetBindings[navigationConfiguration];
            }

            if (!autoCreate)
            {
                return null;
            }

            Type entityType = navigationConfiguration.RelatedClrType;
            EntitySetConfiguration[] matchingSets = _modelBuilder.EntitySets.Where(es => es.EntityType.ClrType == entityType).ToArray();
            if (matchingSets.Count() == 1)
            {
                return AddBinding(navigationConfiguration, matchingSets[0]);
            }
            else if (!matchingSets.Any())
            {
                return null;
            }
            else
            {
                throw Error.NotSupported(
                    SRResources.CannotAutoCreateMultipleCandidates,
                    navigationConfiguration.Name,
                    navigationConfiguration.DeclaringEntityType.FullName,
                    Name,
                    String.Join(", ", matchingSets.Select(entitySet => entitySet.Name)));
            }
        }

        /// <summary>
        /// Gets the entity set URL.
        /// </summary>
        /// <returns>The entity set URL.</returns>
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "This Url property is not required to be a valid Uri")]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Consistent with EF Has/Get pattern")]
        public virtual string GetUrl()
        {
            return _url;
        }

        /// <summary>
        /// Gets the builder used to generate self links for feeds for this entity set.
        /// </summary>
        /// <returns>The link builder.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Consistent with EF Has/Get pattern")]
        public virtual Func<FeedContext, Uri> GetFeedSelfLink()
        {
            return _feedSelfLinkFactory;
        }

        /// <summary>
        /// Gets the builder used to generate edit links for entries from this entity set.
        /// </summary>
        /// <returns>The link builder.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Consistent with EF Has/Get pattern")]
        public virtual SelfLinkBuilder<Uri> GetEditLink()
        {
            return _editLinkBuilder;
        }

        /// <summary>
        /// Gets the builder used to generate read links for entries from this entity set.
        /// </summary>
        /// <returns>The link builder.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Consistent with EF Has/Get pattern")]
        public virtual SelfLinkBuilder<Uri> GetReadLink()
        {
            return _readLinkBuilder;
        }

        /// <summary>
        /// Gets the builder used to generate ID for entries from this entity set.
        /// </summary>
        /// <returns>The builder.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Consistent with EF Has/Get pattern")]
        public virtual SelfLinkBuilder<string> GetIdLink()
        {
            return _idLinkBuilder;
        }

        /// <summary>
        /// Gets the builder used to generate navigation link for the given navigation property for entries from this entity set.
        /// </summary>
        /// <param name="navigationProperty">The navigation property.</param>
        /// <returns>The link builder.</returns>
        public virtual NavigationLinkBuilder GetNavigationPropertyLink(NavigationPropertyConfiguration navigationProperty)
        {
            if (navigationProperty == null)
            {
                throw Error.ArgumentNull("navigationProperty");
            }

            NavigationLinkBuilder navigationPropertyLinkBuilder;
            _navigationPropertyLinkBuilders.TryGetValue(navigationProperty, out navigationPropertyLinkBuilder);
            return navigationPropertyLinkBuilder;
        }

        /// <summary>
        /// Gets the <see cref="NavigationPropertyBindingConfiguration"/> for the navigation property with the given name.
        /// </summary>
        /// <param name="propertyName">The name of the navigation property.</param>
        /// <returns>The <see cref="NavigationPropertyBindingConfiguration" />.</returns>
        public virtual NavigationPropertyBindingConfiguration FindBinding(string propertyName)
        {
            return Bindings.Single(b => b.NavigationProperty.Name == propertyName);
        }
    }
}
