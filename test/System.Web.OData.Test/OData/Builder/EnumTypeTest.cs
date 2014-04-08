// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Formatter;
using System.Web.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;

namespace System.Web.OData.Builder
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
            Assert.Equal(1, complexType.Properties().Count());
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
            Assert.Equal(1, complexType.Properties().Count());
            var colors = complexType.Properties().SingleOrDefault(p => p.Name == "Colors");
            Assert.NotNull(colors);
            Assert.True(colors.Type.IsCollection());
            Assert.True(((IEdmCollectionTypeReference)colors.Type).ElementType().IsEnum());
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
            Assert.Equal(1, complexType.Properties().Count());
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
        public void AddAndRemoveEnumMemberFromEnumType()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var color = builder.EnumType<Color>();

            // Act & Assert
            Assert.Equal(0, color.Members.Count());

            color.Member(Color.Red);
            color.Member(Color.Green);
            Assert.Equal(2, color.Members.Count());

            color.RemoveMember(Color.Red);
            Assert.Equal(1, color.Members.Count());
        }

        [Fact]
        public void EnumPropertyLimitation()
        {
            // Arrange
            var builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>().Add_Color_EnumType();
            var entityTypeConfiguration = builder.EntityType<EntityTypeWithEnumTypePropertyTestModel>();
            entityTypeConfiguration.EnumProperty(c => c.RequiredColor).IsOptional().IsConcurrencyToken();

            // Act
            var model = builder.GetEdmModel();
            var complexType = model.SchemaElements.OfType<IEdmStructuredType>().Single();
            IEdmStructuralProperty requiredColor = complexType.Properties().SingleOrDefault(p => p.Name == "RequiredColor") as IEdmStructuralProperty;

            // Assert
            Assert.NotNull(requiredColor);
            Assert.True(requiredColor.Type.IsNullable);
            Assert.Equal(EdmConcurrencyMode.Fixed, requiredColor.ConcurrencyMode);
        }

        [Fact]
        public void TypeParameterOfEnumTypeIsNotEnumShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();

            // Act & Assert
            Assert.ThrowsArgument(
                () => builder.EnumType<ComplexTypeWithEnumTypePropertyTestModel>(),
                "type",
                "The type 'System.Web.OData.Builder.ComplexTypeWithEnumTypePropertyTestModel' cannot be configured as an enum type.");
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
            Assert.ThrowsArgument(
                () => entityTypeConfiguration.EnumProperty(e => e.ID),
                "propertyInfo",
                "The property 'ID' on type 'System.Web.OData.Builder.EntityTypeWithEnumTypePropertyTestModel' must be an Enum property.");
        }

        [Fact]
        public void ValueOfEnumMemberCannotBeConvertedToLongShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var color = builder.EnumType<ValueOutOfRangeEnum>();
            color.Member(ValueOutOfRangeEnum.Member);

            // Act & Assert
            Assert.ThrowsArgument(
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
            Assert.ThrowsArgument(
                () => builder.EntityType<DerivedTypeWithEnumTypePropertyTestModel>()
                    .DerivesFrom<BaseTypeWithEnumTypePropertyTestModel>()
                    .Property(d => d.Color),
                "propertyInfo",
                "Cannot redefine property 'Color' already defined on the base type 'System.Web.OData.Builder.BaseTypeWithEnumTypePropertyTestModel'.");
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
            Assert.ThrowsArgument(
                () => builder.EntityType<BaseTypeWithEnumTypePropertyTestModel>()
                    .Property(b => b.Color),
                "propertyInfo",
                "Cannot define property 'Color' in the base entity type 'System.Web.OData.Builder.BaseTypeWithEnumTypePropertyTestModel' as the derived type 'System.Web.OData.Builder.DerivedTypeWithEnumTypePropertyTestModel' already defines it.");
        }

        [Fact]
        public void PassNullMemberParameterToEnumMemberConfigurationConstructorShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            builder.EnumType<Color>();
            EnumTypeConfiguration declaringType = builder.EnumTypes.Single();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new EnumMemberConfiguration(null, declaringType),
                "member");
        }

        [Fact]
        public void PassNullDeclaringTypeParameterToEnumMemberConfigurationConstructorShouldThrowException()
        {
            // Arrange
            Enum member = Color.Red;

            // Act & Assert
            Assert.ThrowsArgumentNull(
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
            Assert.ThrowsArgumentNull(
                () => enumMemberConfiguration.Name = null,
                "value");
        }


        [Fact]
        public void PassNullBuilderParameterToEnumTypeConfigurationConstructorShouldThrowException()
        {
            // Arrange
            Type clrType = typeof(Color);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new EnumTypeConfiguration(null, clrType),
                "builder");
        }

        [Fact]
        public void PassNullClrTypeParameterToEnumTypeConfigurationConstructorShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();

            // Act & Assert
            Assert.ThrowsArgumentNull(
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
            Assert.ThrowsArgumentNull(
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
            Assert.ThrowsArgumentNull(
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
            Assert.ThrowsArgumentNull(
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
            Assert.ThrowsArgument(
                () => enumTypeConfiguration.AddMember(SimpleEnum.First),
                "member",
                "The property 'First' does not belong to the type 'System.Web.OData.Builder.TestModels.Color'.");
        }

        [Fact]
        public void PassNullToEnumTypeConfigurationRemoveMemberShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            builder.EnumType<Color>();
            EnumTypeConfiguration enumTypeConfiguration = builder.EnumTypes.Single();

            // Act & Assert
            Assert.ThrowsArgumentNull(
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
            Assert.ThrowsArgument(
                () => enumTypeConfiguration.RemoveMember(SimpleEnum.First),
                "member",
                "The property 'First' does not belong to the type 'System.Web.OData.Builder.TestModels.Color'.");
        }

        [Fact]
        public void PassNullToODataModelBuilderAddEnumTypeShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();

            // Act & Assert
            Assert.ThrowsArgumentNull(
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
            Assert.ThrowsArgument(
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
            Assert.ThrowsArgumentNull(
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
            Assert.ThrowsArgumentNull(
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
            Assert.ThrowsArgument(
                () => structuralTypeConfiguration.AddEnumProperty(propertyInfo),
                "propertyInfo",
                "The property 'RequiredColor' does not belong to the type 'System.Web.OData.Builder.ComplexTypeWithEnumTypePropertyTestModel'.");
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
            Assert.ThrowsArgument(
                () => structuralTypeConfiguration.AddEnumProperty(propertyInfo),
                "propertyInfo",
                "The property 'ID' on type 'System.Web.OData.Builder.EntityTypeWithEnumTypePropertyTestModel' must be an Enum property.");
        }

        [Fact]
        public void ODataModelBuilder_Throws_AddEnumPropertyWithoutEnumType()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var complexTypeConfiguration = builder.ComplexType<ComplexTypeWithEnumTypePropertyTestModel>();
            complexTypeConfiguration.EnumProperty(c => c.RequiredColor);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
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
            Assert.Throws<InvalidOperationException>(
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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

        private IEdmStructuredType AddComplexTypeWithODataConventionModelBuilder()
        {
            var builder = new ODataConventionModelBuilder();
            builder.ComplexType<ComplexTypeWithEnumTypePropertyTestModel>();
            IEdmModel model = builder.GetEdmModel();
            return model.SchemaElements.OfType<IEdmStructuredType>().Single();
        }

        private IEdmEntityType AddEntityTypeWithODataConventionModelBuilder()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<EntityTypeWithEnumTypePropertyTestModel>("Entities");
            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Entities");
            return entitySet.EntityType();
        }
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
}