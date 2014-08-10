// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.OData.Edm.Library.Values;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// <see cref="EdmTypeBuilder"/> builds <see cref="IEdmType"/>'s from <see cref=" StructuralTypeConfiguration"/>'s.
    /// </summary>
    internal class EdmTypeBuilder
    {
        private readonly List<IEdmTypeConfiguration> _configurations;
        private readonly Dictionary<Type, IEdmType> _types = new Dictionary<Type, IEdmType>();
        private readonly Dictionary<PropertyInfo, IEdmProperty> _properties = new Dictionary<PropertyInfo, IEdmProperty>();
        private readonly Dictionary<IEdmProperty, QueryableRestrictions> _propertiesRestrictions = new Dictionary<IEdmProperty, QueryableRestrictions>();
        private readonly Dictionary<Enum, IEdmEnumMember> _members = new Dictionary<Enum, IEdmEnumMember>();
        private readonly Dictionary<IEdmStructuredType, PropertyInfo> _openTypes = new Dictionary<IEdmStructuredType, PropertyInfo>();

        internal EdmTypeBuilder(IEnumerable<IEdmTypeConfiguration> configurations)
        {
            _configurations = configurations.ToList();
        }

        private Dictionary<Type, IEdmType> GetEdmTypes()
        {
            // Reset
            _types.Clear();
            _properties.Clear();
            _members.Clear();
            _openTypes.Clear();

            // Create headers to allow CreateEdmTypeBody to blindly references other things.
            foreach (IEdmTypeConfiguration config in _configurations)
            {
                CreateEdmTypeHeader(config);
            }

            foreach (IEdmTypeConfiguration config in _configurations)
            {
                CreateEdmTypeBody(config);
            }

            return _types;
        }

        private void CreateEdmTypeHeader(IEdmTypeConfiguration config)
        {
            if (GetEdmType(config.ClrType) == null)
            {
                if (config.Kind == EdmTypeKind.Complex)
                {
                    ComplexTypeConfiguration complex = (ComplexTypeConfiguration)config;
                    IEdmComplexType baseType = null;
                    if (complex.BaseType != null)
                    {
                        CreateEdmTypeHeader(complex.BaseType);
                        baseType = GetEdmType(complex.BaseType.ClrType) as IEdmComplexType;

                        Contract.Assert(baseType != null);
                    }

                    EdmComplexType complexType = new EdmComplexType(config.Namespace, config.Name,
                        baseType, complex.IsAbstract ?? false, complex.IsOpen);

                    _types.Add(config.ClrType, complexType);

                    if (complex.IsOpen)
                    {
                        // add a mapping between the open complex type and its dynamic property dictionary.
                        _openTypes.Add(complexType, complex.DynamicPropertyDictionary);
                    }
                }
                else if (config.Kind == EdmTypeKind.Entity)
                {
                    EntityTypeConfiguration entity = config as EntityTypeConfiguration;
                    Contract.Assert(entity != null);

                    IEdmEntityType baseType = null;
                    if (entity.BaseType != null)
                    {
                        CreateEdmTypeHeader(entity.BaseType);
                        baseType = GetEdmType(entity.BaseType.ClrType) as IEdmEntityType;

                        Contract.Assert(baseType != null);
                    }

                    EdmEntityType entityType = new EdmEntityType(config.Namespace, config.Name, baseType,
                        entity.IsAbstract ?? false, entity.IsOpen);
                    _types.Add(config.ClrType, entityType);

                    if (entity.IsOpen)
                    {
                        // add a mapping between the open entity type and its dynamic property dictionary.
                        _openTypes.Add(entityType, entity.DynamicPropertyDictionary);
                    }
                }
                else
                {
                    EnumTypeConfiguration enumTypeConfiguration = config as EnumTypeConfiguration;

                    // The config has to be enum.
                    Contract.Assert(enumTypeConfiguration != null);

                    _types.Add(enumTypeConfiguration.ClrType,
                        new EdmEnumType(enumTypeConfiguration.Namespace, enumTypeConfiguration.Name,
                            GetTypeKind(enumTypeConfiguration.UnderlyingType), enumTypeConfiguration.IsFlags));
                }
            }
        }

        private void CreateEdmTypeBody(IEdmTypeConfiguration config)
        {
            IEdmType edmType = GetEdmType(config.ClrType);

            if (edmType.TypeKind == EdmTypeKind.Complex)
            {
                CreateComplexTypeBody((EdmComplexType)edmType, (ComplexTypeConfiguration)config);
            }
            else if (edmType.TypeKind == EdmTypeKind.Entity)
            {
                CreateEntityTypeBody((EdmEntityType)edmType, (EntityTypeConfiguration)config);
            }
            else
            {
                Contract.Assert(edmType.TypeKind == EdmTypeKind.Enum);
                CreateEnumTypeBody((EdmEnumType)edmType, (EnumTypeConfiguration)config);
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
                        IEdmComplexType complexType = GetEdmType(complexProperty.RelatedClrType) as IEdmComplexType;

                        edmProperty = type.AddStructuralProperty(
                            complexProperty.Name,
                            new EdmComplexTypeReference(complexType, complexProperty.OptionalProperty));
                        break;

                    case PropertyKind.Collection:
                        edmProperty = CreateStructuralTypeCollectionPropertyBody(type, (CollectionPropertyConfiguration)property);
                        break;

                    case PropertyKind.Enum:
                        edmProperty = CreateStructuralTypeEnumPropertyBody(type, config, (EnumPropertyConfiguration)property);
                        break;

                    default:
                        break;
                }

                if (edmProperty != null)
                {
                    if (property.PropertyInfo != null)
                    {
                        _properties[property.PropertyInfo] = edmProperty;
                    }

                    if (property.IsRestricted)
                    {
                        _propertiesRestrictions[edmProperty] = new QueryableRestrictions(property);
                    }
                }
            }
        }

        private IEdmProperty CreateStructuralTypeCollectionPropertyBody(EdmStructuredType type, CollectionPropertyConfiguration collectionProperty)
        {
            IEdmTypeReference elementTypeReference = null;
            Type clrType = TypeHelper.GetUnderlyingTypeOrSelf(collectionProperty.ElementType);

            if (clrType.IsEnum)
            {
                IEdmType edmType = GetEdmType(clrType);

                if (edmType == null)
                {
                    throw Error.InvalidOperation(SRResources.EnumTypeDoesNotExist, clrType.Name);
                }

                IEdmEnumType enumElementType = (IEdmEnumType)edmType;
                bool isNullable = collectionProperty.ElementType != clrType;
                elementTypeReference = new EdmEnumTypeReference(enumElementType, isNullable);
            }
            else
            {
                IEdmType edmType = GetEdmType(collectionProperty.ElementType);
                if (edmType != null)
                {
                    IEdmComplexType elementType = edmType as IEdmComplexType;
                    Contract.Assert(elementType != null);
                    elementTypeReference = new EdmComplexTypeReference(elementType, collectionProperty.OptionalProperty);
                }
                else
                {
                    elementTypeReference =
                        EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(collectionProperty.ElementType);
                    Contract.Assert(elementTypeReference != null);
                }
            }

            return type.AddStructuralProperty(
                collectionProperty.Name,
                new EdmCollectionTypeReference(new EdmCollectionType(elementTypeReference)));
        }

        private IEdmProperty CreateStructuralTypeEnumPropertyBody(EdmStructuredType type, StructuralTypeConfiguration config, EnumPropertyConfiguration enumProperty)
        {
            Type enumPropertyType = TypeHelper.GetUnderlyingTypeOrSelf(enumProperty.RelatedClrType);
            IEdmType edmType = GetEdmType(enumPropertyType);

            if (edmType == null)
            {
                throw Error.InvalidOperation(SRResources.EnumTypeDoesNotExist, enumPropertyType.Name);
            }

            IEdmEnumType enumType = (IEdmEnumType)edmType;
            IEdmTypeReference enumTypeReference = new EdmEnumTypeReference(enumType, enumProperty.OptionalProperty);

            // Set concurrency token if is entity type, and concurrency token is true
            EdmConcurrencyMode enumConcurrencyMode = EdmConcurrencyMode.None;
            if (config.Kind == EdmTypeKind.Entity && enumProperty.ConcurrencyToken)
            {
                enumConcurrencyMode = EdmConcurrencyMode.Fixed;
            }

            return type.AddStructuralProperty(
                enumProperty.Name,
                enumTypeReference,
                defaultValue: null,
                concurrencyMode: enumConcurrencyMode);
        }

        private void CreateComplexTypeBody(EdmComplexType type, ComplexTypeConfiguration config)
        {
            Contract.Assert(type != null);
            Contract.Assert(config != null);

            CreateStructuralTypeBody(type, config);
        }

        private void CreateEntityTypeBody(EdmEntityType type, EntityTypeConfiguration config)
        {
            Contract.Assert(type != null);
            Contract.Assert(config != null);

            CreateStructuralTypeBody(type, config);
            IEnumerable<IEdmStructuralProperty> keys = config.Keys.Select(p => type.DeclaredProperties.OfType<IEdmStructuralProperty>().First(dp => dp.Name == p.Name));
            type.AddKeys(keys);

            foreach (NavigationPropertyConfiguration navProp in config.NavigationProperties)
            {
                EdmNavigationPropertyInfo info = new EdmNavigationPropertyInfo
                {
                    Name = navProp.Name,
                    TargetMultiplicity = navProp.Multiplicity,
                    Target = GetEdmType(navProp.RelatedClrType) as IEdmEntityType,
                    ContainsTarget = navProp.ContainsTarget
                };
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

        private void CreateEnumTypeBody(EdmEnumType type, EnumTypeConfiguration config)
        {
            Contract.Assert(type != null);
            Contract.Assert(config != null);

            foreach (EnumMemberConfiguration member in config.Members)
            {
                // EdmIntegerConstant can only support a value of long type.
                long value;
                try
                {
                    value = Convert.ToInt64(member.MemberInfo, CultureInfo.InvariantCulture);
                }
                catch
                {
                    throw Error.Argument("value", SRResources.EnumValueCannotBeLong, Enum.GetName(member.MemberInfo.GetType(), member.MemberInfo));
                }

                EdmEnumMember edmMember = new EdmEnumMember(type, member.Name,
                    new EdmIntegerConstant(value));
                type.AddMember(edmMember);
                _members[member.MemberInfo] = edmMember;
            }
        }

        private IEdmType GetEdmType(Type clrType)
        {
            Contract.Assert(clrType != null);

            IEdmType edmType;
            _types.TryGetValue(clrType, out edmType);

            return edmType;
        }

        /// <summary>
        /// Builds <see cref="IEdmType"/> and <see cref="IEdmProperty"/>'s from <paramref name="configurations"/>
        /// </summary>
        /// <param name="configurations">A collection of <see cref="IEdmTypeConfiguration"/>'s</param>
        /// <returns>The built dictionary of <see cref="StructuralTypeConfiguration"/>'s indexed by their backing CLR type,
        /// and dictionary of <see cref="StructuralTypeConfiguration"/>'s indexed by their backing CLR property info</returns>
        public static EdmTypeMap GetTypesAndProperties(IEnumerable<IEdmTypeConfiguration> configurations)
        {
            if (configurations == null)
            {
                throw Error.ArgumentNull("configurations");
            }

            EdmTypeBuilder builder = new EdmTypeBuilder(configurations);
            return new EdmTypeMap(builder.GetEdmTypes(),
                builder._properties,
                builder._propertiesRestrictions,
                builder._members,
                builder._openTypes);
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
