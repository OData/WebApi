// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder
{
    public class PropertyConfigurationTest
    {
        private PropertyConfiguration _configuration;
        private string _name = "name";
        private StructuralTypeConfiguration _declaringType;
        private PropertyInfo _propertyInfo;

        public PropertyConfigurationTest()
        {
            Mock<PropertyInfo> mockPropertyInfo = new Mock<PropertyInfo>();
            _propertyInfo = mockPropertyInfo.Object;
            Mock<StructuralTypeConfiguration> mockTypeConfig = new Mock<StructuralTypeConfiguration>();
            _declaringType = mockTypeConfig.Object;
            Mock<PropertyConfiguration> mockConfiguration =
                new Mock<PropertyConfiguration>(_propertyInfo, _declaringType) { CallBase = true };
            mockConfiguration.Object.Name = "Name";
            _configuration = mockConfiguration.Object;
        }

        [Fact]
        public void Property_Name_RoundTrips()
        {
            Assert.Reflection.Property(_configuration, c => c.Name, "Name", allowNull: false, roundTripTestValue: _name);
        }

        [Fact]
        public void Property_DeclaringType_Get()
        {
            Assert.Equal(_declaringType, _configuration.DeclaringType);
        }

        [Fact]
        public void Property_PropertyInfo_Get()
        {
            Assert.Equal(_propertyInfo, _configuration.PropertyInfo);
        }

        [Fact]
        public void Property_AddedExplicitly_RoundTrips()
        {
            Assert.Reflection.BooleanProperty(_configuration, c => c.AddedExplicitly, true);
        }

        [Fact]
        public void Property_HasDefaultValueFalse_NotFilterableAndNonFilterable()
        {
            // Arrange
            StructuralTypeConfiguration structuralType = Mock.Of<StructuralTypeConfiguration>();
            Mock<PropertyInfo> propertyInfo = new Mock<PropertyInfo>();
            propertyInfo.SetupGet(p => p.PropertyType).Returns(typeof(int));

            // Act
            PropertyConfiguration property = new PrimitivePropertyConfiguration(propertyInfo.Object, structuralType);

            // Assert
            Assert.False(property.NotFilterable);
            Assert.False(property.NonFilterable);
        }

        [Theory]
        [InlineData(true, null, true)]
        [InlineData(false, null, false)]
        [InlineData(null, true, true)]
        [InlineData(null, false, false)]
        [InlineData(true, true, true)]
        [InlineData(false, false, false)]
        public void Property_NotFilterableAndNonFilterableAreEqual(
            bool? notFilterable,
            bool? nonFilterable,
            bool expectedResult)
        {
            // Arrange
            StructuralTypeConfiguration structuralType = Mock.Of<StructuralTypeConfiguration>();
            Mock<PropertyInfo> propertyInfo = new Mock<PropertyInfo>();
            propertyInfo.SetupGet(p => p.PropertyType).Returns(typeof(int));
            PropertyConfiguration property = new PrimitivePropertyConfiguration(propertyInfo.Object, structuralType);

            // Act
            if (notFilterable.HasValue)
            {
                property.NotFilterable = notFilterable.Value;
            }
            if (nonFilterable.HasValue)
            {
                property.NonFilterable = nonFilterable.Value;
            }

            // Assert
            Assert.Equal(expectedResult, property.NotFilterable);
            Assert.Equal(expectedResult, property.NonFilterable);
        }

        [Fact]
        public void Property_IsNonFilterable_SetNotFilterableAndNonFilterable()
        {
            // Arrange
            StructuralTypeConfiguration structuralType = Mock.Of<StructuralTypeConfiguration>();
            Mock<PropertyInfo> propertyInfo = new Mock<PropertyInfo>();
            propertyInfo.SetupGet(p => p.PropertyType).Returns(typeof(int));
            PropertyConfiguration property = new PrimitivePropertyConfiguration(propertyInfo.Object, structuralType);

            // Act
            property.IsNonFilterable();

            // Assert
            Assert.True(property.NotFilterable);
            Assert.True(property.NonFilterable);
        }

        [Fact]
        public void Property_IsNotFilterable_SetNotFilterableAndNonFilterable()
        {
            // Arrange
            StructuralTypeConfiguration structuralType = Mock.Of<StructuralTypeConfiguration>();
            Mock<PropertyInfo> propertyInfo = new Mock<PropertyInfo>();
            propertyInfo.SetupGet(p => p.PropertyType).Returns(typeof(int));
            PropertyConfiguration property = new PrimitivePropertyConfiguration(propertyInfo.Object, structuralType);

            // Act
            property.IsNotFilterable();

            // Assert
            Assert.True(property.NotFilterable);
            Assert.True(property.NonFilterable);
        }

        [Fact]
        public void Property_HasDefaultValueFalse_NotSortableAndUnsortable()
        {
            // Arrange
            StructuralTypeConfiguration structuralType = Mock.Of<StructuralTypeConfiguration>();
            Mock<PropertyInfo> propertyInfo = new Mock<PropertyInfo>();
            propertyInfo.SetupGet(p => p.PropertyType).Returns(typeof(int));

            // Act
            PropertyConfiguration property = new PrimitivePropertyConfiguration(propertyInfo.Object, structuralType);

            // Assert
            Assert.False(property.NotSortable);
            Assert.False(property.Unsortable);
        }

        [Theory]
        [InlineData(true, null, true)]
        [InlineData(false, null, false)]
        [InlineData(null, true, true)]
        [InlineData(null, false, false)]
        [InlineData(true, true, true)]
        [InlineData(false, false, false)]
        public void Property_NotSortableAndUnsortableAreEqual(
            bool? notSortable,
            bool? unsortable,
            bool expectedResult)
        {
            // Arrange
            StructuralTypeConfiguration structuralType = Mock.Of<StructuralTypeConfiguration>();
            Mock<PropertyInfo> propertyInfo = new Mock<PropertyInfo>();
            propertyInfo.SetupGet(p => p.PropertyType).Returns(typeof(int));
            PropertyConfiguration property = new PrimitivePropertyConfiguration(propertyInfo.Object, structuralType);


            // Act
            if (notSortable.HasValue)
            {
                property.NotSortable = notSortable.Value;
            }
            if (unsortable.HasValue)
            {
                property.Unsortable = unsortable.Value;
            }

            // Assert
            Assert.Equal(expectedResult, property.NotSortable);
            Assert.Equal(expectedResult, property.Unsortable);
        }

        [Fact]
        public void Property_IsUnsortable_SetNotSortableAndUnsortable()
        {
            // Arrange
            StructuralTypeConfiguration structuralType = Mock.Of<StructuralTypeConfiguration>();
            Mock<PropertyInfo> propertyInfo = new Mock<PropertyInfo>();
            propertyInfo.SetupGet(p => p.PropertyType).Returns(typeof(int));
            PropertyConfiguration property = new PrimitivePropertyConfiguration(propertyInfo.Object, structuralType);

            // Act
            property.IsUnsortable();

            // Assert
            Assert.True(property.NotSortable);
            Assert.True(property.Unsortable);
        }

        [Fact]
        public void Property_IsNotSortable_SetNotSortableAndUnsortable()
        {
            // Arrange
            StructuralTypeConfiguration structuralType = Mock.Of<StructuralTypeConfiguration>();
            Mock<PropertyInfo> propertyInfo = new Mock<PropertyInfo>();
            propertyInfo.SetupGet(p => p.PropertyType).Returns(typeof(int));
            PropertyConfiguration property = new PrimitivePropertyConfiguration(propertyInfo.Object, structuralType);

            // Act
            property.IsNotSortable();

            // Assert
            Assert.True(property.NotSortable);
            Assert.True(property.Unsortable);
        }

        [Fact]
        public void Property_IsNotNavigable_SetsNotSortableAndNotFilterable()
        {
            // Arrange
            StructuralTypeConfiguration structuralType = Mock.Of<StructuralTypeConfiguration>();
            Mock<PropertyInfo> propertyInfo = new Mock<PropertyInfo>();
            propertyInfo.SetupGet(p => p.PropertyType).Returns(typeof(int));
            PropertyConfiguration property = new PrimitivePropertyConfiguration(propertyInfo.Object, structuralType);

            // Act
            property.IsNotNavigable();

            // Assert
            Assert.True(property.NotFilterable);
            Assert.True(property.NotSortable);
        }

        [Fact]
        public void Property_IsNavigable_DoesntSetSortableAndFilterable()
        {
            // Arrange
            StructuralTypeConfiguration structuralType = Mock.Of<StructuralTypeConfiguration>();
            Mock<PropertyInfo> propertyInfo = new Mock<PropertyInfo>();
            propertyInfo.SetupGet(p => p.PropertyType).Returns(typeof(int));
            PropertyConfiguration property = new PrimitivePropertyConfiguration(propertyInfo.Object, structuralType);
            property.IsNotFilterable();
            property.IsNotSortable();

            // Act
            property.IsNavigable();

            // Assert
            Assert.True(property.NotFilterable);
            Assert.True(property.NotSortable);
        }
    }
}
