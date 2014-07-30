// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    public class RequiredAttributeEdmPropertyConventionTests
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            Assert.DoesNotThrow(() => new RequiredAttributeEdmPropertyConvention());
        }

        [Fact]
        public void Apply_SetsOptionalProperty()
        {
            // Arrange
            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(string));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { new RequiredAttribute() });

            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>();
            Mock<StructuralPropertyConfiguration> structuralProperty = new Mock<StructuralPropertyConfiguration>(property.Object, structuralType.Object);
            structuralProperty.Object.AddedExplicitly = false;

            // Act
            new RequiredAttributeEdmPropertyConvention().Apply(structuralProperty.Object, structuralType.Object);

            // Assert
            Assert.False(structuralProperty.Object.OptionalProperty);
        }

        [Fact]
        public void RequiredAttributeEdmPropertyConvention_ConfiguresRequiredPropertyAsRequired()
        {
            MockType type =
                new MockType("Entity")
                .Property(typeof(int), "ID")
                .Property(typeof(int?), "Count", new RequiredAttribute());

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntity(type);

            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType entity = model.AssertHasEntityType(type);
            entity.AssertHasPrimitiveProperty(model, "Count", EdmPrimitiveTypeKind.Int32, isNullable: false);
        }

        [Fact]
        public void RequiredAttributeEdmPropertyConvention_ConfiguresRequiredNavigationPropertyAsRequired()
        {
            MockType anotherType =
                new MockType("RelatedEntity")
                .Property<int>("ID");

            MockType type =
                new MockType("Entity")
                .Property(typeof(int), "ID")
                .Property(anotherType, "RelatedEntity", new RequiredAttribute());

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntity(type);

            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType entity = model.AssertHasEntityType(type);
            entity.AssertHasNavigationProperty(model, "RelatedEntity", anotherType, isNullable: false, multiplicity: EdmMultiplicity.One);
        }

        [Fact]
        public void RequiredAttributeEdmPropertyConvention_DoesnotOverwriteExistingConfiguration()
        {
            MockType type =
                new MockType("Entity")
                .Property(typeof(int), "ID")
                .Property(typeof(int), "Count", new RequiredAttribute());

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntity(type).AddProperty(type.GetProperty("Count")).IsOptional();

            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType entity = model.AssertHasEntityType(type);
            entity.AssertHasPrimitiveProperty(model, "Count", EdmPrimitiveTypeKind.Int32, isNullable: true);
        }

        [Fact]
        public void RequiredAttributeEdmPropertyConvention_DoesnotOverwriteExistingConfigurationForNavigationProperties()
        {
            MockType anotherType =
                new MockType("RelatedEntity")
                .Property<int>("ID");

            MockType type =
                new MockType("Entity")
                .Property(typeof(int), "ID")
                .Property(anotherType, "RelatedEntity", new RequiredAttribute());

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntity(type).AddNavigationProperty(type.GetProperty("RelatedEntity"), EdmMultiplicity.ZeroOrOne);

            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType entity = model.AssertHasEntityType(type);
            entity.AssertHasNavigationProperty(model, "RelatedEntity", anotherType, isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);
        }
    }
}
