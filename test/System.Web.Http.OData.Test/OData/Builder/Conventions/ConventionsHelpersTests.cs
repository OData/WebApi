// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.Formatter.Serialization;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;
using Moq;

namespace System.Web.Http.OData.Builder.Conventions
{
    public class ConventionsHelpersTests
    {
        private ODataSerializerContext _writeContext = new ODataSerializerContext { Model = EdmCoreModel.Instance };

        public static TheoryDataSet<object, string> GetEntityKeyValue_SingleKey_DifferentDataTypes_Data
        {
            get
            {
                return new TheoryDataSet<object, string>
                {
                    { 1, "1" },
                    { "1", "'1'" },
                    { new DateTime(2012,12,31),"datetime'2012-12-31T00:00:00'" },
                    { new byte[] { 1,2 }, "X'0102'" },
                    { false, "false" },
                    { true, "true" },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "guid'dddddddd-dddd-dddd-dddd-dddddddddddd'" }
                };
            }
        }

        public static TheoryDataSet<object, string> GetUriRepresentationForValue_DataSet
        {
            get
            {
                return new TheoryDataSet<object, string>()
                {
                    { (bool)true, "true" },
                    { (char)'1', "'1'" },
                    { (char?)'1', "'1'" },
                    { (string)"123", "'123'" },
                    { (char[])new char[] { '1', '2', '3' }, "'123'" },
                    { (int)123, "123" },
                    { (short)123, "123" },
                    { (long)123, "123L" },
                    { (ushort)123, "123" },
                    { (uint)123, "123L" },
                    { (ulong)123, "123L" },
                    { (int?)123, "123" },
                    { (short?)123, "123" },
                    { (long?)123, "123L" },
                    { (ushort?)123, "123" },
                    { (uint?)123, "123L" },
                    { (ulong?)123, "123L" },
                    { (float)123.123, "123.123f" },
                    { (double)123.123, "123.123D" },
                    { (decimal)123.123, "123.123M" },
                    { Guid.Empty, "guid'00000000-0000-0000-0000-000000000000'" },
                    { DateTime.FromBinary(0), "datetime'0001-01-01T00:00:00'" },
                    { TimeSpan.FromSeconds(86456), "time'P1DT56S'" },
                    { DateTimeOffset.FromFileTime(0).ToUniversalTime(), "datetimeoffset'1601-01-01T00:00:00Z'" },
                    { SimpleEnum.First, "'First'" },
                    { FlagsEnum.One | FlagsEnum.Two, "'One, Two'" },
                };
            }
        }

        [Fact]
        public void GetProperties_ReturnsProperties_FromBaseAndDerived()
        {
            Mock<StructuralTypeConfiguration> edmType = new Mock<StructuralTypeConfiguration>();
            edmType.Setup(t => t.ClrType).Returns(typeof(GetProperties_Derived));

            var properties = ConventionsHelpers.GetAllProperties(edmType.Object, includeReadOnly: true);
            var expectedProperties = new string[] { "Base_I", "Base_Complex", "Base_Str", "Derived_I", "Derived_Complex", "Collection", "PrivateSetPublicGet" };

            Assert.Equal(expectedProperties.OrderByDescending(name => name), properties.Select(p => p.Name).OrderByDescending(name => name));
        }

        [Fact]
        public void GetProperties_Ignores_IgnoredProperties()
        {
            Mock<StructuralTypeConfiguration> edmType = new Mock<StructuralTypeConfiguration>();
            edmType.Setup(t => t.ClrType).Returns(typeof(GetProperties_Derived));
            edmType.Object.RemovedProperties.Add(typeof(GetProperties_Derived).GetProperty("Base_I"));
            edmType.Object.RemovedProperties.Add(typeof(GetProperties_Derived).GetProperty("Derived_I"));

            var properties = ConventionsHelpers.GetAllProperties(edmType.Object, includeReadOnly: true);
            var expectedProperties = new string[] { "Base_Complex", "Base_Str", "Derived_Complex", "Collection", "PrivateSetPublicGet" };

            Assert.Equal(expectedProperties.OrderByDescending(name => name), properties.Select(p => p.Name).OrderByDescending(name => name));
        }

        [Fact]
        public void GetAllProperties_Ignores_IndexerProperties()
        {
            Mock<StructuralTypeConfiguration> edmType = new Mock<StructuralTypeConfiguration>();
            edmType.Setup(t => t.ClrType).Returns(typeof(GetProperties_Derived));

            var properties = ConventionsHelpers.GetAllProperties(edmType.Object, includeReadOnly: true).Select(p => p.Name);
            Assert.DoesNotContain("Item", properties);
        }

        [Fact]
        public void GetAllProperties_Returns_PropertiesOfNestedTypes()
        {
            Mock<StructuralTypeConfiguration> edmType = new Mock<StructuralTypeConfiguration>();
            edmType.Setup(t => t.ClrType).Returns(typeof(GetProperties_NestParent.Nest));

            var properties = ConventionsHelpers.GetAllProperties(edmType.Object, includeReadOnly: true).Select(p => p.Name);

            Assert.Equal(1, properties.Count());
            Assert.Equal("NestProperty", properties.First());
        }

