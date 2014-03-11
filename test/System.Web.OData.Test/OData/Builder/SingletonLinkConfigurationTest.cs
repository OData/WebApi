// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Serialization;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Builder
{
    public class SingletonLinkConfigurationTest
    {
        [Fact]
        public void Singleton_CanConfigureAllLinksViaIdLink()
        {
            // Arrange
            ODataModelBuilder builder = GetSingletonModel();
            const string ExpectedEditLink = "http://server/service/Exchange";

            var product = builder.Singleton<SingletonProduct>("Exchange");
            product.HasIdLink(c => new Uri("http://server/service/Exchange"),
                followsConventions: false);

            var exchange = builder.Singletons.Single();
            var model = builder.GetEdmModel();
            var productType = model.SchemaElements.OfType<IEdmEntityType>().Single();
            var singleton = model.SchemaElements.OfType<IEdmEntityContainer>().Single().FindSingleton("Exchange");
            var singletonInstance = new SingletonProduct { ID = 15 };
            var serializerContext = new ODataSerializerContext { Model = model, NavigationSource = singleton };
            var entityContext = new EntityInstanceContext(serializerContext, productType.AsReference(), singletonInstance);
            var linkBuilderAnnotation = new NavigationSourceLinkBuilderAnnotation(exchange);

            // Act
            var selfLinks = linkBuilderAnnotation.BuildEntitySelfLinks(entityContext, ODataMetadataLevel.Default);

            // Assert
            Assert.NotNull(selfLinks.EditLink);
            Assert.Equal(ExpectedEditLink, selfLinks.EditLink.ToString());
            Assert.NotNull(selfLinks.ReadLink);
            Assert.Equal(ExpectedEditLink, selfLinks.ReadLink.ToString());
            Assert.NotNull(selfLinks.IdLink);
            Assert.Equal(ExpectedEditLink, selfLinks.IdLink.ToString());
        }

        [Fact]
        public void Singleton_CanConfigureLinksIndependently()
        {
            // Arrange
            ODataModelBuilder builder = GetSingletonModel();
            const string ExpectedEditLink = "http://server1/service/Exchange";
            const string ExpectedReadLink = "http://server2/service/Exchange";
            const string ExpectedIdLink = "http://server3/service/Exchange";

            var product = builder.Singleton<SingletonProduct>("Exchange");
            product.HasEditLink(c => new Uri("http://server1/service/Exchange"),
                followsConventions: false);
            product.HasReadLink(c => new Uri("http://server2/service/Exchange"),
                followsConventions: false);
            product.HasIdLink(c => new Uri("http://server3/service/Exchange"),
                followsConventions: false);

            var exchange = builder.Singletons.Single();
            var model = builder.GetEdmModel();
            var productType = model.SchemaElements.OfType<IEdmEntityType>().Single();
            var singleton = model.SchemaElements.OfType<IEdmEntityContainer>().Single().FindSingleton("Exchange");
            var singletonInstance = new SingletonProduct { ID = 15 };
            var serializerContext = new ODataSerializerContext { Model = model, NavigationSource = singleton };
            var entityContext = new EntityInstanceContext(serializerContext, productType.AsReference(), singletonInstance);

            // Act
            var editLink = exchange.GetEditLink().Factory(entityContext);
            var readLink = exchange.GetReadLink().Factory(entityContext);
            var idLink = exchange.GetIdLink().Factory(entityContext);

            // Assert
            Assert.NotNull(editLink);
            Assert.Equal(ExpectedEditLink, editLink.ToString());
            Assert.NotNull(readLink);
            Assert.Equal(ExpectedReadLink, readLink.ToString());
            Assert.NotNull(idLink);
            Assert.Equal(ExpectedIdLink, idLink.ToString());
        }

        [Fact]
        public void FailingToConfigureLinksResultsInNullLinks()
        {
            // Arrange
            ODataModelBuilder builder = GetSingletonModel();
            var exchange = builder.Singletons.Single();
            var model = builder.GetEdmModel();

            // Act & Assert
            Assert.Null(exchange.GetEditLink());
            Assert.Null(exchange.GetReadLink());
            Assert.Null(exchange.GetIdLink());
        }

        [Fact]
        public void FailingToConfigureNavigationLinks_Results_In_ArgumentException_When_BuildingNavigationLink()
        {
            // Arrange
            ODataModelBuilder builder = GetSingletonModel();
            builder.Singleton<SingletonProduct>("Exchange").HasManyBinding(p => p.Orders, "Orders");
            builder.EntitySet<SingletonOrder>("Orders").EntityType.HasKey(p => p.ID);
            var model = builder.GetEdmModel();

            IEdmSingleton exchange = model.EntityContainer.FindSingleton("Exchange");
            IEdmNavigationProperty ordersProperty = exchange.EntityType().DeclaredNavigationProperties().Single();
            var linkBuilder = model.GetNavigationSourceLinkBuilder(exchange);

            // Act & Assert
            Assert.ThrowsArgument(
                () => linkBuilder.BuildNavigationLink(new EntityInstanceContext(), ordersProperty, ODataMetadataLevel.Default),
                "navigationProperty",
                "No NavigationLink factory was found for the navigation property 'Orders' from entity type " +
                "'System.Web.OData.Builder.SingletonProduct' on entity set or singleton 'Exchange'. " +
                "Try calling HasNavigationPropertyLink on the NavigationSourceConfiguration.");
        }

        private ODataModelBuilder GetSingletonModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            var product = builder.Singleton<SingletonProduct>("Exchange");
            var productType = product.EntityType;
            productType.HasKey(p => p.ID);
            productType.Property(p => p.Name);
            productType.Property(p => p.Price);
            productType.Property(p => p.Cost);

            return builder;
        }

        class SingletonProduct
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public Decimal Price { get; set; }
            public Decimal Cost { get; set; }
            public SingletonOrder[] Orders { get; set; }
        }

        class SingletonOrder
        {
            public string ID { get; set; }
        }
    }
}
