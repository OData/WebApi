// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Query
{
    public class QueryCompositionTests : MarshalByRefObject
    {
        private static IEdmModel _queryCompositionCustomerModel;

        public static TheoryDataSet<string, bool> ControllerNames
        {
            get
            {
                return new TheoryDataSet<string, bool> 
                {
                    { "QueryCompositionCustomer", true },
                    { "QueryCompositionCustomerWithTaskOfIEnumerable", false },
                    { "QueryCompositionCustomerWithTaskOfHttpResponseMessage", false },
                    { "QueryCompositionCustomerLowLevel", true },
                    { "QueryCompositionCustomerQueryable", true },
                    { "QueryCompositionCustomerGlobal", true },
                    { "QueryCompositionCustomer", false },
                    { "QueryCompositionCustomerLowLevel", false },
                    { "QueryCompositionCustomerQueryable", false },
                    { "QueryCompositionCustomerGlobal", false }
                };
            }
        }

        public static TheoryDataSet<string, bool, int[]> Filters
        {
            get
            {
                return new TheoryDataSet<string, bool, int[]>
                {
                    { "Name eq 'Highest'", true, new int[] { 33 } },
                    { "Address/City eq 'redmond'", true, new int[] { 11 } },
                    { "Address/City eq null", true, new int[] { 22 , 3 } },
                    { "RelationshipManager/Name eq null", true, new int[] { 11, 22, 3 } },
                    { "RelationshipManager/Name ne null", true, new int[] { 33 } },
                    { "Tags/any(tag: tag eq 'tag1')", true, new int[] { 22, 3 } },
                    { "Tags/any(tag: tag eq 'tag3')", true, new int[] { 3 } },
                    { "Tags/all(tag: tag eq 'tag33')", true, new int[] { 33 } },
                    { "Contacts/any(contact: contact/Name eq 'Primary')", true, new int[] { 11, 33 } },
                };
            }
        }

        [Theory]
        [PropertyData("ControllerNames")]
        public void QueryableOnActionWithoutQueryReturnsOriginalList(string controllerName, bool useCustomEdmModel)
        {
            HttpServer server = new HttpServer(InitializeConfiguration(controllerName, useCustomEdmModel));
            HttpClient client = new HttpClient(server);

            // order by Id
            HttpResponseMessage response = client.GetAsync(string.Format("http://localhost:8080/{0}/", controllerName)).Result;
            response.EnsureSuccessStatusCode();
            List<QueryCompositionCustomer> customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;
            AreEqual(QueryCompositionCustomerController.CustomerList.ToList(), customers);
        }

        [Theory]
        [PropertyData("ControllerNames")]
        public void QueryableOnActionOrderByIdReturnedOrderedList(string controllerName, bool useCustomEdmModel)
        {
            HttpServer server = new HttpServer(InitializeConfiguration(controllerName, useCustomEdmModel));
            HttpClient client = new HttpClient(server);

            // order by Id
            HttpResponseMessage response = client.GetAsync(string.Format("http://localhost:8080/{0}/?$orderby=Id", controllerName)).Result;
            response.EnsureSuccessStatusCode();
            List<QueryCompositionCustomer> customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;
            AreEqual(
                new List<QueryCompositionCustomer>
                {   
                    new QueryCompositionCustomer { Id = 3, Name = "NewLow" },
                    new QueryCompositionCustomer { Id = 11, Name = "Lowest" }, 
                    new QueryCompositionCustomer { Id = 22, Name = "Middle" }, 
                    new QueryCompositionCustomer { Id = 33, Name = "Highest" },
                },
                customers);
        }

        [Theory]
        [PropertyData("ControllerNames")]
        public void QueryableOnActionOrderByIdTopSkipReturnsCorrectList(string controllerName, bool useCustomEdmModel)
        {
            HttpServer server = new HttpServer(InitializeConfiguration(controllerName, useCustomEdmModel));
            HttpClient client = new HttpClient(server);

            // order by Id
            HttpResponseMessage response = client.GetAsync(string.Format("http://localhost:8080/{0}/?$orderby=Id&$skip=1&$top=2", controllerName)).Result;
            response.EnsureSuccessStatusCode();
            List<QueryCompositionCustomer> customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;
            AreEqual(
                new List<QueryCompositionCustomer>
                {
                    new QueryCompositionCustomer { Id = 11, Name = "Lowest" }, 
                    new QueryCompositionCustomer { Id = 22, Name = "Middle" }, 
                },
                customers);
        }

        [Theory]
        [PropertyData("ControllerNames")]
        public void QueryableOnActionTopSkipChoosesDefaultOrderReturnsCorrectList(string controllerName, bool useCustomEdmModel)
        {
            HttpServer server = new HttpServer(InitializeConfiguration(controllerName, useCustomEdmModel));
            HttpClient client = new HttpClient(server);

            // order by Id
            HttpResponseMessage response = client.GetAsync(string.Format("http://localhost:8080/{0}/?$skip=1&$top=2", controllerName)).Result;
            response.EnsureSuccessStatusCode();
            List<QueryCompositionCustomer> customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;
            AreEqual(
                new List<QueryCompositionCustomer>
                {
                    new QueryCompositionCustomer { Id = 11, Name = "Lowest" }, 
                    new QueryCompositionCustomer { Id = 22, Name = "Middle" }, 
                },
                customers);
        }

        [Theory]
        [InlineData("QueryCompositionCustomerLowLevelWithoutDefaultOrder", true)]
        [InlineData("QueryCompositionCustomerLowLevelWithoutDefaultOrder", false)]
        public void QueryableOnActionTopSkipFallsBackToBackendOrderIf_EnsureStableOrdering_IsFalse(string controllerName, bool useCustomEdmModel)
        {
            HttpServer server = new HttpServer(InitializeConfiguration(controllerName, useCustomEdmModel));
            HttpClient client = new HttpClient(server);

            // order by Id
            HttpResponseMessage response = client.GetAsync(string.Format("http://localhost:8080/{0}/?$skip=1&$top=2", controllerName)).Result;
            response.EnsureSuccessStatusCode();
            List<QueryCompositionCustomer> customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;
            AreEqual(
                new List<QueryCompositionCustomer>
                {
                    new QueryCompositionCustomer { Id = 33, Name = "Highest" }, 
                    new QueryCompositionCustomer { Id = 22, Name = "Middle" }, 
                },
                customers);
        }

        [Theory]
        [PropertyData("ControllerNames")]
        public void QueryableOnActionOrderByNameReturnsCorrectList(string controllerName, bool useCustomEdmModel)
        {
            HttpServer server = new HttpServer(InitializeConfiguration(controllerName, useCustomEdmModel));
            HttpClient client = new HttpClient(server);

            // order by Name
            HttpResponseMessage response = client.GetAsync(string.Format("http://localhost:8080/{0}/?$orderby=Name", controllerName)).Result;
            response.EnsureSuccessStatusCode();
            List<QueryCompositionCustomer> customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;
            AreEqual(
                new List<QueryCompositionCustomer>  
                {   
                    new QueryCompositionCustomer { Id = 33, Name = "Highest" },
                    new QueryCompositionCustomer { Id = 11, Name = "Lowest" }, 
                    new QueryCompositionCustomer { Id = 22, Name = "Middle" }, 
                    new QueryCompositionCustomer { Id = 3, Name = "NewLow" },
                },
                customers);
        }

        [Theory]
        [PropertyData("ControllerNames")]
        public void QueryableOnActionUnknownOperatorIsAllowed(string controllerName, bool useCustomEdmModel)
        {
            HttpServer server = new HttpServer(InitializeConfiguration(controllerName, useCustomEdmModel));
            HttpClient client = new HttpClient(server);

            // unsupported operator - ignored
            HttpResponseMessage response = client.GetAsync(string.Format("http://localhost:8080/{0}/?$orderby=Name&unknown=12", controllerName)).Result;
            response.EnsureSuccessStatusCode();
            List<QueryCompositionCustomer> customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;
            AreEqual(
                new List<QueryCompositionCustomer>  
                {   
                    new QueryCompositionCustomer { Id = 33, Name = "Highest" },
                    new QueryCompositionCustomer { Id = 11, Name = "Lowest" }, 
                    new QueryCompositionCustomer { Id = 22, Name = "Middle" }, 
                    new QueryCompositionCustomer { Id = 3, Name = "NewLow" },
                },
                customers);
        }

        [Theory]
        [PropertyData("ControllerNames")]
        public void QueryableOnActionUnknownOperatorStartingDollarSignThrows(string controllerName, bool useCustomEdmModel)
        {
            HttpServer server = new HttpServer(InitializeConfiguration(controllerName, useCustomEdmModel));
            HttpClient client = new HttpClient(server);

            // unsupported operator starting with $ - throws
            HttpResponseMessage response = client.GetAsync(string.Format("http://localhost:8080/{0}/?$orderby=Name&$unknown=12", controllerName)).Result;

            if (controllerName != "QueryCompositionCustomerLowLevel")
            {
                // QueryableAttribute will validate and throws
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Theory]
        [InlineData("QueryCompositionCustomerLowLevel", true)]
        [InlineData("QueryCompositionCustomerLowLevel", false)]
        public void QueryableOnActionUnknownOperatorStartingDollarSignIsAllowedForLowLevelApi(string controllerName, bool useCustomEdmModel)
        {
            HttpServer server = new HttpServer(InitializeConfiguration(controllerName, useCustomEdmModel));
            HttpClient client = new HttpClient(server);

            // unsupported operator starting with $ - throws
            HttpResponseMessage response = client.GetAsync(string.Format("http://localhost:8080/{0}/?$orderby=Name&$unknown=12", controllerName)).Result;

            // using low level api works fine
            response.EnsureSuccessStatusCode();
            List<QueryCompositionCustomer> customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;
            AreEqual(
                new List<QueryCompositionCustomer>  
                {   
                    new QueryCompositionCustomer { Id = 33, Name = "Highest" },
                    new QueryCompositionCustomer { Id = 11, Name = "Lowest" }, 
                    new QueryCompositionCustomer { Id = 22, Name = "Middle" }, 
                    new QueryCompositionCustomer { Id = 3, Name = "NewLow" },
                },
                customers);
        }

        [Theory]
        [PropertyData("ControllerNames")]
        public void QueryableOnActionOrderByUnknownPropertyThrows(string controllerName, bool useCustomEdmModel)
        {
            HttpServer server = new HttpServer(InitializeConfiguration(controllerName, useCustomEdmModel));
            HttpClient client = new HttpClient(server);

            // invalid operator - 400
            HttpResponseMessage response = client.GetAsync(string.Format("http://localhost:8080/{0}/?$orderby=UnknownPropertyName", controllerName)).Result;
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [TestDataSet(typeof(QueryCompositionTests), "ControllerNames", typeof(QueryCompositionTests), "Filters")]
        public void QueryableFilter(string controllerName, bool useCustomEdmModel, string filter, bool nullPropagation, int[] customerIds)
        {
            HttpServer server = new HttpServer(InitializeConfiguration(controllerName, useCustomEdmModel));
            HttpClient client = new HttpClient(server);

            // unsupported operator starting wih $ - throws
            HttpResponseMessage response = client.GetAsync(string.Format("http://localhost:8080/{0}/?$filter={1}", controllerName, filter)).Result;

            // using low level api works fine
            response.EnsureSuccessStatusCode();
            List<QueryCompositionCustomer> customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;
            Assert.Equal(
                customerIds,
                customers.Select(customer => customer.Id));
        }

        [Fact]
        public virtual void QueryableUsesConfiguredAssembliesResolver()
        {
            ODataModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<QueryCompositionCustomer>(typeof(QueryCompositionCustomer).Name);
            IEdmModel model = modelBuilder.GetEdmModel();
            model.SetAnnotationValue<ClrTypeAnnotation>(model.FindType("System.Web.Http.OData.Query.QueryCompositionCustomer"), null);

            HttpConfiguration configuration = InitializeConfiguration("QueryCompositionCustomer", useCustomEdmModel: false);

            bool called = false;
            Mock<IAssembliesResolver> assembliesResolver = new Mock<IAssembliesResolver>();
            assembliesResolver
                .Setup(r => r.GetAssemblies())
                .Returns(new DefaultAssembliesResolver().GetAssemblies())
                .Callback(() => { called = true; })
                .Verifiable();
            configuration.Services.Replace(typeof(IAssembliesResolver), assembliesResolver.Object);

            HttpServer server = new HttpServer(configuration);
            HttpClient client = new HttpClient(server);

            HttpResponseMessage response = client.GetAsync("http://localhost:8080/{0}/?$filter=Id eq 2").Result;
            Assert.True(called);
        }

        [Theory]
        [InlineData("Id eq 10")]
        [InlineData("Locations/any(l : l/City eq 'Redmond')")]
        [InlineData("Locations/any(l : l/Zipcode eq '98052')")]
        public void QueryableWorksWithModelsWithPrimitiveCollectionAndComplexCollection(string filter)
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCategoryController", useCustomEdmModel: false));
            HttpClient client = new HttpClient(server);

            // unsupported operator starting with $ - throws
            HttpResponseMessage response = client.GetAsync("http://localhost:8080/QueryCompositionCategory/?$filter=" + filter).Result;
            response.EnsureSuccessStatusCode();
            Assert.Equal(0, response.Content.ReadAsAsync<List<QueryCompositionCategory>>().Result.Count());
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
        public void AnonymousTypes_Work_With_QueryableAttribute()
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

        [Theory]
        [InlineData("Name gt 'Middle'", "3")]
        [InlineData("Name ge 'Middle'", "22,3")]
        [InlineData("Name lt 'Middle'", "11,33")]
        [InlineData("Name le 'Middle'", "11,33,22")]
        [InlineData("Name ge Name", "11,33,22,3")]
        [InlineData("Name gt null", "11,33,22,3")]
        [InlineData("null gt Name", "")]
        [InlineData("'Middle' gt Name", "11,33")]
        [InlineData("'a' lt 'b'", "11,33,22,3")]
        [InlineData("Address/City gt 'san francisco'", "33")]
        public void QueryableFilter_StringComparisons_Work(string filter, string customerIds)
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCustomer", false));
            HttpClient client = new HttpClient(server);

            HttpResponseMessage response = client.GetAsync(string.Format("http://localhost:8080/{0}/?$filter={1}", "QueryCompositionCustomer", filter)).Result;

            // using low level api works fine
            response.EnsureSuccessStatusCode();
            List<QueryCompositionCustomer> customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;
            Assert.Equal(
                customerIds,
                String.Join(",", customers.Select(customer => customer.Id)));
        }

        [Fact]
        public void Queryable_UsesRouteModel_ForMultipleModels()
        {
            // Model 1 only has Name, Model 2 only has Age
            ODataModelBuilder builder1 = new ODataModelBuilder();
            var personType1 = builder1.Entity<FormatterPerson>().Property(p => p.Name);
            builder1.EntitySet<FormatterPerson>("People").HasIdLink(p => "link", false);
            var model1 = builder1.GetEdmModel();

            ODataModelBuilder builder2 = new ODataModelBuilder();
            builder2.Entity<FormatterPerson>().Property(p => p.Age);
            builder2.EntitySet<FormatterPerson>("People").HasIdLink(p => "link", false);
            var model2 = builder2.GetEdmModel();

            var config = new HttpConfiguration();
            config.Routes.MapODataRoute("OData1", "v1", model1);
            config.Routes.MapODataRoute("OData2", "v2", model2);

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
        public void QueryableWorksWith_ByteArrayComparison()
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCustomer", useCustomEdmModel: false));
            HttpClient client = new HttpClient(server);

            // unsupported operator starting with $ - throws
            HttpResponseMessage response = client.GetAsync("http://localhost:8080/QueryCompositionCustomer/?$filter=Image eq binary'010203'").Result;

            // using low level api works fine
            response.EnsureSuccessStatusCode();
            List<QueryCompositionCustomer> customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;
            Assert.Equal(new[] { 11 }, customers.Select(customer => customer.Id));
        }

        [Fact]
        public void MaxSkip_ThrowsWhenLimitReached()
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

        [Fact]
        public void MaxTop_ThrowsWhenLimitReached()
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCustomerValidation", useCustomEdmModel: false));
            HttpClient client = new HttpClient(server);

            // top = 2 is ok
            HttpResponseMessage response = client.GetAsync("http://localhost:8080/QueryCompositionCustomerValidation/?$top=2").Result;
            response.EnsureSuccessStatusCode();

            List<QueryCompositionCustomer> customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;
            Assert.Equal(new[] { 3, 11 }, customers.Select(customer => customer.Id));

            // top = 3 exceeds the limit
            response = client.GetAsync("http://localhost:8080/QueryCompositionCustomerValidation/?$top=3").Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.True(response.Content.ReadAsStringAsync().Result.Contains("The limit of '2' for Top query has been exceeded. The value from the incoming request is '3'."));
        }

        [Fact]
        public void AllowedArithmeticOperators_ThrowsOnNotAllowedOperators()
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCustomerValidation", useCustomEdmModel: false));
            HttpClient client = new HttpClient(server);

            // mod is ok
            HttpResponseMessage response = client.GetAsync("http://localhost:8080/QueryCompositionCustomerValidation/?$filter=Id mod 2 eq 0").Result;
            response.EnsureSuccessStatusCode();

            List<QueryCompositionCustomer> customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;
            Assert.Equal(new[] { 22 }, customers.Select(customer => customer.Id));

            // add is not allowed
            response = client.GetAsync("http://localhost:8080/QueryCompositionCustomerValidation/?$filter=Id add 2 eq 5").Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.True(response.Content.ReadAsStringAsync().Result.Contains("Arithmetic operator 'Add' is not allowed. To allow it, set the 'AllowedArithmeticOperators' property on QueryableAttribute or QueryValidationSettings."));
        }

        [Fact]
        public void AllowedFunctions_ThrowsOnNotAllowedFunctions()
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCustomerValidation", useCustomEdmModel: false));
            HttpClient client = new HttpClient(server);

            // length is ok
            HttpResponseMessage response = client.GetAsync("http://localhost:8080/QueryCompositionCustomerValidation/?$filter=length(Name) eq 6").Result;
            response.EnsureSuccessStatusCode();

            List<QueryCompositionCustomer> customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;
            Assert.Equal(new[] { 11, 22, 3 }, customers.Select(customer => customer.Id));

            // endswith is not allowed
            response = client.GetAsync("http://localhost:8080/QueryCompositionCustomerValidation/?$filter=endswith(Name,'t')").Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.True(response.Content.ReadAsStringAsync().Result.Contains("Function 'endswith' is not allowed. To allow it, set the 'AllowedFunctions' property on QueryableAttribute or QueryValidationSettings."));
        }

        [Fact]
        public void AllowedLogicalOperators_ThrowsOnNotAllowedOperators()
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCustomerValidation", useCustomEdmModel: false));
            HttpClient client = new HttpClient(server);

            // eq is ok
            HttpResponseMessage response = client.GetAsync("http://localhost:8080/QueryCompositionCustomerValidation/?$filter=length(Name) eq 6").Result;
            response.EnsureSuccessStatusCode();

            List<QueryCompositionCustomer> customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;
            Assert.Equal(new[] { 11, 22, 3 }, customers.Select(customer => customer.Id));

            // ne is not allowed
            response = client.GetAsync("http://localhost:8080/QueryCompositionCustomerValidation/?$filter=length(Name) ne 6").Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.True(response.Content.ReadAsStringAsync().Result.Contains("Logical operator 'NotEqual' is not allowed. To allow it, set the 'AllowedLogicalOperators' property on QueryableAttribute or QueryValidationSettings."));
        }

        [Fact]
        public void Top_ThrowsWhenItIsNotAllowed()
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCategoryValidation", useCustomEdmModel: false));
            HttpClient client = new HttpClient(server);

            // orderby is ok
            HttpResponseMessage response = client.GetAsync("http://localhost:8080/QueryCompositionCategoryValidation/?$orderby=Id").Result;
            response.EnsureSuccessStatusCode();

            // top = 3 will fail
            response = client.GetAsync("http://localhost:8080/QueryCompositionCategoryValidation/?$top=3").Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.True(response.Content.ReadAsStringAsync().Result.Contains("Query option 'Top' is not allowed. To allow it, set the 'AllowedQueryOptions' property on QueryableAttribute or QueryValidationSettings."));
        }

        [Fact]
        public void OnlyAllowOrderById()
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCategoryValidation", useCustomEdmModel: false));
            HttpClient client = new HttpClient(server);

            // orderby id is ok
            HttpResponseMessage response = client.GetAsync("http://localhost:8080/QueryCompositionCategoryValidation/1?$orderby=Id").Result;
            response.EnsureSuccessStatusCode();

            // orderby Name will fail
            response = client.GetAsync("http://localhost:8080/QueryCompositionCategoryValidation/1?$orderby=Name").Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.True(response.Content.ReadAsStringAsync().Result.Contains("Order by 'Name' is not allowed. To allow it, set the 'AllowedOrderByProperties' property on QueryableAttribute or QueryValidationSettings."));
        }

        [Fact]
        public void CustomFilterQueryValidator()
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCustomerValidation", useCustomEdmModel: false));
            HttpClient client = new HttpClient(server);

            // constant 11 is ok
            HttpResponseMessage response = client.GetAsync("http://localhost:8080/QueryCompositionCustomerValidation/1?$filter=Id eq 11").Result;
            response.EnsureSuccessStatusCode();

            List<QueryCompositionCustomer> customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;
            Assert.Equal(new[] { 11 }, customers.Select(customer => customer.Id));

            // constant 101 is not allowed
            response = client.GetAsync("http://localhost:8080/QueryCompositionCustomerValidation/1?$filter=Id eq 101").Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.True(response.Content.ReadAsStringAsync().Result.Contains("Any constant that is more than 100 is not allowed."));
        }

        [Fact]
        public void CustomOrderByQueryValidator()
        {
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionCustomerValidation", useCustomEdmModel: false));
            HttpClient client = new HttpClient(server);

            // orderby Id is ok
            HttpResponseMessage response = client.GetAsync("http://localhost:8080/QueryCompositionCustomerValidation/1?$orderby=Id").Result;
            response.EnsureSuccessStatusCode();

            List<QueryCompositionCustomer> customers = response.Content.ReadAsAsync<List<QueryCompositionCustomer>>().Result;
            Assert.Equal(new[] { 3, 11, 22, 33 }, customers.Select(customer => customer.Id));

            // orderby Id then by Name is not allowed
            response = client.GetAsync("http://localhost:8080/QueryCompositionCustomerValidation/1?$orderby=Id,Name").Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.True(response.Content.ReadAsStringAsync().Result.Contains("Order by more than one property is not allowed."));
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
            // Arrange
            HttpServer server = new HttpServer(InitializeConfiguration("QueryCompositionPrimitive", useCustomEdmModel: false));
            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync("http://localhost:8080/QueryCompositionPrimitive?" + query).Result;
            response.EnsureSuccessStatusCode();

            // Assert
            List<int> results = response.Content.ReadAsAsync<List<int>>().Result;
            Assert.Equal(expectedResults, results);
        }

        private static HttpConfiguration InitializeConfiguration(string controllerName, bool useCustomEdmModel)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("default", "{controller}/{key}", new { key = RouteParameter.Optional });
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            if (controllerName == "QueryCompositionCustomerGlobal")
            {
                config.Filters.Add(new QueryableAttribute());
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
                actionContext.Request.SetEdmModel(_model);
            }
        }
    }
}
