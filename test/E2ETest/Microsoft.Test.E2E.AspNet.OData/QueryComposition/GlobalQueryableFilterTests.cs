//-----------------------------------------------------------------------------
// <copyright file="GlobalQueryableFilterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.ProductFamilies;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class DerivedQueryableAttribute : EnableQueryAttribute
    {
        public DerivedQueryableAttribute()
        {
            this.PageSize = 3;
        }

#if NETCORE
        public override void ValidateQuery(Microsoft.AspNetCore.Http.HttpRequest request, ODataQueryOptions queryOptions)
#else
        public override void ValidateQuery(System.Net.Http.HttpRequestMessage request, ODataQueryOptions queryOptions)
#endif
        {
            // Skip validation.
        }
    }

    public class DerivedODataQueryOptions : ODataQueryOptions
    {
#if NETCORE
        public DerivedODataQueryOptions(ODataQueryContext context, Microsoft.AspNetCore.Http.HttpRequest request)
#else
        public DerivedODataQueryOptions(ODataQueryContext context, System.Net.Http.HttpRequestMessage request)
#endif
            : base(context, request)
        {
        }
    }

    public class GlobalQueryableFilterController : TestNonODataController
    {
        private static List<Product> products = new List<Product>();
        static GlobalQueryableFilterController()
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

        public IQueryable GetQueryable()
        {
            return products.AsQueryable();
        }

        public IQueryable<Product> GetQueryableT()
        {
            return products.AsQueryable();
        }

        public IQueryable<Product> GetQueryableTWithOptions(ODataQueryOptions option)
        {
            return products.AsQueryable();
        }

        public ITestActionResult GetHttpResponseMessage()
        {
            return Ok<IQueryable<Product>>(products.AsQueryable());
        }

        public Task<IQueryable> GetTaskIQueryableT()
        {
            return new TaskFactory().StartNew<IQueryable>(
                () =>
                {
                    return products.AsQueryable();
                });
        }

        public IQueryable<Product> GetQueryableTWithDerivedOptions(DerivedODataQueryOptions option)
        {
            return products.AsQueryable();
        }

        public IEnumerable<Product> GetEnumerableT()
        {
            return products.AsEnumerable();
        }

        [EnableQuery(PageSize = 5)]
        public IQueryable<Product> GetQueryableTWithResultLimit()
        {
            return products.AsQueryable();
        }

        [EnableQuery]
        public IEnumerable GetEnumerableWithQAttr()
        {
            return products.AsEnumerable();
        }

        [DerivedQueryable(PageSize = 3)]
        public IQueryable<Product> GetQueryableTWithDerivedQAttrResultLimit()
        {
            return products.AsQueryable();
        }

        [EnableQuery]
        public Product GetProductWithQAttr()
        {
            return products[0];
        }

        [DerivedQueryable]
        public Product GetProductWithDerivedQAttr()
        {
            return products[0];
        }

        public Product GetProduct()
        {
            return products[0];
        }
    }

    public class DerivedEnitySetController : TestODataController
    {
        [DerivedQueryable]
        public IQueryable<Product> Get()
        {
            return GlobalQueryableFilterController.Products.AsQueryable();
        }
    }

    public class GlobalQueryableFilterWithoutResultLimitTests : WebHostTestBase
    {
        public GlobalQueryableFilterWithoutResultLimitTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddODataQueryFilter(new EnableQueryAttribute() { PageSize = 3 });
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.EnableDependencyInjection();
        }

        [Theory]
        [InlineData("/api/GlobalQueryableFilter/GetQueryable")]
        [InlineData("/api/GlobalQueryableFilter/GetQueryableT")]
        [InlineData("/api/GlobalQueryableFilter/GetEnumerableWithQAttr")]
        [InlineData("/api/GlobalQueryableFilter/GetQueryableTWithResultLimit")]
        [InlineData("/api/GlobalQueryableFilter/GetTaskIQueryableT")]
        public async Task TestQueryableWorksUnderGlobalFilter(string url)
        {
            var response = await this.Client.GetAsync(this.BaseAddress + url + "?$top=1");
            var actual = await response.Content.ReadAsObject<IEnumerable<Product>>();

            Assert.Single(actual);
        }
        [Theory]
        [InlineData("/api/GlobalQueryableFilter/GetQueryableTWithDerivedOptions")]
        [InlineData("/api/GlobalQueryableFilter/GetQueryableTWithOptions")]
        [InlineData("/api/GlobalQueryableFilter/GetEnumerableT")]
        [InlineData("/api/GlobalQueryableFilter/GetHttpResponseMessage")]
        public async Task TestActionsThatAreIgnoredByGlobalFilter(string url)
        {
            var response = await this.Client.GetAsync(this.BaseAddress + url + "?$top=1");
            var actual = await response.Content.ReadAsObject<IEnumerable<Product>>();

            Assert.NotEqual(1, actual.Count());
        }

        [Theory]
        [InlineData("/api/GlobalQueryableFilter/GetProductWithQAttr")]
        public async Task TestActionsThatNotAllowedByQueryableAttribute(string url)
        {
            var response = await this.Client.GetAsync(this.BaseAddress + url + "?$top=1");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            Assert.Contains("The requested resource is not a collection.", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("/api/GlobalQueryableFilter/GetQueryableTWithResultLimit")]
        public virtual async Task TestQueryableAttributeShouldWinGlobalQueryableFilter(string url)
        {
            var response = await this.Client.GetAsync(this.BaseAddress + url);
            var actual = await response.Content.ReadAsObject<IEnumerable<Product>>();

            Assert.Equal(5, actual.Count());
        }

        [Theory]
        [InlineData("/api/GlobalQueryableFilter/GetProduct")]
        public virtual async Task TestQueryableAttributeWontAffectOtherActions(string url)
        {
            var response = await this.Client.GetAsync(this.BaseAddress + url);
            response.EnsureSuccessStatusCode();
        }
    }

    public class GlobalQueryableFilterWithResultLimitTests : GlobalQueryableFilterWithoutResultLimitTests
    {
        public GlobalQueryableFilterWithResultLimitTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        internal static void UpdateConfiguration1(WebRouteConfiguration configuration)
        {
            configuration.AddODataQueryFilter(new EnableQueryAttribute() { PageSize = 3 });
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.EnableDependencyInjection();
        }
    }

    public class GlobalQueryableFilterWithDerivedEnableQueryAttribute : WebHostTestBase
    {
        public GlobalQueryableFilterWithDerivedEnableQueryAttribute(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddODataQueryFilter(new DerivedQueryableAttribute());
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.EnableDependencyInjection();
        }

        [Theory]
        [InlineData("/api/GlobalQueryableFilter/GetQueryable")]
        [InlineData("/api/GlobalQueryableFilter/GetQueryableT")]
        [InlineData("/api/GlobalQueryableFilter/GetQueryableTWithDerivedQAttrResultLimit")]
        [InlineData("/api/GlobalQueryableFilter/GetTaskIQueryableT")]
        public async Task TestQueryableWorksUnderGlobalFilter(string url)
        {
            var response = await this.Client.GetAsync(this.BaseAddress + url + "?$top=4");
            var actual = await response.Content.ReadAsObject<IEnumerable<Product>>();

            Assert.Equal(3, actual.Count());
        }

        [Theory]
        [InlineData("/api/GlobalQueryableFilter/GetQueryable?$top=4&$customQuery=1")]
        public async Task TestCustomQueryWorksUnderGlobalFilter(string url)
        {
            var response = await this.Client.GetAsync(this.BaseAddress + url);
            var actual = await response.Content.ReadAsObject<IEnumerable<Product>>();

            Assert.Equal(3, actual.Count());
        }

        [Theory]
        [InlineData("/api/GlobalQueryableFilter/GetQueryableTWithDerivedOptions")]
        [InlineData("/api/GlobalQueryableFilter/GetQueryableTWithOptions")]
        [InlineData("/api/GlobalQueryableFilter/GetEnumerableT")]
        [InlineData("/api/GlobalQueryableFilter/GetHttpResponseMessage")]
        public async Task TestActionsThatAreIgnoredByGlobalFilter(string url)
        {
            var response = await this.Client.GetAsync(this.BaseAddress + url + "?$top=4");
            var actual = await response.Content.ReadAsObject<IEnumerable<Product>>();

            Assert.NotEqual(4, actual.Count());
        }

        [Theory]
        [InlineData("/api/GlobalQueryableFilter/GetProductWithDerivedQAttr")]
        public async Task TestActionsThatNotAllowedByQueryableAttribute(string url)
        {
            var response = await this.Client.GetAsync(this.BaseAddress + url + "?$top=1");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            Assert.Contains("The requested resource is not a collection.", await response.Content.ReadAsStringAsync());
        }
    }
}
