// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
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
            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { new IgnoreDataMemberAttribute() });

            Mock<Type> type = new Mock<Type>();

            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>(MockBehavior.Strict);
            Mock<PropertyConfiguration> primitiveProperty = new Mock<PropertyConfiguration>(property.Object, structuralType.Object);
            primitiveProperty.Object.AddedExplicitly = false;
            structuralType.Setup(e => e.RemoveProperty(property.Object)).Verifiable();
            structuralType.Setup(s => s.ClrType).Returns(type.Object);

            // Act
            new IgnoreDataMemberAttributeEdmPropertyConvention().Apply(primitiveProperty.Object, structuralType.Object);

            // Assert
            structuralType.Verify();
        }

        [Fact]
        public void Apply_DoesnotRemoveProperty_TypeIsDataContractAndPropertyHasDataMember()
        {
            // Arrange
            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>())).Returns(new Attribute[] { new IgnoreDataMemberAttribute(), new DataContractAttribute() });
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new Attribute[] { new IgnoreDataMemberAttribute(), new DataContractAttribute() });

            Mock<Type> type = new Mock<Type>();
            type.Setup(t => t.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>())).Returns(new[] { new DataContractAttribute() });

            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>(MockBehavior.Strict);
            Mock<PropertyConfiguration> primitiveProperty = new Mock<PropertyConfiguration>(property.Object, structuralType.Object);
            structuralType.Setup(s => s.ClrType).Returns(type.Object);

            // Act
            new IgnoreDataMemberAttributeEdmPropertyConvention().Apply(primitiveProperty.Object, structuralType.Object);

            // Assert
            structuralType.Verify();
        }

        [Fact]
        public void Apply_DoesnotRemove_ExplicitlyAddedProperties()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            PropertyInfo propertyInfo = typeof(TestEntity).GetProperty("ExplicitlyAddedProperty");
            EntityTypeConfiguration entity = builder.AddEntity(typeof(TestEntity));
            PropertyConfiguration property = entity.AddProperty(propertyInfo);

            // Act
            new IgnoreDataMemberAttributeEdmPropertyConvention().Apply(property, entity);

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
