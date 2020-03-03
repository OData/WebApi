// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.AspNet.OData.Test.Formatter;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.AspNet.OData.Test.Formatter;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;
#endif

namespace Microsoft.AspNet.OData.Test.Query
{
    public class QueryCompositionTests
    {
#if !NETCORE // TODO #939: Enable these test on AspNetCore.
        private static IEdmModel _queryCompositionCustomerModel;

        [Theory]
        [InlineData("QueryCompositionCustomer")]
        [InlineData("QueryCompositionCustomerQueryable")]
        [InlineData("QueryCompositionCustomerLowLevel")]
        [InlineData("QueryCompositionCustomerGlobal")]
        [InlineData("QueryCompositionCustomerWithTaskOfIEnumerable")]
        public async Task QueryComposition_Works(string controllerName)
        {
            HttpServer server = new HttpServer(InitializeConfiguration(controllerName, useCustomEdmModel: true));
            HttpClient client = new HttpClient(server);

            HttpResponseMessage response = await GetResponse(client, server.Configuration,
                String.Format("http://localhost:8080/{0}?$filter=Id ge 22 and Address/City ne 'seattle'&$orderby=Name&$skip=0&$top=1", controllerName));
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            var customers = await response.Content.ReadAsObject<List<QueryCompositionCustomer>>();

            Assert.Equal(new[] { 22 }, customers.Select(c => c.Id));
        }

        [Fact]
        public async Task QueryComposition_Works_WithCustomEdmModel()
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCustomer", useCustomEdmModel: true));
            HttpClient client = new HttpClient(server);

            HttpResponseMessage response = await GetResponse(client, server.Configuration,
                String.Format("http://localhost:8080/QueryCompositionCustomer?$filter=Id ge 22 and Address/City ne 'seattle'&$orderby=Name&$skip=0&$top=1", "QueryCompositionCustomer"));
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            var customers = await response.Content.ReadAsObject<List<QueryCompositionCustomer>>();

            Assert.Equal(new[] { 22 }, customers.Select(c => c.Id));
        }

