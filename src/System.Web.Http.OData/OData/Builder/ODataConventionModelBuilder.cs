// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Web.Http.OData.Builder.Conventions;
using System.Web.Http.OData.Formatter;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    public class ODataConventionModelBuilder : ODataModelBuilder
    {
        private static readonly IConvention[] _conventions = 
        {
            // IEdmTypeConvention's
            new EntityKeyConvention(),
            
            // IEntitySetConvention's
            new SelfLinksGenerationConvention(),
            new NavigationLinksGenerationConvention(),

            // IEdmPropertyConvention's
            new KeyAttributeConvention(),
        };

        // These hashset's keep track of edmtypes/entitysets for which conventions
        // have been applied or being applied so that we don't run a convention twice on the
        // same type/set.
        private HashSet<IStructuralTypeConfiguration> _configuredTypes;
        private HashSet<IEntitySetConfiguration> _configuredEntitySets;

        private bool _isModelBeingBuilt;

        public ODataConventionModelBuilder()
        {
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
                ApplyTypeConventions(entityTypeConfiguration);
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
                ApplyTypeConventions(complexTypeConfiguration);
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
            _isModelBeingBuilt = true;
            IEnumerable<IStructuralTypeConfiguration> explicitlyAddedTypes = new List<IStructuralTypeConfiguration>(StructuralTypes);

            // Map Types
            MapTypes();

            // configure edm types
            IEnumerable<IStructuralTypeConfiguration> edmTypes = new List<IStructuralTypeConfiguration>(StructuralTypes);
            foreach (IStructuralTypeConfiguration edmTypeConfiguration in edmTypes)
            {
                ApplyTypeConventions(edmTypeConfiguration);
            }

            // configure properties of edm types
            foreach (IStructuralTypeConfiguration edmTypeConfiguration in StructuralTypes)
            {
                foreach (PropertyConfiguration property in edmTypeConfiguration.Properties)
                {
                    ApplyPropertyConventions(property, edmTypeConfiguration);
                }
            }

            // re-discover complex types
            RediscoverComplexTypes(explicitlyAddedTypes);

            // configure entity sets
            IEnumerable<IEntitySetConfiguration> explictlyConfiguredEntitySets = new List<IEntitySetConfiguration>(EntitySets);
            foreach (IEntitySetConfiguration entitySet in explictlyConfiguredEntitySets)
            {
                ApplyEntitySetConventions(entitySet);
            }

            return base.GetEdmModel();
        }

        private void RediscoverComplexTypes(IEnumerable<IStructuralTypeConfiguration> explicitlyAddedTypes)
        {
            IEnumerable<IEntityTypeConfiguration> misconfiguredEntityTypes = StructuralTypes
                                                                            .Except(explicitlyAddedTypes)
                                                                            .OfType<IEntityTypeConfiguration>()
                                                                            .Where(entity => !entity.Keys.Any())
                                                                            .ToArray();

            IEnumerable<IEntityTypeConfiguration> actualEntityTypes = StructuralTypes
                                                                            .Except(misconfiguredEntityTypes)
                                                                            .OfType<IEntityTypeConfiguration>()
                                                                            .ToArray();

            foreach (IEntityTypeConfiguration misconfiguredEntityType in misconfiguredEntityTypes)
            {
                RemoveStructuralType(misconfiguredEntityType.ClrType);

                foreach (IEntityTypeConfiguration entityToBePatched in actualEntityTypes)
                {
                    NavigationPropertyConfiguration[] propertiesToBeRemoved = entityToBePatched
                                                                            .NavigationProperties
                                                                            .Where(navigationProperty => navigationProperty.RelatedClrType == misconfiguredEntityType.ClrType)
                                                                            .ToArray();
                    foreach (NavigationPropertyConfiguration propertyToBeRemoved in propertiesToBeRemoved)
                    {
                        entityToBePatched.RemoveProperty(propertyToBeRemoved.PropertyInfo);
                        entityToBePatched.AddComplexProperty(propertyToBeRemoved.PropertyInfo);
                    }
                }

                AddComplexType(misconfiguredEntityType.ClrType);
            }
        }

        private void MapTypes()
        {
            IEnumerable<IStructuralTypeConfiguration> explictlyConfiguredTypes = new List<IStructuralTypeConfiguration>(StructuralTypes);
            foreach (IStructuralTypeConfiguration edmType in explictlyConfiguredTypes)
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
            PropertyInfo[] properties = ConventionsHelpers.GetProperties(entity.ClrType);
            foreach (PropertyInfo property in properties)
            {
                if (EdmLibHelpers.GetEdmPrimitiveTypeOrNull(property.PropertyType) != null)
                {
                    PrimitivePropertyConfiguration primitiveProperty = entity.AddProperty(property);
                    primitiveProperty.OptionalProperty = IsNullable(property.PropertyType);
                }
                else
                {
                    IStructuralTypeConfiguration mappedType = GetStructuralTypeOrNull(property.PropertyType);
                    if (mappedType != null)
                    {
                        if (mappedType is IComplexTypeConfiguration)
                        {
                            entity.AddComplexProperty(property);
                        }
                        else
                        {
                            entity.AddNavigationProperty(property, property.PropertyType.IsCollection() ? EdmMultiplicity.Many : EdmMultiplicity.ZeroOrOne);
                        }
                    }
                    else
                    {
                        // we are not really sure if this should be a complex property or an navigation property.
                        // Assume that it is a navigation property and patch later in RediscoverComplexTypes().
                        entity.AddNavigationProperty(property, property.PropertyType.IsCollection() ? EdmMultiplicity.Many : EdmMultiplicity.ZeroOrOne);
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822: MarkMembersAsStatic", Justification = "To be consistent with MapEntityType")]
        private void MapComplexType(IComplexTypeConfiguration complexType)
        {
            PropertyInfo[] properties = ConventionsHelpers.GetProperties(complexType.ClrType);
            foreach (PropertyInfo property in properties)
            {
                if (EdmLibHelpers.GetEdmPrimitiveTypeOrNull(property.PropertyType) != null)
                {
                    PrimitivePropertyConfiguration primitiveProperty = complexType.AddProperty(property);
                    primitiveProperty.OptionalProperty = IsNullable(property.PropertyType);
                }
                else
                {
                    complexType.AddComplexProperty(property);
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

        private static bool IsNullable(Type type)
        {
            return type.IsClass || Nullable.GetUnderlyingType(type) != null;
        }

        private static void ApplyPropertyConventions(PropertyConfiguration property, IStructuralTypeConfiguration edmTypeConfiguration)
        {
            foreach (IConvention convention in _conventions)
            {
                PrimitivePropertyConfiguration primitivePropertyConfiguration;
                ComplexPropertyConfiguration complexPropertyConfiguration;
                NavigationPropertyConfiguration navigationPropertyConfiguration;

                if ((primitivePropertyConfiguration = property as PrimitivePropertyConfiguration) != null)
                {
                    IEdmPropertyConvention<PrimitivePropertyConfiguration> propertyConfigurationConvention = convention as IEdmPropertyConvention<PrimitivePropertyConfiguration>;
                    if (propertyConfigurationConvention != null)
                    {
                        ApplyPropertyConvention(primitivePropertyConfiguration, edmTypeConfiguration, propertyConfigurationConvention);
                    }
                }
                else if ((complexPropertyConfiguration = property as ComplexPropertyConfiguration) != null)
                {
                    IEdmPropertyConvention<ComplexPropertyConfiguration> propertyConfigurationConvention = convention as IEdmPropertyConvention<ComplexPropertyConfiguration>;
                    if (propertyConfigurationConvention != null)
                    {
                        ApplyPropertyConvention(complexPropertyConfiguration, edmTypeConfiguration, propertyConfigurationConvention);
                    }
                }
                else if ((navigationPropertyConfiguration = property as NavigationPropertyConfiguration) != null)
                {
                    IEdmPropertyConvention<NavigationPropertyConfiguration> propertyConfigurationConvention = convention as IEdmPropertyConvention<NavigationPropertyConfiguration>;
                    if (propertyConfigurationConvention != null)
                    {
                        ApplyPropertyConvention(navigationPropertyConfiguration, edmTypeConfiguration, propertyConfigurationConvention);
                    }
                }
            }
        }

        private static void ApplyPropertyConvention<TPropertyConfiguration>(
            TPropertyConfiguration property,
            IStructuralTypeConfiguration structuralTypeConfiguration,
            IEdmPropertyConvention<TPropertyConfiguration> convention)
            where TPropertyConfiguration : PropertyConfiguration
        {
            convention.Apply(property, structuralTypeConfiguration);
        }
    }
}
