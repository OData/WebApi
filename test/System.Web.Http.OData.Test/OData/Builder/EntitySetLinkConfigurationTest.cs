// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Builder
{
    public class EntitySetLinkConfigurationTest
    {
        [Fact]
        public void CanConfigureAllLinksViaEditLink()
        {
            // Arrange
            ODataModelBuilder builder = GetCommonModel();
            var expectedEditLink = "http://server/service/Products(15)";

            var products = builder.EntitySet<EntitySetLinkConfigurationTest_Product>("Products");
            products.HasEditLink(c => new Uri(
                string.Format(
                    "http://server/service/Products({0})",
                    c.EntityInstance.ID
                )
            ));

            var actor = builder.EntitySets.Single();
            var model = builder.GetEdmModel();
            var productType = model.SchemaElements.OfType<IEdmEntityType>().Single();
            var productsSet = model.SchemaElements.OfType<IEdmEntityContainer>().Single().EntitySets().Single();
            var productInstance = new EntitySetLinkConfigurationTest_Product { ID = 15 };
            var entityContext = new EntityInstanceContext { EdmModel = model, EntitySet = productsSet, EntityType = productType, EntityInstance = productInstance, UrlHelper = new UrlHelper(new HttpRequestMessage()) };
            var entitySetLinkBuilderAnnotation = new EntitySetLinkBuilderAnnotation(actor);

            // Act
            var editLinkUri = entitySetLinkBuilderAnnotation.BuildEditLink(entityContext);
            var readLinkUri = entitySetLinkBuilderAnnotation.BuildReadLink(entityContext);
            var idLink = entitySetLinkBuilderAnnotation.BuildIdLink(entityContext);

            // Assert
            Assert.NotNull(editLinkUri);
            Assert.Equal(expectedEditLink, editLinkUri.ToString());
            Assert.NotNull(readLinkUri);
            Assert.Equal(expectedEditLink, readLinkUri.ToString());
            Assert.NotNull(idLink);
            Assert.Equal(expectedEditLink, idLink);
        }

        [Fact]
        public void CanConfigureLinksIndependently()
        {
            // Arrange
            ODataModelBuilder builder = GetCommonModel();
            var expectedEditLink = "http://server1/service/Products(15)";
            var expectedReadLink = "http://server2/service/Products/15";
            var expectedIdLink = "http://server3/service/Products(15)";

            var products = builder.EntitySet<EntitySetLinkConfigurationTest_Product>("Products");
            products.HasEditLink(c => new Uri(
                string.Format(
                    "http://server1/service/Products({0})",
                    c.EntityInstance.ID
                )
            ));
            products.HasReadLink(c => new Uri(
                string.Format(
                    "http://server2/service/Products/15",
                    c.EntityInstance.ID
                )
            ));
            products.HasIdLink(c =>
                string.Format(
                    "http://server3/service/Products({0})",
                    c.EntityInstance.ID
                )
            );

            var actor = builder.EntitySets.Single();
            var model = builder.GetEdmModel();
            var productType = model.SchemaElements.OfType<IEdmEntityType>().Single();
            var productsSet = model.SchemaElements.OfType<IEdmEntityContainer>().Single().EntitySets().Single();
            var productInstance = new EntitySetLinkConfigurationTest_Product { ID = 15 };
            var entityContext = new EntityInstanceContext { EdmModel = model, EntitySet = productsSet, EntityType = productType, EntityInstance = productInstance, UrlHelper = new UrlHelper(new HttpRequestMessage()) };

            // Act
            var editLink = actor.GetEditLink()(entityContext);
            var readLink = actor.GetReadLink()(entityContext);
            var idLink = actor.GetIdLink()(entityContext);

            // Assert
            Assert.NotNull(editLink);
            Assert.Equal(expectedEditLink, editLink.ToString());
            Assert.NotNull(readLink);
            Assert.Equal(expectedReadLink, readLink.ToString());
            Assert.NotNull(idLink);
            Assert.Equal(expectedIdLink, idLink);
        }

        [Fact]
        public void FailingToConfigureLinksResultsInNullLinks()
        {
            // Arrange
            ODataModelBuilder builder = GetCommonModel();
            var actor = builder.EntitySets.Single();
            var model = builder.GetEdmModel();

            // Act & Assert
            Assert.Null(actor.GetEditLink());
            Assert.Null(actor.GetReadLink());
            Assert.Null(actor.GetIdLink());
        }

        private ODataModelBuilder GetCommonModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            var products = builder.EntitySet<EntitySetLinkConfigurationTest_Product>("Products");
            var product = products.EntityType;
            product.HasKey(p => p.ID);
            product.Property(p => p.Name);
            product.Property(p => p.Price);
            product.Property(p => p.Cost);
            return builder;
        }

        class EntitySetLinkConfigurationTest_Product
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public Decimal Price { get; set; }
            public Decimal Cost { get; set; }
        }
    }
}
