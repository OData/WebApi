// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;

namespace System.Web.Http.OData.Builder
{
    public class EdmTypeBuilder
    {
        private readonly List<IStructuralTypeConfiguration> _configurations;
        private readonly Dictionary<string, IEdmType> _types = new Dictionary<string, IEdmType>();

        internal EdmTypeBuilder(IEnumerable<IStructuralTypeConfiguration> configurations)
        {
            _configurations = configurations.ToList();
        }

        private IEnumerable<IEdmType> GetEdmTypes()
        {
            // Reset
            _types.Clear();
            // Create headers to allow CreateEdmTypeBody to blindly references other things.
            foreach (IStructuralTypeConfiguration config in _configurations)
            {
                _types.Add(config.FullName, CreateEdmTypeHeader(config));
            }
            foreach (IStructuralTypeConfiguration config in _configurations)
            {
                CreateEdmTypeBody(config);
            }

            foreach (IStructuralTypeConfiguration config in _configurations)
            {
                yield return _types[config.FullName];
            }
        }

        private static IEdmType CreateEdmTypeHeader(IStructuralTypeConfiguration config)
        {
            if (config.Kind == StructuralTypeKind.ComplexType)
            {
                return new EdmComplexType(config.Namespace, config.Name);
            }
            else
            {
                return new EdmEntityType(config.Namespace, config.Name);
            }
        }

        private void CreateEdmTypeBody(IStructuralTypeConfiguration config)
        {
            IEdmType edmType = _types[config.FullName];

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
            foreach (PrimitivePropertyConfiguration prop in config.Properties.OfType<PrimitivePropertyConfiguration>())
            {
                type.AddStructuralProperty(
                    prop.PropertyInfo.Name,
                    GetTypeKind(prop.PropertyInfo.PropertyType),
                    prop.OptionalProperty);
            }
            foreach (ComplexPropertyConfiguration prop in config.Properties.OfType<ComplexPropertyConfiguration>())
            {
                IEdmComplexType complexType = _types[prop.RelatedClrType.FullName] as IEdmComplexType;

                type.AddStructuralProperty(
                    prop.PropertyInfo.Name,
                    new EdmComplexTypeReference(complexType, prop.OptionalProperty));
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
                info.Target = _types[navProp.RelatedClrType.FullName] as IEdmEntityType;
                //TODO: If target end has a multiplity of 1 this assumes the source end is 0..1.
                //      I think a better default multiplicity is *
                type.AddUnidirectionalNavigation(info);
            }
        }

        public static IEnumerable<IEdmType> GetTypes(IEnumerable<IStructuralTypeConfiguration> configurations)
        {
            if (configurations == null)
            {
                throw Error.ArgumentNull("configurations");
            }

            EdmTypeBuilder builder = new EdmTypeBuilder(configurations);
            return builder.GetEdmTypes();
        }

        public static EdmPrimitiveTypeKind GetTypeKind(Type clrType)
        {
            IEdmPrimitiveType primitiveType = EdmLibHelpers.GetEdmPrimitiveTypeOrNull(clrType);
            if (primitiveType == null)
            {
                throw Error.Argument("clrType", SRResources.MustBePrimitiveType);
            }

            return primitiveType.PrimitiveKind;
        }
    }
}
