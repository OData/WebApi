// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using Microsoft.Web.Http.Data.Test.Models;
using Xunit;

namespace Microsoft.Web.Http.Data.Test
{
    public class DataControllerQueryTests
    {
        /// <summary>
        /// Execute a simple query with limited results
        /// </summary>
        [Fact]
        public void GetProducts()
        {
            HttpConfiguration config = GetTestConfiguration();
            HttpServer server = GetTestCatalogServer(config);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);

            HttpRequestMessage request = TestHelpers.CreateTestMessage(TestConstants.CatalogUrl + "GetProducts", HttpMethod.Get, config);
            HttpResponseMessage response = invoker.SendAsync(request, CancellationToken.None).Result;

            Product[] products = response.Content.ReadAsAsync<IQueryable<Product>>().Result.ToArray();
            Assert.Equal(9, products.Length);
        }

        /// <summary>
        /// Execute a query with an OData filter specified
        /// </summary>
        [Fact]
        public void Query_Filter()
        {
            HttpConfiguration config = GetTestConfiguration();
            HttpServer server = GetTestCatalogServer(config);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);

            string query = "?$filter=UnitPrice lt 5.0";
            HttpRequestMessage request = TestHelpers.CreateTestMessage(TestConstants.CatalogUrl + "GetProducts" + query, HttpMethod.Get, config);
            HttpResponseMessage response = invoker.SendAsync(request, CancellationToken.None).Result;

