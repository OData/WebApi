﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
{
    public class EntitySetLinkConfigurationTest
    {
        [Fact]
        public void CanConfigureOnIdLinkViaIdLinkFactory()
        {
            // Arrange
            ODataModelBuilder builder = GetCommonModel();
            var expectedEditLink = "http://server/service/Products(15)";

            var products = builder.EntitySet<EntitySetLinkConfigurationTest_Product>("Products");
            products.HasIdLink(c =>
                new Uri(string.Format(
                    "http://server/service/Products({0})",
                    c.GetPropertyValue("ID"))
                ),
                followsConventions: false);

            var actor = builder.EntitySets.Single();
            var model = builder.GetEdmModel();
            var productType = model.SchemaElements.OfType<IEdmEntityType>().Single();
            var productsSet = model.SchemaElements.OfType<IEdmEntityContainer>().Single().EntitySets().Single();
            var productInstance = new EntitySetLinkConfigurationTest_Product { ID = 15 };
            var serializerContext = new ODataSerializerContext { Model = model, NavigationSource = productsSet };
            var entityContext = new ResourceContext(serializerContext, productType.AsReference(), productInstance);
            var linkBuilderAnnotation = new NavigationSourceLinkBuilderAnnotation(actor);

            // Act
            var selfLinks = linkBuilderAnnotation.BuildEntitySelfLinks(entityContext, ODataMetadataLevel.FullMetadata);

            // Assert
            Assert.Null(selfLinks.EditLink);
            Assert.Null(selfLinks.ReadLink);
            Assert.NotNull(selfLinks.IdLink);
            Assert.Equal(expectedEditLink, selfLinks.IdLink.ToString());
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
                    c.GetPropertyValue("ID")
                )
            ),
            followsConventions: false);
            products.HasReadLink(c => new Uri(
                string.Format(
                    "http://server2/service/Products/15",
                    c.GetPropertyValue("ID")
                )
            ),
            followsConventions: false);
            products.HasIdLink(c =>
                new Uri(string.Format(
                    "http://server3/service/Products({0})",
                    c.GetPropertyValue("ID"))
                ),
            followsConventions: false
            );

            var actor = builder.EntitySets.Single();
            var model = builder.GetEdmModel();
            var productType = model.SchemaElements.OfType<IEdmEntityType>().Single();
            var productsSet = model.SchemaElements.OfType<IEdmEntityContainer>().Single().EntitySets().Single();
            var productInstance = new EntitySetLinkConfigurationTest_Product { ID = 15 };
            var serializerContext = new ODataSerializerContext { Model = model, NavigationSource = productsSet };
            var entityContext = new ResourceContext(serializerContext, productType.AsReference(), productInstance);

            // Act
            var editLink = actor.GetEditLink().Factory(entityContext);
            var readLink = actor.GetReadLink().Factory(entityContext);
            var idLink = actor.GetIdLink().Factory(entityContext);

            // Assert
            Assert.NotNull(editLink);
            Assert.Equal(expectedEditLink, editLink.ToString());
            Assert.NotNull(readLink);
            Assert.Equal(expectedReadLink, readLink.ToString());
            Assert.NotNull(idLink);
            Assert.Equal(expectedIdLink, idLink.ToString());
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

            public EntitySetLinkConfigurationTest_Order[] Orders { get; set; }
        }

        class EntitySetLinkConfigurationTest_Order
        {
            public string ID { get; set; }
        }
    }
}
