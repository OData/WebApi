// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;
using Moq;
using Xunit;
using Xunit.Extensions;

namespace System.Web.Http.OData.Builder.Conventions
{
    public class ConventionsHelpersTests
    {
        public static TheoryDataSet<object, string> GetEntityKeyValue_SingleKey_DifferentDataTypes_Data
        {
            get
            {
                return new TheoryDataSet<object, string>
                {
                    { 1, "1" },
                    { "1", "'1'" },
                    { new DateTime(2012,12,31),"datetime'2012-12-31T00:00:00'" },
                    { new byte[] { 1,2 }, "binary'0102'" },
                    { false, "false" },
                    { true, "true" },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "guid'dddddddd-dddd-dddd-dddd-dddddddddddd'" }
                };
            }
        }

        [Theory]
        [InlineData(typeof(ICollection<string>), typeof(string))]
        [InlineData(typeof(IList<string>), typeof(string))]
        [InlineData(typeof(List<int>), typeof(int))]
        [InlineData(typeof(IsCollection_with_Collections_TestClass), typeof(bool))]
        [InlineData(typeof(IEnumerable<int>), typeof(int))]
        [InlineData(typeof(int[]), typeof(int))]
        public void IsCollection_with_Collections(Type collectionType, Type elementType)
        {
            Type type;
            Assert.True(collectionType.IsCollection(out type));
            Assert.Equal(elementType, type);
        }

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(ICollection))]
        [InlineData(typeof(IEnumerable))]
        public void IsCollection_with_NonCollections(Type type)
        {
            Assert.False(type.IsCollection());
        }

        [Theory]
        [InlineData(typeof(GetKeyProperty_validEntityType_TestClass_Id), "Id")]
        [InlineData(typeof(GetKeyProperty_validEntityType_TestClass2_ClassName), "GetKeyProperty_validEntityType_TestClass2_ClassNameId")]
        public void GetKeyProperty_ValidEntityType(Type type, string propertyName)
        {
            var key = ConventionsHelpers.GetKeyProperty(type, throwOnError: false);
            Assert.NotNull(key);
            Assert.Equal(key.Name, propertyName);
        }

        [Theory]
        [InlineData(typeof(GetKeyProperty_InValidEntityType_NoId))]
        [InlineData(typeof(GetKeyProperty_InValidEntityType_ComplexId))]
        public void GetKeyProperty_InValidEntityType(Type type)
        {
            var key = ConventionsHelpers.GetKeyProperty(type, throwOnError: false);
            Assert.Null(key);
        }

        [Fact]
        public void GetProperties_ReturnsProperties_FromBaseAndDerived()
        {
            var properties = ConventionsHelpers.GetProperties(typeof(GetProperties_Derived));
            var expectedProperties = new string[] { "Base_I", "Base_Complex", "Base_Str", "Derived_I", "Derived_Complex" };

            Assert.Equal(expectedProperties.OrderByDescending(name => name), properties.Select(p => p.Name).OrderByDescending(name => name));
        }

        [Fact]
        public void GetEntityKeyValue_SingleKey()
        {
            // Arrange
            var entityInstance = new { Key = "key" };
            PrimitivePropertyConfiguration[] keys = { new PrimitivePropertyConfiguration(entityInstance.GetType().GetProperty("Key")) };

            Mock<IEntityTypeConfiguration> entityType = new Mock<IEntityTypeConfiguration>();
            entityType
                .Setup(e => e.Keys)
                .Returns(keys);

            // Act
            var keyValue = ConventionsHelpers.GetEntityKeyValue(new EntityInstanceContext { EntityInstance = entityInstance }, entityType.Object);

            // Assert
            Assert.Equal("'key'", keyValue);
        }

        [Theory]
        [PropertyData("GetEntityKeyValue_SingleKey_DifferentDataTypes_Data")]
        public void GetEntityKeyValue_SingleKey_DifferentDataTypes(object value, object expectedValue)
        {
            // Arrange
            var entityInstance = new { Key = value };
            PrimitivePropertyConfiguration[] keys = { new PrimitivePropertyConfiguration(entityInstance.GetType().GetProperty("Key")) };

            Mock<IEntityTypeConfiguration> entityType = new Mock<IEntityTypeConfiguration>();
            entityType
                .Setup(e => e.Keys)
                .Returns(keys);

            // Act
            var keyValue = ConventionsHelpers.GetEntityKeyValue(new EntityInstanceContext { EntityInstance = entityInstance }, entityType.Object);

            // Assert
            Assert.Equal(expectedValue, keyValue);
        }

        [Fact]
        public void GetEntityKeyValue_MultipleKeys()
        {
            // Arrange
            var entityInstance = new { Key1 = "key1", Key2 = 2, Key3 = true };
            PrimitivePropertyConfiguration[] keys = 
            {
                new PrimitivePropertyConfiguration(entityInstance.GetType().GetProperty("Key1")),
                new PrimitivePropertyConfiguration(entityInstance.GetType().GetProperty("Key2")),
                new PrimitivePropertyConfiguration(entityInstance.GetType().GetProperty("Key3")),
            };

            Mock<IEntityTypeConfiguration> entityType = new Mock<IEntityTypeConfiguration>();
            entityType
                .Setup(e => e.Keys)
                .Returns(keys);

            // Act
            var keyValue = ConventionsHelpers.GetEntityKeyValue(new EntityInstanceContext { EntityInstance = entityInstance }, entityType.Object);

            // Assert
            Assert.Equal("Key1='key1',Key2=2,Key3=true", keyValue);
        }

        private sealed class IsCollection_with_Collections_TestClass : List<bool>
        {
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
            public Type Id { get; set; }
        }
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
        public string Derived_I { get; set; }

        public GetProperties_Complex Derived_Complex { get; set; }
    }

    class GetProperties_Complex
    {
        public int A { get; set; }
    }
}
