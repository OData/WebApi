// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData.Formatter;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Builder
{
    public static class EdmModelAsserts
    {
        public static IEdmEntityType AssertHasEntitySet(this IEdmModel model, string entitySetName, Type mappedEntityClrType)
        {
            string entityTypeName = mappedEntityClrType.FullName;

            var entitySet = model.EntityContainers().Single().EntitySets().Where(set => set.Name == entitySetName).Single();
            Assert.NotNull(entitySet);
            Assert.Equal(entitySet.Name, entitySetName);
            Assert.Equal(entitySet.ElementType.FullName(), entityTypeName);
            Assert.True(model.GetEdmType(mappedEntityClrType).IsEquivalentTo(entitySet.ElementType));

            var entityType = model.SchemaElements.OfType<IEdmEntityType>().Where(t => t.FullName() == entityTypeName).SingleOrDefault();
            Assert.NotNull(entityType);
            Assert.Equal(entityType, entitySet.ElementType);
            return entityType;
        }

        public static IEdmEntityType AssertHasEntityType(this IEdmModel model, Type mappedEntityClrType, Type mappedEntityBaseType)
        {
            IEdmEntityType entity = AssertHasEntityType(model, mappedEntityClrType);
            IEdmEntityType baseEntity = AssertHasEntityType(model, mappedEntityBaseType);

            Assert.Equal(baseEntity, entity.BaseEntityType());
            return entity;
        }

        public static IEdmEntityType AssertHasEntityType(this IEdmModel model, Type mappedEntityClrType)
        {
            string entityTypeName = mappedEntityClrType.FullName;

            var entityType = model.SchemaElements.OfType<IEdmEntityType>().Where(t => t.FullName() == entityTypeName).SingleOrDefault();
            Assert.NotNull(entityType);
            Assert.True(model.GetEdmType(mappedEntityClrType).IsEquivalentTo(entityType));
            return entityType;
        }

        public static IEdmComplexType AssertHasComplexType(this IEdmModel model, Type mappedClrType)
        {
            string complexTypeName = mappedClrType.FullName;

            var complexType = model.SchemaElements.OfType<IEdmComplexType>().Where(t => t.FullName() == complexTypeName).SingleOrDefault();
            Assert.NotNull(complexType);
            Assert.True(model.GetEdmType(mappedClrType).IsEquivalentTo(complexType));
            return complexType;
        }

        public static IEdmStructuralProperty AssertHasKey(this IEdmEntityType entity, IEdmModel model, string keyName, EdmPrimitiveTypeKind primitiveTypeKind)
        {
            IEdmStructuralProperty key = entity.AssertHasPrimitiveProperty(model, keyName, primitiveTypeKind, isNullable: false);
            Assert.Contains(key, entity.Key());
            return key;
        }

        public static IEdmStructuralProperty AssertHasPrimitiveProperty(this IEdmStructuredType edmType, IEdmModel model, string propertyName, EdmPrimitiveTypeKind primitiveTypeKind, bool isNullable)
        {
            return edmType.AssertHasProperty<IEdmStructuralProperty>(model, propertyName, EdmCoreModel.Instance.GetPrimitiveType(primitiveTypeKind).FullName(), isNullable);
        }

        public static IEdmStructuralProperty AssertHasComplexProperty(this IEdmStructuredType edmType, IEdmModel model, string propertyName, string propertyType, bool isNullable)
        {
            IEdmStructuralProperty complexProperty = edmType.AssertHasProperty<IEdmStructuralProperty>(model, propertyName, propertyType, isNullable);
            Assert.True(complexProperty.Type.IsComplex());
            return complexProperty;
        }

        public static IEdmNavigationProperty AssertHasNavigationProperty(this IEdmStructuredType edmType, IEdmModel model, string propertyName, Type mappedPropertyClrType, bool isNullable, EdmMultiplicity multiplicity)
        {
            string propertyType = mappedPropertyClrType.FullName;

            IEdmNavigationProperty navigationProperty = edmType.AssertHasProperty<IEdmNavigationProperty>(model, propertyName, propertyType: null, isNullable: isNullable);

            // Bug 468693: in EdmLib. remove when fixed.
            if (multiplicity != EdmMultiplicity.Many)
            {
                Assert.Equal(multiplicity, navigationProperty.Multiplicity());
            }

            Assert.True(navigationProperty.ToEntityType().IsEquivalentTo(model.FindType(propertyType)));
            return navigationProperty;
        }

        public static TPropertyType AssertHasProperty<TPropertyType>(this IEdmStructuredType edmType, IEdmModel model, string propertyName, string propertyType, bool isNullable)
            where TPropertyType : IEdmProperty
        {
            IEnumerable<TPropertyType> properties =
                edmType
                .Properties()
                .OfType<TPropertyType>()
                .Where(p => p.Name == propertyName);

            Assert.True(properties.Count() == 1);
            TPropertyType property = properties.Single();

            if (propertyType != null)
            {
                Assert.True(property.Type.Definition.IsEquivalentTo(model.FindType(propertyType)));
            }

            Assert.Equal(isNullable, property.Type.IsNullable);

            return property;
        }
    }
}
