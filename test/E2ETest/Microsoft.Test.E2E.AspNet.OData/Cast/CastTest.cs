//-----------------------------------------------------------------------------
// <copyright file="CastTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Cast
{
    public class CastTest : WebHostTestBase
    {
        private static string[] dataSourceTypes = new string[] { "IM", "EF" };// In Memory and Entity Framework

        public CastTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(ProductsController), typeof(MetadataController) };
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);

            IEdmModel edmModel = CastEdmModel.GetEdmModel(configuration);
            foreach (string dataSourceType in dataSourceTypes)
            {
                configuration.MapODataServiceRoute(dataSourceType, dataSourceType, edmModel);
            }
            configuration.EnsureInitialized();
        }

        public static TheoryDataSet<string, string, int> Combinations
        {
            get
            {
                var combinations = new TheoryDataSet<string, string, int>();
                foreach (string dataSourceType in dataSourceTypes)
                {
                    // To Edm.String
                    combinations.Add(dataSourceType, "?$filter=cast('Name1',Edm.String) eq Name", 1);
                    combinations.Add(dataSourceType, "?$filter=contains(cast(Name,Edm.String),'Name')", 6);
                    combinations.Add(dataSourceType, "?$filter=cast(Microsoft.Test.E2E.AspNet.OData.Cast.Domain'Civil',Edm.String) eq '2'", 6);
                    combinations.Add(dataSourceType, "?$filter=cast(Domain,Edm.String) eq '3'", 2);
                    combinations.Add(dataSourceType, "?$filter=cast(ID,Edm.String) gt '1'", 5);
                    // TODO bug 1889: Cast function reports error if it is used against a collection of primitive value.
                    // Delete $it after the bug if fixed.
                    combinations.Add(dataSourceType, "(1)/DimensionInCentimeter?$filter=cast($it,Edm.String) gt '1'", 2);
                    combinations.Add(dataSourceType, "?$filter=cast(Weight,Edm.String) gt '1.1'", 5);
                    combinations.Add(dataSourceType, "?$filter=contains(cast(ManufacturingDate,Edm.String),'2011')", 1);
                    // TODO bug 1982: The result of casting a value of DateTimeOffset to String is not always the literal representation used in payloads
                    // combinations.Add(dataSourceType, "?$filter=contains(cast(2011-01-01T00:00:00%2B08:00,Edm.String),'2011-01-01')", 3);

                    // To Edm.Int32
                    combinations.Add(dataSourceType, "?$filter=cast(Weight,Edm.Int32) eq 1", 1);
                    combinations.Add(dataSourceType, "?$filter=cast(cast(Name,Edm.Int32),Edm.Int32) eq null", 6);

                    // To DateTimeOffset
                    combinations.Add(dataSourceType, "?$filter=cast(ManufacturingDate,Edm.DateTimeOffset) eq 2011-01-01T00:00:00%2B08:00", 1);
                    combinations.Add(dataSourceType, "?$filter=cast(null,Edm.DateTimeOffset) eq null", 6);

                    // To Enum
                    combinations.Add(dataSourceType, "?$filter=cast('Both',Microsoft.Test.E2E.AspNet.OData.Cast.Domain) eq Domain", 2);
                    combinations.Add(dataSourceType, "?$filter=cast('1',Microsoft.Test.E2E.AspNet.OData.Cast.Domain) eq Domain", 2);
                    combinations.Add(dataSourceType, "?$filter=cast(null,Microsoft.Test.E2E.AspNet.OData.Cast.Domain) eq Domain", 0);

                    //To Derived Structured Types
                    combinations.Add(dataSourceType, "?$filter=cast('Microsoft.Test.E2E.AspNet.OData.Cast.AirPlane')/Speed eq 100", 2);
                    combinations.Add(dataSourceType, "?$filter=cast('Microsoft.Test.E2E.AspNet.OData.Cast.AirPlane')/Speed eq 500", 1);
                    combinations.Add(dataSourceType, "?$filter=cast('Microsoft.Test.E2E.AspNet.OData.Cast.JetPlane')/Company eq 'Boeing'", 1);
                }

                return combinations;
            }
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task Query(string dataSourceMode, string dollarFormat, int expectedEntityCount)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}/Products{2}", this.BaseAddress, dataSourceMode, dollarFormat);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            JObject responseString = await response.Content.ReadAsObject<JObject>();

            // Assert
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                string.Format("Response status code, expected: {0}, actual: {1}, request url: {2}",
                HttpStatusCode.OK, response.StatusCode, requestUri));

            JArray value = responseString["value"] as JArray;
            Assert.True(expectedEntityCount == value.Count,
                string.Format("The entity count in response, expected: {0}, actual: {1}, request url: {2}",
                expectedEntityCount, value.Count, requestUri));
        }
    }

    public class AdvancedCastTest : WebHostTestBase
    {
        public AdvancedCastTest(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(ItemsController) };
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);

            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Item>("Items");
            IEdmModel model = modelBuilder.GetEdmModel();

            configuration.MapODataServiceRoute("nullpropagation", "nullpropagation", model);
            configuration.MapODataServiceRoute("nonnullpropagation", "nonnullpropagation", model);
            configuration.MapODataServiceRoute("maxfunctioncalldepth", "maxfunctioncalldepth", model);
            configuration.MapODataServiceRoute("defaultenablequery", "defaultenablequery", model);
            configuration.MapODataServiceRoute("reconfigedenablequery", "reconfigedenablequery", model);
            configuration.EnsureInitialized();
        }

        [Theory]
        [InlineData(4, true)]
        [InlineData(6, true)]
        [InlineData(8, true)]
        [InlineData(10, true)]
        [InlineData(15, true)]
        [InlineData(16, false)]
        [InlineData(20, false)]
        public async Task UseNestedCastForDifferentDepthWorksFine_ForNullPropagation(int depth, bool success)
        {
            // Arrange
            string expr = GetCastExpr(depth); // "cast(....cast(cast(Name eq 'x',Edm.String) eq 'x',Edm.String))))"
            string queryUrl = $"{this.BaseAddress}/nullpropagation/Items?$filter={expr} eq 'x'";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpResponseMessage response;

            // Act
            response = await Client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            var payload = await response.Content.ReadAsStringAsync();

            if (success)
            {
#if NETCORE
                Assert.Equal(HttpStatusCode.OK , response.StatusCode);
#else
                Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode); // no result from content negotiation indicates that 406 should be sent..
#endif
            }
            else
            {
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Contains(Error.Format(SRResources.MaxFunctionCallDepthExceeded, 15, "MaxFunctionCallDepth"), payload);
            }
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(5, true)]
        [InlineData(6, true)]
        [InlineData(7, true)]
        [InlineData(15, true)]
        [InlineData(16, false)]
        [InlineData(50, false)]
        public async Task UseNestedCastForDifferentDepthWorksFine_ForNonNullPropagation(int depth, bool success)
        {
            // Arrange
            string expr = GetCastExpr(depth);
            string filterStr = expr + " eq 'x'"; // "cast(....cast(cast(Name eq 'x',Edm.String) eq 'x',Edm.String)))) eq 'x'"

            string queryUrl = $"{this.BaseAddress}/nonnullpropagation/Items?$filter={filterStr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);
            var payload = await response.Content.ReadAsStringAsync();

            if (success)
            {
                // Assert
                Assert.NotNull(response);
#if NETCORE
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
#else
                Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);
#endif
                Assert.NotNull(response.Content);
            }
            else
            {
                Assert.Contains(Error.Format(SRResources.MaxFunctionCallDepthExceeded, 15, "MaxFunctionCallDepth"), payload);
            }
        }

        private static string GetCastExpr(int depth)
        {
            string expr = $"Name eq 'x'";
            for (int i = 0; i < depth; i++)
            {
                expr = $"cast({expr},Edm.String)";
            }
            return expr;
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        // Each depth here produces two nested function calls (contains + cast), so depth 7 = 14 function calls (within the default MaxFunctionCallDepth of 15).
        public async Task UseNestedCastWithNestedContainsForDifferentDepth_ReturnsAsExpected_ForNullPropagation(int depth)
        {
            // Arrange
            string expr = BuildMixedExpression(depth); // contains('a', cast(contains('a', .....('a', cast(Name eq 'x', Edm.String)), Edm.String)), Edm.String)), Edm.String))

            string queryUrl = $"{this.BaseAddress}/nullpropagation/Items?$filter={expr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpResponseMessage response;

            // Act
            response = await Client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
#if NETCORE
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
#else
            Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);
#endif
            Assert.NotNull(response.Content);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(5, true)]
        [InlineData(6, true)]
        [InlineData(7, true)]
        [InlineData(8, false)]
        [InlineData(12, false)]
        [InlineData(20, false)]
        public async Task UseNestedCastWithNestedContainsForDifferentDepth_ReturnsAsExpected_ForNonNullPropagation(int depth, bool success)
        {
            // Arrange
            string expr = BuildMixedExpression(depth); // contains('a', cast(contains('a', .....('a', cast(Name eq 'x', Edm.String)), Edm.String)), Edm.String)), Edm.String))

            string queryUrl = $"{this.BaseAddress}/nonnullpropagation/Items?$filter={expr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.NotNull(response);
            var payload = await response.Content.ReadAsStringAsync();

            if (success)
            {
                // Assert
#if NETCORE
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
#else
                Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);
#endif
                Assert.NotNull(response.Content);
            }
            else
            {
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Contains(Error.Format(SRResources.MaxFunctionCallDepthExceeded, 15, "MaxFunctionCallDepth"), payload);
            }
        }

        [Theory]
        [InlineData(4, true)]
        [InlineData(6, true)]
        [InlineData(8, true)]
        [InlineData(10, true)]
        [InlineData(12, true)]
        [InlineData(18, false)]
        [InlineData(20, false)]
        public async Task UseNestedCastWithNestedContainsWithMaxFunctionCallDepthReconfigurationForDifferentDepthWorksFine(int depth, bool success)
        {
            // Arrange
            string expr = BuildMixedExpression(depth);

            string queryUrl = $"{this.BaseAddress}/maxfunctioncalldepth/Items?$filter={expr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.NotNull(response);

            if (success)
            {
                // Assert
#if NETCORE
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
#else
                Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);
#endif
                Assert.NotNull(response.Content);
            }
            else
            {
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                var payload = await response.Content.ReadAsStringAsync();
                Assert.Contains(Error.Format(SRResources.MaxFunctionCallDepthExceeded, 30, "MaxFunctionCallDepth"), payload);
            }
        }

        [Fact]
        public async Task UseVeryDeepDepthWithMaxFunctionCallDepthReconfiguration_ThrowsFromODataLibraryFirst()
        {
            // Arrange
            string expr = BuildMixedExpression(50);

            string queryUrl = $"{this.BaseAddress}/maxfunctioncalldepth/Items?$filter={expr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            var response = await Client.SendAsync(request);
            var payload = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("Recursion depth exceeded allowed limit.", payload);
        }

        [Theory]
        [InlineData(4, HttpStatusCode.OK)]
        [InlineData(6, HttpStatusCode.OK)]
        [InlineData(7, HttpStatusCode.OK)]
        [InlineData(8, HttpStatusCode.BadRequest)]
        [InlineData(10, HttpStatusCode.BadRequest)]
        public async Task UseDifferentDepth_OnDefaultEnableQuery_ReturnsResponseAsExpected(int depth, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            string expr = BuildMixedExpression(depth);

            string queryUrl = $"{this.BaseAddress}/defaultenablequery/Items?$filter={expr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            var response = await Client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(expectedStatusCode, response.StatusCode);
            if (expectedStatusCode == HttpStatusCode.BadRequest)
            {
                string payload = await response.Content.ReadAsStringAsync();
                Assert.Contains(Error.Format(SRResources.MaxFunctionCallDepthExceeded, 15, "MaxFunctionCallDepth"), payload);
            }
        }

        [Theory]
        [InlineData(4, HttpStatusCode.OK)]
        [InlineData(6, HttpStatusCode.OK)]
        [InlineData(7, HttpStatusCode.OK)]
        [InlineData(8, HttpStatusCode.OK)]
        [InlineData(10, HttpStatusCode.OK)]
        [InlineData(18, HttpStatusCode.OK)]
        [InlineData(19, HttpStatusCode.OK)]
        [InlineData(20, HttpStatusCode.BadRequest)] // Even though MaxFunctionCallDepth is 100, ODL rejects this first with 'The node count limit of '100' has been exceeded' (default MaxNodeCount = 100).
        [InlineData(50, HttpStatusCode.BadRequest)]
        public async Task UseDifferentDepth_OnReConfiguredEnableQuery_ReturnsResponseAsExpected(int depth, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            string expr = BuildMixedExpression(depth);

            string queryUrl = $"{this.BaseAddress}/reconfigedenablequery/Items?$filter={expr}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            var response = await Client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(expectedStatusCode, response.StatusCode);
            if (expectedStatusCode == HttpStatusCode.BadRequest)
            {
                string payload = await response.Content.ReadAsStringAsync();

                if (depth < 50)
                    Assert.Contains("node count limit", payload);
                else
                    Assert.Contains("Recursion depth exceeded allowed limit.", payload);
            }
        }

        // Note: each depth here produces two nested function calls (contains + cast). So depth 4 = 8 function calls.
        private static string BuildMixedExpression(int depth)
        {
            string expr = "Name eq 'x'";
            for (int i = 0; i < depth; i++)
            {
                expr = $"contains('a',cast({expr},Edm.String))";
            }

            return expr;
        }
    }

    public class ItemsController : TestODataController
    {
        private static readonly List<Item> _items = Enumerable.Range(1, 10).Select(i => new Item { Id = i, Name = $"Item-{i}" }).ToList();

        [Common.Controllers.HttpGet]
        [ODataRoute("Items", RouteName = "nullpropagation")]
        public ITestActionResult GetItemsNullPropagation(ODataQueryOptions<Item> queryOptions)
        {
            return Ok(queryOptions.ApplyTo(_items.AsQueryable())); // the default is true for null propagation for LINQ-To-Object.
        }

        [Common.Controllers.HttpGet]
        [ODataRoute("Items", RouteName = "nonnullpropagation")]
        public ITestActionResult GetItemsNonNullPropagation(ODataQueryOptions<Item> queryOptions)
        {
            return Ok(queryOptions.ApplyTo(_items.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False }));
        }

        [Common.Controllers.HttpGet]
        [ODataRoute("Items", RouteName = "maxfunctioncalldepth")]
        public ITestActionResult GetItemsMaxFunctionCallDepth(ODataQueryOptions<Item> queryOptions)
        {
            // It's developer's responsibility to call 'Validate()' before applying the query to data source to make sure the query is valid and safe to execute.
            int maxFunctionCallDepth = 30;
            queryOptions.Validate(new ODataValidationSettings
            {
                MaxFunctionCallDepth = maxFunctionCallDepth
            });

            return Ok(queryOptions.ApplyTo(_items.AsQueryable(), new ODataQuerySettings { MaxFunctionCallDepth = maxFunctionCallDepth, HandleNullPropagation = HandleNullPropagationOption.False }));
        }

        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        [Common.Controllers.HttpGet]
        [ODataRoute("Items", RouteName = "defaultenablequery")]
        public ITestActionResult GetItemsUsingDefaultEnableQuery()
        {
            return Ok(_items.AsQueryable());
        }

        [EnableQuery(MaxFunctionCallDepth = 100, HandleNullPropagation = HandleNullPropagationOption.False)]
        [Common.Controllers.HttpGet]
        [ODataRoute("Items", RouteName = "reconfigedenablequery")]
        public ITestActionResult GetItemsUsingEnableQuery()
        {
            return Ok(_items.AsQueryable());
        }
    }

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}
