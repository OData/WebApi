// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// <see cref="EdmTypeBuilder"/> builds <see cref="IEdmType"/>'s from <see cref=" StructuralTypeConfiguration"/>'s.
    /// </summary>
    internal class EdmTypeBuilder
    {
        private readonly List<StructuralTypeConfiguration> _configurations;
        private readonly Dictionary<Type, IEdmStructuredType> _types = new Dictionary<Type, IEdmStructuredType>();
        private readonly Dictionary<PropertyInfo, IEdmProperty> _properties = new Dictionary<PropertyInfo, IEdmProperty>();
        private readonly Dictionary<IEdmProperty, QueryableRestrictions> _propertiesRestrictions = new Dictionary<IEdmProperty, QueryableRestrictions>();

        internal EdmTypeBuilder(IEnumerable<StructuralTypeConfiguration> configurations)
        {
            _configurations = configurations.ToList();
        }

        private Dictionary<Type, IEdmStructuredType> GetEdmTypes()
        {
            // Reset
            _types.Clear();
            _properties.Clear();

            // Create headers to allow CreateEdmTypeBody to blindly references other things.
            foreach (StructuralTypeConfiguration config in _configurations)
            {
                CreateEdmTypeHeader(config);
            }

            foreach (StructuralTypeConfiguration config in _configurations)
            {
                CreateEdmTypeBody(config);
            }

            return _types;
        }

        private void CreateEdmTypeHeader(StructuralTypeConfiguration config)
        {
            if (!_types.ContainsKey(config.ClrType))
            {
                if (config.Kind == EdmTypeKind.Complex)
                {
                    _types.Add(config.ClrType, new EdmComplexType(config.Namespace, config.Name));
                }
                else
                {
                    EntityTypeConfiguration entity = config as EntityTypeConfiguration;
                    Contract.Assert(entity != null);

                    IEdmEntityType baseType = null;

                    if (entity.BaseType != null)
                    {
                        CreateEdmTypeHeader(entity.BaseType);
                        baseType = _types[entity.BaseType.ClrType] as IEdmEntityType;

                        Contract.Assert(baseType != null);
                    }

                    _types.Add(config.ClrType, new EdmEntityType(config.Namespace, config.Name, baseType, entity.IsAbstract ?? false, isOpen: false));
                }
            }
        }

        private void CreateEdmTypeBody(StructuralTypeConfiguration config)
        {
            IEdmType edmType = _types[config.ClrType];

            if (edmType.TypeKind == EdmTypeKind.Complex)
            {
                CreateComplexTypeBody(edmType as EdmComplexType, config as ComplexTypeConfiguration);
            }
            else
            {
                if (edmType.TypeKind == EdmTypeKind.Entity)
                {
                    CreateEntityTypeBody(edmType as EdmEntityType, config as EntityTypeConfiguration);
                }
            }
        }

        private void CreateStructuralTypeBody(EdmStructuredType type, StructuralTypeConfiguration config)
        {
            foreach (PropertyConfiguration property in config.Properties)
            {
                IEdmProperty edmProperty = null;

                switch (property.Kind)
                {
                    case PropertyKind.Primitive:
                        PrimitivePropertyConfiguration primitiveProperty = property as PrimitivePropertyConfiguration;
                        EdmPrimitiveTypeKind typeKind = GetTypeKind(primitiveProperty.PropertyInfo.PropertyType);
                        IEdmTypeReference primitiveTypeReference = EdmCoreModel.Instance.GetPrimitive(
                            typeKind,
                            primitiveProperty.OptionalProperty);

                        // Set concurrency token if is entity type, and concurrency token is true
                        EdmConcurrencyMode concurrencyMode = EdmConcurrencyMode.None;
                        if (config.Kind == EdmTypeKind.Entity && primitiveProperty.ConcurrencyToken)
                        {
                            concurrencyMode = EdmConcurrencyMode.Fixed;
                        }
                        edmProperty = type.AddStructuralProperty(
                            primitiveProperty.Name,
                            primitiveTypeReference,
                            defaultValue: null,
                            concurrencyMode: concurrencyMode);
                        break;

                    case PropertyKind.Complex:
                        ComplexPropertyConfiguration complexProperty = property as ComplexPropertyConfiguration;
                        IEdmComplexType complexType = _types[complexProperty.RelatedClrType] as IEdmComplexType;

                        edmProperty = type.AddStructuralProperty(
                            complexProperty.Name,
                            new EdmComplexTypeReference(complexType, complexProperty.OptionalProperty));
                        break;

                    case PropertyKind.Collection:
                        CollectionPropertyConfiguration collectionProperty = property as CollectionPropertyConfiguration;
                        IEdmTypeReference elementTypeReference = null;
                        if (_types.ContainsKey(collectionProperty.ElementType))
                        {
                            IEdmComplexType elementType = _types[collectionProperty.ElementType] as IEdmComplexType;
                            elementTypeReference = new EdmComplexTypeReference(elementType, false);
                        }
                        else
                        {
                            elementTypeReference = EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(collectionProperty.ElementType);
                        }
                        edmProperty = type.AddStructuralProperty(
                            collectionProperty.Name,
                            new EdmCollectionTypeReference(
                                new EdmCollectionType(elementTypeReference),
                                collectionProperty.OptionalProperty));
                        break;

                    default:
                        break;
                }

                if (property.PropertyInfo != null && edmProperty != null)
                {
                    _properties[property.PropertyInfo] = edmProperty;
                }

                if (edmProperty != null && property.IsRestricted)
                {
                    _propertiesRestrictions[edmProperty] = new QueryableRestrictions(property);
                }
            }
        }

        private void CreateComplexTypeBody(EdmComplexType type, ComplexTypeConfiguration config)
        {
            CreateStructuralTypeBody(type, config);
        }

        private void CreateEntityTypeBody(EdmEntityType type, EntityTypeConfiguration config)
        {
            CreateStructuralTypeBody(type, config);
            IEdmStructuralProperty[] keys = config.Keys.Select(p => type.DeclaredProperties.OfType<IEdmStructuralProperty>().First(dp => dp.Name == p.Name)).ToArray();
            type.AddKeys(keys);

            foreach (NavigationPropertyConfiguration navProp in config.NavigationProperties)
            {
                EdmNavigationPropertyInfo info = new EdmNavigationPropertyInfo();
                info.Name = navProp.Name;
                info.TargetMultiplicity = navProp.Multiplicity;
                info.Target = _types[navProp.RelatedClrType] as IEdmEntityType;
                //TODO: If target end has a multiplity of 1 this assumes the source end is 0..1.
                //      I think a better default multiplicity is *
                IEdmProperty edmProperty = type.AddUnidirectionalNavigation(info);
                if (navProp.PropertyInfo != null && edmProperty != null)
                {
                    _properties[navProp.PropertyInfo] = edmProperty;
                }

                if (edmProperty != null && navProp.IsRestricted)
                {
                    _propertiesRestrictions[edmProperty] = new QueryableRestrictions(navProp);
                }
            }
        }

        /// <summary>
        /// Builds <see cref="IEdmType"/> and <see cref="IEdmProperty"/>'s from <paramref name="configurations"/>
        /// </summary>
        /// <param name="configurations">A collection of <see cref="StructuralTypeConfiguration"/>'s</param>
        /// <returns>The built dictionary of <see cref="StructuralTypeConfiguration"/>'s indexed by their backing CLR type,
        /// and dictionary of <see cref="StructuralTypeConfiguration"/>'s indexed by their backing CLR property info</returns>
        public static EdmTypeMap GetTypesAndProperties(IEnumerable<StructuralTypeConfiguration> configurations)
        {
            if (configurations == null)
            {
                throw Error.ArgumentNull("configurations");
            }

            EdmTypeBuilder builder = new EdmTypeBuilder(configurations);
            return new EdmTypeMap(builder.GetEdmTypes(), builder._properties, builder._propertiesRestrictions);
        }
        
        /// <summary>
        /// Gets the <see cref="EdmPrimitiveTypeKind"/> that maps to the <see cref="Type"/>
        /// </summary>
        /// <param name="clrType">The clr type</param>
        /// <returns>The corresponding Edm primitive kind.</returns>
        public static EdmPrimitiveTypeKind GetTypeKind(Type clrType)
        {
            IEdmPrimitiveType primitiveType = EdmLibHelpers.GetEdmPrimitiveTypeOrNull(clrType);
            if (primitiveType == null)
            {
                throw Error.Argument("clrType", SRResources.MustBePrimitiveType, clrType.FullName);
            }

            return primitiveType.PrimitiveKind;
        }
    }
}
