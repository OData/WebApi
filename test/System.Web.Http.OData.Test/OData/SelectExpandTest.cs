// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Query;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Newtonsoft.Json;
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

            _configuration.Routes.MapODataServiceRoute("odata", "odata", GetModel());
            _configuration.Routes.MapODataServiceRoute("odata-inheritance", "odata-inheritance", GetModelWithInheritance());
            _configuration.Routes.MapHttpRoute("api", "api/{controller}", new { controller = "NonODataSelectExpandTestCustomers" });

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
            Assert.Equal("http://localhost/odata/$metadata#SelectExpandTestCustomers&$select=ID,Orders", result["odata.metadata"]);
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
        public void SelectExpand_WithNonODataJson_Respects_JsonProperty()
        {
            // Arrange
            string uri = "/api/AttributedSelectExpandCustomers?$select=Id, Orders/Total&$expand=Orders";

            // Act
            HttpResponseMessage response = GetResponse(uri, AcceptJson);

            // Assert
            JArray result = JArray.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(1, result[1]["JsonId"]);
            Assert.Equal(1, result[1]["JsonOrders"][0]["JsonTotal"]);
        }

        [Fact]
        public void SelectExpand_WithNonODataJson_JsonProperty_Wins_OverDataMember()
        {
            // Arrange
            string uri = "/api/AttributedSelectExpandCustomers?$select=Name";

            // Act
            HttpResponseMessage response = GetResponse(uri, AcceptJson);

            // Assert
            JArray result = JArray.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal("Name 1", result[1]["JsonName"]);
        }

        [Fact]
        public void SelectExpand_WithNonODataJson_DataMember_Works_WithInheritance()
        {
            // Arrange
            string cast = typeof(AttributedSpecialSelectExpandCustomer).FullName;
            string uri = String.Format("/api/AttributedSelectExpandCustomers?$select={0}/Age", cast);

            // Act
            HttpResponseMessage response = GetResponse(uri, AcceptJson);

            // Assert
            JArray result = JArray.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(-1, result[0]["DataMemberAge"]);
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
            Assert.Equal("http://localhost/odata/$metadata#SelectExpandTestCustomers/@Element&$select=ID,Orders", result["odata.metadata"]);
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
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));
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

    public class AttributedSelectExpandCustomersController : ApiController
    {
        public IHttpActionResult Get(ODataQueryOptions<AttributedSelectExpandCustomer> options)
        {
            IQueryable result = options.ApplyTo(
                Enumerable.Repeat(new AttributedSpecialSelectExpandCustomer
                {
                    Id = -1,
                    Name = "Special Customer",
                    Orders = Enumerable.Range(1, 10).Select(j => new AttributedSelectExpandOrder
                {
                    CustomerName = "Customer Name" + j,
                    Id = j,
                    Total = 1 * j
                }).ToList(),
                    Age = -1
                }, 1)
                .Concat(Enumerable.Range(1, 10).Select(i => new AttributedSelectExpandCustomer
                {
                    Id = i,
                    Name = "Name " + i,
                    Orders = Enumerable.Range(1, 10).Select(j => new AttributedSelectExpandOrder
                    {
                        CustomerName = "Customer Name" + j,
                        Id = j,
                        Total = i * j
                    }).ToList()
                }))
                .AsQueryable());
            return Ok(result);
        }

    }

    [DataContract]
    public class AttributedSelectExpandCustomer
    {
        [DataMember]
        [JsonProperty(PropertyName = "JsonId")]
        public int Id { get; set; }

        [DataMember(Name = "DataMemberCustomerName")]
        [JsonProperty(PropertyName = ("JsonName"))]
        public string Name { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "JsonOrders")]
        public ICollection<AttributedSelectExpandOrder> Orders { get; set; }
    }

    public class AttributedSpecialSelectExpandCustomer : AttributedSelectExpandCustomer
    {
        [DataMember(Name = "DataMemberAge")]
        public int Age { get; set; }
    }

    [DataContract]
    public class AttributedSelectExpandOrder
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string CustomerName { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "JsonTotal")]
        public double Total { get; set; }
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
        [EnableQuery]
        public IEnumerable<SelectExpandTestCustomer> Get()
        {
            return SelectExpandTestCustomer.Customers;
        }

        [EnableQuery]
        public SelectExpandTestCustomer GetSelectExpandTestCustomer([FromODataUri]int key)
        {
            return SelectExpandTestCustomer.Customers[0];
        }
    }

    public class NonODataSelectExpandTestCustomersController : ApiController
    {
        [EnableQuery]
        public IEnumerable<SelectExpandTestCustomer> Get()
        {
            return SelectExpandTestCustomer.Customers;
        }

        [EnableQuery]
        public SingleResult Get(int id)
        {
            IQueryable<SelectExpandTestCustomer> singleCustomer = SelectExpandTestCustomer.Customers.AsQueryable().Take(1);
            return SingleResult.Create(singleCustomer);
        }
    }
}
