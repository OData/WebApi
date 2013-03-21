// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Web.Http.OData
{
    public class SelectExpandTest
    {
        private const string AcceptJsonFullMetadata = "application/json;odata=fullmetadata";
        private const string AcceptJson = "application/json";

        private HttpConfiguration _configuration;
        private HttpClient _client;

        public SelectExpandTest()
        {
            _configuration = new HttpConfiguration();
            _configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            _configuration.Routes.MapODataRoute("odata", "odata", GetModel());
            _configuration.Routes.MapODataRoute("odata-inheritance", "odata-inheritance", GetModelWithInheritance());
            _configuration.Routes.MapHttpRoute("api", "api", new { controller = "NonODataSelectExpandTestCustomers" });

            HttpServer server = new HttpServer(_configuration);
            _client = new HttpClient(server);
        }

        [Fact]
        public void SelectExpand_Works()
        {
            // Arrange
            string uri = "/odata/SelectExpandTestCustomers?$select=ID,Orders&$expand=Orders";

            // Act
            HttpResponseMessage response = GetResponse(uri, AcceptJsonFullMetadata);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            ValidateCustomer(result["value"][0]);
        }

        [Fact]
        public void SelectExpand_WithInheritance_Works()
        {
            // Arrange
            string @namespace = typeof(SelectExpandTestCustomer).Namespace;
            string select = String.Format("$select={0}.SelectExpandTestSpecialCustomer/Rank,{0}.SelectExpandTestSpecialCustomer/SpecialOrders", @namespace);
            string expand = String.Format("$expand={0}.SelectExpandTestSpecialCustomer/SpecialOrders", @namespace);
            string uri = "/odata-inheritance/SelectExpandTestCustomers?" + select + "&" + expand;

            // Act
            HttpResponseMessage response = GetResponse(uri, AcceptJsonFullMetadata);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Null(result["value"][0]["ID"]);
            Assert.Null(result["value"][0]["SpecialOrders"]);
            Assert.Equal(100, result["value"][1]["Rank"]);
            Assert.NotNull(result["value"][1]["SpecialOrders"]);
        }

        [Fact]
        public void SelectExpand_WithInheritanceAndNonODataJson_Works()
        {
            // Arrange
            string customerNamespace = typeof(SelectExpandTestCustomer).Namespace;
            string select = String.Format("$select={0}.SelectExpandTestSpecialCustomer/Rank,{0}.SelectExpandTestSpecialCustomer/SpecialOrders", customerNamespace);
            string expand = String.Format("$expand={0}.SelectExpandTestSpecialCustomer/SpecialOrders", customerNamespace);
            string uri = "/api/?" + select + "&" + expand;

            // Act
            HttpResponseMessage response = GetResponse(uri, AcceptJson);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JArray result = JArray.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Empty(result[0]);
            Assert.Null(result[1]["ID"]);
            Assert.Equal(100, result[1]["Rank"]);
            Assert.NotNull(result[1]["SpecialOrders"]);
        }

        [Fact]
        public void SelectExpand_QueryableOnSingleEntity_Works()
        {
            // Arrange
            string uri = "/odata/SelectExpandTestCustomers(42)?$select=ID,Orders&$expand=Orders";

            // Act
            HttpResponseMessage response = GetResponse(uri, AcceptJson);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            ValidateCustomer(result);
        }

        [Fact]
        public void SelectExpand_QueryableOnSingleResult_Works()
        {
            // Arrange
            string uri = "/api/?id=42&$select=ID,Orders&$expand=Orders";

            // Act
            HttpResponseMessage response = GetResponse(uri, AcceptJson);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(42, result["ID"]);
            Assert.Null(result["Name"]);
            Assert.NotNull(result["Orders"]);
        }

        private HttpResponseMessage GetResponse(string uri, string acceptHeader)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost" + uri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=fullmetadata"));
            return _client.SendAsync(request).Result;
        }

        private static void ValidateCustomer(JToken customer)
        {
            Assert.Equal(42, customer["ID"]);
            Assert.Null(customer["Name"]);
            var orders = customer["Orders"] as JArray;
            Assert.NotNull(orders);
            Assert.Equal(1, orders.Count);
            var order = orders[0];
            Assert.Equal(24, order["ID"]);
            Assert.Equal(100, order["Amount"]);
        }

        private IEdmModel GetModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<SelectExpandTestCustomer>("SelectExpandTestCustomers");
            builder.EntitySet<SelectExpandTestOrder>("SelectExpandTestOrders");
            builder.Ignore<SelectExpandTestSpecialCustomer>();
            builder.Ignore<SelectExpandTestSpecialOrder>();
            return builder.GetEdmModel();
        }

        private IEdmModel GetModelWithInheritance()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<SelectExpandTestCustomer>("SelectExpandTestCustomers");
            builder.EntitySet<SelectExpandTestOrder>("SelectExpandTestOrders");
            return builder.GetEdmModel();
        }
    }

    public class SelectExpandTestCustomer
    {
        public static IList<SelectExpandTestCustomer> Customers
        {
            get
            {
                SelectExpandTestCustomer customer = new SelectExpandTestCustomer { ID = 42, Name = "Name" };
                SelectExpandTestOrder order = new SelectExpandTestOrder { ID = 24, Amount = 100, Customer = customer };
                customer.Orders = new[] { order };

                SelectExpandTestSpecialCustomer specialCustomer = new SelectExpandTestSpecialCustomer { ID = 43, Name = "Name", Rank = 100 };
                SelectExpandTestSpecialOrder specialOrder =
                    new SelectExpandTestSpecialOrder { ID = 25, Amount = 100, SpecialDiscount = 100, SpecialCustomer = specialCustomer };
                specialCustomer.SpecialOrders = new[] { specialOrder };

                return new[] { customer, specialCustomer };
            }
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public SelectExpandTestOrder[] Orders { get; set; }
    }

    public class SelectExpandTestSpecialCustomer : SelectExpandTestCustomer
    {
        public int Rank { get; set; }

        public SelectExpandTestSpecialOrder[] SpecialOrders { get; set; }
    }

    public class SelectExpandTestOrder
    {
        public int ID { get; set; }

        public int Amount { get; set; }

        public SelectExpandTestCustomer Customer { get; set; }
    }

    public class SelectExpandTestSpecialOrder : SelectExpandTestOrder
    {
        public int SpecialDiscount { get; set; }

        public SelectExpandTestSpecialCustomer SpecialCustomer { get; set; }
    }

    public class SelectExpandTestCustomersController : ODataController
    {
        [Queryable]
        public IEnumerable<SelectExpandTestCustomer> Get()
        {
            return SelectExpandTestCustomer.Customers;
        }

        [Queryable]
        public SelectExpandTestCustomer GetSelectExpandTestCustomer([FromODataUri]int key)
        {
            return SelectExpandTestCustomer.Customers[0];
        }
    }

    public class NonODataSelectExpandTestCustomersController : ApiController
    {
        [Queryable]
        public IEnumerable<SelectExpandTestCustomer> Get()
        {
            return SelectExpandTestCustomer.Customers;
        }

        [Queryable]
        public SingleResult Get(int id)
        {
            IQueryable<SelectExpandTestCustomer> singleCustomer = SelectExpandTestCustomer.Customers.AsQueryable().Take(1);
            return SingleResult.Create(singleCustomer);
        }
    }
}
