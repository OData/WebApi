//-----------------------------------------------------------------------------
// <copyright file="NavigationLinksGenerationConventionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Builder.Conventions;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder.Conventions
{
    public class NavigationLinksGenerationConventionTest
    {
        private NavigationLinksGenerationConvention _convention = new NavigationLinksGenerationConvention();

        [Fact]
        public void DefaultCtor_DoesntThrow()
        {
            NavigationLinksGenerationConvention convention = new NavigationLinksGenerationConvention();
        }

        [Fact]
        public void Apply_AddsNavigationLinkFor_AllBaseDeclaredAndDerivedProperties()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            EntityTypeConfiguration vehicle = builder.AddEntityType(typeof(Vehicle));

            EntityTypeConfiguration car = builder.AddEntityType(typeof(Car)).DerivesFrom(vehicle);
            NavigationPropertyConfiguration carNavigationProperty = car.AddNavigationProperty(typeof(Car).GetProperty("Manufacturer"), EdmMultiplicity.ZeroOrOne);

            EntityTypeConfiguration motorcycle = builder.AddEntityType(typeof(Motorcycle)).DerivesFrom(vehicle);
            NavigationPropertyConfiguration motorcycleNavigationProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.ZeroOrOne);

            EntityTypeConfiguration manufacturer = builder.AddEntityType(typeof(Manufacturer));
            EntityTypeConfiguration motorcycleManufacturer = builder.AddEntityType(typeof(MotorcycleManufacturer)).DerivesFrom(manufacturer);
            EntityTypeConfiguration carManufacturer = builder.AddEntityType(typeof(CarManufacturer)).DerivesFrom(manufacturer);

            EntitySetConfiguration manufacturers = builder.AddEntitySet("manufacturers", manufacturer);


            Mock<EntitySetConfiguration> entitySet = new Mock<EntitySetConfiguration>(MockBehavior.Strict);
            entitySet.Setup(v => v.EntityType).Returns(vehicle);
            entitySet.Setup(v => v.GetNavigationPropertyLink(motorcycleNavigationProperty)).Returns<NavigationPropertyConfiguration>(null);
            entitySet.Setup(v => v.GetNavigationPropertyLink(carNavigationProperty)).Returns<NavigationPropertyConfiguration>(null);

            entitySet
                .Setup(v => v.HasNavigationPropertyLink(motorcycleNavigationProperty, It.IsAny<NavigationLinkBuilder>()))
                .Callback((NavigationPropertyConfiguration property, NavigationLinkBuilder navBuilder) => Assert.True(navBuilder.FollowsConventions))
                .Returns<EntitySetConfiguration>(null);
            entitySet
                .Setup(v => v.HasNavigationPropertyLink(carNavigationProperty, It.IsAny<NavigationLinkBuilder>()))
                .Returns<EntitySetConfiguration>(null);

            // Act
            _convention.Apply(entitySet.Object, builder);

            // Assert
            entitySet.VerifyAll();
        }

        [Fact]
        public void NavigationLinksGenerationConvention_GeneratesLinksWithCast_ForDerivedProperties()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Vehicle>("vehicles");
            builder.EntitySet<Manufacturer>("manufacturers");

            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet vehiclesEdmEntitySet = model.EntityContainer.FindEntitySet("vehicles");
            IEdmEntityType carType = model.AssertHasEntityType(typeof(Car));
            IEdmNavigationProperty carManufacturerProperty = carType.AssertHasNavigationProperty(model, "Manufacturer", typeof(CarManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);

            var configuration = RoutingConfigurationFactory.Create();
            string routeName = "Route";
            configuration.MapODataServiceRoute(routeName, null, model);

            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost", configuration, routeName);

            NavigationSourceLinkBuilderAnnotation linkBuilder = model.GetNavigationSourceLinkBuilder(vehiclesEdmEntitySet);
            linkBuilder.AddNavigationPropertyLinkBuilder(carManufacturerProperty, new NavigationLinkBuilder((context, property) => context.GenerateNavigationPropertyLink(property, includeCast: true), false));
            var serializerContext = ODataSerializerContextFactory.Create(model, vehiclesEdmEntitySet, request);
            var entityContext = new ResourceContext(serializerContext, carType.AsReference(), new Car { Model = 2009, Name = "Accord" });

            Uri uri = linkBuilder.BuildNavigationLink(entityContext, carManufacturerProperty, ODataMetadataLevel.MinimalMetadata);

            Assert.Equal("http://localhost/vehicles(Model=2009,Name='Accord')/Microsoft.AspNet.OData.Test.Builder.TestModels.Car/Manufacturer", uri.AbsoluteUri);
        }

        [Fact]
        public void Apply_AddsLinkBuilder_ForAllNavigationProperties()
        {
            // Arrange
            Mock<EntityTypeConfiguration> entity = new Mock<EntityTypeConfiguration>();
            NavigationPropertyConfiguration navigationProperty = new NavigationPropertyConfiguration(new MockPropertyInfo(typeof(Motorcycle), "Motorcycle"), EdmMultiplicity.One, entity.Object);
            entity.Setup(e => e.NavigationProperties).Returns(new[] { navigationProperty });
            entity.SetupGet(e => e.Kind).Returns(EdmTypeKind.Entity);

            var mockEntitySet = new Mock<EntitySetConfiguration>();
            mockEntitySet.Setup(e => e.EntityType).Returns(entity.Object);
            bool navigationLinkSetup = false;
            mockEntitySet
                .Setup(e => e.HasNavigationPropertyLink(navigationProperty, It.IsAny<NavigationLinkBuilder>()))
                .Callback((NavigationPropertyConfiguration property, NavigationLinkBuilder navBuilder) =>
                {
                    Assert.True(navBuilder.FollowsConventions);
                    navigationLinkSetup = true;
                });

            var mockModelBuilder = new Mock<ODataModelBuilder>();

            // Act
            new NavigationLinksGenerationConvention().Apply(mockEntitySet.Object, mockModelBuilder.Object);

            // Assert
            Assert.True(navigationLinkSetup);
        }

        [Fact]
        public void NavigationLinksGenerationConvention_GeneratesLinksWithoutCast_ForDeclaredProperties()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Car>("vehicles");
            builder.EntitySet<Manufacturer>("manufacturers");

            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet vehiclesEdmEntitySet = model.EntityContainer.FindEntitySet("vehicles");
            IEdmEntityType carType = model.AssertHasEntityType(typeof(Car));
            IEdmNavigationProperty carManufacturerProperty = carType.AssertHasNavigationProperty(model, "Manufacturer", typeof(CarManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);

            var configuration = RoutingConfigurationFactory.Create();
            string routeName = "Route";
            configuration.MapODataServiceRoute(routeName, null, model);

            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost", configuration, routeName);

            NavigationSourceLinkBuilderAnnotation linkBuilder = model.GetNavigationSourceLinkBuilder(vehiclesEdmEntitySet);
            linkBuilder.AddNavigationPropertyLinkBuilder(carManufacturerProperty, new NavigationLinkBuilder((context, property) => context.GenerateNavigationPropertyLink(property, includeCast: false), false));
            var serializerContext = ODataSerializerContextFactory.Create(model, vehiclesEdmEntitySet, request);
            var entityContext = new ResourceContext(serializerContext, carType.AsReference(), new Car { Model = 2009, Name = "Accord" });

            Uri uri = linkBuilder.BuildNavigationLink(entityContext, carManufacturerProperty, ODataMetadataLevel.MinimalMetadata);

            Assert.Equal("http://localhost/vehicles(Model=2009,Name='Accord')/Manufacturer", uri.AbsoluteUri);
        }

        [Fact]
        public void NavigationLinksGenerationConvention_GeneratesLinksWithoutCast_ForDeclaredProperties_OnSingleton()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.Singleton<Car>("Contoso");
            builder.EntitySet<Manufacturer>("manufacturers");

            IEdmModel model = builder.GetEdmModel();
            IEdmSingleton vehicleEdmSingleton = model.EntityContainer.FindSingleton("Contoso");
            IEdmEntityType carType = model.AssertHasEntityType(typeof(Car));
            IEdmNavigationProperty carManufacturerProperty = carType.AssertHasNavigationProperty(model, "Manufacturer", typeof(CarManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);

            var configuration = RoutingConfigurationFactory.Create();
            string routeName = "Route";
            configuration.MapODataServiceRoute(routeName, null, model);

            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost", configuration, routeName);

            NavigationSourceLinkBuilderAnnotation linkBuilder = model.GetNavigationSourceLinkBuilder(vehicleEdmSingleton);
            linkBuilder.AddNavigationPropertyLinkBuilder(carManufacturerProperty, new NavigationLinkBuilder((context, property) => context.GenerateNavigationPropertyLink(property, includeCast: false), false));
            var serializerContext = ODataSerializerContextFactory.Create(model, vehicleEdmSingleton, request);
            var entityContext = new ResourceContext(serializerContext, carType.AsReference(), new Car { Model = 2014, Name = "Contoso2014" });

            Uri uri = linkBuilder.BuildNavigationLink(entityContext, carManufacturerProperty, ODataMetadataLevel.MinimalMetadata);

            Assert.Equal("http://localhost/Contoso/Manufacturer", uri.AbsoluteUri);
        }

        [Fact]
        public void NavigationLinksGenerationConvention_GeneratesLinksWithoutCast_ForBaseProperties()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<SportBike>("vehicles");
            builder.EntitySet<Manufacturer>("manufacturers");

            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet vehiclesEdmEntitySet = model.EntityContainer.FindEntitySet("vehicles");
            IEdmEntityType sportbikeType = model.AssertHasEntityType(typeof(SportBike));
            IEdmNavigationProperty motorcycleManufacturerProperty = sportbikeType.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);

            var configuration = RoutingConfigurationFactory.Create();
            string routeName = "Route";
            configuration.MapODataServiceRoute(routeName, null, model);

            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost", configuration, routeName);

            NavigationSourceLinkBuilderAnnotation linkBuilder = model.GetNavigationSourceLinkBuilder(vehiclesEdmEntitySet);
            linkBuilder.AddNavigationPropertyLinkBuilder(motorcycleManufacturerProperty, new NavigationLinkBuilder((context, property) => context.GenerateNavigationPropertyLink(property, includeCast: false), false));
            var serializerContext = ODataSerializerContextFactory.Create(model, vehiclesEdmEntitySet, request);
            var entityContext = new ResourceContext(serializerContext, sportbikeType.AsReference(), new SportBike { Model = 2009, Name = "Ninja" });

            // We might get one of these:
            // http://localhost/vehicles(Model=2009,Name='Ninja')/Manufacturer
            // http://localhost/vehicles(Name='Ninja',Model=2009)/Manufacturer
            Uri uri = linkBuilder.BuildNavigationLink(entityContext, motorcycleManufacturerProperty, ODataMetadataLevel.MinimalMetadata);
            string aboluteUri = uri.AbsoluteUri;
            Assert.Contains("http://localhost/vehicles(", aboluteUri);
            Assert.Contains("Model=2009", aboluteUri);
            Assert.Contains("Name='Ninja'", aboluteUri);
            Assert.Contains(")/Manufacturer", aboluteUri);
        }
    }

    public class NavigationLinksGenerationConventionTest_Order
    {
        public int ID { get; set; }

        public NavigationLinksGenerationConventionTest_Customer Customer { get; set; }
    }

    public class NavigationLinksGenerationConventionTest_Customer
    {
        public int ID { get; set; }
    }
}
