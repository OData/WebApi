// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder.Conventions.Attributes
{
    public class ComplexTypeAttributeConventionTests
    {
        [Fact]
        public void ComplexTypeConvention_RemovesAnyKeyFromEntityConfiguration()
        {
            // Arrange
            EntityTypeConfiguration entity = CreateEntitytypeConfigurationMock();
            entity.HasKey(new MockPropertyInfo(typeof(int), "Id"));
            ComplexTypeAttributeConvention convention = new ComplexTypeAttributeConvention();

            // Act
            convention.Apply(entity, null, null);

            // Assert
            Assert.Equal(0, entity.Keys.Count());
        }

        [Fact]
        public void ComplexTypeConvention_DoesntApplyToExplicitlyDefinedTypes()
        {
            // Arrange
            EntityTypeConfiguration entity = CreateEntitytypeConfigurationMock();
            entity.AddedExplicitly = true;
            entity.HasKey(new MockPropertyInfo(typeof(int), "Id"));
            ComplexTypeAttributeConvention convention = new ComplexTypeAttributeConvention();

            // Act
            convention.Apply(entity, null, null);

            // Assert
            Assert.Equal(1, entity.Keys.Count());
        }

        private static EntityTypeConfiguration CreateEntitytypeConfigurationMock()
        {
            Mock<EntityTypeConfiguration> entity = new Mock<EntityTypeConfiguration>();
            entity.Setup(e => e.ModelBuilder).Returns(new ODataModelBuilder());
            entity.Setup(e => e.ClrType).Returns(typeof(object));
            entity.CallBase = true;
            return entity.Object;
        }
    }
}