        [Fact]
        public void GetProperties_IncludeReadOnlyProperties()
        {
            Mock<StructuralTypeConfiguration> edmType = new Mock<StructuralTypeConfiguration>();
            edmType.Setup(t => t.ClrType).Returns(typeof(GetProperties_Derived));

            var properties = ConventionsHelpers.GetAllProperties(edmType.Object, includeReadOnly: true);
            var expectedProperties = new string[] { "PrivateSetPublicGet", "Derived_I", "Derived_Complex", "Collection", "Base_Str", "Base_I", "Base_Complex" };

            Assert.Equal(expectedProperties.OrderByDescending(name => name), properties.Select(p => p.Name).OrderByDescending(name => name));
        }

        [Fact]
        public void GetProperties_ExcludeReadOnlyProperties()
        {
            Mock<StructuralTypeConfiguration> edmType = new Mock<StructuralTypeConfiguration>();
            edmType.Setup(t => t.ClrType).Returns(typeof(GetProperties_Derived));

            var properties = ConventionsHelpers.GetAllProperties(edmType.Object, includeReadOnly: false);
            var expectedProperties = new string[] { "Derived_I", "Derived_Complex", "Collection", "Base_Str", "Base_I", "Base_Complex" };

            Assert.Equal(expectedProperties.OrderByDescending(name => name), properties.Select(p => p.Name).OrderByDescending(name => name));
        }

        [Fact]
        public void IgnoredProperties_ReturnsPropertiesFromBaseTypes()
        {
            // Arrange
            Mock<EntityTypeConfiguration> baseEntity = new Mock<EntityTypeConfiguration>();
            baseEntity.Object.RemovedProperties.Add(new MockPropertyInfo(typeof(int), "IgnoredBaseProperty"));

            Mock<EntityTypeConfiguration> derivedEntity = new Mock<EntityTypeConfiguration>();
            derivedEntity.Setup(e => e.BaseType).Returns(baseEntity.Object);

            // Act
            var ignoredProperties = derivedEntity.Object.IgnoredProperties();

            // Assert
            Assert.Equal(1, ignoredProperties.Count());
            Assert.Contains("IgnoredBaseProperty", ignoredProperties.Select(p => p.Name));
        }

        [Fact]
        public void GetEntityKeyValue_SingleKey()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Name");
            entityType.AddKeys(entityType.AddStructuralProperty("Property", EdmPrimitiveTypeKind.String));
            var entityInstance = new { Property = "key" };

            EntityInstanceContext entityInstanceContext = new EntityInstanceContext(_writeContext, entityType.AsReference(), entityInstance);

            // Act
            var keyValue = ConventionsHelpers.GetEntityKeyValue(entityInstanceContext);

