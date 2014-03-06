// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder.Conventions.Attributes
{
    public class IgnoreDataMemberAttributeEdmPropertyConventionTests
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            Assert.DoesNotThrow(() => new IgnoreDataMemberAttributeEdmPropertyConvention());
        }

        [Fact]
        public void Apply_Calls_RemovesProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { new IgnoreDataMemberAttribute() });

            Mock<Type> type = new Mock<Type>();
            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>(MockBehavior.Strict);
            structuralType.Setup(e => e.RemoveProperty(property.Object)).Verifiable();
            structuralType.Setup(s => s.ClrType).Returns(type.Object);

            Mock<PropertyConfiguration> primitiveProperty = new Mock<PropertyConfiguration>(property.Object, structuralType.Object);
            primitiveProperty.Object.AddedExplicitly = false;

            // Act
            new IgnoreDataMemberAttributeEdmPropertyConvention().Apply(primitiveProperty.Object, structuralType.Object, builder);

            // Assert
            structuralType.Verify();
        }

        [Fact]
        public void Apply_DoesnotRemoveProperty_TypeIsDataContractAndPropertyHasDataMember()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>())).Returns(new Attribute[] 
            { 
                new IgnoreDataMemberAttribute(), 
                new DataContractAttribute() 
            });
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new Attribute[] 
            { new IgnoreDataMemberAttribute(), 
                new DataContractAttribute() 
            });

            Mock<Type> type = new Mock<Type>();
            type.Setup(t => t.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>())).Returns(new[] { new DataContractAttribute() });
            
            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>(MockBehavior.Strict);
            structuralType.Setup(s => s.ClrType).Returns(type.Object);

            Mock<PropertyConfiguration> primitiveProperty = new Mock<PropertyConfiguration>(property.Object, structuralType.Object);

            // Act
            new IgnoreDataMemberAttributeEdmPropertyConvention().Apply(primitiveProperty.Object, structuralType.Object, builder);

            // Assert
            structuralType.Verify();
        }

        [Fact]
        public void Apply_DoesnotRemove_ExplicitlyAddedProperties()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            PropertyInfo propertyInfo = typeof(TestEntity).GetProperty("ExplicitlyAddedProperty");
            EntityTypeConfiguration entity = builder.AddEntityType(typeof(TestEntity));
            PropertyConfiguration property = entity.AddProperty(propertyInfo);

            // Act
            new IgnoreDataMemberAttributeEdmPropertyConvention().Apply(property, entity, builder);

            // Assert
            Assert.Contains(propertyInfo, entity.ExplicitProperties.Keys);
            Assert.DoesNotContain(propertyInfo, entity.RemovedProperties);

        }

        [DataContract]
        private class TestEntity
        {
            [IgnoreDataMember]
            public int ExplicitlyAddedProperty { get; set; }
        }
    }
}
