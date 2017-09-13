// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Models.ProductFamilies;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public class DerivedQueryableAttribute : EnableQueryAttribute
    {
        public DerivedQueryableAttribute()
        {
            this.PageSize = 3;
        }

        public override void ValidateQuery(HttpRequestMessage request, ODataQueryOptions queryOptions)
        {
            //base.ValidateQuery(request, queryOptions);
        }
    }

    public class DerivedODataQueryOptions : ODataQueryOptions
    {
        public DerivedODataQueryOptions(ODataQueryContext context, HttpRequestMessage request)
            : base(context, request)
        {
        }
    }

    public class GlobalQueryableFilterController : ApiController
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

        public HttpResponseMessage GetHttpResponseMessage()
        {
            return this.Request.CreateResponse<IQueryable<Product>>(HttpStatusCode.OK, products.AsQueryable());
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

    public class DerivedEnitySetController : ODataController
    {
        [DerivedQueryable]
        public IQueryable<Product> Get()
        {
            return GlobalQueryableFilterController.Products.AsQueryable();
        }
    }

    public class GlobalQueryableFilterWithoutResultLimitTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
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
        public void TestQueryableWorksUnderGlobalFilter(string url)
        {
            var response = this.Client.GetAsync(this.BaseAddress + url + "?$top=1").Result;
            var actual = response.Content.ReadAsAsync<IEnumerable<Product>>().Result;

            Assert.Equal(1, actual.Count());
        }
        [Theory]
        [InlineData("/api/GlobalQueryableFilter/GetQueryableTWithDerivedOptions")]
        [InlineData("/api/GlobalQueryableFilter/GetQueryableTWithOptions")]
        [InlineData("/api/GlobalQueryableFilter/GetEnumerableT")]
        [InlineData("/api/GlobalQueryableFilter/GetHttpResponseMessage")]
        public void TestActionsThatAreIgnoredByGlobalFilter(string url)
        {
            var response = this.Client.GetAsync(this.BaseAddress + url + "?$top=1").Result;
            var actual = response.Content.ReadAsAsync<IEnumerable<Product>>().Result;

            Assert.NotEqual(1, actual.Count());
        }

        [Theory]
        [InlineData("/api/GlobalQueryableFilter/GetProductWithQAttr")]
        public void TestActionsThatNotAllowedByQueryableAttribute(string url)
        {
            var response = this.Client.GetAsync(this.BaseAddress + url + "?$top=1").Result;
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            Assert.Contains("The requested resource is not a collection.", response.Content.ReadAsStringAsync().Result);
        }

        [Theory]
        [InlineData("/api/GlobalQueryableFilter/GetQueryableTWithResultLimit")]
        public virtual void TestQueryableAttributeShouldWinGlobalQueryableFilter(string url)
        {
            var response = this.Client.GetAsync(this.BaseAddress + url).Result;
            var actual = response.Content.ReadAsAsync<IEnumerable<Product>>().Result;

            Assert.Equal(5, actual.Count());
        }

        [Theory]
        [InlineData("/api/GlobalQueryableFilter/GetProduct")]
        public virtual void TestQueryableAttributeWontAffectOtherActions(string url)
        {
            var response = this.Client.GetAsync(this.BaseAddress + url).Result;
            response.EnsureSuccessStatusCode();
        }
    }

    public class GlobalQueryableFilterWithResultLimitTests : GlobalQueryableFilterWithoutResultLimitTests
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration1(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.AddODataQueryFilter(new EnableQueryAttribute() { PageSize = 3 });
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.EnableDependencyInjection();
        }
    }

    public class GlobalQueryableFilterWithDerivedEnableQueryAttribute : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.AddODataQueryFilter(new DerivedQueryableAttribute());
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.EnableDependencyInjection();
        }

        [Theory]
        [InlineData("/api/GlobalQueryableFilter/GetQueryable")]
        [InlineData("/api/GlobalQueryableFilter/GetQueryable")]
        [InlineData("/api/GlobalQueryableFilter/GetQueryableT")]
        [InlineData("/api/GlobalQueryableFilter/GetQueryableTWithDerivedQAttrResultLimit")]
        [InlineData("/api/GlobalQueryableFilter/GetTaskIQueryableT")]
        public void TestQueryableWorksUnderGlobalFilter(string url)
        {
            var response = this.Client.GetAsync(this.BaseAddress + url + "?$top=4").Result;
            var actual = response.Content.ReadAsAsync<IEnumerable<Product>>().Result;

            Assert.Equal(3, actual.Count());
        }

        [Theory]
        [InlineData("/api/GlobalQueryableFilter/GetQueryable?$top=4&$customQuery=1")]
        public void TestCustomQueryWorksUnderGlobalFilter(string url)
        {
            var response = this.Client.GetAsync(this.BaseAddress + url).Result;
            var actual = response.Content.ReadAsAsync<IEnumerable<Product>>().Result;

            Assert.Equal(3, actual.Count());
        }

        [Theory]
        [InlineData("/api/GlobalQueryableFilter/GetQueryableTWithDerivedOptions")]
        [InlineData("/api/GlobalQueryableFilter/GetQueryableTWithOptions")]
        [InlineData("/api/GlobalQueryableFilter/GetEnumerableT")]
        [InlineData("/api/GlobalQueryableFilter/GetHttpResponseMessage")]
        public void TestActionsThatAreIgnoredByGlobalFilter(string url)
        {
            var response = this.Client.GetAsync(this.BaseAddress + url + "?$top=4").Result;
            var actual = response.Content.ReadAsAsync<IEnumerable<Product>>().Result;

            Assert.NotEqual(4, actual.Count());
        }

        [Theory]
        [InlineData("/api/GlobalQueryableFilter/GetProductWithDerivedQAttr")]
        public void TestActionsThatNotAllowedByQueryableAttribute(string url)
        {
            var response = this.Client.GetAsync(this.BaseAddress + url + "?$top=1").Result;
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            Assert.Contains("The requested resource is not a collection.", response.Content.ReadAsStringAsync().Result);
        }
    }
}