        [Fact]
        public async Task QueryComposition_ThrowsException_ForCaseSensitive()
        {
            // Arrange
            const string caseInSensitive = "?$fIlTer=iD Eq 33";
            ODataUriResolver resolver = new ODataUriResolver
            {
                EnableCaseInsensitive = false
            };
            HttpServer server =
                new HttpServer(InitializeConfiguration("QueryCompositionCustomer", true, resolver));
            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = await GetResponse(client, server.Configuration,
                "http://localhost:8080/QueryCompositionCustomer" + caseInSensitive);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Contains("The query parameter '$fIlTer' is not supported.", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task QueryComposition_WorkAsExpect_ForCaseInsensitive()
        {
            // Arrange
            const string caseInSensitive = "?$fIlTer=iD Eq 33";
            ODataUriResolver resolver = new ODataUriResolver
            {
                EnableCaseInsensitive = true,
            };
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCustomer", true, resolver));
            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = await GetResponse(client, server.Configuration,
                "http://localhost:8080/QueryCompositionCustomer" + caseInSensitive);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("[{\"Name\":\"Highest\",\"Add", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task ODataQueryOptionsOfT_Works()
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCustomerLowLevel_ODataQueryOptionsOfT", true));
            HttpClient client = new HttpClient(server);

            HttpResponseMessage response = await GetResponse(client, server.Configuration,
                "http://localhost:8080/QueryCompositionCustomerLowLevel_ODataQueryOptionsOfT/?$filter=Id ge 22");
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            int count = await response.Content.ReadAsObject<int>();
            Assert.Equal(2, count);
        }

        [Theory]
        [InlineData("?filter=id eq 33", true)]
        [InlineData("?filter=Id eq 33", false)]
        public async void QueryComposition_WorkAsExpect_ForOptionalDollarSignPrefixForSystemQuery(
            string noDollarSignSystemQuery, bool enableCaseInsensitive)
        {
            // Arrange
            ODataUriResolver resolver = new ODataUriResolver
            {
                EnableNoDollarQueryOptions = true,
                EnableCaseInsensitive = enableCaseInsensitive

            };
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCustomer", true, resolver));
            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = await GetResponse(client, server.Configuration,
                "http://localhost:8080/QueryCompositionCustomer" + noDollarSignSystemQuery);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("[{\"Name\":\"Highest\",\"Add", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public async Task AnonymousTypes_Work_With_EnableQueryAttribute()
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionAnonymousTypesController", useCustomEdmModel: false));
            HttpClient client = new HttpClient(server);

            HttpResponseMessage response = await GetResponse(client, server.Configuration,
                "http://localhost:8080/QueryCompositionAnonymousTypes/?$filter=Id ge 5");
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            Type anon_type = new { Id = default(int) }.GetType();
            dynamic result = await response.Content.ReadAsAsync(anon_type.MakeArrayType());

            Assert.Equal(5, result[0].Id);
            Assert.Equal(6, result.Length);
        }

        [Fact]
        public async Task Queryable_UsesRouteModel_ForMultipleModels()
        {
            // Model 1 only has Name, Model 2 only has Age
            ODataModelBuilder builder1 = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            var personType1 = builder1.EntityType<FormatterPerson>().Property(p => p.Name);
            builder1.EntitySet<FormatterPerson>("People").HasIdLink(p => new Uri("http://link/"), false);
            var model1 = builder1.GetEdmModel();

            ODataModelBuilder builder2 = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            builder2.EntityType<FormatterPerson>().Property(p => p.Age);
            builder2.EntitySet<FormatterPerson>("People").HasIdLink(p => new Uri("http://link/"), false);
            var model2 = builder2.GetEdmModel();

            var config = RoutingConfigurationFactory.CreateWithTypes(new[] { typeof(PeopleController) });
            config.MapODataServiceRoute("OData1", "v1", model1);
            config.MapODataServiceRoute("OData2", "v2", model2);

            using (HttpServer host = new HttpServer(config))
            using (HttpClient client = new HttpClient(host))
            {
                // Model 1 has the Name property but not the Age property
                await AssertRespondsWithExpectedStatusCode(client, "http://localhost/v1/People?$orderby=Name", HttpStatusCode.OK);
                await AssertRespondsWithExpectedStatusCode(client, "http://localhost/v1/People?$orderby=Age", HttpStatusCode.BadRequest);

                // Model 2 has the Age property but not the Name property
                await AssertRespondsWithExpectedStatusCode(client, "http://localhost/v2/People?$orderby=Name", HttpStatusCode.BadRequest);
                await AssertRespondsWithExpectedStatusCode(client, "http://localhost/v2/People?$orderby=Age", HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task QueryValidationErrors_Are_SentToTheClient()
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCustomerValidation", useCustomEdmModel: false));
            HttpClient client = new HttpClient(server);

            // skip = 1 is ok
            HttpResponseMessage response = await GetResponse(client, server.Configuration,
                "http://localhost:8080/QueryCompositionCustomerValidation/?$skip=1");
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            List<QueryCompositionCustomer> customers = await response.Content.ReadAsObject<List<QueryCompositionCustomer>>();
            Assert.Equal(new[] { 11, 22, 33 }, customers.Select(customer => customer.Id));

            // skip = 2 exceeds the limit
            response = await GetResponse(client, server.Configuration,
                "http://localhost:8080/QueryCompositionCustomerValidation/?$skip=2");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The limit of '1' for Skip query has been exceeded. The value from the incoming request is '2'.", await response.Content.ReadAsStringAsync());
        }

        public static TheoryDataSet<string, IEnumerable<int>> PrimitiveTypesQueryCompositionData
        {
            get
            {
                var range = Enumerable.Range(0, 100).AsQueryable();
                return new TheoryDataSet<string, IEnumerable<int>>
                {
                    { "", range.ToArray() },
                    { "$filter=$it eq 1", new[] { 1 } },
                    { "$filter=$it mod 2 eq 1", range.Where(i => i%2 == 1).ToArray() },
                    { "$filter=$it mod 2 eq 1 &$orderby=$it desc", range.Where(i => i%2 == 1).OrderByDescending(i => i).ToArray() },
                    { "$filter=$it mod 2 eq 1 &$skip=10", range.Where(i => i%2 == 1).Skip(10).ToArray() },
                    { "$filter=$it mod 2 eq 1 &$skip=10&$top=10", range.Where(i => i%2 == 1).Skip(10).Take(10).ToArray() },
                };
            }
        }

        [Theory]
        [MemberData(nameof(PrimitiveTypesQueryCompositionData))]
        public virtual void PrimitiveTypesQueryComposition(string query, IEnumerable<int> expectedResults)
        {
            var message = RequestFactory.Create(HttpMethod.Get, "http://localhost/?" + query);

            ODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(EdmCoreModel.Instance, typeof(int)), message);
            var results = queryOptions.ApplyTo(Enumerable.Range(0, 100).AsQueryable()) as IQueryable<int>;

            // Assert
            Assert.Equal(expectedResults, results);
        }

        private static HttpConfiguration InitializeConfiguration(string controllerName, bool useCustomEdmModel,
            ODataUriResolver resolver = null)
        {
            var controllers = new[]
            {
                typeof(QueryCompositionPrimitiveController), typeof(QueryCompositionCustomerController),
                typeof(QueryCompositionCustomerQueryableController),
                typeof(QueryCompositionCustomerWithTaskOfIEnumerableController),
                typeof(QueryCompositionCustomerGlobalController), typeof(QueryCompositionCustomerValidationController),
                typeof(QueryCompositionCustomerLowLevelController),
                typeof(QueryCompositionCustomerLowLevel_ODataQueryOptionsOfTController),
                typeof(QueryCompositionCategoryController), typeof(QueryCompositionAnonymousTypesController)
            };

            var config = RoutingConfigurationFactory.CreateWithTypes(controllers);
            config.Routes.MapHttpRoute("default", "{controller}/{key}", new { key = RouteParameter.Optional });
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            if (controllerName == "QueryCompositionCustomerGlobal")
            {
                config.Filters.Add(new EnableQueryAttribute());
            }

            if (useCustomEdmModel)
            {
                if (_queryCompositionCustomerModel == null)
                {
                    ODataModelBuilder modelBuilder = ODataConventionModelBuilderFactory.Create();
                    modelBuilder.EntitySet<QueryCompositionCustomer>(typeof(QueryCompositionCustomer).Name);
                    _queryCompositionCustomerModel = modelBuilder.GetEdmModel();
                }
                if (resolver == null)
                {
                    config.EnableODataDependencyInjectionSupport("default", _queryCompositionCustomerModel);
                }
                else
                {
                    config.EnableODataDependencyInjectionSupport("default",
                        b => b.AddService(ServiceLifetime.Singleton, sp => _queryCompositionCustomerModel)
                            .AddService(ServiceLifetime.Singleton, sp => resolver));
                }

                config.Filters.Add(new SetModelFilter(_queryCompositionCustomerModel));
            }
            else
            {
                config.EnableODataDependencyInjectionSupport("default");
            }

            return config;
        }

        private static async Task<HttpResponseMessage> GetResponse(HttpClient client, HttpConfiguration config, string requestUri)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.SetConfiguration(config);
            request.EnableODataDependencyInjectionSupport("default");
            return await client.SendAsync(request);
        }

        private static async Task AssertRespondsWithExpectedStatusCode(HttpClient client, string uri, HttpStatusCode expectedStatusCode)
        {
            using (HttpResponseMessage response = await client.GetAsync(uri))
            {
                Assert.Equal(expectedStatusCode, response.StatusCode);
            }
        }

        private class SetModelFilter : ActionFilterAttribute
        {
            private IEdmModel _model;

            public SetModelFilter(IEdmModel model)
            {
                _model = model;
            }

            public override void OnActionExecuting(HttpActionContext actionContext)
            {
                Assert.Equal(_model, actionContext.Request.GetModel());
            }
        }
#endif
    }
}
