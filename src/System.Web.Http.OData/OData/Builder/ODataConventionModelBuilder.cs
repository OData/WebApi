// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData.Builder.Conventions;
using System.Web.Http.OData.Builder.Conventions.Attributes;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// <see cref="ODataConventionModelBuilder"/> is used to automatically map CLR classes to an EDM model based on a set of <see cref="IConvention"/>.
    /// </summary>
    public class ODataConventionModelBuilder : ODataModelBuilder
    {
        private static readonly List<IConvention> _conventions = new List<IConvention>
        {
            // type and property conventions (ordering is important here).
            new AbstractEntityTypeDiscoveryConvention(),
            new DataContractAttributeEdmTypeConvention(),
            new NotMappedAttributeConvention(), // NotMappedAttributeConvention has to run before EntityKeyConvention
            new EntityKeyConvention(),
            new RequiredAttributeEdmPropertyConvention(),
            new KeyAttributeEdmPropertyConvention(),
            new IgnoreDataMemberAttributeEdmPropertyConvention(),

            // IEntitySetConvention's
            new SelfLinksGenerationConvention(),
            new NavigationLinksGenerationConvention(),

            // IEdmPropertyConvention's
            new NotMappedAttributeConvention(),
            new RequiredAttributeEdmPropertyConvention(),
            new KeyAttributeEdmPropertyConvention(),
            new IgnoreDataMemberAttributeEdmPropertyConvention(),

            // IEdmFunctionImportConventions's
            new ActionLinkGenerationConvention(),
        };

        // These hashset's keep track of edmtypes/entitysets for which conventions
        // have been applied or being applied so that we don't run a convention twice on the
        // same type/set.
        private HashSet<IStructuralTypeConfiguration> _mappedTypes;
        private HashSet<IEntitySetConfiguration> _configuredEntitySets;
        private HashSet<Type> _ignoredTypes;

        private IEnumerable<IStructuralTypeConfiguration> _explicitlyAddedTypes;

        private bool _isModelBeingBuilt;
        private bool _isQueryCompositionMode;

        // build the mapping between type and its derived types to be used later.
        private Lazy<IDictionary<Type, List<Type>>> _allTypesWithDerivedTypeMapping;

        /// <summary>
        /// Initializes a new <see cref="ODataConventionModelBuilder"/>.
        /// </summary>
        public ODataConventionModelBuilder()
        {
            Initialize(new DefaultAssembliesResolver(), isQueryCompositionMode: false);
        }

        /// <summary>
        /// Initializes a new <see cref="ODataConventionModelBuilder"/>.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use.</param>
        public ODataConventionModelBuilder(HttpConfiguration configuration)
            : this(configuration, isQueryCompositionMode: false)
        {
        }

        internal ODataConventionModelBuilder(HttpConfiguration configuration, bool isQueryCompositionMode)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            Initialize(configuration.Services.GetAssembliesResolver(), isQueryCompositionMode);
        }

        internal void Initialize(IAssembliesResolver assembliesResolver, bool isQueryCompositionMode)
        {
            _isQueryCompositionMode = isQueryCompositionMode;
            _configuredEntitySets = new HashSet<IEntitySetConfiguration>();
            _mappedTypes = new HashSet<IStructuralTypeConfiguration>();
            _ignoredTypes = new HashSet<Type>();
            _allTypesWithDerivedTypeMapping = new Lazy<IDictionary<Type, List<Type>>>(
                () => BuildDerivedTypesMapping(assembliesResolver),
                isThreadSafe: false);
        }

        /// <summary>
        /// Excludes a type from the model. This is used to remove types from the model that were added by convention during initial model discovery.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The same <c ref="ODataConventionModelBuilder"/> so that multiple calls can be chained.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004: GenericMethodsShouldProvideTypeParameter", Justification = "easier to call the generic version than using typeof().")]
        public ODataConventionModelBuilder Ignore<T>()
        {
            _ignoredTypes.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// Excludes a type or types from the model. This is used to remove types from the model that were added by convention during initial model discovery.
        /// </summary>
        /// <param name="types">The types to be excluded from the model.</param>
        /// <returns>The same <c ref="ODataConventionModelBuilder"/> so that multiple calls can be chained.</returns>
        public ODataConventionModelBuilder Ignore(params Type[] types)
        {
            foreach (Type type in types)
            {
                _ignoredTypes.Add(type);
            }

            return this;
        }

        public override IEntityTypeConfiguration AddEntity(Type type)
        {
            IEntityTypeConfiguration entityTypeConfiguration = base.AddEntity(type);
            if (_isModelBeingBuilt)
            {
                MapType(entityTypeConfiguration);
            }

            return entityTypeConfiguration;
        }

        public override IComplexTypeConfiguration AddComplexType(Type type)
        {
            IComplexTypeConfiguration complexTypeConfiguration = base.AddComplexType(type);
            if (_isModelBeingBuilt)
            {
                MapType(complexTypeConfiguration);
            }

            return complexTypeConfiguration;
        }

        public override IEntitySetConfiguration AddEntitySet(string name, IEntityTypeConfiguration entityType)
        {
            IEntitySetConfiguration entitySetConfiguration = base.AddEntitySet(name, entityType);
            if (_isModelBeingBuilt)
            {
                ApplyEntitySetConventions(entitySetConfiguration);
            }

            return entitySetConfiguration;
        }

        public override IEdmModel GetEdmModel()
        {
            if (_isModelBeingBuilt)
            {
                throw Error.NotSupported(SRResources.GetEdmModelCalledMoreThanOnce);
            }

            // before we begin, get the set of types the user had added explicitly.
            _explicitlyAddedTypes = new List<IStructuralTypeConfiguration>(StructuralTypes);

            _isModelBeingBuilt = true;

            MapTypes();

            DiscoverInheritanceRelationships();

            // Don't RediscoverComplexTypes() and treat everything as an entity type if buidling a model for QueryableAttribute.
            if (!_isQueryCompositionMode)
            {
                RediscoverComplexTypes();
            }

            // prune unreachable types
            PruneUnreachableTypes();

            // Apply entity set conventions.
            IEnumerable<IEntitySetConfiguration> explictlyConfiguredEntitySets = new List<IEntitySetConfiguration>(EntitySets);
            foreach (IEntitySetConfiguration entitySet in explictlyConfiguredEntitySets)
            {
                ApplyEntitySetConventions(entitySet);
            }

            foreach (ProcedureConfiguration procedure in Procedures)
            {
                ApplyProcedureConventions(procedure);
            }

            return base.GetEdmModel();
        }

        internal bool IsIgnoredType(Type type)
        {
            Contract.Requires(type != null);

            return _ignoredTypes.Contains(type);
        }

        // patch up the base type for all entities that don't have any yet.
        internal void DiscoverInheritanceRelationships()
        {
            Dictionary<Type, IEntityTypeConfiguration> entityMap = StructuralTypes.OfType<IEntityTypeConfiguration>().ToDictionary(e => e.ClrType);

            foreach (IEntityTypeConfiguration entity in StructuralTypes.OfType<IEntityTypeConfiguration>().Where(e => !e.BaseTypeConfigured).ToArray())
            {
                Type baseClrType = entity.ClrType.BaseType;
                while (baseClrType != null)
                {
                    // see if we there is an entity that we know mapping to this clr types base type.
                    IEntityTypeConfiguration baseEntityType;
                    if (entityMap.TryGetValue(baseClrType, out baseEntityType))
                    {
                        RemoveBaseTypeProperties(entity, baseEntityType);
                        entity.DerivesFrom(baseEntityType);
                        break;
                    }

                    baseClrType = baseClrType.BaseType;
                }
            }
        }

        internal void MapDerivedTypes(IEntityTypeConfiguration entity)
        {
            HashSet<Type> visitedEntities = new HashSet<Type>();

            Queue<IEntityTypeConfiguration> entitiesToBeVisited = new Queue<IEntityTypeConfiguration>();
            entitiesToBeVisited.Enqueue(entity);

            // populate all the derived types
            while (entitiesToBeVisited.Count != 0)
            {
                IEntityTypeConfiguration baseEntity = entitiesToBeVisited.Dequeue();
                visitedEntities.Add(baseEntity.ClrType);

                List<Type> derivedTypes;
                if (_allTypesWithDerivedTypeMapping.Value.TryGetValue(baseEntity.ClrType, out derivedTypes))
                {
                    foreach (Type derivedType in derivedTypes)
                    {
                        if (!visitedEntities.Contains(derivedType) && !IsIgnoredType(derivedType))
                        {
                            IEntityTypeConfiguration derivedEntity = AddEntity(derivedType);
                            entitiesToBeVisited.Enqueue(derivedEntity);
                        }
                    }
                }
            }
        }

        // remove base type properties from the derived types.
        internal void RemoveBaseTypeProperties(IEntityTypeConfiguration derivedEntity, IEntityTypeConfiguration baseEntity)
        {
            IEnumerable<IEntityTypeConfiguration> typesToLift = new[] { derivedEntity }.Concat(this.DerivedTypes(derivedEntity));

            foreach (PropertyConfiguration property in baseEntity.Properties.Concat(baseEntity.DerivedProperties()))
            {
                foreach (IEntityTypeConfiguration entity in typesToLift)
                {
                    PropertyConfiguration derivedPropertyToRemove = entity.Properties.Where(p => p.Name == property.Name).SingleOrDefault();
                    if (derivedPropertyToRemove != null)
                    {
                        entity.RemoveProperty(derivedPropertyToRemove.PropertyInfo);
                    }
                }
            }
        }

        private void RediscoverComplexTypes()
        {
            Contract.Assert(_explicitlyAddedTypes != null);

            IEnumerable<IEntityTypeConfiguration> misconfiguredEntityTypes = StructuralTypes
                                                                            .Except(_explicitlyAddedTypes)
                                                                            .OfType<IEntityTypeConfiguration>()
                                                                            .Where(entity => !entity.Keys().Any())
                                                                            .ToArray();

            ReconfigureEntityTypesAsComplexType(misconfiguredEntityTypes);
        }

        private void ReconfigureEntityTypesAsComplexType(IEnumerable<IEntityTypeConfiguration> misconfiguredEntityTypes)
        {
            IEnumerable<IEntityTypeConfiguration> actualEntityTypes = StructuralTypes
                                                                            .Except(misconfiguredEntityTypes)
                                                                            .OfType<IEntityTypeConfiguration>()
                                                                            .ToArray();

            foreach (IEntityTypeConfiguration misconfiguredEntityType in misconfiguredEntityTypes)
            {
                RemoveStructuralType(misconfiguredEntityType.ClrType);

                IComplexTypeConfiguration newComplexType = AddComplexType(misconfiguredEntityType.ClrType);
                foreach (PropertyInfo ignoredProperty in misconfiguredEntityType.IgnoredProperties)
                {
                    newComplexType.RemoveProperty(ignoredProperty);
                }

                foreach (IEntityTypeConfiguration entityToBePatched in actualEntityTypes)
                {
                    NavigationPropertyConfiguration[] propertiesToBeRemoved = entityToBePatched
                                                                            .NavigationProperties
                                                                            .Where(navigationProperty => navigationProperty.RelatedClrType == misconfiguredEntityType.ClrType)
                                                                            .ToArray();
                    foreach (NavigationPropertyConfiguration propertyToBeRemoved in propertiesToBeRemoved)
                    {
                        entityToBePatched.RemoveProperty(propertyToBeRemoved.PropertyInfo);

                        if (propertyToBeRemoved.Multiplicity == EdmMultiplicity.Many)
                        {
                            entityToBePatched.AddCollectionProperty(propertyToBeRemoved.PropertyInfo);
                        }
                        else
                        {
                            entityToBePatched.AddComplexProperty(propertyToBeRemoved.PropertyInfo);
                        }
                    }
                }
            }
        }

        private void MapTypes()
        {
            foreach (IStructuralTypeConfiguration edmType in _explicitlyAddedTypes)
            {
                MapType(edmType);
            }
        }

        private void MapType(IStructuralTypeConfiguration edmType)
        {
            if (!_mappedTypes.Contains(edmType))
            {
                _mappedTypes.Add(edmType);
                IEntityTypeConfiguration entity = edmType as IEntityTypeConfiguration;
                if (entity != null)
                {
                    MapEntityType(entity);
                }
                else
                {
                    MapComplexType(edmType as IComplexTypeConfiguration);
                }

                ApplyTypeAndPropertyConventions(edmType);
            }
        }

        private void MapEntityType(IEntityTypeConfiguration entity)
        {
            IEnumerable<PropertyInfo> properties = ConventionsHelpers.GetProperties(entity);
            foreach (PropertyInfo property in properties)
            {
                bool isCollection;
                IStructuralTypeConfiguration mappedType;

                PropertyKind propertyKind = GetPropertyType(property, out isCollection, out mappedType);

                if (propertyKind == PropertyKind.Primitive || propertyKind == PropertyKind.Complex)
                {
                    MapStructuralProperty(entity, property, propertyKind, isCollection);
                }
                else
                {
                    if (!isCollection)
                    {
                        entity.AddNavigationProperty(property, EdmMultiplicity.ZeroOrOne);
                    }
                    else
                    {
                        entity.AddNavigationProperty(property, EdmMultiplicity.Many);
                    }
                }
            }

            MapDerivedTypes(entity);
        }

        private void MapComplexType(IComplexTypeConfiguration complexType)
        {
            IEnumerable<PropertyInfo> properties = ConventionsHelpers.GetAllProperties(complexType);
            foreach (PropertyInfo property in properties)
            {
                bool isCollection;
                IStructuralTypeConfiguration mappedType;

                PropertyKind propertyKind = GetPropertyType(property, out isCollection, out mappedType);

                if (propertyKind == PropertyKind.Primitive || propertyKind == PropertyKind.Complex)
                {
                    MapStructuralProperty(complexType, property, propertyKind, isCollection);
                }
                else
                {
                    // navigation property in a complex type ?
                    if (mappedType == null)
                    {
                        // the user told nothing about this type and this is the first time we are seeing this type.
                        // complex types cannot contain entities. So, treat it as complex property.
                        MapStructuralProperty(complexType, property, PropertyKind.Complex, isCollection);
                    }
                    else if (_explicitlyAddedTypes.Contains(mappedType))
                    {
                        // user told us that this is an entity type.
                        throw Error.InvalidOperation(SRResources.ComplexTypeRefersToEntityType, complexType.ClrType.FullName, mappedType.ClrType.FullName, property.Name);
                    }
                    else
                    {
                        // we tried to be over-smart earlier and made the bad choice. so patch up now.
                        IEntityTypeConfiguration mappedTypeAsEntity = mappedType as IEntityTypeConfiguration;
                        Contract.Assert(mappedTypeAsEntity != null);

                        ReconfigureEntityTypesAsComplexType(new IEntityTypeConfiguration[] { mappedTypeAsEntity });

                        MapStructuralProperty(complexType, property, PropertyKind.Complex, isCollection);
                    }
                }
            }
        }

        private void MapStructuralProperty(IStructuralTypeConfiguration type, PropertyInfo property, PropertyKind propertyKind, bool isCollection)
        {
            Contract.Assert(type != null);
            Contract.Assert(property != null);
            Contract.Assert(propertyKind == PropertyKind.Complex || propertyKind == PropertyKind.Primitive);

            if (!isCollection)
            {
                if (propertyKind == PropertyKind.Primitive)
                {
                    type.AddProperty(property);
                }
                else
                {
                    type.AddComplexProperty(property);
                }
            }
            else
            {
                if (_isQueryCompositionMode)
                {
                    Contract.Assert(propertyKind != PropertyKind.Complex, "we don't create complex types in query composition mode.");
                }

                type.AddCollectionProperty(property);
            }
        }

        // figures out the type of the property (primitive, complex, navigation) and the corresponding edm type if we have seen this type
        // earlier or the user told us about it.
        private PropertyKind GetPropertyType(PropertyInfo property, out bool isCollection, out IStructuralTypeConfiguration mappedType)
        {
            Contract.Assert(property != null);

            if (EdmLibHelpers.GetEdmPrimitiveTypeOrNull(property.PropertyType) != null)
            {
                isCollection = false;
                mappedType = null;
                return PropertyKind.Primitive;
            }
            else
            {
                mappedType = GetStructuralTypeOrNull(property.PropertyType);
                if (mappedType != null)
                {
                    isCollection = false;

                    if (mappedType is IComplexTypeConfiguration)
                    {
                        return PropertyKind.Complex;
                    }
                    else
                    {
                        return PropertyKind.Navigation;
                    }
                }
                else
                {
                    Type elementType;
                    if (property.PropertyType.IsCollection(out elementType))
                    {
                        isCollection = true;

                        // Collection properties - can be a collection of primitives, complex or entities.
                        if (EdmLibHelpers.GetEdmPrimitiveTypeOrNull(elementType) != null)
                        {
                            return PropertyKind.Primitive;
                        }
                        else
                        {
                            mappedType = GetStructuralTypeOrNull(elementType);
                            if (mappedType != null && mappedType is IComplexTypeConfiguration)
                            {
                                return PropertyKind.Complex;
                            }
                            else
                            {
                                // if we know nothing about this type we assume it to be an entity
                                // and patch up later
                                return PropertyKind.Navigation;
                            }
                        }
                    }
                    else
                    {
                        // if we know nothing about this type we assume it to be an entity
                        // and patch up later
                        isCollection = false;
                        return PropertyKind.Navigation;
                    }
                }
            }
        }

        // the convention model builder MapTypes() method might have went through deep object graphs and added a bunch of types
        // only to realise after applying the conventions that the user has ignored some of the properties. So, prune the unreachable stuff.
        private void PruneUnreachableTypes()
        {
            Contract.Assert(_explicitlyAddedTypes != null);

            // Do a BFS starting with the types the user has explicitly added to find out the unreachable nodes.
            Queue<IStructuralTypeConfiguration> reachableTypes = new Queue<IStructuralTypeConfiguration>(_explicitlyAddedTypes);
            HashSet<IStructuralTypeConfiguration> visitedTypes = new HashSet<IStructuralTypeConfiguration>();

            while (reachableTypes.Count != 0)
            {
                IStructuralTypeConfiguration currentType = reachableTypes.Dequeue();

                // go visit other end of each of this node's edges.
                foreach (PropertyConfiguration property in currentType.Properties.Where(property => property.Kind != PropertyKind.Primitive))
                {
                    if (property.Kind == PropertyKind.Collection)
                    {
                        // if the elementType is primitive we don't need to do anything.
                        CollectionPropertyConfiguration colProperty = property as CollectionPropertyConfiguration;
                        if (EdmLibHelpers.GetEdmPrimitiveTypeOrNull(colProperty.ElementType) != null)
                        {
                            continue;
                        }
                    }

                    IStructuralTypeConfiguration propertyType = GetStructuralTypeOrNull(property.RelatedClrType);
                    Contract.Assert(propertyType != null, "we should already have seen this type");

                    if (!visitedTypes.Contains(propertyType))
                    {
                        reachableTypes.Enqueue(propertyType);
                    }
                }

                // all derived types and the base type are also reachable
                IEntityTypeConfiguration currentEntityType = currentType as IEntityTypeConfiguration;
                if (currentEntityType != null)
                {
                    if (currentEntityType.BaseType != null && !visitedTypes.Contains(currentEntityType.BaseType))
                    {
                        reachableTypes.Enqueue(currentEntityType.BaseType);
                    }

                    foreach (IEntityTypeConfiguration derivedType in this.DerivedTypes(currentEntityType))
                    {
                        if (!visitedTypes.Contains(derivedType))
                        {
                            reachableTypes.Enqueue(derivedType);
                        }
                    }
                }

                visitedTypes.Add(currentType);
            }

            IStructuralTypeConfiguration[] allConfiguredTypes = StructuralTypes.ToArray();
            foreach (IStructuralTypeConfiguration type in allConfiguredTypes)
            {
                if (!visitedTypes.Contains(type))
                {
                    // we don't have to fix up any properties because this type is unreachable and cannot be a property of any reachable type.
                    RemoveStructuralType(type.ClrType);
                }
            }
        }

        private void ApplyTypeAndPropertyConventions(IStructuralTypeConfiguration edmTypeConfiguration)
        {
            foreach (IConvention convention in _conventions)
            {
                IEdmTypeConvention typeConvention = convention as IEdmTypeConvention;
                if (typeConvention != null)
                {
                    typeConvention.Apply(edmTypeConfiguration, this);
                }

                IEdmPropertyConvention propertyConvention = convention as IEdmPropertyConvention;
                if (propertyConvention != null)
                {
                    ApplyPropertyConvention(propertyConvention, edmTypeConfiguration);
                }
            }
        }

        private void ApplyEntitySetConventions(IEntitySetConfiguration entitySetConfiguration)
        {
            if (!_configuredEntitySets.Contains(entitySetConfiguration))
            {
                _configuredEntitySets.Add(entitySetConfiguration);

                foreach (IEntitySetConvention convention in _conventions.OfType<IEntitySetConvention>())
                {
                    if (convention != null)
                    {
                        convention.Apply(entitySetConfiguration, this);
                    }
                }
            }
        }

        private void ApplyProcedureConventions(ProcedureConfiguration procedure)
        {
            foreach (IProcedureConvention convention in _conventions.OfType<IProcedureConvention>())
            {
                convention.Apply(procedure, this);
            }
        }

        private IStructuralTypeConfiguration GetStructuralTypeOrNull(Type clrType)
        {
            return StructuralTypes.Where(edmType => edmType.ClrType == clrType).SingleOrDefault();
        }

        private static void ApplyPropertyConvention(IEdmPropertyConvention propertyConvention, IStructuralTypeConfiguration edmTypeConfiguration)
        {
            Contract.Assert(propertyConvention != null);
            Contract.Assert(edmTypeConfiguration != null);

            foreach (PropertyConfiguration property in edmTypeConfiguration.Properties.ToArray())
            {
                propertyConvention.Apply(property, edmTypeConfiguration);
            }
        }

        private static Dictionary<Type, List<Type>> BuildDerivedTypesMapping(IAssembliesResolver assemblyResolver)
        {
            IEnumerable<Type> allTypes = TypeHelper.GetLoadedTypes(assemblyResolver).Where(t => t.IsVisible && t.IsClass && t != typeof(object));
            Dictionary<Type, List<Type>> allTypeMapping = allTypes.ToDictionary(k => k, k => new List<Type>());

            foreach (Type type in allTypes)
            {
                List<Type> derivedTypes;
                if (type.BaseType != null && allTypeMapping.TryGetValue(type.BaseType, out derivedTypes))
                {
                    derivedTypes.Add(type);
                }
            }

            return allTypeMapping;
        }
    }
}
