// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.OData.Builder.TestModels;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Formatter.Serialization;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder.Conventions
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

            EntityTypeConfiguration vehicle = builder.AddEntity(typeof(Vehicle));

            EntityTypeConfiguration car = builder.AddEntity(typeof(Car)).DerivesFrom(vehicle);
            NavigationPropertyConfiguration carNavigationProperty = car.AddNavigationProperty(typeof(Car).GetProperty("Manufacturer"), EdmMultiplicity.ZeroOrOne);

            EntityTypeConfiguration motorcycle = builder.AddEntity(typeof(Motorcycle)).DerivesFrom(vehicle);
            NavigationPropertyConfiguration motorcycleNavigationProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.ZeroOrOne);

            EntityTypeConfiguration manufacturer = builder.AddEntity(typeof(Manufacturer));
            EntityTypeConfiguration motorcycleManufacturer = builder.AddEntity(typeof(MotorcycleManufacturer)).DerivesFrom(manufacturer);
            EntityTypeConfiguration carManufacturer = builder.AddEntity(typeof(CarManufacturer)).DerivesFrom(manufacturer);

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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Vehicle>("vehicles");
            builder.EntitySet<Manufacturer>("manufacturers");

            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet vehiclesEdmEntitySet = model.EntityContainers().Single().FindEntitySet("vehicles");
            IEdmEntityType carType = model.AssertHasEntityType(typeof(Car));
            IEdmNavigationProperty carManufacturerProperty = carType.AssertHasNavigationProperty(model, "Manufacturer", typeof(CarManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);

            HttpConfiguration configuration = new HttpConfiguration();
            string routeName = "Route";
            configuration.Routes.MapODataServiceRoute(routeName, null, model);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            request.SetConfiguration(configuration);
            request.ODataProperties().RouteName = routeName;

            EntitySetLinkBuilderAnnotation linkBuilder = model.GetEntitySetLinkBuilder(vehiclesEdmEntitySet);

            var serializerContext = new ODataSerializerContext { Model = model, EntitySet = vehiclesEdmEntitySet, Url = request.GetUrlHelper() };
            var entityContext = new EntityInstanceContext(serializerContext, carType.AsReference(), new Car { Model = 2009, Name = "Accord" });

            Uri uri = linkBuilder.BuildNavigationLink(entityContext, carManufacturerProperty, ODataMetadataLevel.Default);

            Assert.Equal("http://localhost/vehicles(Model=2009,Name='Accord')/System.Web.Http.OData.Builder.TestModels.Car/Manufacturer", uri.AbsoluteUri);
        }

        [Fact]
        public void Apply_AddsLinkBuilder_ForAllNavigationProperties()
        {
            // Arrange
            Mock<EntityTypeConfiguration> entity = new Mock<EntityTypeConfiguration>();
            NavigationPropertyConfiguration navigationProperty = new NavigationPropertyConfiguration(new MockPropertyInfo(typeof(Motorcycle), "Motorcycle"), EdmMultiplicity.One, entity.Object);
            entity.Setup(e => e.NavigationProperties).Returns(new[] { navigationProperty });

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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Car>("vehicles");
            builder.EntitySet<Manufacturer>("manufacturers");

            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet vehiclesEdmEntitySet = model.EntityContainers().Single().FindEntitySet("vehicles");
            IEdmEntityType carType = model.AssertHasEntityType(typeof(Car));
            IEdmNavigationProperty carManufacturerProperty = carType.AssertHasNavigationProperty(model, "Manufacturer", typeof(CarManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);

            HttpConfiguration configuration = new HttpConfiguration();
            string routeName = "Route";
            configuration.Routes.MapODataServiceRoute(routeName, null, model);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            request.SetConfiguration(configuration);
            request.ODataProperties().RouteName = routeName;

            EntitySetLinkBuilderAnnotation linkBuilder = model.GetEntitySetLinkBuilder(vehiclesEdmEntitySet);

            var serializerContext = new ODataSerializerContext { Model = model, EntitySet = vehiclesEdmEntitySet, Url = request.GetUrlHelper() };
            var entityContext = new EntityInstanceContext(serializerContext, carType.AsReference(), new Car { Model = 2009, Name = "Accord" });

            Uri uri = linkBuilder.BuildNavigationLink(entityContext, carManufacturerProperty, ODataMetadataLevel.Default);

            Assert.Equal("http://localhost/vehicles(Model=2009,Name='Accord')/Manufacturer", uri.AbsoluteUri);
        }

        [Fact]
        public void NavigationLinksGenerationConvention_GeneratesLinksWithoutCast_ForBaseProperties()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<SportBike>("vehicles");
            builder.EntitySet<Manufacturer>("manufacturers");

            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet vehiclesEdmEntitySet = model.EntityContainers().Single().FindEntitySet("vehicles");
            IEdmEntityType sportbikeType = model.AssertHasEntityType(typeof(SportBike));
            IEdmNavigationProperty motorcycleManufacturerProperty = sportbikeType.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);

            HttpConfiguration configuration = new HttpConfiguration();
            string routeName = "Route";
            configuration.Routes.MapODataServiceRoute(routeName, null, model);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            request.SetConfiguration(configuration);
            request.ODataProperties().RouteName = routeName;

            EntitySetLinkBuilderAnnotation linkBuilder = model.GetEntitySetLinkBuilder(vehiclesEdmEntitySet);

            var serializerContext = new ODataSerializerContext { Model = model, EntitySet = vehiclesEdmEntitySet, Url = request.GetUrlHelper() };
            var entityContext = new EntityInstanceContext(serializerContext, sportbikeType.AsReference(), new SportBike { Model = 2009, Name = "Ninja" });

            Uri uri = linkBuilder.BuildNavigationLink(entityContext, motorcycleManufacturerProperty, ODataMetadataLevel.Default);

            Assert.Equal("http://localhost/vehicles(Model=2009,Name='Ninja')/Manufacturer", uri.AbsoluteUri);
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
