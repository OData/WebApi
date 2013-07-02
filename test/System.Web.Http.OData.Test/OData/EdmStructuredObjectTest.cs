// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.Http.OData
{
    public class EdmStructuredObjectTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmTypeOfTypeIEdmStructuredType()
        {
            Assert.ThrowsArgumentNull(() => new TestEdmStructuredObject((IEdmStructuredType)null), "edmType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmTypeOfTypeIEdmStructuredTypeReference()
        {
            Assert.ThrowsArgumentNull(() => new TestEdmStructuredObject((IEdmStructuredTypeReference)null), "type");
        }

        [Fact]
        public void Property_IsNullable()
        {
            TestEdmStructuredObject edmObject = new TestEdmStructuredObject(new EdmComplexType("NS", "Complex"));

            Assert.Reflection.BooleanProperty(edmObject, e => e.IsNullable, expectedDefaultValue: false);
        }

        [Fact]
        public void Property_ActualEdmType()
        {
            EdmEntityType edmBaseType = new EdmEntityType("NS", "Base");
            EdmEntityType edmDerivedType = new EdmEntityType("NS", "Derived", edmBaseType);
            TestEdmStructuredObject edmObject = new TestEdmStructuredObject(edmBaseType);

            Assert.Reflection.Property(edmObject, o => o.ActualEdmType, edmBaseType, allowNull: false, roundTripTestValue: edmDerivedType);
        }

        [Fact]
        public void Property_ExpectedEdmType()
        {
            EdmEntityType edmBaseType = new EdmEntityType("NS", "Base");
            EdmEntityType edmDerivedType = new EdmEntityType("NS", "Derived", edmBaseType);
            TestEdmStructuredObject edmObject = new TestEdmStructuredObject(edmDerivedType);

            Assert.Reflection.Property(edmObject, o => o.ExpectedEdmType, edmDerivedType, allowNull: false, roundTripTestValue: edmBaseType);
        }

        [Fact]
        public void Property_ActualEdmType_CanBeSameAs_ExpectedEdmType()
        {
            EdmEntityType edmType = new EdmEntityType("NS", "Entity");
            TestEdmStructuredObject edmObject = new TestEdmStructuredObject(edmType);

            edmObject.ActualEdmType = edmType;

            Assert.Same(edmType, edmObject.ActualEdmType);
        }

        [Fact]
        public void Property_ExpectedEdmType_CanBeSameAs_ActualEdmType()
        {
            EdmEntityType edmType = new EdmEntityType("NS", "Entity");
            TestEdmStructuredObject edmObject = new TestEdmStructuredObject(edmType);

            edmObject.ExpectedEdmType = edmType;

            Assert.Same(edmType, edmObject.ExpectedEdmType);
        }

        [Fact]
        public void Property_ActualEdmType_ThrowsDeltaEntityTypeNotAssignable()
        {
            EdmEntityType edmType1 = new EdmEntityType("NS", "Entity1");
            EdmEntityType edmType2 = new EdmEntityType("NS", "Entity2");
            TestEdmStructuredObject edmObject = new TestEdmStructuredObject(edmType1);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    edmObject.ActualEdmType = edmType2;
                },
                "The actual entity type 'NS.Entity2' is not assignable to the expected type 'NS.Entity1'.");
        }

        [Fact]
        public void Property_ExpectedEdmType_ThrowsDeltaEntityTypeNotAssignable()
        {
            EdmEntityType edmType1 = new EdmEntityType("NS", "Entity1");
            EdmEntityType edmType2 = new EdmEntityType("NS", "Entity2");
            TestEdmStructuredObject edmObject = new TestEdmStructuredObject(edmType1);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    edmObject.ExpectedEdmType = edmType2;
                },
                "The actual entity type 'NS.Entity1' is not assignable to the expected type 'NS.Entity2'.");
        }

        [Fact]
        public void TrySetPropertyValue_ReturnsTrue_IfPropertyExists()
        {
            EdmComplexType edmType = new EdmComplexType("NS", "Complex");
            edmType.AddStructuralProperty("Property", EdmPrimitiveTypeKind.Int32);
            var edmObject = new TestEdmStructuredObject(edmType);

            bool result = edmObject.TrySetPropertyValue("Property", 42);

            Assert.True(result);
        }

        [Fact]
        public void TrySetPropertyValue_IfPropertyExists_UpdatesGetChangedPropertyNames()
        {
            EdmComplexType edmType = new EdmComplexType("NS", "Complex");
            edmType.AddStructuralProperty("Property", EdmPrimitiveTypeKind.Int32);
            var edmObject = new TestEdmStructuredObject(edmType);
            edmObject.TrySetPropertyValue("Property", 42);

            Assert.Contains("Property", edmObject.GetChangedPropertyNames());
        }

        [Fact]
        public void TrySetPropertyValue_ReturnsFalse_IfPropertyDoesNotExist()
        {
            EdmComplexType edmType = new EdmComplexType("NS", "Complex");
            var edmObject = new TestEdmStructuredObject(edmType);

            bool result = edmObject.TrySetPropertyValue("NotPresentProperty", 42);

            Assert.False(result);
        }

        [Fact]
        public void TrySetPropertyValue_IfPropertyDoesNotExist_DoesNotUpdateGetChangedPropertyNames()
        {
            EdmComplexType edmType = new EdmComplexType("NS", "Complex");
            var edmObject = new TestEdmStructuredObject(edmType);

            edmObject.TrySetPropertyValue("NotPresentProperty", 42);

            Assert.DoesNotContain("Property", edmObject.GetChangedPropertyNames());
        }

        [Fact]
        public void TryGetPropertyValue_ReturnsTrue_IfPropertyExists()
        {
            EdmComplexType edmType = new EdmComplexType("NS", "Complex");
            edmType.AddStructuralProperty("Property", EdmPrimitiveTypeKind.Int32);
            var edmObject = new TestEdmStructuredObject(edmType);

            object propertyValue;
            bool result = edmObject.TryGetPropertyValue("Property", out propertyValue);

            Assert.True(result);
        }

        [Fact]
        public void TryGetPropertyValue_ReturnsFalse_IfPropertyDoesNotExist()
        {
            EdmComplexType edmType = new EdmComplexType("NS", "Complex");
            var edmObject = new TestEdmStructuredObject(edmType);

            object propertyValue;
            bool result = edmObject.TryGetPropertyValue("NotPresentProperty", out propertyValue);

            Assert.False(result);
        }

        [Fact]
        public void TryGetPropertyValue_After_TrySetPropertyValue()
        {
            string propertyName = "Property";
            EdmComplexType edmType = new EdmComplexType("NS", "Complex");
            edmType.AddStructuralProperty(propertyName, EdmPrimitiveTypeKind.Int32);
            var edmObject = new TestEdmStructuredObject(edmType);
            object propertyValue = new object();

            object result;
            edmObject.TrySetPropertyValue(propertyName, propertyValue);
            edmObject.TryGetPropertyValue(propertyName, out result);

            Assert.Same(propertyValue, result);
        }

        [Fact]
        public void TryGetPropertyValue_Without_TrySetPropertyValue_ReturnsDefault()
        {
            string propertyName = "Property";
            EdmComplexType edmType = new EdmComplexType("NS", "Complex");
            edmType.AddStructuralProperty(propertyName, EdmPrimitiveTypeKind.Int32, isNullable: false);
            var edmObject = new TestEdmStructuredObject(edmType);

            object result;
            edmObject.TryGetPropertyValue(propertyName, out result);

            Assert.Equal(0, result);
        }

        public static TheoryDataSet<IEdmTypeReference, object> GetDefaultValueTestData
        {
            get
            {
                IEdmTypeReference nullableDouble = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Double, isNullable: true);
                IEdmTypeReference nullableEntity = new EdmEntityTypeReference(new EdmEntityType("NS", "Entity"), isNullable: true);
                IEdmTypeReference nullableComplex = new EdmComplexTypeReference(new EdmComplexType("NS", "Complex"), isNullable: true);

                return new TheoryDataSet<IEdmTypeReference, object>
                {
                    { EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Double, isNullable: true), null },
                    { EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Double, isNullable: false), default(double) },
                    { EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.DateTime, isNullable: true), null },
                    { EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.DateTime, isNullable: false), default(DateTime) },
                    { new EdmComplexTypeReference(new EdmComplexType("NS", "Complex"), isNullable: true), null },
                    { new EdmEntityTypeReference(new EdmEntityType("NS", "Entity"), isNullable: true), null },
                    { new EdmCollectionTypeReference(new EdmCollectionType(nullableDouble), isNullable: false),  new List<double?>() },
                    { new EdmCollectionTypeReference(new EdmCollectionType(nullableDouble), isNullable: true),  null },
                    { new EdmCollectionTypeReference(new EdmCollectionType(nullableEntity), isNullable: true),  null },
                };
            }
        }

        [Theory]
        [PropertyData("GetDefaultValueTestData")]
        public void GetDefaultValue(IEdmTypeReference edmType, object expectedResult)
        {
            Assert.Equal(expectedResult, EdmStructuredObject.GetDefaultValue(edmType));
        }

        [Fact]
        public void GetDefaultValue_NonNullableComplexCollection()
        {
            IEdmTypeReference elementType = new EdmComplexTypeReference(new EdmComplexType("NS", "Complex"), isNullable: true);
            IEdmCollectionTypeReference complexCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(elementType), isNullable: false);

            var result = EdmStructuredObject.GetDefaultValue(complexCollectionType);

            var complexCollectionObject = Assert.IsType<EdmComplexObjectCollection>(result);
            Assert.Equal(complexCollectionType, complexCollectionObject.GetEdmType(), new EdmTypeReferenceEqualityComparer());
        }

        [Fact]
        public void GetDefaultValue_NonNullableEntityCollection()
        {
            IEdmTypeReference elementType = new EdmEntityTypeReference(new EdmEntityType("NS", "Entity"), isNullable: true);
            IEdmCollectionTypeReference entityCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(elementType), isNullable: false);

            var result = EdmStructuredObject.GetDefaultValue(entityCollectionType);

            var entityCollectionObject = Assert.IsType<EdmEntityObjectCollection>(result);
            Assert.Equal(entityCollectionType, entityCollectionObject.GetEdmType(), new EdmTypeReferenceEqualityComparer());
        }

        [Fact]
        public void GetDefaultValue_NonNullableComplex()
        {
            IEdmTypeReference nonNullableComplexType = new EdmComplexTypeReference(new EdmComplexType("NS", "Complex"), isNullable: false);

            var result = EdmStructuredObject.GetDefaultValue(nonNullableComplexType);

            var complexObject = Assert.IsType<EdmComplexObject>(result);
            Assert.Equal(nonNullableComplexType, complexObject.GetEdmType(), new EdmTypeReferenceEqualityComparer());
        }

        [Fact]
        public void GetDefaultValue_NonNullableEntity()
        {
            IEdmTypeReference nonNullableEntityType = new EdmEntityTypeReference(new EdmEntityType("NS", "Entity"), isNullable: false);

            var result = EdmStructuredObject.GetDefaultValue(nonNullableEntityType);

            var entityObject = Assert.IsType<EdmEntityObject>(result);
            Assert.Equal(nonNullableEntityType, entityObject.GetEdmType(), new EdmTypeReferenceEqualityComparer());
        }

        [Fact]
        public void TryGetPropertyType_ReturnsTrue_IfPropertyExists()
        {
            EdmComplexType edmType = new EdmComplexType("NS", "Complex");
            edmType.AddStructuralProperty("Property", EdmPrimitiveTypeKind.Int32);
            var edmObject = new TestEdmStructuredObject(edmType);

            Type propertyType;
            bool result = edmObject.TryGetPropertyType("Property", out propertyType);

            Assert.True(result);
        }

        [Fact]
        public void TryGetPropertyType_ReturnsFalse_IfPropertyDoesNotExist()
        {
            EdmComplexType edmType = new EdmComplexType("NS", "Complex");
            var edmObject = new TestEdmStructuredObject(edmType);

            Type propertyType;
            bool result = edmObject.TryGetPropertyType("NotPresentProperty", out propertyType);

            Assert.False(result);
        }

        public static TheoryDataSet<IEdmTypeReference, Type> GetClrTypeForUntypedDeltaTestData
        {
            get
            {
                IEdmTypeReference nullableDouble = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Double, isNullable: true);
                IEdmTypeReference entity = new EdmEntityTypeReference(new EdmEntityType("NS", "Entity"), isNullable: true);
                IEdmTypeReference complex = new EdmComplexTypeReference(new EdmComplexType("NS", "Complex"), isNullable: true);

                return new TheoryDataSet<IEdmTypeReference, Type>
                {
                    { EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: true), typeof(int?) },
                    { EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false), typeof(int) },
                    { EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, isNullable: true), typeof(string) },
                    { EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, isNullable: false), typeof(string) },
                    { entity, typeof(EdmEntityObject) },
                    { complex, typeof(EdmComplexObject) },
                    { new EdmCollectionTypeReference(new EdmCollectionType(entity), isNullable: false), typeof(EdmEntityObjectCollection) },
                    { new EdmCollectionTypeReference(new EdmCollectionType(complex), isNullable: false), typeof(EdmComplexObjectCollection) },
                    { new EdmCollectionTypeReference(new EdmCollectionType(nullableDouble), isNullable: false), typeof(List<double?>) }
                };
            }
        }

        [Theory]
        [PropertyData("GetClrTypeForUntypedDeltaTestData")]
        public void GetClrTypeForUntypedDelta(IEdmTypeReference edmType, Type expectedType)
        {
            Assert.Equal(expectedType, EdmStructuredObject.GetClrTypeForUntypedDelta(edmType));
        }

        [Fact]
        public void GetClrTypeForUntypedDelta_Throws_UnsupportedEdmType()
        {
            IEdmTypeReference edmType = new EdmRowTypeReference(new EdmRowType(), isNullable: true);
            Assert.Throws<InvalidOperationException>(
                () => EdmStructuredObject.GetClrTypeForUntypedDelta(edmType),
                "The EDM type '[Row() Nullable=True]' of kind 'Row' is not supported.");
        }

        [Fact]
        public void GetEdmType_HasSameDefinition_AsInitializedEdmType()
        {
            var complexType = new EdmComplexType("NS", "Complex");
            var edmObject = new TestEdmStructuredObject(complexType);

            Assert.Equal(complexType, edmObject.GetEdmType().Definition);
        }

        [Fact]
        public void GetEdmType_AgreesWithPropertyIsNullable()
        {
            var complexType = new EdmComplexType("NS", "Complex");
            var edmObject = new TestEdmStructuredObject(complexType);
            edmObject.IsNullable = true;

            Assert.True(edmObject.GetEdmType().IsNullable);
        }

        private class TestEdmStructuredObject : EdmStructuredObject
        {
            public TestEdmStructuredObject(IEdmStructuredType edmType)
                : base(edmType)
            {
            }

            public TestEdmStructuredObject(IEdmStructuredTypeReference edmType)
                : base(edmType)
            {
            }

            public TestEdmStructuredObject(IEdmStructuredType edmType, bool isNullable)
                : base(edmType, isNullable)
            {
            }
        }
    }
}
