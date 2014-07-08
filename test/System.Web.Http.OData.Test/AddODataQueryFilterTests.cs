// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class AddODataQueryFilterTests
    {
        private static readonly IQueryable<Customer> _customers =
            Enumerable.Range(1, 10).Select(i => new Customer { Id = i, Orders = new List<Order>() })
            .AsQueryable();

        [Theory]
        [InlineData("http://any/odata/ODataControllerWithQueryableOnAction")]
        [InlineData("http://any/api/ApiControllerWithEnableQueryOnAction/Get")]
        [InlineData("http://any/api/ApiControllerWithQueryableOnController/GetMoreCustomers")]
        public void AddODataQueryFilter_WorksWithActionsWithQuerable_And_EnableQuery(string url)
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.AddODataQueryFilter();
            configuration.Routes.MapHttpRoute("api", "api/{controller}/{action}");
            configuration.Routes.MapODataServiceRoute("odata", "odata", GetModel());
            HttpServer server = new HttpServer(configuration);
            HttpClient client = new HttpClient(server);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url + "?$expand=Orders");
            
            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("ODataControllerWithQueryableOnAction");
            builder.EntitySet<Order>("Orders");
            return builder.GetEdmModel();
        }

        public class ODataControllerWithQueryableOnActionController : ODataController
        {
            #pragma warning disable 0618
            [Queryable]
            #pragma warning restore 0618
            public IQueryable<Customer> Get()
            {
                return _customers;
            }
        }

        public class ApiControllerWithEnableQueryOnActionController : ApiController
        {
            [EnableQuery]
            public IQueryable<Customer> Get()
            {
                return _customers;
            }
        }

        #pragma warning disable 0618
        [Queryable]
        #pragma warning restore 0618
        public class ApiControllerWithQueryableOnControllerController : ApiController
        {
            public IQueryable<Customer> GetMoreCustomers()
            {
                return _customers;
            }
        }

        public class Customer
        {
            public int Id { get; set; }

            public ICollection<Order> Orders { get; set; }
        }

        public class Order
        {
            public int Id { get; set; }
        }
    }
}
