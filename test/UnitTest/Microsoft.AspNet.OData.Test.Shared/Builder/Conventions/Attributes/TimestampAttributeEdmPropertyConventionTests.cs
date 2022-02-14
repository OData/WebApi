//-----------------------------------------------------------------------------
// <copyright file="TimestampAttributeEdmPropertyConventionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Builder.Conventions.Attributes;
using Microsoft.AspNet.OData.Test.Abstraction;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder.Conventions.Attributes
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
            convention.Apply(primitiveProperty, entityType, ODataConventionModelBuilderFactory.Create());

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
            convention.Apply(primitiveProperty, complexType, ODataConventionModelBuilderFactory.Create());

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
            convention.Apply(primitiveProperty, entityType, ODataConventionModelBuilderFactory.Create());

            // Assert
            Assert.False(primitiveProperty.ConcurrencyToken);
        }

        [Fact]
        public void TimestampConvention_DoesntApplyWhenTheAttributeIsAppliedToMultipleProperties_InATypeHierarchy()
        {
            // Arrange
            PropertyInfo property = CreateMockPropertyInfo("TestProperty");
            PropertyInfo otherProperty = CreateMockPropertyInfo("OtherTestProperty");
            ODataModelBuilder modelBuilder = ODataConventionModelBuilderFactory.Create();
            EntityTypeConfiguration baseEntityType = new EntityTypeConfiguration(modelBuilder, typeof(object));

            EntityTypeConfiguration entityType = new EntityTypeConfiguration(modelBuilder, typeof(Int32));
            entityType.BaseType = baseEntityType;

            PrimitivePropertyConfiguration primitiveProperty = new PrimitivePropertyConfiguration(property, entityType);
            entityType.ExplicitProperties.Add(property, primitiveProperty);
            baseEntityType.ExplicitProperties.Add(otherProperty, new PrimitivePropertyConfiguration(otherProperty, baseEntityType));
            TimestampAttributeEdmPropertyConvention convention = new TimestampAttributeEdmPropertyConvention();

            // Act
            convention.Apply(primitiveProperty, entityType, ODataConventionModelBuilderFactory.Create());

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
