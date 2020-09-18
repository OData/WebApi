// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// <see cref="EdmTypeBuilder"/> builds <see cref="IEdmType"/>'s from <see cref="StructuralTypeConfiguration"/>'s.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling acceptable")]
    internal class EdmTypeBuilder
    {
        private readonly List<IEdmTypeConfiguration> _configurations;
        private readonly Dictionary<Type, IEdmType> _types = new Dictionary<Type, IEdmType>();
        private readonly Dictionary<PropertyInfo, IEdmProperty> _properties = new Dictionary<PropertyInfo, IEdmProperty>();
        private readonly Dictionary<IEdmProperty, QueryableRestrictions> _propertiesRestrictions = new Dictionary<IEdmProperty, QueryableRestrictions>();
        private readonly Dictionary<IEdmProperty, ModelBoundQuerySettings> _propertiesQuerySettings = new Dictionary<IEdmProperty, ModelBoundQuerySettings>();
        private readonly Dictionary<IEdmProperty, PropertyConfiguration> _propertyConfigurations = new Dictionary<IEdmProperty, PropertyConfiguration>();
        private readonly Dictionary<IEdmStructuredType, ModelBoundQuerySettings> _structuredTypeQuerySettings = new Dictionary<IEdmStructuredType, ModelBoundQuerySettings>();
        private readonly Dictionary<Enum, IEdmEnumMember> _members = new Dictionary<Enum, IEdmEnumMember>();
        private readonly Dictionary<IEdmStructuredType, PropertyInfo> _openTypes = new Dictionary<IEdmStructuredType, PropertyInfo>();
        private readonly Dictionary<IEdmStructuredType, PropertyInfo> _instanceAnnotableTypes = new Dictionary<IEdmStructuredType, PropertyInfo>();

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
            _propertyConfigurations.Clear();
            _instanceAnnotableTypes.Clear();

            // Create headers to allow CreateEdmTypeBody to blindly references other things.
            foreach (IEdmTypeConfiguration config in _configurations)
            {
                CreateEdmTypeHeader(config);
            }

            foreach (IEdmTypeConfiguration config in _configurations)
            {
                CreateEdmTypeBody(config);
            }

            foreach (StructuralTypeConfiguration structrual in _configurations.OfType<StructuralTypeConfiguration>())
            {
                CreateNavigationProperty(structrual);
            }

            return _types;
        }

        private void CreateEdmTypeHeader(IEdmTypeConfiguration config)
        {
            IEdmType edmType = GetEdmType(config.ClrType);
            if (edmType == null)
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

                    if (complex.SupportsInstanceAnnotations)
                    {
                        // add a mapping between the complex type and its instance annotation dictionary.
                        _instanceAnnotableTypes.Add(complexType, complex.InstanceAnnotationsContainer);
                    }

                    edmType = complexType;
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
                        entity.IsAbstract ?? false, entity.IsOpen, entity.HasStream);
                    _types.Add(config.ClrType, entityType);

                    if (entity.IsOpen)
                    {
                        // add a mapping between the open entity type and its dynamic property dictionary.
                        _openTypes.Add(entityType, entity.DynamicPropertyDictionary);
                    }

                    if (entity.SupportsInstanceAnnotations)
                    {
                        // add a mapping between the entity type and its instance annotation dictionary.
                        _instanceAnnotableTypes.Add(entityType, entity.InstanceAnnotationsContainer);
                    }

                    edmType = entityType;
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

            IEdmStructuredType structuredType = edmType as IEdmStructuredType;
            StructuralTypeConfiguration structuralTypeConfiguration = config as StructuralTypeConfiguration;
            if (structuredType != null && structuralTypeConfiguration != null &&
                !_structuredTypeQuerySettings.ContainsKey(structuredType))
            {
                ModelBoundQuerySettings querySettings =
                    structuralTypeConfiguration.QueryConfiguration.ModelBoundQuerySettings;
                if (querySettings != null)
                {
                    _structuredTypeQuerySettings.Add(structuredType,
                        structuralTypeConfiguration.QueryConfiguration.ModelBoundQuerySettings);
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

        private static IEdmTypeReference AddPrecisionConfigInPrimitiveTypeReference(
            PrecisionPropertyConfiguration precisionProperty,
            IEdmTypeReference primitiveTypeReference)
        {
            if (primitiveTypeReference is EdmTemporalTypeReference && precisionProperty.Precision.HasValue)
            {
                return new EdmTemporalTypeReference(
                    (IEdmPrimitiveType)primitiveTypeReference.Definition,
                    primitiveTypeReference.IsNullable,
                    precisionProperty.Precision);
            }
            return primitiveTypeReference;
        }

        private static IEdmTypeReference AddLengthConfigInPrimitiveTypeReference(
            LengthPropertyConfiguration lengthProperty,
            IEdmTypeReference primitiveTypeReference)
        {
            if (lengthProperty.MaxLength.HasValue)
            {
                if (primitiveTypeReference is EdmStringTypeReference)
                {
                    return new EdmStringTypeReference(
                        (IEdmPrimitiveType)primitiveTypeReference.Definition,
                        primitiveTypeReference.IsNullable,
                        false,
                        lengthProperty.MaxLength,
                        true);
                }
                if (primitiveTypeReference is EdmBinaryTypeReference)
                {
                    return new EdmBinaryTypeReference(
                        (IEdmPrimitiveType)primitiveTypeReference.Definition,
                        primitiveTypeReference.IsNullable,
                        false,
                        lengthProperty.MaxLength);
                }
            }
            return primitiveTypeReference;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling acceptable")]
        private void CreateStructuralTypeBody(EdmStructuredType type, StructuralTypeConfiguration config)
        {
            foreach (PropertyConfiguration property in config.Properties)
            {
                IEdmProperty edmProperty = null;

                switch (property.Kind)
                {
                    case PropertyKind.Primitive:
                        PrimitivePropertyConfiguration primitiveProperty = (PrimitivePropertyConfiguration)property;
                        EdmPrimitiveTypeKind typeKind = primitiveProperty.TargetEdmTypeKind ??
                                                        GetTypeKind(primitiveProperty.PropertyInfo.PropertyType);
                        IEdmTypeReference primitiveTypeReference = EdmCoreModel.Instance.GetPrimitive(
                            typeKind,
                            primitiveProperty.OptionalProperty);

                        if (typeKind == EdmPrimitiveTypeKind.Decimal)
                        {
                            DecimalPropertyConfiguration decimalProperty =
                                primitiveProperty as DecimalPropertyConfiguration;
                            if (decimalProperty.Precision.HasValue || decimalProperty.Scale.HasValue)
                            {
                                primitiveTypeReference = new EdmDecimalTypeReference(
                                    (IEdmPrimitiveType)primitiveTypeReference.Definition,
                                    primitiveTypeReference.IsNullable,
                                    decimalProperty.Precision,
                                    decimalProperty.Scale.HasValue ? decimalProperty.Scale : 0);
                            }
                        }
                        else if (EdmLibHelpers.HasPrecision(typeKind))
                        {
                            PrecisionPropertyConfiguration precisionProperty =
                                primitiveProperty as PrecisionPropertyConfiguration;
                            primitiveTypeReference = AddPrecisionConfigInPrimitiveTypeReference(
                                precisionProperty,
                                primitiveTypeReference);
                        }
                        else if (EdmLibHelpers.HasLength(typeKind))
                        {
                            LengthPropertyConfiguration lengthProperty =
                                primitiveProperty as LengthPropertyConfiguration;
                            primitiveTypeReference = AddLengthConfigInPrimitiveTypeReference(
                                lengthProperty,
                                primitiveTypeReference);
                        }
                        edmProperty = type.AddStructuralProperty(
                            primitiveProperty.Name,
                            primitiveTypeReference,
                            defaultValue: primitiveProperty.DefaultValueString);
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
                        edmProperty = CreateStructuralTypeEnumPropertyBody(type, (EnumPropertyConfiguration)property);
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

                    if (property.QueryConfiguration.ModelBoundQuerySettings != null)
                    {
                        _propertiesQuerySettings.Add(edmProperty, property.QueryConfiguration.ModelBoundQuerySettings);
                    }

                    _propertyConfigurations[edmProperty] = property;
                }
            }
        }

        private IEdmProperty CreateStructuralTypeCollectionPropertyBody(EdmStructuredType type, CollectionPropertyConfiguration collectionProperty)
        {
            IEdmTypeReference elementTypeReference = null;
            Type clrType = TypeHelper.GetUnderlyingTypeOrSelf(collectionProperty.ElementType);

            if (TypeHelper.IsEnum(clrType))
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

        private IEdmProperty CreateStructuralTypeEnumPropertyBody(EdmStructuredType type, EnumPropertyConfiguration enumProperty)
        {
            Type enumPropertyType = TypeHelper.GetUnderlyingTypeOrSelf(enumProperty.RelatedClrType);
            IEdmType edmType = GetEdmType(enumPropertyType);

            if (edmType == null)
            {
                throw Error.InvalidOperation(SRResources.EnumTypeDoesNotExist, enumPropertyType.Name);
            }

            IEdmEnumType enumType = (IEdmEnumType)edmType;
            IEdmTypeReference enumTypeReference = new EdmEnumTypeReference(enumType, enumProperty.OptionalProperty);

            return type.AddStructuralProperty(
                enumProperty.Name,
                enumTypeReference,
                defaultValue: enumProperty.DefaultValueString);
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
            var keys = ((IEnumerable<PropertyConfiguration>)config.Keys)
                                     .Concat(config.EnumKeys)
                                     .OrderBy(p => p.Order)
                                     .ThenBy(p => p.Name)
                                     .Select(p => type.DeclaredProperties.OfType<IEdmStructuralProperty>().First(dp => dp.Name == p.Name));
            type.AddKeys(keys);
        }

        private void CreateNavigationProperty(StructuralTypeConfiguration config)
        {
            Contract.Assert(config != null);

            EdmStructuredType type = (EdmStructuredType)(GetEdmType(config.ClrType));

            foreach (NavigationPropertyConfiguration navProp in config.NavigationProperties)
            {
                Func<NavigationPropertyConfiguration, EdmNavigationPropertyInfo> getInfo = nav =>
                {
                    EdmNavigationPropertyInfo info = new EdmNavigationPropertyInfo
                    {
                        Name = nav.Name,
                        TargetMultiplicity = nav.Multiplicity,
                        Target = GetEdmType(nav.RelatedClrType) as IEdmEntityType,
                        ContainsTarget = nav.ContainsTarget,
                        OnDelete = nav.OnDeleteAction
                    };

                    // Principal properties
                    if (nav.PrincipalProperties.Any())
                    {
                        info.PrincipalProperties = GetDeclaringPropertyInfo(nav.PrincipalProperties);
                    }

                    // Dependent properties
                    if (nav.DependentProperties.Any())
                    {
                        info.DependentProperties = GetDeclaringPropertyInfo(nav.DependentProperties);
                    }

                    return info;
                };

                var navInfo = getInfo(navProp);
                var props = new Dictionary<IEdmProperty, NavigationPropertyConfiguration>();
                EdmEntityType entityType = type as EdmEntityType;
                if (entityType != null && navProp.Partner != null)
                {
                    var edmProperty = entityType.AddBidirectionalNavigation(navInfo, getInfo(navProp.Partner));
                    var partnerEdmProperty = (navInfo.Target as EdmEntityType).Properties().Single(p => p.Name == navProp.Partner.Name);
                    props.Add(edmProperty, navProp);
                    props.Add(partnerEdmProperty, navProp.Partner);
                }
                else
                {
                    // Do not add this if we have have a partner relationship configured, as this
                    // property will be added automatically through the AddBidirectionalNavigation
                    var targetConfig = config.ModelBuilder.GetTypeConfigurationOrNull(navProp.RelatedClrType) as StructuralTypeConfiguration;
                    if (!targetConfig.NavigationProperties.Any(p => p.Partner != null && p.Partner.Name == navInfo.Name))
                    {
                        var edmProperty = type.AddUnidirectionalNavigation(navInfo);
                        props.Add(edmProperty, navProp);
                    }
                }

                foreach (var item in props)
                {
                    var edmProperty = item.Key;
                    var prop = item.Value;
                    if (prop.PropertyInfo != null)
                    {
                        _properties[prop.PropertyInfo] = edmProperty;
                    }

                    if (prop.IsRestricted)
                    {
                        _propertiesRestrictions[edmProperty] = new QueryableRestrictions(prop);
                    }

                    if (prop.QueryConfiguration.ModelBoundQuerySettings != null)
                    {
                        _propertiesQuerySettings.Add(edmProperty, prop.QueryConfiguration.ModelBoundQuerySettings);
                    }

                    _propertyConfigurations[edmProperty] = prop;
                }
            }
        }

        private IList<IEdmStructuralProperty> GetDeclaringPropertyInfo(IEnumerable<PropertyInfo> propertyInfos)
        {
            IList<IEdmProperty> edmProperties = new List<IEdmProperty>();
            foreach (PropertyInfo propInfo in propertyInfos)
            {
                IEdmProperty edmProperty;
                if (_properties.TryGetValue(propInfo, out edmProperty))
                {
                    edmProperties.Add(edmProperty);
                }
                else
                {
                    Contract.Assert(TypeHelper.GetReflectedType(propInfo) != null);
                    Type baseType = TypeHelper.GetBaseType(TypeHelper.GetReflectedType(propInfo));
                    while (baseType != null)
                    {
                        PropertyInfo basePropInfo = baseType.GetProperty(propInfo.Name);
                        if (_properties.TryGetValue(basePropInfo, out edmProperty))
                        {
                            edmProperties.Add(edmProperty);
                            break;
                        }

                        baseType = TypeHelper.GetBaseType(baseType);
                    }

                    Contract.Assert(baseType != null);
                }
            }

            return edmProperties.OfType<IEdmStructuralProperty>().ToList();
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
                    new EdmEnumMemberValue(value));
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
                builder._propertiesQuerySettings,
                builder._structuredTypeQuerySettings,
                builder._members,
                builder._openTypes,
                builder._propertyConfigurations,
                builder._instanceAnnotableTypes);
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
