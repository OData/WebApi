using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.NestedPaths
{
    public class NestedPathsTests: WebHostTestBase
    {
        public NestedPathsTests(WebHostTestFixture fixture)
            : base(fixture)
        { }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(
                typeof(CustomersController),
                typeof(TopCustomerController));
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("nestedpaths", "nestedpaths", GetEdmModel(configuration));
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();

            builder.EntitySet<Customer>("Customers");
            builder.Singleton<Customer>("TopCustomer");

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task FetchingEntitySetWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/nestedpaths/Customers", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(result);

            JsonAssert.ArrayLength(10, "value", result);
            JArray customers = (JArray)result["value"];

            for (int i = 0; i < 10; i++)
            {
                Assert.Equal(i, customers[i]["Id"].ToObject<int>());
                Assert.Equal($"customer{i}", customers[i]["Name"].ToObject<string>());
                AssertJsonAddress(customers[i]["HomeAddress"] as JObject, $"City{i}", $"Street{i}");
                JsonAssert.ArrayLength(2, "OtherAddresses", customers[i] as JObject);
                AssertJsonAddress(customers[i]["OtherAddresses"][0] as JObject, $"CityA{i}", $"StreetA{i}");
                AssertJsonAddress(customers[i]["OtherAddresses"][1] as JObject, $"CityB{i}", $"StreetB{i}");
            }
        }

        [Fact]
        public async Task FetchingEntityWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/nestedpaths/Customers(2)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject customer = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(customer);

            Assert.Equal(2, customer["Id"].ToObject<int>());
            Assert.Equal("customer2", customer["Name"].ToObject<string>());
            AssertJsonAddress(customer["HomeAddress"] as JObject, "City2", "Street2");
            JsonAssert.ArrayLength(2, "OtherAddresses", customer);
            AssertJsonAddress(customer["OtherAddresses"][0] as JObject, "CityA2", "StreetA2");
            AssertJsonAddress(customer["OtherAddresses"][1] as JObject, "CityB2", "StreetB2");
        }

        [Fact]
        public async Task FetchingEntityPrimitivePropertyWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/nestedpaths/Customers(2)/Name", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject name = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(name);

            Assert.Equal("customer2", name["value"].ToObject<string>());
        }
        
        [Fact]
        public async Task FetchingPrimitiveCollectionPropertyWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/nestedpaths/Customers(1)/Hobbies", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(result);

            JsonAssert.ArrayLength(2, "value", result);
            JArray hobbies = (JArray)result["value"];

            Assert.Equal("Running1", hobbies[0].ToObject<string>());
            Assert.Equal("Swimming1", hobbies[1].ToObject<string>());
        }

        [Fact]
        public async Task FetchingComplexPropertyWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/nestedpaths/Customers(2)/HomeAddress", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject address = JObject.Parse(await response.Content.ReadAsStringAsync());
            AssertJsonAddress(address, "City2", "Street2");
        }

        [Fact]
        public async Task FetchingPropertyOfComplexPropertyWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/nestedpaths/Customers(3)/HomeAddress/Street", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject street = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(street);

            Assert.Equal("Street3", street["value"].ToObject<string>());
        }

        [Fact]
        public async Task FetchingComplexCollectionPropertyWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/nestedpaths/Customers(1)/OtherAddresses", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(result);

            JsonAssert.ArrayLength(2, "value", result);
            JArray addresses = (JArray)result["value"];

            AssertJsonAddress(addresses[0] as JObject, "CityA1", "StreetA1");
            AssertJsonAddress(addresses[1] as JObject, "CityB1", "StreetB1");
        }

        [Fact]
        public async Task FetchingNavigationPropertyWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/nestedpaths/Customers(2)/FavoriteProduct", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject product = JObject.Parse(await response.Content.ReadAsStringAsync());
            AssertJsonProduct(product, 2, "Product2");
        }

        [Fact]
        public async Task FetchingPropertyOfNavigationPropertyWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/nestedpaths/Customers(2)/FavoriteProduct/Name", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject name = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(name);

            Assert.Equal("Product2", name["value"].ToObject<string>());
        }

        [Fact]
        public async Task FetchingCollectionNavigationPropertyWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/nestedpaths/Customers(1)/Products", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(result);

            JsonAssert.ArrayLength(2, "value", result);
            JArray products = (JArray)result["value"];

            AssertJsonProduct(products[0] as JObject, 101, "Product101");
            AssertJsonProduct(products[1] as JObject, 102, "Product102");
        }

        [Fact]
        public async Task FetchingEntityFromNavigationPropertyWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/nestedpaths/Customers(2)/Products(102)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject product = JObject.Parse(await response.Content.ReadAsStringAsync());
            AssertJsonProduct(product, 102, "Product102");
        }

        [Fact]
        public async Task FetchingPropertyOfEntityFromCollectionNavigationPropertyWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/nestedpaths/Customers(2)/Products(103)/Name", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject name = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(name);

            Assert.Equal("Product103", name["value"].ToObject<string>());
        }

        [Fact]
        public async Task FetchingSingletonWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/nestedpaths/TopCustomer", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject customer = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(customer);

            Assert.Equal(1, customer["Id"].ToObject<int>());
            Assert.Equal("customer1", customer["Name"].ToObject<string>());
            AssertJsonAddress(customer["HomeAddress"] as JObject, "City1", "Street1");
            JsonAssert.ArrayLength(2, "OtherAddresses", customer);
            AssertJsonAddress(customer["OtherAddresses"][0] as JObject, "CityA1", "StreetA1");
            AssertJsonAddress(customer["OtherAddresses"][1] as JObject, "CityB1", "StreetB1");
        }

        [Fact]
        public async Task NavigatingFromSingletonWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/nestedpaths/TopCustomer/Products(101)/Name", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            JObject name = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(name);

            Assert.Equal("Product101", name["value"].ToObject<string>());
        }

        [Fact]
        public async Task DollarValueWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/nestedpaths/Customers(2)/Name/$value", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/plain"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            string name = await response.Content.ReadAsStringAsync();

            Assert.Equal("customer2", name);
        }

        [Fact]
        public async Task DollarCountWorks()
        {
            // Arrange
            string queryUrl = string.Format("{0}/nestedpaths/Customers(2)/Products/$count", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/plain"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(response.Content);
            string count = await response.Content.ReadAsStringAsync();

            Assert.Equal("2", count);
        }

        private static void AssertJsonAddress(JObject address, string city, string street)
        {
            Assert.NotNull(address);
            Assert.Equal(city, address["City"].ToObject<string>());
            Assert.Equal(street, address["Street"].ToObject<string>());
        }

        private static void AssertJsonProduct(JObject product, int id, string name)
        {
            Assert.NotNull(product);
            Assert.Equal(id, product["Id"].ToObject<int>());
            Assert.Equal(name, product["Name"].ToObject<string>());
        }
    }

    public class CustomersController
    {
        public IList<Customer> Customers { get; set; }

        public CustomersController()
        {
            Customers = Enumerable.Range(0, 10).Select(i => new Customer
            {
                Id = i,
                Name = $"customer{i}",
                Hobbies = new List<string> { $"Running{i}", $"Swimming{i}"},
                HomeAddress = new Address { City = $"City{i}", Street = $"Street{i}" },
                OtherAddresses = new List<Address>()
                {
                    new Address { City = $"CityA{i}", Street = $"StreetA{i}" },
                    new Address { City = $"CityB{i}", Street = $"StreetB{i}" }
                },
                FavoriteProduct = new Product { Id = i, Name = $"Product{i}" },
                Products = new List<Product>
                {
                    new Product { Id = 100 + i, Name = $"Product{100 + i }"},
                    new Product { Id = 101 + i, Name = $"Product{101 + i }"}
                }
            }).ToList();
        }

        [EnableQuery]
        [EnableNestedPaths]
        public IQueryable<Customer> Get()
        {
            return Customers.AsQueryable();
        }
    }

    public class TopCustomerController
    {
        [EnableNestedPaths]
        public SingleResult<Customer> Get()
        {
            var customer = new Customer
            {
                Id = 1,
                Name = "customer1",
                Hobbies = new List<string> { "Running1", "Swimming1" },
                HomeAddress = new Address { City = "City1", Street = "Street1" },
                OtherAddresses = new List<Address>()
                {
                    new Address { City = "CityA1", Street = "StreetA1" },
                    new Address { City = "CityB1", Street = "StreetB1" }
                },
                FavoriteProduct = new Product { Id = 1, Name = "Product1" },
                Products = new List<Product>
                {
                    new Product { Id = 101, Name = "Product101" },
                    new Product { Id = 102, Name = "Product102" }
                }
            };

            return SingleResult.Create<Customer>(new Customer[] { customer }.AsQueryable());
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> Hobbies { get; set; }
        public Address HomeAddress { get; set; }
        public List<Address> OtherAddresses { get; set; }
        public Product FavoriteProduct { get; set; }
        public List<Product> Products { get; set; }
    }

    public class Address
    {
        public string City { get; set; }
        public string Street { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
