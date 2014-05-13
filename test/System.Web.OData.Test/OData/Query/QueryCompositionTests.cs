// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.OData.Query
{
    public class QueryCompositionTests
    {
        private static IEdmModel _queryCompositionCustomerModel;

        [Theory]
        [InlineData("QueryCompositionCustomer")]
        [InlineData("QueryCompositionCustomerQueryable")]
        [InlineData("QueryCompositionCustomerLowLevel")]
        [InlineData("QueryCompositionCustomerGlobal")]
        [InlineData("QueryCompositionCustomerWithTaskOfIEnumerable")]
        public void QueryComposition_Works(string controllerName)
        {
            HttpServer server = new HttpServer(InitializeConfiguration(controllerName, useCustomEdmModel: false));
            HttpClient client = new HttpClient(server);

            HttpResponseMessage response = client.GetAsync(
                String.Format("http://localhost:8080/{0}?$filter=Id ge 22 and Address/City ne 'seattle'&$orderby=Name&$skip=0&$top=1", controllerName))
                .Result;
            response.EnsureSuccessStatusCode();
            var customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;

            Assert.Equal(new[] { 22 }, customers.Select(c => c.Id));
        }

        [Fact]
        public void QueryComposition_Works_WithCustomEdmModel()
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCustomer", useCustomEdmModel: true));
            HttpClient client = new HttpClient(server);

            HttpResponseMessage response = client.GetAsync(
                String.Format("http://localhost:8080/QueryCompositionCustomer?$filter=Id ge 22 and Address/City ne 'seattle'&$orderby=Name&$skip=0&$top=1", "QueryCompositionCustomer"))
                .Result;
            response.EnsureSuccessStatusCode();
            var customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;

            Assert.Equal(new[] { 22 }, customers.Select(c => c.Id));
        }

        [Fact]
        public void ODataQueryOptionsOfT_Works()
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCustomerLowLevel_ODataQueryOptionsOfT", false));
            HttpClient client = new HttpClient(server);

            HttpResponseMessage response = client.GetAsync("http://localhost:8080/QueryCompositionCustomerLowLevel_ODataQueryOptionsOfT/?$filter=Id ge 22").Result;
            response.EnsureSuccessStatusCode();
            int count = response.Content.ReadAsAsync<int>().Result;
            Assert.Equal(2, count);
        }

        [Fact]
        public void AnonymousTypes_Work_With_EnableQueryAttribute()
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionAnonymousTypesController", useCustomEdmModel: false));
            HttpClient client = new HttpClient(server);

            HttpResponseMessage response = client.GetAsync("http://localhost:8080/QueryCompositionAnonymousTypes/?$filter=Id ge 5").Result;
            response.EnsureSuccessStatusCode();

            Type anon_type = new { Id = default(int) }.GetType();
            dynamic result = response.Content.ReadAsAsync(anon_type.MakeArrayType()).Result;

            Assert.Equal(5, result[0].Id);
            Assert.Equal(6, result.Length);
        }

        [Fact]
        public void Queryable_UsesRouteModel_ForMultipleModels()
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

            var config = new[] { typeof(PeopleController) }.GetHttpConfiguration();
            config.MapODataServiceRoute("OData1", "v1", model1);
            config.MapODataServiceRoute("OData2", "v2", model2);

            using (HttpServer host = new HttpServer(config))
            using (HttpClient client = new HttpClient(host))
            {
                // Model 1 has the Name property but not the Age property
                AssertRespondsWithExpectedStatusCode(client, "http://localhost/v1/People?$orderby=Name", HttpStatusCode.OK);
                AssertRespondsWithExpectedStatusCode(client, "http://localhost/v1/People?$orderby=Age", HttpStatusCode.BadRequest);

                // Model 2 has the Age property but not the Name property
                AssertRespondsWithExpectedStatusCode(client, "http://localhost/v2/People?$orderby=Name", HttpStatusCode.BadRequest);
                AssertRespondsWithExpectedStatusCode(client, "http://localhost/v2/People?$orderby=Age", HttpStatusCode.OK);
            }
        }

        [Fact]
        public void QueryValidationErrors_Are_SentToTheClient()
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCustomerValidation", useCustomEdmModel: false));
            HttpClient client = new HttpClient(server);

            // skip = 1 is ok
            HttpResponseMessage response = client.GetAsync("http://localhost:8080/QueryCompositionCustomerValidation/?$skip=1").Result;
            response.EnsureSuccessStatusCode();

            List<QueryCompositionCustomer> customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;
            Assert.Equal(new[] { 11, 22, 33 }, customers.Select(customer => customer.Id));

            // skip = 2 exceeds the limit
            response = client.GetAsync("http://localhost:8080/QueryCompositionCustomerValidation/?$skip=2").Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.True(response.Content.ReadAsStringAsync().Result.Contains("The limit of '1' for Skip query has been exceeded. The value from the incoming request is '2'."));
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
        [PropertyData("PrimitiveTypesQueryCompositionData")]
        public virtual void PrimitiveTypesQueryComposition(string query, IEnumerable<int> expectedResults)
        {
            ODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(EdmCoreModel.Instance, typeof(int)), new HttpRequestMessage(HttpMethod.Get, "http://localhost/?" + query));
            var results = queryOptions.ApplyTo(Enumerable.Range(0, 100).AsQueryable()) as IQueryable<int>;

            // Assert
            Assert.Equal(expectedResults, results);
        }

        private static HttpConfiguration InitializeConfiguration(string controllerName, bool useCustomEdmModel)
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
            HttpConfiguration config = controllers.GetHttpConfiguration();
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
                    ODataModelBuilder modelBuilder = new ODataConventionModelBuilder();
                    modelBuilder.EntitySet<QueryCompositionCustomer>(typeof(QueryCompositionCustomer).Name);
                    _queryCompositionCustomerModel = modelBuilder.GetEdmModel();
                }
                config.Filters.Add(new SetModelFilter(_queryCompositionCustomerModel));
            }

            return config;
        }

        private static void AreEqual(List<QueryCompositionCustomer> expectedList, List<QueryCompositionCustomer> actualList)
        {
            Assert.NotNull(expectedList);
            Assert.NotNull(actualList);
            Assert.Equal(expectedList.Count, actualList.Count);

            for (int i = 0; i < expectedList.Count; i++)
            {
                QueryCompositionCustomer expected = expectedList[i];
                QueryCompositionCustomer actual = actualList[i];
                AreEqual(expected, actual);
            }
        }

        private static void AreEqual(QueryCompositionCustomer expected, QueryCompositionCustomer actual)
        {
            Assert.NotNull(expected);
            Assert.NotNull(actual);

            Assert.True(expected.Name == actual.Name && expected.Id == actual.Id);
        }

        private static void AssertRespondsWithExpectedStatusCode(HttpClient client, string uri, HttpStatusCode expectedStatusCode)
        {
            using (HttpResponseMessage response = client.GetAsync(uri).Result)
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
                actionContext.Request.ODataProperties().Model = _model;
            }
        }
    }
}
