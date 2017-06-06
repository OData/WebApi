using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Models.ProductFamilies;
using Xunit;

namespace WebStack.QA.Test.OData.ModelBuilder
{
    public class ExplicitModelBuilderTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetExplicitEdmModel());
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        private static IEdmModel GetExplicitEdmModel()
        {
            ODataModelBuilder modelBuilder = new ODataModelBuilder();

            var products = modelBuilder.EntitySet<Product>("Products");
            products.HasEditLink(entityContext =>
                {
                    object id;
                    entityContext.EdmObject.TryGetPropertyValue("ID", out id);
                    return entityContext.Url.Link(ODataTestConstants.DefaultRouteName, new
                    {
                        odataPath = entityContext.Url.CreateODataLink(
                            new EntitySetPathSegment(entityContext.EntitySet.Name),
                            new KeyValuePathSegment(id.ToString()))
                    });
                }, true);

            var suppliers = modelBuilder.EntitySet<Supplier>("Suppliers");
            suppliers.HasEditLink(entityContext =>
                {
                    object id;
                    entityContext.EdmObject.TryGetPropertyValue("ID", out id);
                    return entityContext.Url.Link(ODataTestConstants.DefaultRouteName, new
                    {
                        odataPath = entityContext.Url.CreateODataLink(
                            new EntitySetPathSegment(entityContext.EntitySet.Name),
                            new KeyValuePathSegment(id.ToString()))
                    });
                }, true);

            var families = modelBuilder.EntitySet<ProductFamily>("ProductFamilies");
            families.HasEditLink(entityContext =>
                {
                    object id;
                    entityContext.EdmObject.TryGetPropertyValue("ID", out id);
                    return entityContext.Url.Link(ODataTestConstants.DefaultRouteName, new
                    {
                        odataPath = entityContext.Url.CreateODataLink(
                            new EntitySetPathSegment(entityContext.EntitySet.Name),
                            new KeyValuePathSegment(id.ToString()))
                    });
                }, true);

            var product = products.EntityType;

            product.HasKey(p => p.ID);
            product.Property(p => p.Name);
            product.Property(p => p.ReleaseDate);
            product.Property(p => p.SupportedUntil);

            var address = modelBuilder.ComplexType<Address>();
            address.Property(a => a.City);
            address.Property(a => a.Country);
            address.Property(a => a.State);
            address.Property(a => a.Street);
            address.Property(a => a.ZipCode);

            var supplier = suppliers.EntityType;
            supplier.HasKey(s => s.ID);
            supplier.Property(s => s.Name);
            supplier.CollectionProperty(s => s.Addresses);
            supplier.CollectionProperty(s => s.Tags);
            supplier.Property(s => s.Country);

            var productFamily = families.EntityType;
            productFamily.HasKey(pf => pf.ID);
            productFamily.Property(pf => pf.Name);
            productFamily.Property(pf => pf.Description);

            // Create relationships and bindings in one go
            products.HasRequiredBinding(p => p.Family, families);
            families.HasManyBinding(pf => pf.Products, products);
            families.HasOptionalBinding(pf => pf.Supplier, suppliers);
            suppliers.HasManyBinding(s => s.ProductFamilies, families);

            // Create navigation Link builders
            products.HasNavigationPropertiesLink(
                product.NavigationProperties,
                (entityContext, navigationProperty) =>
                {
                    object id;
                    entityContext.EdmObject.TryGetPropertyValue("ID", out id);
                    return new Uri(entityContext.Url.Link(ODataTestConstants.DefaultRouteName,
                new
                {
                    odataPath = entityContext.Url.CreateODataLink(
                        new EntitySetPathSegment(entityContext.EntitySet.Name),
                        new KeyValuePathSegment(id.ToString()),
                        new NavigationPathSegment(navigationProperty))
                }));
                }, true);
            families.HasNavigationPropertiesLink(
                productFamily.NavigationProperties,
                (entityContext, navigationProperty) =>
                {
                    object id;
                    entityContext.EdmObject.TryGetPropertyValue("ID", out id);
                    return new Uri(entityContext.Url.Link(ODataTestConstants.DefaultRouteName,
                new
                {
                    odataPath = entityContext.Url.CreateODataLink(
                        new EntitySetPathSegment(entityContext.EntitySet.Name),
                        new KeyValuePathSegment(id.ToString()),
                        new NavigationPathSegment(navigationProperty))
                }));
                }, true);
            suppliers.HasNavigationPropertiesLink(
                supplier.NavigationProperties,
                (entityContext, navigationProperty) =>
                {
                    object id;
                    entityContext.EdmObject.TryGetPropertyValue("ID", out id);
                    return new Uri(entityContext.Url.Link(
                ODataTestConstants.DefaultRouteName,
                new
                {
                    odataPath = entityContext.Url.CreateODataLink(
                        new EntitySetPathSegment(entityContext.EntitySet.Name),
                        new KeyValuePathSegment(id.ToString()),
                        new NavigationPathSegment(navigationProperty))
                }));
                }, true);

            return modelBuilder.GetEdmModel();
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