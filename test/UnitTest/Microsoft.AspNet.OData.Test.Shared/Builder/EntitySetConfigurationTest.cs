﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
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
            ExceptionAssert.ThrowsArgumentNull(
                () => new EntitySetConfiguration(modelBuilder: null, entityClrType: typeof(EntitySetConfigurationTest), name: "entityset"),
                "modelBuilder");
        }

        [Fact]
        public void CtorThatTakesClrType_Throws_ArgumentNull_For_EntityType()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new EntitySetConfiguration(modelBuilder: new ODataModelBuilder(), entityClrType: (Type)null, name: "entityset"),
                "clrType");
        }

        [Fact]
        public void CtorThatTakesClrType_Throws_ArgumentNull_For_Name()
        {
            ExceptionAssert.Throws<ArgumentException>(
                () => new EntitySetConfiguration(modelBuilder: new ODataModelBuilder(), entityClrType: typeof(EntitySetConfigurationTest), name: null),
                "The argument 'name' is null or empty.\r\nParameter name: name");
        }

        [Fact]
        public void CtorThatTakesEntityTypeConfiguration_Throws_ArgumentNull_For_ModelBuilder()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new EntitySetConfiguration(
                    modelBuilder: null,
                    entityType: new EntityTypeConfiguration(new ODataModelBuilder(), typeof(EntitySetConfigurationTest)),
                    name: "entityset"),
                "modelBuilder");
        }

        [Fact]
        public void CtorThatTakesEntityTypeConfiguration_Throws_ArgumentNull_For_EntityType()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new EntitySetConfiguration(
                    modelBuilder: new ODataModelBuilder(),
                    entityType: (EntityTypeConfiguration)null,
                    name: "entityset"),
                "entityType");
        }

        [Fact]
        public void CtorThatTakesEntityTypeConfiguration_Throws_ArgumentNull_For_Name()
        {
            ExceptionAssert.Throws<ArgumentException>(
                () => new EntitySetConfiguration(
                    modelBuilder: new ODataModelBuilder(),
                    entityType: new EntityTypeConfiguration(new ODataModelBuilder(), typeof(EntitySetConfigurationTest)),
                    name: null),
                    "The argument 'name' is null or empty.\r\nParameter name: name");
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
            SelfLinkBuilder<Uri> idLinkBuilder = new SelfLinkBuilder<Uri>((ctxt) => null, followsConventions: true);

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
            var entity = _builder.AddEntityType(typeof(Motorcycle));
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
