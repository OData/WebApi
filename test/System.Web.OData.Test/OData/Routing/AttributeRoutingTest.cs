// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Extensions;
using System.Web.OData.TestCommon;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing
{
    public class AttributeRoutingTest
    {
        [Theory]
        [InlineData("GET", "http://localhost/$metadata", "GetMetadata")]
        [InlineData("GET", "http://localhost/", "GetServiceDocument")]
        [InlineData("GET", "http://localhost/Customers", "GetAllCustomers")]
        [InlineData("GET", "http://localhost/Customers()", "GetAllCustomers")]
        [InlineData("GET", "http://localhost/Customers(42)", "GetOneCustomer_42")]
        [InlineData("GET", "http://localhost/Customers(42)/ID", "GetIDOfACustomer_42")]
        [InlineData("GET", "http://localhost/Customers(42)/Address/City", "GetCityOfACustomer_42")]
        [InlineData("GET", "http://localhost/Customers/NS.SpecialCustomer", "GetCustomersFromSpecialCustomer")]
        [InlineData("POST", "http://localhost/Customers/NS.SpecialCustomer/", "PostCustomerFromSpecialCustomer")]
        [InlineData("GET", "http://localhost/Customers(42)/Orders", "GetAllOrdersOfACustomer_42")]
        [InlineData("GET", "http://localhost/Customers(42)/Orders(24)", "GetAParticularOrder_24_OfACustomer_42")] // containment scenario
        [InlineData("POST", "http://localhost/Customers", "CreateCustomer")] // use explicit http verbs attribute on the method
        [InlineData("PATCH", "http://localhost/Customers(42)", "PatchCustomer_42")] // use implicit http verb through method name convention
        [InlineData("POST", "http://localhost/Customers(42)/NS.upgrade", "InvokeODataAction_Upgrade_42")] // action bound to entity
        [InlineData("GET", "http://localhost/Customers(42)/NS.IsUpgradedWithParam(city='Redmond')", "IsUpgradedWithParam_Redmond")] // function bound to entity
        [InlineData("GET", "http://localhost/Customers/NS.IsAnyUpgraded", "IsAnyUpgraded")] // function bound to entity collection
        [InlineData("GET", "http://localhost/Customers/NS.IsAnyUpgraded()", "IsAnyUpgraded")] // function bound to entity collection
        [InlineData("GET", "http://localhost/Customers(42)/NS.SpecialCustomer/NS.IsSpecialUpgraded()", "IsSpecialUpgraded_42")] // function bound to derived entity type
        [InlineData("GET", "http://localhost/Customers(22)/NS.GetSalary()", "GetSalary_22")] // call function on base entity type
        [InlineData("GET", "http://localhost/Customers(12)/NS.SpecialCustomer/NS.GetSalary()", "GetSalaryFromSpecialCustomer_12")] // call function on derived entity type
        public async Task AttributeRouting_SelectsExpectedControllerAndAction(string method, string requestUri,
            string expectedResult)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();

            var controllers = new[] { typeof(CustomersController), typeof(MetadataAndServiceController), typeof(OrdersController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new MockAssembly(controllers));

            HttpConfiguration config = new HttpConfiguration();
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            config.Services.Replace(typeof(IAssembliesResolver), resolver);

            config.MapODataServiceRoute("odata", "", model.Model);

            HttpServer server = new HttpServer(config);
            config.EnsureInitialized();

            HttpClient client = new HttpClient(server);
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), requestUri);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                Assert.False(true, await response.Content.ReadAsStringAsync());
            }
            var result = await response.Content.ReadAsAsync<AttributeRoutingTestODataResponse>();
            Assert.Equal(expectedResult, result.Value);
        }

        [Fact]
        public async Task AttributeRouting_QueryProperty_AfterCallBoundFunction()
        {
            // Arrange
            const string RequestUri = @"http://localhost/Customers(12)/NS.GetOrder(orderId=4)/Amount";
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();

            HttpConfiguration config = new[] { typeof(CustomersController) }.GetHttpConfiguration();
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            config.MapODataServiceRoute("odata", "", model.Model);

            HttpServer server = new HttpServer(config);
            config.EnsureInitialized();

            HttpClient client = new HttpClient(server);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RequestUri);

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("{\r\n  \"@odata.context\":\"http://localhost/$metadata#Edm.Int32\",\"value\":56\r\n}",
                responseString);
        }

        private class AttributeRoutingTestODataResponse
        {
            public string Value { get; set; }
        }

        public class CustomersController : ODataController
        {
            [ODataRoute("Customers")]
            public string GetAllCustomers()
            {
                return "GetAllCustomers";
            }

            [ODataRoute("Customers/NS.SpecialCustomer")]
            public string GetCustomersFromSpecialCustomer()
            {
                return "GetCustomersFromSpecialCustomer";
            }

            [HttpPost]
            [ODataRoute("Customers/NS.SpecialCustomer")]
            public string PostCustomerFromSpecialCustomer()
            {
                return "PostCustomerFromSpecialCustomer";
            }

            [HttpPost]
            [ODataRoute("Customers")]
            public string CreateCustomer()
            {
                return "CreateCustomer";
            }

            [ODataRoute("Customers({customerKeyProperty})")]
            public string GetOneCustomer([FromODataUri]int customerKeyProperty)
            {
                return "GetOneCustomer_" + customerKeyProperty;
            }

            [ODataRoute("Customers({key})")]
            public string PatchCustomer([FromODataUri] int key)
            {
                return "PatchCustomer_" + key;
            }

            [ODataRoute("Customers({id})/ID")]
            public string GetIDOfACustomer([FromODataUri]int id)
            {
                return "GetIDOfACustomer_" + id;
            }

            [ODataRoute("Customers({id})/Address/City")]
            public string GetCityOfACustomer([FromODataUri]int id)
            {
                return "GetCityOfACustomer_" + id;
            }

            [HttpPost]
            [ODataRoute("Customers({id})/NS.upgrade")]
            public string InvokeODataAction_Upgrade([FromODataUri]int id)
            {
                return "InvokeODataAction_Upgrade_" + id;
            }

            [HttpGet]
            [ODataRoute("Customers({id})/NS.IsUpgradedWithParam(city={city})")]
            public string IsUpgradedWithParam([FromODataUri] int id, [FromODataUri]string city)
            {
                return "IsUpgradedWithParam_" + city;
            }

            [HttpGet]
            [ODataRoute("Customers/NS.IsAnyUpgraded()")]
            public string IsAnyUpgraded()
            {
                return "IsAnyUpgraded";
            }

            [HttpGet]
            [ODataRoute("Customers({id})/NS.SpecialCustomer/NS.IsSpecialUpgraded()")]
            public string IsSpecialUpgraded([FromODataUri] int id)
            {
                return "IsSpecialUpgraded_" + id;
            }

            [HttpGet]
            [ODataRoute("Customers({id})/NS.GetSalary()")]
            public string GetSalary([FromODataUri] int id)
            {
                return "GetSalary_" + id;
            }

            [HttpGet]
            [ODataRoute("Customers({id})/NS.SpecialCustomer/NS.GetSalary()")]
            public string GetSalaryFromSpecialCustomer([FromODataUri] int id)
            {
                return "GetSalaryFromSpecialCustomer_" + id;
            }

            [HttpGet]
            [ODataRoute("Customers({id})/NS.GetOrder(orderId={orderId})/Amount")]
            public IHttpActionResult GetAmountFromOrder(int id, int orderId)
            {
                return Ok(id + (orderId * 11));
            }
        }

        [ODataRoutePrefix("Customers({customerId})/Orders")]
        public class OrdersController : ODataController
        {
            [ODataRoute]
            public string GetAllOrdersOfACustomer([FromODataUri]int customerId)
            {
                return "GetAllOrdersOfACustomer_" + customerId;
            }

            [ODataRoute("({orderId})")]
            public string GetAParticularOrderOfACustomer([FromODataUri]int customerId, [FromODataUri]int orderId)
            {
                return "GetAParticularOrder_" + orderId + "_OfACustomer_" + customerId;
            }
        }

        public class MetadataAndServiceController : ODataController
        {
            [ODataRoute("$metadata")]
            public string GetMetadata()
            {
                return "GetMetadata";
            }

            [ODataRoute("")]
            public string GetServiceDocument()
            {
                return "GetServiceDocument";
            }
        }
    }
}
