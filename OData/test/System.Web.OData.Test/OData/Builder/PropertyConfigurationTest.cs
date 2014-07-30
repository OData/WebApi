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
        public void Property_IsNotNavigable_SetsUnsortableAndNonFilterable()
        {
            // Arrange
            StructuralTypeConfiguration structuralType = Mock.Of<StructuralTypeConfiguration>();
            Mock<PropertyInfo> propertyInfo = new Mock<PropertyInfo>();
            propertyInfo.SetupGet(p => p.PropertyType).Returns(typeof(int));
            PropertyConfiguration property = new PrimitivePropertyConfiguration(propertyInfo.Object, structuralType);

            // Act
            property.IsNotNavigable();

            // Assert
            Assert.True(property.NonFilterable);
            Assert.True(property.Unsortable);
        }

        [Fact]
        public void Property_IsNavigable_DoesntSetSortableAndFilterable()
        {
            // Arrange
            StructuralTypeConfiguration structuralType = Mock.Of<StructuralTypeConfiguration>();
            Mock<PropertyInfo> propertyInfo = new Mock<PropertyInfo>();
            propertyInfo.SetupGet(p => p.PropertyType).Returns(typeof(int));
            PropertyConfiguration property = new PrimitivePropertyConfiguration(propertyInfo.Object, structuralType);
            property.IsNonFilterable();
            property.IsUnsortable();

            // Act
            property.IsNavigable();

            // Assert
            Assert.True(property.NonFilterable);
            Assert.True(property.Unsortable);
        }
    }
}
