// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Builder.TestModels;
using System.Web.Http.Routing;
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
            Assert.Null(convention.PropertyNavigationRouteName);
        }

        [Fact]
        public void Apply_AddsNavigationLinkFor_AllBaseDeclaredAndDerivedProperties()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            IEntityTypeConfiguration vehicle = builder.AddEntity(typeof(Vehicle));

            IEntityTypeConfiguration car = builder.AddEntity(typeof(Car)).DerivesFrom(vehicle);
            NavigationPropertyConfiguration carNavigationProperty = car.AddNavigationProperty(typeof(Car).GetProperty("Manufacturer"), EdmMultiplicity.ZeroOrOne);

            IEntityTypeConfiguration motorcycle = builder.AddEntity(typeof(Motorcycle)).DerivesFrom(vehicle);
            NavigationPropertyConfiguration motorcycleNavigationProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.ZeroOrOne);

            IEntityTypeConfiguration manufacturer = builder.AddEntity(typeof(Manufacturer));
            IEntityTypeConfiguration motorcycleManufacturer = builder.AddEntity(typeof(MotorcycleManufacturer)).DerivesFrom(manufacturer);
            IEntityTypeConfiguration carManufacturer = builder.AddEntity(typeof(CarManufacturer)).DerivesFrom(manufacturer);

            IEntitySetConfiguration manufacturers = builder.AddEntitySet("manufacturers", manufacturer);


            Mock<IEntitySetConfiguration> entitySet = new Mock<IEntitySetConfiguration>(MockBehavior.Strict);
            entitySet.Setup(v => v.EntityType).Returns(vehicle);
            entitySet.Setup(v => v.GetNavigationPropertyLink(motorcycleNavigationProperty)).Returns<NavigationPropertyConfiguration>(null);
            entitySet.Setup(v => v.GetNavigationPropertyLink(carNavigationProperty)).Returns<NavigationPropertyConfiguration>(null);

            entitySet
                .Setup(v => v.HasNavigationPropertyLink(motorcycleNavigationProperty, It.IsAny<Func<EntityInstanceContext, IEdmNavigationProperty, Uri>>()))
                .Returns<IEntitySetConfiguration>(null);
            entitySet
                .Setup(v => v.HasNavigationPropertyLink(carNavigationProperty, It.IsAny<Func<EntityInstanceContext, IEdmNavigationProperty, Uri>>()))
                .Returns<IEntitySetConfiguration>(null);

            // Act
            _convention.Apply(entitySet.Object, builder);

            // Assert
            entitySet.VerifyAll();
        }

        [Fact]
        public void Apply_AddsLinkBuilder_ForAllNavigationProperties()
        {
            // Arrange
            Mock<IEntityTypeConfiguration> entity = new Mock<IEntityTypeConfiguration>();
            NavigationPropertyConfiguration navigationProperty = new NavigationPropertyConfiguration(new MockPropertyInfo(typeof(Motorcycle), "Motorcycle"), EdmMultiplicity.One, entity.Object);
            entity.Setup(e => e.NavigationProperties).Returns(new[] { navigationProperty });

            var mockEntitySet = new Mock<IEntitySetConfiguration>();
            mockEntitySet.Setup(e => e.EntityType).Returns(entity.Object);
            mockEntitySet
                .Setup(e => e.HasNavigationPropertyLink(navigationProperty, It.IsAny<Func<EntityInstanceContext, IEdmNavigationProperty, Uri>>()))
                .Verifiable();

            var mockModelBuilder = new Mock<ODataModelBuilder>();

            // Act
            new NavigationLinksGenerationConvention().Apply(mockEntitySet.Object, mockModelBuilder.Object);

            // Assert
            mockEntitySet.Verify();
        }

        [Fact]
        public void GenerateNavigationLink_GeneratesLink()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var orders = builder.AddEntitySet("Orders", builder.AddEntity(typeof(NavigationLinksGenerationConventionTest_Order)));

            IEdmModel model = builder.GetEdmModel();
            var edmEntitySet = model.EntityContainers().Single().EntitySets().Single();

            HttpConfiguration configuration = new HttpConfiguration();
            var route = configuration.Routes.MapHttpRoute(ODataRouteNames.PropertyNavigation, "{controller}({parentId})/{navigationProperty}");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(new HttpRoute());

            Uri uri =
                new NavigationLinksGenerationConvention()
                .GenerateNavigationLink(
                    new EntityInstanceContext
                    {
                        EdmModel = model,
                        EntityInstance = new NavigationLinksGenerationConventionTest_Order { ID = 100 },
                        EntitySet = edmEntitySet,
                        EntityType = edmEntitySet.ElementType,
                        UrlHelper = request.GetUrlHelper()
                    },
                    edmEntitySet.ElementType.NavigationProperties().Single(),
                    orders);

            Assert.Equal("http://localhost/Orders(100)/Customer", uri.AbsoluteUri);
        }

        [Fact]
        public void GenerateNavigationLink_GeneratesCorrectLink_EvenIfRouteDataPointsToADifferentController()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var orders = builder.AddEntitySet("Orders", builder.AddEntity(typeof(NavigationLinksGenerationConventionTest_Order)));

            IEdmModel model = builder.GetEdmModel();
            var edmEntitySet = model.EntityContainers().Single().EntitySets().Single();

            HttpConfiguration configuration = new HttpConfiguration();
            var route = configuration.Routes.MapHttpRoute(ODataRouteNames.PropertyNavigation, "{controller}({parentId})/{navigationProperty}");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(route, new HttpRouteValueDictionary(new { controller = "Customers" }));

            Uri uri =
                new NavigationLinksGenerationConvention()
                .GenerateNavigationLink(
                    new EntityInstanceContext
                    {
                        EdmModel = model,
                        EntityInstance = new NavigationLinksGenerationConventionTest_Order { ID = 100 },
                        EntitySet = edmEntitySet,
                        EntityType = edmEntitySet.ElementType,
                        UrlHelper = request.GetUrlHelper()
                    },
                    edmEntitySet.ElementType.NavigationProperties().Single(),
                    orders);

            Assert.Equal("http://localhost/Orders(100)/Customer", uri.AbsoluteUri);
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
