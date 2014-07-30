// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    public class DataContractAttributeEdmTypeConventionTests
    {
        private DataContractAttributeEdmTypeConvention _convention = new DataContractAttributeEdmTypeConvention();

        [Fact]
        public void DefaultCtor()
        {
            Assert.DoesNotThrow(() => new DataContractAttributeEdmTypeConvention());
        }

        [Fact]
        public void Apply_RemovesAllPropertiesThatAreNotDataMembers()
        {
            // Arrange
            Mock<Type> clrType = new Mock<Type>();
            clrType.Setup(t => t.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { new DataContractAttribute() });

            Mock<StructuralTypeConfiguration> type = new Mock<StructuralTypeConfiguration>(MockBehavior.Strict);
            type.Setup(t => t.ClrType).Returns(clrType.Object);

            var mockPropertyWithoutAttributes = CreateMockProperty();
            type.Object.ExplicitProperties.Add(new MockPropertyInfo(), CreateMockProperty(new DataMemberAttribute()));
            type.Object.ExplicitProperties.Add(new MockPropertyInfo(), CreateMockProperty(new DataMemberAttribute()));
            type.Object.ExplicitProperties.Add(new MockPropertyInfo(), mockPropertyWithoutAttributes);

            type.Setup(t => t.RemoveProperty(mockPropertyWithoutAttributes.PropertyInfo)).Verifiable();

            // Act
            _convention.Apply(type.Object, new Mock<ODataModelBuilder>().Object);

            // Assert
            type.Verify();
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
            _convention.Apply(entity, builder);

            // Assert
            Assert.Contains(propertyInfo, entity.ExplicitProperties.Keys);
            Assert.DoesNotContain(propertyInfo, entity.RemovedProperties);

        }

        private static PropertyConfiguration CreateMockProperty(params Attribute[] attributes)
        {
            StructuralTypeConfiguration structuralType = new Mock<StructuralTypeConfiguration>().Object;
            Mock<PropertyInfo> propertyInfo = new Mock<PropertyInfo>();
            propertyInfo.Setup(p => p.PropertyType).Returns(typeof(int));
            propertyInfo.Setup(p => p.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>())).Returns(attributes);
            return new PrimitivePropertyConfiguration(propertyInfo.Object, structuralType) { AddedExplicitly = false };
        }

        [DataContract]
        private class TestEntity
        {
            [DataMember]
            public int DataMemberProperty { get; set; }

            public int ExplicitlyAddedProperty { get; set; }
        }
    }
}
