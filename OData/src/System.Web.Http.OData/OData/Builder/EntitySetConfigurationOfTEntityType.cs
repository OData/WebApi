// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="IEdmEntitySet"/> that can be built using <see cref="ODataModelBuilder"/>.
    /// <typeparam name="TEntityType">The element type of the entity set.</typeparam>
    /// </summary>
    public class EntitySetConfiguration<TEntityType> where TEntityType : class
    {
        private EntitySetConfiguration _configuration;
        private EntityTypeConfiguration<TEntityType> _entityType;
        private ODataModelBuilder _modelBuilder;

        internal EntitySetConfiguration(ODataModelBuilder modelBuilder, string name)
            : this(modelBuilder, new EntitySetConfiguration(modelBuilder, typeof(TEntityType), name))
        {
        }

        internal EntitySetConfiguration(ODataModelBuilder modelBuilder, EntitySetConfiguration configuration)
        {
            if (modelBuilder == null)
            {
                throw Error.ArgumentNull("modelBuilder");
            }

            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            _configuration = configuration;
            _modelBuilder = modelBuilder;
            _entityType = new EntityTypeConfiguration<TEntityType>(modelBuilder, _configuration.EntityType);
        }

        internal EntitySetConfiguration EntitySet
        {
            get { return _configuration; }
        }

        /// <summary>
        /// Gets the entity type contained in this entity set.
        /// </summary>
        public EntityTypeConfiguration<TEntityType> EntityType
        {
            get
            {
                return _entityType;
            }
        }

        /// <summary>
        /// Configures a many relationship from this entity type and binds the corresponding navigation property to the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target entity set type.</typeparam>
        /// <typeparam name="TDerivedEntityType">The target entity type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="entitySetName">The target entity set name for the binding. It will be created if it does not exist.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasManyBinding<TTargetType, TDerivedEntityType>(
            Expression<Func<TDerivedEntityType, IEnumerable<TTargetType>>> navigationExpression, string entitySetName)
            where TTargetType : class
            where TDerivedEntityType : class, TEntityType
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            EntityTypeConfiguration<TDerivedEntityType> derivedEntityType =
                _modelBuilder.Entity<TDerivedEntityType>().DerivesFrom<TEntityType>();

            return _configuration.AddBinding(derivedEntityType.HasMany(navigationExpression), _modelBuilder.EntitySet<TTargetType>(entitySetName)._configuration);
        }

        /// <summary>
        /// Configures a many relationship from this entity type and binds the corresponding navigation property to the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target entity set type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="entitySetName">The target entity set name for the binding. It will be created if it does not exist.</param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasManyBinding<TTargetType>(
            Expression<Func<TEntityType, IEnumerable<TTargetType>>> navigationExpression, string entitySetName)
            where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            return _configuration.AddBinding(EntityType.HasMany(navigationExpression), _modelBuilder.EntitySet<TTargetType>(entitySetName)._configuration);
        }

        /// <summary>
        /// Configures a many relationship from this entity type and binds the corresponding navigation property to the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target entity set type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetSet">The target entity set for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasManyBinding<TTargetType>(
            Expression<Func<TEntityType, IEnumerable<TTargetType>>> navigationExpression,
            EntitySetConfiguration<TTargetType> targetSet) where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (targetSet == null)
            {
                throw Error.ArgumentNull("targetSet");
            }

            return _configuration.AddBinding(EntityType.HasMany(navigationExpression), targetSet._configuration);
        }

        /// <summary>
        /// Configures a many relationship from this entity type and binds the corresponding navigation property to the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target entity set type.</typeparam>
        /// <typeparam name="TDerivedEntityType">The target entity type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetSet">The target entity set for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasManyBinding<TTargetType, TDerivedEntityType>(
            Expression<Func<TDerivedEntityType, IEnumerable<TTargetType>>> navigationExpression,
            EntitySetConfiguration<TTargetType> targetSet)
            where TTargetType : class
            where TDerivedEntityType : class, TEntityType
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (targetSet == null)
            {
                throw Error.ArgumentNull("targetSet");
            }

            EntityTypeConfiguration<TDerivedEntityType> derivedEntityType =
                _modelBuilder.Entity<TDerivedEntityType>().DerivesFrom<TEntityType>();

            return _configuration.AddBinding(derivedEntityType.HasMany(navigationExpression), targetSet._configuration);
        }

        /// <summary>
        /// Configures a required relationship from this entity type and binds the corresponding navigation property to the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target entity set type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="entitySetName">The target entity set name for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasRequiredBinding<TTargetType>(
            Expression<Func<TEntityType, TTargetType>> navigationExpression, string entitySetName)
            where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            return _configuration.AddBinding(EntityType.HasRequired(navigationExpression), _modelBuilder.EntitySet<TTargetType>(entitySetName)._configuration);
        }

        /// <summary>
        /// Configures a required relationship from this entity type and binds the corresponding navigation property to the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target entity set type.</typeparam>
        /// <typeparam name="TDerivedEntityType">The target entity type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="entitySetName">The target entity set name for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasRequiredBinding<TTargetType, TDerivedEntityType>(
            Expression<Func<TDerivedEntityType, TTargetType>> navigationExpression, string entitySetName)
            where TTargetType : class
            where TDerivedEntityType : class, TEntityType
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            EntityTypeConfiguration<TDerivedEntityType> derivedEntityType =
                _modelBuilder.Entity<TDerivedEntityType>().DerivesFrom<TEntityType>();

            return _configuration.AddBinding(derivedEntityType.HasRequired(navigationExpression), _modelBuilder.EntitySet<TTargetType>(entitySetName)._configuration);
        }

        /// <summary>
        /// Configures a required relationship from this entity type and binds the corresponding navigation property to the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target entity set type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetSet">The target entity set for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasRequiredBinding<TTargetType>(
            Expression<Func<TEntityType, TTargetType>> navigationExpression,
            EntitySetConfiguration<TTargetType> targetSet) where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (targetSet == null)
            {
                throw Error.ArgumentNull("targetSet");
            }

            return _configuration.AddBinding(EntityType.HasRequired(navigationExpression), targetSet._configuration);
        }

        /// <summary>
        /// Configures a required relationship from this entity type and binds the corresponding navigation property to the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target entity set type.</typeparam>
        /// <typeparam name="TDerivedEntityType">The target entity type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetSet">The target entity set for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasRequiredBinding<TTargetType, TDerivedEntityType>(
            Expression<Func<TDerivedEntityType, TTargetType>> navigationExpression,
            EntitySetConfiguration<TTargetType> targetSet)
            where TTargetType : class
            where TDerivedEntityType : class, TEntityType
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (targetSet == null)
            {
                throw Error.ArgumentNull("targetSet");
            }

            EntityTypeConfiguration<TDerivedEntityType> derivedEntityType =
                _modelBuilder.Entity<TDerivedEntityType>().DerivesFrom<TEntityType>();

            return _configuration.AddBinding(derivedEntityType.HasRequired(navigationExpression), targetSet._configuration);
        }

        /// <summary>
        /// Configures an optional relationship from this entity type and binds the corresponding navigation property to the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target entity set type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="entitySetName">The target entity set name for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasOptionalBinding<TTargetType>(
            Expression<Func<TEntityType, TTargetType>> navigationExpression, string entitySetName)
            where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            return _configuration.AddBinding(EntityType.HasOptional(navigationExpression), _modelBuilder.EntitySet<TTargetType>(entitySetName)._configuration);
        }

        /// <summary>
        /// Configures an optional relationship from this entity type and binds the corresponding navigation property to the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target entity set type.</typeparam>
        /// <typeparam name="TDerivedEntityType">The target entity type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="entitySetName">The target entity set name for the binding. It will be created if it does not exist.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasOptionalBinding<TTargetType, TDerivedEntityType>(
            Expression<Func<TDerivedEntityType, TTargetType>> navigationExpression, string entitySetName)
            where TTargetType : class
            where TDerivedEntityType : class, TEntityType
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            EntityTypeConfiguration<TDerivedEntityType> derivedEntityType =
                _modelBuilder.Entity<TDerivedEntityType>().DerivesFrom<TEntityType>();

            return _configuration.AddBinding(derivedEntityType.HasOptional(navigationExpression), _modelBuilder.EntitySet<TTargetType>(entitySetName)._configuration);
        }

        /// <summary>
        /// Configures an optional relationship from this entity type and binds the corresponding navigation property to the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target entity set type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetSet">The target entity set for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasOptionalBinding<TTargetType>(
            Expression<Func<TEntityType, TTargetType>> navigationExpression,
            EntitySetConfiguration<TTargetType> targetSet) where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (targetSet == null)
            {
                throw Error.ArgumentNull("targetSet");
            }

            return _configuration.AddBinding(EntityType.HasOptional(navigationExpression), targetSet._configuration);
        }

        /// <summary>
        /// Configures an optional relationship from this entity type and binds the corresponding navigation property to the given entity set.
        /// </summary>
        /// <typeparam name="TTargetType">The target entity set type.</typeparam>
        /// <typeparam name="TDerivedEntityType">The target entity type.</typeparam>
        /// <param name="navigationExpression">A lambda expression representing the navigation property for the relationship.
        /// For example, in C# <c>t => t.MyProperty</c> and in Visual Basic .NET <c>Function(t) t.MyProperty</c>.</param>
        /// <param name="targetSet">The target entity set for the binding.</param>
        /// <returns>A configuration object that can be used to further configure the relationship further.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBindingConfiguration HasOptionalBinding<TTargetType, TDerivedEntityType>(
            Expression<Func<TDerivedEntityType, TTargetType>> navigationExpression,
            EntitySetConfiguration<TTargetType> targetSet)
            where TTargetType : class
            where TDerivedEntityType : class, TEntityType
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            if (targetSet == null)
            {
                throw Error.ArgumentNull("targetSet");
            }

            EntityTypeConfiguration<TDerivedEntityType> derivedEntityType =
                _modelBuilder.Entity<TDerivedEntityType>().DerivesFrom<TEntityType>();

            return _configuration.AddBinding(derivedEntityType.HasOptional(navigationExpression), targetSet._configuration);
        }

        /// <summary>
        /// Adds a self link to the feed.
        /// </summary>
        /// <param name="feedSelfLinkFactory">The builder used to generate the link URL.</param>
        public void HasFeedSelfLink(Func<FeedContext, string> feedSelfLinkFactory)
        {
            if (feedSelfLinkFactory == null)
            {
                throw Error.ArgumentNull("feedSelfLinkFactory");
            }

            _configuration.HasFeedSelfLink(feedContext => new Uri(feedSelfLinkFactory(feedContext)));
        }

        /// <summary>
        /// Adds a self link to the feed.
        /// </summary>
        /// <param name="feedSelfLinkFactory">The builder used to generate the link URL.</param>
        public void HasFeedSelfLink(Func<FeedContext, Uri> feedSelfLinkFactory)
        {
            if (feedSelfLinkFactory == null)
            {
                throw Error.ArgumentNull("feedSelfLinkFactory");
            }

            _configuration.HasFeedSelfLink(feedSelfLinkFactory);
        }

        /// <summary>
        /// Configures the edit link for the entities from this entity set.
        /// </summary>
        /// <param name="editLinkFactory">The factory used to generate the edit link.</param>
        /// <param name="followsConventions"><see langword="true"/> if the factory follows OData edit link conventions; otherwise, <see langword="false"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public void HasEditLink(Func<EntityInstanceContext<TEntityType>, string> editLinkFactory, bool followsConventions)
        {
            if (editLinkFactory == null)
            {
                throw Error.ArgumentNull("editLinkFactory");
            }

            HasEditLink(entityInstanceContext => new Uri(editLinkFactory(entityInstanceContext)), followsConventions);
        }

        /// <summary>
        /// Configures the edit link for the entities from this entity set.
        /// </summary>
        /// <param name="editLinkFactory">The factory used to generate the edit link.</param>
        /// <param name="followsConventions"><see langword="true"/> if the factory follows OData edit link conventions; otherwise, <see langword="false"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public void HasEditLink(Func<EntityInstanceContext<TEntityType>, Uri> editLinkFactory, bool followsConventions)
        {
            if (editLinkFactory == null)
            {
                throw Error.ArgumentNull("editLinkFactory");
            }

            _configuration.HasEditLink(new SelfLinkBuilder<Uri>((entity) => editLinkFactory(UpCastEntityInstanceContext(entity)), followsConventions));
        }

        /// <summary>
        /// Configures the read link for the entities from this entity set.
        /// </summary>
        /// <param name="readLinkFactory">The factory used to generate the read link.</param>
        /// <param name="followsConventions"><see langword="true"/> if the factory follows OData read link conventions; otherwise, <see langword="false"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public void HasReadLink(Func<EntityInstanceContext<TEntityType>, string> readLinkFactory, bool followsConventions)
        {
            if (readLinkFactory == null)
            {
                throw Error.ArgumentNull("readLinkFactory");
            }

            HasReadLink(entityInstanceContext => new Uri(readLinkFactory(entityInstanceContext)), followsConventions);
        }

        /// <summary>
        /// Configures the read link for the entities from this entity set.
        /// </summary>
        /// <param name="readLinkFactory">The factory used to generate the read link.</param>
        /// <param name="followsConventions"><see langword="true"/> if the factory follows OData read link conventions; otherwise, <see langword="false"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public void HasReadLink(Func<EntityInstanceContext<TEntityType>, Uri> readLinkFactory, bool followsConventions)
        {
            if (readLinkFactory == null)
            {
                throw Error.ArgumentNull("readLinkFactory");
            }

            _configuration.HasReadLink(new SelfLinkBuilder<Uri>((entity) => readLinkFactory(UpCastEntityInstanceContext(entity)), followsConventions));
        }

        /// <summary>
        /// Configures the ID link for the entities from this entity set.
        /// </summary>
        /// <param name="idLinkFactory">The factory used to generate the ID link.</param>
        /// <param name="followsConventions"><see langword="true"/> if the factory follows OData ID link conventions; otherwise, <see langword="false"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public void HasIdLink(Func<EntityInstanceContext<TEntityType>, string> idLinkFactory, bool followsConventions)
        {
            if (idLinkFactory == null)
            {
                throw Error.ArgumentNull("idLinkFactory");
            }

            _configuration.HasIdLink(new SelfLinkBuilder<string>((entity) => idLinkFactory(UpCastEntityInstanceContext(entity)), followsConventions));
        }

        /// <summary>
        /// Configures the navigation link for the given navigation property for entities from this entity set.
        /// </summary>
        /// <param name="navigationProperty">The navigation property for which the navigation link is being generated.</param>
        /// <param name="navigationLinkFactory">The factory used to generate the navigation link.</param>
        /// <param name="followsConventions"><see langword="true"/> if the factory follows OData navigation link conventions; otherwise, <see langword="false"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public void HasNavigationPropertyLink(NavigationPropertyConfiguration navigationProperty, Func<EntityInstanceContext<TEntityType>, IEdmNavigationProperty, Uri> navigationLinkFactory, bool followsConventions)
        {
            if (navigationProperty == null)
            {
                throw Error.ArgumentNull("navigationProperty");
            }

            if (navigationLinkFactory == null)
            {
                throw Error.ArgumentNull("navigationLinkFactory");
            }

            _configuration.HasNavigationPropertyLink(navigationProperty, new NavigationLinkBuilder((entity, property) => navigationLinkFactory(UpCastEntityInstanceContext(entity), property), followsConventions));
        }

        /// <summary>
        /// Configures the navigation link for the given navigation properties for entities from this entity set.
        /// </summary>
        /// <param name="navigationProperties">The navigation properties for which the navigation link is being generated.</param>
        /// <param name="navigationLinkFactory">The factory used to generate the navigation link.</param>
        /// <param name="followsConventions"><see langword="true"/> if the factory follows OData navigation link conventions; otherwise, <see langword="false"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public void HasNavigationPropertiesLink(IEnumerable<NavigationPropertyConfiguration> navigationProperties, Func<EntityInstanceContext<TEntityType>, IEdmNavigationProperty, Uri> navigationLinkFactory, bool followsConventions)
        {
            if (navigationProperties == null)
            {
                throw Error.ArgumentNull("navigationProperties");
            }

            if (navigationLinkFactory == null)
            {
                throw Error.ArgumentNull("navigationLinkFactory");
            }

            _configuration.HasNavigationPropertiesLink(navigationProperties, new NavigationLinkBuilder((entity, property) => navigationLinkFactory(UpCastEntityInstanceContext(entity), property), followsConventions));
        }

        /// <summary>
        /// Finds the <see cref="NavigationPropertyBindingConfiguration"/> for the navigation property with the given name.
        /// </summary>
        /// <param name="propertyName">The name of the navigation property.</param>
        /// <returns>The binding, if found; otherwise, <see langword="null"/>.</returns>
        public NavigationPropertyBindingConfiguration FindBinding(string propertyName)
        {
            return _configuration.FindBinding(propertyName);
        }

        /// <summary>
        /// Finds the <see cref="NavigationPropertyBindingConfiguration"/> for the given navigation property and creates it if it does not exist.
        /// </summary>
        /// <param name="navigationConfiguration">The navigation property.</param>
        /// <returns>The binding if found else the created binding.</returns>
        public NavigationPropertyBindingConfiguration FindBinding(NavigationPropertyConfiguration navigationConfiguration)
        {
            return _configuration.FindBinding(navigationConfiguration, autoCreate: true);
        }

        /// <summary>
        /// Finds the <see cref="NavigationPropertyBindingConfiguration"/> for the given navigation property.
        /// </summary>
        /// <param name="navigationConfiguration">The navigation property.</param>
        /// <param name="autoCreate">Represents a value specifying if the binding should be created if it is not found.</param>
        /// <returns>The binding if found.</returns>
        public NavigationPropertyBindingConfiguration FindBinding(NavigationPropertyConfiguration navigationConfiguration, bool autoCreate)
        {
            return _configuration.FindBinding(navigationConfiguration, autoCreate);
        }

        private static EntityInstanceContext<TEntityType> UpCastEntityInstanceContext(EntityInstanceContext context)
        {
            return new EntityInstanceContext<TEntityType>
            {
                SerializerContext = context.SerializerContext,
                EdmObject = context.EdmObject,
                EntityType = context.EntityType
            };
        }
    }
}
