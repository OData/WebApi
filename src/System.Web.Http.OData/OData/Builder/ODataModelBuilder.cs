// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// <see cref="ODataModelBuilder"/> is used to map CLR classes to an EDM model.
    /// </summary>
    // TODO: Feature 443884: add support for starting from an original model
    public class ODataModelBuilder
    {
        private Dictionary<Type, StructuralTypeConfiguration> _structuralTypes = new Dictionary<Type, StructuralTypeConfiguration>();
        private Dictionary<string, IEntitySetConfiguration> _entitySets = new Dictionary<string, IEntitySetConfiguration>();

        /// <summary>
        /// The collection of EDM entity sets in the model to be built.
        /// </summary>
        public virtual IEnumerable<IEntitySetConfiguration> EntitySets
        {
            get { return _entitySets.Values; }
        }

        /// <summary>
        /// The collection of EDM types in the model to be built.
        /// </summary>
        public IEnumerable<IStructuralTypeConfiguration> StructuralTypes
        {
            get { return _structuralTypes.Values; }
        }

        /// <summary>
        /// Registers an entity type as part of the model and returns an object that can be used to configure the entity.
        /// This method can be called multiple times for the same entity to perform multiple lines of configuration.
        /// </summary>
        /// <typeparam name="TEntityType">The type to be registered or configured.</typeparam>
        /// <returns>The configuration object for the specified entity type.</returns>
        public EntityTypeConfiguration<TEntityType> Entity<TEntityType>() where TEntityType : class
        {
            return new EntityTypeConfiguration<TEntityType>(AddEntity(typeof(TEntityType)));
        }

        /// <summary>
        /// Registers a type as a complex type in the model and returns an object that can be used to configure the complex type.
        /// This method can be called multiple times for the same type to perform multiple lines of configuration.
        /// </summary>
        /// <typeparam name="TComplexType">The type to be registered or configured.</typeparam>
        /// <returns>The configuration object for the specified complex type.</returns>
        public ComplexTypeConfiguration<TComplexType> ComplexType<TComplexType>() where TComplexType : class
        {
            return new ComplexTypeConfiguration<TComplexType>(AddComplexType(typeof(TComplexType)));
        }

        /// <summary>
        /// Registers an entity set as a part of the model and returns an object that can be used to configure the entity set.
        /// This method can be called multiple times for the same type to perform multiple lines of configuration.
        /// </summary>
        /// <typeparam name="TEntityType">The entity type of the entity set.</typeparam>
        /// <param name="name">The name of the entity set.</param>
        /// <returns>The configuration object for the specified entity set.</returns>
        public EntitySetConfiguration<TEntityType> EntitySet<TEntityType>(string name) where TEntityType : class
        {
            IEntityTypeConfiguration entity = AddEntity(typeof(TEntityType));
            return new EntitySetConfiguration<TEntityType>(this, AddEntitySet(name, entity));
        }

        /// <summary>
        /// Registers an entity type as part of the model and returns an object that can be used to configure the entity.
        /// This method can be called multiple times for the same entity to perform multiple lines of configuration.
        /// </summary>
        /// <param name="type">The type to be registered or configured.</param>
        /// <returns>The configuration object for the specified entity type.</returns>
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

        /// <summary>
        /// Registers an complex type as part of the model and returns an object that can be used to configure the entity.
        /// This method can be called multiple times for the same entity to perform multiple lines of configuration.
        /// </summary>
        /// <param name="type">The type to be registered or configured.</param>
        /// <returns>The configuration object for the specified complex type.</returns>
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

        /// <summary>
        /// Registers an entity set as a part of the model and returns an object that can be used to configure the entity set.
        /// This method can be called multiple times for the same type to perform multiple lines of configuration.
        /// </summary>
        /// <param name="name">The name of the entity set.</param>
        /// <param name="entityType">The type to be registered or configured.</param>
        /// <returns>The configuration object for the specified entity set.</returns>
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

        /// <summary>
        /// Removes the type from the model.
        /// </summary>
        /// <param name="type">The type to be removed</param>
        /// <returns><see>true</see> if the type is present in the model and <see>false</see> otherwise.</returns>
        public virtual bool RemoveStructuralType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            return _structuralTypes.Remove(type);
        }

        /// <summary>
        /// Removes the entity set from the model.
        /// </summary>
        /// <param name="name">The name of the entity set to be removed</param>
        /// <returns><see>true</see> if the entity set is present in the model and <see>false</see> otherwise.</returns>
        public virtual bool RemoveEntitySet(string name)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }

            return _entitySets.Remove(name);
        }

        /// <summary>
        /// Creates a <see cref="IEdmModel"/> based on the configuration performed using this builder. 
        /// </summary>
        /// <returns>The model that was built.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Property is not appropriate, method does work")]
        public virtual IEdmModel GetEdmModel()
        {
            return EdmModelHelperMethods.BuildEdmModel("Default", "Container", StructuralTypes, EntitySets);
        }
    }
}
