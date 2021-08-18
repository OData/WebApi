//-----------------------------------------------------------------------------
// <copyright file="IgnoreDataMemberAttributeEdmPropertyConventionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Builder.Conventions.Attributes;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder.Conventions.Attributes
{
    public class IgnoreDataMemberAttributeEdmPropertyConventionTests
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            ExceptionAssert.DoesNotThrow(() => new IgnoreDataMemberAttributeEdmPropertyConvention());
        }

        [Fact]
        public void Apply_Calls_RemovesProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
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
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();

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
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
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
