// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.ProductFamilies;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.Vehicle;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBuilder
{
    public class ImplicitModelBuilder_EntityWithIndexers
    {
        public int ID { get; set; }

        public string this[int i]
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public string this[string i]
        {
            get
            {
                return null;
            }
            set
            {
            }
        }
    }

    public class ImplicitModelBuilder_EntityWithObjectProperty
    {
        public int ID { get; set; }

        public object ObjectProperty { get; set; }

        public object[] ObjectArray { get; set; }

        public IEnumerable<object> ObjectEnumerable { get; set; }

        public ICollection<object> ObjectCollection { get; set; }
    }

    public class ImplicitModelBuilder_EntityWithStaticProperty
    {
        public int ID { get; set; }
        public static string StaticProperty { get; set; }
    }

    public class ImplicitModelBuilderTests : WebHostTestBase
    {
        public ImplicitModelBuilderTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
        }

        [Fact]
        public void ShouldIgnoreStaticProperty()
        {
            ODataModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntityType<ImplicitModelBuilder_EntityWithStaticProperty>();
            var model = modelBuilder.GetEdmModel();

            var actual = model.SchemaElements.Where(e => e.Name == typeof(ImplicitModelBuilder_EntityWithStaticProperty).Name).OfType<EdmEntityType>().Single();
            Assert.Single(actual.Properties());
        }

        [Fact]
        public void ShouldIgnoreObjectProperties()
        {
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<ImplicitModelBuilder_EntityWithObjectProperty>("Entities");
            var model = modelBuilder.GetEdmModel();

            var actual = model.SchemaElements.Where(e => e.Name == typeof(ImplicitModelBuilder_EntityWithObjectProperty).Name).OfType<EdmEntityType>().Single();

            Assert.Single(actual.Properties());
        }

        [Fact]
        public void ShouldIgnoreIndexerProperty()
        {
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<ImplicitModelBuilder_EntityWithIndexers>("Entities");
            var model = modelBuilder.GetEdmModel();

            var actual = model.SchemaElements.Where(e => e.Name == typeof(ImplicitModelBuilder_EntityWithIndexers).Name).OfType<EdmEntityType>().Single();

            Assert.Single(actual.Properties());
        }

        [Fact]
        public void ChangeBaseTypeShouldWork()
        {
            ODataConventionModelBuilder mb = new ODataConventionModelBuilder();
            mb.EntitySet<Vehicle>("Vehicles");
            mb.EntityType<SportBike>().DerivesFrom<Vehicle>();

            var model = mb.GetEdmModel();
            var sportBike = model.SchemaElements.First(e => e.Name == typeof(SportBike).Name) as EdmEntityType;
            Assert.Equal(typeof(Vehicle).Name, ((EdmEntityType)sportBike.BaseType).Name);
        }

        [Fact]
        public void ChangeRequiredOnNavigationPropertyShouldWork()
        {
            ODataConventionModelBuilder mb = new ODataConventionModelBuilder();
            mb.EntityType<Product>().HasRequired(p => p.Family);
            //mb.EntitySet<Product>("Products");

            var model = mb.GetEdmModel();
            var product = model.SchemaElements.First(e => e.Name == typeof(Product).Name) as EdmEntityType;
            var familyProperty = product.Properties().Single(p => p.Name == "Family") as EdmNavigationProperty;
            Assert.Equal(EdmMultiplicity.One, familyProperty.TargetMultiplicity());
        }

        [Fact]
        public void ChangeNavigationLinkShouldWork()
        {
            ODataConventionModelBuilder mb = new ODataConventionModelBuilder();
            var products = mb.EntitySet<Product>("Products");
            mb.OnModelCreating = mb2 =>
                {
                    products.HasNavigationPropertiesLink(
                        products.EntityType.NavigationProperties,
                        (entityContext, navigationProperty) =>
                        {
                            object id;
                            entityContext.EdmObject.TryGetPropertyValue("ID", out id);
                            return new Uri(ResourceContextHelper.CreateODataLink(entityContext,
                                new EntitySetSegment(entityContext.NavigationSource as IEdmEntitySet),
                                new KeySegment(new[] {new KeyValuePair<string, object>("ID", id)}, entityContext.StructuredType as IEdmEntityType, null),
                                new NavigationPropertySegment(navigationProperty, null)));
                        },
                        false);
                };

            var model = mb.GetEdmModel();
        }

        [Fact]
        public void MultiplicityAssociationShouldWork()
        {
            ODataConventionModelBuilder mb = new ODataConventionModelBuilder();
            mb.EntitySet<Car>("Cars");
            mb.EntitySet<Vehicle>("Vehicles");
            var model = mb.GetEdmModel();
            var car = model.SchemaElements.First(e => e.Name == typeof(Car).Name) as EdmEntityType;
            var navigationProperty = car.NavigationProperties().First(p => p.Name == "BaseTypeNavigationProperty");
            Assert.Equal(EdmMultiplicity.Many, navigationProperty.TargetMultiplicity());
        }
    }

    public class ImplicitModelBuilderE2ETests : WebHostTestBase
    {
        public ImplicitModelBuilderE2ETests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.EnableODataSupport(GetImplicitEdmModel(configuration));
        }

        private static IEdmModel GetImplicitEdmModel(WebRouteConfiguration configuration)
        {
            var modelBuilder = configuration.CreateConventionModelBuilder();
            modelBuilder.EntitySet<Product>("Products");
            modelBuilder.EntitySet<Supplier>("Suppliers");
            modelBuilder.EntitySet<ProductFamily>("ProductFamilies");

            var model = modelBuilder.GetEdmModel();
            
            return model;
        }

        [Fact]
        public async Task VerifyMetaDataIsGeneratedCorrectly()
        {
            var response = await Client.GetAsync(BaseAddress + "/$metadata");

            var stream = await response.Content.ReadAsStreamAsync();
            IODataResponseMessage message = new ODataMessageWrapper(stream, response.Content.Headers);
            var reader = new ODataMessageReader(message);
            var edmModel = reader.ReadMetadataDocument();

            Assert.Equal(4, edmModel.GetEdmVersion().Major);
           
            var container = edmModel.EntityContainer;
            Assert.Equal("Container", container.Name);
            Assert.Equal(3, container.Elements.Count());

            var address = edmModel.SchemaElements.OfType<IEdmComplexType>().First();
            Assert.Equal("Address", address.Name);
            Assert.Equal(5, address.Properties().Count());

            var product = edmModel.SchemaElements.Where(e => e.Name == "Product").First() as IEdmEntityType;
            Assert.Single(product.Key());
            Assert.Equal("ID", product.Key().First().Name);
            Assert.Equal(5, product.Properties().Count());

            var supplier = edmModel.SchemaElements.Where(e => e.Name == "Supplier").First() as IEdmEntityType;
            Assert.Single(supplier.Key());
            Assert.Equal("ID", supplier.Key().First().Name);
            Assert.Equal(7, supplier.Properties().Count());

            var addressesProperty = supplier.Properties().First(p => p.Name == "Addresses").Type.AsCollection();
            Assert.Equal(typeof(Address).FullName, addressesProperty.CollectionDefinition().ElementType.FullName());
            
            // [ODATA-CSDL] 
            // In 6.2.1 
            // a) If not value is specified for a property whose Type attribute does not specify a collection,
            //    the Nuallable attribute defaults to true
            // b) A property whose Type attribute specifies a collection MUST NOT specify a value for the 
            //    Nullable attribute as the collection always exists, it may just be empty.
            Assert.True(addressesProperty.IsNullable);

            var tagsProperty = supplier.Properties().First(p => p.Name == "Tags").Type.AsCollection();
            Assert.Equal("Edm.String", tagsProperty.CollectionDefinition().ElementType.FullName());
            Assert.True(tagsProperty.IsNullable);
        }
    }
}