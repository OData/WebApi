// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder
{
    public class PrimitivePropertyConfigurationExtensionsTest
    {
        private Mock<StructuralTypeConfiguration> _structuralType;
        public PrimitivePropertyConfigurationExtensionsTest()
        {
            _structuralType = new Mock<StructuralTypeConfiguration>();
            _structuralType.Setup(t => t.FullName).Returns("NS.Customer");
        }

        [Fact]
        public void AsDate_ThrowsArgumentNull()
        {
            // Arrange
            PrimitivePropertyConfiguration property = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => property.AsDate(), "property");
        }

        [Fact]
        public void AsTimeOfDay_ThrowsArgumentNull()
        {
            // Arrange
            PrimitivePropertyConfiguration property = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => property.AsTimeOfDay(), "property");
        }

        [Theory]
        [InlineData(typeof(DateTimeOffset))]
        [InlineData(typeof(TimeSpan))]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        public void AsDate_ThrowsArgument(Type propertyType)
        {
            // Arrange
            MockType type = new MockType().Property(propertyType, "Birthday");
            PropertyInfo property = type.GetProperty("Birthday");
            _structuralType.Setup(t => t.ClrType).Returns(type);

            // Act
            PrimitivePropertyConfiguration propertyConfig = new PrimitivePropertyConfiguration(property, _structuralType.Object);

            // Assert
            Assert.ThrowsArgument(() => propertyConfig.AsDate(), "property",
                "The property 'Birthday' on type 'NS.Customer' must be a System.DateTime property");
        }

        [Theory]
        [InlineData(typeof(DateTimeOffset))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        public void AsTimeOfDay_ThrowsArgument(Type propertyType)
        {
            // Arrange
            MockType type = new MockType().Property(propertyType, "CreatedTime");
            PropertyInfo property = type.GetProperty("CreatedTime");
            _structuralType.Setup(t => t.ClrType).Returns(type);

            // Act
            PrimitivePropertyConfiguration propertyConfig = new PrimitivePropertyConfiguration(property, _structuralType.Object);

            // Assert
            Assert.ThrowsArgument(() => propertyConfig.AsTimeOfDay(), "property",
                "The property 'CreatedTime' on type 'NS.Customer' must be a System.TimeSpan property");
        }

        [Fact]
        public void AsDate_Works()
        {
            // Arrange
            MockType type = new MockType().Property(typeof(DateTime), "Birthday");
            PropertyInfo property = type.GetProperty("Birthday");
            _structuralType.Setup(t => t.ClrType).Returns(type);

            // Act
            PrimitivePropertyConfiguration propertyConfig = new PrimitivePropertyConfiguration(property, _structuralType.Object);
            EdmPrimitiveTypeKind? typeKind = propertyConfig.AsDate().TargetEdmTypeKind;

            // Assert
            Assert.NotNull(typeKind);
            Assert.Equal(EdmPrimitiveTypeKind.Date, typeKind);
        }

        [Fact]
        public void AsTimeOfDay_Works()
        {
            // Arrange
            MockType type = new MockType().Property(typeof(TimeSpan), "CreatedTime");
            PropertyInfo property = type.GetProperty("CreatedTime");
            _structuralType.Setup(t => t.ClrType).Returns(type);

            // Act
            PrimitivePropertyConfiguration propertyConfig = new PrimitivePropertyConfiguration(property, _structuralType.Object);
            EdmPrimitiveTypeKind? typeKind = propertyConfig.AsTimeOfDay().TargetEdmTypeKind;

            // Assert
            Assert.NotNull(typeKind);
            Assert.Equal(EdmPrimitiveTypeKind.TimeOfDay, typeKind);
        }
    }
}
