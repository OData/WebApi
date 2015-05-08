﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    public class ConcurrencyCheckAttributeEdmPropertyConventionTest
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            // Assert
            Assert.DoesNotThrow(() => new ConcurrencyCheckAttributeEdmPropertyConvention());
        }

        [Fact]
        public void Apply_SetsConcurrencyTokenProperty()
        {
            // Arrange
            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { new ConcurrencyCheckAttribute() });

            Mock<EntityTypeConfiguration> entityType = new Mock<EntityTypeConfiguration>(MockBehavior.Strict);
            Mock<PrimitivePropertyConfiguration> primitiveProperty = new Mock<PrimitivePropertyConfiguration>(
                property.Object, entityType.Object);
            primitiveProperty.Object.AddedExplicitly = false;

            // Act
            new ConcurrencyCheckAttributeEdmPropertyConvention().Apply(primitiveProperty.Object, entityType.Object);

            // Assert
            Assert.True(primitiveProperty.Object.ConcurrencyToken);
        }

        [Fact]
        public void ConcurrencyCheckAttributeEdmPropertyConvention_ConfiguresETagPropertyAsETag()
        {
            // Arrange
            MockType type = new MockType("Entity")
                .Property(typeof(int), "ID")
                .Property(typeof(int?), "Count", new ConcurrencyCheckAttribute());

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntity(type);

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
            // Arrange
            MockType type = new MockType("Entity")
                .Property(typeof(int), "ID")
                .Property(typeof(int?), "Count", new ConcurrencyCheckAttribute());

            // Act
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntity(type).AddProperty(type.GetProperty("Count")).IsOptional();
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType entity = model.AssertHasEntityType(type);
            IEdmStructuralProperty property = entity.AssertHasPrimitiveProperty(
                model,
                "Count",
                EdmPrimitiveTypeKind.Int32,
                isNullable: true);
            Assert.Equal(EdmConcurrencyMode.None, property.ConcurrencyMode);
        }
    }
}
