// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class SelectExpandEFTests : WebHostTestBase<SelectExpandEFTests>
    {
        public SelectExpandEFTests(WebHostTestFixture<SelectExpandEFTests> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(
                    typeof(EFSelectCustomersController),
                    typeof(EFSelectOrdersController),
                    typeof(EFWideCustomersController));
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("selectexpand", "selectexpand", GetEdmModel(configuration));
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();

            builder.EntitySet<EFSelectCustomer>("EFSelectCustomers");
            builder.EntitySet<EFSelectOrder>("EFSelectOrders");
            builder.EntitySet<EFWideCustomer>("EFWideCustomers");
            builder.Action("ResetDataSource-Customer");
            builder.Action("ResetDataSource-WideCustomer");
            builder.Action("ResetDataSource-Order");

            IEdmModel model = builder.GetEdmModel();
            for (int idx = 1; idx <= 5; idx++)
            {
                IEdmSchemaType nestedType = model.FindDeclaredType("Microsoft.Test.E2E.AspNet.OData.QueryComposition.CustomProperties" + idx);
                model.SetAnnotationValue(nestedType, new Microsoft.AspNet.OData.Query.ModelBoundQuerySettings()
                {
                    DefaultSelectType = SelectExpandType.Automatic
                });
            }

            return model;
        }

        [Fact]
        public async Task QueryForAnEntryWithExpandNavigationPropertyExceedPageSize()
        {
            // Arrange
            await RestoreData("-Customer");
            string queryUrl = string.Format("{0}/selectexpand/EFSelectCustomers?$expand=SelectOrders", BaseAddress);
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
            var expandProp = result[0]["SelectOrders"] as JArray;
            Assert.Equal(2, expandProp.Count);
            Assert.Equal(1, expandProp[0]["Id"]);
            Assert.Equal(2, expandProp[1]["Id"]);
        }

        [Fact]
        public async Task QueryForAnEntryWithExpandSingleNavigationPropertyFilterWorks()
        {
            await RestoreData("-Order");
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Arrange
            Func<string, Task<JArray>> TestBody = async (url) =>
            {
                string queryUrl = url;
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

                // Act
                response = await client.SendAsync(request);

                // Assert
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(response.Content);

                var responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                return responseObject["value"] as JArray;
            };

            var result = await TestBody(string.Format("{0}/selectexpand/EFSelectOrders?$expand=SelectCustomer($filter=Id ne 1)", BaseAddress));
            Assert.False(result[0]["SelectCustomer"].HasValues);

            result = await TestBody(string.Format("{0}/selectexpand/EFSelectOrders?$expand=SelectCustomer($filter=Id eq 1)", BaseAddress));
            Assert.Equal(1, (int)result[0]["SelectCustomer"]["Id"]);
        }

        [Fact]
        public async Task NestedTopSkipOrderByInDollarExpandWorksWithEF()
        {
            // Arrange
            await RestoreData("-Customer");
            string queryUrl = string.Format("{0}/selectexpand/EFSelectCustomers?$expand=SelectOrders($orderby=Id desc;$skip=1;$top=1)", BaseAddress);
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
            var expandProp = result[0]["SelectOrders"] as JArray;
            Assert.Single(expandProp);
            Assert.Equal(2, expandProp[0]["Id"]);
        }

        [Fact]
        public async Task QueryForLongSelectList()
        {
            // Arrange
            await RestoreData("-WideCustomer");
            // Create long $slect/$expand Custom1-4 will be autoexpanded to avoid maxUrl error
            string queryUrl = string.Format("{0}/selectexpand/EFWideCustomers?$select=Id&$expand=Custom1,Custom2,Custom3,Custom4,Custom5($select="
                + string.Join(",", Enumerable.Range(1601, 399).Select(i => string.Format("Prop{0:0000}", i))) + ")",
                BaseAddress);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient() { Timeout = TimeSpan.FromMinutes(10) };
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            JObject responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            JArray result = responseObject["value"] as JArray;
            Assert.Single(result);
            Assert.Equal("Prop0001", result[0]["Custom1"]["Prop0001"]);
            Assert.Equal("Prop0099", result[0]["Custom1"]["Prop0099"]);
            Assert.Equal("Prop0199", result[0]["Custom1"]["Prop0199"]);
            Assert.Equal("Prop0298", result[0]["Custom1"]["Prop0298"]);
            Assert.Equal("Prop0798", result[0]["Custom2"]["Prop0798"]);
            Assert.Equal("Prop1198", result[0]["Custom3"]["Prop1198"]);
            Assert.Equal("Prop1598", result[0]["Custom4"]["Prop1598"]);
            Assert.Equal("Prop1998", result[0]["Custom5"]["Prop1998"]);
            Assert.Null(result[0]["Custom5"]["Prop2000"]);
        }

        private async Task RestoreData(string suffix)
        {
            string requestUri = BaseAddress + string.Format("/selectexpand/ResetDataSource{0}", suffix);
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
        }
    }

    public class EFSelectCustomersController : TestODataController
