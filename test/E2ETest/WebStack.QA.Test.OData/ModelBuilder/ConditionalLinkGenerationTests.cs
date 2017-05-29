using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using WebStack.QA.Test.OData.Common.Models.ProductFamilies;
using Xunit;

namespace WebStack.QA.Test.OData.ModelBuilder
{
    public class ConditionalLinkGeneration_ProductsController : InMemoryODataController<Product, int>
    {
        public ConditionalLinkGeneration_ProductsController()
            : base("ID")
        {
        }
    }

    public class ConditionalLinkGeneration_SuppliersController : InMemoryODataController<Supplier, int>
    {
        public ConditionalLinkGeneration_SuppliersController()
            : base("ID")
        {
        }
    }

    public class ConditionalLinkGeneration_ProductFamiliesController : InMemoryODataController<ProductFamily, int>
    {
        public ConditionalLinkGeneration_ProductFamiliesController()
            : base("ID")
        {
        }
    }

    public class ConditionalLinkGeneration_ConventionModelBuilder_Tests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var repo = ConditionalLinkGeneration_ProductsController.Repository;
            repo[typeof(Product)] = new System.Collections.Concurrent.ConcurrentDictionary<int, Product>();
            var productRepo = repo[typeof(Product)];
            productRepo.TryAdd(1, new Product
            {
                ID = 1,
                Name = "Product 1",
                Family = new ProductFamily
                {
                    ID = 1,
                    Name = "Product Family 1",
                    Supplier = new Supplier
                    {
                        ID = 1,
                        Name = "Supplier 1"
                    }
                }
            });

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            configuration.EnableODataSupport(GetImplicitEdmModel());
        }

        private static Microsoft.OData.Edm.IEdmModel GetImplicitEdmModel()
        {
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();
            var products = modelBuilder.EntitySet<Product>("ConditionalLinkGeneration_Products");
            products.HasEditLink(
               ctx => { return (Uri)null; },
               false);
            products.HasReadLink(
                ctx =>
                {
                    object id;
                    ctx.EdmObject.TryGetPropertyValue("ID", out id);
                    return new Uri(ctx.Url.CreateODataLink(
                                    new EntitySetPathSegment("ConditionalLinkGeneration_Products"),
                                    new KeyValuePathSegment(id.ToString())));
                },
                true);

            // Navigation Property is not working because of bug: http://aspnetwebstack.codeplex.com/workitem/780
            //products.HasNavigationPropertiesLink(
            //    products.EntityType.NavigationProperties,
            //    (ctx, nav) => null,
            //    false);

            modelBuilder.EntitySet<Supplier>("ConditionalLinkGeneration_Suppliers");
            modelBuilder.EntitySet<ProductFamily>("ConditionalLinkGeneration_ProductFamilies");

            var model = modelBuilder.GetEdmModel();
            return model;
        }

        [Fact(Skip="when we compute navigationlink and association link, if readlink is set but editlink is not set, it will throw null exception")]
        public void EditLinkWithNullValueShouldResultInNoEditLinkinPayload()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, this.BaseAddress + "/ConditionalLinkGeneration_Products(1)/");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            var response = this.Client.SendAsync(request).Result;
            var content = response.Content.ReadAsStringAsync().Result;
            Assert.DoesNotContain("<link rel=\"edit\"", content);
            Assert.Contains("<link rel=\"self\"", content);
        }
    }
}
