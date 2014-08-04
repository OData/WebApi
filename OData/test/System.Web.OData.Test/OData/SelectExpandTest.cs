// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace System.Web.OData
{
    public class SelectExpandTest
    {
        private const string AcceptJsonFullMetadata = "application/json;odata.metadata=full";
        private const string AcceptJson = "application/json";

        private HttpConfiguration _configuration;
        private HttpClient _client;

        public SelectExpandTest()
        {
            _configuration =
                new[]
                {
                    typeof(SelectExpandTestCustomersController), typeof(SelectExpandTestCustomersAliasController),
                    typeof(PlayersController), typeof(NonODataSelectExpandTestCustomersController),
                    typeof(AttributedSelectExpandCustomersController), typeof(SelectExpandTestCustomer),
                    typeof(SelectExpandTestSpecialCustomer), typeof(SelectExpandTestCustomerWithAlias),
                    typeof(SelectExpandTestOrder), typeof(SelectExpandTestSpecialOrder),
                    typeof(SelectExpandTestSpecialOrderWithAlias)
                }.GetHttpConfiguration();
            _configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            _configuration.MapODataServiceRoute("odata", "odata", GetModel());
            _configuration.MapODataServiceRoute("odata-inheritance", "odata-inheritance", GetModelWithInheritance());
            _configuration.MapODataServiceRoute("odata-alias", "odata-alias", GetModelWithCustomerAlias());
            _configuration.MapODataServiceRoute(
                "odata-alias2-inheritance",
                "odata-alias2-inheritance",
                GetModelWithCustomerAliasAndInheritance());
            _configuration.MapODataServiceRoute("odata2", "odata2", GetModelWithProcedures());
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
            Assert.Equal("http://localhost/odata/$metadata#SelectExpandTestCustomers(ID,Orders)", result["@odata.context"]);
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
        public void SelectExpand_Works_WithAlias()
        {
            // Arrange
            string uri = "/odata-alias/SelectExpandTestCustomersAlias?$select=ID,OrdersAlias&$expand=OrdersAlias";

            // Act
            HttpResponseMessage response = GetResponse(uri, AcceptJsonFullMetadata);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(
                "http://localhost/odata-alias/$metadata#SelectExpandTestCustomersAlias(ID,OrdersAlias)",
                result["@odata.context"]);
            ValidateCustomerAlias(result["value"][0]);
        }

        [Fact]
        public void SelectExpand_WithInheritance_Alias_Works()
        {
            // Arrange
            string @namespace = "com.contoso";
            string select = String.Format("$select={0}.SelectExpandTestSpecialCustomerAlias/RankAlias,{0}.SelectExpandTestSpecialCustomerAlias/SpecialOrdersAlias", @namespace);
            string expand = String.Format("$expand={0}.SelectExpandTestSpecialCustomerAlias/SpecialOrdersAlias", @namespace);
            string uri = "/odata-alias2-inheritance/SelectExpandTestCustomersAlias?" + select + "&" + expand;

            // Act
            HttpResponseMessage response = GetResponse(uri, AcceptJsonFullMetadata);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Null(result["value"][0]["ID"]);
            Assert.Null(result["value"][0]["SpecialOrdersAlias"]);
            Assert.Equal(100, result["value"][1]["RankAlias"]);
            Assert.NotNull(result["value"][1]["SpecialOrdersAlias"]);
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
            string uri = "/api/AttributedSelectExpandCustomers?$select=Id,Orders&$expand=Orders($select=Total)";

            // Act
            HttpResponseMessage response = GetResponse(uri, AcceptJson);

            // Assert
            JArray result = JArray.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(1, result[0]["JsonId"]);
            Assert.Equal(1, result[0]["JsonOrders"][0]["JsonTotal"]);
        }

        [Fact]
        public void SelectExpand_WithNonODataJson_JsonProperty_Wins_OverDataMember()
        {
            // Arrange
            string uri = "/api/AttributedSelectExpandCustomers?$select=DataMemberCustomerName";

            // Act
            HttpResponseMessage response = GetResponse(uri, AcceptJson);

            // Assert
            JArray result = JArray.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal("Name 1", result[0]["JsonName"]);
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
            Assert.Equal("http://localhost/odata/$metadata#SelectExpandTestCustomers(ID,Orders)/$entity", result["@odata.context"]);
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

        [Fact]
        public void SelectExpand_Works_WithLevels()
        {
            // Arrange
            string uri = "/api/?id=44&$select=ID&$expand=PreviousCustomer($levels=2)";

            // Act
            HttpResponseMessage response = GetResponse(uri, AcceptJson);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(44, result["ID"]);
            Assert.Equal(43, result["PreviousCustomer"]["ID"]);
            Assert.Equal(42, result["PreviousCustomer"]["PreviousCustomer"]["ID"]);
            Assert.Null(result["PreviousCustomer"]["PreviousCustomer"]["PreviousCustomer"]);
        }

        [Fact]
        public void SelectExpand_Works_ForSelectAction_WithNamespaceQualifiedName()
        {
            // Arrange
            const string URI = "/odata2/Players?$select=Name,Default.*";

            // Act
            HttpResponseMessage response = GetResponse(URI, AcceptJsonFullMetadata);
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(responseString);
            Assert.Equal(5, result["value"].Count());
            for (int i = 0; i < 5; i++)
            {
                Assert.Equal("http://localhost/odata2/Players(" + i + ")/Default.PlayerAction1", result["value"][i]["#Default.PlayerAction1"]["target"]);
                Assert.Equal("http://localhost/odata2/Players(" + i + ")/Default.PlayerAction2", result["value"][i]["#Default.PlayerAction2"]["target"]);
                Assert.Equal("http://localhost/odata2/Players(" + i + ")/Default.PlayerAction3", result["value"][i]["#Default.PlayerAction3"]["target"]);
                Assert.Equal("http://localhost/odata2/Players(" + i + ")/Default.PlayerFunction1", result["value"][i]["#Default.PlayerFunction1"]["target"]);
                Assert.Equal("http://localhost/odata2/Players(" + i + ")/Default.PlayerFunction2", result["value"][i]["#Default.PlayerFunction2"]["target"]);
            }
        }

        [Theory]
        [InlineData("Default.Container.*")]
        [InlineData("Container.*")]
        public void SelectExpand_DoesnotWork_ForSelectAction_WithNonNamespaceQualifiedName(string nonNamespaceQualifiedName)
        {
            // Arrange
            string uri = "/odata2/Players?$select=Name," + nonNamespaceQualifiedName;

            // Act
            HttpResponseMessage response = GetResponse(uri, AcceptJsonFullMetadata);
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Contains("The query specified in the URI is not valid. " +
                "A path within the select or expand query option is not supported.",
                responseString);
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

        private static void ValidateCustomerAlias(JToken customer)
        {
            Assert.Equal(42, customer["ID"]);
            Assert.Null(customer["NameAlias"]);
            var orders = customer["OrdersAlias"] as JArray;
            Assert.NotNull(orders);
            Assert.Equal(1, orders.Count);
            var order = orders[0];
            Assert.Equal(24, order["ID"]);
            Assert.Equal(100, order["AmountAlias"]);
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

        private IEdmModel GetModelWithCustomerAlias()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder { ModelAliasingEnabled = true };
            builder.EntitySet<SelectExpandTestCustomerWithAlias>("SelectExpandTestCustomersAlias");
            builder.EntitySet<SelectExpandTestOrderWithAlias>("SelectExpandTestOrdersAlias");
            builder.Ignore<SelectExpandTestSpecialCustomerWithAlias>();
            builder.Ignore<SelectExpandTestSpecialOrderWithAlias>();
            return builder.GetEdmModel();
        }

        private IEdmModel GetModelWithCustomerAliasAndInheritance()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder { ModelAliasingEnabled = true };
            builder.EntitySet<SelectExpandTestCustomerWithAlias>("SelectExpandTestCustomersAlias");
            builder.EntitySet<SelectExpandTestOrderWithAlias>("SelectExpandTestOrdersAlias");
            return builder.GetEdmModel();
        }

        private IEdmModel GetModelWithProcedures()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Player>("Players");

            // Actions
            builder.EntityType<Player>().Action("PlayerAction1");
            builder.EntityType<Player>().Action("PlayerAction2");
            builder.EntityType<Player>().Action("PlayerAction3");

            // Functions
            builder.EntityType<Player>().Function("PlayerFunction1").Returns<int>();
            builder.EntityType<Player>().Function("PlayerFunction2").Returns<int>();

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

                SelectExpandTestSpecialCustomer specialCustomer = new SelectExpandTestSpecialCustomer
                {
                    ID = 43,
                    Name = "Name",
                    Rank = 100,
                    PreviousCustomer = customer
                };
                SelectExpandTestSpecialOrder specialOrder = new SelectExpandTestSpecialOrder
                {
                    ID = 25,
                    Amount = 100,
                    SpecialDiscount = 100,
                    SpecialCustomer = specialCustomer

                };
                specialCustomer.SpecialOrders = new[] { specialOrder };

                SelectExpandTestCustomer nextCustomer = new SelectExpandTestCustomer
                {
                    ID = 44,
                    Name = "Name",
                    PreviousCustomer = specialCustomer
                };
                SelectExpandTestOrder nextOrder = new SelectExpandTestOrder
                {
                    ID = 26,
                    Amount = 100,
                    Customer = nextCustomer
                };
                nextCustomer.Orders = new[] { nextOrder };

                return new[] { customer, specialCustomer, nextCustomer };
            }
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public SelectExpandTestOrder[] Orders { get; set; }

        public SelectExpandTestCustomer PreviousCustomer { get; set; }
    }

    public class SelectExpandTestSpecialCustomer : SelectExpandTestCustomer
    {
        public int Rank { get; set; }

        public SelectExpandTestSpecialOrder[] SpecialOrders { get; set; }
    }

    [DataContract(Namespace = "com.contoso", Name = "SelectExpandTestCustomerAlias")]
    public class SelectExpandTestCustomerWithAlias
    {
        public static IList<SelectExpandTestCustomerWithAlias> Customers
        {
            get
            {
                SelectExpandTestCustomerWithAlias customer = new SelectExpandTestCustomerWithAlias { ID = 42, Name = "Name" };
                SelectExpandTestOrderWithAlias order = new SelectExpandTestOrderWithAlias { ID = 24, Amount = 100, Customer = customer };
                customer.Orders = new[] { order };

                SelectExpandTestSpecialCustomerWithAlias specialCustomer =
                    new SelectExpandTestSpecialCustomerWithAlias { ID = 43, Name = "Name", Rank = 100 };
                SelectExpandTestSpecialOrderWithAlias specialOrder =
                    new SelectExpandTestSpecialOrderWithAlias { ID = 25, Amount = 100, SpecialDiscount = 100, SpecialCustomer = specialCustomer };
                specialCustomer.SpecialOrders = new[] { specialOrder };

                return new[] { customer, specialCustomer };
            }
        }

        [DataMember]
        public int ID { get; set; }

        [DataMember(Name = "NameAlias")]
        public string Name { get; set; }

        [DataMember(Name = "OrdersAlias")]
        public SelectExpandTestOrderWithAlias[] Orders { get; set; }
    }

    [DataContract(Namespace = "com.contoso", Name = "SelectExpandTestSpecialCustomerAlias")]
    public class SelectExpandTestSpecialCustomerWithAlias : SelectExpandTestCustomerWithAlias
    {
        [DataMember(Name = "RankAlias")]
        public int Rank { get; set; }

        [DataMember(Name = "SpecialOrdersAlias")]
        public SelectExpandTestSpecialOrderWithAlias[] SpecialOrders { get; set; }
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

    [DataContract(Namespace = "com.contoso", Name = "SelectExpandTestOrderAlias")]
    public class SelectExpandTestOrderWithAlias
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember(Name = "AmountAlias")]
        public int Amount { get; set; }

        [DataMember(Name = "CustomerAlias")]
        public SelectExpandTestCustomerWithAlias Customer { get; set; }
    }

    [DataContract(Namespace = "com.contoso", Name = "SelectExpandTestSpecialOrderAlias")]
    public class SelectExpandTestSpecialOrderWithAlias : SelectExpandTestOrderWithAlias
    {
        [DataMember(Name = "SpecialDiscountAlias")]
        public int SpecialDiscount { get; set; }

        [DataMember(Name = "SpecialCustomerAlias")]
        public SelectExpandTestSpecialCustomerWithAlias SpecialCustomer { get; set; }
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

    public class SelectExpandTestCustomersAliasController : ODataController
    {
        [EnableQuery]
        public IEnumerable<SelectExpandTestCustomerWithAlias> Get()
        {
            return SelectExpandTestCustomerWithAlias.Customers;
        }

        [EnableQuery]
        public SelectExpandTestCustomerWithAlias GetSelectExpandTestCustomer([FromODataUri]int key)
        {
            return SelectExpandTestCustomerWithAlias.Customers[0];
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
            IQueryable<SelectExpandTestCustomer> singleCustomer = SelectExpandTestCustomer.Customers
                .Where(c => c.ID == id).AsQueryable();
            return SingleResult.Create(singleCustomer);
        }
    }

    public class AttributedSelectExpandCustomersController : ApiController
    {
        public IHttpActionResult Get(ODataQueryOptions<AttributedSelectExpandCustomer> options)
        {
            IQueryable result = options.ApplyTo(Enumerable.Range(1, 10).Select(i => new AttributedSelectExpandCustomer
            {
                Id = i,
                Name = "Name " + i,
                Orders = Enumerable.Range(1, 10).Select(j => new AttributedSelectExpandOrder
                {
                    CustomerName = "Customer Name" + j,
                    Id = j,
                    Total = i * j
                }).ToList()
            }).AsQueryable());
            return Ok(result);
        }
    }

    public class PlayersController : ODataController
    {
        private IList<Player> players = Enumerable.Range(0, 5).Select(i =>
                    new Player
                    {
                        Id = i,
                        Name = "PayerName " + i,
                        Category = "Category " + i,
                        Address = "Address " + i
                    }).ToList();

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(players);
        }
    }

    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Address { get; set; }
    }
}
