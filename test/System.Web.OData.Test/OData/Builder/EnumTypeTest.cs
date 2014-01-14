// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;

namespace System.Web.Http.OData.Builder
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
            var model = builder.GetServiceModel();
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
            var model = builder.GetServiceModel();
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
            var model = builder.GetServiceModel();
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
            var model = builder.GetServiceModel();
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
            var model = builder.GetServiceModel();
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
            var entityTypeConfiguration = builder.Entity<EntityTypeWithEnumTypePropertyTestModel>();
            entityTypeConfiguration.HasKey(e => e.ID);
            entityTypeConfiguration.EnumProperty(e => e.Color);
            entityTypeConfiguration.EnumProperty(e => e.LongEnum);

            // Act
            var model = builder.GetServiceModel();
            var entityType = model.SchemaElements.OfType<IEdmStructuredType>().Single();

            // Assert
            Assert.Equal(3, entityType.Properties().Count());
            var color = entityType.Properties().SingleOrDefault(p => p.Name == "Color");
            var longEnum = entityType.Properties().SingleOrDefault(p => p.Name == "LongEnum");
            Assert.NotNull(color);
            Assert.NotNull(longEnum);
            Assert.True(color.Type.IsEnum());
            Assert.True(longEnum.Type.IsEnum());
            Assert.True(EdmCoreModel.Instance.GetInt32(false).Definition == color.Type.AsEnum().EnumDefinition().UnderlyingType);
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
            var builder = new ODataModelBuilder().Add_Color_EnumType();
            var entityTypeConfiguration = builder.Entity<EntityTypeWithEnumTypePropertyTestModel>();
            entityTypeConfiguration.EnumProperty(c => c.Color).IsOptional().IsConcurrencyToken();

            // Act
            var model = builder.GetServiceModel();
            var complexType = model.SchemaElements.OfType<IEdmStructuredType>().Single();
            IEdmStructuralProperty color = complexType.Properties().SingleOrDefault(p => p.Name == "Color") as IEdmStructuralProperty;

            // Assert
            Assert.NotNull(color);
            Assert.True(color.Type.IsNullable);
            Assert.Equal(EdmConcurrencyMode.Fixed, color.ConcurrencyMode);
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
                "The type 'System.Web.Http.OData.Builder.ComplexTypeWithEnumTypePropertyTestModel' cannot be configured as an enum type.");
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
                "The property 'ID' on type 'System.Web.Http.OData.Builder.EntityTypeWithEnumTypePropertyTestModel' must be an Enum property.");
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
            builder.Entity<BaseTypeWithEnumTypePropertyTestModel>()
                .Property(b => b.Color);

            // Act & Assert
            Assert.ThrowsArgument(
                () => builder.Entity<DerivedTypeWithEnumTypePropertyTestModel>()
                    .DerivesFrom<BaseTypeWithEnumTypePropertyTestModel>()
                    .Property(d => d.Color),
                "propertyInfo",
                "Cannot redefine property 'Color' already defined on the base type 'System.Web.Http.OData.Builder.BaseTypeWithEnumTypePropertyTestModel'.");
        }

        [Fact]
        public void DefiningEnumPropertyOnBaseTypeAlreadyPresentInDerivedTypeShouldThrowException()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            builder.Entity<DerivedTypeWithEnumTypePropertyTestModel>()
                .DerivesFrom<BaseTypeWithEnumTypePropertyTestModel>()
                .Property(d => d.Color);

            // Act & Assert
            Assert.ThrowsArgument(
                () => builder.Entity<BaseTypeWithEnumTypePropertyTestModel>()
                    .Property(b => b.Color),
                "propertyInfo",
                "Cannot define property 'Color' in the base entity type 'System.Web.Http.OData.Builder.BaseTypeWithEnumTypePropertyTestModel' as the derived type 'System.Web.Http.OData.Builder.DerivedTypeWithEnumTypePropertyTestModel' already defines it.");
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
                "The property 'First' does not belong to the type 'System.Web.Http.OData.Builder.TestModels.Color'.");
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
                "The property 'First' does not belong to the type 'System.Web.Http.OData.Builder.TestModels.Color'.");
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
            Expression<Func<EntityTypeWithEnumTypePropertyTestModel, Color>> propertyExpression = e => e.Color;
            PropertyInfo propertyInfo = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);

            // Act & Assert
            Assert.ThrowsArgument(
                () => structuralTypeConfiguration.AddEnumProperty(propertyInfo),
                "propertyInfo",
                "The property 'Color' does not belong to the type 'System.Web.Http.OData.Builder.ComplexTypeWithEnumTypePropertyTestModel'.");
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
                "The property 'ID' on type 'System.Web.Http.OData.Builder.EntityTypeWithEnumTypePropertyTestModel' must be an Enum property.");
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
        public Color Color { get; set; }
        public LongEnum LongEnum { get; set; }
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