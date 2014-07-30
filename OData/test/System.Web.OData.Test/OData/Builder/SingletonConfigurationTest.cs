// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.OData.Builder.TestModels;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Builder
{
    public class SingletonConfigurationTest
    {
        private ODataModelBuilder _builder;
        private SingletonConfiguration _singleton;

        public SingletonConfigurationTest()
        {
            _builder = new ODataModelBuilder();
            _singleton = new SingletonConfiguration(_builder, typeof(SingletonConfigurationTest), "singleton");
        }

        [Fact]
        public void CtorThatTakesClrType_Throws_ArgumentNull_For_ModelBuilder()
        {
            Assert.ThrowsArgumentNull(
                () => new SingletonConfiguration(modelBuilder: null, entityClrType: typeof(SingletonConfigurationTest), name: "singleton"),
                "modelBuilder");
        }

        [Fact]
        public void CtorThatTakesClrType_Throws_ArgumentNull_For_EntityType()
        {
            Assert.ThrowsArgumentNull(
                () => new SingletonConfiguration(modelBuilder: new ODataModelBuilder(), entityClrType: (Type)null, name: "singleton"),
                "clrType");
        }

        [Fact]
        public void CtorThatTakesClrType_Throws_ArgumentNull_For_Name()
        {
            Assert.Throws<ArgumentException>(
                () => new SingletonConfiguration(modelBuilder: new ODataModelBuilder(), entityClrType: typeof(SingletonConfigurationTest), name: null),
                "The argument 'name' is null or empty.\r\nParameter name: name");
        }

        [Fact]
        public void CtorThatTakesEntityTypeConfiguration_Throws_ArgumentNull_For_ModelBuilder()
        {
            Assert.ThrowsArgumentNull(
                () => new SingletonConfiguration(
                    modelBuilder: null,
                    entityType: new EntityTypeConfiguration(new ODataModelBuilder(), typeof(SingletonConfigurationTest)),
                    name: "singleton"),
                "modelBuilder");
        }

        [Fact]
        public void CtorThatTakesEntityTypeConfiguration_Throws_ArgumentNull_For_EntityType()
        {
            Assert.ThrowsArgumentNull(
                () => new SingletonConfiguration(
                    modelBuilder: new ODataModelBuilder(),
                    entityType: (EntityTypeConfiguration)null,
                    name: "singleton"),
                "entityType");
        }

        [Fact]
        public void CtorThatTakesEntityTypeConfiguration_Throws_ArgumentNull_For_Name()
        {
            Assert.Throws<ArgumentException>(
                () => new SingletonConfiguration(
                    modelBuilder: new ODataModelBuilder(),
                    entityType: new EntityTypeConfiguration(new ODataModelBuilder(), typeof(SingletonConfigurationTest)),
                    name: null),
                    "The argument 'name' is null or empty.\r\nParameter name: name");
        }

        [Fact]
        public void CtorThatTakesEntityTypeConfiguration_Sets_Property_EntityType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration entityType = new EntityTypeConfiguration(new ODataModelBuilder(), typeof(SingletonConfigurationTest));

            // Act
            SingletonConfiguration singleton = new SingletonConfiguration(builder, entityType, "singleton");

            // Assert
            Assert.Equal(entityType, singleton.EntityType);
        }

        [Fact]
        public void CtorThatTakesClrType_Sets_Property_EntityType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            SingletonConfiguration singleton = new SingletonConfiguration(builder, typeof(SingletonConfigurationTest), "singleton");

            // Assert
            Assert.NotNull(singleton.EntityType);
            Assert.Equal(typeof(SingletonConfigurationTest), singleton.EntityType.ClrType);
        }

        [Fact]
        public void CtorThatTakesEntityTypeConfiguration_Sets_Property_ClrType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration entityType = new EntityTypeConfiguration(new ODataModelBuilder(), typeof(SingletonConfigurationTest));

            // Act
            SingletonConfiguration singleton = new SingletonConfiguration(builder, entityType, "singleton");

            // Assert
            Assert.Equal(typeof(SingletonConfigurationTest), singleton.ClrType);
        }

        [Fact]
        public void CtorThatTakesClrType_Sets_Property_ClrType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            SingletonConfiguration singleton = new SingletonConfiguration(builder, typeof(SingletonConfigurationTest), "singleton");

            // Assert
            Assert.NotNull(singleton.ClrType);
            Assert.Equal(typeof(SingletonConfigurationTest), singleton.ClrType);
        }

        [Fact]
        public void CtorThatTakesEntityTypeConfiguration_Sets_Property_Name()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration entityType = new EntityTypeConfiguration(new ODataModelBuilder(), typeof(SingletonConfigurationTest));

            // Act
            SingletonConfiguration singleton = new SingletonConfiguration(builder, entityType, "singleton");

            // Assert
            Assert.Equal("singleton", singleton.Name);
        }

        [Fact]
        public void CtorThatTakesClrType_Sets_Property_Name()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            SingletonConfiguration singleton = new SingletonConfiguration(builder, typeof(SingletonConfigurationTest), "singleton");

            // Assert
            Assert.Equal("singleton", singleton.Name);
        }

        [Fact]
        public void HasIdLink_RoundTrips()
        {
            // Arrange
            SelfLinkBuilder<Uri> idLinkBuilder = new SelfLinkBuilder<Uri>((ctxt) => null, followsConventions: true);

            // Act
            _singleton.HasIdLink(idLinkBuilder);

            // Assert
            Assert.Equal(idLinkBuilder, _singleton.GetIdLink());
        }

        [Fact]
        public void HasEditLink_RoundTrips()
        {
            // Arrange
            SelfLinkBuilder<Uri> editLinkBuilder = new SelfLinkBuilder<Uri>((ctxt) => null, followsConventions: true);

            // Act
            _singleton.HasEditLink(editLinkBuilder);

            // Assert
            Assert.Equal(editLinkBuilder, _singleton.GetEditLink());
        }

        [Fact]
        public void HasReadLink_RoundTrips()
        {
            // Arrange
            SelfLinkBuilder<Uri> readLinkBuilder = new SelfLinkBuilder<Uri>((ctxt) => null, followsConventions: true);

            // Act
            _singleton.HasReadLink(readLinkBuilder);

            // Assert
            Assert.Equal(readLinkBuilder, _singleton.GetReadLink());
        }

        [Fact]
        public void HasNavigationPropertyLink_CanReplaceExistingLinks()
        {
            // Arrange
            var entity = _builder.AddEntityType(typeof(Motorcycle));
            var navigationProperty = entity.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.One);
            var singleton = _builder.AddSingleton("MyVehicle", entity);
            Uri link1 = new Uri("http://link1");
            Uri link2 = new Uri("http://link2");
            singleton.HasNavigationPropertyLink(navigationProperty, new NavigationLinkBuilder((entityContext, property) => link1, followsConventions: true));

            // Act
            singleton.HasNavigationPropertyLink(navigationProperty, new NavigationLinkBuilder((entityContext, property) => link2, followsConventions: false));

            // Assert
            var navigationLink = singleton.GetNavigationPropertyLink(navigationProperty);
            Assert.False(navigationLink.FollowsConventions);
            Assert.Equal(link2, navigationLink.Factory(null, null));
        }
    }
}
