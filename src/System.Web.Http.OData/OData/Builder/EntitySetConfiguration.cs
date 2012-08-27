// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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
            IEntitySetConfiguration[] matchingSets = _modelBuilder.EntitySets.Where(es => es.EntityType.ClrType == entityType).ToArray();
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
}
