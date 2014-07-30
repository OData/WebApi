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
    public class SelfLinksGenerationConventionTest
    {
        [Fact]
        public void Apply_AddsFeedSelfLink()
        {
            // Arrange
            var mockEntityType = new Mock<EntityTypeConfiguration>();
            var mockEntitySet = new Mock<EntitySetConfiguration>();
            mockEntitySet.Setup(entitySet => entitySet.GetFeedSelfLink()).Returns((Func<FeedContext, Uri>)null).Verifiable();
            mockEntitySet.Setup(entitySet => entitySet.HasFeedSelfLink(It.IsAny<Func<FeedContext, Uri>>())).Returns(mockEntitySet.Object).Verifiable();
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
            IEdmEntitySet vehiclesEdmEntitySet = model.EntityContainers().Single().EntitySets().Single();
            IEdmEntityType carType = model.AssertHasEntityType(typeof(Car));

            HttpRequestMessage request = GetODataRequest(model);
            EntitySetLinkBuilderAnnotation linkBuilder = model.GetEntitySetLinkBuilder(vehiclesEdmEntitySet);
            var serializerContext = new ODataSerializerContext { Model = model, EntitySet = vehiclesEdmEntitySet, Url = request.GetUrlHelper() };
            var entityContext = new EntityInstanceContext(serializerContext, carType.AsReference(), new Car { Model = 2009, Name = "Accord" });

            EntitySelfLinks selfLinks = linkBuilder.BuildEntitySelfLinks(entityContext, ODataMetadataLevel.Default);

            Assert.Equal("http://localhost/vehicles(Model=2009,Name='Accord')/System.Web.Http.OData.Builder.TestModels.Car", selfLinks.IdLink);
        }

        [Fact]
        public void SelfLinksGenerationConvention_Uses_GetByIdWithoutCast_IfDerivedTypeDoesnotHaveNavigationProperty()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var motorcycles = builder.EntitySet<Motorcycle>("motorcycles");

            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet vehiclesEdmEntitySet = model.EntityContainers().Single().EntitySets().Single();
            IEdmEntityType sportbikeType = model.AssertHasEntityType(typeof(SportBike));

            HttpRequestMessage request = GetODataRequest(model);
            EntitySetLinkBuilderAnnotation linkBuilder = model.GetEntitySetLinkBuilder(vehiclesEdmEntitySet);
            var serializerContext = new ODataSerializerContext { Model = model, EntitySet = vehiclesEdmEntitySet, Url = request.GetUrlHelper() };
            var entityContext = new EntityInstanceContext(serializerContext, sportbikeType.AsReference(), new SportBike { Model = 2009, Name = "Ninja" });

            EntitySelfLinks selfLinks = linkBuilder.BuildEntitySelfLinks(entityContext, ODataMetadataLevel.Default);

            Assert.Equal("http://localhost/motorcycles(Model=2009,Name='Ninja')", selfLinks.IdLink);
        }

        private static HttpRequestMessage GetODataRequest(IEdmModel model)
        {
            HttpConfiguration configuration = new HttpConfiguration();
            string routeName = "Route";
            configuration.Routes.MapODataServiceRoute(routeName, null, model);

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
