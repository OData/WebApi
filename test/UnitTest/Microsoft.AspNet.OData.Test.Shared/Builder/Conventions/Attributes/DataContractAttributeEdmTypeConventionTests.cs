//-----------------------------------------------------------------------------
// <copyright file="DataContractAttributeEdmTypeConventionTests.cs" company=".NET Foundation">
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
    public class DataContractAttributeEdmTypeConventionTests
    {
        private DataContractAttributeEdmTypeConvention _convention = new DataContractAttributeEdmTypeConvention();

        [Fact]
        public void DefaultCtor()
        {
            ExceptionAssert.DoesNotThrow(() => new DataContractAttributeEdmTypeConvention());
        }

        [Fact]
        public void Apply_RemovesAllPropertiesThatAreNotDataMembers()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderHelper.CreateWithModelAliasing(modelAliasing: false);
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
            ODataConventionModelBuilder builder = ODataConventionModelBuilderHelper.CreateWithModelAliasing(modelAliasing: false);
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
            _convention.Apply(configuration, ODataConventionModelBuilderFactory.Create());

            // Assert
            Assert.Equal("NameAlias", configuration.Name);
            Assert.Equal("com.contoso", configuration.Namespace);
            Assert.False(configuration.AddedExplicitly);
        }

        [Fact]
        public void Apply_NameAliased_IfModelAliasingEnabled()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
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
            _convention.Apply(configuration, ODataConventionModelBuilderFactory.Create());

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
            _convention.Apply(configuration, ODataConventionModelBuilderFactory.Create());

            // Assert
            Assert.Equal("Name", configuration.Name);
            Assert.Equal("Namespace", configuration.Namespace);
            Assert.True(configuration.AddedExplicitly);
        }

        [Fact]
        public void Apply_NameAndNamespaceNotAliased_IfAddedExplicitly()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderHelper.CreateWithModelAliasing(modelAliasing: true);

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
