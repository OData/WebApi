// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ActionResult
{
    /// <summary>
    /// NetCore 2.2+ <see cref="Microsoft.AspNetCore.Mvc.ActionResult{TValue}"/> was introduced. EnableQuery attribute works correctly.
    /// </summary>
    public class ActionResultTests : WebHostTestBase
    {
        private const string BaseUrl = "{0}/api/Customers";

        public ActionResultTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(CustomersController));
            configuration.JsonReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("actionresult", "actionresult",
                ActionResultEdmModel.GetEdmModel(configuration));

            configuration.MapHttpRoute("api", "api/{controller}", new { controller = "CustomersController", action = "GetCustomers" });
            configuration.EnableDependencyInjection();
        }

        /// <summary>
        /// For Non-OData json based paths. EnableQuery should work.
        /// </summary>
        /// <returns>Task tracking operation.</returns>
        [Fact]
        public async Task ActionResultNonODataPathReturnsExpansion()
        {
            // Arrange
            string queryUrl = string.Format(BaseUrl + "?$expand=books", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<Customer> customers = JsonConvert.DeserializeObject<List<Customer>>(await response.Content.ReadAsStringAsync());

            Assert.Single(customers);
            Assert.Equal("CustId", customers.First().Id);
            Assert.Single(customers.First().Books);
            Assert.Equal("BookId", customers.First().Books.First().Id);
        }

        /// <summary>
        /// For OData paths enable query should work with expansion.
        /// </summary>
        /// <returns>Task tracking operation.</returns>
        [Fact]
        public async Task ActionResultODataPathReturnsExpansion()
        {
            // Arrange
            string queryUrl = string.Format("{0}/actionresult/Customers?$expand=books", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<Customer> customers = JToken.Parse(await response.Content.ReadAsStringAsync())["value"].ToObject<List<Customer>>();

            Assert.Single(customers);
            Assert.Equal("CustId", customers.First().Id);
            Assert.Single(customers.First().Books);
            Assert.Equal("BookId", customers.First().Books.First().Id);
        }

        /// <summary>
        /// For OData paths enable query should work without expansion.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActionResultODataPathReturnsBaseWithoutExpansion()
        {
            // Arrange
            string queryUrl = string.Format("{0}/actionresult/Customers", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<Customer> customers = JToken.Parse(await response.Content.ReadAsStringAsync())["value"].ToObject<List<Customer>>();

            Assert.Single(customers);
            Assert.Equal("CustId", customers.First().Id);
            Assert.Null(customers.First().Books);
        }
    }
}
