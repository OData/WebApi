// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder.Conventions;
using System.Web.OData.Builder.Conventions.Attributes;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// <see cref="ODataConventionModelBuilder"/> is used to automatically map CLR classes to an EDM model based on a set of <see cref="IConvention"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Most of the referenced types are helper types needed for operation.")]
    public class ODataConventionModelBuilder : ODataModelBuilder
    {
        private static readonly List<IConvention> _conventions = new List<IConvention>
        {
            // type and property conventions (ordering is important here).
            new AbstractEntityTypeDiscoveryConvention(),
            new DataContractAttributeEdmTypeConvention(),
            new NotMappedAttributeConvention(), // NotMappedAttributeConvention has to run before EntityKeyConvention
            new DataMemberAttributeEdmPropertyConvention(),
            new RequiredAttributeEdmPropertyConvention(),
            new ConcurrencyCheckAttributeEdmPropertyConvention(),
            new TimestampAttributeEdmPropertyConvention(),
            new KeyAttributeEdmPropertyConvention(), // KeyAttributeEdmPropertyConvention has to run before EntityKeyConvention
            new EntityKeyConvention(),
            new ComplexTypeAttributeConvention(), // This has to run after Key conventions, basically overrules them if there is a ComplexTypeAttribute
            new IgnoreDataMemberAttributeEdmPropertyConvention(),
            new NonFilterableAttributeEdmPropertyConvention(),
            new UnsortableAttributeEdmPropertyConvention(),
            new NotNavigableAttributeEdmPropertyConvention(),
            new NotExpandableAttributeEdmPropertyConvention(),

            // INavigationSourceConvention's
            new SelfLinksGenerationConvention(),
            new NavigationLinksGenerationConvention(),
            new AssociationSetDiscoveryConvention(),

            // IEdmFunctionImportConventions's
            new ActionLinkGenerationConvention(),
            new FunctionLinkGenerationConvention(),
        };

        // These hashset's keep track of edmtypes/navigation sources for which conventions
        // have been applied or being applied so that we don't run a convention twice on the
        // same type/set.
        private HashSet<StructuralTypeConfiguration> _mappedTypes;
        private HashSet<INavigationSourceConfiguration> _configuredNavigationSources;
        private HashSet<Type> _ignoredTypes;

        private IEnumerable<StructuralTypeConfiguration> _explicitlyAddedTypes;

        private bool _isModelBeingBuilt;
        private bool _isQueryCompositionMode;

        // build the mapping between type and its derived types to be used later.
        private Lazy<IDictionary<Type, List<Type>>> _allTypesWithDerivedTypeMapping;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use.</param>
        /// <param name="isQueryCompositionMode">If the model is being built for only querying.</param>
        /// <remarks>The model built if <paramref name="isQueryCompositionMode"/> is <see langword="true"/> has more relaxed
        /// inference rules and also treats all types as entity types. This constructor is intended for use by unit testing only.</remarks>
        public ODataConventionModelBuilder(HttpConfiguration configuration, bool isQueryCompositionMode)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            Initialize(configuration.Services.GetAssembliesResolver(), isQueryCompositionMode);
        }

        /// <summary>
        /// Gets or sets if model aliasing is enabled or not. The default value is true.
        /// </summary>
        public bool ModelAliasingEnabled { get; set; }

        /// <summary>
        /// This action is invoked after the <see cref="ODataConventionModelBuilder"/> has run all the conventions, but before the configuration is locked
        /// down and used to build the <see cref="IEdmModel"/>.
        /// </summary>
        /// <remarks>Use this action to modify the <see cref="ODataModelBuilder"/> configuration that has been inferred by convention.</remarks>
        public Action<ODataConventionModelBuilder> OnModelCreating { get; set; }

        internal void Initialize(IAssembliesResolver assembliesResolver, bool isQueryCompositionMode)
        {
            _isQueryCompositionMode = isQueryCompositionMode;
            _configuredNavigationSources = new HashSet<INavigationSourceConfiguration>();
            _mappedTypes = new HashSet<StructuralTypeConfiguration>();
            _ignoredTypes = new HashSet<Type>();
            ModelAliasingEnabled = true;
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

        /// <inheritdoc />
        public override EntityTypeConfiguration AddEntityType(Type type)
        {
            EntityTypeConfiguration entityTypeConfiguration = base.AddEntityType(type);
            if (_isModelBeingBuilt)
            {
                MapType(entityTypeConfiguration);
            }

            return entityTypeConfiguration;
        }

        /// <inheritdoc />
        public override ComplexTypeConfiguration AddComplexType(Type type)
        {
            ComplexTypeConfiguration complexTypeConfiguration = base.AddComplexType(type);
            if (_isModelBeingBuilt)
            {
                MapType(complexTypeConfiguration);
            }

            return complexTypeConfiguration;
        }

        /// <inheritdoc />
        public override EntitySetConfiguration AddEntitySet(string name, EntityTypeConfiguration entityType)
        {
            EntitySetConfiguration entitySetConfiguration = base.AddEntitySet(name, entityType);
            if (_isModelBeingBuilt)
            {
                ApplyNavigationSourceConventions(entitySetConfiguration);
            }

            return entitySetConfiguration;
        }

        /// <inheritdoc />
        public override SingletonConfiguration AddSingleton(string name, EntityTypeConfiguration entityType)
        {
            SingletonConfiguration singletonConfiguration = base.AddSingleton(name, entityType);
            if (_isModelBeingBuilt)
            {
                ApplyNavigationSourceConventions(singletonConfiguration);
            }

            return singletonConfiguration;
        }

        /// <inheritdoc />
        public override EnumTypeConfiguration AddEnumType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (!type.IsEnum)
            {
                throw Error.Argument("type", SRResources.TypeCannotBeEnum, type.FullName);
            }

            EnumTypeConfiguration enumTypeConfiguration = EnumTypes.SingleOrDefault(e => e.ClrType == type);

            if (enumTypeConfiguration == null)
            {
                enumTypeConfiguration = base.AddEnumType(type);

                foreach (object member in Enum.GetValues(type))
                {
                    enumTypeConfiguration.AddMember((Enum)member);
                }
            }

            return enumTypeConfiguration;
        }

        /// <inheritdoc />
        public override IEdmModel GetEdmModel()
        {
            if (_isModelBeingBuilt)
            {
                throw Error.NotSupported(SRResources.GetEdmModelCalledMoreThanOnce);
            }

            // before we begin, get the set of types the user had added explicitly.
            _explicitlyAddedTypes = new List<StructuralTypeConfiguration>(StructuralTypes);

            _isModelBeingBuilt = true;

            MapTypes();

            DiscoverInheritanceRelationships();

            // Don't RediscoverComplexTypes() and treat everything as an entity type if buidling a model for EnableQueryAttribute.
            if (!_isQueryCompositionMode)
            {
                RediscoverComplexTypes();
            }

            // prune unreachable types
            PruneUnreachableTypes();

            // Apply navigation source conventions.
            IEnumerable<INavigationSourceConfiguration> explictlyConfiguredNavigationSource =
                new List<INavigationSourceConfiguration>(NavigationSources);
            foreach (INavigationSourceConfiguration navigationSource in explictlyConfiguredNavigationSource)
            {
                ApplyNavigationSourceConventions(navigationSource);
            }

            foreach (ProcedureConfiguration procedure in Procedures)
            {
                ApplyProcedureConventions(procedure);
            }

            if (OnModelCreating != null)
            {
                OnModelCreating(this);
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
            Dictionary<Type, EntityTypeConfiguration> entityMap = StructuralTypes.OfType<EntityTypeConfiguration>().ToDictionary(e => e.ClrType);

            foreach (EntityTypeConfiguration entity in StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => !e.BaseTypeConfigured))
            {
                Type baseClrType = entity.ClrType.BaseType;
                while (baseClrType != null)
                {
                    // see if we there is an entity that we know mapping to this clr types base type.
                    EntityTypeConfiguration baseEntityType;
                    if (entityMap.TryGetValue(baseClrType, out baseEntityType))
                    {
                        RemoveBaseTypeProperties(entity, baseEntityType);

                        // disable derived type key check if we are building a model for query composition.
                        if (_isQueryCompositionMode)
                        {
                            // modifying the collection in the iterator, hence the ToArray().
                            foreach (PrimitivePropertyConfiguration keyProperty in entity.Keys.ToArray())
                            {
                                entity.RemoveKey(keyProperty);
                            }
                        }

                        entity.DerivesFrom(baseEntityType);
                        break;
                    }

                    baseClrType = baseClrType.BaseType;
                }
            }
        }

        internal void MapDerivedTypes(EntityTypeConfiguration entity)
        {
            HashSet<Type> visitedEntities = new HashSet<Type>();

            Queue<EntityTypeConfiguration> entitiesToBeVisited = new Queue<EntityTypeConfiguration>();
            entitiesToBeVisited.Enqueue(entity);

            // populate all the derived types
            while (entitiesToBeVisited.Count != 0)
            {
                EntityTypeConfiguration baseEntity = entitiesToBeVisited.Dequeue();
                visitedEntities.Add(baseEntity.ClrType);

                List<Type> derivedTypes;
                if (_allTypesWithDerivedTypeMapping.Value.TryGetValue(baseEntity.ClrType, out derivedTypes))
                {
                    foreach (Type derivedType in derivedTypes)
                    {
                        if (!visitedEntities.Contains(derivedType) && !IsIgnoredType(derivedType))
                        {
                            EntityTypeConfiguration derivedEntity = AddEntityType(derivedType);
                            entitiesToBeVisited.Enqueue(derivedEntity);
                        }
                    }
                }
            }
        }

        // remove base type properties from the derived types.
        internal void RemoveBaseTypeProperties(EntityTypeConfiguration derivedEntity, EntityTypeConfiguration baseEntity)
        {
            IEnumerable<EntityTypeConfiguration> typesToLift = new[] { derivedEntity }.Concat(this.DerivedTypes(derivedEntity));

            foreach (PropertyConfiguration property in baseEntity.Properties.Concat(baseEntity.DerivedProperties()))
            {
                foreach (EntityTypeConfiguration entity in typesToLift)
                {
                    PropertyConfiguration derivedPropertyToRemove = entity.Properties.SingleOrDefault(
                        p => p.PropertyInfo.Name == property.PropertyInfo.Name);
                    if (derivedPropertyToRemove != null)
                    {
                        entity.RemoveProperty(derivedPropertyToRemove.PropertyInfo);
                    }
                }
            }

            foreach (PropertyInfo ignoredProperty in baseEntity.IgnoredProperties())
            {
                foreach (EntityTypeConfiguration entity in typesToLift)
                {
                    PropertyConfiguration derivedPropertyToRemove = entity.Properties.SingleOrDefault(
                        p => p.PropertyInfo.Name == ignoredProperty.Name);
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

            EntityTypeConfiguration[] misconfiguredEntityTypes = StructuralTypes
                                                                            .Except(_explicitlyAddedTypes)
                                                                            .OfType<EntityTypeConfiguration>()
                                                                            .Where(entity => !entity.Keys().Any())
                                                                            .ToArray();

            ReconfigureEntityTypesAsComplexType(misconfiguredEntityTypes);
        }

        private void ReconfigureEntityTypesAsComplexType(EntityTypeConfiguration[] misconfiguredEntityTypes)
        {
            IEnumerable<EntityTypeConfiguration> actualEntityTypes = StructuralTypes
                                                                            .Except(misconfiguredEntityTypes)
                                                                            .OfType<EntityTypeConfiguration>();

            foreach (EntityTypeConfiguration misconfiguredEntityType in misconfiguredEntityTypes)
            {
                RemoveStructuralType(misconfiguredEntityType.ClrType);

                // this is a wrongly inferred type. so just ignore any pending configuration from it.
                AddComplexType(misconfiguredEntityType.ClrType);

                foreach (EntityTypeConfiguration entityToBePatched in actualEntityTypes)
                {
                    NavigationPropertyConfiguration[] propertiesToBeRemoved = entityToBePatched
                                                                            .NavigationProperties
                                                                            .Where(navigationProperty => navigationProperty.RelatedClrType == misconfiguredEntityType.ClrType)
                                                                            .ToArray();
                    foreach (NavigationPropertyConfiguration propertyToBeRemoved in propertiesToBeRemoved)
                    {
                        string propertyNameAlias = propertyToBeRemoved.Name;
                        PropertyConfiguration propertyConfiguration;

                        entityToBePatched.RemoveProperty(propertyToBeRemoved.PropertyInfo);

                        if (propertyToBeRemoved.Multiplicity == EdmMultiplicity.Many)
                        {
                            propertyConfiguration = entityToBePatched.AddCollectionProperty(propertyToBeRemoved.PropertyInfo);
                        }
                        else
                        {
                            propertyConfiguration = entityToBePatched.AddComplexProperty(propertyToBeRemoved.PropertyInfo);
                        }

                        propertyConfiguration.Name = propertyNameAlias;
                    }
                }
            }
        }

        private void MapTypes()
        {
            foreach (StructuralTypeConfiguration edmType in _explicitlyAddedTypes)
            {
                MapType(edmType);
            }
        }

        private void MapType(StructuralTypeConfiguration edmType)
        {
            if (!_mappedTypes.Contains(edmType))
            {
                _mappedTypes.Add(edmType);
                EntityTypeConfiguration entity = edmType as EntityTypeConfiguration;
                if (entity != null)
                {
                    MapEntityType(entity);
                }
                else
                {
                    MapComplexType(edmType as ComplexTypeConfiguration);
                }

                ApplyTypeAndPropertyConventions(edmType);
            }
        }

        private void MapEntityType(EntityTypeConfiguration entity)
        {
            IEnumerable<PropertyInfo> properties = ConventionsHelpers.GetProperties(entity, includeReadOnly: _isQueryCompositionMode);
            foreach (PropertyInfo property in properties)
            {
                bool isCollection;
                IEdmTypeConfiguration mappedType;

                PropertyKind propertyKind = GetPropertyType(property, out isCollection, out mappedType);

                if (propertyKind == PropertyKind.Primitive || propertyKind == PropertyKind.Complex || propertyKind == PropertyKind.Enum)
                {
                    MapStructuralProperty(entity, property, propertyKind, isCollection);
                }
                else if (propertyKind == PropertyKind.Dynamic)
                {
                    // Skip the dynamic property for entity type because open entity type is not supported.
                }
                else
                {
                    // don't add this property if the user has already added it.
                    if (!entity.NavigationProperties.Any(p => p.Name == property.Name))
                    {
                        NavigationPropertyConfiguration addedNavigationProperty;
                        if (!isCollection)
                        {
                            addedNavigationProperty = entity.AddNavigationProperty(property, EdmMultiplicity.ZeroOrOne);
                        }
                        else
                        {
                            addedNavigationProperty = entity.AddNavigationProperty(property, EdmMultiplicity.Many);
                        }

                        ContainedAttribute containedAttribute = property.GetCustomAttribute<ContainedAttribute>();
                        if (containedAttribute != null)
                        {
                            addedNavigationProperty.Contained();
                        }

                        addedNavigationProperty.AddedExplicitly = false;
                    }
                }
            }

            MapDerivedTypes(entity);
        }

        private void MapComplexType(ComplexTypeConfiguration complexType)
        {
            IEnumerable<PropertyInfo> properties = ConventionsHelpers.GetAllProperties(complexType, includeReadOnly: _isQueryCompositionMode);
            foreach (PropertyInfo property in properties)
            {
                bool isCollection;
                IEdmTypeConfiguration mappedType;

                PropertyKind propertyKind = GetPropertyType(property, out isCollection, out mappedType);

                if (propertyKind == PropertyKind.Primitive || propertyKind == PropertyKind.Complex || propertyKind == PropertyKind.Enum)
                {
                    MapStructuralProperty(complexType, property, propertyKind, isCollection);
                }
                else if (propertyKind == PropertyKind.Dynamic)
                {
                    complexType.AddDynamicPropertyDictionary(property);
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
                        EntityTypeConfiguration mappedTypeAsEntity = mappedType as EntityTypeConfiguration;
                        Contract.Assert(mappedTypeAsEntity != null);

                        ReconfigureEntityTypesAsComplexType(new EntityTypeConfiguration[] { mappedTypeAsEntity });

                        MapStructuralProperty(complexType, property, PropertyKind.Complex, isCollection);
                    }
                }
            }
        }

        private void MapStructuralProperty(StructuralTypeConfiguration type, PropertyInfo property, PropertyKind propertyKind, bool isCollection)
        {
            Contract.Assert(type != null);
            Contract.Assert(property != null);
            Contract.Assert(propertyKind == PropertyKind.Complex || propertyKind == PropertyKind.Primitive || propertyKind == PropertyKind.Enum);

            bool addedExplicitly = type.Properties.Any(p => p.PropertyInfo.Name == property.Name);

            PropertyConfiguration addedEdmProperty;
            if (!isCollection)
            {
                if (propertyKind == PropertyKind.Primitive)
                {
                    addedEdmProperty = type.AddProperty(property);
                }
                else if (propertyKind == PropertyKind.Enum)
                {
                    AddEnumType(TypeHelper.GetUnderlyingTypeOrSelf(property.PropertyType));
                    addedEdmProperty = type.AddEnumProperty(property);
                }
                else
                {
                    addedEdmProperty = type.AddComplexProperty(property);
                }
            }
            else
            {
                if (_isQueryCompositionMode)
                {
                    Contract.Assert(propertyKind != PropertyKind.Complex, "we don't create complex types in query composition mode.");
                }

                if (property.PropertyType.IsGenericType)
                {
                    Type elementType = property.PropertyType.GetGenericArguments().First();
                    Type elementUnderlyingTypeOrSelf = TypeHelper.GetUnderlyingTypeOrSelf(elementType);

                    if (elementUnderlyingTypeOrSelf.IsEnum)
                    {
                        AddEnumType(elementUnderlyingTypeOrSelf);
                    }
                }

                addedEdmProperty = type.AddCollectionProperty(property);
            }

            addedEdmProperty.AddedExplicitly = addedExplicitly;
        }

        // figures out the type of the property (primitive, complex, navigation) and the corresponding edm type if we have seen this type
        // earlier or the user told us about it.
        private PropertyKind GetPropertyType(PropertyInfo property, out bool isCollection, out IEdmTypeConfiguration mappedType)
        {
            Contract.Assert(property != null);

            // IDictionary<string, object> is used as a container to save/retrieve dynamic properties for an open type.
            // It is different from other collections (for example, IEnumerable<T> or IDictionary<string, int>)
            // which are used as navigation properties.
            if (typeof(IDictionary<string, object>).IsAssignableFrom(property.PropertyType))
            {
                mappedType = null;
                isCollection = false;
                return PropertyKind.Dynamic;
            }

            PropertyKind propertyKind;
            if (TryGetPropertyTypeKind(property.PropertyType, out mappedType, out propertyKind))
            {
                isCollection = false;
                return propertyKind;
            }

            Type elementType;
            if (property.PropertyType.IsCollection(out elementType))
            {
                isCollection = true;
                if (TryGetPropertyTypeKind(elementType, out mappedType, out propertyKind))
                {
                    return propertyKind;
                }

                // if we know nothing about this type we assume it to be collection of entities
                // and patch up later
                return PropertyKind.Navigation;
            }

            // if we know nothing about this type we assume it to be an entity
            // and patch up later
            isCollection = false;
            return PropertyKind.Navigation;
        }

        private bool TryGetPropertyTypeKind(Type propertyType, out IEdmTypeConfiguration mappedType, out PropertyKind propertyKind)
        {
            Contract.Assert(propertyType != null);

            if (EdmLibHelpers.GetEdmPrimitiveTypeOrNull(propertyType) != null)
            {
                mappedType = null;
                propertyKind = PropertyKind.Primitive;
                return true;
            }

            mappedType = GetStructuralTypeOrNull(propertyType);
            if (mappedType != null)
            {
                if (mappedType is ComplexTypeConfiguration)
                {
                    propertyKind = PropertyKind.Complex;
                }
                else if (mappedType is EnumTypeConfiguration)
                {
                    propertyKind = PropertyKind.Enum;
                }
                else
                {
                    propertyKind = PropertyKind.Navigation;
                }

                return true;
            }

            if (TypeHelper.IsEnum(propertyType))
            {
                propertyKind = PropertyKind.Enum;
                return true;
            }

            propertyKind = PropertyKind.Navigation;
            return false;
        }

        // the convention model builder MapTypes() method might have went through deep object graphs and added a bunch of types
        // only to realise after applying the conventions that the user has ignored some of the properties. So, prune the unreachable stuff.
        private void PruneUnreachableTypes()
        {
            Contract.Assert(_explicitlyAddedTypes != null);

            // Do a BFS starting with the types the user has explicitly added to find out the unreachable nodes.
            Queue<StructuralTypeConfiguration> reachableTypes = new Queue<StructuralTypeConfiguration>(_explicitlyAddedTypes);
            HashSet<StructuralTypeConfiguration> visitedTypes = new HashSet<StructuralTypeConfiguration>();

            while (reachableTypes.Count != 0)
            {
                StructuralTypeConfiguration currentType = reachableTypes.Dequeue();

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

                    IEdmTypeConfiguration propertyType = GetStructuralTypeOrNull(property.RelatedClrType);
                    Contract.Assert(propertyType != null, "we should already have seen this type");

                    var structuralTypeConfiguration = propertyType as StructuralTypeConfiguration;
                    if (structuralTypeConfiguration != null && !visitedTypes.Contains(propertyType))
                    {
                        reachableTypes.Enqueue(structuralTypeConfiguration);
                    }
                }

                // all derived types and the base type are also reachable
                EntityTypeConfiguration currentEntityType = currentType as EntityTypeConfiguration;
                if (currentEntityType != null)
                {
                    if (currentEntityType.BaseType != null && !visitedTypes.Contains(currentEntityType.BaseType))
                    {
                        reachableTypes.Enqueue(currentEntityType.BaseType);
                    }

                    foreach (EntityTypeConfiguration derivedType in this.DerivedTypes(currentEntityType))
                    {
                        if (!visitedTypes.Contains(derivedType))
                        {
                            reachableTypes.Enqueue(derivedType);
                        }
                    }
                }

                visitedTypes.Add(currentType);
            }

            StructuralTypeConfiguration[] allConfiguredTypes = StructuralTypes.ToArray();
            foreach (StructuralTypeConfiguration type in allConfiguredTypes)
            {
                if (!visitedTypes.Contains(type))
                {
                    // we don't have to fix up any properties because this type is unreachable and cannot be a property of any reachable type.
                    RemoveStructuralType(type.ClrType);
                }
            }
        }

        private void ApplyTypeAndPropertyConventions(StructuralTypeConfiguration edmTypeConfiguration)
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

        private void ApplyNavigationSourceConventions(INavigationSourceConfiguration navigationSourceConfiguration)
        {
            if (!_configuredNavigationSources.Contains(navigationSourceConfiguration))
            {
                _configuredNavigationSources.Add(navigationSourceConfiguration);

                foreach (INavigationSourceConvention convention in _conventions.OfType<INavigationSourceConvention>())
                {
                    if (convention != null)
                    {
                        convention.Apply(navigationSourceConfiguration, this);
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

        private IEdmTypeConfiguration GetStructuralTypeOrNull(Type clrType)
        {
            IEdmTypeConfiguration configuration = StructuralTypes.SingleOrDefault(edmType => edmType.ClrType == clrType);
            if (configuration == null)
            {
                Type type = TypeHelper.GetUnderlyingTypeOrSelf(clrType);
                configuration = EnumTypes.SingleOrDefault(edmType => edmType.ClrType == type);
            }

            return configuration;
        }

        private void ApplyPropertyConvention(IEdmPropertyConvention propertyConvention, StructuralTypeConfiguration edmTypeConfiguration)
        {
            Contract.Assert(propertyConvention != null);
            Contract.Assert(edmTypeConfiguration != null);

            foreach (PropertyConfiguration property in edmTypeConfiguration.Properties.ToArray())
            {
                propertyConvention.Apply(property, edmTypeConfiguration, this);
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

        /// <inheritdoc />
        public override void ValidateModel(IEdmModel model)
        {
            if (!_isQueryCompositionMode)
            {
                base.ValidateModel(model);
            }
        }
    }
}
