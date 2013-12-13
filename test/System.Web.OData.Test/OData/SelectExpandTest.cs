// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using System.Web.Http.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Web.Http.OData
{
    public class SelectExpandTest
    {
        private const string AcceptJsonFullMetadata = "application/json;odata.metadata=full";
        private const string AcceptJson = "application/json";

        private HttpConfiguration _configuration;
        private HttpClient _client;

        public SelectExpandTest()
        {
            _configuration = new HttpConfiguration();
            _configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            _configuration.Routes.MapODataRoute("odata", "odata", GetModel());
            _configuration.Routes.MapODataRoute("odata-inheritance", "odata-inheritance", GetModelWithInheritance());
            _configuration.Routes.MapODataRoute("odata-alias", "odata-alias", GetModelWithCustomerAlias());
            _configuration.Routes.MapODataRoute(
                "odata-alias2-inheritance",
                "odata-alias2-inheritance",
                GetModelWithCustomerAliasAndInheritance()); 
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

        private HttpResponseMessage GetResponse(string uri, string acceptHeader)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost" + uri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
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

    public class SelectExpandTestCustomersAliasController : ODataController
    {
        [Queryable]
        public IEnumerable<SelectExpandTestCustomerWithAlias> Get()
        {
            return SelectExpandTestCustomerWithAlias.Customers;
        }

        [Queryable]
        public SelectExpandTestCustomerWithAlias GetSelectExpandTestCustomer([FromODataUri]int key)
        {
            return SelectExpandTestCustomerWithAlias.Customers[0];
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