            Product[] products = response.Content.ReadAsAsync<IQueryable<Product>>().Result.ToArray();
            Assert.Equal(8, products.Length);
        }

        /// <summary>
        /// Verify that the json/xml formatter instances are not shared between controllers, since
        /// their serializers are configured per controller.
        /// </summary>
        [Fact(Skip = "Need to verify if this test still makes sense given changed ObjectContent design")]
        public void Query_VerifyFormatterConfiguration()
        {
            HttpConfiguration config = GetTestConfiguration();
            HttpServer catalogServer = GetTestCatalogServer(config);
            HttpMessageInvoker catalogInvoker = new HttpMessageInvoker(catalogServer);

            HttpServer citiesServer = GetTestCitiesServer(config);
            HttpMessageInvoker citiesInvoker = new HttpMessageInvoker(citiesServer);

            // verify products query
            HttpRequestMessage request = TestHelpers.CreateTestMessage(TestConstants.CatalogUrl + "GetProducts", HttpMethod.Get, config);
            HttpResponseMessage response = catalogInvoker.SendAsync(request, CancellationToken.None).Result;
            Product[] products = response.Content.ReadAsAsync<IQueryable<Product>>().Result.ToArray();
            Assert.Equal(9, products.Length);

            // verify serialization
            QueryResult qr = new QueryResult(products, products.Length);
            ObjectContent oc = (ObjectContent)response.Content;
            MemoryStream ms = new MemoryStream();
            Task task = new JsonMediaTypeFormatter().WriteToStreamAsync(typeof(QueryResult), qr, ms, oc.Headers, null);
            task.Wait();
            Assert.True(ms.Length > 0);

            // verify cities query
            request = TestHelpers.CreateTestMessage(TestConstants.CitiesUrl + "GetCities", HttpMethod.Get, config);
            response = citiesInvoker.SendAsync(request, CancellationToken.None).Result;
            City[] cities = response.Content.ReadAsAsync<IQueryable<City>>().Result.ToArray();
            Assert.Equal(11, cities.Length);

            // verify serialization
            qr = new QueryResult(cities, cities.Length);
            oc = (ObjectContent)response.Content;
            ms = new MemoryStream();
            task = new JsonMediaTypeFormatter().WriteToStreamAsync(typeof(QueryResult), qr, ms, oc.Headers, null);
            task.Wait();
            Assert.True(ms.Length > 0);
        }

        /// <summary>
        /// Execute a query that requests an inline count with a paging query applied.
        /// </summary>
        [Fact]
        public void Query_InlineCount_SkipTop()
        {
            HttpConfiguration config = GetTestConfiguration();
            HttpServer server = GetTestCatalogServer(config);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);

            string query = "?$filter=UnitPrice lt 5.0&$skip=2&$top=5&$inlinecount=allpages";
            HttpRequestMessage request = TestHelpers.CreateTestMessage(TestConstants.CatalogUrl + "GetProducts" + query, HttpMethod.Get, config);
            HttpResponseMessage response = invoker.SendAsync(request, CancellationToken.None).Result;

            QueryResult queryResult = response.Content.ReadAsAsync<QueryResult>().Result;
            Assert.Equal(5, queryResult.Results.Cast<object>().Count());
            Assert.Equal(8, queryResult.TotalCount);
        }

        /// <summary>
        /// Execute a query that requests an inline count with only a top operation applied in the query.
        /// Expect the total count to not inlcude the take operation.
        /// </summary>
        [Fact]
        public void Query_IncludeTotalCount_Top()
        {
            HttpConfiguration config = GetTestConfiguration();
            HttpServer server = GetTestCatalogServer(config);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);

            string query = "?$filter=UnitPrice lt 5.0&$top=5&$inlinecount=allpages";
            HttpRequestMessage request = TestHelpers.CreateTestMessage(TestConstants.CatalogUrl + "GetProducts" + query, HttpMethod.Get, config);
            HttpResponseMessage response = invoker.SendAsync(request, CancellationToken.None).Result;

            QueryResult queryResult = response.Content.ReadAsAsync<QueryResult>().Result;
            Assert.Equal(5, queryResult.Results.Cast<object>().Count());
            Assert.Equal(8, queryResult.TotalCount);
        }

        /// <summary>
        /// Execute a query that requests an inline count with no paging operations specified in the 
        /// user query. There is however still a server specified limit.
        /// </summary>
        [Fact]
        public void Query_IncludeTotalCount_NoPaging()
        {
            HttpConfiguration config = GetTestConfiguration();
            HttpServer server = GetTestCatalogServer(config);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);

            string query = "?$filter=UnitPrice lt 5.0&$inlinecount=allpages";
            HttpRequestMessage request = TestHelpers.CreateTestMessage(TestConstants.CatalogUrl + "GetProducts" + query, HttpMethod.Get, config);
            HttpResponseMessage response = invoker.SendAsync(request, CancellationToken.None).Result;

            QueryResult queryResult = response.Content.ReadAsAsync<QueryResult>().Result;
            Assert.Equal(8, queryResult.Results.Cast<object>().Count());
            Assert.Equal(8, queryResult.TotalCount);
        }

        /// <summary>
        /// Execute a query that sets the inlinecount option explicitly to 'none', and verify count is not returned.
        /// </summary>
        [Fact]
        public void Query_IncludeTotalCount_False()
        {
            HttpConfiguration config = GetTestConfiguration();
            HttpServer server = GetTestCatalogServer(config);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);

            string query = "?$filter=UnitPrice lt 5.0&$inlinecount=none";
            HttpRequestMessage request = TestHelpers.CreateTestMessage(TestConstants.CatalogUrl + "GetProducts" + query, HttpMethod.Get, config);
            HttpResponseMessage response = invoker.SendAsync(request, CancellationToken.None).Result;

            Product[] products = response.Content.ReadAsAsync<IQueryable<Product>>().Result.ToArray();
            Assert.Equal(8, products.Length);
        }

        /// <summary>
        /// Verify that when no skip/top query operations are performed (and no result limits are active
        /// on the action server side), the total count returned is -1, indicating that the total count
        /// equals the result count. This avoids the fx having to double enumerate the query results to
        /// set the count server side.
        /// </summary>
        [Fact]
        public void Query_TotalCount_Equals_ResultCount()
        {
            HttpConfiguration config = GetTestConfiguration();
            HttpServer server = GetTestCatalogServer(config);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);

            string query = "?$inlinecount=allpages";
            HttpRequestMessage request = TestHelpers.CreateTestMessage(TestConstants.CatalogUrl + "GetOrders" + query, HttpMethod.Get, config);
            HttpResponseMessage response = invoker.SendAsync(request, CancellationToken.None).Result;

            QueryResult result = response.Content.ReadAsAsync<QueryResult>().Result;
            Assert.Equal(2, result.Results.Cast<object>().Count());
            Assert.Equal(-1, result.TotalCount);
        }

        private HttpConfiguration GetTestConfiguration()
        {
            HttpConfiguration config = new HttpConfiguration();
            return config;
        }

        private HttpServer GetTestCatalogServer(HttpConfiguration config)
        {
            HttpControllerDispatcher dispatcher = new HttpControllerDispatcher(config);

            HttpRoute route = new HttpRoute("{controller}/{action}", new HttpRouteValueDictionary("Catalog"));
            config.Routes.Add("catalog", route);

            HttpServer server = new HttpServer(config, dispatcher);

            return server;
        }

        private HttpServer GetTestCitiesServer(HttpConfiguration config)
        {
            HttpControllerDispatcher dispatcher = new HttpControllerDispatcher(config);

            HttpRoute route = new HttpRoute("{controller}/{action}", new HttpRouteValueDictionary("Cities"));
            config.Routes.Add("cities", route);

            HttpServer server = new HttpServer(config, dispatcher);

            return server;
        }
    }
}
