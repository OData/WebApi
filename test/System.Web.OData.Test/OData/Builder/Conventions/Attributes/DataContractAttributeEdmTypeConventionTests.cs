// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder.Conventions.Attributes
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder() { ModelAliasingEnabled = false };
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
            _convention.Apply(type.Object, builder);

            // Assert
            type.Verify();
        }

        [Fact]
        public void Apply_DoesnotRemove_ExplicitlyAddedProperties()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder() { ModelAliasingEnabled = false };
            PropertyInfo propertyInfo = typeof(TestEntity).GetProperty("ExplicitlyAddedProperty");
            EntityTypeConfiguration entity = builder.AddEntityType(typeof(TestEntity));
            PropertyConfiguration property = entity.AddProperty(propertyInfo);

            // Act
            _convention.Apply(entity, builder);

            // Assert
            Assert.Contains(propertyInfo, entity.ExplicitProperties.Keys);
            Assert.DoesNotContain(propertyInfo, entity.RemovedProperties);
        }

        [Fact]
        public void Apply_NameAndNamespaceAliased_IfModelAliasingEnabled()
        {
            // Arrange
            Mock<Type> clrType = new Mock<Type>();
            clrType.Setup(t => t.GetCustomAttributes(It.IsAny<bool>()))
                   .Returns(new[] { new DataContractAttribute { Name = "NameAlias", Namespace = "com.contoso" } });

            Mock<StructuralTypeConfiguration> type = new Mock<StructuralTypeConfiguration> { CallBase = true };
            type.Setup(t => t.ClrType).Returns(clrType.Object);
            StructuralTypeConfiguration configuration = type.Object;

            // Act
            _convention.Apply(configuration, new ODataConventionModelBuilder());

            // Assert
            Assert.Equal("NameAlias", configuration.Name);
            Assert.Equal("com.contoso", configuration.Namespace);
            Assert.False(configuration.AddedExplicitly);
        }

        [Fact]
        public void Apply_NameAliased_IfModelAliasingEnabled()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            Mock<Type> clrType = new Mock<Type>();
            clrType.Setup(t => t.GetCustomAttributes(It.IsAny<bool>()))
                   .Returns(new[] { new DataContractAttribute { Name = "NameAlias" } });

            Mock<StructuralTypeConfiguration> type = new Mock<StructuralTypeConfiguration> { CallBase = true };
            type.Setup(t => t.ClrType).Returns(clrType.Object);
            StructuralTypeConfiguration configuration = type.Object;

            // Act
            _convention.Apply(configuration, builder);

            // Assert
            Assert.Equal("NameAlias", configuration.Name);
            Assert.False(configuration.AddedExplicitly);
        }

        [Fact]
        public void Apply_NamespaceAliased_IfModelAliasingEnabled()
        {
            // Arrange
            Mock<Type> clrType = new Mock<Type>();
            clrType.Setup(t => t.GetCustomAttributes(It.IsAny<bool>()))
                   .Returns(new[] { new DataContractAttribute { Namespace = "com.contoso" } });

            Mock<StructuralTypeConfiguration> type = new Mock<StructuralTypeConfiguration> { CallBase = true };
            type.Setup(t => t.ClrType).Returns(clrType.Object);
            type.SetupProperty(t => t.Namespace);
            StructuralTypeConfiguration configuration = type.Object;

            // Act
            _convention.Apply(configuration, new ODataConventionModelBuilder());

            // Assert
            Assert.Equal("com.contoso", configuration.Namespace);
            Assert.False(configuration.AddedExplicitly);
        }

        [Fact]
        public void Apply_NameAndNamespaceNotAliased_IfDataContractHasNoValue()
        {
            // Arrange
            Mock<Type> clrType = new Mock<Type>();
            clrType.Setup(t => t.GetCustomAttributes(It.IsAny<bool>()))
                   .Returns(new[] { new DataContractAttribute() });

            Mock<StructuralTypeConfiguration> type = new Mock<StructuralTypeConfiguration> { CallBase = true };
            type.Setup(t => t.ClrType).Returns(clrType.Object);
            StructuralTypeConfiguration configuration = type.Object;
            configuration.Name = "Name";
            configuration.Namespace = "Namespace";

            // Act
            _convention.Apply(configuration, new ODataConventionModelBuilder());

            // Assert
            Assert.Equal("Name", configuration.Name);
            Assert.Equal("Namespace", configuration.Namespace);
            Assert.True(configuration.AddedExplicitly);
        }

        [Fact]
        public void Apply_NameAndNamespaceNotAliased_IfAddedExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder() { ModelAliasingEnabled = true };

            Mock<Type> clrType = new Mock<Type>();
            clrType.Setup(t => t.GetCustomAttributes(It.IsAny<bool>()))
                   .Returns(new[] { new DataContractAttribute { Name = "NameAlias", Namespace = "com.contoso" } });

            Mock<StructuralTypeConfiguration> type = new Mock<StructuralTypeConfiguration> { CallBase = true };
            type.Setup(t => t.ClrType).Returns(clrType.Object);
            StructuralTypeConfiguration configuration = type.Object;
            configuration.Name = "Name";
            configuration.Namespace = "Namespace";

            // Act
            _convention.Apply(configuration, builder);

            // Assert
            Assert.Equal("Name", configuration.Name);
            Assert.Equal("Namespace", configuration.Namespace);
            Assert.True(configuration.AddedExplicitly);
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
