// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Web.Http.OData.Builder.Conventions;
using System.Web.Http.OData.Builder.Conventions.Attributes;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    public class ODataConventionModelBuilder : ODataModelBuilder
    {
        private static readonly List<IConvention> _conventions = new List<IConvention>
        {
            // IEdmTypeConvention's
            new EntityKeyConvention(),
            
            // IEntitySetConvention's
            new SelfLinksGenerationConvention(),
            new NavigationLinksGenerationConvention(),

            // IEdmPropertyConvention's
            new NotMappedAttributeConvention(),
            new KeyAttributeConvention(),
        };

        // These hashset's keep track of edmtypes/entitysets for which conventions
        // have been applied or being applied so that we don't run a convention twice on the
        // same type/set.
        private HashSet<IStructuralTypeConfiguration> _configuredTypes;
        private HashSet<IEntitySetConfiguration> _configuredEntitySets;

        private IEnumerable<IStructuralTypeConfiguration> _explicitlyAddedTypes;

        private bool _isModelBeingBuilt;
        private bool _isQueryCompositionMode;
        private bool _conventionsBeingApplied;

        public ODataConventionModelBuilder()
            : this(isQueryCompositionMode: false)
        {
        }

        internal ODataConventionModelBuilder(bool isQueryCompositionMode)
        {
            _isQueryCompositionMode = isQueryCompositionMode;
            _configuredEntitySets = new HashSet<IEntitySetConfiguration>();
            _configuredTypes = new HashSet<IStructuralTypeConfiguration>();
        }

        public override IEntityTypeConfiguration AddEntity(Type type)
        {
            bool alreadyExists = (GetStructuralTypeOrNull(type) != null);

            IEntityTypeConfiguration entityTypeConfiguration = base.AddEntity(type);
            if (_isModelBeingBuilt && !alreadyExists)
            {
                MapEntityType(entityTypeConfiguration);

                if (_conventionsBeingApplied)
                {
                    ApplyTypeConventions(entityTypeConfiguration);
                    ApplyPropertyConventions(entityTypeConfiguration);
                }
            }

            return entityTypeConfiguration;
        }

        public override IComplexTypeConfiguration AddComplexType(Type type)
        {
            bool alreadyExists = (GetStructuralTypeOrNull(type) != null);

            IComplexTypeConfiguration complexTypeConfiguration = base.AddComplexType(type);
            if (_isModelBeingBuilt && !alreadyExists)
            {
                MapComplexType(complexTypeConfiguration);

                if (_conventionsBeingApplied)
                {
                    ApplyTypeConventions(complexTypeConfiguration);
                    ApplyPropertyConventions(complexTypeConfiguration);
                }
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

            _conventionsBeingApplied = true;

            // Apply type conventions. Note the call to ToArray() is required as the StructuralTypes collection
            // could get modified during ApplyTypeConventions().
            foreach (IStructuralTypeConfiguration edmTypeConfiguration in StructuralTypes.ToArray())
            {
                ApplyTypeConventions(edmTypeConfiguration);
            }

            // Apply property conventions. Note the call to ToArray() is required as the StructuralTypes collection
            // could get modified during ApplyPropertyConventions(). Also, type conventions might have
            // modified this. So, call ToArray() again.
            foreach (IStructuralTypeConfiguration edmTypeConfiguration in StructuralTypes.ToArray())
            {
                ApplyPropertyConventions(edmTypeConfiguration);
            }

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

            return base.GetEdmModel();
        }

        private void RediscoverComplexTypes()
        {
            Contract.Assert(_explicitlyAddedTypes != null);

            IEnumerable<IEntityTypeConfiguration> misconfiguredEntityTypes = StructuralTypes
                                                                            .Except(_explicitlyAddedTypes)
                                                                            .OfType<IEntityTypeConfiguration>()
                                                                            .Where(entity => !entity.Keys.Any())
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
                foreach (var ignoredProperty in misconfiguredEntityType.IgnoredProperties)
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
                        if (propertyToBeRemoved.Multiplicity == EdmMultiplicity.Many)
                        {
                            // complex collections are not supported.
                            throw Error.NotSupported(SRResources.CollectionPropertiesNotSupported, propertyToBeRemoved.PropertyInfo.Name, propertyToBeRemoved.PropertyInfo.ReflectedType.FullName);
                        }
                        entityToBePatched.RemoveProperty(propertyToBeRemoved.PropertyInfo);
                        entityToBePatched.AddComplexProperty(propertyToBeRemoved.PropertyInfo);
                    }
                }

                AddComplexType(misconfiguredEntityType.ClrType);
            }
        }

        private void MapTypes()
        {
            foreach (IStructuralTypeConfiguration edmType in _explicitlyAddedTypes)
            {
                IEntityTypeConfiguration entity = edmType as IEntityTypeConfiguration;
                if (entity != null)
                {
                    MapEntityType(entity);
                }
                else
                {
                    MapComplexType(edmType as IComplexTypeConfiguration);
                }
            }
        }

        private void MapEntityType(IEntityTypeConfiguration entity)
        {
            PropertyInfo[] properties = ConventionsHelpers.GetProperties(entity);
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
        }

        private void MapComplexType(IComplexTypeConfiguration complexType)
        {
            PropertyInfo[] properties = ConventionsHelpers.GetProperties(complexType);
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
                    if (!isCollection)
                    {
                        if (_explicitlyAddedTypes.Contains(mappedType))
                        {
                            // user told us that this an entity type.
                            throw Error.InvalidOperation(SRResources.ComplexTypeRefersToEntityType, complexType.ClrType.FullName, mappedType.ClrType.FullName, property.Name);
                        }
                        else
                        {
                            // we tried to be over-smart earlier and made the bad choice. so patch up now.
                            ReconfigureEntityTypesAsComplexType(new IEntityTypeConfiguration[] { mappedType as IEntityTypeConfiguration });
                            complexType.AddComplexProperty(property);
                        }
                    }
                    else
                    {
                        throw Error.NotSupported(SRResources.CollectionPropertiesNotSupported, property.Name, property.ReflectedType.FullName);
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
                if (!_isQueryCompositionMode)
                {
                    throw Error.NotSupported(SRResources.CollectionPropertiesNotSupported, property.Name, property.ReflectedType.FullName);
                }
                else
                {
                    Contract.Assert(propertyKind != PropertyKind.Complex, "we don't create complex types in query composition mode.");
                    // Ignore these primitive collection properties. They cannot be queried anyways.
                }
            }
        }

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
                                return PropertyKind.Navigation;
                            }
                        }
                    }
                    else
                    {
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
                    IStructuralTypeConfiguration propertyType = GetStructuralTypeOrNull(property.RelatedClrType);
                    Contract.Assert(propertyType != null, "we should already have seen this type");

                    if (!visitedTypes.Contains(propertyType))
                    {
                        reachableTypes.Enqueue(propertyType);
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

        private void ApplyTypeConventions(IStructuralTypeConfiguration edmTypeConfiguration)
        {
            if (!_configuredTypes.Contains(edmTypeConfiguration))
            {
                _configuredTypes.Add(edmTypeConfiguration);

                foreach (IEdmTypeConvention convention in _conventions.OfType<IEdmTypeConvention>())
                {
                    if (convention != null)
                    {
                        convention.Apply(edmTypeConfiguration, this);
                    }
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

        private IStructuralTypeConfiguration GetStructuralTypeOrNull(Type clrType)
        {
            return StructuralTypes.Where(edmType => edmType.ClrType == clrType).SingleOrDefault();
        }

        private static void ApplyPropertyConventions(IStructuralTypeConfiguration edmTypeConfiguration)
        {
            foreach (PropertyConfiguration property in edmTypeConfiguration.Properties.ToArray())
            {
                foreach (IEdmPropertyConvention propertyConvention in _conventions.OfType<IEdmPropertyConvention>())
                {
                    propertyConvention.Apply(property, edmTypeConfiguration);
                }
            }
        }
    }
}
