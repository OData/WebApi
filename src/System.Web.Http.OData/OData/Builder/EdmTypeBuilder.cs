// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// <see cref="EdmTypeBuilder"/> builds <see cref="IEdmType"/>'s from <see cref=" IStructuralTypeConfiguration"/>'s.
    /// </summary>
    public class EdmTypeBuilder
    {
        private readonly List<IStructuralTypeConfiguration> _configurations;
        private readonly Dictionary<Type, IEdmStructuredType> _types = new Dictionary<Type, IEdmStructuredType>();

        internal EdmTypeBuilder(IEnumerable<IStructuralTypeConfiguration> configurations)
        {
            _configurations = configurations.ToList();
        }

        private IEnumerable<IEdmStructuredType> GetEdmTypes()
        {
            // Reset
            _types.Clear();

            // Create headers to allow CreateEdmTypeBody to blindly references other things.
            foreach (IStructuralTypeConfiguration config in _configurations)
            {
                CreateEdmTypeHeader(config);
            }

            foreach (IStructuralTypeConfiguration config in _configurations)
            {
                CreateEdmTypeBody(config);
            }

            foreach (IStructuralTypeConfiguration config in _configurations)
            {
                yield return _types[config.ClrType];
            }
        }

        private void CreateEdmTypeHeader(IStructuralTypeConfiguration config)
        {
            if (!_types.ContainsKey(config.ClrType))
            {
                if (config.Kind == EdmTypeKind.Complex)
                {
                    _types.Add(config.ClrType, new EdmComplexType(config.Namespace, config.Name));
                }
                else
                {
                    IEntityTypeConfiguration entity = config as IEntityTypeConfiguration;
                    Contract.Assert(entity != null);

                    IEdmEntityType baseType = null;

                    if (entity.BaseType != null)
                    {
                        CreateEdmTypeHeader(entity.BaseType);
                        baseType = _types[entity.BaseType.ClrType] as IEdmEntityType;

                        Contract.Assert(baseType != null);
                    }

                    _types.Add(config.ClrType, new EdmEntityType(config.Namespace, config.Name, baseType, entity.IsAbstract, isOpen: false));
                }
            }
        }

        private void CreateEdmTypeBody(IStructuralTypeConfiguration config)
        {
            IEdmType edmType = _types[config.ClrType];

            if (edmType.TypeKind == EdmTypeKind.Complex)
            {
                CreateComplexTypeBody(edmType as EdmComplexType, config as IComplexTypeConfiguration);
            }
            else
            {
                if (edmType.TypeKind == EdmTypeKind.Entity)
                {
                    CreateEntityTypeBody(edmType as EdmEntityType, config as IEntityTypeConfiguration);
                }
            }
        }

        private void CreateStructuralTypeBody(EdmStructuredType type, IStructuralTypeConfiguration config)
        {
            foreach (PropertyConfiguration property in config.Properties)
            {
                switch (property.Kind)
                {
                    case PropertyKind.Primitive:
                        PrimitivePropertyConfiguration primitiveProperty = property as PrimitivePropertyConfiguration;
                        type.AddStructuralProperty(
                            primitiveProperty.PropertyInfo.Name,
                            GetTypeKind(primitiveProperty.PropertyInfo.PropertyType),
                            primitiveProperty.OptionalProperty);
                        break;

                    case PropertyKind.Complex:
                        ComplexPropertyConfiguration complexProperty = property as ComplexPropertyConfiguration;
                        IEdmComplexType complexType = _types[complexProperty.RelatedClrType] as IEdmComplexType;

                        type.AddStructuralProperty(
                            complexProperty.PropertyInfo.Name,
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
                        type.AddStructuralProperty(
                            collectionProperty.PropertyInfo.Name,
                            new EdmCollectionTypeReference(
                                new EdmCollectionType(elementTypeReference),
                                collectionProperty.OptionalProperty));
                        break;

                    default:
                        break;
                }
            }
        }

        private void CreateComplexTypeBody(EdmComplexType type, IComplexTypeConfiguration config)
        {
            CreateStructuralTypeBody(type, config);
        }

        private void CreateEntityTypeBody(EdmEntityType type, IEntityTypeConfiguration config)
        {
            CreateStructuralTypeBody(type, config);
            IEdmStructuralProperty[] keys = config.Keys.Select(p => type.DeclaredProperties.OfType<IEdmStructuralProperty>().First(dp => dp.Name == p.PropertyInfo.Name)).ToArray();
            type.AddKeys(keys);

            foreach (NavigationPropertyConfiguration navProp in config.NavigationProperties)
            {
                EdmNavigationPropertyInfo info = new EdmNavigationPropertyInfo();
                info.Name = navProp.Name;
                info.TargetMultiplicity = navProp.Multiplicity;
                info.Target = _types[navProp.RelatedClrType] as IEdmEntityType;
                //TODO: If target end has a multiplity of 1 this assumes the source end is 0..1.
                //      I think a better default multiplicity is *
                type.AddUnidirectionalNavigation(info);
            }
        }

        /// <summary>
        /// Builds <see cref="IEdmType"/>'s from <paramref name="configurations"/>
        /// </summary>
        /// <param name="configurations">A collection of <see cref="IStructuralTypeConfiguration"/>'s</param>
        /// <returns>The built collection of <see cref="IEdmType"/></returns>
        public static IEnumerable<IEdmStructuredType> GetTypes(IEnumerable<IStructuralTypeConfiguration> configurations)
        {
            if (configurations == null)
            {
                throw Error.ArgumentNull("configurations");
            }

            EdmTypeBuilder builder = new EdmTypeBuilder(configurations);
            return builder.GetEdmTypes();
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
