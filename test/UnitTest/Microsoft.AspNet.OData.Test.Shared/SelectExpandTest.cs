//-----------------------------------------------------------------------------
// <copyright file="SelectExpandTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test
{
    public class SelectExpandTest
    {
        private const string AcceptJsonFullMetadata = "application/json;odata.metadata=full";
        private const string AcceptJson = "application/json";

        [Fact]
        public async Task SelectExpand_Works()
        {
            // Arrange
            string uri = "/odata/SelectExpandTestCustomers?$select=ID,Orders&$expand=Orders";

            // Act
            HttpResponseMessage response = await GetResponse(uri, AcceptJsonFullMetadata);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("http://localhost/odata/$metadata#SelectExpandTestCustomers(ID,Orders,Orders())", result["@odata.context"]);
            ValidateCustomer(result["value"][0]);
        }

        [Theory]
        [InlineData("*")]
        [InlineData("*,Orders")]
        [InlineData("*,PreviousCustomer($levels=1)")]
        public async Task SelectExpand_Works_WithExpandStar(string expand)
        {
            // Arrange
            string uri = "/odata/SelectExpandTestCustomers?$expand=" + expand;

            // Act
            HttpResponseMessage response = await GetResponse(uri, AcceptJsonFullMetadata);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(result["value"][0]["ID"]);
            Assert.NotNull(result["value"][0]["Orders"]);
            Assert.NotNull(result["value"][0]["PreviousCustomer"]);
        }

        [Fact]
        public async Task SelectExpand_Works_Collection_WithNestedFilter()
        {
            // Arrange
            var uri = "/odata/SelectExpandTestCustomers?$expand=Orders($filter=ID eq 28)";
            // Act
            var response = await GetResponse(uri, AcceptJson);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            var content = await response.Content.ReadAsStringAsync();
            dynamic result = JObject.Parse(content);
            var customer = result.value[0];
            var orders = customer.Orders;
            Assert.Single(orders);
            Assert.Equal(10, (int)orders[0].Amount);
        }

        [Fact]
        public async Task SelectExpand_Works_Single_NavigationPropertyExpandFilterFalse_WithNestedFilter()
        {
            // Arrange
            var uri = "/odata/SelectExpandTestCustomers?$expand=PreviousCustomer($filter=ID ne 43)&$filter=ID eq 44";

            // Act
            var response = await GetResponse(uri, AcceptJson);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            dynamic result = JObject.Parse(content);
            var customer = result.value[0];
            Assert.Equal(44, (int)customer.ID);
            Assert.Equal(43, (int)customer.PreviousCustomer.ID);
        }

        [Fact]
        public async Task SelectExpand_Works_Single_NavigationPropertyExpandFilterTrue_WithNestedFilter()
        {
            // Arrange
            var uri = "/odata-expandfilter/ReferenceNavigationPropertyExpandFilter?$expand=PreviousCustomer($filter=ID ne 43)&$filter=ID eq 44";

            // Act
            var response = await GetResponse(uri, AcceptJson);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            dynamic result = JObject.Parse(content);
            var customer = result.value[0] as JObject;
            Assert.Equal(44, (int)customer["ID"]);
            Assert.False(customer["PreviousCustomer"].HasValues);
        }

        [Fact]
        public async Task SelectExpand_WithInheritance_Works()
        {
            // Arrange
            string @namespace = typeof(SelectExpandTestCustomer).Namespace;
            string select = String.Format("$select={0}.SelectExpandTestSpecialCustomer/Rank,{0}.SelectExpandTestSpecialCustomer/SpecialOrders", @namespace);
            string expand = String.Format("$expand={0}.SelectExpandTestSpecialCustomer/SpecialOrders", @namespace);
            string uri = "/odata-inheritance/SelectExpandTestCustomers?" + select + "&" + expand;

            // Act
            HttpResponseMessage response = await GetResponse(uri, AcceptJsonFullMetadata);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Null(result["value"][0]["ID"]);
            Assert.Null(result["value"][0]["SpecialOrders"]);
            Assert.Equal(100, result["value"][1]["Rank"]);
            Assert.NotNull(result["value"][1]["SpecialOrders"]);
        }

        [Fact]
        public async Task SelectExpand_Works_WithAlias()
        {
            // Arrange
            string uri = "/odata-alias/SelectExpandTestCustomersAlias?$select=ID,OrdersAlias&$expand=OrdersAlias";

            // Act
            HttpResponseMessage response = await GetResponse(uri, AcceptJsonFullMetadata);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(
                "http://localhost/odata-alias/$metadata#SelectExpandTestCustomersAlias(ID,OrdersAlias,OrdersAlias())",
                result["@odata.context"]);
            ValidateCustomerAlias(result["value"][0]);
        }

        [Fact]
        public async Task SelectExpand_WithInheritance_Alias_Works()
        {
            // Arrange
            string @namespace = "com.contoso";
            string select = String.Format("$select={0}.SelectExpandTestSpecialCustomerAlias/RankAlias,{0}.SelectExpandTestSpecialCustomerAlias/SpecialOrdersAlias", @namespace);
            string expand = String.Format("$expand={0}.SelectExpandTestSpecialCustomerAlias/SpecialOrdersAlias", @namespace);
            string uri = "/odata-alias2-inheritance/SelectExpandTestCustomersAlias?" + select + "&" + expand;

            // Act
            HttpResponseMessage response = await GetResponse(uri, AcceptJsonFullMetadata);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Null(result["value"][0]["ID"]);
            Assert.Null(result["value"][0]["SpecialOrdersAlias"]);
            Assert.Equal(100, result["value"][1]["RankAlias"]);
            Assert.NotNull(result["value"][1]["SpecialOrdersAlias"]);
        }

        [Fact]
        public async Task SelectExpand_WithInheritanceAndNonODataJson_Works()
        {
            // Arrange
            string customerNamespace = typeof(SelectExpandTestCustomer).Namespace;
            string select = String.Format("$select={0}.SelectExpandTestSpecialCustomer/Rank,{0}.SelectExpandTestSpecialCustomer/SpecialOrders", customerNamespace);
            string expand = String.Format("$expand={0}.SelectExpandTestSpecialCustomer/SpecialOrders", customerNamespace);
            string uri = "/api/?" + select + "&" + expand;

            // Act
            HttpResponseMessage response = await GetResponse(uri, AcceptJson);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JArray result = JArray.Parse(await response.Content.ReadAsStringAsync());
            Assert.Empty(result[0]);
            Assert.Null(result[1]["ID"]);
            Assert.Equal(100, result[1]["Rank"]);
            Assert.NotNull(result[1]["SpecialOrders"]);
        }

        [Fact]
        public async Task SelectExpand_WithNonODataJson_Respects_JsonProperty()
        {
            // Arrange
            string uri = "/api/AttributedSelectExpandCustomers?$select=Id,Orders&$expand=Orders($select=Total)";

            // Act
            HttpResponseMessage response = await GetResponse(uri, AcceptJson);

            // Assert
            JArray result = JArray.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(1, result[0]["JsonId"]);
            Assert.Equal(1, result[0]["JsonOrders"][0]["JsonTotal"]);
        }

        [Fact]
        public async Task SelectExpand_WithNonODataJson_JsonProperty_Wins_OverDataMember()
        {
            // Arrange
            string uri = "/api/AttributedSelectExpandCustomers?$select=DataMemberCustomerName";

            // Act
            HttpResponseMessage response = await GetResponse(uri, AcceptJson);

            // Assert
            JArray result = JArray.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("Name 1", result[0]["JsonName"]);
        }

        [Fact]
        public async Task SelectExpand_WithNullComplexProperty()
        {
            // Arrange
            var uri = "/odata/SelectExpandTestCustomers?$select=ID,Address";

            // Act
            var response = await GetResponse(uri, AcceptJson);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            SelectExpandTestCustomer[] customers = JObject.Parse(content).GetValue("value").ToObject<SelectExpandTestCustomer[]>();
            Assert.NotNull(customers);
            Assert.Equal(3, customers.Length);

            for (int i = 0; i < 3; i++)
            {
                SelectExpandTestCustomer customer = customers[i];
                Assert.Equal(42 + i, customer.ID);
                Assert.Null(customer.Address);
            }
        }

        [Fact]
        public async Task SelectExpand_QueryableOnSingleEntity_Works()
        {
            // Arrange
            string uri = "/odata/SelectExpandTestCustomers(42)?$select=ID,Orders&$expand=Orders";

            // Act
            HttpResponseMessage response = await GetResponse(uri, AcceptJson);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("http://localhost/odata/$metadata#SelectExpandTestCustomers(ID,Orders,Orders())/$entity", result["@odata.context"]);
            ValidateCustomer(result);
        }

        [Fact]
        public async Task SelectExpand_QueryableOnSingleResult_Works()
        {
            // Arrange
            string uri = "/api/?id=42&$select=ID,Orders&$expand=Orders";

            // Act
            HttpResponseMessage response = await GetResponse(uri, AcceptJson);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(42, result["ID"]);
            Assert.Null(result["Name"]);
            Assert.NotNull(result["Orders"]);
        }

        [Fact]
        public async Task SelectExpand_Works_WithLevels()
        {
            // Arrange
            string uri = "/api/?id=44&$select=ID&$expand=PreviousCustomer($levels=2)";

            // Act
            HttpResponseMessage response = await GetResponse(uri, AcceptJson);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(44, result["ID"]);
            Assert.Equal(43, result["PreviousCustomer"]["ID"]);
            Assert.Equal(42, result["PreviousCustomer"]["PreviousCustomer"]["ID"]);
            Assert.Null(result["PreviousCustomer"]["PreviousCustomer"]["PreviousCustomer"]);
        }

        [Fact]
        public async Task SelectExpand_Works_ForSelectAction_WithNamespaceQualifiedName()
        {
            // Arrange
            const string URI = "/odata2/Players?$select=Name,Default.*";

            // Act
            HttpResponseMessage response = await GetResponse(URI, AcceptJsonFullMetadata);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(responseString);
            Assert.Equal(5, result["value"].Count());
            for (int i = 0; i < 5; i++)
            {
                Assert.Equal("http://localhost/odata2/Players(" + i + ")/Default.PlayerAction1", result["value"][i]["#Default.PlayerAction1"]["target"]);
                Assert.Equal("http://localhost/odata2/Players(" + i + ")/Default.PlayerAction2", result["value"][i]["#Default.PlayerAction2"]["target"]);
                Assert.Equal("http://localhost/odata2/Players(" + i + ")/Default.PlayerAction3", result["value"][i]["#Default.PlayerAction3"]["target"]);
                Assert.Equal("http://localhost/odata2/Players(" + i + ")/Default.PlayerFunction1()", result["value"][i]["#Default.PlayerFunction1"]["target"]);
                Assert.Equal("http://localhost/odata2/Players(" + i + ")/Default.PlayerFunction2()", result["value"][i]["#Default.PlayerFunction2"]["target"]);
            }
        }

        [Fact]
        public async Task SelectExpand_Works_ForSelectInstanceFunction()
        {
            // Arrange
            const string URI = "/odata2/Players?$select=Name,Default.PlayerFunction1";

            // Act
            HttpResponseMessage response = await GetResponse(URI, AcceptJsonFullMetadata);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(responseString);
            Assert.Equal(5, result["value"].Count());
        }

        [Fact]
        public async Task SelectExpand_Works_ForSelectCaseSensitivityProperties()
        {
            // Arrange
            const string URI = "/odata2/Players?$select=Title,TITLE";

            // Act
            HttpResponseMessage response = await GetResponse(URI, AcceptJsonFullMetadata);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(responseString);
            Assert.Equal(5, result["value"].Count());
        }

        [Theory]
        [InlineData("Default.Container.*")]
        [InlineData("Container.*")]
        public async Task SelectExpand_DoesnotWork_ForSelectAction_WithNonNamespaceQualifiedName(string nonNamespaceQualifiedName)
        {
            // Arrange
            string uri = "/odata2/Players?$select=Name," + nonNamespaceQualifiedName;

            // Act
            HttpResponseMessage response = await GetResponse(uri, AcceptJsonFullMetadata);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Contains("The query specified in the URI is not valid. " +
                String.Format("Can not resolve the segment identifier '{0}' in query option.", nonNamespaceQualifiedName),
                responseString);
        }

        private Task<HttpResponseMessage> GetResponse(string uri, string acceptHeader)
        {
            var controllers = new[] {
                typeof(SelectExpandTestCustomersController),
                typeof(SelectExpandTestCustomersAliasController),
                typeof(PlayersController),
                typeof(NonODataSelectExpandTestCustomersController),
                typeof(AttributedSelectExpandCustomersController),
                typeof(SelectExpandTestCustomer),
                typeof(SelectExpandTestSpecialCustomer),
                typeof(SelectExpandTestCustomerWithAlias),
                typeof(SelectExpandTestOrder),
                typeof(SelectExpandTestSpecialOrder),
                typeof(SelectExpandTestSpecialOrderWithAlias),
                typeof(ReferenceNavigationPropertyExpandFilterController),
            };

            var server = TestServerFactory.Create(controllers, (config) =>
            {
#if NETFX
                config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
#endif
                config.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
                config.MapODataServiceRoute("odata", "odata", GetModel());
                config.MapODataServiceRoute("odata-inheritance", "odata-inheritance", GetModelWithInheritance());
                config.MapODataServiceRoute("odata-alias", "odata-alias", GetModelWithCustomerAlias());
                config.MapODataServiceRoute("odata-alias2-inheritance", "odata-alias2-inheritance", GetModelWithCustomerAliasAndInheritance());
                config.MapODataServiceRoute("odata2", "odata2", GetModelWithOperations());
                config.MapODataServiceRoute("odata-expandfilter", "odata-expandfilter", GetModelWithReferenceNavigationPropertyFilter());
#if NETCORE
                config.MapRoute("api", "api/{controller}", new { controller = "NonODataSelectExpandTestCustomers", action="Get" });
#else
                config.Routes.MapHttpRoute("api", "api/{controller}", new { controller = "NonODataSelectExpandTestCustomers" });
#endif
                config.EnableDependencyInjection();
            });

            HttpClient client = TestServerFactory.CreateClient(server);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost" + uri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));
#if NETFX
            request.SetConfiguration(server.Configuration);
#endif
            return client.SendAsync(request);
        }

        private static void ValidateCustomer(JToken customer)
        {
            Assert.Equal(42, customer["ID"]);
            Assert.Null(customer["Name"]);
            var orders = customer["Orders"] as JArray;
            Assert.NotNull(orders);
            Assert.Equal(2, orders.Count);
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
            Assert.Single(orders);
            var order = orders[0];
            Assert.Equal(24, order["ID"]);
            Assert.Equal(100, order["AmountAlias"]);
        }

        private IEdmModel GetModel()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<SelectExpandTestCustomer>("SelectExpandTestCustomers");
            builder.EntitySet<SelectExpandTestOrder>("SelectExpandTestOrders");
            builder.Ignore<SelectExpandTestSpecialCustomer>();
            builder.Ignore<SelectExpandTestSpecialOrder>();
            return builder.GetEdmModel();
        }

        private IEdmModel GetModelWithInheritance()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
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

        private IEdmModel GetModelWithOperations()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
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

        private IEdmModel GetModelWithReferenceNavigationPropertyFilter()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<SelectExpandTestCustomer>("ReferenceNavigationPropertyExpandFilter");
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
                SelectExpandTestOrder anotherOrder = new SelectExpandTestSpecialOrder
                {
                    ID = 28,
                    Amount = 10,
                    Customer = customer,
                };
                customer.Orders = new[] { order, anotherOrder };

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

        public SelectExpandTestCustomerAddress Address { get; set; }

        public int ID { get; set; }

        public string Name { get; set; }

        public SelectExpandTestOrder[] Orders { get; set; }

        public SelectExpandTestCustomer PreviousCustomer { get; set; }
    }

    public class SelectExpandTestCustomerAddress
    {
        public string City { get; set; }
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

#if NETCORE // Use this attribute to help ASP.NET Core to fiter on the action.
    [AttributeUsage(AttributeTargets.Method)]
    public class IdSpecificAttribute : Attribute, IActionConstraint
    {
        public int Order => 0;

        public bool Accept(ActionConstraintContext context)
        {
            return context.RouteContext.HttpContext.Request.Query.ContainsKey("id");
        }
    }
#endif

    public class NonODataSelectExpandTestCustomersController : TestNonODataController
    {
        [EnableQuery]
        public IEnumerable<SelectExpandTestCustomer> Get()
        {
            return SelectExpandTestCustomer.Customers;
        }

        [EnableQuery]
#if NETCORE
        [IdSpecific]
#endif
        public SingleResult Get(int id)
        {
            IQueryable<SelectExpandTestCustomer> singleCustomer = SelectExpandTestCustomer.Customers
                .Where(c => c.ID == id).AsQueryable();
            return SingleResult.Create(singleCustomer);
        }
    }

    public class ReferenceNavigationPropertyExpandFilterController : TestODataController
    {
        [EnableQuery(HandleReferenceNavigationPropertyExpandFilter = true)]
        public ITestActionResult Get()
        {
            return Ok(SelectExpandTestCustomer.Customers);
        }
    }

    public class AttributedSelectExpandCustomersController : TestNonODataController
    {
        public ITestActionResult Get(ODataQueryOptions<AttributedSelectExpandCustomer> options)
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

    public class PlayersController : TestODataController
    {
        private IList<Player> players = Enumerable.Range(0, 5).Select(i =>
                    new Player
                    {
                        Id = i,
                        Name = "PayerName " + i,
                        Category = "Category " + i,
                        Address = "Address " + i,
                        Title = "Title" + i,
                        TITLE = "TITLE" + i
                    }).ToList();

        [EnableQuery]
        public ITestActionResult Get()
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
        public string Title { get; set; }
        public string TITLE { get; set; }
    }
}
