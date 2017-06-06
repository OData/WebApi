using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using WebStack.QA.Test.OData.Common.Models.ProductFamilies;
using Xunit;

namespace WebStack.QA.Test.OData.Formatter
{
    public class ServerDrivenPaging_ProductsController : InMemoryEntitySetController<Product, int>
    {
        public ServerDrivenPaging_ProductsController()
            : base("ID")
        {
            if (this.LocalTable.Count == 0)
            {
                for (int i = 0; i < 100; i++)
                {
                    this.LocalTable.TryAdd(i, new Product 
                    { 
                        ID = i,
                        Name = "Test " + i,
                    });
                }
            }
        }

        [EnableQuery(PageSize = 10)]
        public override System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Product>> Get()
        {
            return base.Get();
        }
    }

    public class ServerDrivenPagingTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.Clear();
            configuration.EnableODataSupport(GetEdmModel(configuration));
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebStack.QA.Common.WebHost.WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        protected static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            var product = mb.EntitySet<Product>("ServerDrivenPaging_Products").EntityType;
            product.Ignore(p => p.Family);
            return mb.GetEdmModel();
        }

        [Fact]
        public void VerifyNextPageLinkAndInlineCountGeneratedCorrect()
        {
            var result = this.Client.GetStringAsync(this.BaseAddress + "/ServerDrivenPaging_Products?$inlinecount=allPages").Result;
            Assert.Contains("\"odata.count\":\"100\"", result);
            Assert.Contains("\"odata.nextLink\":", result);
            Assert.Contains("/ServerDrivenPaging_Products?$inlinecount=allPages&$skip=10", result);
        }
    }
}
