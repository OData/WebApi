// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder.Conventions.Attributes
{
    public class ConcurrencyCheckAttributeEdmPropertyConventionTest
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            Assert.DoesNotThrow(() => new ConcurrencyCheckAttributeEdmPropertyConvention());
        }

        [Fact]
        public void Apply_SetsConcurrencyTokenProperty()
        {
            // Arrange
            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(string));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { new ConcurrencyCheckAttribute() });

            Mock<EntityTypeConfiguration> entityType = new Mock<EntityTypeConfiguration>();
            Mock<PrimitivePropertyConfiguration> primitiveProperty = new Mock<PrimitivePropertyConfiguration>(property.Object, entityType.Object);
            primitiveProperty.Object.AddedExplicitly = false;

            // Act
            new ConcurrencyCheckAttributeEdmPropertyConvention().Apply(primitiveProperty.Object, entityType.Object, new ODataConventionModelBuilder());

            // Assert
            Assert.True(primitiveProperty.Object.ConcurrencyToken);
        }

        [Fact]
        public void ConcurrencyCheckAttributeEdmPropertyConvention_ConfiguresETagPropertyAsETag()
        {
            // Arrange
            MockType type =
                new MockType("Entity")
                .Property(typeof(int), "ID")
                .Property(typeof(int?), "Count", new ConcurrencyCheckAttribute());

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntityType(type);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType entity = model.AssertHasEntityType(type);
            IEdmStructuralProperty property = entity.AssertHasPrimitiveProperty(
                model,
                "Count",
                EdmPrimitiveTypeKind.Int32,
                isNullable: true);
            Assert.Equal(EdmConcurrencyMode.Fixed, property.ConcurrencyMode); 
        }

        [Fact]
        public void ConcurrencyCheckAttributeEdmPropertyConvention_DoesnotOverwriteExistingConfiguration()
        {
            MockType type =
                new MockType("Entity")
                .Property(typeof(int), "ID")
                .Property(typeof(int), "Count", new ConcurrencyCheckAttribute());

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntityType(type).AddProperty(type.GetProperty("Count")).IsOptional();

            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType entity = model.AssertHasEntityType(type);
            IEdmStructuralProperty property = entity.AssertHasPrimitiveProperty(
                model,
                "Count",
                EdmPrimitiveTypeKind.Int32,
                isNullable: true);
            Assert.Equal(EdmConcurrencyMode.Fixed, property.ConcurrencyMode);
        }
    }
}
