using System.Web.Http;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using WebStack.QA.Test.OData.Common.Models.ProductFamilies;
using Xunit;

namespace WebStack.QA.Test.OData.ModelBuilder
{
    public class MultipleEntitySetOnSameClrType_Products1Controller : InMemoryEntitySetController<Product, int>
    {
        public MultipleEntitySetOnSameClrType_Products1Controller()
            : base("ID")
        { 
        }
    }

    public class MultipleEntitySetOnSameClrType_Products2Controller : InMemoryEntitySetController<Product, int>
    {
        public MultipleEntitySetOnSameClrType_Products2Controller()
            : base("ID")
        { 
        }
    }

    public class MultipleEntitySetOnSameClrTypeTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var repo = MultipleEntitySetOnSameClrType_Products1Controller.Repository;
            repo.TryAdd(
                typeof(Product),
                new System.Collections.Concurrent.ConcurrentDictionary<int, Product>());
            repo[typeof(Product)].TryAdd(
                1,
                new Product
                {
                    ID = 1,
                    Name = "Product 1"
                });
            repo[typeof(Product)].TryAdd(
                2,
                new Product
                {
                    ID = 2,
                    Name = "Product 2"
                });

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetImplicitEdmModel());
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebStack.QA.Common.WebHost.WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        private static IEdmModel GetImplicitEdmModel()
        {
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Product>("MultipleEntitySetOnSameClrType_Products1").EntityType.Ignore(p => p.Family);
            modelBuilder.EntitySet<Product>("MultipleEntitySetOnSameClrType_Products2").EntityType.Ignore(p => p.Family);

            var model = modelBuilder.GetEdmModel();
            return model;
        }

        [Fact]
        public void QueryableShouldWork()
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/MultipleEntitySetOnSameClrType_Products1?$top=1").Result;
            response.EnsureSuccessStatusCode();

            response = this.Client.GetAsync(this.BaseAddress + "/MultipleEntitySetOnSameClrType_Products2?$top=1").Result;
            response.EnsureSuccessStatusCode();
        }
    }
}
