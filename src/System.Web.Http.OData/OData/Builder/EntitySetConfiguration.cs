// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    public class EntitySetConfiguration : IEntitySetConfiguration
    {
        private readonly ODataModelBuilder _modelBuilder = null;

        private readonly Dictionary<NavigationPropertyConfiguration, NavigationPropertyBinding> _entitySetBindings =
            new Dictionary<NavigationPropertyConfiguration, NavigationPropertyBinding>();

        private string _url = null;
        private Func<EntityInstanceContext, Uri> _editLinkFactory = null;
        private Func<EntityInstanceContext, Uri> _readLinkFactory = null;
        private Func<EntityInstanceContext, string> _idLinkFactory = null;
        private readonly IDictionary<string, Func<EntityInstanceContext, IEdmNavigationProperty, Uri>> _navigationPropertyLinkBuilders = null;

        internal EntitySetConfiguration(ODataModelBuilder modelBuilder, Type entityType, string name)
            : this(modelBuilder, new EntityTypeConfiguration(modelBuilder, entityType), name)
        {
        }

        internal EntitySetConfiguration(ODataModelBuilder modelBuilder, IEntityTypeConfiguration entityType, string name)
        {
            _modelBuilder = modelBuilder;
            Name = name;
            EntityType = entityType;
            ClrType = entityType.ClrType;
            _url = Name;

            _editLinkFactory = null;
            _readLinkFactory = null;
            _navigationPropertyLinkBuilders = new Dictionary<string, Func<EntityInstanceContext, IEdmNavigationProperty, Uri>>();
        }

        public IEnumerable<NavigationPropertyBinding> Bindings
        {
            get { return _entitySetBindings.Values; }
        }

        public IEntityTypeConfiguration EntityType { get; private set; }

        public Type ClrType { get; private set; }

        public string Name { get; private set; }

        public IEntitySetConfiguration HasUrl(string url)
        {
            _url = url;
            return this;
        }

        public IEntitySetConfiguration HasEditLink(Func<EntityInstanceContext, Uri> editLinkFactory)
        {
            _editLinkFactory = editLinkFactory;
            return this;
        }

        public IEntitySetConfiguration HasReadLink(Func<EntityInstanceContext, Uri> readLinkFactory)
        {
            _readLinkFactory = readLinkFactory;
            return this;
        }

        public IEntitySetConfiguration HasIdLink(Func<EntityInstanceContext, string> idLinkFactory)
        {
            _idLinkFactory = idLinkFactory;
            return this;
        }

        public IEntitySetConfiguration HasNavigationPropertyLink(NavigationPropertyConfiguration navigationProperty, Func<EntityInstanceContext, IEdmNavigationProperty, Uri> navigationLinkFactory)
        {
            if (navigationProperty == null)
            {
                throw Error.ArgumentNull("navigationProperty");
            }

            if (navigationLinkFactory == null)
            {
                throw Error.ArgumentNull("navigationLinkFactory");
            }

            _navigationPropertyLinkBuilders.Add(navigationProperty.Name, navigationLinkFactory);
            return this;
        }

        public IEntitySetConfiguration HasNavigationPropertiesLink(IEnumerable<NavigationPropertyConfiguration> navigationProperties, Func<EntityInstanceContext, IEdmNavigationProperty, Uri> navigationLinkFactory)
        {
            if (navigationProperties == null)
            {
                throw Error.ArgumentNull("navigationProperties");
            }

            if (navigationLinkFactory == null)
            {
                throw Error.ArgumentNull("navigationLinkFactory");
            }

            foreach (NavigationPropertyConfiguration navigationProperty in navigationProperties)
            {
                HasNavigationPropertyLink(navigationProperty, navigationLinkFactory);
            }

            return this;
        }

        public NavigationPropertyBinding AddBinding(NavigationPropertyConfiguration navigationConfiguration, IEntitySetConfiguration targetEntitySet)
        {
            NavigationPropertyBinding navigationPropertyBinding = null;
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
                navigationPropertyBinding = new NavigationPropertyBinding(navigationConfiguration, targetEntitySet);
                _entitySetBindings[navigationConfiguration] = navigationPropertyBinding;
            }
            return navigationPropertyBinding;
        }

        public void RemoveBinding(NavigationPropertyConfiguration navigationConfiguration)
        {
            if (_entitySetBindings.ContainsKey(navigationConfiguration))
            {
                _entitySetBindings.Remove(navigationConfiguration);
            }
        }

        public NavigationPropertyBinding FindBinding(NavigationPropertyConfiguration navigationConfiguration)
        {
            return FindBinding(navigationConfiguration, autoCreate: true);
        }

        public NavigationPropertyBinding FindBinding(NavigationPropertyConfiguration navigationConfiguration, bool autoCreate)
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
            IEntitySetConfiguration[] matchingSets = _modelBuilder.EntitySets.Where(es => es.EntityType.FullName == entityType.FullName).ToArray();
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
                throw Error.NotSupported(SRResources.CannotAutoCreateMultipleCandidates);
            }
        }

        public string GetUrl()
        {
            return _url;
        }

        public Func<EntityInstanceContext, Uri> GetEditLink()
        {
            return _editLinkFactory;
        }

        public Func<EntityInstanceContext, Uri> GetReadLink()
        {
            return _readLinkFactory;
        }

        public Func<EntityInstanceContext, string> GetIdLink()
        {
            return _idLinkFactory;
        }

        public Func<EntityInstanceContext, IEdmNavigationProperty, Uri> GetNavigationPropertyLink(string navigationPropertyName)
        {
            if (String.IsNullOrEmpty(navigationPropertyName))
            {
                throw Error.ArgumentNullOrEmpty("navigationProperty");
            }

            Func<EntityInstanceContext, IEdmNavigationProperty, Uri> navigationPropertyLinkBuilder;
            _navigationPropertyLinkBuilders.TryGetValue(navigationPropertyName, out navigationPropertyLinkBuilder);
            return navigationPropertyLinkBuilder;
        }

        public NavigationPropertyBinding FindBinding(string propertyName)
        {
            return Bindings.Single(b => b.NavigationProperty.Name == propertyName);
        }
    }

    public class EntitySetConfiguration<TEntityType> where TEntityType : class
    {
        private IEntitySetConfiguration _configuration;
        private EntityTypeConfiguration<TEntityType> _entityType;
        private ODataModelBuilder _modelBuilder;

        public EntitySetConfiguration(ODataModelBuilder modelBuilder, string name)
            : this(modelBuilder, new EntitySetConfiguration(modelBuilder, typeof(TEntityType), name))
        {
            _configuration = new EntitySetConfiguration(modelBuilder, typeof(TEntityType), name);
            _entityType = new EntityTypeConfiguration<TEntityType>(_configuration.EntityType); // TBD: fix this
            _modelBuilder = modelBuilder;
        }

        public EntitySetConfiguration(ODataModelBuilder modelBuilder, IEntitySetConfiguration configuration)
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
            _entityType = new EntityTypeConfiguration<TEntityType>(_configuration.EntityType);
        }

        public EntityTypeConfiguration<TEntityType> EntityType
        {
            get
            {
                return _entityType;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public NavigationPropertyBinding HasManyBinding<TTargetType>(
            Expression<Func<TEntityType, ICollection<TTargetType>>> navigationExpression, string entitySetName)
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
            Expression<Func<TEntityType, ICollection<TTargetType>>> navigationExpression,
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
        public void HasEditLink(Func<EntityInstanceContext<TEntityType>, string> editLinkFactory)
        {
            if (editLinkFactory == null)
            {
                throw Error.ArgumentNull("editLinkFactory");
            }

            HasEditLink(entityInstanceContext => new Uri(editLinkFactory(entityInstanceContext)));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public void HasEditLink(Func<EntityInstanceContext<TEntityType>, Uri> editLinkFactory)
        {
            if (editLinkFactory == null)
            {
                throw Error.ArgumentNull("editLinkFactory");
            }

            _configuration.HasEditLink((entity) => editLinkFactory(UpCastEntityInstanceContext(entity)));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public void HasReadLink(Func<EntityInstanceContext<TEntityType>, string> readLinkFactory)
        {
            if (readLinkFactory == null)
            {
                throw Error.ArgumentNull("readLinkFactory");
            }

            HasReadLink(entityInstanceContext => new Uri(readLinkFactory(entityInstanceContext)));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public void HasReadLink(Func<EntityInstanceContext<TEntityType>, Uri> readLinkFactory)
        {
            if (readLinkFactory == null)
            {
                throw Error.ArgumentNull("readLinkFactory");
            }

            _configuration.HasReadLink((entity) => readLinkFactory(UpCastEntityInstanceContext(entity)));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public void HasIdLink(Func<EntityInstanceContext<TEntityType>, string> idLinkFactory)
        {
            if (idLinkFactory == null)
            {
                throw Error.ArgumentNull("idLinkFactory");
            }

            _configuration.HasIdLink((entity) => idLinkFactory(UpCastEntityInstanceContext(entity)));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public void HasNavigationPropertyLink(NavigationPropertyConfiguration navigationProperty, Func<EntityInstanceContext<TEntityType>, IEdmNavigationProperty, Uri> navigationLinkFactory)
        {
            if (navigationProperty == null)
            {
                throw Error.ArgumentNull("navigationProperty");
            }

            if (navigationLinkFactory == null)
            {
                throw Error.ArgumentNull("navigationLinkFactory");
            }

            _configuration.HasNavigationPropertyLink(navigationProperty, (entity, property) => navigationLinkFactory(UpCastEntityInstanceContext(entity), property));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generic appropriate here")]
        public void HasNavigationPropertiesLink(IEnumerable<NavigationPropertyConfiguration> navigationProperties, Func<EntityInstanceContext<TEntityType>, IEdmNavigationProperty, Uri> navigationLinkFactory)
        {
            if (navigationProperties == null)
            {
                throw Error.ArgumentNull("navigationProperties");
            }

            if (navigationLinkFactory == null)
            {
                throw Error.ArgumentNull("navigationLinkFactory");
            }

            _configuration.HasNavigationPropertiesLink(navigationProperties, (entity, property) => navigationLinkFactory(UpCastEntityInstanceContext(entity), property));
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
            return new EntityInstanceContext<TEntityType>(context.EdmModel, context.EntitySet, context.EntityType, context.UrlHelper, context.EntityInstance as TEntityType);
        }
    }
}
