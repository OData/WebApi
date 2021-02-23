// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Types;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.V1;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
{
    public class EnumTypeTest
    {
        [Fact]
        public void CreateEnumTypeWithFlags()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var color = builder.EnumType<Color>();
            color.Member(Color.Red);
            color.Member(Color.Green);
            color.Member(Color.Blue);

            // Act
            var model = builder.GetEdmModel();
            var colorType = model.SchemaElements.OfType<IEdmEnumType>().Single();

            // Assert
            Assert.Equal("Color", colorType.Name);
            Assert.True(colorType.IsFlags);
            Assert.Equal(3, colorType.Members.Count());
            Assert.Equal("Edm.Int32", colorType.UnderlyingType.FullName());

            var redMember = colorType.Members.SingleOrDefault(m => m.Name == "Red");
            var greenMember = colorType.Members.SingleOrDefault(m => m.Name == "Green");
            var blueMember = colorType.Members.SingleOrDefault(m => m.Name == "Blue");

            Assert.NotNull(redMember);
            Assert.NotNull(greenMember);
            Assert.NotNull(blueMember);
        }

        [Fact]
        public void CreateEnumTypeWithoutFlags()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var simple = builder.EnumType<SimpleEnum>();
            simple.Member(SimpleEnum.First);
            simple.Member(SimpleEnum.Second);
            simple.Member(SimpleEnum.Third);

            // Act
            var model = builder.GetEdmModel();
            var colorType = model.SchemaElements.OfType<IEdmEnumType>().Single();

            // Assert
            Assert.False(colorType.IsFlags);
        }

        [Fact]
        public void CreateNullableEnumType()
        {
            // Arrange
            var builder = new ODataModelBuilder().Add_Color_EnumType();
            var complexTypeConfiguration = builder.ComplexType<ComplexTypeWithEnumTypePropertyTestModel>();
            complexTypeConfiguration.EnumProperty(c => c.NullableColor);

            // Act
            var model = builder.GetEdmModel();
            var complexType = model.SchemaElements.OfType<IEdmStructuredType>().Single();

            // Assert
            Assert.Single(complexType.Properties());
            var nullableColor = complexType.Properties().SingleOrDefault(p => p.Name == "NullableColor");
            Assert.NotNull(nullableColor);
            Assert.True(nullableColor.Type.IsEnum());
            Assert.True(nullableColor.Type.IsNullable);
        }

        [Fact]
        public void CreateCollectionOfEnumTypeProperty()
        {
            // Arrange
            var builder = new ODataModelBuilder().Add_Color_EnumType();
            var complexTypeConfiguration = builder.ComplexType<ComplexTypeWithEnumTypePropertyTestModel>();
            complexTypeConfiguration.CollectionProperty(c => c.Colors);

            // Act
            var model = builder.GetEdmModel();
            var complexType = model.SchemaElements.OfType<IEdmStructuredType>().Single();

            // Assert
            Assert.Single(complexType.Properties());
            var colors = complexType.Properties().SingleOrDefault(p => p.Name == "Colors");
            Assert.NotNull(colors);
            Assert.True(colors.Type.IsCollection());
            Assert.True(((IEdmCollectionTypeReference)colors.Type).ElementType().IsEnum());
        }

        [Fact]
        public void CreateArrayOfEnumTypeProperty()
        {
            // Arrange
            var builder = new ODataModelBuilder().Add_Color_EnumType();
            var complexTypeConfiguration = builder.ComplexType<ArrayEnumTypePropertyTestModel>();
            complexTypeConfiguration.CollectionProperty(c => c.Colors);
            complexTypeConfiguration.CollectionProperty(c => c.NullableColors);

            // Act
            var model = builder.GetEdmModel();
            var complexType = model.SchemaElements.OfType<IEdmStructuredType>().Single();

            // Assert
            Assert.Equal(2, complexType.Properties().Count());
            var colors = complexType.Properties().SingleOrDefault(p => p.Name == "Colors");
            Assert.NotNull(colors);
            Assert.True(colors.Type.IsCollection());
            Assert.False(colors.Type.IsNullable);
            Assert.True(((IEdmCollectionTypeReference)colors.Type).ElementType().IsEnum());

            var nullablecolors = complexType.Properties().SingleOrDefault(p => p.Name == "NullableColors");
            Assert.NotNull(nullablecolors);
            Assert.True(nullablecolors.Type.IsCollection());
            Assert.True(nullablecolors.Type.IsNullable);
            Assert.True(((IEdmCollectionTypeReference)nullablecolors.Type).ElementType().IsEnum());
        }

        [Fact]
        public void CreateEnumTypePropertyInComplexType()
        {
            // Arrange
            var builder = new ODataModelBuilder().Add_Color_EnumType();
            var complexTypeConfiguration = builder.ComplexType<ComplexTypeWithEnumTypePropertyTestModel>();
            complexTypeConfiguration.EnumProperty(c => c.RequiredColor);

            // Act
            var model = builder.GetEdmModel();
            var complexType = model.SchemaElements.OfType<IEdmStructuredType>().Single();

            // Assert
            Assert.Single(complexType.Properties());
            var requiredColor = complexType.Properties().SingleOrDefault(p => p.Name == "RequiredColor");
            Assert.NotNull(requiredColor);
            Assert.True(requiredColor.Type.IsEnum());
            Assert.False(requiredColor.Type.IsNullable);
        }

        [Fact]
        public void CreateEnumTypePropertyInEntityType()
        {
            // Arrange
            var builder = new ODataModelBuilder().Add_Color_EnumType().Add_LongEnum_EnumType();
            var entityTypeConfiguration = builder.EntityType<EntityTypeWithEnumTypePropertyTestModel>();
            entityTypeConfiguration.HasKey(e => e.ID);
            entityTypeConfiguration.EnumProperty(e => e.RequiredColor);
            entityTypeConfiguration.EnumProperty(e => e.LongEnum);

            // Act
            var model = builder.GetEdmModel();
            var entityType = model.SchemaElements.OfType<IEdmStructuredType>().Single();

            // Assert
            Assert.Equal(3, entityType.Properties().Count());
            var requiredColor = entityType.Properties().SingleOrDefault(p => p.Name == "RequiredColor");
            var longEnum = entityType.Properties().SingleOrDefault(p => p.Name == "LongEnum");
            Assert.NotNull(requiredColor);
            Assert.NotNull(longEnum);
            Assert.True(requiredColor.Type.IsEnum());
            Assert.True(longEnum.Type.IsEnum());
            Assert.True(EdmCoreModel.Instance.GetInt32(false).Definition == requiredColor.Type.AsEnum().EnumDefinition().UnderlyingType);
            Assert.True(EdmCoreModel.Instance.GetInt64(false).Definition == longEnum.Type.AsEnum().EnumDefinition().UnderlyingType);
        }

        [Fact]
        public void CreateEnumKeyInEntityType()
        {
            // Arrange
            var builder = new ODataModelBuilder().Add_Color_EnumType().Add_LongEnum_EnumType();
            var entityTypeConfiguration = builder.EntityType<EntityTypeWithEnumTypePropertyTestModel>();
            entityTypeConfiguration.HasKey(e => e.RequiredColor);

            // Act
            var model = builder.GetEdmModel();
            var entityType = model.SchemaElements.OfType<IEdmEntityType>().Single();

            // Assert
            IEdmProperty requiredColorProperty = Assert.Single(entityType.Properties());
            Assert.NotNull(requiredColorProperty);

            IEdmStructuralProperty requiredColorKey = Assert.Single(entityType.DeclaredKey);
            Assert.NotNull(requiredColorKey);

            Assert.Same(requiredColorProperty, requiredColorKey);

            Assert.Equal(EdmTypeKind.Enum, requiredColorKey.Type.TypeKind());
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Color", requiredColorKey.Type.Definition.FullTypeName());
        }

        [Fact]
        public void AddAndRemoveEnumMemberFromEnumType()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var color = builder.EnumType<Color>();

            // Act & Assert
            Assert.Empty(color.Members);

            color.Member(Color.Red);
            color.Member(Color.Green);
            Assert.Equal(2, color.Members.Count());

            color.RemoveMember(Color.Red);
            Assert.Single(color.Members);
        }

        [Fact]
        public void EnumPropertyLimitation()
        {
            // Arrange
            var builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>().Add_Color_EnumType();
            var entityTypeConfiguration = builder.EntityType<EntityTypeWithEnumTypePropertyTestModel>();
            entityTypeConfiguration.EnumProperty(c => c.RequiredColor).IsOptional().IsConcurrencyToken();
            builder.EntitySet<EntityTypeWithEnumTypePropertyTestModel>("EntitySet");

            // Act
            var model = builder.GetEdmModel();
            var complexType = model.SchemaElements.OfType<IEdmStructuredType>().Single();
            IEdmStructuralProperty requiredColor =
                complexType.Properties().SingleOrDefault(p => p.Name == "RequiredColor") as IEdmStructuralProperty;
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("EntitySet");

            // Assert
            Assert.NotNull(requiredColor);
            Assert.True(requiredColor.Type.IsNullable);

            Assert.NotNull(entitySet);
            IEnumerable<IEdmStructuralProperty> currencyProperties = model.GetConcurrencyProperties(entitySet);
            IEdmStructuralProperty currencyProperty = Assert.Single(currencyProperties);
            Assert.Same(requiredColor, currencyProperty);
        }

        [Fact]
        public void EnumPropertyWithConcurrencyToken_SetsVocabuaryAnnotaion()
        {
            // Arrange
            var builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>().Add_Color_EnumType();
            var entityTypeConfiguration = builder.EntityType<EntityTypeWithEnumTypePropertyTestModel>();
            entityTypeConfiguration.EnumProperty(c => c.RequiredColor).IsOptional().IsConcurrencyToken();
            builder.EntitySet<EntityTypeWithEnumTypePropertyTestModel>("EnumEntities");

            // Act
            var model = builder.GetEdmModel();

            // Assert
            var entityset = model.FindDeclaredEntitySet("EnumEntities");
            Assert.NotNull(entityset);

            var annotations = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(entityset, CoreVocabularyModel.ConcurrencyTerm);
            IEdmVocabularyAnnotation concurrencyAnnotation = Assert.Single(annotations);

            IEdmCollectionExpression properties = concurrencyAnnotation.Value as IEdmCollectionExpression;
            Assert.NotNull(properties);

            Assert.Single(properties.Elements);
            var element = properties.Elements.First() as IEdmPathExpression;
            Assert.NotNull(element);

            string path = Assert.Single(element.PathSegments);
            Assert.Equal("RequiredColor", path);
        }

        [Fact]
        public void TypeParameterOfEnumTypeIsNotEnumShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => builder.EnumType<ComplexTypeWithEnumTypePropertyTestModel>(),
                "type",
                "The type 'Microsoft.AspNet.OData.Test.Builder.ComplexTypeWithEnumTypePropertyTestModel' cannot be configured as an enum type.");
        }

        [Fact]
        public void PamameterOfEnumPropertyIsNotEnumShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var color = builder.EnumType<Color>();
            color.Member(Color.Red);
            color.Member(Color.Green);
            color.Member(Color.Blue);
            var entityTypeConfiguration = builder.ComplexType<EntityTypeWithEnumTypePropertyTestModel>();

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => entityTypeConfiguration.EnumProperty(e => e.ID),
                "propertyInfo",
                "The property 'ID' on type 'Microsoft.AspNet.OData.Test.Builder.EntityTypeWithEnumTypePropertyTestModel' must be an Enum property.");
        }

        [Fact]
        public void ValueOfEnumMemberCannotBeConvertedToLongShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var color = builder.EnumType<ValueOutOfRangeEnum>();
            color.Member(ValueOutOfRangeEnum.Member);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => builder.GetServiceModel(),
                "value",
                "The value of enum member 'Member' cannot be converted to a long type.");
        }

        [Fact]
        public void RedefiningBaseTypeEnumPropertyShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            builder.EntityType<BaseTypeWithEnumTypePropertyTestModel>()
                .Property(b => b.Color);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => builder.EntityType<DerivedTypeWithEnumTypePropertyTestModel>()
                    .DerivesFrom<BaseTypeWithEnumTypePropertyTestModel>()
                    .Property(d => d.Color),
                "propertyInfo",
                "Cannot redefine property 'Color' already defined on the base type 'Microsoft.AspNet.OData.Test.Builder.BaseTypeWithEnumTypePropertyTestModel'.");
        }

        [Fact]
        public void DefiningEnumPropertyOnBaseTypeAlreadyPresentInDerivedTypeShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            builder.EntityType<DerivedTypeWithEnumTypePropertyTestModel>()
                .DerivesFrom<BaseTypeWithEnumTypePropertyTestModel>()
                .Property(d => d.Color);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => builder.EntityType<BaseTypeWithEnumTypePropertyTestModel>()
                    .Property(b => b.Color),
                "propertyInfo",
                "Cannot define property 'Color' in the base type 'Microsoft.AspNet.OData.Test.Builder.BaseTypeWithEnumTypePropertyTestModel' as the derived type 'Microsoft.AspNet.OData.Test.Builder.DerivedTypeWithEnumTypePropertyTestModel' already defines it.");
        }

        [Fact]
        public void PassNullMemberParameterToEnumMemberConfigurationConstructorShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            builder.EnumType<Color>();
            EnumTypeConfiguration declaringType = builder.EnumTypes.Single();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new EnumMemberConfiguration(null, declaringType),
                "member");
        }

        [Fact]
        public void PassNullDeclaringTypeParameterToEnumMemberConfigurationConstructorShouldThrowException()
        {
            // Arrange
            Enum member = Color.Red;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new EnumMemberConfiguration(member, null),
                "declaringType");
        }

        [Fact]
        public void PassNullToEnumMemberConfigurationNameSetterShouldThrowException()
        {
            // Arrange
            Enum member = Color.Red;
            var builder = new ODataModelBuilder();
            builder.EnumType<Color>();
            EnumTypeConfiguration declaringType = builder.EnumTypes.Single();
            var enumMemberConfiguration = new EnumMemberConfiguration(member, declaringType);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => enumMemberConfiguration.Name = null,
                "value");
        }


        [Fact]
        public void PassNullBuilderParameterToEnumTypeConfigurationConstructorShouldThrowException()
        {
            // Arrange
            Type clrType = typeof(Color);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new EnumTypeConfiguration(null, clrType),
                "builder");
        }

        [Fact]
        public void PassNullClrTypeParameterToEnumTypeConfigurationConstructorShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new EnumTypeConfiguration(builder, null),
                "clrType");
        }

        [Fact]
        public void PassNullToEnumTypeConfigurationNamespaceSetterShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            builder.EnumType<Color>();
            EnumTypeConfiguration enumTypeConfiguration = builder.EnumTypes.Single();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => enumTypeConfiguration.Namespace = null,
                "value");
        }

        [Fact]
        public void PassNullToEnumTypeConfigurationNameSetterShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            builder.EnumType<Color>();
            EnumTypeConfiguration enumTypeConfiguration = builder.EnumTypes.Single();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => enumTypeConfiguration.Name = null,
                "value");
        }

        [Fact]
        public void PassNullToEnumTypeConfigurationAddMemberShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            builder.EnumType<Color>();
            EnumTypeConfiguration enumTypeConfiguration = builder.EnumTypes.Single();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => enumTypeConfiguration.AddMember(null),
                "member");
        }

        [Fact]
        public void AddWrongEnumTypeMemberToConfigurationShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            builder.EnumType<Color>();
            EnumTypeConfiguration enumTypeConfiguration = builder.EnumTypes.Single();

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => enumTypeConfiguration.AddMember(SimpleEnum.First),
                "member",
                "The property 'First' does not belong to the type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Color'.");
        }

        [Fact]
        public void PassNullToEnumTypeConfigurationRemoveMemberShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            builder.EnumType<Color>();
            EnumTypeConfiguration enumTypeConfiguration = builder.EnumTypes.Single();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => enumTypeConfiguration.RemoveMember(null),
                "member");
        }

        [Fact]
        public void RemoveWrongEnumTypeMemberFromConfigurationShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            builder.EnumType<Color>();
            EnumTypeConfiguration enumTypeConfiguration = builder.EnumTypes.Single();

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => enumTypeConfiguration.RemoveMember(SimpleEnum.First),
                "member",
                "The property 'First' does not belong to the type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Color'.");
        }

        [Fact]
        public void PassNullToODataModelBuilderAddEnumTypeShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => builder.AddEnumType(null),
                "type");
        }

        [Fact]
        public void PassNotEnumTypeToODataModelBuilderAddEnumTypeShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            Type type = typeof(Int32);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => builder.AddEnumType(type),
                "type",
                "The type 'System.Int32' cannot be configured as an enum type.");
        }

        [Fact]
        public void PassNullToRemoveEnumTypeShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => builder.RemoveEnumType(null),
                "type");
        }

        [Fact]
        public void PassNullToStructuralTypeConfigurationAddEnumPropertyShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            builder.ComplexType<ComplexTypeWithEnumTypePropertyTestModel>();
            StructuralTypeConfiguration structuralTypeConfiguration = builder.StructuralTypes.Single();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => structuralTypeConfiguration.AddEnumProperty(null),
                "propertyInfo");
        }

        [Fact]
        public void PassNotExistingPropertyoStructuralTypeConfigurationAddEnumPropertyShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            builder.ComplexType<ComplexTypeWithEnumTypePropertyTestModel>();
            StructuralTypeConfiguration structuralTypeConfiguration = builder.StructuralTypes.Single();
            Expression<Func<EntityTypeWithEnumTypePropertyTestModel, Color>> propertyExpression = e => e.RequiredColor;
            PropertyInfo propertyInfo = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => structuralTypeConfiguration.AddEnumProperty(propertyInfo),
                "propertyInfo",
                "The property 'RequiredColor' does not belong to the type 'Microsoft.AspNet.OData.Test.Builder.ComplexTypeWithEnumTypePropertyTestModel'.");
        }

        [Fact]
        public void PassNotEnumPropertyoStructuralTypeConfigurationAddEnumPropertyShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            builder.ComplexType<EntityTypeWithEnumTypePropertyTestModel>();
            StructuralTypeConfiguration structuralTypeConfiguration = builder.StructuralTypes.Single();
            Expression<Func<EntityTypeWithEnumTypePropertyTestModel, int>> propertyExpression = e => e.ID;
            PropertyInfo propertyInfo = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => structuralTypeConfiguration.AddEnumProperty(propertyInfo),
                "propertyInfo",
                "The property 'ID' on type 'Microsoft.AspNet.OData.Test.Builder.EntityTypeWithEnumTypePropertyTestModel' must be an Enum property.");
        }

        [Fact]
        public void ODataModelBuilder_Throws_AddEnumPropertyWithoutEnumType()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var complexTypeConfiguration = builder.ComplexType<ComplexTypeWithEnumTypePropertyTestModel>();
            complexTypeConfiguration.EnumProperty(c => c.RequiredColor);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => builder.GetServiceModel(),
                "The enum type 'Color' does not exist.");
        }

        [Fact]
        public void ODataModelBuilder_Throws_AddCollectionOfEnumPropertyWithoutEnumType()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var complexTypeConfiguration = builder.ComplexType<ComplexTypeWithEnumTypePropertyTestModel>();
            complexTypeConfiguration.CollectionProperty(c => c.Colors);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => builder.GetServiceModel(),
                "The enum type 'Color' does not exist.");
        }

        [Fact]
        public void ODataConventionModelBuilder_CreateRequiredEnumTypePropertyInComplexType()
        {
            // Arrange
            IEdmStructuredType complexType = AddComplexTypeWithODataConventionModelBuilder();

            // Act & Assert
            Assert.Equal(3, complexType.Properties().Count());
            var requiredColor = complexType.Properties().SingleOrDefault(p => p.Name == "RequiredColor");
            Assert.NotNull(requiredColor);
            Assert.True(requiredColor.Type.IsEnum());
            Assert.False(requiredColor.Type.IsNullable);
        }

        [Fact]
        public void ODataConventionModelBuilder_CreateNullableEnumTypePropertyInComplexType()
        {
            // Arrange
            IEdmStructuredType complexType = AddComplexTypeWithODataConventionModelBuilder();

            // Act & Assert
            Assert.Equal(3, complexType.Properties().Count());
            var nullableColor = complexType.Properties().SingleOrDefault(p => p.Name == "NullableColor");
            Assert.NotNull(nullableColor);
            Assert.True(nullableColor.Type.IsEnum());
            Assert.True(nullableColor.Type.IsNullable);
        }

        [Fact]
        public void ODataConventionModelBuilder_CreateEnumTypeCollectionPropertyInComplexType()
        {
            // Arrange
            IEdmStructuredType complexType = AddComplexTypeWithODataConventionModelBuilder();

            // Act & Assert
            Assert.Equal(3, complexType.Properties().Count());
            var colors = complexType.Properties().SingleOrDefault(p => p.Name == "Colors");
            Assert.NotNull(colors);
            Assert.True(colors.Type.IsCollection());
            Assert.True(((IEdmCollectionTypeReference)colors.Type).ElementType().IsEnum());
        }

        [Fact]
        public void ODataConventionModelBuilder_CreateRequiredEnumTypePropertyInEntityType()
        {
            // Arrange
            IEdmEntityType entityType = AddEntityTypeWithODataConventionModelBuilder();

            // Act & Assert
            Assert.Equal(5, entityType.Properties().Count());
            var requiredColor = entityType.Properties().SingleOrDefault(p => p.Name == "RequiredColor");
            Assert.NotNull(requiredColor);
            Assert.True(requiredColor.Type.IsEnum());
            Assert.False(requiredColor.Type.IsNullable);
        }

        [Fact]
        public void ODataConventionModelBuilder_CreateNullableEnumTypePropertyInEntityType()
        {
            // Arrange
            IEdmEntityType entityType = AddEntityTypeWithODataConventionModelBuilder();

            // Act & Assert
            Assert.Equal(5, entityType.Properties().Count());
            var nullableColor = entityType.Properties().SingleOrDefault(p => p.Name == "NullableColor");
            Assert.NotNull(nullableColor);
            Assert.True(nullableColor.Type.IsEnum());
            Assert.True(nullableColor.Type.IsNullable);
        }

        [Fact]
        public void ODataConventionModelBuilder_CreateEnumTypeCollectionPropertyInEntityType()
        {
            // Arrange
            IEdmEntityType entityType = AddEntityTypeWithODataConventionModelBuilder();

            // Act & Assert
            Assert.Equal(5, entityType.Properties().Count());
            var colors = entityType.Properties().SingleOrDefault(p => p.Name == "Colors");
            Assert.NotNull(colors);
            Assert.True(colors.Type.IsCollection());
            Assert.True(((IEdmCollectionTypeReference)colors.Type).ElementType().IsEnum());
        }

        [Fact]
        public void ODataConventionModelBuilder_CreateArrayEnumTypeCollectionPropertyWithInComplexType()
        {
            // Arrange
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<ArrayEnumTypePropertyTestModel>();
            IEdmModel model = builder.GetEdmModel();
            IEdmComplexType complexType = model.SchemaElements.OfType<IEdmComplexType>().Single();

            // Act & Assert
            Assert.Equal(3, complexType.Properties().Count());
            var colors = complexType.Properties().SingleOrDefault(p => p.Name == "Colors");
            Assert.NotNull(colors);
            Assert.True(colors.Type.IsCollection());
            Assert.False(colors.Type.IsNullable);
            Assert.True(((IEdmCollectionTypeReference)colors.Type).ElementType().IsEnum());

            var nullableColors = complexType.Properties().SingleOrDefault(p => p.Name == "NullableColors");
            Assert.NotNull(nullableColors);
            Assert.True(nullableColors.Type.IsCollection());
            Assert.True(nullableColors.Type.IsNullable);
            Assert.True(((IEdmCollectionTypeReference)nullableColors.Type).ElementType().IsEnum());
        }

        [Fact]
        public void ODataConventionModelBuilder_CreateArrayEnumTypeCollectionPropertyWithInEntityType()
        {
            // Arrange
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<ArrayEnumTypePropertyTestModel>();
            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>().Single();

            // Act & Assert
            Assert.Equal(3, entityType.Properties().Count());
            var colors = entityType.Properties().SingleOrDefault(p => p.Name == "Colors");
            Assert.NotNull(colors);
            Assert.True(colors.Type.IsCollection());
            Assert.False(colors.Type.IsNullable);
            Assert.True(((IEdmCollectionTypeReference)colors.Type).ElementType().IsEnum());

            var nullableColors = entityType.Properties().SingleOrDefault(p => p.Name == "NullableColors");
            Assert.NotNull(nullableColors);
            Assert.True(nullableColors.Type.IsCollection());
            Assert.True(nullableColors.Type.IsNullable);
            Assert.True(((IEdmCollectionTypeReference)nullableColors.Type).ElementType().IsEnum());
        }

        [Fact]
        public void ODataConventionModelBuilder_CreateLongEnumTypePropertyInEntityType()
        {
            // Arrange
            IEdmEntityType entityType = AddEntityTypeWithODataConventionModelBuilder();

            // Act & Assert
            Assert.Equal(5, entityType.Properties().Count());
            var longEnum = entityType.Properties().SingleOrDefault(p => p.Name == "LongEnum");
            Assert.NotNull(longEnum);
            Assert.True(longEnum.Type.IsEnum());
            Assert.False(longEnum.Type.IsNullable);
            Assert.Same(EdmCoreModel.Instance.GetInt64(false).Definition, longEnum.Type.AsEnum().EnumDefinition().UnderlyingType);
        }

        [Fact]
        public void GetEdmPrimitiveTypeOrNull_ReturnNull_ForEnumType()
        {
            // Arrange
            Type clrType = typeof(Color);

            // Act & Act
            Assert.Null(EdmLibHelpers.GetEdmPrimitiveTypeOrNull(clrType));
        }

        [Fact]
        public void GetEdmPrimitiveTypeOrNull_ReturnNull_ForNullableEnumType()
        {
            // Arrange
            Type clrType = typeof(Color?);

            // Act & Assert
            Assert.Null(EdmLibHelpers.GetEdmPrimitiveTypeOrNull(clrType));
        }

        [Fact]
        public void UnboundAction_ForEnumTypeInODataModelBuilder()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder().Add_Color_EnumType();
            ActionConfiguration actionConfiguration = builder.Action("UnboundAction");
            actionConfiguration.Parameter<Color>("Color");
            actionConfiguration.Returns<Color>();

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();
            IEdmActionImport actionImport = model.EntityContainer.OperationImports().Single(o => o.Name == "UnboundAction") as IEdmActionImport;

            IEdmTypeReference color = actionImport.Action.Parameters.Single(p => p.Name == "Color").Type;
            IEdmTypeReference returnType = actionImport.Action.ReturnType;
            IEdmEnumType colorType = model.SchemaElements.OfType<IEdmEnumType>().Single(e => e.Name == "Color");

            Assert.False(color.IsNullable);
            Assert.Same(colorType, color.Definition);
            Assert.False(returnType.IsNullable);
            Assert.Same(colorType, returnType.Definition);
        }

        [Fact]
        public void UnboundFunction_ForEnumTypeInODataModelBuilder()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            FunctionConfiguration functionConfiguration = builder.Function("UnboundFunction");
            functionConfiguration.CollectionParameter<Color>("Colors");
            functionConfiguration.ReturnsCollection<Color>();
            builder.Add_Color_EnumType();

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();
            IEdmFunctionImport functionImport = model.EntityContainer.OperationImports().Single(o => o.Name == "UnboundFunction") as IEdmFunctionImport;

            IEdmTypeReference colors = functionImport.Function.Parameters.Single(p => p.Name == "Colors").Type;
            IEdmTypeReference returnType = functionImport.Function.ReturnType;
            IEdmEnumType colorType = model.SchemaElements.OfType<IEdmEnumType>().Single(e => e.Name == "Color");

            Assert.True(colors.IsCollection());
            Assert.Same(colorType, colors.AsCollection().ElementType().Definition);
            Assert.True(returnType.IsCollection());
            Assert.False(returnType.AsCollection().ElementType().IsNullable);
            Assert.Same(colorType, returnType.AsCollection().ElementType().Definition);
        }

        [Fact]
        public void BoundAction_ForEnumTypeInODataModelBuilder()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            EntityTypeConfiguration<EnumModel> entityTypeConfiguration = builder.EntityType<EnumModel>();
            ActionConfiguration actionConfiguration = entityTypeConfiguration.Action("BoundAction");
            actionConfiguration.CollectionParameter<Color>("Colors");
            actionConfiguration.ReturnsCollection<Color?>();
            builder.Add_Color_EnumType();

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();
            IEdmAction action = model.FindDeclaredOperations("Default.BoundAction").Single() as IEdmAction;

            IEdmTypeReference colors = action.Parameters.Single(p => p.Name == "Colors").Type;
            IEdmTypeReference returnType = action.ReturnType;
            IEdmEnumType colorType = model.SchemaElements.OfType<IEdmEnumType>().Single(e => e.Name == "Color");

            Assert.True(colors.IsCollection());
            Assert.Same(colorType, colors.AsCollection().ElementType().Definition);
            Assert.True(returnType.IsCollection());
            Assert.True(returnType.AsCollection().ElementType().IsNullable);
            Assert.Same(colorType, returnType.AsCollection().ElementType().Definition);
        }

        [Fact]
        public void BoundFunction_ForEnumTypeInODataModelBuilder()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>().Add_Color_EnumType();
            EntityTypeConfiguration<EnumModel> entityTypeConfiguration = builder.EntityType<EnumModel>();
            FunctionConfiguration functionConfiguration = entityTypeConfiguration.Function("BoundFunction");
            functionConfiguration.Parameter<Color?>("Color");
            functionConfiguration.Returns<Color>();

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();
            IEdmFunction function = model.FindDeclaredOperations("Default.BoundFunction").Single() as IEdmFunction;

            IEdmTypeReference color = function.Parameters.Single(p => p.Name == "Color").Type;
            IEdmTypeReference returnType = function.ReturnType;
            IEdmEnumType colorType = model.SchemaElements.OfType<IEdmEnumType>().Single(e => e.Name == "Color");

            Assert.True(color.IsNullable);
            Assert.Same(colorType, color.Definition);
            Assert.Same(colorType, returnType.Definition);
        }

        [Fact]
        public void UnboundAction_ForEnumTypeInODataConventionModelBuilder()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            ActionConfiguration actionConfiguration = builder.Action("UnboundAction");
            actionConfiguration.CollectionParameter<Color>("Colors");
            actionConfiguration.Returns<Color?>();

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();
            IEdmActionImport actionImport = model.EntityContainer.OperationImports().Single(o => o.Name == "UnboundAction") as IEdmActionImport;

            IEdmTypeReference colors = actionImport.Action.Parameters.Single(p => p.Name == "Colors").Type;
            IEdmTypeReference returnType = actionImport.Action.ReturnType;
            IEdmEnumType colorType = model.SchemaElements.OfType<IEdmEnumType>().Single(e => e.Name == "Color");

            Assert.True(colors.IsCollection());
            Assert.False(colors.AsCollection().ElementType().IsNullable);
            Assert.Same(colorType, colors.AsCollection().ElementType().Definition);
            Assert.True(returnType.IsNullable);
            Assert.Same(colorType, returnType.Definition);
        }

        [Fact]
        public void UnboundFunction_ForEnumTypeInODataConventionModelBuilder()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            FunctionConfiguration functionConfiguration = builder.Function("UnboundFunction");
            functionConfiguration.Parameter<Color>("Color");
            functionConfiguration.ReturnsCollection<Color>();

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();
            IEdmFunctionImport functionImport = model.EntityContainer.OperationImports().Single(o => o.Name == "UnboundFunction") as IEdmFunctionImport;

            IEdmTypeReference color = functionImport.Function.Parameters.Single(p => p.Name == "Color").Type;
            IEdmTypeReference returnType = functionImport.Function.ReturnType;
            IEdmEnumType colorType = model.SchemaElements.OfType<IEdmEnumType>().Single(e => e.Name == "Color");

            Assert.Same(colorType, color.Definition);
            Assert.True(returnType.IsCollection());
            Assert.Same(colorType, returnType.AsCollection().ElementType().Definition);
        }

        [Fact]
        public void BoundAction_ForEnumTypeInODataConventionModelBuilder()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            EntityTypeConfiguration<EnumModel> entityTypeConfiguration = builder.EntityType<EnumModel>();
            ActionConfiguration actionConfiguration = entityTypeConfiguration.Action("BoundAction");
            actionConfiguration.Parameter<Color>("Color");
            actionConfiguration.ReturnsCollection<Color>();

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();
            IEdmAction action = model.FindDeclaredOperations("Default.BoundAction").Single() as IEdmAction;

            IEdmTypeReference color = action.Parameters.Single(p => p.Name == "Color").Type;
            IEdmTypeReference returnType = action.ReturnType;
            IEdmEnumType colorType = model.SchemaElements.OfType<IEdmEnumType>().Single(e => e.Name == "Color");

            Assert.Same(colorType, color.Definition);
            Assert.True(returnType.IsCollection());
            Assert.Same(colorType, returnType.AsCollection().ElementType().Definition);
        }

        [Fact]
        public void BoundFunction_ForEnumTypeInODataConventionModelBuilder()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            EntityTypeConfiguration<EnumModel> entityTypeConfiguration = builder.EntityType<EnumModel>();
            FunctionConfiguration functionConfiguration = entityTypeConfiguration.Function("BoundFunction");
            functionConfiguration.CollectionParameter<Color?>("Colors");
            functionConfiguration.Returns<Color>();

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();
            IEdmFunction function = model.FindDeclaredOperations("Default.BoundFunction").Single() as IEdmFunction;

            IEdmTypeReference colors = function.Parameters.Single(p => p.Name == "Colors").Type;
            IEdmTypeReference returnType = function.ReturnType;
            IEdmEnumType colorType = model.SchemaElements.OfType<IEdmEnumType>().Single(e => e.Name == "Color");

            Assert.True(colors.IsCollection());
            Assert.True(colors.AsCollection().ElementType().IsNullable);
            Assert.Same(colorType, colors.AsCollection().ElementType().Definition);
            Assert.Same(colorType, returnType.Definition);
        }

        [Fact]
        public void BoundFunction_ForEnumWithLongUnderlyingTypeInODataModelBuilder()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            builder.Add_LongEnum_EnumType();
            EntityTypeConfiguration<EnumModel> entityTypeConfiguration = builder.EntityType<EnumModel>();
            FunctionConfiguration functionConfiguration = entityTypeConfiguration.Function("BoundFunction");
            functionConfiguration.Parameter<LongEnum>("LongEnum");
            functionConfiguration.Returns<int>();

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();
            IEdmFunction function = model.FindDeclaredOperations("Default.BoundFunction").Single() as IEdmFunction;

            IEdmTypeReference longEnumParameter = function.Parameters.Single(p => p.Name == "LongEnum").Type;

            IEdmEnumType longEnumType = model.SchemaElements.OfType<IEdmEnumType>().Single(e => e.Name == "LongEnum");

            Assert.Same(longEnumType, longEnumParameter.Definition);
            Assert.Equal(EdmPrimitiveTypeKind.Int64, longEnumParameter.AsEnum().EnumDefinition().UnderlyingType.PrimitiveKind);
        }

        [Fact]
        public void UnboundAction_ForEnumWithShortUnderlyingTypeInODataConventionModelBuilder()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            ActionConfiguration actionConfiguration = builder.Action("UnboundAction");
            actionConfiguration.Returns<ShortEnum>();

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();
            IEdmAction action = model.FindDeclaredOperations("Default.UnboundAction").Single() as IEdmAction;

            IEdmTypeReference returnType = action.ReturnType;
            IEdmEnumType shortEnumType = model.SchemaElements.OfType<IEdmEnumType>().Single(e => e.Name == "ShortEnum");

            Assert.Same(shortEnumType, returnType.Definition);
            Assert.Equal(EdmPrimitiveTypeKind.Int16, returnType.AsEnum().EnumDefinition().UnderlyingType.PrimitiveKind);
        }

        [Fact]
        public void ODataConventionModelBuilder_HasCorrectEnumMember_AddUnboundFunctionAfterEntitySet()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<EntityTypeWithEnumTypePropertyTestModel>("Entities");
            builder.Function("UnboundFunction").Returns<Color>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEnumerable<IEdmEnumType> colors = model.SchemaElements.OfType<IEdmEnumType>().Where(e => e.Name == "Color");
            IEdmEnumType color = Assert.Single(colors);
            Assert.Equal(3, color.Members.Count());
            Assert.Single(color.Members.Where(m => m.Name == "Red"));
            Assert.Single(color.Members.Where(m => m.Name == "Green"));
            Assert.Single(color.Members.Where(m => m.Name == "Blue"));
        }

        [Fact]
        public void ODataConventionModelBuilder_HasCorrectEnumMember_AddBoundActionAfterEntitySet()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            EntityTypeConfiguration<EntityTypeWithEnumTypePropertyTestModel> entity =
                builder.EntitySet<EntityTypeWithEnumTypePropertyTestModel>("Entities").EntityType;
            entity.Action("BoundAction").Parameter<Color?>("Color");
            builder.EnumTypes.Single(e => e.Name == "Color").RemoveMember(Color.Green);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEnumerable<IEdmEnumType> colors = model.SchemaElements.OfType<IEdmEnumType>().Where(e => e.Name == "Color");
            IEdmEnumType color = Assert.Single(colors);
            Assert.Equal(2, color.Members.Count());
            Assert.Single(color.Members.Where(m => m.Name == "Red"));
            Assert.Single(color.Members.Where(m => m.Name == "Blue"));
        }

        [Fact]
        public void ODataConventionModelBuilder_DataContractAttribute_WorksOnEnumType()
        {
            // Arrange
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EnumType<Life>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEnumType enumType = model.SchemaElements.OfType<IEdmEnumType>().Single();
            Assert.NotNull(enumType);
            Assert.Equal(4, enumType.Members.Count());
            Assert.Equal("Feelings", enumType.Name);
            Assert.Equal("Test", enumType.Namespace);
            Assert.Contains(enumType.Members, (m) => m.Name.Equals("happy"));
            Assert.Contains(enumType.Members, (m) => m.Name.Equals("sad"));
            Assert.Contains(enumType.Members, (m) => m.Name.Equals("KeepDefaultName"));
            Assert.Contains(enumType.Members, (m) => m.Name.Equals("soso"));

            var annotation = model.GetClrEnumMemberAnnotation(enumType);
            Assert.NotNull(annotation);

            IEdmEnumMember enumMember = enumType.Members.Single(m => m.Name.Equals("soso"));
            Assert.Same(enumMember, annotation.GetEdmEnumMember(Life.NotTooBad));
            Assert.Equal(Life.NotTooBad, annotation.GetClrEnumMember(enumMember));
        }

        [Fact]
        public void ODataConventionModelBuilder_DataContractAttribute_AllowsReferencingSameEnumTwice()
        {
            // Arrange
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EnumType<Life>();
        
            // Act
            builder.EnumType<Life>();
            IEdmModel model = builder.GetEdmModel();
        }

        [Fact]
        public void ODataConventionModelBuilder_DataContractAttribute_WithAddedExplicitlyMember()
        {
            // Arrange
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EnumType<Life>();
            builder.EnumTypes.Single(e => e.Name == "Feelings").AddMember(Life.JustSoSo);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEnumType enumType = model.SchemaElements.OfType<IEdmEnumType>().Single();
            Assert.NotNull(enumType);
            Assert.Equal(5, enumType.Members.Count());
            Assert.Equal("Feelings", enumType.Name);
            Assert.Equal("Test", enumType.Namespace);
            Assert.Contains(enumType.Members, (m) => m.Name.Equals("happy"));
            Assert.Contains(enumType.Members, (m) => m.Name.Equals("sad"));
            Assert.Contains(enumType.Members, (m) => m.Name.Equals("JustSoSo"));
            Assert.Contains(enumType.Members, (m) => m.Name.Equals("KeepDefaultName"));
            Assert.Contains(enumType.Members, (m) => m.Name.Equals("soso"));
        }

        /// <summary>
        /// Tests the namespace assignment logic to ensure that user assigned namespaces are honored during registration.
        /// </summary>
        [Fact]
        public void NamespaceAssignment_AutoAssignsNamespaceToEnumType_AssignedNamespace()
        {
            // Arrange and Act.
            string expectedNamespace = "TestingNamespace";
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder()
            {
                Namespace = expectedNamespace
            };

            // Assert
            Assert.Equal(expectedNamespace, modelBuilder.EnumType<ValueOutOfRangeEnum>().Namespace);
            Assert.Equal("Test", modelBuilder.EnumType<Life>().Namespace);
        }

        /// <summary>
        /// Tests the namespace assignment logic to ensure that user assigned namespaces are honored during registration.
        /// </summary>
        [Fact]
        public void NamespaceAssignment_AutoAssignsNamespaceToEnumType_DefaultNamespace()
        {
            // Arrange and Act.
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();

            // Assert
            Assert.Equal(typeof(Life).Namespace, modelBuilder.EnumType<ValueOutOfRangeEnum>().Namespace);
            Assert.Equal("Test", modelBuilder.EnumType<Life>().Namespace);
        }

        /// <summary>
        /// Tests the full name property getter logic with an empty namespace to ensure the full name doesn't begin with a period.
        /// </summary>
        [Fact]
        public void NamespaceAssignment_WithEmptyNamespace_FullNameDoesNotBeginWithPeriod()
        {
            // Arrange and Act.
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();

            // Assert
            Assert.Equal("EnumWithEmptyNamespace", modelBuilder.EnumType<EnumWithEmptyNamespace>().FullName);
        }

        private IEdmStructuredType AddComplexTypeWithODataConventionModelBuilder()
        {
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<ComplexTypeWithEnumTypePropertyTestModel>();
            IEdmModel model = builder.GetEdmModel();
            return model.SchemaElements.OfType<IEdmStructuredType>().Single();
        }

        private IEdmEntityType AddEntityTypeWithODataConventionModelBuilder()
        {
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<EntityTypeWithEnumTypePropertyTestModel>("Entities");
            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Entities");
            return entitySet.EntityType();
        }
    }

    public class ArrayEnumTypePropertyTestModel
    {
        public int Id { get; set; }

        public Color[] Colors { get; set; }

        public Color?[] NullableColors { get; set; }
    }

    public class ComplexTypeWithEnumTypePropertyTestModel
    {
        public Color RequiredColor { get; set; }
        public Color? NullableColor { get; set; }
        public List<Color> Colors { get; set; }
    }

    public class EntityTypeWithEnumTypePropertyTestModel
    {
        public int ID { get; set; }
        public Color RequiredColor { get; set; }
        public LongEnum LongEnum { get; set; }
        public Color? NullableColor { get; set; }
        public List<Color> Colors { get; set; }
    }

    public abstract class BaseTypeWithEnumTypePropertyTestModel
    {
        public abstract int ID { get; set; }
        public abstract Color Color { get; set; }
    }

    public class DerivedTypeWithEnumTypePropertyTestModel : BaseTypeWithEnumTypePropertyTestModel
    {
        public override int ID { get; set; }
        public override Color Color { get; set; }
    }

    public enum ValueOutOfRangeEnum : ulong
    {
        Member = ulong.MaxValue
    }

    [DataContract(Name = "Feelings", Namespace = "Test")]
    public enum Life
    {
        [EnumMember(Value = "happy")]
        Happy = 1,

        [EnumMember(Value = "sad")]
        Sad = 2,

        JustSoSo = 3,

        [EnumMember]
        KeepDefaultName = 4,

        [EnumMember(Value = "soso")]
        NotTooBad
    }

    [DataContract(Namespace = "")]
    public enum EnumWithEmptyNamespace
    {
    }
}