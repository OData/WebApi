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
            customers.EntityType.Property(p => p.Name).IsNotFilterable().IsNotSortable();
            customers.EntityType.CollectionProperty(p => p.Addresses).IsNotCountable();

            // Can override the behavior specified by the attributes for primitive properties
            customers.EntityType.Property(p => p.Age).IsFilterable().IsSortable();
            customers.EntityType.CollectionProperty(p => p.Numbers).IsCountable();

            // Can limit on relationships
            customers.EntityType.HasMany(c => c.Orders).IsNotNavigable().IsNotExpandable().IsNotCountable();

            return builder.GetEdmModel();
        }

        [Fact]
        public void QueryableLimitation_NotSortableFromModelTest()
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

        [Theory]
        [InlineData("NotFilterableNotSortableLastName")]
        [InlineData("NonFilterableUnsortableLastName")]
        public void QueryableLimitation_NotSortableFromAttributeTest(string property)
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/QueryLimitCustomers?$orderby=" + property;

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = _client.SendAsync(request).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
            Assert.Contains(
                String.Format("The query specified in the URI is not valid. The property '{0}' cannot be used in the $orderby query option.", property),
                responseString);
        }

        [Fact]
        public void QueryableLimitation_NotFilterableFromModelTest()
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

        [Theory]
        [InlineData("NotFilterableNotSortableLastName")]
        [InlineData("NonFilterableUnsortableLastName")]
        public void QueryableLimitation_NotFilterableFromAttributeTest(string property)
        {
            // Arrange
            string requestUri = BaseAddress +
                String.Format("/odata/QueryLimitCustomers?$filter={0} eq 'LastName 1'", property);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = _client.SendAsync(request).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(response.StatusCode, HttpStatusCode.BadRequest);
            Assert.Contains(
                String.Format(
                    "The query specified in the URI is not valid. The property '{0}' cannot be used in the $filter query option.",
                    property),
                responseString);
        }

        [Fact]
        public void QueryableLimitation_NotFilterableAttributeOverrideByModelTest()
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

        [Theory]
        [InlineData("QueryLimitCustomers(1)/Addresses?$count=true")]
        [InlineData("QueryLimitCustomers(1)/Addresses/$count")]
        [InlineData("QueryLimitCustomers(1)/System.Web.OData.DerivedQueryLimitCustomer/Addresses?$count=true")]
        [InlineData("QueryLimitCustomers(1)/System.Web.OData.DerivedQueryLimitCustomer/Addresses/$count")]
        public void QueryableLimitation_NotCountableFromModelTest(string uri)
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/" + uri;

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = _client.SendAsync(request).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The query specified in the URI is not valid. The property 'Addresses' cannot be used for $count.",
                responseString);
        }

        [Theory]
        [InlineData("QueryLimitCustomers(1)/ImportantOrders?$count=true")]
        [InlineData("QueryLimitCustomers(1)/ImportantOrders/$count")]
        [InlineData("QueryLimitCustomers(1)/System.Web.OData.DerivedQueryLimitCustomer/ImportantOrders?$count=true")]
        [InlineData("QueryLimitCustomers(1)/System.Web.OData.DerivedQueryLimitCustomer/ImportantOrders/$count")]
        public void QueryableLimitation_NotCountableFromAttributeTest(string uri)
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/" + uri;

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = _client.SendAsync(request).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The query specified in the URI is not valid. The property 'ImportantOrders' cannot be used for $count.",
                responseString);
        }

        [Theory]
        [InlineData("QueryLimitCustomers(1)/Numbers?$count=true")]
        [InlineData("QueryLimitCustomers(1)/Numbers/$count")]
        [InlineData("QueryLimitCustomers(1)/System.Web.OData.DerivedQueryLimitCustomer/Numbers?$count=true")]
        [InlineData("QueryLimitCustomers(1)/System.Web.OData.DerivedQueryLimitCustomer/Numbers/$count")]
        public void QueryableLimitation_NotCountableAttributeOverrideByModelTest(string uri)
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/" + uri;

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = _client.SendAsync(request).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("QueryLimitCustomers(1)/Notes?$count=true")]
        [InlineData("QueryLimitCustomers(1)/Notes/$count")]
        [InlineData("QueryLimitCustomers(1)/System.Web.OData.DerivedQueryLimitCustomer/Notes?$count=true")]
        [InlineData("QueryLimitCustomers(1)/System.Web.OData.DerivedQueryLimitCustomer/Notes/$count")]
        public void QueryableLimitation_CountNotAllowedInQueryOptionsTest(string uri)
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/" + uri;

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = _client.SendAsync(request).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(
                "The query specified in the URI is not valid. Query option 'Count' is not allowed. To allow it, " +
                "set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.",
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
                        NotFilterableNotSortableLastName = "NotFilterableNotSortableLastName " + i,
                        NonFilterableUnsortableLastName = "NonFilterableUnsortableLastName " + i,
                        Age = 30 + i,
                        Address = "Address " + i,
                        Addresses = new[] { "Address " + i },
                        Numbers = new[] { i },
                        Notes = new[] { "Note " + i },
                        Orders = Enumerable.Range(0, i).Select(j =>
                            new QueryLimitOrder
                            {
                                Id = j,
                                OrderName = "Order_" + i + "_" + j,
                                OrderValue = j
                            }).ToList(),
                        ImportantOrders = Enumerable.Range(0, i).Select(j =>
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

            [EnableQuery]
            public IHttpActionResult GetAddresses(int key)
            {
                return Ok(customers.Single(customer => customer.Id == key).Addresses);
            }

            [EnableQuery]
            public IHttpActionResult GetNumbers(int key)
            {
                return Ok(customers.Single(customer => customer.Id == key).Numbers);
            }

            [EnableQuery]
            public IHttpActionResult GetImportantOrders(int key)
            {
                return Ok(customers.Single(customer => customer.Id == key).ImportantOrders);
            }

            [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.All ^ AllowedQueryOptions.Count)]
            public IHttpActionResult GetNotes(int key)
            {
                return Ok(customers.Single(customer => customer.Id == key).Notes);
            }
        }

        // Models
        public class QueryLimitCustomer
        {
            public int Id { get; set; }

            public string Name { get; set; }

            [NotFilterable]
            [NotSortable]
            public string NotFilterableNotSortableLastName { get; set; }

            [NonFilterable]
            [Unsortable]
            public string NonFilterableUnsortableLastName { get; set; }

            [NotFilterable]
            [NotSortable]
            public int Age { get; set; }

            [NotNavigable]
            public string Address { get; set; }

            public IEnumerable<string> Addresses { get; set; }

            [NotCountable]
            public int[] Numbers { get; set; }

            public string[] Notes { get; set; }

            public ICollection<QueryLimitOrder> Orders { get; set; }

            [NotCountable]
            public IList<QueryLimitOrder> ImportantOrders { get; set; }
        }

        public class QueryLimitOrder
        {
            public int Id { get; set; }
            public string OrderName { get; set; }
            public decimal OrderValue { get; set; }
        }

        public class DerivedQueryLimitCustomer : QueryLimitCustomer
        {
            public string DerivedName { get; set; }
        }
    }
}
