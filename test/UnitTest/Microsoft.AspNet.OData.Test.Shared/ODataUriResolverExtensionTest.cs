//-----------------------------------------------------------------------------
// <copyright file="ODataUriResolverExtensionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Models;
using Microsoft.AspNet.OData.Test.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class ODataUriResolverExtensionTest
    {
        public static TheoryDataSet<string, string, string> PathSegmentIdentifierCaseInsensitiveCases
        {
            get
            {
                return new TheoryDataSet<string, string, string>()
                {
                    { "GET", "$meTadata", "" },
                    { "GET", "rouTINGCustomers", "GetRoutingCustomers" },
                    { "GET", "rouTINGCustomers/Microsoft.AspNet.OData.Test.Routing.VIP", "GetRoutingCustomersFromVIP" },
                    { "GET", "proDucts(10)", "Get(10)" },
                    { "PATCH", "rouTINGCustomers(10)", "PatchRoutingCustomer(10)" },
                    { "GET", "rouTINGCustomers(10)/ProDucTs", "GetProducts(10)" },
                    { "GET", "rouTINGCustomers(10)/naMe", "GetName(10)" },
                    { "GET", "rouTINGCustomers(10)/Microsoft.AspNet.OData.Test.Routing.VIP/Name", "GetName(10)" },
                    { "PUT", "rouTINGCustomers(1)/Products/$ReF", "CreateRef(1)(Products)" },
                    { "POST", "rouTINGCustomers(1)/Products/$rEf", "CreateRef(1)(Products)" },
                    { "GET", "rouTINGCustomers(10)/Name/$vAlUe", "GetName(10)" },
                    { "GET", "rouTINGCustomers(10)/Microsoft.AspNet.OData.Test.Routing.VIP/Name/$value", "GetName(10)" },
                    { "POST", "rouTINGCustomers(1)/default.getRElatedRoutingCustomers", "GetRelatedRoutingCustomers(1)" },
                    { "POST",
                        "rouTINGCustomers(1)/Microsoft.AspNet.OData.Test.Routing.VIP/getRelatedrouTingCustomers",
                        "GetRelatedRoutingCustomers(1)" },
                    { "POST", "rouTINGCustomers/DefaulT.getProducts", "GetProducts" },
                    { "POST", "rouTINGCustomers/Microsoft.AspNet.OData.Test.Routing.VIP/getProducts", "GetProducts" },
                    { "GET", "ProDucts(1)/ToppRoDuctId", "TopProductId(1)" },
                    { "GET", "ProDucts(1)/Default.ToppRoductIdByCity(ciTy='any')", "TopProductIdByCity(1, any)" },
                    { "GET", "ProDucts/TopProductofaLl", "TopProductOfAll" },
                    { "GET", "ProDucts/Default.TopProductOfallbyCity(CiTy='any')", "TopProductOfAllByCity(any)" }
                };
            }
        }

        [Theory]
        [MemberData(nameof(PathSegmentIdentifierCaseInsensitiveCases))]
        public async Task DefaultResolver_DoesnotWork_CaseInsensitive(string httpMethod, string odataPath, string expect)
        {
            // Arrange
            HttpClient client =
                new HttpClient(new HttpServer(GetConfiguration(caseInsensitive: false, unqualifiedNameCall: false)));

            // Act
            HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(
                new HttpMethod(httpMethod), "http://localhost/odata/" + odataPath));

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(expect);
        }

        [Theory]
        [MemberData(nameof(PathSegmentIdentifierCaseInsensitiveCases))]
        public async Task ExtensionResolver_Works_CaseInsensitive(string httpMethod, string odataPath, string expect)
        {
            // Arrange
            HttpClient client =
                new HttpClient(new HttpServer(GetConfiguration(caseInsensitive: true, unqualifiedNameCall: true)));

            // Act
            HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(
                new HttpMethod(httpMethod), "http://localhost/odata/" + odataPath));

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            if (!string.IsNullOrEmpty(expect))
            {
                Assert.Equal(expect, (response.Content as ObjectContent<string>).Value);
            }
        }

        private static HttpConfiguration GetConfiguration(bool caseInsensitive, bool unqualifiedNameCall)
        {
            IEdmModel model = ODataRoutingModel.GetModel();
            HttpConfiguration config = RoutingConfigurationFactory.CreateWithTypes(new[]
            {
                typeof(MetadataController),
                typeof(ProductsController),
                typeof(RoutingCustomersController),
            });

            ODataUriResolver resolver = new ODataUriResolver();
            if (unqualifiedNameCall)
            {
                resolver = new UnqualifiedODataUriResolver();
            }
            resolver.EnableCaseInsensitive = caseInsensitive;

            config.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            config.MapODataServiceRoute("odata", "odata",
                builder =>
                    builder.AddService(ServiceLifetime.Singleton, sp => model)
                        .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                            ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", config))
                        .AddService(ServiceLifetime.Singleton, sp => resolver));
            return config;
        }

        public static TheoryDataSet<string, string> QueryOptionCaseInsensitiveCases
        {
            get
            {
                return new TheoryDataSet<string, string>()
                {
                    { "$select=Id", "$sElEct=iD" },
                    { "$select=Id", "$select=ID" },
                    { "$filter=Id eq 2", "$filTeR=Id eq 2" },
                    { "$filter=Id gt 2", "$filTeR=iD GT 2" },
                    { "$top=2", "$tOP=2" },
                    { "$skip=2", "$sKIp=2" },
                    { "$count=true", "$cOUnt=true" },
                    { "$filter=cast(Id,Edm.String) eq '3'", "$filter=cASt(iD,Edm.String) eq '3'" },
                    { "$expand=Orders", "$expand=orDeRs" },
                    { "$expand=Orders", "$eXpAnd=OrDeRs" },
                    { "$orderby=Title desc&$count=true", "$orderby=tiTle desc&$count=true" },
                    { "$orderby=Title desc&$top=3&$count=true", "$orderby=tiTle desc&$top=3&$count=true" },
                    { "$orderby=Title desc&$skip=2&$count=true", "$orderby=tiTle desc&$skip=2&$count=true" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(QueryOptionCaseInsensitiveCases))]
        public async Task DefaultResolver_DoesnotWork_ForQueryOption(string queryOption, string caseInsensitive)
        {
            // Arrange
            HttpClient client = new HttpClient(new HttpServer(GetQueryOptionConfiguration(caseInsensitive: false)));

            // Act
            HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(
                HttpMethod.Get, "http://localhost/query/ParserExtenstionCustomers?" + caseInsensitive));

            // Assert
            Assert.NotNull(queryOption);
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(QueryOptionCaseInsensitiveCases))]
        public async Task ExtensionResolver_Works_ForQueryOption(string queryOption, string caseInsensitive)
        {
            // Arrange
            HttpClient client = new HttpClient(new HttpServer(GetQueryOptionConfiguration(caseInsensitive: true)));

            // Act
            HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(
                HttpMethod.Get, "http://localhost/query/ParserExtenstionCustomers?" + caseInsensitive));

            // Assert
            Assert.NotNull(queryOption);
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
        }

        [Theory]
        [MemberData(nameof(QueryOptionCaseInsensitiveCases))]
        public async Task ExtensionResolver_ReturnsSameResult_ForCaseSensitiveAndCaseInsensitive(string queryOption, string caseInsensitive)
        {
            // Arrange
            HttpClient caseSensitiveclient = new HttpClient(new HttpServer(GetQueryOptionConfiguration(caseInsensitive: true)));
            HttpClient caseInsensitiveclient = new HttpClient(new HttpServer(GetQueryOptionConfiguration(caseInsensitive: true)));

            // Act
            HttpResponseMessage response = await caseSensitiveclient.SendAsync(new HttpRequestMessage(
                HttpMethod.Get, "http://localhost/query/ParserExtenstionCustomers?" + queryOption));
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode()); // Guard
            string caseSensitivePayload = await response.Content.ReadAsStringAsync();

            response = await caseInsensitiveclient.SendAsync(new HttpRequestMessage(
                HttpMethod.Get, "http://localhost/query/ParserExtenstionCustomers?" + caseInsensitive));
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode()); // Guard
            string caseInsensitivePayload = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(caseSensitivePayload, caseInsensitivePayload);
        }

        private static HttpConfiguration GetQueryOptionConfiguration(bool caseInsensitive)
        {
            var config = RoutingConfigurationFactory.CreateWithTypes(new[] { typeof(ParserExtenstionCustomersController) });
            ODataUriResolver resolver = new ODataUriResolver
            {
                EnableCaseInsensitive = caseInsensitive,
            };

            config.Count().OrderBy().Filter().Expand().MaxTop(null).Select();
            config.MapODataServiceRoute("query", "query",
                builder =>
                    builder.AddService(ServiceLifetime.Singleton, sp => GetEdmModel())
                        .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                            ODataRoutingConventions.CreateDefaultWithAttributeRouting("query", config))
                        .AddService(ServiceLifetime.Singleton, sp => resolver));
            return config;
        }

        [Theory]
        [InlineData("gender='Male'", true, HttpStatusCode.OK)]
        [InlineData("gender='Male'", false, HttpStatusCode.OK)]
        [InlineData("gender=Microsoft.AspNet.OData.Test.Common.Models.Gender'Male'", true, HttpStatusCode.OK)]
        [InlineData("gender=Microsoft.AspNet.OData.Test.Common.Models.Gender'Male'", false, HttpStatusCode.OK)]
        [InlineData("gender='SomeUnknowValue'", true, HttpStatusCode.NotFound)]
        [InlineData("gender=Microsoft.AspNet.OData.Test.Common.Models.Gender'SomeUnknowValue'", true, HttpStatusCode.NotFound)]
        public async Task ExtensionResolver_Works_EnumPrefixFree(string parameter, bool enableEnumPrefix, HttpStatusCode statusCode)
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            var config = RoutingConfigurationFactory.CreateWithTypes(new[] { typeof(ParserExtenstionCustomersController) });
            ODataUriResolver resolver = new ODataUriResolver();
            if (enableEnumPrefix)
            {
                resolver = new StringAsEnumResolver();
            }

            config.MapODataServiceRoute("odata", "odata",
                builder =>
                    builder.AddService(ServiceLifetime.Singleton, sp => model)
                        .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                            ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", config))
                        .AddService(ServiceLifetime.Singleton, sp => resolver));
            HttpClient client = new HttpClient(new HttpServer(config));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                String.Format("http://localhost/odata/ParserExtenstionCustomers/Default.GetCustomerByGender({0})", parameter));
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(statusCode, response.StatusCode);

            if (statusCode == HttpStatusCode.OK)
            {
                Assert.Equal("GetCustomerByGender/Male", (response.Content as ObjectContent<string>).Value);
            }
        }

        [Theory]
        [InlineData("$filter=Gender eq 'Male'", true, HttpStatusCode.OK, "0,2,4,6,8")]
        [InlineData("$filter=Gender eq 'Male'", false, HttpStatusCode.OK, "0,2,4,6,8")]
        [InlineData("$filter=Gender eq 'Female'", true, HttpStatusCode.OK, "1,3,5,7,9")]
        [InlineData("$filter=Gender eq 'Female'", false, HttpStatusCode.OK, "1,3,5,7,9")]
        [InlineData("$filter=Gender eq Microsoft.AspNet.OData.Test.Common.Models.Gender'Male'", true, HttpStatusCode.OK, "0,2,4,6,8")]
        [InlineData("$filter=Gender eq Microsoft.AspNet.OData.Test.Common.Models.Gender'Male'", false, HttpStatusCode.OK, "0,2,4,6,8")]
        [InlineData("$filter=Gender eq Microsoft.AspNet.OData.Test.Common.Models.Gender'Female'", true, HttpStatusCode.OK, "1,3,5,7,9")]
        [InlineData("$filter=Gender eq Microsoft.AspNet.OData.Test.Common.Models.Gender'Female'", false, HttpStatusCode.OK, "1,3,5,7,9")]
        [InlineData("$filter=Gender eq 'SomeUnknowValue'", true, HttpStatusCode.BadRequest, null)]
        [InlineData("$filter=Gender eq Microsoft.AspNet.OData.Test.Common.Models.Gender'SomeUnknowValue'", true, HttpStatusCode.BadRequest, null)]
        [InlineData("$filter=NullableGender eq 'Male'", true, HttpStatusCode.OK, "")]
        [InlineData("$filter=NullableGender eq Microsoft.AspNet.OData.Test.Common.Models.Gender'Male'", true, HttpStatusCode.OK, "")]
        [InlineData("$filter=NullableGender eq 'Female'", true, HttpStatusCode.OK, "1,3,5,7,9")]
        [InlineData("$filter=NullableGender eq Microsoft.AspNet.OData.Test.Common.Models.Gender'Female'", true, HttpStatusCode.OK, "1,3,5,7,9")]
        [InlineData("$filter=NullableGender eq null", true, HttpStatusCode.OK, "0,2,4,6,8")]
        public async Task ExtensionResolver_Works_EnumPrefixFree_QueryOption(string query, bool enableEnumPrefix, HttpStatusCode statusCode, string output)
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            var config = RoutingConfigurationFactory.CreateWithTypes(new[] { typeof(ParserExtenstionCustomersController) });
            ODataUriResolver resolver = new ODataUriResolver();
            if (enableEnumPrefix)
            {
                resolver = new StringAsEnumResolver();
            }

            config.MapODataServiceRoute("odata", "odata",
                builder =>
                    builder.AddService(ServiceLifetime.Singleton, sp => model)
                        .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                            ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", config))
                        .AddService(ServiceLifetime.Singleton, sp => resolver));
            HttpClient client = new HttpClient(new HttpServer(config));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                String.Format("http://localhost/odata/ParserExtenstionCustomers?{0}", query));
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(statusCode, response.StatusCode);

            if (statusCode == HttpStatusCode.OK)
            {
                JObject content = await response.Content.ReadAsAsync<JObject>();
                Assert.Equal(output, String.Join(",", content["value"].Select(e => e["Id"])));
            }
        }

        /* TODO: it's random failed when run all at CMD. So far, skip it.
        [Fact]
        public async Task DefaultResolver_DoesnotWorks_UnqualifiedNameTemplate()
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            var config = RoutingConfigurationFactory.CreateWithTypes(new[] { typeof(ParserExtenstionCustomers2Controller) });
            config.MapODataServiceRoute("odata", "odata", model);
            HttpClient client = new HttpClient(new HttpServer(config));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                "http://localhost/odata/ParserExtenstionCustomers2");

            // Assert
            await ExceptionAssertions.ThrowsAsync<InvalidOperationException>(() => client.SendAsync(request),
                "The path template 'ParserExtenstionCustomers2/GetCustomerTitleById(id={id})' on the action 'GetCustomerByTitleVarN' " +
                "in controller 'ParserExtenstionCustomers2' is not a valid OData path template. The request URI is not valid. Since the " +
                "segment 'ParserExtenstionCustomers2' refers to a collection, this must be the last segment in the request URI or it must be " +
                "followed by an function or action that can be bound to it otherwise all intermediate segments must refer to a single resource.");
        }
         * */

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<ParserExtenstionCustomer>("ParserExtenstionCustomers");
            builder.EntitySet<ParserExtenstionCustomer>("ParserExtenstionCustomers2");
            builder.EntitySet<ParserExtenstionOrder>("ParserExtenstionOrders");

            builder.EntityType<ParserExtenstionCustomer>()
                .Collection.Function("GetCustomerByGender")
                .Returns<string>()
                .Parameter<Gender>("gender");
            builder.EntityType<ParserExtenstionCustomer>()
                .Collection.Function("GetCustomerTitleById")
                .Returns<string>()
                .Parameter<int>("id");
            return builder.GetEdmModel();
        }
    }

    public class ParserExtenstionCustomersController : ODataController
    {
        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(ParserExtensionCustomersContext.customers);
        }

        [EnableQuery]
        public IHttpActionResult GetOrders(int key)
        {
            ParserExtenstionCustomer customer = ParserExtensionCustomersContext.customers.First(c => c.Id == key);
            return Ok(customer.Orders);
        }

        [HttpGet]
        public IHttpActionResult GetCustomerByGender(Gender gender)
        {
            return Ok("GetCustomerByGender/" + gender);
        }
    }

    public class ParserExtenstionCustomers2Controller : ODataController
    {
        [HttpGet]
        [ODataRoute("ParserExtenstionCustomers2/GetCustomerTitleById(id={id})")]
        public IHttpActionResult GetCustomerByTitleVarN([FromODataUri]int id)
        {
            var t = ModelState.IsValid;
            return Ok("GetCustomerTitleById/" + id);
        }
    }

    class ParserExtensionCustomersContext
    {
        public static IList<ParserExtenstionCustomer> customers = Enumerable.Range(0, 10).Select(i =>
            new ParserExtenstionCustomer
            {
                Id = i,
                Title = "Title # " + i,
                Orders = Enumerable.Range(0, i).Select(j =>
                    new ParserExtenstionOrder
                    {
                        Id = j,
                        Price = i * j
                    }).ToList(),
                Gender = i % 2 == 0 ? Gender.Male : Gender.Female,
                NullableGender = i % 2 == 0 ? (Gender?)null : Gender.Female
            }).ToList();
    }

    public class ParserExtenstionCustomer
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public Gender Gender { get; set; }
        public Gender? NullableGender { get; set; }
        public IList<ParserExtenstionOrder> Orders { get; set; }
    }

    public class ParserExtenstionOrder
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
    }
}
#endif
