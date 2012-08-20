// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    // TODO: Feature 443884: add support for starting from an original model
    public class ODataModelBuilder
    {
        private Dictionary<Type, StructuralTypeConfiguration> _structuralTypes = new Dictionary<Type, StructuralTypeConfiguration>();
        private Dictionary<string, IEntitySetConfiguration> _entitySets = new Dictionary<string, IEntitySetConfiguration>();

        public virtual IEnumerable<IEntitySetConfiguration> EntitySets
        {
            get { return _entitySets.Values; }
        }

        public IEnumerable<IStructuralTypeConfiguration> StructuralTypes
        {
            get { return _structuralTypes.Values; }
        }

        public EntityTypeConfiguration<TEntityType> Entity<TEntityType>() where TEntityType : class
        {
            return new EntityTypeConfiguration<TEntityType>(AddEntity(typeof(TEntityType)));
        }

        public ComplexTypeConfiguration<TComplexType> ComplexType<TComplexType>() where TComplexType : class
        {
            return new ComplexTypeConfiguration<TComplexType>(AddComplexType(typeof(TComplexType)));
        }

        public EntitySetConfiguration<TEntityType> EntitySet<TEntityType>(string name) where TEntityType : class
        {
            IEntityTypeConfiguration entity = AddEntity(typeof(TEntityType));
            return new EntitySetConfiguration<TEntityType>(this, AddEntitySet(name, entity));
        }

        public virtual IEntityTypeConfiguration AddEntity(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (!_structuralTypes.ContainsKey(type))
            {
                EntityTypeConfiguration entityTypeConfig = new EntityTypeConfiguration(this, type);
                _structuralTypes.Add(type, entityTypeConfig);
                return entityTypeConfig;
            }
            else
            {
                EntityTypeConfiguration config = _structuralTypes[type] as EntityTypeConfiguration;
                if (config == null || config.ClrType != type)
                {
                    throw Error.Argument("type", SRResources.TypeCannotBeEntityWasComplex, type.FullName);
                }

                return config;
            }
        }

        public virtual IComplexTypeConfiguration AddComplexType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (!_structuralTypes.ContainsKey(type))
            {
                ComplexTypeConfiguration complexTypeConfig = new ComplexTypeConfiguration(this, type);
                _structuralTypes.Add(type, complexTypeConfig);
                return complexTypeConfig;
            }
            else
            {
                ComplexTypeConfiguration complexTypeConfig = _structuralTypes[type] as ComplexTypeConfiguration;
                if (complexTypeConfig == null || complexTypeConfig.ClrType != type)
                {
                    throw Error.Argument("type", SRResources.TypeCannotBeComplexWasEntity, type.FullName);
                }

                return complexTypeConfig;
            }
        }

        public virtual IEntitySetConfiguration AddEntitySet(string name, IEntityTypeConfiguration entityType)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw Error.ArgumentNullOrEmpty("name");
            }

            if (entityType == null)
            {
                throw Error.ArgumentNull("entityType");
            }

            if (name.Contains("."))
            {
                throw Error.NotSupported(SRResources.InvalidEntitySetName, name);
            }

            EntitySetConfiguration entitySet = null;
            if (_entitySets.ContainsKey(name))
            {
                entitySet = _entitySets[name] as EntitySetConfiguration;
                if (entitySet.EntityType != entityType)
                {
                    throw Error.Argument("entityType", SRResources.EntitySetAlreadyConfiguredDifferentEntityType, entitySet.Name, entitySet.EntityType.Name);
                }
            }
            else
            {
                entitySet = new EntitySetConfiguration(this, entityType, name);
                _entitySets[name] = entitySet;
            }
            return entitySet;
        }

        public virtual bool RemoveStructuralType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            return _structuralTypes.Remove(type);
        }

        public virtual bool RemoveEntitySet(string name)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }

            return _entitySets.Remove(name);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Property is not appropriate, method does work")]
        public virtual IEdmModel GetEdmModel()
        {
            return EdmModelHelperMethods.BuildEdmModel("Default", "Container", StructuralTypes, EntitySets);
        }
    }
}
