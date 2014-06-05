// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Serialization;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder.Conventions
{
    public class SelfLinksGenerationConventionTest
    {
        [Fact]
        public void Apply_AddsFeedSelfLink()
        {
            // Arrange
            var mockEntityType = new Mock<EntityTypeConfiguration>();
            var mockEntitySet = new Mock<EntitySetConfiguration>();
            mockEntitySet.Setup(entitySet => entitySet.GetFeedSelfLink()).Returns((Func<FeedContext, Uri>)null).Verifiable();
            mockEntitySet.Setup(entitySet => entitySet.HasFeedSelfLink(It.IsAny<Func<FeedContext, Uri>>()))
                .Returns(mockEntitySet.Object).Verifiable();
            mockEntitySet.Setup(entitySet => entitySet.EntityType).Returns(mockEntityType.Object);

            var mockModelBuilder = new Mock<ODataModelBuilder>();

            // Act
            new SelfLinksGenerationConvention().Apply(mockEntitySet.Object, mockModelBuilder.Object);

            // Assert
            mockEntitySet.Verify();
        }

        [Fact]
        public void Apply_DoesNotAddFeedSelfLink_IfOneIsPresent()
        {
            // Arrange
            var mockEntitySet = new Mock<EntitySetConfiguration>();
            var mockEntityType = new Mock<EntityTypeConfiguration>();
            mockEntitySet.Setup(entitySet => entitySet.GetFeedSelfLink()).Returns(feedContext => new Uri("http://www.cool.com")).Verifiable();
            mockEntitySet.Setup(entitySet => entitySet.EntityType).Returns(mockEntityType.Object);

            var mockModelBuilder = new Mock<ODataModelBuilder>();

            // Act
            new SelfLinksGenerationConvention().Apply(mockEntitySet.Object, mockModelBuilder.Object);

            // Assert
            mockEntitySet.Verify();
            mockEntitySet.Verify(entitySet => entitySet.HasFeedSelfLink(It.IsAny<Func<FeedContext, Uri>>()), Times.Never());
        }

        [Fact]
        public void SelfLinksGenerationConvention_Uses_GetByIdWithCast_IfDerivedTypeHasNavigationProperty()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var vehicles = builder.EntitySet<Vehicle>("vehicles");

            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet vehiclesEdmEntitySet = model.EntityContainer.EntitySets().Single();
            IEdmEntityType carType = model.AssertHasEntityType(typeof(Car));

            HttpRequestMessage request = GetODataRequest(model);
            NavigationSourceLinkBuilderAnnotation linkBuilder = model.GetNavigationSourceLinkBuilder(vehiclesEdmEntitySet);
            var serializerContext = new ODataSerializerContext { Model = model, NavigationSource = vehiclesEdmEntitySet, Url = request.GetUrlHelper() };
            var entityContext = new EntityInstanceContext(serializerContext, carType.AsReference(), new Car { Model = 2009, Name = "Contoso" });

            EntitySelfLinks selfLinks = linkBuilder.BuildEntitySelfLinks(entityContext, ODataMetadataLevel.FullMetadata);

            Assert.Equal("http://localhost/vehicles(Model=2009,Name='Contoso')", selfLinks.IdLink.ToString());
            Assert.Equal("http://localhost/vehicles(Model=2009,Name='Contoso')/System.Web.OData.Builder.TestModels.Car", selfLinks.EditLink.ToString());
        }

        [Fact]
        public void SelfLinksGenerationConvention_Uses_WithCast_IfDerivedTypeHasNavigationProperty_ForSingleton()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var myVehicle = builder.Singleton<Vehicle>("MyVehicle");

            IEdmModel model = builder.GetEdmModel();
            IEdmSingleton vehicleEdmSingleton = model.EntityContainer.FindSingleton("MyVehicle");
            IEdmEntityType carType = model.AssertHasEntityType(typeof(Car));

            HttpRequestMessage request = GetODataRequest(model);
            NavigationSourceLinkBuilderAnnotation linkBuilder = model.GetNavigationSourceLinkBuilder(vehicleEdmSingleton);
            var serializerContext = new ODataSerializerContext { Model = model, NavigationSource = vehicleEdmSingleton, Url = request.GetUrlHelper() };
            var entityContext = new EntityInstanceContext(serializerContext, carType.AsReference(), new Car { Model = 2014, Name = "Contoso" });

            // Act
            EntitySelfLinks selfLinks = linkBuilder.BuildEntitySelfLinks(entityContext, ODataMetadataLevel.FullMetadata);

            // Assert
            Assert.Equal("http://localhost/MyVehicle", selfLinks.IdLink.ToString());
            Assert.Equal("http://localhost/MyVehicle/System.Web.OData.Builder.TestModels.Car", selfLinks.EditLink.ToString());
        }

        [Fact]
        public void SelfLinksGenerationConvention_Uses_GetByIdWithoutCast_IfDerivedTypeDoesnotHaveNavigationProperty()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var motorcycles = builder.EntitySet<Motorcycle>("motorcycles");

            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet vehiclesEdmEntitySet = model.EntityContainer.EntitySets().Single();
            IEdmEntityType sportbikeType = model.AssertHasEntityType(typeof(SportBike));

            HttpRequestMessage request = GetODataRequest(model);
            NavigationSourceLinkBuilderAnnotation linkBuilder = model.GetNavigationSourceLinkBuilder(vehiclesEdmEntitySet);
            var serializerContext = new ODataSerializerContext { Model = model, NavigationSource = vehiclesEdmEntitySet, Url = request.GetUrlHelper() };
            var entityContext = new EntityInstanceContext(serializerContext, sportbikeType.AsReference(), new SportBike { Model = 2009, Name = "Ninja" });

            EntitySelfLinks selfLinks = linkBuilder.BuildEntitySelfLinks(entityContext, ODataMetadataLevel.FullMetadata);

            Assert.Equal("http://localhost/motorcycles(Model=2009,Name='Ninja')", selfLinks.IdLink.ToString());
        }

        [Fact]
        public void SelfLinksGenerationConvention_Uses_WithoutCast_IfDerivedTypeDoesnotHaveNavigationProperty_ForSingleton()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var myMotorcycle = builder.Singleton<Motorcycle>("MyMotor");

            IEdmModel model = builder.GetEdmModel();
            IEdmSingleton vehicleEdmSingleton = model.EntityContainer.FindSingleton("MyMotor");
            IEdmEntityType sportbikeType = model.AssertHasEntityType(typeof(SportBike));

            HttpRequestMessage request = GetODataRequest(model);
            NavigationSourceLinkBuilderAnnotation linkBuilder = model.GetNavigationSourceLinkBuilder(vehicleEdmSingleton);
            var serializerContext = new ODataSerializerContext { Model = model, NavigationSource = vehicleEdmSingleton, Url = request.GetUrlHelper() };
            var entityContext = new EntityInstanceContext(serializerContext, sportbikeType.AsReference(), new SportBike { Model = 2014, Name = "Ninja" });

            // Act
            EntitySelfLinks selfLinks = linkBuilder.BuildEntitySelfLinks(entityContext, ODataMetadataLevel.FullMetadata);

            // Assert
            Assert.Equal("http://localhost/MyMotor", selfLinks.IdLink.ToString());
        }

        private static HttpRequestMessage GetODataRequest(IEdmModel model)
        {
            HttpConfiguration configuration = new HttpConfiguration();
            string routeName = "Route";
            configuration.MapODataServiceRoute(routeName, null, model);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            request.SetConfiguration(configuration);
            request.ODataProperties().RouteName = routeName;

            return request;
        }

        class SelfLinkConventionTests_EntityType
        {
            public string ID { get; set; }
        }
    }
}
