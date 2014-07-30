// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder.Conventions.Attributes
{
    public class TimestampAttributeEdmPropertyConventionTests
    {
        [Fact]
        public void TimestampConvention_AppliesWhenTheAttributeIsAppliedToASingleProperty()
        {
            // Arrange
            PropertyInfo property = CreateMockPropertyInfo("TestProperty");
            EntityTypeConfiguration entityType = new EntityTypeConfiguration();
            PrimitivePropertyConfiguration primitiveProperty = new PrimitivePropertyConfiguration(property, entityType);
            entityType.ExplicitProperties.Add(property, primitiveProperty);
            TimestampAttributeEdmPropertyConvention convention = new TimestampAttributeEdmPropertyConvention();

            // Act
            convention.Apply(primitiveProperty, entityType, new ODataConventionModelBuilder());

            // Assert
            Assert.True(primitiveProperty.ConcurrencyToken);
        }

        [Fact]
        public void TimestampConvention_DoesntApplyWhenTheAttributeIsAppliedOnANonEntityType()
        {
            // Arrange
            PropertyInfo property = CreateMockPropertyInfo("TestProperty");
            ComplexTypeConfiguration complexType = new ComplexTypeConfiguration();
            PrimitivePropertyConfiguration primitiveProperty = new PrimitivePropertyConfiguration(property, complexType);
            complexType.ExplicitProperties.Add(property, primitiveProperty);
            TimestampAttributeEdmPropertyConvention convention = new TimestampAttributeEdmPropertyConvention();

            // Act
            convention.Apply(primitiveProperty, complexType, new ODataConventionModelBuilder());

            // Assert
            Assert.False(primitiveProperty.ConcurrencyToken);
        }

        [Fact]
        public void TimestampConvention_DoesntApplyWhenTheAttributeIsAppliedToMultipleProperties()
        {
            // Arrange
            PropertyInfo property = CreateMockPropertyInfo("TestProperty");
            PropertyInfo otherProperty = CreateMockPropertyInfo("OtherTestProperty");
            EntityTypeConfiguration entityType = new EntityTypeConfiguration();
            PrimitivePropertyConfiguration primitiveProperty = new PrimitivePropertyConfiguration(property, entityType);
            entityType.ExplicitProperties.Add(property, primitiveProperty);
            entityType.ExplicitProperties.Add(otherProperty, new PrimitivePropertyConfiguration(otherProperty, entityType));
            TimestampAttributeEdmPropertyConvention convention = new TimestampAttributeEdmPropertyConvention();

            // Act
            convention.Apply(primitiveProperty, entityType, new ODataConventionModelBuilder());

            // Assert
            Assert.False(primitiveProperty.ConcurrencyToken);
        }

        [Fact]
        public void TimestampConvention_DoesntApplyWhenTheAttributeIsAppliedToMultipleProperties_InATypeHierarchy()
        {
            // Arrange
            PropertyInfo property = CreateMockPropertyInfo("TestProperty");
            PropertyInfo otherProperty = CreateMockPropertyInfo("OtherTestProperty");
            EntityTypeConfiguration baseEntityType = new EntityTypeConfiguration();
            EntityTypeConfiguration entityType = new Mock<EntityTypeConfiguration>().SetupAllProperties().Object;
            entityType.BaseType = baseEntityType;
            PrimitivePropertyConfiguration primitiveProperty = new PrimitivePropertyConfiguration(property, entityType);
            entityType.ExplicitProperties.Add(property, primitiveProperty);
            baseEntityType.ExplicitProperties.Add(otherProperty, new PrimitivePropertyConfiguration(otherProperty, baseEntityType));
            TimestampAttributeEdmPropertyConvention convention = new TimestampAttributeEdmPropertyConvention();

            // Act
            convention.Apply(primitiveProperty, entityType, new ODataConventionModelBuilder());

            // Assert
            Assert.False(primitiveProperty.ConcurrencyToken);
        }

        private static PropertyInfo CreateMockPropertyInfo(string propertyName)
        {
            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns(propertyName);
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new object[] { new TimestampAttribute() });
            property.Setup(p => p.GetCustomAttributes(typeof(TimestampAttribute), It.IsAny<bool>())).Returns(new object[] { new TimestampAttribute() });
            return property.Object;
        }
    }
}
