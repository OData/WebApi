//-----------------------------------------------------------------------------
// <copyright file="StableOrderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.ProductFamilies;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class StableOrderController : TestNonODataController
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

    public class StableOrderWithoutResultLimitTests : WebHostTestBase
    {
        public StableOrderWithoutResultLimitTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.AddODataQueryFilter();
            configuration.EnableDependencyInjection();
        }

        [Theory]
        [InlineData("/api/StableOrder/GetQueryable")]
        [InlineData("/api/StableOrder/GetEnumerable")]
        [InlineData("/api/StableOrder/GetEnumerable?a=b")]
        [InlineData("/api/StableOrder/GetQueryable?a=b")]
        [InlineData("/api/StableOrder/GetQueryable?$filter=true")]
        public async Task TestNoOrderChange(string url)
        {
            var response = await this.Client.GetAsync(this.BaseAddress + url);
            response.EnsureSuccessStatusCode();
            var actual = (await response.Content.ReadAsObject<IEnumerable<Product>>()).ToList();
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
        public async Task TestHasStableOrder(string url)
        {
            var response = await this.Client.GetAsync(this.BaseAddress + url);
            response.EnsureSuccessStatusCode();
            var actual = (await response.Content.ReadAsObject<IEnumerable<Product>>()).ToList();
            var expected = StableOrderController.Products.OrderBy(p => p.ID).Skip(1).ToList();
            for (int i = 0; i < expected.Count(); i++)
            {
                Assert.Equal(expected[i].ID, actual[i].ID);
            }
        }
    }
}
