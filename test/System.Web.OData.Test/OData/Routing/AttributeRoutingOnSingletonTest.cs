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
    public class AttributeRoutingOnSingletonTest
    {
        [Theory]
        [InlineData("GET", "http://localhost/Mary", "GetSingleMary")]
        [InlineData("GET", "http://localhost/Mary/ID", "GetIDOfMary")]
        [InlineData("GET", "http://localhost/Mary/Address/City", "GetCityOfMary")]
        [InlineData("GET", "http://localhost/Mary/NS.SpecialCustomer", "CastMaryToSpecialCustomer")]
        [InlineData("GET", "http://localhost/Mary/Orders", "GetOrdersOfMary")]
        [InlineData("GET", "http://localhost/Mary/Orders(24)", "GetAParticularOrder(24)OfMary")]
        [InlineData("PATCH", "http://localhost/Mary", "PatchMary")]
        [InlineData("POST", "http://localhost/Mary/NS.upgrade", "InvokeODataAction_Upgrade")] // action bound to entity
        [InlineData("GET", "http://localhost/Mary/NS.IsUpgradedWithParam(city='Redmond')", "IsUpgradedWithParam_Redmond")] // function bound to entity
        [InlineData("GET", "http://localhost/Mary/NS.SpecialCustomer/NS.IsSpecialUpgraded()", "IsSpecialUpgraded")] // function bound to derived entity type
        public async Task AttriubteRoutingOnSingleton_SelectsExpectedControllerAndAction(string method, string requestUri,
            string expectedResult)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();

            var controllers = new[] { typeof(MaryController), typeof(MaryOrdersController) };
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

        private class AttributeRoutingTestODataResponse
        {
            public string Value { get; set; }
        }

        // controllers
        public class MaryController : ODataController
        {
            [ODataRoute("Mary")]
            public string GetSingleMary()
            {
                return "GetSingleMary";
            }

            [ODataRoute("Mary/ID")]
            public string GetID()
            {
                return "GetIDOfMary";
            }

            [ODataRoute("Mary/Address/City")]
            public string GetCity()
            {
                return "GetCityOfMary";
            }

            [HttpGet]
            [ODataRoute("Mary/NS.SpecialCustomer")]
            public string SpecialCustomer()
            {
                return "CastMaryToSpecialCustomer";
            }

            [HttpPatch]
            [ODataRoute("Mary")]
            public string PatchCustomer()
            {
                return "PatchMary";
            }

            [HttpPost]
            [ODataRoute("Mary/NS.upgrade")]
            public string InvokeODataAction_Upgrade()
            {
                return "InvokeODataAction_Upgrade";
            }

            [HttpGet]
            [ODataRoute("Mary/NS.IsUpgradedWithParam(city={city})")]
            public string IsUpgradedWithParam([FromODataUri]string city)
            {
                return "IsUpgradedWithParam_" + city;
            }

            [HttpGet]
            [ODataRoute("Mary/NS.SpecialCustomer/NS.IsSpecialUpgraded()")]
            public string IsSpecialUpgraded()
            {
                return "IsSpecialUpgraded";
            }
        }

        [ODataRoutePrefix("Mary/Orders")]
        public class MaryOrdersController : ODataController
        {
            [ODataRoute]
            public string GetOrders()
            {
                return "GetOrdersOfMary";
            }

            [ODataRoute("({orderId})")]
            public string GetAParticularOrder([FromODataUri]int orderId)
            {
                return "GetAParticularOrder(" + orderId + ")OfMary" ;
            }
        }
    }
}
