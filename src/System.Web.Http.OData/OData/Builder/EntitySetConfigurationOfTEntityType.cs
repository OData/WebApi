// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
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

        public EntityTypeConfiguration<TEntityType> EntityType
        {
            get
            {
                return _entityType;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBinding HasManyBinding<TTargetType, TDerivedEntityType>(
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

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBinding HasManyBinding<TTargetType>(
            Expression<Func<TEntityType, IEnumerable<TTargetType>>> navigationExpression, string entitySetName)
            where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            return _configuration.AddBinding(EntityType.HasMany(navigationExpression), _modelBuilder.EntitySet<TTargetType>(entitySetName)._configuration);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBinding HasManyBinding<TTargetType>(
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

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBinding HasManyBinding<TTargetType, TDerivedEntityType>(
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

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBinding HasRequiredBinding<TTargetType>(
            Expression<Func<TEntityType, TTargetType>> navigationExpression, string entitySetName)
            where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            return _configuration.AddBinding(EntityType.HasRequired(navigationExpression), _modelBuilder.EntitySet<TTargetType>(entitySetName)._configuration);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBinding HasRequiredBinding<TTargetType, TDerivedEntityType>(
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

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBinding HasRequiredBinding<TTargetType>(
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

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBinding HasRequiredBinding<TTargetType, TDerivedEntityType>(
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

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBinding HasOptionalBinding<TTargetType>(
            Expression<Func<TEntityType, TTargetType>> navigationExpression, string entitySetName)
            where TTargetType : class
        {
            if (navigationExpression == null)
            {
                throw Error.ArgumentNull("navigationExpression");
            }

            return _configuration.AddBinding(EntityType.HasOptional(navigationExpression), _modelBuilder.EntitySet<TTargetType>(entitySetName)._configuration);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBinding HasOptionalBinding<TTargetType, TDerivedEntityType>(
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

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBinding HasOptionalBinding<TTargetType>(
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

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBinding HasOptionalBinding<TTargetType, TDerivedEntityType>(
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

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public void HasEditLink(Func<EntityInstanceContext<TEntityType>, string> editLinkFactory, bool followsConventions)
        {
            if (editLinkFactory == null)
            {
                throw Error.ArgumentNull("editLinkFactory");
            }

            HasEditLink(entityInstanceContext => new Uri(editLinkFactory(entityInstanceContext)), followsConventions);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public void HasEditLink(Func<EntityInstanceContext<TEntityType>, Uri> editLinkFactory, bool followsConventions)
        {
            if (editLinkFactory == null)
            {
                throw Error.ArgumentNull("editLinkFactory");
            }

            _configuration.HasEditLink(new SelfLinkBuilder<Uri>((entity) => editLinkFactory(UpCastEntityInstanceContext(entity)), followsConventions));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public void HasReadLink(Func<EntityInstanceContext<TEntityType>, string> readLinkFactory, bool followsConventions)
        {
            if (readLinkFactory == null)
            {
                throw Error.ArgumentNull("readLinkFactory");
            }

            HasReadLink(entityInstanceContext => new Uri(readLinkFactory(entityInstanceContext)), followsConventions);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public void HasReadLink(Func<EntityInstanceContext<TEntityType>, Uri> readLinkFactory, bool followsConventions)
        {
            if (readLinkFactory == null)
            {
                throw Error.ArgumentNull("readLinkFactory");
            }

            _configuration.HasReadLink(new SelfLinkBuilder<Uri>((entity) => readLinkFactory(UpCastEntityInstanceContext(entity)), followsConventions));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public void HasIdLink(Func<EntityInstanceContext<TEntityType>, string> idLinkFactory, bool followsConventions)
        {
            if (idLinkFactory == null)
            {
                throw Error.ArgumentNull("idLinkFactory");
            }

            _configuration.HasIdLink(new SelfLinkBuilder<string>((entity) => idLinkFactory(UpCastEntityInstanceContext(entity)), followsConventions));
        }

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

        public NavigationPropertyBinding FindBinding(string propertyName)
        {
            return _configuration.FindBinding(propertyName);
        }

        public NavigationPropertyBinding FindBinding(NavigationPropertyConfiguration navigationConfiguration)
        {
            return _configuration.FindBinding(navigationConfiguration, autoCreate: true);
        }

        public NavigationPropertyBinding FindBinding(NavigationPropertyConfiguration navigationConfiguration, bool autoCreate)
        {
            return _configuration.FindBinding(navigationConfiguration, autoCreate);
        }

        private static EntityInstanceContext<TEntityType> UpCastEntityInstanceContext(EntityInstanceContext context)
        {
            return new EntityInstanceContext<TEntityType>
            {
                EdmModel = context.EdmModel,
                EntitySet = context.EntitySet,
                EntityType = context.EntityType,
                UrlHelper = context.UrlHelper,
                EntityInstance = context.EntityInstance as TEntityType
            };
        }
    }
}
