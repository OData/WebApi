//-----------------------------------------------------------------------------
// <copyright file="AttributeRoutingTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
#else
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Extensions;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Routing
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
        [InlineData("GET", "http://localhost/Customers(12)/NS.GetCustomer(customer=@p)?@p={\"@odata.type\":\"%23NS.Customer\",\"ID\":9,\"City\":\"MyCity\"}",
            "GetCustomer(ID=9,City=MyCity,)")]
        [InlineData("GET", "http://localhost/Customers(42)/Account/DynamicPropertyName", "GetDynamicPropertyFromAccount_42_DynamicPropertyName")]
        [InlineData("GET", "http://localhost/Customers(42)/Orders(24)/DynamicPropertyName", "GetDynamicPropertyFromOrder_42_24_DynamicPropertyName")]
        [InlineData("GET", "http://localhost/Customers/NS.GetWholeSalary(minSalary=7)", "GetWholeSalary(7,0,9)")]
        [InlineData("GET", "http://localhost/Customers/NS.GetWholeSalary(minSalary=7,maxSalary=1)", "GetWholeSalary(7,1,9)")]
        [InlineData("GET", "http://localhost/Customers/NS.GetWholeSalary(minSalary=7,maxSalary=2,aveSalary=5)", "GetWholeSalary(7,2,5)")]
        public async Task AttributeRouting_SelectsExpectedControllerAndAction(string method, string requestUri,
            string expectedResult)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();

            var controllers = new[] { typeof(CustomersController), typeof(MetadataAndServiceController), typeof(OrdersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "", model.Model);
            });

            HttpClient client = TestServerFactory.CreateClient(server);

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), requestUri);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                Assert.False(true, await response.Content.ReadAsStringAsync());
            }
            var result = await response.Content.ReadAsObject<AttributeRoutingTestODataResponse>();
            Assert.Equal(expectedResult, result.Value);
        }

        [Fact]
        public async Task AttributeRouting_QueryProperty_AfterCallBoundFunction()
        {
            // Arrange
            const string RequestUri = @"http://localhost/Customers(12)/NS.GetOrder(orderId=4)/Amount";
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();

            var controllers = new[] { typeof(CustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "", model.Model);
            });

            HttpClient client = TestServerFactory.CreateClient(server);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            //request.ODataProperties().RouteName = HttpRouteCollectionExtensions.RouteName;

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("{\"@odata.context\":\"http://localhost/$metadata#Edm.Int32\",\"value\":56}",
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
            public string GetIsUpgradedWithParam([FromODataUri] int id, [FromODataUri]string city)
            {
                return "IsUpgradedWithParam_" + city;
            }

            [HttpGet]
            [ODataRoute("Customers/NS.IsAnyUpgraded()")]
            public string GetIsAnyUpgraded()
            {
                return "IsAnyUpgraded";
            }

            [HttpGet]
            [ODataRoute("Customers({id})/NS.SpecialCustomer/NS.IsSpecialUpgraded()")]
            public string GetIsSpecialUpgraded([FromODataUri] int id)
            {
                return "IsSpecialUpgraded_" + id;
            }

            //[HttpGet]
            [ODataRoute("Customers({id})/NS.GetSalary()")]
            public string GetSalary([FromODataUri] int id)
            {
                return "GetSalary_" + id;
            }

            [HttpGet]
            [ODataRoute("Customers/NS.GetWholeSalary(minSalary={min})")]
            public string GetWholeSalaryWithOptionalParameters(int min)
            {
                return GetWholeSalaryWithOptionalParameters(min, 0);
            }

            [HttpGet]
            [ODataRoute("Customers/NS.GetWholeSalary(minSalary={min},maxSalary={max})")]
            public string GetWholeSalaryWithOptionalParameters(int min, int max)
            {
                return GetWholeSalaryWithOptionalParameters(min, max, 9);
            }

            [HttpGet]
            [ODataRoute("Customers/NS.GetWholeSalary(minSalary={min},maxSalary={max},aveSalary={ave})")]
            public string GetWholeSalaryWithOptionalParameters(int min, int max, int ave)
            {
                return "GetWholeSalary(" + min + "," + max + "," + ave + ")";
            }

            [HttpGet]
            [ODataRoute("Customers({id})/NS.SpecialCustomer/NS.GetSalary()")]
            public string GetSalaryFromSpecialCustomer([FromODataUri] int id)
            {
                return "GetSalaryFromSpecialCustomer_" + id;
            }

            [HttpGet]
            [ODataRoute("Customers({id})/NS.GetOrder(orderId={orderId})/Amount")]
            public int GetAmountFromOrder(int id, int orderId)
            {
                return id + (orderId * 11);
            }

            [HttpGet]
            [ODataRoute("Customers({key})/NS.GetCustomer(customer={customer})")]
            public string GetCustomer(int key, [FromODataUri]EdmEntityObject customer)
            {
                Assert.NotNull(customer);

                StringBuilder sb = new StringBuilder();
                IEnumerable<string> propertyNames = customer.GetChangedPropertyNames();
                foreach (string name in propertyNames)
                {
                    object value;
                    customer.TryGetPropertyValue(name, out value);
                    sb.Append(name + "=").Append(value).Append(",");
                }

                return "GetCustomer(" + sb.ToString() + ")";
            }

            [HttpGet]
            [ODataRoute("Customers({id})/Account/{pName:dynamicproperty}")]
            public string GetDynamicPropertyFromAccount([FromODataUri] int id, [FromODataUri]string pName)
            {
                return "GetDynamicPropertyFromAccount_" + id + "_" + pName;
            }

            [HttpGet]
            [ODataRoute("Customers({cId})/Orders({oId})/{pName:dynamicproperty}")]
            public string GetDynamicPropertyFromOrder([FromODataUri] int cId, [FromODataUri] int oId, [FromODataUri]string pName)
            {
                return "GetDynamicPropertyFromOrder_" + cId + "_" + oId + "_" + pName;
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
