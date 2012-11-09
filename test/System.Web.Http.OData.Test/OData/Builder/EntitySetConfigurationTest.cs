// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.OData.Builder
{
    public class EntitySetConfigurationTest
    {
        [Fact]
        public void CtorThatTakesClrType_Throws_ArgumentNull_For_ModelBuilder()
        {
            Assert.ThrowsArgumentNull(
                () => new EntitySetConfiguration(modelBuilder: null, entityType: typeof(EntitySetConfigurationTest), name: "entityset"),
                "modelBuilder");
        }

        [Fact]
        public void CtorThatTakesClrType_Throws_ArgumentNull_For_EntityType()
        {
            Assert.ThrowsArgumentNull(
                () => new EntitySetConfiguration(modelBuilder: new ODataModelBuilder(), entityType: (Type)null, name: "entityset"),
                "clrType");
        }

        [Fact]
        public void CtorThatTakesClrType_Throws_ArgumentNull_For_Name()
        {
            Assert.ThrowsArgumentNull(
                () => new EntitySetConfiguration(modelBuilder: new ODataModelBuilder(), entityType: typeof(EntitySetConfigurationTest), name: null),
                "name");
        }

        [Fact]
        public void CtorThatTakesEntityTypeConfiguration_Throws_ArgumentNull_For_ModelBuilder()
        {
            Assert.ThrowsArgumentNull(
                () => new EntitySetConfiguration(
                    modelBuilder: null,
                    entityType: new EntityTypeConfiguration(new ODataModelBuilder(), typeof(EntitySetConfigurationTest)),
                    name: "entityset"),
                "modelBuilder");
        }

        [Fact]
        public void CtorThatTakesEntityTypeConfiguration_Throws_ArgumentNull_For_EntityType()
        {
            Assert.ThrowsArgumentNull(
                () => new EntitySetConfiguration(
                    modelBuilder: new ODataModelBuilder(),
                    entityType: (EntityTypeConfiguration)null,
                    name: "entityset"),
                "entityType");
        }

        [Fact]
        public void CtorThatTakesEntityTypeConfiguration_Throws_ArgumentNull_For_Name()
        {
            Assert.ThrowsArgumentNull(
                () => new EntitySetConfiguration(
                    modelBuilder: new ODataModelBuilder(),
                    entityType: new EntityTypeConfiguration(new ODataModelBuilder(), typeof(EntitySetConfigurationTest)),
                    name: null),
                "name");
        }

        [Fact]
        public void CtorThatTakesEntityTypeConfiguration_Sets_Property_EntityType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration entityType = new EntityTypeConfiguration(new ODataModelBuilder(), typeof(EntitySetConfigurationTest));

            // Act
            EntitySetConfiguration entityset = new EntitySetConfiguration(builder, entityType, "entityset");

            // Assert
            Assert.Equal(entityType, entityset.EntityType);
        }

        [Fact]
        public void CtorThatTakesClrType_Sets_Property_EntityType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            EntitySetConfiguration entityset = new EntitySetConfiguration(builder, typeof(EntitySetConfigurationTest), "entityset");

            // Assert
            Assert.NotNull(entityset.EntityType);
            Assert.Equal(typeof(EntitySetConfigurationTest), entityset.EntityType.ClrType);
        }

        [Fact]
        public void CtorThatTakesEntityTypeConfiguration_Sets_Property_ClrType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration entityType = new EntityTypeConfiguration(new ODataModelBuilder(), typeof(EntitySetConfigurationTest));

            // Act
            EntitySetConfiguration entityset = new EntitySetConfiguration(builder, entityType, "entityset");

            // Assert
            Assert.Equal(typeof(EntitySetConfigurationTest), entityset.ClrType);
        }

        [Fact]
        public void CtorThatTakesClrType_Sets_Property_ClrType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            EntitySetConfiguration entityset = new EntitySetConfiguration(builder, typeof(EntitySetConfigurationTest), "entityset");

            // Assert
            Assert.NotNull(entityset.ClrType);
            Assert.Equal(typeof(EntitySetConfigurationTest), entityset.ClrType);
        }

        [Fact]
        public void CtorThatTakesEntityTypeConfiguration_Sets_Property_Name()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration entityType = new EntityTypeConfiguration(new ODataModelBuilder(), typeof(EntitySetConfigurationTest));

            // Act
            EntitySetConfiguration entityset = new EntitySetConfiguration(builder, entityType, "entityset");

            // Assert
            Assert.Equal("entityset", entityset.Name);
        }

        [Fact]
        public void CtorThatTakesClrType_Sets_Property_Name()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            EntitySetConfiguration entityset = new EntitySetConfiguration(builder, typeof(EntitySetConfigurationTest), "entityset");

            // Assert
            Assert.Equal("entityset", entityset.Name);
        }
    }
}
