// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData
{
    public class QueryableLimitationTest
    {
        private const string BaseAddress = @"http://localhost";
        private HttpConfiguration _configuration;
        private HttpClient _client;

        public QueryableLimitationTest()
        {
            _configuration =
                new[] { typeof(QueryLimitCustomersController), typeof(OpenCustomersController) }.GetHttpConfiguration();
            _configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());
            HttpServer server = new HttpServer(_configuration);
            _client = new HttpClient(server);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<QueryLimitCustomer> customers = builder.EntitySet<QueryLimitCustomer>("QueryLimitCustomers");
            EntitySetConfiguration<QueryLimitOrder> orders = builder.EntitySet<QueryLimitOrder>("QueryLimitOrders");

            // Can limit sorting and filtering primitive properties
            customers.EntityType.Property(p => p.Name).IsNonFilterable().IsUnsortable();

            // Can override the behavior specified by the attributes for primitive properties
            customers.EntityType.Property(p => p.Age).IsFilterable().IsSortable();

            // Can limit on relationships
            customers.EntityType.HasMany(c => c.Orders).IsNotNavigable().IsNotExpandable();

            return builder.GetEdmModel();
        }

        [Fact]
        public void QueryableLimitation_UnsortableFromModelTest()
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/QueryLimitCustomers?$orderby=Name";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = _client.SendAsync(request).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
            Assert.Contains("The query specified in the URI is not valid. The property 'Name' cannot be used in the $orderby query option.",
                responseString);
        }

        [Fact]
        public void QueryableLimitation_UnsortableFromAttributeTest()
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/QueryLimitCustomers?$orderby=LastName";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = _client.SendAsync(request).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
            Assert.Contains("The query specified in the URI is not valid. The property 'LastName' cannot be used in the $orderby query option.",
                responseString);
        }

        [Fact]
        public void QueryableLimitation_NonFilterableFromModelTest()
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/QueryLimitCustomers?$filter=Name eq 'FirstName 1'";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = _client.SendAsync(request).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
            Assert.Contains("The query specified in the URI is not valid. The property 'Name' cannot be used in the $filter query option.",
                responseString);
        }

        [Fact]
        public void QueryableLimitation_NonFilterableFromAttributeTest()
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/QueryLimitCustomers?$filter=LastName eq 'LastName 1'";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = _client.SendAsync(request).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
            Assert.Contains("The query specified in the URI is not valid. The property 'LastName' cannot be used in the $filter query option.",
                responseString);
        }

        [Fact]
        public void QueryableLimitation_NonFilterableAttributeOverrideByModelTest()
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/QueryLimitCustomers?$filter=Age eq 31";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = _client.SendAsync(request).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(response.StatusCode, HttpStatusCode.OK);
            Assert.Contains("\"Age\":31", responseString);
        }

        [Fact]
        public void QueryableLimitation_NotNavigableFromModelTest()
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/QueryLimitCustomers?$select=Orders";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = _client.SendAsync(request).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
            Assert.Contains("The query specified in the URI is not valid. The property 'Orders' cannot be used for navigation.",
                responseString);
        }

        [Fact]
        public void QueryableLimitation_NotExpandableFromModelTest()
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/QueryLimitCustomers?$expand=Orders";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = _client.SendAsync(request).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
            Assert.Contains("The query specified in the URI is not valid. The property 'Orders' cannot be used in the $expand query option.",
                responseString);
        }

        // Controller
        public class QueryLimitCustomersController : ODataController
        {
            private IList<QueryLimitCustomer> customers = Enumerable.Range(0, 10).Select(i =>
                    new QueryLimitCustomer
                    {
                        Id = i,
                        Name = "FirstName " + i,
                        LastName = "LastName " + i,
                        Age = 30 + i,
                        Address = "Address " + i,
                        Orders = Enumerable.Range(0, i).Select(j =>
                            new QueryLimitOrder
                            {
                                Id = j,
                                OrderName = "Order_" + i + "_" + j,
                                OrderValue = j
                            }).ToList()
                    }).ToList();

            [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
            public IHttpActionResult Get()
            {
                return Ok(customers);
            }
        }

        // Models
        public class QueryLimitCustomer
        {
            public int Id { get; set; }

            public string Name { get; set; }

            [NonFilterable]
            [Unsortable]
            public string LastName { get; set; }

            [NonFilterable]
            [Unsortable]
            public int Age { get; set; }

            [NotNavigable]
            public string Address { get; set; }

            public ICollection<QueryLimitOrder> Orders { get; set; }
        }

        public class QueryLimitOrder
        {
            public int Id { get; set; }
            public string OrderName { get; set; }
            public decimal OrderValue { get; set; }
        }
    }
}
