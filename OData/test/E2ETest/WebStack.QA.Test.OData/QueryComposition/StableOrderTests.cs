using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Extensions;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Models.ProductFamilies;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public class StableOrderController : ApiController
    {
        private static List<Product> products = new List<Product>();
        static StableOrderController()
        {
            products.Add(new Product()
            {
                ID = 3
            });
            products.Add(new Product()
            {
                ID = 2
            });
            products.Add(new Product()
            {
                ID = 1
            });
            products.Add(new Product()
            {
                ID = 9
            });
            products.Add(new Product()
            {
                ID = 6
            });
            products.Add(new Product()
            {
                ID = 7
            });
            products.Add(new Product()
            {
                ID = 8
            });
        }

        public static List<Product> Products
        {
            get
            {
                return products;
            }
        }

        public IQueryable<Product> GetQueryable()
        {
            return products.AsQueryable();
        }

        [EnableQuery]
        public IEnumerable<Product> GetEnumerable()
        {
            return products.AsEnumerable();
        }

        [EnableQuery(PageSize = 100)]
        public IEnumerable<Product> GetEnumerableWithResultLimit()
        {
            return products.AsEnumerable();
        }
    }

    public class StableOrderWithoutResultLimitTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.AddODataQueryFilter();
            configuration.EnableDependencyInjection("api default");
        }

        [Theory]
        [InlineData("/api/StableOrder/GetQueryable")]
        [InlineData("/api/StableOrder/GetEnumerable")]
        [InlineData("/api/StableOrder/GetEnumerable?a=b")]
        [InlineData("/api/StableOrder/GetQueryable?a=b")]
        [InlineData("/api/StableOrder/GetQueryable?$filter=true")]
        public void TestNoOrderChange(string url)
        {
            var response = this.Client.GetAsync(this.BaseAddress + url).Result;
            response.EnsureSuccessStatusCode();
            var actual = response.Content.ReadAsAsync<IEnumerable<Product>>().Result.ToList();
            var expected = StableOrderController.Products;
            for (int i = 0; i < expected.Count(); i++)
            {
                Assert.Equal(expected[i].ID, actual[i].ID);
            }
        }

        [Theory]
        [InlineData("/api/StableOrder/GetQueryable?$skip=1&$top=100")]
        [InlineData("/api/StableOrder/GetEnumerable?$skip=1")]
        [InlineData("/api/StableOrder/GetQueryable?$skip=1")]
        [InlineData("/api/StableOrder/GetEnumerableWithResultLimit?$skip=1")]
        public void TestHasStableOrder(string url)
        {
            var response = this.Client.GetAsync(this.BaseAddress + url).Result;
            response.EnsureSuccessStatusCode();
            var actual = response.Content.ReadAsAsync<IEnumerable<Product>>().Result.ToList();
            var expected = StableOrderController.Products.OrderBy(p => p.ID).Skip(1).ToList();
            for (int i = 0; i < expected.Count(); i++)
            {
                Assert.Equal(expected[i].ID, actual[i].ID);
            }
        }
    }
}
