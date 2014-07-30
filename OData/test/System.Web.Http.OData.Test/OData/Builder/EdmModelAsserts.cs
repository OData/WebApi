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
            Assert.True(model.GetEdmType(mappedEntityClrType).IsEquivalentTo(entitySet.ElementType));

            return entitySet.ElementType;
        }

        public static IEdmNavigationTargetMapping AssertHasNavigationTarget(this IEdmEntitySet entitySet, IEdmNavigationProperty navigationProperty, string targetEntitySet)
        {
            IEdmNavigationTargetMapping navMapping = entitySet.NavigationTargets.Where(n => n.NavigationProperty == navigationProperty).SingleOrDefault();
            Assert.NotNull(navMapping);
            Assert.Equal(targetEntitySet, navMapping.TargetEntitySet.Name);
            return navMapping;
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
            var entityType = model.SchemaElements.OfType<IEdmEntityType>().Where(t => model.GetEdmType(mappedEntityClrType).IsEquivalentTo(t)).SingleOrDefault();
            Assert.NotNull(entityType);
            Assert.True(model.GetEdmType(mappedEntityClrType).IsEquivalentTo(entityType));
            return entityType;
        }

        public static IEdmComplexType AssertHasComplexType(this IEdmModel model, Type mappedClrType)
        {
            var complexType = model.SchemaElements.OfType<IEdmComplexType>().Where(t => model.GetEdmType(mappedClrType).IsEquivalentTo(t)).SingleOrDefault();
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
            Type primitiveType = EdmLibHelpers.GetClrType(new EdmPrimitiveTypeReference(EdmCoreModel.Instance.GetPrimitiveType(primitiveTypeKind), isNullable), model);
            return edmType.AssertHasProperty<IEdmStructuralProperty>(model, propertyName, primitiveType, isNullable);
        }

        public static IEdmStructuralProperty AssertHasComplexProperty(this IEdmStructuredType edmType, IEdmModel model, string propertyName, Type propertyType, bool isNullable)
        {
            IEdmStructuralProperty complexProperty = edmType.AssertHasProperty<IEdmStructuralProperty>(model, propertyName, propertyType, isNullable);
            Assert.True(complexProperty.Type.IsComplex());
            return complexProperty;
        }

        public static IEdmStructuralProperty AssertHasCollectionProperty(this IEdmStructuredType edmType, IEdmModel model, string propertyName, Type propertyType, bool isNullable)
        {
            IEdmStructuralProperty complexProperty = edmType.AssertHasProperty<IEdmStructuralProperty>(model, propertyName, propertyType, isNullable, isCollection: true);
            Assert.True(complexProperty.Type.IsCollection());
            return complexProperty;
        }

        public static IEdmNavigationProperty AssertHasNavigationProperty(this IEdmStructuredType edmType, IEdmModel model, string propertyName, Type mappedPropertyClrType, bool isNullable, EdmMultiplicity multiplicity)
        {
            IEdmNavigationProperty navigationProperty = edmType.AssertHasProperty<IEdmNavigationProperty>(model, propertyName, propertyType: null, isNullable: isNullable);

            Assert.Equal(multiplicity, navigationProperty.Partner.Multiplicity());

            Assert.True(navigationProperty.ToEntityType().IsEquivalentTo(model.GetEdmType(mappedPropertyClrType)));
            return navigationProperty;
        }

        public static TPropertyType AssertHasProperty<TPropertyType>(this IEdmStructuredType edmType, IEdmModel model, string propertyName, Type propertyType, bool isNullable, bool isCollection = false)
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
                if (isCollection)
                {
                    Assert.True(property.Type.AsCollection().ElementType().Definition.IsEquivalentTo(model.GetEdmType(propertyType)));
                }
                else
                {
                    Assert.True(property.Type.Definition.IsEquivalentTo(model.GetEdmType(propertyType)));
                }
            }

            Assert.Equal(isNullable, property.Type.IsNullable);

            return property;
        }
    }
}
