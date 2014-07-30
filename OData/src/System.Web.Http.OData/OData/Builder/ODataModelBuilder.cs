// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http.OData.Formatter;
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
        private static readonly Version _defaultDataServiceVersion = new Version(3, 0);
        private static readonly Version _defaultMaxDataServiceVersionn = new Version(3, 0);

        private Dictionary<Type, StructuralTypeConfiguration> _structuralTypes = new Dictionary<Type, StructuralTypeConfiguration>();
        private Dictionary<string, EntitySetConfiguration> _entitySets = new Dictionary<string, EntitySetConfiguration>();
        private Dictionary<Type, PrimitiveTypeConfiguration> _primitiveTypes = new Dictionary<Type, PrimitiveTypeConfiguration>();
        private List<ProcedureConfiguration> _procedures = new List<ProcedureConfiguration>();

        private Version _dataServiceVersion;
        private Version _maxDataServiceVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataModelBuilder"/> class.
        /// </summary>
        public ODataModelBuilder()
        {
            Namespace = "Default";
            ContainerName = "Container";
            DataServiceVersion = _defaultDataServiceVersion;
            MaxDataServiceVersion = _defaultMaxDataServiceVersionn;
        }

        /// <summary>
        /// Gets or sets the namespace that will be used for the resulting model
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the name of the container that will hold all the EntitySets, Actions and Functions
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Gets or sets the data service version of the model. The default value is 3.0.
        /// </summary>
        public Version DataServiceVersion
        {
            get
            {
                return _dataServiceVersion;
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _dataServiceVersion = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum data service version of the model. The default value is 3.0.
        /// </summary>
        public Version MaxDataServiceVersion
        {
            get
            {
                return _maxDataServiceVersion;
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _maxDataServiceVersion = value;
            }
        }

        /// <summary>
        /// Gets the collection of EDM entity sets in the model to be built.
        /// </summary>
        public virtual IEnumerable<EntitySetConfiguration> EntitySets
        {
            get { return _entitySets.Values; }
        }

        /// <summary>
        /// Gets the collection of EDM types in the model to be built.
        /// </summary>
        public virtual IEnumerable<StructuralTypeConfiguration> StructuralTypes
        {
            get { return _structuralTypes.Values; }
        }

        /// <summary>
        /// Gets the collection of Procedures (i.e. Actions, Functions and ServiceOperations) in the model to be built
        /// </summary>
        public virtual IEnumerable<ProcedureConfiguration> Procedures
        {
            get { return _procedures; }
        }

        /// <summary>
        /// Registers an entity type as part of the model and returns an object that can be used to configure the entity.
        /// This method can be called multiple times for the same entity to perform multiple lines of configuration.
        /// </summary>
        /// <typeparam name="TEntityType">The type to be registered or configured.</typeparam>
        /// <returns>The configuration object for the specified entity type.</returns>
        public EntityTypeConfiguration<TEntityType> Entity<TEntityType>() where TEntityType : class
        {
            return new EntityTypeConfiguration<TEntityType>(this, AddEntity(typeof(TEntityType)));
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
            EntityTypeConfiguration entity = AddEntity(typeof(TEntityType));
            return new EntitySetConfiguration<TEntityType>(this, AddEntitySet(name, entity));
        }

        /// <summary>
        /// Adds a non-bindable action to the builder.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <returns>The configuration object for the specified action.</returns>
        public virtual ActionConfiguration Action(string name)
        {
            ActionConfiguration action = new ActionConfiguration(this, name);
            _procedures.Add(action);
            return action;
        }

        /// <summary>
        /// Registers an entity type as part of the model and returns an object that can be used to configure the entity.
        /// This method can be called multiple times for the same entity to perform multiple lines of configuration.
        /// </summary>
        /// <param name="type">The type to be registered or configured.</param>
        /// <returns>The configuration object for the specified entity type.</returns>
        public virtual EntityTypeConfiguration AddEntity(Type type)
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
        public virtual ComplexTypeConfiguration AddComplexType(Type type)
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
        /// Adds a procedure to the model.
        /// </summary>
        public virtual void AddProcedure(ProcedureConfiguration procedure)
        {
            _procedures.Add(procedure);
        }

        /// <summary>
        /// Registers an entity set as a part of the model and returns an object that can be used to configure the entity set.
        /// This method can be called multiple times for the same type to perform multiple lines of configuration.
        /// </summary>
        /// <param name="name">The name of the entity set.</param>
        /// <param name="entityType">The type to be registered or configured.</param>
        /// <returns>The configuration object for the specified entity set.</returns>
        public virtual EntitySetConfiguration AddEntitySet(string name, EntityTypeConfiguration entityType)
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
        /// Remove the procedure from the model
        /// <remarks>
        /// If there is more than one procedure with the name specified this method will not work.
        /// You need to use the other RemoveProcedure(..) overload instead.
        /// </remarks>
        /// </summary>
        /// <param name="name">The name of the procedure to be removed</param>
        /// <returns><see>true</see> if the procedure is present in the model and <see>false</see> otherwise.</returns>
        public virtual bool RemoveProcedure(string name)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }

            ProcedureConfiguration[] toRemove = _procedures.Where(p => p.Name == name).ToArray();
            int count = toRemove.Count();
            if (count == 1)
            {
                return RemoveProcedure(toRemove[0]);
            }
            else if (count == 0)
            {
                // For consistency with RemoveStructuralType().
                // uses same semantics as Dictionary.Remove(key).
                return false;
            }
            else
            {
                throw Error.InvalidOperation(SRResources.MoreThanOneProcedureFound, name);
            }
        }

        /// <summary>
        /// Remove the procedure from the model
        /// </summary>
        /// <param name="procedure">The procedure to be removed</param>
        /// <returns><see>true</see> if the procedure is present in the model and <see>false</see> otherwise.</returns>
        public virtual bool RemoveProcedure(ProcedureConfiguration procedure)
        {
            if (procedure == null)
            {
                throw Error.ArgumentNull("procedure");
            }
            return _procedures.Remove(procedure);
        }

        /// <summary>
        /// Attempts to find either a pre-configured structural type or a primitive type that matches the T.
        /// If no matches are found NULL is returned.
        /// </summary>
        public IEdmTypeConfiguration GetTypeConfigurationOrNull(Type type)
        {
            if (_primitiveTypes.ContainsKey(type))
            {
                return _primitiveTypes[type];
            }
            else
            {
                IEdmPrimitiveType edmType = EdmLibHelpers.GetEdmPrimitiveTypeOrNull(type);
                PrimitiveTypeConfiguration primitiveType = null;
                if (edmType != null)
                {
                    primitiveType = new PrimitiveTypeConfiguration(this, edmType, type);
                    _primitiveTypes[type] = primitiveType;
                    return primitiveType;
                }
                else if (_structuralTypes.ContainsKey(type))
                {
                    return _structuralTypes[type];
                }
            }
            return null;
        }

        /// <summary>
        /// Creates a <see cref="IEdmModel"/> based on the configuration performed using this builder. 
        /// </summary>
        /// <returns>The model that was built.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Property is not appropriate, method does work")]
        public virtual IEdmModel GetEdmModel()
        {
            return EdmModelHelperMethods.BuildEdmModel(this);
        }
    }
}
