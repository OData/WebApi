// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library.Values;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    public class DatabaseGeneratedAttributeEdmPropertyConventionTest
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            // Assert
            Assert.DoesNotThrow(() => new DatabaseGeneratedAttributeEdmPropertyConvention());
        }

        [Theory]
        [InlineData(DatabaseGeneratedOption.Identity)]
        [InlineData(DatabaseGeneratedOption.Computed)]
        [InlineData(DatabaseGeneratedOption.None)]
        public void Apply_SetsDatabaseGeneratedAttributes(DatabaseGeneratedOption option)
        {
            // Arrange
            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>()))
                .Returns(new[] { new DatabaseGeneratedAttribute(option) });

            Mock<EntityTypeConfiguration> entityType = new Mock<EntityTypeConfiguration>(MockBehavior.Strict);
            Mock<PrimitivePropertyConfiguration> primitiveProperty =
                new Mock<PrimitivePropertyConfiguration>(property.Object, entityType.Object);
            primitiveProperty.Object.AddedExplicitly = false;

            // Act
            new DatabaseGeneratedAttributeEdmPropertyConvention().Apply(primitiveProperty.Object, entityType.Object);

            // Assert
            Assert.Equal(option, primitiveProperty.Object.StoreGeneratedPattern);
        }

        [Fact]
        public void DatabaseGeneratedAttributeEdmPropertyConvention_DoesnotOverwriteExistingConfiguration()
        {
            // Arrange
            MockType type =
                new MockType("Entity")
                .Property(typeof(int), "ID")
                .Property(typeof(int?), "Count", new DatabaseGeneratedAttribute(DatabaseGeneratedOption.Computed));

            // Act
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntity(type).AddProperty(type.GetProperty("Count")).IsOptional();
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType entity = model.AssertHasEntityType(type);
            IEdmStructuralProperty property = entity.AssertHasPrimitiveProperty(model, "Count",
                EdmPrimitiveTypeKind.Int32, isNullable: true);

            var idAnnotation = model.GetAnnotationValue<EdmStringConstant>(
                property,
                StoreGeneratedPatternAnnotation.AnnotationsNamespace,
                "StoreGeneratedPattern");
            Assert.Null(idAnnotation);
        }
    }
}
