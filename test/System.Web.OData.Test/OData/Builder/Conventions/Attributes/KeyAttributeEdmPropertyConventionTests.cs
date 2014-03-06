// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder.Conventions.Attributes
{
    public class KeyAttributeEdmPropertyConventionTests
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            Assert.DoesNotThrow(() => new KeyAttributeEdmPropertyConvention());
        }

        [Fact]
        public void Apply_AddsKey_EntityTypeConfiguration()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { new KeyAttribute() });

            Mock<EntityTypeConfiguration> entityType = new Mock<EntityTypeConfiguration>(MockBehavior.Strict);

            Mock<ComplexPropertyConfiguration> complexProperty = 
                new Mock<ComplexPropertyConfiguration>(property.Object, entityType.Object);

            // Act
            new KeyAttributeEdmPropertyConvention().Apply(complexProperty.Object, entityType.Object, builder);

            // Assert
            entityType.Verify();
        }

        [Fact]
        public void Apply_IgnoresKey_NavigationProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { new KeyAttribute() });

            Mock<EntityTypeConfiguration> entityType = new Mock<EntityTypeConfiguration>(MockBehavior.Strict);
            
            Mock<NavigationPropertyConfiguration> navigationProperty = 
                new Mock<NavigationPropertyConfiguration>(property.Object, EdmMultiplicity.ZeroOrOne, entityType.Object);

            // Act
            new KeyAttributeEdmPropertyConvention().Apply(navigationProperty.Object, entityType.Object, builder);

            // Assert
            entityType.Verify();
        }
    }
}
