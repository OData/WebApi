// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Builder
{
    public class EntitySetConfigurationTest
    {
        private ODataModelBuilder _builder;
        private EntitySetConfiguration _entityset;

        public EntitySetConfigurationTest()
        {
            _builder = new ODataModelBuilder();
            _entityset = new EntitySetConfiguration(_builder, typeof(EntitySetConfigurationTest), "entityset");
        }

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

        [Fact]
        public void HasIdLink_RoundTrips()
        {
            // Arrange
            SelfLinkBuilder<string> idLinkBuilder = new SelfLinkBuilder<string>((ctxt) => null, followsConventions: true);

            // Act
            _entityset.HasIdLink(idLinkBuilder);

            // Assert
            Assert.Equal(idLinkBuilder, _entityset.GetIdLink());
        }

        [Fact]
        public void HasEditLink_RoundTrips()
        {
            // Arrange
            SelfLinkBuilder<Uri> editLinkBuilder = new SelfLinkBuilder<Uri>((ctxt) => null, followsConventions: true);

            // Act
            _entityset.HasEditLink(editLinkBuilder);

            // Assert
            Assert.Equal(editLinkBuilder, _entityset.GetEditLink());
        }

        [Fact]
        public void HasReadLink_RoundTrips()
        {
            // Arrange
            SelfLinkBuilder<Uri> readLinkBuilder = new SelfLinkBuilder<Uri>((ctxt) => null, followsConventions: true);

            // Act
            _entityset.HasReadLink(readLinkBuilder);

            // Assert
            Assert.Equal(readLinkBuilder, _entityset.GetReadLink());
        }

        [Fact]
        public void HasNavigationPropertyLink_CanReplaceExistingLinks()
        {
            // Arrange
            var entity = _builder.AddEntity(typeof(Motorcycle));
            var navigationProperty = entity.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.One);
            var entityset = _builder.AddEntitySet("vehicles", entity);
            Uri link1 = new Uri("http://link1");
            Uri link2 = new Uri("http://link2");
            entityset.HasNavigationPropertyLink(navigationProperty, new NavigationLinkBuilder((entityContext, property) => link1, followsConventions: true));

            // Act
            entityset.HasNavigationPropertyLink(navigationProperty, new NavigationLinkBuilder((entityContext, property) => link2, followsConventions: false));

            // Assert
            var navigationLink = entityset.GetNavigationPropertyLink(navigationProperty);
            Assert.False(navigationLink.FollowsConventions);
            Assert.Equal(link2, navigationLink.Factory(null, null));
        }
    }
}