            // Assert
            Assert.Equal("'key'", keyValue);
        }

        [Theory]
        [PropertyData("GetEntityKeyValue_SingleKey_DifferentDataTypes_Data")]
        public void GetEntityKeyValue_SingleKey_DifferentDataTypes(object value, object expectedValue)
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Name");
            entityType.AddKeys(entityType.AddStructuralProperty("Property", EdmPrimitiveTypeKind.String));
            var entityInstance = new { Property = value };

            EntityInstanceContext entityInstanceContext = new EntityInstanceContext(_writeContext, entityType.AsReference(), entityInstance);

            // Act
            var keyValue = ConventionsHelpers.GetEntityKeyValue(entityInstanceContext);

            // Assert
            Assert.Equal(expectedValue, keyValue);
        }

        [Fact]
        public void GetEntityKeyValue_MultipleKeys()
        {
            // Arrange
            var entityInstance = new { Key1 = "key1", Key2 = 2, Key3 = true };
            EdmEntityType entityType = new EdmEntityType("NS", "Name");
            entityType.AddKeys(entityType.AddStructuralProperty("Key1", EdmPrimitiveTypeKind.String));
            entityType.AddKeys(entityType.AddStructuralProperty("Key2", EdmPrimitiveTypeKind.Int32));
            entityType.AddKeys(entityType.AddStructuralProperty("Key3", EdmPrimitiveTypeKind.Boolean));

            EntityInstanceContext entityInstanceContext = new EntityInstanceContext(_writeContext, entityType.AsReference(), entityInstance);

            // Act
            var keyValue = ConventionsHelpers.GetEntityKeyValue(entityInstanceContext);

            // Assert
            Assert.Equal("Key1='key1',Key2=2,Key3=true", keyValue);
        }

        [Fact]
        public void GetEntityKeyValue_ThrowsForNullKeys()
        {
            // Arrange
            var entityInstance = new { Key = (string)null };
            EdmEntityType entityType = new EdmEntityType("NS", "Name");
            entityType.AddKeys(entityType.AddStructuralProperty("Key", EdmPrimitiveTypeKind.String));

            EntityInstanceContext entityInstanceContext = new EntityInstanceContext(_writeContext, entityType.AsReference(), entityInstance);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => ConventionsHelpers.GetEntityKeyValue(entityInstanceContext),
                "Key property 'Key' of type 'NS.Name' is null. Key properties cannot have null values.");
        }

        [Fact]
        public void GetEntityKeyValue_ThrowsForNullKeys_WithMultipleKeys()
        {
            // Arrange
            var entityInstance = new { Key1 = "abc", Key2 = "def", Key3 = (string)null };
            EdmEntityType entityType = new EdmEntityType("NS", "Name");
            entityType.AddKeys(entityType.AddStructuralProperty("Key1", EdmPrimitiveTypeKind.String));
            entityType.AddKeys(entityType.AddStructuralProperty("Key2", EdmPrimitiveTypeKind.Int32));
            entityType.AddKeys(entityType.AddStructuralProperty("Key3", EdmPrimitiveTypeKind.Boolean));

            EntityInstanceContext entityInstanceContext = new EntityInstanceContext(_writeContext, entityType.AsReference(), entityInstance);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => ConventionsHelpers.GetEntityKeyValue(entityInstanceContext),
                "Key property 'Key3' of type 'NS.Name' is null. Key properties cannot have null values.");
        }

        [Fact]
        public void GetEntityKeyValue_DerivedType()
        {
            // Arrange
            var entityInstance = new { Key = "key" };
            EdmEntityType baseEntityType = new EdmEntityType("NS", "Name");
            baseEntityType.AddKeys(baseEntityType.AddStructuralProperty("Key", EdmPrimitiveTypeKind.String));
            EdmEntityType derivedEntityType = new EdmEntityType("NS", "Derived", baseEntityType);

            EntityInstanceContext entityInstanceContext = new EntityInstanceContext(_writeContext, derivedEntityType.AsReference(), entityInstance);

            // Act
            var keyValue = ConventionsHelpers.GetEntityKeyValue(entityInstanceContext);

            // Assert
            Assert.Equal("'key'", keyValue);
        }

        [Fact]
        public void GetEntityKeyValue_MultipleKeys_DerivedType()
        {
            // Arrange
            var entityInstance = new { Key1 = "key1", Key2 = 2, Key3 = true };
            EdmEntityType baseEntityType = new EdmEntityType("NS", "Name");
            baseEntityType.AddKeys(baseEntityType.AddStructuralProperty("Key1", EdmPrimitiveTypeKind.String));
            baseEntityType.AddKeys(baseEntityType.AddStructuralProperty("Key2", EdmPrimitiveTypeKind.Int32));
            baseEntityType.AddKeys(baseEntityType.AddStructuralProperty("Key3", EdmPrimitiveTypeKind.Boolean));
            EdmEntityType derivedEntityType = new EdmEntityType("NS", "Derived", baseEntityType);

            EntityInstanceContext entityInstanceContext = new EntityInstanceContext(_writeContext, derivedEntityType.AsReference(), entityInstance);

            // Act
            var keyValue = ConventionsHelpers.GetEntityKeyValue(entityInstanceContext);

            // Assert
            Assert.Equal("Key1='key1',Key2=2,Key3=true", keyValue);
        }

        [Theory]
        [PropertyData("GetUriRepresentationForValue_DataSet")]
        public void GetUriRepresentationForValue_Works(object value, string result)
        {
            Assert.Equal(
                result,
                ConventionsHelpers.GetUriRepresentationForValue(value));
        }

        private class GetKeyProperty_validEntityType_TestClass_Id
        {
            public int Id { get; set; }
        }

        private class GetKeyProperty_validEntityType_TestClass2_ClassName
        {
            public string GetKeyProperty_validEntityType_TestClass2_ClassNameId { get; set; }
        }

        private class GetKeyProperty_InValidEntityType_NoId
        {
            public int IDD { get; set; }
        }

        private class GetKeyProperty_InValidEntityType_ComplexId
        {
            public GetProperties_Complex Id { get; set; }
        }

        class GetProperties_Base
        {
            public int Base_I { get; set; }

            private int Pri { get; set; }

            public int InternalGet { internal get; set; }

            public GetProperties_Complex Base_Complex { get; set; }

            public virtual string Base_Str { get; set; }
        }

        class GetProperties_Derived : GetProperties_Base
        {
            public static int SomeStaticProperty1 { get; set; }

            internal static int SomeStaticProperty2 { get; set; }

            private static int SomeStaticProperty3 { get; set; }

            public string Derived_I { get; set; }

            public string PrivateSetPublicGet { get; private set; }

            public string PrivateGetPublicSet { private get; set; }

            public GetProperties_Complex Derived_Complex { get; set; }

            public int[] Collection { get; private set; }

            public string this[string str]
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }
        }

        class GetProperties_Complex
        {
            public int A { get; set; }
        }

        public class GetProperties_NestParent
        {
            public class Nest
            {
                public NestPropertyType NestProperty { get; set; }
            }

            public class NestPropertyType
            {
            }
        }
    }
}
