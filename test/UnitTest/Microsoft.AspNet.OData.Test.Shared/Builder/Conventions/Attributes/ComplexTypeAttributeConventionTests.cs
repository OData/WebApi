// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Builder.Conventions.Attributes;
using Microsoft.Test.AspNet.OData.Common;
using Moq;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Builder.Conventions.Attributes
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
            Assert.Empty(entity.Keys);
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
            Assert.Single(entity.Keys);
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
