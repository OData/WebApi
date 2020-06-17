// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Endpoint
{
    public class EndpointEfCoreTests : EndpointTestBase<EndpointEfCoreTests>
    {
        public static IEdmModel EdmModel;

        private const string CustomersBaseUrl = "{0}/odata/EpCustomers";

        public EndpointEfCoreTests(EndpointTestFixture<EndpointEfCoreTests> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            // Add your custom services
            services.AddDbContext<EndpointDbContext>(opt =>
                opt.UseLazyLoadingProxies().UseInMemoryDatabase("EndpointCustomerOrderList"));
        }

        protected static void UpdateConfigure(EndpointRouteConfiguration configuration)
        {
            // Add your custom routes
            configuration.AddControllers(typeof(EpCustomersController));

            configuration.MaxTop(2).Expand().Select().OrderBy().Filter();

            EdmModel = EndpointModelGenerator.GetConventionalEdmModel();
            configuration.MapODataRoute("odata", "odata",
                EdmModel,
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
                new DefaultODataBatchHandler());
        }

        [Fact]
        public async Task QueryEntitySetUsingEndpointRoutingWorks()
        {
            // Arrange: GET ~/odata/EpCustomers
            string requestUri = string.Format(CustomersBaseUrl, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            HttpResponseMessage response = Client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();

            Assert.Contains("$metadata#EpCustomers", result["@odata.context"].ToString());
            var customers = result["value"] as JArray;

            Assert.Equal(3, customers.Count);
            Assert.Equal("Jonier", customers[0]["Name"].ToString());
            Assert.Equal("Sam", customers[1]["Name"].ToString());
            Assert.Equal("Peter", customers[2]["Name"].ToString());
        }

        [Fact]
        public async Task QueryEntityUsingEndpointRoutingWorks()
        {
            // Arrange : GET ~/odata/EpCustomers(1)
            string requestUri = string.Format(CustomersBaseUrl, BaseAddress);
            requestUri += "(1)";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            HttpResponseMessage response = Client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();

            Assert.Contains("$metadata#EpCustomers/$entity", result["@odata.context"].ToString());

            Assert.Equal("Jonier", result["Name"].ToString());

            JObject homeAddress = result["HomeAddress"] as JObject;
            Assert.Equal("Redmond", homeAddress["City"].ToString());
            Assert.Equal("156 AVE NE", homeAddress["Street"].ToString());

            JArray favoriteAddresses = result["FavoriteAddresses"] as JArray;
            Assert.Equal(2, favoriteAddresses.Count);
        }

        [Fact]
        public async Task QueryPropertyOnEntityUsingEndpointRoutingWorks()
        {
            // Arrange : GET ~/odata/EpCustomers(2)/HomeAddress
            string requestUri = string.Format(CustomersBaseUrl, BaseAddress);
            requestUri += "(2)/HomeAddress";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            HttpResponseMessage response = Client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();

            Assert.Equal(new[] { "@odata.context", "City", "Street" }, result.Properties().Select(p => p.Name));

            Assert.Contains("$metadata#EpCustomers(2)/HomeAddress", result["@odata.context"].ToString());
            Assert.Equal("Bellevue", result["City"].ToString());
            Assert.Equal("Main St NE", result["Street"].ToString());
        }

        [Fact]
        public void CreateEntityUsingEndpointRoutingWorks()
        {
            // Arrange: POST ~/odata/EpCustomers
            string requestUri = string.Format(CustomersBaseUrl, BaseAddress);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            string content = @"{
                'Name':'NewCustomerName',
                'HomeAddress':{'City':'NewCity','Street':'NewStreet'},
                'FavoriteAddresses':[]
            }";
            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = Client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public void DeleteEntityUsingEndpointRoutingWorks()
        {
            // Arrange: DELETE ~/odata/EpCustomers(99)
            string requestUri = string.Format(CustomersBaseUrl, BaseAddress);
            requestUri += "(99)"; // magic integer to test in controller

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, requestUri);

            // Act
            HttpResponseMessage response = Client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task QueryOptoinsOnEntityUsingEndpointRoutingWorks()
        {
            // Arrange : GET ~/odata/EpCustomers(1)?$options
            string requestUri = string.Format(CustomersBaseUrl, BaseAddress);
            requestUri += "(1)?$select=HomeAddress($select=City),FavoriteAddresses($top=1)&$expand=Order";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            HttpResponseMessage response = Client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();

            Assert.Equal(new[] { "@odata.context", "HomeAddress", "FavoriteAddresses", "Order" }, result.Properties().Select(p => p.Name));

            Assert.Contains("$metadata#EpCustomers(HomeAddress,FavoriteAddresses,Order())/$entity", result["@odata.context"].ToString());
            
            JObject homeAddress = result["HomeAddress"] as JObject;
            Assert.Equal("Redmond", homeAddress["City"].ToString());
            Assert.Null(homeAddress.Property("Street")); // not select "Street"

            JArray favoriteAddresses = result["FavoriteAddresses"] as JArray;
            Assert.Single(favoriteAddresses); // only 1 because $top=1

            // $expand=Order
            JObject order = result["Order"] as JObject;
            Assert.Equal(new[] { "Id", "Title" }, order.Properties().Select(p => p.Name));
            Assert.Equal("1", order["Id"].ToString());
            Assert.Equal("104m", order["Title"].ToString());
        }

        [Fact]
        public async Task CanReadDataInBatchUsingEndpointRouting()
        {
            // Arrange
            var requestUri = string.Format("{0}/odata/$batch", this.BaseAddress);
            Uri address = new Uri(this.BaseAddress, UriKind.Absolute);

            string relativeToServiceRootUri = "EpCustomers";
            string relativeToHostUri = address.LocalPath.TrimEnd(new char[] { '/' }) + "/odata/EpCustomers";
            string absoluteUri = this.BaseAddress + "/odata/EpCustomers";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpContent content = new StringContent(@"
            {
                ""requests"":[
                    {
                    ""id"": ""2"",
                    ""method"": ""get"",
                    ""url"": """ + relativeToServiceRootUri + @""",
                    ""headers"": { ""Accept"": ""application/json""}
                    },
                    {
                    ""id"": ""3"",
                    ""method"": ""get"",
                    ""url"": """ + relativeToHostUri + @"""
                    },
                    {
                    ""id"": ""4"",
                    ""method"": ""get"",
                    ""url"": """ + absoluteUri + @""",
                    ""headers"": { ""Accept"": ""application/json""}
                    }
                ]
            }");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content = content;
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

            var stream = await response.Content.ReadAsStreamAsync();
            IODataResponseMessage odataResponseMessage = new ODataMessageWrapper(stream, response.Content.Headers);
            int subResponseCount = 0;
            var model = EdmModel;
            using (var messageReader = new ODataMessageReader(odataResponseMessage, new ODataMessageReaderSettings(), model))
            {
                var batchReader = messageReader.CreateODataBatchReader();
                while (batchReader.Read())
                {
                    switch (batchReader.State)
                    {
                        case ODataBatchReaderState.Operation:
                            var operationMessage = batchReader.CreateOperationResponseMessage();
                            subResponseCount++;
                            Assert.Equal(200, operationMessage.StatusCode);
                            Assert.Contains("application/json", operationMessage.Headers.Single(h => String.Equals(h.Key, "Content-Type", StringComparison.OrdinalIgnoreCase)).Value);
                            using (var innerMessageReader = new ODataMessageReader(operationMessage, new ODataMessageReaderSettings(), model))
                            {
                                var innerReader = innerMessageReader.CreateODataResourceSetReader();
                                while (innerReader.Read()) ;
                            }
                            break;
                    }
                }
            }

            Assert.Equal(3, subResponseCount);
        }
    }
}

