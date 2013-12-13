// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData.TestCommon;
using System.Web.Http.TestCommon;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Routing
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
        [InlineData("GET", "http://localhost/Customers/NS.SpecialCustomer", "GetAllSpecialCustomers")]
        [InlineData("GET", "http://localhost/Customers(42)/Orders", "GetAllOrdersOfACustomer_42")]
        [InlineData("GET", "http://localhost/Customers(42)/Orders(24)", "GetAParticularOrder_24_OfACustomer_42")] // containment scenario
        [InlineData("POST", "http://localhost/Customers", "CreateCustomer")] // use explicit http verbs attribute on the method
        [InlineData("PATCH", "http://localhost/Customers(42)", "PatchCustomer_42")] // use implicit http verb through method name convention
        [InlineData("POST", "http://localhost/Customers(42)/upgrade", "InvokeODataAction_Upgrade_42")] // action bound to entity
        [InlineData("GET", "http://localhost/Customers(42)/IsUpgradedWithParam(city='Redmond')", "IsUpgradedWithParam_Redmond")] // function bound to entity
        [InlineData("GET", "http://localhost/Customers/IsAnyUpgraded", "IsAnyUpgraded")] // function bound to entity collection
        [InlineData("GET", "http://localhost/Customers/IsAnyUpgraded()", "IsAnyUpgraded")] // function bound to entity collection
        [InlineData("GET", "http://localhost/Customers(42)/NS.SpecialCustomer/IsSpecialUpgraded()", "IsSpecialUpgraded_42")] // function bound to derived entity type
        public async Task AttriubteRouting_SelectsExpectedControllerAndAction(string method, string requestUri,
            string expectedResult)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();

            var controllers = new[] { typeof(CustomersController), typeof(MetadataController), typeof(OrdersController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new MockAssembly(controllers));

            HttpConfiguration config = new HttpConfiguration();
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            config.Services.Replace(typeof(IAssembliesResolver), resolver);

            config.Routes
                .MapODataRoute("odata", "", model.Model)
                .MapODataRouteAttributes(config);

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

            [HttpPost]
            [ODataRoute("Customers")]
            public string CreateCustomer()
            {
                return "CreateCustomer";
            }

            [ODataRoute("Customers/NS.SpecialCustomer")]
            public string GetAllSpecialCustomers()
            {
                return "GetAllSpecialCustomers";
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
            [ODataRoute("Customers({id})/upgrade")]
            public string InvokeODataAction_Upgrade([FromODataUri]int id)
            {
                return "InvokeODataAction_Upgrade_" + id;
            }

            [HttpGet]
            [ODataRoute("Customers({id})/IsUpgradedWithParam(city={city})")]
            public string IsUpgradedWithParam([FromODataUri] int id, [FromODataUri]string city)
            {
                return "IsUpgradedWithParam_" + city;
            }

            [HttpGet]
            [ODataRoute("Customers/IsAnyUpgraded()")]
            public string IsAnyUpgraded()
            {
                return "IsAnyUpgraded";
            }

            [HttpGet]
            [ODataRoute("Customers({id})/NS.SpecialCustomer/IsSpecialUpgraded()")]
            public string IsSpecialUpgraded([FromODataUri] int id)
            {
                return "IsSpecialUpgraded_" + id;
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

        public class MetadataController : ODataController
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
