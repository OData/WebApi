// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing
{
    public class AttributeRoutingUnboundTest
    {
        private HttpConfiguration _configuration;
        private HttpServer _server;
        private HttpClient _client;

        public AttributeRoutingUnboundTest()
        {
            _configuration = new HttpConfiguration();
            _configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            _configuration.MapODataServiceRoute("odata", "", GetEdmModel(_configuration));

            var controllers = new[] { typeof(ConventionCustomersController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new MockAssembly(controllers));
            _configuration.Services.Replace(typeof(IAssembliesResolver), resolver);
            _server = new HttpServer(_configuration);
            _configuration.EnsureInitialized();
            _client = new HttpClient(_server);
        }

        [Fact]
        public async Task AttributeRouting_TopFunctionWithoutParameters()
        {
            // Arrange
            string requestUri = "http://localhost/GetAllConventionCustomers()";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await _client.SendAsync(request);
            string responseString = response.Content.ReadAsStringAsync().Result;

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
            string responseString = response.Content.ReadAsStringAsync().Result;

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
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("\"@odata.context\":\"http://localhost/$metadata#ConventionOrders/$entity\"", responseString);
            Assert.Contains("\"@odata.type\":\"#System.Web.OData.Routing.ConventionOrder", responseString);
            Assert.Contains("\"OrderName\":\"OrderName 5\"", responseString);
            Assert.Contains("\"Price@odata.type\":\"#Decimal\",\"Price\":13", responseString);
        }

        [Fact]
        public async Task AttriubteRouting_TopActionWithoutParametersOnPrimitiveType()
        {
            // Arrange
            string requestUri = "http://localhost/CreateCollectionMessages";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            // Act
            var response = await _client.SendAsync(request);
            string responseString = response.Content.ReadAsStringAsync().Result;

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
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("\"ID\":908", responseString);
        }

        private IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            ODataConventionModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataConventionModelBuilder>(configuration);
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
            getCustomersById.Parameter<int>("CustomerId");
            getCustomersById.ReturnsFromEntitySet<ConventionCustomer>("ConventionCustomers");

            // Top level function import with two parameters
            FunctionConfiguration getOrder = builder.Function("GetConventionOrderByCustomerIdAndOrderName");
            getOrder.Parameter<int>("CustomerId");
            getOrder.Parameter<string>("OrderName");
            getOrder.ReturnsFromEntitySet<ConventionOrder>("ConventionOrders");

            return builder.GetEdmModel();
        }
    }

    public class ConventionCustomersController : ODataController
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
