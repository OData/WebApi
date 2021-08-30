//-----------------------------------------------------------------------------
// <copyright file="EnableNestedPathsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNetCore.TestHost;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test
{
    public class EnableNestedPathsTest
    {
        private readonly ODataDeserializerProvider _deserializerProvider;
        private readonly ODataResourceSetDeserializer _resourceSetDeserializer;
        private readonly ODataResourceDeserializer _resourceDeserializer;
        private readonly ODataPrimitiveDeserializer _primitiveDeserializer;
        private readonly IEdmModel _model;
        private readonly TestServer _server;
        private readonly HttpClient _client;
        private readonly EnableNestedPathsDatabase _db;
        private readonly string _baseUrl = "http://localhost/odata/";

        public EnableNestedPathsTest()
        {
            _deserializerProvider = ODataDeserializerProviderFactory.Create();
            _resourceSetDeserializer = new ODataResourceSetDeserializer(_deserializerProvider);
            _resourceDeserializer = new ODataResourceDeserializer(_deserializerProvider);
            _primitiveDeserializer = new ODataPrimitiveDeserializer();

            ODataModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<EnableNestedPathsCustomer>("EnableNestedPathsCustomers");
            builder.EntitySet<EnableNestedPathsProduct>("EnableNestedPathsProducts");
            builder.Singleton<EnableNestedPathsCustomer>("EnableNestedPathsTopCustomer");
            builder.EntityType<EnableNestedPathsVipCustomer>();

            builder.EntityType<EnableNestedPathsVipCustomer>()
                .Function("GetMostPurchasedProduct")
                .ReturnsFromEntitySet<EnableNestedPathsProduct>("EnableNestedPathsProduct");

            builder.EntityType<EnableNestedPathsProduct>()
                .Collection
                .Action("SetDiscountRate");

            _model = builder.GetEdmModel();

            Type[] controllers = new Type[] {
                typeof(EnableNestedPathsCustomersController),
                typeof(EnableNestedPathsProductsController),
                typeof(EnableNestedPathsTopCustomerController)
            };

            _server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", _model);
                config.Count().OrderBy().Filter().Expand().MaxTop(null).Select();
            });

            _client = TestServerFactory.CreateClient(_server);

            _db = new EnableNestedPathsDatabase();
        }

        [Fact]
        public async Task EnableNestedPaths_ReturnsEntiySetCollection()
        {
            // Arrange
            string url = $"{_baseUrl}EnableNestedPathsCustomers";

            // Act
            HttpResponseMessage response = await _client.GetAsync(url);

            var stream = await response.Content.ReadAsStreamAsync();
            IEnumerable<EnableNestedPathsCustomer> readCustomers = ReadCollectionResponse<EnableNestedPathsCustomer>(stream, _model);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(_db.Customers,
                readCustomers.Cast<EnableNestedPathsCustomer>(),
                new EnableNestedPathsCustomerComparer());
        }

        [Fact]
        public async Task EnableNestedPaths_ReturnsEntityById()
        {
            // Arrange
            string url = $"{_baseUrl}EnableNestedPathsCustomers(2)";

            // Act
            HttpResponseMessage response = await _client.GetAsync(url);

            var stream = await response.Content.ReadAsStreamAsync();
            var entitySet = _model.EntityContainer.FindEntitySet("EnableNestedPathsCustomers");
            var path = new ODataPath(new EntitySetSegment(entitySet));
            var readCustomer = ReadSingleResponse<EnableNestedPathsCustomer>(stream, _model, path);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(_db.Customers.First(c => c.Id == 2),
                readCustomer,
                new EnableNestedPathsCustomerComparer());
        }

        [Fact]
        public async Task EnableNestedPaths_ReturnsSingleton()
        {
            // Arrange
            string url = $"{_baseUrl}EnableNestedPathsTopCustomer";

            // Act
            HttpResponseMessage response = await _client.GetAsync(url);

            var stream = await response.Content.ReadAsStreamAsync();
            var singleton = _model.EntityContainer.FindSingleton("EnableNestedPathsTopCustomer");
            var path = new ODataPath(new SingletonSegment(singleton));
            var readCustomer = ReadSingleResponse<EnableNestedPathsCustomer>(stream, _model, path);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(_db.Customers.First(),
                readCustomer,
                new EnableNestedPathsCustomerComparer());
        }

        [Fact]
        public async Task EnableNestedPaths_ReturnsPropertyOfSingleton()
        {
            // Arrange
            string url = $"{_baseUrl}EnableNestedPathsTopCustomer/FavoriteProduct";

            // Act
            HttpResponseMessage response = await _client.GetAsync(url);

            var stream = await response.Content.ReadAsStreamAsync();
            var entitySet = _model.EntityContainer.FindEntitySet("EnableNestedPathsProducts");
            var path = new ODataPath(new EntitySetSegment(entitySet));
            var readProduct = ReadSingleResponse<EnableNestedPathsProduct>(stream, _model, path);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(_db.Customers.First().FavoriteProduct,
                readProduct,
                new EnableNestedPathsProductComparer());
        }

        [Fact]
        public async Task EnableNestedPaths_ReturnsPrimitiveResults()
        {
            // Arrange
            string url = $"{_baseUrl}EnableNestedPathsCustomers(1)/Name";

            // Act
            HttpResponseMessage response = await _client.GetAsync(url);

            var stream = await response.Content.ReadAsStreamAsync();
            var readName = ReadPrimitiveResponse<string>(stream, _model);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(_db.Customers.First().Name, readName);
        }


        [Theory]
        [InlineData("EnableNestedPathsCustomers(5)")]
        [InlineData("EnableNestedPathsCustomers(5)/Name")]
        // FavoriteProduct is null
        [InlineData("EnableNestedPathsCustomers(2)/FavoriteProduct/Name")]
        [InlineData("EnableNestedPathsCustomers(2)/FavoriteProduct")]
        // Products(3) does not belong to Customers(1)
        [InlineData("EnableNestedPathsCustomers(1)/Products(3)")]
        [InlineData("EnableNestedPathsCustomers(1)/Products(3)/Name")]
        [InlineData("EnableNestedPathsCustomers(2)/HomeAddress")]
        [InlineData("EnableNestedPathsCustomers(3)/HomeAddress/City")]
        [InlineData("EnableNestedPathsCustomers(3)/Products(2)")]
        [InlineData("EnableNestedPathsCustomers(3)/Products(2)/Name")]
        public async Task EnableNestedPaths_Returns404_WhenAccessNonExistentData(string path)
        {
            // Arrange
            string url = $"{_baseUrl}{path}";

            // Act
            HttpResponseMessage response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        // Type Cast is currently not supported
        [InlineData("EnableNestedPathsCustomers(1)/Microsoft.AspNet.OData.Test.EnableNestedPathsVipCustomer")]
        // Functions and actions not supported
        [InlineData("EnableNestedPathsCustomers(1)/GetMostPurchasedProduct()")]
        [InlineData("EnableNestedPathsProducts/SetDiscountRate()")]
        public async Task EnableNestedPaths_Returns404_WhenPathHasUnsupportedSegments(string path)
        {
            // Arrange
            string url = $"{_baseUrl}{path}";

            // Act
            HttpResponseMessage response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(Skip ="Temp")]        
        public async Task EnableNestedPaths_AppliedBeforeEnableQuery()
        {
            // Arrange
            string url = $"{_baseUrl}EnableNestedPathsCustomers(1)/Products?$orderby=Id desc";

            // Act
            HttpResponseMessage response = await _client.GetAsync(url);

            var stream = await response.Content.ReadAsStreamAsync();
            var readCustomer = ReadCollectionResponse<EnableNestedPathsProduct>(stream, _model);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(_db.Customers.First().Products.OrderByDescending(p => p.Id).ToList(),
                readCustomer,
                new EnableNestedPathsProductComparer());
        }

        private IEnumerable<T> ReadCollectionResponse<T>(Stream stream, IEdmModel model)
        {
            ODataMessageWrapper message = new ODataMessageWrapper(stream);
            ODataMessageReader messageReader = new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), model);
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = model };
            IEnumerable readEntities = _resourceSetDeserializer.Read(messageReader, typeof(T[]), readContext) as IEnumerable;
            return readEntities.Cast<T>();
        }

        private T ReadSingleResponse<T>(Stream stream, IEdmModel model, ODataPath path)
        {
            ODataMessageWrapper message = new ODataMessageWrapper(stream);
            ODataMessageReader messageReader = new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), model);
            ODataDeserializerContext readContext = new ODataDeserializerContext() {
                Path = path,
                Model = model,
                ResourceType = typeof(T)
            };
            object readEntity = _resourceDeserializer.Read(messageReader, typeof(T), readContext);
            return (T)readEntity;
        }

        private T ReadPrimitiveResponse<T>(Stream stream, IEdmModel model)
        {
            ODataMessageWrapper message = new ODataMessageWrapper(stream);
            ODataMessageReader messageReader = new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), model);
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = model };
            object readValue = _primitiveDeserializer.Read(messageReader, typeof(T), readContext);
            return (T)readValue;
        }
    }

    class EnableNestedPathsDatabase
    {
        public EnableNestedPathsDatabase()
        {
            var addresses = new List<EnableNestedPathsAddress>()
            {
                new EnableNestedPathsAddress { City = "Addr1" },
                new EnableNestedPathsAddress { City = "Addr2" },
                new EnableNestedPathsAddress { City = null }
            };

            Products = new List<EnableNestedPathsProduct>()
            {
                new EnableNestedPathsProduct()
                {
                    Id = 1,
                    Name = "Prod1",
                },
                new EnableNestedPathsProduct()
                {
                    Id = 2,
                    Name = "Prod2"
                },
                new EnableNestedPathsProduct()
                {
                    Id = 3,
                    Name = "Prod3"
                }
            };

            Customers = new List<EnableNestedPathsCustomer>()
            {
                new EnableNestedPathsCustomer()
                {
                    Id = 1,
                    Name = "Cust1",
                    Products = new List<EnableNestedPathsProduct> { Products[0], Products[1] },
                    Emails = new List<string> { "email1", "email2" },
                    FavoriteProduct = Products[0],
                    HomeAddress = addresses[0],
                    Addresses = new List<EnableNestedPathsAddress> { addresses[0], addresses[1]}
                },
                new EnableNestedPathsCustomer()
                {
                    Id = 2,
                    Name = "Cust2",
                    Products = new List<EnableNestedPathsProduct>() { Products[1], Products[2] },
                    Emails = new List<string>(),
                    FavoriteProduct = null,
                    HomeAddress = null,
                    Addresses = null
                },
                new EnableNestedPathsCustomer()
                {
                    Id = 3,
                    Name = "Cust3",
                    Products = new List<EnableNestedPathsProduct>(),
                    Emails = new List<string>(),
                    FavoriteProduct = null,
                    HomeAddress = addresses[2],
                    Addresses = new List<EnableNestedPathsAddress> { addresses[1], addresses[2] }
                }
            };
        }

        public IList<EnableNestedPathsProduct> Products { get; set; }
        public IList<EnableNestedPathsCustomer> Customers { get; set; }
    }

    class EnableNestedPathsCustomerComparer : IEqualityComparer<EnableNestedPathsCustomer>
    {
        public bool Equals(EnableNestedPathsCustomer x, EnableNestedPathsCustomer y)
        {
            return x.Name == y.Name && x.Id == y.Id;
        }

        public int GetHashCode(EnableNestedPathsCustomer obj)
        {
            throw new NotImplementedException();
        }
    }

    class EnableNestedPathsProductComparer : IEqualityComparer<EnableNestedPathsProduct>
    {
        public bool Equals(EnableNestedPathsProduct x, EnableNestedPathsProduct y)
        {
            return x.Name == y.Name && x.Id == y.Id;
        }

        public int GetHashCode(EnableNestedPathsProduct obj)
        {
            throw new NotImplementedException();
        }
    }

    class EnableNestedPathsCustomersController
    {
        readonly EnableNestedPathsDatabase _db = new EnableNestedPathsDatabase();

        [EnableNestedPaths]
        [EnableQuery]
        public IQueryable<EnableNestedPathsCustomer> Get()
        {
            return _db.Customers.AsQueryable();
        }
    }

    class EnableNestedPathsTopCustomerController
    {
        readonly EnableNestedPathsDatabase _db = new EnableNestedPathsDatabase();

        [EnableNestedPaths]
        public SingleResult<EnableNestedPathsCustomer> Get()
        {
            return new SingleResult<EnableNestedPathsCustomer>(_db.Customers.Where(p => p.Id == 1).AsQueryable());
        }
    }

    class EnableNestedPathsProductsController
    {
        readonly EnableNestedPathsDatabase _db = new EnableNestedPathsDatabase();

        [EnableNestedPaths]
        public IQueryable<EnableNestedPathsProduct> Get()
        {
            return _db.Products.AsQueryable();
        }
    }

    class EnableNestedPathsTopProductController
    {
        readonly EnableNestedPathsDatabase _db = new EnableNestedPathsDatabase();

        [EnableNestedPaths]
        public SingleResult<EnableNestedPathsProduct> Get()
        {
            return new SingleResult<EnableNestedPathsProduct>(_db.Products.Where(p => p.Id == 1).AsQueryable());
        }
    }

    class EnableNestedPathsCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<string> Emails { get; set; }
        public IList<EnableNestedPathsProduct> Products { get; set; }
        public EnableNestedPathsProduct FavoriteProduct { get; set; }
        public EnableNestedPathsAddress HomeAddress { get; set; }
        public List<EnableNestedPathsAddress> Addresses { get; set; }
    }

    class EnableNestedPathsVipCustomer : EnableNestedPathsCustomer
    {

    }

    class EnableNestedPathsProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    class EnableNestedPathsAddress
    {
        public string City { get; set; }
    }
}

#endif
