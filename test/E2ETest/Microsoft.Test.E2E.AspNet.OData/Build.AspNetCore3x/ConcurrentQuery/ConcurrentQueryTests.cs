// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ConcurrentQuery
{
    /// <summary>
    /// Ensures that concurrent execution of EnableQuery is thread-safe.
    /// </summary>
    public class ConcurrentQueryTests : WebHostTestBase
    {
        public ConcurrentQueryTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(CustomersController));
            configuration.JsonReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("concurrentquery", "concurrentquery",
                ConcurrentQueryEdmModel.GetEdmModel(configuration));

            configuration.EnableDependencyInjection();
        }

        /// <summary>
        /// For OData paths enable query should work with expansion.
        /// </summary>
        /// <returns>Task tracking operation.</returns>
        [Fact]
        public async Task ConcurrentQueryExecutionIsThreadSafe()
        {
            // Arrange
            HttpClient client = new HttpClient();

            // Bumping thread count to allow higher parallelization.
            ThreadPool.SetMinThreads(100, 100);

            // Act
            var results = await Task.WhenAll(
                Enumerable.Range(1, 100)
                .Select(async i =>
                {
                    string queryUrl = string.Format("{0}/concurrentquery/Customers?$filter=Id gt {1}", BaseAddress, i);
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
                    request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

                    HttpResponseMessage response = await client.SendAsync(request);

                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    List<Customer> customers = JToken.Parse(await response.Content.ReadAsStringAsync())["value"].ToObject<List<Customer>>();

                    return (i: i, length: customers.Count);
                }));

            // Assert
            foreach (var result in results)
            {
                Assert.Equal(100 - result.i, result.length);
            }
        }
    }
}
