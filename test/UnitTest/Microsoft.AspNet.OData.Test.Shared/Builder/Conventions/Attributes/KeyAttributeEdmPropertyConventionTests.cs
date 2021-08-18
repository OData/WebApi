//-----------------------------------------------------------------------------
// <copyright file="KeyAttributeEdmPropertyConventionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Builder.Conventions.Attributes;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder.Conventions.Attributes
{
    public class KeyAttributeEdmPropertyConventionTests
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            ExceptionAssert.DoesNotThrow(() => new KeyAttributeEdmPropertyConvention());
        }

        [Fact]
        public void Apply_AddsKey_EntityTypeConfiguration()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();

            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { new KeyAttribute() });

            Mock<EntityTypeConfiguration> entityType = new Mock<EntityTypeConfiguration>(MockBehavior.Strict);
            entityType.Setup(e => e.HasKey(property.Object)).Returns(entityType.Object).Verifiable();
            
            Mock<PrimitivePropertyConfiguration> primitiveProperty = 
                new Mock<PrimitivePropertyConfiguration>(property.Object, entityType.Object);

            // Act
            new KeyAttributeEdmPropertyConvention().Apply(primitiveProperty.Object, entityType.Object, builder);

            // Assert
            entityType.Verify();
        }

        [Fact]
        public void Apply_IgnoresKey_NonEntityTypeConfiguration()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();

            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { new KeyAttribute() });

            Mock<ComplexTypeConfiguration> complexType = new Mock<ComplexTypeConfiguration>(MockBehavior.Strict);

            Mock<PrimitivePropertyConfiguration> primitiveProperty = 
                new Mock<PrimitivePropertyConfiguration>(property.Object, complexType.Object);

            // Act
            new KeyAttributeEdmPropertyConvention().Apply(primitiveProperty.Object, complexType.Object, builder);

            // Assert
            complexType.Verify();
        }

        [Fact]
        public void Apply_IgnoresKey_ComplexProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();

            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { new KeyAttribute() });

            Mock<EntityTypeConfiguration> entityType = new Mock<EntityTypeConfiguration>(MockBehavior.Strict);

            Mock<ComplexPropertyConfiguration> complexProperty = 
                new Mock<ComplexPropertyConfiguration>(property.Object, entityType.Object);
            complexProperty.Setup(c => c.Kind).Returns(PropertyKind.Complex);

            // Act
            new KeyAttributeEdmPropertyConvention().Apply(complexProperty.Object, entityType.Object, builder);

            // Assert
            entityType.Verify();
        }

        [Fact]
        public void Apply_IgnoresKey_NavigationProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();

            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { new KeyAttribute() });

            Mock<EntityTypeConfiguration> entityType = new Mock<EntityTypeConfiguration>(MockBehavior.Strict);
            
            Mock<NavigationPropertyConfiguration> navigationProperty = 
                new Mock<NavigationPropertyConfiguration>(property.Object, EdmMultiplicity.ZeroOrOne, entityType.Object);
            navigationProperty.Setup(c => c.Kind).Returns(PropertyKind.Navigation);

            // Act
            new KeyAttributeEdmPropertyConvention().Apply(navigationProperty.Object, entityType.Object, builder);

            // Assert
            entityType.Verify();
        }

        [Fact]
        public void Apply_AddsEnumKey_EntityTypeConfiguration()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();

            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(MyEnumType));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { new KeyAttribute() });

            Mock<EntityTypeConfiguration> entityType = new Mock<EntityTypeConfiguration>(MockBehavior.Strict);
            entityType.Setup(e => e.HasKey(property.Object)).Returns(entityType.Object).Verifiable();

            Mock<EnumPropertyConfiguration> enumProperty =
                new Mock<EnumPropertyConfiguration>(property.Object, entityType.Object);

            // Act
            new KeyAttributeEdmPropertyConvention().Apply(enumProperty.Object, entityType.Object, builder);

            // Assert
            entityType.Verify();
        }

        enum MyEnumType
        {
        }
    }
}
