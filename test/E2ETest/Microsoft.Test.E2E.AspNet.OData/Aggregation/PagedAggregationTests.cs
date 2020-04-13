// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Aggregation
{
    public class PagedAggregationTests : WebHostTestBase<PagedAggregationTests>
    {
        private const string AggregationTestBaseUrl = "{0}/pagedaggregation/Customers";

        public PagedAggregationTests(WebHostTestFixture<PagedAggregationTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof (Paged.CustomersController));
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("pagedaggregation", "pagedaggregation",
                AggregationEdmModel.GetEdmModel(configuration));
        }

        [Theory]
        [InlineData("?$apply=groupby((Order/Name), aggregate(Id with sum as TotalId))" +
                    "/filter(Order/Name ne 'Order0')&$orderby=Order/Name")]
        [InlineData("?$apply=groupby((Order/Name), aggregate(Id with sum as TotalId))" +
                    "&$filter=Order/Name ne 'Order0'&$orderby=Order/Name")]
        public async Task PagedAggregationWorks(string query)
        {
            // Arrange
            string queryUrl =
                string.Format(
                    AggregationTestBaseUrl + query,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            var result = await response.Content.ReadAsObject<JObject>();
            System.Console.WriteLine(result);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Single(results);
        }
    }
}
