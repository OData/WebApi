// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder.Conventions.Attributes
{
    public class NotMappedAttributeConventionTests
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            Assert.DoesNotThrow(() => new NotMappedAttributeConvention());
        }

        [Fact]
        public void Apply_Calls_RemovesProperty_ForInferredProperties()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { new NotMappedAttribute() });

            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>(MockBehavior.Strict);
            structuralType.Setup(e => e.RemoveProperty(property.Object)).Verifiable();
            
            Mock<PropertyConfiguration> primitiveProperty = new Mock<PropertyConfiguration>(property.Object, structuralType.Object);
            primitiveProperty.Object.AddedExplicitly = false;

            // Act
            new NotMappedAttributeConvention().Apply(primitiveProperty.Object, structuralType.Object, builder);

            // Assert
            structuralType.Verify();
        }

        [Fact]
        public void Apply_DoesnotRemove_ExplicitlyAddedProperties()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            
            PropertyInfo propertyInfo = typeof(TestEntity).GetProperty("Property");
            EntityTypeConfiguration entity = builder.AddEntityType(typeof(TestEntity));
            PropertyConfiguration property = entity.AddProperty(propertyInfo);

            // Act
            new NotMappedAttributeConvention().Apply(property, entity, builder);

            // Assert
            Assert.Contains(propertyInfo, entity.ExplicitProperties.Keys);
            Assert.DoesNotContain(propertyInfo, entity.RemovedProperties);
        }

        private class TestEntity
        {
            [NotMapped]
            public int Property { get; set; }
        }
    }
}
