// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Builder.Conventions.Attributes;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder.Conventions.Attributes
{
    public class NotMappedAttributeConventionTests
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            ExceptionAssert.DoesNotThrow(() => new NotMappedAttributeConvention());
        }

        [Fact]
        public void Apply_Calls_RemovesProperty_ForInferredProperties()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();

            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { new NotMappedAttribute() });

            Mock<PropertyDescriptor> propertyDescriptor = new Mock<PropertyDescriptor>(property.Object);

            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>(MockBehavior.Strict);
            structuralType.Setup(e => e.RemoveProperty(property.Object)).Verifiable();
            
            Mock<PropertyConfiguration> primitiveProperty = new Mock<PropertyConfiguration>(propertyDescriptor.Object, structuralType.Object);
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
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            
            PropertyInfo propertyInfo = typeof(TestEntity).GetProperty("Property");
            PropertyDescriptor propertyDescriptor = new PropertyDescriptor(propertyInfo);
            EntityTypeConfiguration entity = builder.AddEntityType(typeof(TestEntity));
            PropertyConfiguration property = entity.AddProperty(propertyInfo);

            // Act
            new NotMappedAttributeConvention().Apply(property, entity, builder);

            // Assert
            Assert.Contains(propertyDescriptor, entity.ExplicitProperties.Keys);
            Assert.DoesNotContain(propertyInfo, entity.RemovedProperties);
        }

        private class TestEntity
        {
            [NotMapped]
            public int Property { get; set; }
        }
    }
}
