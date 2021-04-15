using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.NestedPaths
{
    public class NestedPathEFTests : WebHostTestBase
    {
        public NestedPathEFTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(
                    typeof(EFCustomersController),
                    typeof(EFOrdersController),
                    typeof(EFProductsController));
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("nestedpaths", "nestedpaths", GetEdmModel(configuration));
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();

            builder.EntitySet<EFCustomer>("EFCustomers");
            builder.EntitySet<EFProduct>("EFProducts");
            builder.EntitySet<EFOrder>("EFOrders");
            builder.Action("ResetDataSource-Customer");
            builder.Action("ResetDataSource-Order");
            builder.Action("ResetDataSource-Product");

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task QueriesDataCorrectlyBasedOnNestedPath()
        {
            // Arrange
            await RestoreData("-Customer");
            string queryUrl = GetFullUrl("EFCustomers(2)/Orders(6)/Products");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            var result = responseObject["value"] as JArray;
            JsonAssert.ArrayLength(2, "value", responseObject);

            AssertJsonProduct(result[0] as JObject, 6, "Product6");
            AssertJsonProduct(result[1] as JObject, 7, "Product7");
        }

        [Fact]
        public async Task QueryOptionsAreAppliedToResultIfEnableQueryProvided()
        {
            // Arrange
            await RestoreData("-Order");
            string queryUrl = GetFullUrl("EFOrders(1)?$expand=Products");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            var products = responseObject["Products"] as JArray;
            JsonAssert.ArrayLength(2, "Products", responseObject);

            AssertJsonProduct(products[0] as JObject, 1, "Product1");
            AssertJsonProduct(products[1] as JObject, 2, "Product2");
        }

        [Fact]
        public async Task SupportsComplexProperties()
        {
            // Arrange
            await RestoreData("-Customer");
            string queryUrl = GetFullUrl("EFCustomers(1)/HomeAddress");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            AssertJsonAddress(responseObject, "City1", "Street1");
        }

        [Fact]
        public async Task SupportsNavigationProperties()
        {
            // Arrange
            await RestoreData("-Customer");
            string queryUrl = GetFullUrl("EFCustomers(1)/FavoriteProduct");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            AssertJsonProduct(responseObject, 1, "Product1");
        }

        [Theory]
        [InlineData("EFCustomers(4)/FavoriteProduct")]
        [InlineData("EFCustomers(4)/Favoriteproduct/Name")]
        [InlineData("EFCustomers(4)/HomeAddress/City")]
        public async Task Returns404IfAccessingNullProperties(string path)
        {
            // Arrange
            await RestoreData("-Customer");
            string queryUrl = GetFullUrl(path);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private string GetFullUrl(string relativeUrl)
        {
            return $"{BaseAddress}/nestedpaths/{relativeUrl}";
        }

        private void AssertJsonProduct(JObject product, int id, string name)
        {
            Assert.NotNull(product);
            Assert.Equal(id, product["Id"].ToObject<int>());
            Assert.Equal(name, product["Name"].ToObject<string>());
        }

        private void AssertJsonAddress(JObject address, string city, string street)
        {
            Assert.NotNull(address);
            Assert.Equal(city, address["City"].ToObject<string>());
            Assert.Equal(street, address["Street"].ToObject<string>());
        }

        private async Task RestoreData(string suffix)
        {
            string requestUri = GetFullUrl($"ResetDataSource{suffix}");
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
        }
    }


    public static class Database
    {
        public static void RestoreData(SampleContext db)
        {
            db.Database.Delete();
            GenerateData(db);
        }

        public static void GenerateData(SampleContext db)
        {
            for (int i = 0; i < 10; i++)
            {
                int id = i + 1;
                db.Products.Add(new EFProduct
                {
                    Id = id,
                    Name = $"Product{id}"
                });
            }

            db.SaveChanges();

            for (int i = 0; i < 9; i++)
            {
                int id = i + 1;
                db.Orders.Add(new EFOrder
                {
                    Id = id,
                    // link order 1 to product 1 and 2, order 2 to product 2 and 3, etc.
                    Products = db.Products.OrderBy(p => p.Id).Skip(i).Take(2).ToList()
                });
            }

            db.SaveChanges();

            for (int i = 0; i < 3; i++)
            {
                int id = i + 1;
                db.Customers.Add(new EFCustomer
                {
                    Id = id,
                    Name = $"Customer{id}",
                    HomeAddress = new EFAddress { City = $"City{id}", Street = $"Street{id}" },
                    FavoriteProduct = db.Products.Where(p => p.Id == id).FirstOrDefault(),
                    // link customer 1 to orders 1 to 3, customer 2 to orders 4 to 6, etc.
                    Orders = db.Orders.OrderBy(o => o.Id).Skip(i * 3).Take(3).ToList()
                });
            }

            db.Customers.Add(new EFCustomer
            {
                Id = 4,
                Name = "Customer4",
                HomeAddress = new EFAddress(),
                FavoriteProduct = null,
                Orders = new List<EFOrder>()
            });

            db.SaveChanges();
        }
    }

    public class SampleContext : DbContext
    {
        public static string ConnectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=NestedPathsTests";

        public SampleContext()
            : base(ConnectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EFOrder>().HasMany(o => o.Products).WithMany();
            modelBuilder.Entity<EFCustomer>().HasMany(c => c.Orders).WithMany();
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<EFCustomer> Customers { get; set; }

        public DbSet<EFOrder> Orders { get; set; }

        public DbSet<EFProduct> Products { get; set; }
    }


    public class EFCustomersController: TestODataController
    {
        SampleContext _db = new SampleContext();

        [EnableNestedPaths]
        public IQueryable<EFCustomer> Get()
        {
            return _db.Customers;
        }

        [HttpGet]
        [ODataRoute("ResetDataSource-Customer")]
        public ITestActionResult ResetDataSource()
        {
            Database.RestoreData(_db);
            return Ok();
        }
    }

    public class EFOrdersController: TestODataController
    {
        SampleContext _db = new SampleContext();

        [EnableNestedPaths]
        [EnableQuery]
        public IQueryable<EFOrder> Get()
        {
            return _db.Orders;
        }

        [HttpGet]
        [ODataRoute("ResetDataSource-Order")]
        public ITestActionResult ResetDataSource()
        {
            Database.RestoreData(_db);
            return Ok();
        }
    }

    public class EFProductsController: TestODataController
    {
        SampleContext _db = new SampleContext();

        [EnableNestedPaths]
        public IQueryable<EFProduct> Get()
        {
            return _db.Products;
        }

        [HttpGet]
        [ODataRoute("ResetDataSource-Product")]
        public ITestActionResult ResetDataSource()
        {
            Database.RestoreData(_db);
            return Ok();
        }
    }

    public class EFCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual EFAddress HomeAddress { get; set; }
        public virtual EFProduct FavoriteProduct { get; set; }
        public virtual List<EFOrder> Orders { get; set; }
    }

    public class EFAddress
    {
        public string City { get; set; }
        public string Street { get; set; }
    }

    public class EFOrder
    {
        public int Id { get; set; }
        public virtual List<EFProduct> Products { get; set; }
    }

    public class EFProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
