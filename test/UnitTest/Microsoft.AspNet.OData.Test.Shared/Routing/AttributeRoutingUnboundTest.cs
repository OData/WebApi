//-----------------------------------------------------------------------------
// <copyright file="AttributeRoutingUnboundTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData.Edm;
using Xunit;
#else
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Routing
{
    public class AttributeRoutingUnboundTest
    {
        private HttpClient _client;

        public AttributeRoutingUnboundTest()
        {
            Type[] controllers = new[] { typeof(ConventionCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                var builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataConventionModelBuilder>(config);
                IEdmModel model = GetEdmModel(builder);

                config.MapODataServiceRoute("odata", "", model);
            });

            _client = TestServerFactory.CreateClient(server);
        }

        [Fact]
        public async Task AttributeRouting_TopFunctionWithoutParameters()
        {
            // Arrange
            string requestUri = "http://localhost/GetAllConventionCustomers()";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("\"@odata.context\":\"http://localhost/$metadata#ConventionCustomers\"", responseString);
            foreach (ConventionCustomer customer in ModelDataBase.Instance.Customers)
            {
                string expected = "\"ID\":" + customer.ID;
                Assert.Contains(expected, responseString);
                expected = "\"Name\":\"" + customer.Name + "\"";
                Assert.Contains(expected, responseString);
            }
        }

        [Fact]
        public async Task AttributeRouting_TopFunctionWithOneParameters()
        {
            // Arrange
            const int CustomerId = 407;// a magic customer id, just for test
            ConventionCustomer expectedCustomer = ModelDataBase.Instance.GetCustomerById(CustomerId); // expected customer instance
            string requestUri = "http://localhost/GetConventionCustomerById(CustomerId=" + CustomerId + ")";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.NotNull(expectedCustomer);
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("\"@odata.context\":\"http://localhost/$metadata#ConventionCustomers/$entity\"", responseString);
            string expect = "\"ID\":" + expectedCustomer.ID;
            Assert.Contains(expect, responseString);
            expect = "\"Name\":\"" + expectedCustomer.Name + "\"";
            Assert.Contains(expect, responseString);
        }

        [Fact]
        public async Task AttributeRouting_TopFunctionWithMoreThanOneParameters()
        {
            // Arrange
            const int CustomerId = 408;// a magic customer id, just for test
            const string OrderName = "OrderName 5";
            string requestUri = "http://localhost/GetConventionOrderByCustomerIdAndOrderName(CustomerId=" + CustomerId + ",OrderName='" + OrderName + "')";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));

            // Act
            var response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("\"@odata.context\":\"http://localhost/$metadata#ConventionOrders/$entity\"", responseString);
            Assert.Contains("\"@odata.type\":\"#Microsoft.AspNet.OData.Test.Routing.ConventionOrder", responseString);
            Assert.Contains("\"OrderName\":\"OrderName 5\"", responseString);
            Assert.Contains("\"Price@odata.type\":\"#Decimal\",\"Price\":13", responseString);
        }

        [Fact]
        public async Task AttributeRouting_TopFunctionWithComplexParameter()
        {
            // Arrange
            string requestUri = "http://localhost/ComplexFunction(address=@p)?@p={\"@odata.type\":\"%23Microsoft.AspNet.OData.Test.Routing.ConventionAddress\",\"Street\":\"NE 24th St.\",\"City\":\"Redmond\",\"ZipCode\":\"911\"}";

            // Act
            var response = await _client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("911", responseString);
        }

        [Fact]
        public async Task AttributeRouting_TopFunctionWithEntityParameter()
        {
            // Arrange
            string requestUri = "http://localhost/EntityFunction(order=@p)?@p={\"@odata.type\":\"%23Microsoft.AspNet.OData.Test.Routing.ConventionOrder\",\"Price\":9.9}";

            // Act
            var response = await _client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("9.9", responseString);
        }

        [Theory]
        [InlineData("", "9")]
        [InlineData("param=1", "1")]
        [InlineData("param=9", "9")]
        public async Task AttributeRouting_TopFunctionWithOptionalParameter(string param, string expected)
        {
            // Arrange
            string requestUri = "http://localhost/OptionalFunction(" + param + ")";

            // Act
            var response = await _client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains(expected, responseString);
        }

        [Fact]
        public async Task AttriubteRouting_TopActionWithoutParametersOnPrimitiveType()
        {
            // Arrange
            string requestUri = "http://localhost/CreateCollectionMessages";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            // Act
            var response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("\"@odata.context\":\"http://localhost/$metadata#Collection(Edm.String)\"",
                responseString);
            Assert.Contains("No. 4\",\"This is a test message No. 5\",\"This is a",
                responseString);
        }

        [Fact]
        public async Task AttributeRouting_TopActionWithSinglePrimitiveParameter()
        {
            // Arrange
            string message = "{ \"value\":908 }";
            string requestUri = "http://localhost/CreateConventionCustomerById";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent(message);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));

            // Act
            var response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("\"ID\":908", responseString);
        }

        [Fact]
        public async Task AttributeRouting_QueryProperty_AfterCallUnboundFunction()
        {
            // Arrange
            const string ExpectPayload = "{\"@odata.context\":\"http://localhost/$metadata#Edm.String\",\"value\":\"Name 7\"}";

            string requestUri = "http://localhost/GetConventionCustomerById(CustomerId=407)/Name";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(ExpectPayload, responseString);
        }

        private IEdmModel GetEdmModel(ODataConventionModelBuilder builder)
        {
            builder.EntitySet<ConventionCustomer>("ConventionCustomers");
            builder.EntitySet<ConventionOrder>("ConventionOrders");
            builder.ComplexType<ConventionPerson>();
            builder.EntityType<ConventionCustomer>().ComplexProperty<ConventionAddress>(c => c.Address);

            // Top level action import
            ActionConfiguration createConventionCustomerById = builder.Action("CreateConventionCustomerById");
            createConventionCustomerById.Parameter<int>("ID");
            createConventionCustomerById.ReturnsFromEntitySet<ConventionCustomer>("ConventionCustomers");

            // Top level action import without parameter and with a collection of primitive return type
            ActionConfiguration topCollectionPrimitiveAction = builder.Action("CreateCollectionMessages");
            topCollectionPrimitiveAction.ReturnsCollection<string>();

            // Top level function import
            FunctionConfiguration getAllCustomers = builder.Function("GetAllConventionCustomers");
            getAllCustomers.ReturnsCollectionFromEntitySet<ConventionCustomer>("ConventionCustomers");

            // Top level function import with one parameter
            FunctionConfiguration getCustomersById = builder.Function("GetConventionCustomerById");
            getCustomersById.IsComposable = true;
            getCustomersById.Parameter<int>("CustomerId");
            getCustomersById.ReturnsFromEntitySet<ConventionCustomer>("ConventionCustomers");

            // Top level function import with two parameters
            FunctionConfiguration getOrder = builder.Function("GetConventionOrderByCustomerIdAndOrderName");
            getOrder.Parameter<int>("CustomerId");
            getOrder.Parameter<string>("OrderName");
            getOrder.ReturnsFromEntitySet<ConventionOrder>("ConventionOrders");

            // Top level function import with complex parameter
            FunctionConfiguration complexFunction = builder.Function("ComplexFunction");
            complexFunction.Parameter<ConventionAddress>("address");
            complexFunction.Returns<string>();

            // Top level function import with entity parameter
            FunctionConfiguration entityFunction = builder.Function("EntityFunction");
            entityFunction.Parameter<ConventionOrder>("order");
            entityFunction.Returns<string>();

            // Top level function import with optional parameter
            FunctionConfiguration optionalFunction = builder.Function("OptionalFunction");
            optionalFunction.Parameter<int>("param").HasDefaultValue("9");
            optionalFunction.Returns<string>();

            return builder.GetEdmModel();
        }
    }

    public class ConventionCustomersController : TestODataController
    {
        // It's a top level function without parameters
        [HttpGet]
        [ODataRoute("GetAllConventionCustomers()")]
        public IEnumerable<ConventionCustomer> GetAllConventionCustomers()
        {
            return ModelDataBase.Instance.Customers;
        }

        // It's a top level function with one parameter
        [ODataRoute("GetConventionCustomerById(CustomerId={CustomerId})")]
        public ConventionCustomer GetConventionCustomerById([FromODataUri]int CustomerId)
        {
            return ModelDataBase.Instance.Customers.Where(c => c.ID == CustomerId).FirstOrDefault();
        }

        [ODataRoute("GetConventionCustomerById(CustomerId={CustomerId})/Name")]
        public ITestActionResult GetNameById([FromODataUri]int CustomerId)
        {
            ConventionCustomer customer = ModelDataBase.Instance.Customers.Where(c => c.ID == CustomerId).FirstOrDefault();
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer.Name);
        }

        // TODO: Remove [FromODataUri] after issue 1734 is fixed
        [ODataRoute("GetConventionOrderByCustomerIdAndOrderName(CustomerId={CustomerId},OrderName={OrderName})")]
        public ConventionOrder GetConventionOrderByCustomerIdAndOrderName(int CustomerId, [FromODataUri]string OrderName)
        {
            ConventionCustomer customer = ModelDataBase.Instance.Customers.Where(c => c.ID == CustomerId).FirstOrDefault();
            return customer.Orders.Where(o => o.OrderName == OrderName).FirstOrDefault();
        }

        // It's an action post call
        [HttpPost]
        [ODataRoute("CreateConventionCustomerById")]
        public ConventionCustomer CreateConventionCustomerById([FromBody]int ID)
        {
            return ModelDataBase.Instance.CreateCustomerById(ID);
        }

        [HttpPost]
        [ODataRoute("CreateCollectionMessages")]
        public IEnumerable<string> CreateCollectionMessages()
        {
            return Enumerable.Range(0, 10).Select(i => "This is a test message No. " + i);
        }

        [HttpGet]
        [ODataRoute("ComplexFunction(address={ad})")]
        public string ComplexFunction([FromODataUri] ConventionAddress ad)
        {
            Assert.NotNull(ad);
            return "{\"Street\":\"" + ad.Street + "\",\"City\":\"" + ad.City + "\",\"ZipCode\":\"" + ad.ZipCode + "\"}";
        }

        [HttpGet]
        [ODataRoute("EntityFunction(order={order})")]
        public string EntityFunction([FromODataUri] ConventionOrder order)
        {
            Assert.NotNull(order);
            return order.Price.ToString(CultureInfo.InvariantCulture);
        }

        [HttpGet]
        [ODataRoute("OptionalFunction()")]
        public string OptionalFunction()
        {
            return OptionalFunction(9);
        }

        [HttpGet]
        [ODataRoute("OptionalFunction(param={p})")]
        public string OptionalFunction(int p)
        {
            return p.ToString();
        }
    }

    public class ModelDataBase
    {
        private IList<ConventionCustomer> _customers = Enumerable.Range(1, 10).Select(i =>
                new ConventionCustomer
                {
                    ID = 400 + i,
                    Name = "Name " + i,
                    Address = new ConventionAddress()
                    {
                        Street = "Street " + i,
                        City = "City " + i,
                        ZipCode = (201100 + i).ToString()
                    },
                    Orders = Enumerable.Range(0, i).Select(j =>
                    new ConventionOrder
                    {
                        OrderName = "OrderName " + j,
                        Price = i + j,
                        OrderGuid = Guid.Empty
                    }).ToList()
                }).ToList();

        public static ModelDataBase Instance = new ModelDataBase();

        private ModelDataBase()
        { }

        public IEnumerable<ConventionCustomer> Customers
        {
            get { return _customers; }
        }

        public ConventionCustomer GetCustomerById(int customerId)
        {
            return _customers.Where(c => c.ID == customerId).FirstOrDefault();
        }

        public ConventionCustomer CreateCustomerById(int customerId)
        {
            ConventionCustomer customer = _customers.Where(c => c.ID == customerId).FirstOrDefault();
            if (customer != null)
            {
                string message = String.Format("Customer with ID {0} already exists.", customerId);
                throw new Exception(message);
            }

            _customers.Add(new ConventionCustomer()
            {
                ID = customerId,
                Name = "Name " + customerId,
                Address = new ConventionAddress()
                {
                    Street = "Street " + customerId,
                    City = "City " + customerId,
                    ZipCode = (201100 + customerId).ToString()
                }
            });

            return GetCustomerById(customerId);
        }

        public ConventionCustomer CreateCustomer(ConventionCustomer customer)
        {
            // Just for tests; Normally, should check to ensure the ID does not exist.
            _customers.Add(customer);
            return customer;
        }
    }

    public class ConventionCustomer
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ConventionAddress Address { get; set; }
        public List<ConventionOrder> Orders { get; set; }
    }

    public class ConventionAddress
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
    }

    public class ConventionOrder
    {
        public string OrderName { get; set; }
        public decimal Price { get; set; }
        public Guid OrderGuid { get; set; }
    }

    public class ConventionPerson
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public bool Male { get; set; }
    }
}
