//-----------------------------------------------------------------------------
// <copyright file="MatchesPatternTimeoutTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class MatchesPatternTimeoutTests : IDisposable
    {
        private readonly TestServer _server;
        private static readonly MatchesPatternProduct[] Products =
        {
            new MatchesPatternProduct { Id = 1, Name = "Alpha" },
            new MatchesPatternProduct { Id = 2, Name = "Beta" },
            new MatchesPatternProduct { Id = 3, Name = "Gamma" },
            new MatchesPatternProduct { Id = 4, Name = "Alabama" },
            new MatchesPatternProduct { Id = 5, Name = new string('a', 40) + "!" }
        };

        public MatchesPatternTimeoutTests()
        {
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<MatchesPatternProduct>("Products");
            builder.EntitySet<MatchesPatternProduct>("BoundedProducts");
            builder.EntitySet<MatchesPatternProduct>("DefaultBoundedProducts");
            builder.EntitySet<MatchesPatternProduct>("AttributeBoundedProducts");
            builder.EntitySet<MatchesPatternProduct>("ClampedProducts");
            var model = builder.GetEdmModel();
            _server = TestServerFactory.Create(
                new[] { typeof(ProductsController), typeof(BoundedProductsController), typeof(DefaultBoundedProductsController), typeof(AttributeBoundedProductsController), typeof(ClampedProductsController) },
                routes =>
                {
                    routes.Filter().OrderBy().Select().Count();
                    routes.MapODataServiceRoute("odata", "odata", model);
                });
        }

        [Theory]
        [InlineData("Products", "matchesPattern(Name,'^Al')", 1, 4)]
        [InlineData("BoundedProducts", "matchesPattern(Name,'^Al')", 1, 4)]
        [InlineData("DefaultBoundedProducts", "matchesPattern(Name,'^Al')", 1, 4)]
        [InlineData("AttributeBoundedProducts", "matchesPattern(Name,'^Al')", 1, 4)]
        [InlineData("ClampedProducts", "matchesPattern(Name,'^Al')", 1, 4)]
        [InlineData("BoundedProducts", "matchesPattern(Name,'^Zzz')")]
        [InlineData("BoundedProducts", "startswith(Name,'Be')", 2)]
        public async Task MatchesPatternTimeout_BenignQueriesReturnExpectedProducts(string entitySet, string filter, params int[] expectedIds)
        {
            using (var client = _server.CreateClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, "odata/" + entitySet + "?$filter=" + filter))
            {
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
                using (var response = await client.SendAsync(request))
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    var result = Assert.IsType<JArray>(JObject.Parse(await response.Content.ReadAsStringAsync())["value"]);
                    Assert.Equal(expectedIds.Length, result.Count);
                    for (int i = 0; i < expectedIds.Length; i++)
                    {
                        Assert.Equal(expectedIds[i], (int)result[i]["Id"]);
                    }
                }
            }
        }

        [Theory]
        [InlineData("BoundedProducts")]
        [InlineData("DefaultBoundedProducts")]
        [InlineData("AttributeBoundedProducts")]
        public async Task MatchesPatternTimeout_BacktrackingReturnsBadRequest(string entitySet)
        {
            using (var client = _server.CreateClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, "odata/" + entitySet + "?$filter=matchesPattern(Name,'(a%2B)%2B$')"))
            {
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
                using (var response = await client.SendAsync(request))
                {
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                }
            }
        }

        [Fact]
        public async Task MatchesPatternTimeout_PagelessBacktrackingSurfacesDuringSerialization()
        {
            // Without paging the predicate runs after EnableQuery, during response serialization.
            using (var client = _server.CreateClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, "odata/Products?$filter=matchesPattern(Name,'(a%2B)%2B$')"))
            {
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
                await Assert.ThrowsAsync<HttpRequestException>(() => client.SendAsync(request));
            }
        }

        [Fact]
        public async Task MatchesPatternTimeout_PagelessWithoutFilterReturnsAllProducts()
        {
            using (var client = _server.CreateClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, "odata/Products"))
            {
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
                using (var response = await client.SendAsync(request))
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    var result = Assert.IsType<JArray>(JObject.Parse(await response.Content.ReadAsStringAsync())["value"]);
                    Assert.Equal(5, result.Count);
                }
            }
        }

        public void Dispose()
        {
            _server.Dispose();
        }

        public class MatchesPatternProduct
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class ProductsController : ODataController
        {
            [EnableQuery]
            public IActionResult Get()
            {
                return Ok(Products);
            }
        }

        public class BoundedProductsController : ODataController
        {
            [BoundedEnableQuery]
            public IActionResult Get()
            {
                return Ok(Products);
            }
        }

        public class DefaultBoundedProductsController : ODataController
        {
            [EnableQuery(PageSize = 100)]
            public IActionResult Get()
            {
                return Ok(Products);
            }
        }

        public class AttributeBoundedProductsController : ODataController
        {
            [EnableQuery(MatchesPatternTimeoutMilliseconds = 100, PageSize = 100)]
            public IActionResult Get()
            {
                return Ok(Products);
            }
        }

        public class ClampedProductsController : ODataController
        {
            [ClampedEnableQuery]
            public IActionResult Get()
            {
                return Ok(Products);
            }
        }

        private sealed class BoundedEnableQueryAttribute : EnableQueryAttribute
        {
            public BoundedEnableQueryAttribute()
            {
                MatchesPatternTimeout = TimeSpan.FromMilliseconds(100);
                PageSize = 100;
            }
        }

        private sealed class ClampedEnableQueryAttribute : EnableQueryAttribute
        {
            public ClampedEnableQueryAttribute()
            {
                MatchesPatternTimeout = TimeSpan.FromDays(30);
                PageSize = 100;
            }
        }
    }
}