#if NETCORE
        , IDisposable
#endif
    {
        private readonly SampleContext _db = new SampleContext();

        [EnableQuery(PageSize = 2)]
        public ITestActionResult Get()
        {
            return Ok(_db.Customers);
        }

        [HttpGet]
        [ODataRoute("ResetDataSource-Customer")]
        public ITestActionResult ResetDataSource()
        {
            _db.Database.Delete();  // Start from scratch so that tests aren't broken by schema changes.
            Generate(_db);
            return Ok();
        }

        public static void Generate(SampleContext db)
        {
            var customer = new EFSelectCustomer
            {
                Id = 1,
                SelectOrders = new List<EFSelectOrder>
                {
                    new EFSelectOrder
                    {
                        Id = 3,
                    },
                    new EFSelectOrder
                    {
                        Id = 1,
                    },
                    new EFSelectOrder
                    {
                        Id = 2,
                    }
                }
            };

            db.Customers.Add(customer);
            db.SaveChanges();
        }

#if NETCORE
        public void Dispose()
        {
             //_db.Dispose();
        }
#endif
    }

    public class EFWideCustomersController : TestODataController
#if NETCORE
        , IDisposable
#endif
    {
        private readonly SampleContext _db = new SampleContext();

        [EnableQuery(PageSize = 2)]
        public IQueryable<EFWideCustomer> Get()
        {
            return (_db.WideCustomers as IQueryable<IEFCastTest>).Cast<EFWideCustomer>();
        }

        [HttpGet]
        [ODataRoute("ResetDataSource-WideCustomer")]
        public ITestActionResult ResetDataSource()
        {
            _db.Database.Delete();  // Start from scratch so that tests aren't broken by schema changes.
            Generate();
            return Ok();
        }

        public void Generate()
        {
            var wideCustomer = new EFWideCustomer
            {
                Id = 1,
                Custom1 = new CustomProperties1
                {
                    Prop0001 = "Prop0001",
                    Prop0099 = "Prop0099",
                    Prop0199 = "Prop0199",
                    Prop0298 = "Prop0298",
                    Prop0299 = "Prop0299",
                },
                Custom2 = new CustomProperties2
                {
                    Prop0798 = "Prop0798",
                },
                Custom3 = new CustomProperties3
                {
                    Prop1198 = "Prop1198",
                },
                Custom4 = new CustomProperties4
                {
                    Prop1598 = "Prop1598",
                    Prop1600 = "Prop1600",
                },
                Custom5 = new CustomProperties5
                {
                    Prop1998 = "Prop1998",
                    Prop2000 = "Prop2000",
                },
            };

            _db.WideCustomers.Add(wideCustomer);
            _db.SaveChanges();
        }

#if NETCORE
        public void Dispose()
        {
            // _db.Dispose();
        }
#endif
    }

    public class EFSelectOrdersController : TestODataController
#if NETCORE
        , IDisposable
#endif
    {
        private readonly SampleContext _db = new SampleContext();

        [EnableQuery(HandleReferenceNavigationPropertyExpandFilter = true)]
        public ITestActionResult Get()
        {
            return Ok(_db.Orders);
        }

        [HttpGet]
        [ODataRoute("ResetDataSource-Order")]
        public ITestActionResult ResetDataSource()
        {
            _db.Database.Delete();  // Start from scratch so that tests aren't broken by schema changes.
            EFSelectCustomersController.Generate(_db);
            return Ok();
        }

#if NETCORE
        public void Dispose()
        {
             //_db.Dispose();
        }
#endif
    }

    public class SampleContext : DbContext
    {
        public static string ConnectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=SelectExpandTest2";

        public SampleContext()
            : base(ConnectionString)
        {
        }

        public DbSet<EFSelectCustomer> Customers { get; set; }

        public DbSet<EFSelectOrder> Orders { get; set; }

        public DbSet<EFWideCustomer> WideCustomers { get; set; }
    }

    public class EFSelectCustomer
    {
        public int Id { get; set; }
        public virtual IList<EFSelectOrder> SelectOrders { get; set; }
    }

    public class EFSelectOrder
    {
        public int Id { get; set; }
        public virtual EFSelectCustomer SelectCustomer { get; set; }
    }
}        