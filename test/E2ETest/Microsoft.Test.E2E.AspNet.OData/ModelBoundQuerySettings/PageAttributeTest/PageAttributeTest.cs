// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.PageAttributeTest
{
    public class PageAttributeTest : WebHostTestBase
    {
        private const string CustomerBaseUrl = "{0}/enablequery/Customers";
        private const string OrderBaseUrl = "{0}/enablequery/Orders";
        private const string ModelBoundCustomerBaseUrl = "{0}/modelboundapi/Customers";
        private const string ModelBoundOrderBaseUrl = "{0}/modelboundapi/Orders";

        public PageAttributeTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Services.Replace(
                typeof (IAssembliesResolver),
                new TestAssemblyResolver(typeof(CustomersController), typeof(OrdersController)));
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.MaxTop(2).Expand();
            configuration.MapODataServiceRoute("enablequery", "enablequery",
                PageAttributeEdmModel.GetEdmModel());
            configuration.MapODataServiceRoute("modelboundapi", "modelboundapi",
                PageAttributeEdmModel.GetEdmModelByModelBoundAPI());
        }

        [Theory]
        [InlineData(OrderBaseUrl + "?$top=3", 2)]
        [InlineData(ModelBoundOrderBaseUrl + "?$top=3", 2)]
        public async Task DefaultMaxTop(string url, int maxTop)
        {
            // If there is no attribute on type then the page is disabled, 
            // MaxTop is 0 or the value set in DefaultQuerySetting.
            string queryUrl =
                string.Format(
                    url,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(string.Format("The limit of '{0}' for Top query has been exceeded", maxTop), result);
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "?$top=10")]
        [InlineData(OrderBaseUrl + "/Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.PageAttributeTest.SpecialOrder?$top=10")]
        [InlineData(ModelBoundCustomerBaseUrl + "?$top=10")]
        [InlineData(ModelBoundOrderBaseUrl + "/Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.PageAttributeTest.SpecialOrder?$top=10")]
        public async Task MaxTopOnEnitityType(string url)
        {
            // MaxTop on entity type
            string queryUrl =
                string.Format(
                    url,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The limit of '5' for Top query has been exceeded", result);
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "?$expand=Orders($top=3)", (int)HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl + "?$expand=Customers($top=10)", (int)HttpStatusCode.OK)]
        [InlineData(CustomerBaseUrl + "(1)/Orders?$top=3", (int)HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl + "(1)/Customers?$top=10", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl + "?$expand=Orders($top=3)", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl + "?$expand=Customers($top=10)", (int)HttpStatusCode.OK)]
        [InlineData(ModelBoundCustomerBaseUrl + "(1)/Orders?$top=3", (int)HttpStatusCode.BadRequest)]
        [InlineData(ModelBoundOrderBaseUrl + "(1)/Customers?$top=10", (int)HttpStatusCode.OK)]
        public async Task MaxTopOnProperty(string url, int statusCode)
        {
            // MaxTop on property override on entity type
            string queryUrl =
                string.Format(
                    url,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(statusCode, (int)response.StatusCode);
            if (statusCode == (int)HttpStatusCode.BadRequest)
            {
                Assert.Contains("The limit of '2' for Top query has been exceeded", result);
            }
        }

        [Theory]
        [InlineData(CustomerBaseUrl)]
        [InlineData(ModelBoundCustomerBaseUrl)]
        public async Task PageSizeOnEntityType(string url)
        {
            string queryUrl = string.Format(url, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(string.Format(url, "") + "?$skip=1", result);
        }

        [Theory]
        [InlineData(CustomerBaseUrl, "?$expand=Orders")]
        [InlineData(CustomerBaseUrl, "(1)/Orders")]
        [InlineData(ModelBoundCustomerBaseUrl, "?$expand=Orders")]
        [InlineData(ModelBoundCustomerBaseUrl, "(1)/Orders")]
        public async Task PageSizeOnProperty(string url, string expand)
        {
            string queryUrl = string.Format(url + expand, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Orders?$skip=1", result);
        }
    }
}