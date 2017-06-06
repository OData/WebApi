using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Models.ProductFamilies;
using WebStack.QA.Test.OData.Common.Models.Vehicle;
using Xunit;

namespace WebStack.QA.Test.OData.ModelBuilder
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

    public class ImplicitModelBuilderTests
    {
        [Fact]
        public void ShouldIgnoreStaticProperty()
        {
            ODataModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.Entity<ImplicitModelBuilder_EntityWithStaticProperty>();
            var model = modelBuilder.GetEdmModel();

            var actual = model.SchemaElements.Where(e => e.Name == typeof(ImplicitModelBuilder_EntityWithStaticProperty).Name).OfType<EdmEntityType>().Single();
            Assert.Equal(1, actual.Properties().Count());
        }

        [Fact]
        public void ShouldIgnoreObjectProperties()
        {
            HttpConfiguration config = new HttpConfiguration();
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder(config);
            modelBuilder.EntitySet<ImplicitModelBuilder_EntityWithObjectProperty>("Entities");
            var model = modelBuilder.GetEdmModel();

            var actual = model.SchemaElements.Where(e => e.Name == typeof(ImplicitModelBuilder_EntityWithObjectProperty).Name).OfType<EdmEntityType>().Single();

            Assert.Equal(1, actual.Properties().Count());
        }

        [Fact]
        public void ShouldIgnoreIndexerProperty()
        {
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<ImplicitModelBuilder_EntityWithIndexers>("Entities");
            var model = modelBuilder.GetEdmModel();

            var actual = model.SchemaElements.Where(e => e.Name == typeof(ImplicitModelBuilder_EntityWithIndexers).Name).OfType<EdmEntityType>().Single();

            Assert.Equal(1, actual.Properties().Count());
        }

        [Fact]
        public void ChangeBaseTypeShouldWork()
        {
            HttpConfiguration config = new HttpConfiguration();
            ODataConventionModelBuilder mb = new ODataConventionModelBuilder(config);
            mb.EntitySet<Vehicle>("Vehicles");
            mb.Entity<SportBike>().DerivesFrom<Vehicle>();

            var model = mb.GetEdmModel();
            var sportBike = model.SchemaElements.First(e => e.Name == typeof(SportBike).Name) as EdmEntityType;
            Assert.Equal(typeof(Vehicle).Name, ((EdmEntityType)sportBike.BaseType).Name);
        }

        [Fact]
        public void ChangeRequiredOnNavigationPropertyShouldWork()
        {
            HttpConfiguration config = new HttpConfiguration();
            ODataConventionModelBuilder mb = new ODataConventionModelBuilder(config);
            mb.Entity<Product>().HasRequired(p => p.Family);
            //mb.EntitySet<Product>("Products");

            var model = mb.GetEdmModel();
            var product = model.SchemaElements.First(e => e.Name == typeof(Product).Name) as EdmEntityType;
            var familyProperty = product.Properties().Single(p => p.Name == "Family") as EdmNavigationProperty;
            Assert.Equal(EdmMultiplicity.One, familyProperty.Partner.Multiplicity());
        }

        [Fact]
        public void ChangeNavigationLinkShouldWork()
        {
            HttpConfiguration config = new HttpConfiguration();
            ODataConventionModelBuilder mb = new ODataConventionModelBuilder(config);
            var products = mb.EntitySet<Product>("Products");
            mb.OnModelCreating = mb2 =>
                {
                    products.HasNavigationPropertiesLink(
                        products.EntityType.NavigationProperties,
                        (entityContext, navigationProperty) =>
                        {
                            object id;
                            entityContext.EdmObject.TryGetPropertyValue("ID", out id);
                            return new Uri(entityContext.Url.CreateODataLink(
                                new EntitySetPathSegment("Products"),
                                new KeyValuePathSegment(id.ToString()),
                                new NavigationPathSegment(navigationProperty.Name)));
                        },
                        false);
                };

            var model = mb.GetEdmModel();
        }

        [Fact]
        public void MultiplicityAssociationShouldWork()
        {
            HttpConfiguration config = new HttpConfiguration();
            ODataConventionModelBuilder mb = new ODataConventionModelBuilder(config);
            mb.EntitySet<Car>("Cars");
            mb.EntitySet<Vehicle>("Vehicles");
            var model = mb.GetEdmModel();
            var car = model.SchemaElements.First(e => e.Name == typeof(Car).Name) as EdmEntityType;
            var navigationProperty = car.NavigationProperties().First(p => p.Name == "BaseTypeNavigationProperty");
            Assert.Equal(EdmMultiplicity.Many, navigationProperty.Partner.Multiplicity());
        }
    }

    public class ImplicitModelBuilderE2ETests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetImplicitEdmModel());
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        private static IEdmModel GetImplicitEdmModel()
        {
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Product>("Products");
            //.EntityType.HasRequired(p => p.Family);
            modelBuilder.EntitySet<Supplier>("Suppliers");
            modelBuilder.EntitySet<ProductFamily>("ProductFamilies");

            var model = modelBuilder.GetEdmModel();
            model.SetMaxDataServiceVersion(Version.Parse("3.0"));
            return model;
        }

        [Fact]
        public void VerifyMetaDataIsGeneratedCorrectly()
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/$metadata").Result;
            var stream = response.Content.ReadAsStreamAsync().Result;
            IODataResponseMessage message = new ODataMessageWrapper(stream, response.Content.Headers);
            var reader = new ODataMessageReader(message);
            var edmModel = reader.ReadMetadataDocument();

            Assert.Equal(3, edmModel.GetEdmVersion().Major);

            Assert.Equal(3, edmModel.GetMaxDataServiceVersion().Major);

            Assert.Equal(1, edmModel.EntityContainers().Count());

            var container = edmModel.EntityContainers().First();
            Assert.Equal("Container", container.Name);
            Assert.Equal(3, container.Elements.Count());

            var address = edmModel.SchemaElements.OfType<IEdmComplexType>().First();
            Assert.Equal("Address", address.Name);
            Assert.Equal(5, address.Properties().Count());

            var product = edmModel.SchemaElements.Where(e => e.Name == "Product").First() as IEdmEntityType;
            Assert.Equal(1, product.Key().Count());
            Assert.Equal("ID", product.Key().First().Name);
            Assert.Equal(5, product.Properties().Count());

            var supplier = edmModel.SchemaElements.Where(e => e.Name == "Supplier").First() as IEdmEntityType;
            Assert.Equal(1, supplier.Key().Count());
            Assert.Equal("ID", supplier.Key().First().Name);
            Assert.Equal(6, supplier.Properties().Count());

            var addressesProperty = supplier.Properties().First(p => p.Name == "Addresses").Type.AsCollection();
            Assert.Equal(typeof(Address).FullName, addressesProperty.CollectionDefinition().ElementType.FullName());
            Assert.Equal(false, addressesProperty.IsNullable);

            var tagsProperty = supplier.Properties().First(p => p.Name == "Tags").Type.AsCollection();
            Assert.Equal("Edm.String", tagsProperty.CollectionDefinition().ElementType.FullName());
            Assert.Equal(false, tagsProperty.IsNullable);
        }
    }
}