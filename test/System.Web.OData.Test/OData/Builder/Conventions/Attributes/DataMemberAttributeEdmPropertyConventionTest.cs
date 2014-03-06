// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder.Conventions.Attributes
{
    public class DataMemberAttributeEdmPropertyConventionTest
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            Assert.DoesNotThrow(() => new DataMemberAttributeEdmPropertyConvention());
        }

        [Fact]
        public void Apply_SetsRequiredProperty()
        {
            // Arrange
            MockType type =
                new MockType("Mocktype")
                .Property(typeof(string), "Property", new[] { new DataMemberAttribute { IsRequired = true } });
            type.Setup(t => t.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>())).Returns(new[] { new DataContractAttribute() });

            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>();
            structuralType.Setup(t => t.ClrType).Returns(type);

            PropertyInfo property = type.GetProperty("Property");
            Mock<StructuralPropertyConfiguration> structuralProperty = new Mock<StructuralPropertyConfiguration>(property, structuralType.Object);
            structuralProperty.Object.AddedExplicitly = false;

            // Act
            new DataMemberAttributeEdmPropertyConvention().Apply(structuralProperty.Object, structuralType.Object, new ODataConventionModelBuilder());

            // Assert
            Assert.False(structuralProperty.Object.OptionalProperty);
        }

        [Fact]
        public void Apply_DoesnotSetRequiredProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            MockType type =
                new MockType("Mocktype")
                .Property(typeof(string), "Property", new[] { new DataMemberAttribute { IsRequired = false } });
            type.Setup(t => t.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>())).Returns(new[] { new DataContractAttribute() });

            PropertyInfo property = type.GetProperty("Property");
            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>();
            structuralType.Setup(s => s.ModelBuilder).Returns(builder);
            Mock<StructuralPropertyConfiguration> structuralProperty = new Mock<StructuralPropertyConfiguration>(property, structuralType.Object);
            structuralProperty.Object.AddedExplicitly = false;
            structuralType.Setup(t => t.ClrType).Returns(type);

            // Act
            new DataMemberAttributeEdmPropertyConvention().Apply(structuralProperty.Object, structuralType.Object, builder);

            // Assert
            Assert.True(structuralProperty.Object.OptionalProperty);
        }

        [Fact]
        public void Apply_DoesnotSetRequiredProperty_IfTypeIsNotADataContract()
        {
            // Arrange
            MockType type =
                new MockType("Mocktype")
                .Property(typeof(string), "Property", new[] { new DataMemberAttribute { IsRequired = true } });
            type.Setup(t => t.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>())).Returns(new object[0]);

            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>();
            structuralType.Setup(t => t.ClrType).Returns(type);

            PropertyInfo property = type.GetProperty("Property");
            Mock<StructuralPropertyConfiguration> structuralProperty = new Mock<StructuralPropertyConfiguration>(property, structuralType.Object);
            structuralProperty.Object.AddedExplicitly = false;

            // Act
            new DataMemberAttributeEdmPropertyConvention().Apply(structuralProperty.Object, structuralType.Object, new ODataConventionModelBuilder());

            // Assert
            Assert.True(structuralProperty.Object.OptionalProperty);
        }

        [Theory]
        [InlineData("PropertyAlias", true, "PropertyAlias")]
        [InlineData("PropertyAlias", false, "Property")]
        public void Apply_AliasSetIfEnabled_ValidPropertyAlias(string propertyAlias, bool modelAliasing, string expectedProptertyName)
        {
            // Arrange
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder() { ModelAliasingEnabled = modelAliasing };

            MockType type =
                new MockType("Mocktype")
                .Property(typeof(string), "Property", new[] { new DataMemberAttribute { Name = propertyAlias } });
            type.Setup(t => t.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>())).Returns(new[] { new DataContractAttribute() });

            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>();
            structuralType.Setup(t => t.ClrType).Returns(type);

            PropertyInfo property = type.GetProperty("Property");
            Mock<StructuralPropertyConfiguration> structuralProperty = new Mock<StructuralPropertyConfiguration>(property, structuralType.Object);
            structuralProperty.Object.AddedExplicitly = false;

            // Act
            new DataMemberAttributeEdmPropertyConvention().Apply(structuralProperty.Object, structuralType.Object, modelBuilder);

            // Assert
            Assert.Equal(expectedProptertyName, structuralProperty.Object.Name);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData(" ", true)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        public void Apply_AliasNotSet_InvalidPropertyAlias(string propertyAlias, bool modelAliasing)
        {
            // Arrange
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder() { ModelAliasingEnabled = modelAliasing };

            MockType type =
                new MockType("Mocktype")
                .Property(typeof(string), "Property", new[] { new DataMemberAttribute { Name = propertyAlias } });
            type.Setup(t => t.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>())).Returns(new[] { new DataContractAttribute() });

            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>();
            structuralType.Setup(t => t.ClrType).Returns(type);

            PropertyInfo property = type.GetProperty("Property");
            Mock<StructuralPropertyConfiguration> structuralProperty = new Mock<StructuralPropertyConfiguration>(property, structuralType.Object);
            structuralProperty.Object.AddedExplicitly = false;

            // Act
            new DataMemberAttributeEdmPropertyConvention().Apply(structuralProperty.Object, structuralType.Object, modelBuilder);

            // Assert
            Assert.Equal("Property", structuralProperty.Object.Name);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Apply_AliasNotSet_NoPropertyAlias(bool modelAliasing)
        {
            // Arrange
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder() { ModelAliasingEnabled = modelAliasing };

            MockType type =
                new MockType("Mocktype")
                .Property(typeof(string), "Property", new[] { new DataMemberAttribute() });
            type.Setup(t => t.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>())).Returns(new[] { new DataContractAttribute() });

            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>();
            structuralType.Setup(t => t.ClrType).Returns(type);

            PropertyInfo property = type.GetProperty("Property");
            Mock<StructuralPropertyConfiguration> structuralProperty = new Mock<StructuralPropertyConfiguration>(property, structuralType.Object);
            structuralProperty.Object.AddedExplicitly = false;

            // Act
            new DataMemberAttributeEdmPropertyConvention().Apply(structuralProperty.Object, structuralType.Object, modelBuilder);

            // Assert
            Assert.Equal("Property", structuralProperty.Object.Name);
        }

        [Fact]
        public void DataMemberAttributeEdmPropertyConvention_ConfiguresRequiredDataMembersAsRequired()
        {
            MockType type =
                new MockType("Entity")
                .Property(typeof(int), "ID", new DataMemberAttribute())
                .Property(typeof(string), "Name", new DataMemberAttribute { IsRequired = true });
            type.Setup(t => t.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>())).Returns(new[] { new DataContractAttribute() });

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntityType(type);

            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType entity = model.AssertHasEntityType(type);
            entity.AssertHasKey(model, "ID", EdmPrimitiveTypeKind.Int32);
            entity.AssertHasPrimitiveProperty(model, "Name", EdmPrimitiveTypeKind.String, isNullable: false);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DataMemberAttributeEdmPropertyConvention_ConfiguresNavigationDataMembers(bool isRequired)
        {
            MockType relatedType =
                new MockType("RelatedEntity")
                .Property<int>("ID");
            MockType type =
                new MockType("Entity")
                .Property(typeof(int), "ID")
                .Property(relatedType, "RelatedEntity", new DataMemberAttribute { IsRequired = isRequired });
            type.Setup(t => t.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>())).Returns(new[] { new DataContractAttribute() });

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntityType(type);

            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType entity = model.AssertHasEntityType(type);
            entity.AssertHasKey(model, "ID", EdmPrimitiveTypeKind.Int32);
            entity.AssertHasNavigationProperty(model, "RelatedEntity", relatedType, isNullable: !isRequired, multiplicity: isRequired ? EdmMultiplicity.One : EdmMultiplicity.ZeroOrOne);
        }

        [Fact]
        public void DataMemberAttributeEdmPropertyConvention_ConfiguresNonRequiredDataMembersAsOptional()
        {
            MockType type =
                new MockType("Entity")
                .Property(typeof(int), "ID", new DataMemberAttribute())
                .Property(typeof(int), "Count", new DataMemberAttribute());
            type.Setup(t => t.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>())).Returns(new[] { new DataContractAttribute() });

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntityType(type);

            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType entity = model.AssertHasEntityType(type);
            entity.AssertHasKey(model, "ID", EdmPrimitiveTypeKind.Int32);
            entity.AssertHasPrimitiveProperty(model, "Count", EdmPrimitiveTypeKind.Int32, isNullable: true);
        }

        [Fact]
        public void DataMemberAttributeEdmPropertyConvention_DoesnotOverwriteExistingConfiguration()
        {
            MockType type =
                new MockType("Entity")
                .Property(typeof(int), "ID", new DataMemberAttribute())
                .Property(typeof(int), "Count", new DataMemberAttribute { IsRequired = true });
            type.Setup(t => t.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>())).Returns(new[] { new DataContractAttribute() });

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntityType(type).AddProperty(type.GetProperty("Count")).IsOptional();

            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType entity = model.AssertHasEntityType(type);
            entity.AssertHasPrimitiveProperty(model, "Count", EdmPrimitiveTypeKind.Int32, isNullable: true);
        }

        [Fact]
        public void DerivedType_DataMemberRequired_IsNotHonored_IfDerivedtypeIsNotDataContract()
        {
            // Arrange
            MockType baseType = new MockType("BaseMocktype");
            baseType.Setup(t => t.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>())).Returns(new[] { new DataContractAttribute() });
            
            MockType derivedType =
                new MockType("DerivedMockType")
                .Property(typeof(string), "Property", new[] { new DataMemberAttribute { IsRequired = true } })
                .BaseType(baseType);

            PropertyInfo property = derivedType.GetProperty("Property");
            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>();

            Mock<StructuralPropertyConfiguration> structuralProperty = new Mock<StructuralPropertyConfiguration>(property, structuralType.Object);
            structuralType.Setup(t => t.ClrType).Returns(derivedType);

            // Act
            new DataMemberAttributeEdmPropertyConvention().Apply(structuralProperty.Object, structuralType.Object, new ODataConventionModelBuilder());

            // Assert
            Assert.True(structuralProperty.Object.OptionalProperty);
        }

        [Fact]
        public void DerivedType_DataMemberRequired_IsHonored_IfDerivedtypeIsDataContract()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

            MockType baseType = new MockType("BaseMocktype");
            baseType.Setup(t => t.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>())).Returns(new[] { new DataContractAttribute() });

            MockType derivedType =
                new MockType("DerivedMockType")
                .Property(typeof(int), "Property", new[] { new DataMemberAttribute { IsRequired = true } })
                .BaseType(baseType);

            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>();
            structuralType.Setup(t => t.ClrType).Returns(derivedType);

            PropertyInfo property = derivedType.GetProperty("Property");
            Mock<StructuralPropertyConfiguration> structuralProperty = new Mock<StructuralPropertyConfiguration>(property, structuralType.Object);

            // Act
            new DataMemberAttributeEdmPropertyConvention().Apply(structuralProperty.Object, structuralType.Object, builder);

            // Assert
            Assert.False(structuralProperty.Object.OptionalProperty);
        }

        [Theory]
        [InlineData("AliasRelatedEntity", true)]
        [InlineData("RelatedEntity", false)]
        public void DataMemberAttributeEdmPropertyConvention_PropertyAliased_IfEnabled(string propertyAlias, bool modelAliasing)
        {
            // Arrange
            MockType relatedType =
                new MockType("RelatedEntity")
                .Property<int>("ID");
            MockType type =
                new MockType("Entity")
                .Property(typeof(int), "ID")
                .Property(relatedType, "RelatedEntity", new DataMemberAttribute { Name = "AliasRelatedEntity" });
            type.Setup(t => t.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>()))
                .Returns(new[] { new DataContractAttribute() });

            // Act
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder { ModelAliasingEnabled = modelAliasing };
            builder.AddEntityType(type);
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType entity = model.AssertHasEntityType(type);
            entity.AssertHasKey(model, "ID", EdmPrimitiveTypeKind.Int32);
            entity.AssertHasNavigationProperty(
                model,
                propertyAlias,
                relatedType,
                isNullable: true,
                multiplicity: EdmMultiplicity.ZeroOrOne);
        }

        [Theory]
        [InlineData("AliasRelatedEntity", true)]
        [InlineData("RelatedEntity", false)]
        public void DataMemberAttributeEdmPropertyConvention_DerivedClassPropertyAliased_IfEnabled(string propertyAlias, bool modelAliasing)
        {
            // Arrange
            MockType baseType = new MockType("BaseMocktype")
                .Property<int>("ID");
            baseType.Setup(t => t.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>()))
                .Returns(new[] { new DataContractAttribute() });

            MockType relatedType =
                new MockType("RelatedEntity")
                .Property<int>("ID");

            MockType derivedType =
                new MockType("DerivedMockType")
                .Property(relatedType, "RelatedEntity", new DataMemberAttribute { Name = "AliasRelatedEntity" })
                .BaseType(baseType);
            derivedType.Setup(t => t.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>()))
                .Returns(new[] { new DataContractAttribute() });

            // Act
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder { ModelAliasingEnabled = modelAliasing };
            builder.AddEntityType(derivedType);
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType entity = model.AssertHasEntityType(derivedType);
            entity.AssertHasKey(model, "ID", EdmPrimitiveTypeKind.Int32);
            entity.AssertHasNavigationProperty(
                model,
                propertyAlias,
                relatedType,
                isNullable: true,
                multiplicity: EdmMultiplicity.ZeroOrOne);
        }
    }
}
