// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.TestCommon.Models;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Web.OData
{
    public class ODataUriResolverExtensionTest
    {
        public static TheoryDataSet<string, string, string> PathSegmentIdentifierCaseInsensitiveCases
        {
            get
            {
                return new TheoryDataSet<string, string, string>()
                {
                    { "GET", "$meTadata", null },
                    { "GET", "rouTINGCustomers", "GetRoutingCustomers" },
                    { "GET", "rouTINGCustomers/System.Web.OData.Routing.VIP", "GetRoutingCustomersFromVIP" },
                    { "GET", "proDucts(10)", "Get(10)" },
                    { "PATCH", "rouTINGCustomers(10)", "PatchRoutingCustomer(10)" },
                    { "GET", "rouTINGCustomers(10)/ProDucTs", "GetProducts(10)" },
                    { "GET", "rouTINGCustomers(10)/naMe", "GetName(10)" },
                    { "GET", "rouTINGCustomers(10)/System.Web.ODATA.Routing.VIP/Name", "GetName(10)" },
                    { "PUT", "rouTINGCustomers(1)/Products/$ReF", "CreateRef(1)(Products)" },
                    { "POST", "rouTINGCustomers(1)/Products/$rEf", "CreateRef(1)(Products)" },
                    { "GET", "rouTINGCustomers(10)/Name/$vAlUe", "GetName(10)" },
                    { "GET", "rouTINGCustomers(10)/System.Web.OData.Routing.VIP/Name/$value", "GetName(10)" },
                    { "POST", "rouTINGCustomers(1)/default.getRElatedRoutingCustomers", "GetRelatedRoutingCustomers(1)" },
                    { "POST",
                        "rouTINGCustomers(1)/System.Web.OData.Routing.VIP/getRelatedrouTingCustomers",
                        "GetRelatedRoutingCustomers(1)" },
                    { "POST", "rouTINGCustomers/DefaulT.getProducts", "GetProducts" },
                    { "POST", "rouTINGCustomers/System.Web.OData.Routing.VIP/getProducts", "GetProducts" },
                    { "GET", "ProDucts(1)/ToppRoDuctId", "TopProductId(1)" },
                    { "GET", "ProDucts(1)/Default.ToppRoductIdByCity(ciTy='any')", "TopProductIdByCity(1, any)" },
                    { "GET", "ProDucts/TopProductofaLl", "TopProductOfAll" },
                    { "GET", "ProDucts/Default.TopProductOfallbyCity(CiTy='any')", "TopProductOfAllByCity(any)" }
                };
            }
        }

        [Theory]
        [PropertyData("PathSegmentIdentifierCaseInsensitiveCases")]
        public void DefaultResolver_DoesnotWork_CaseInsensitive(string httpMethod, string odataPath, string expect)
        {
            // Arrange
            HttpClient client =
                new HttpClient(new HttpServer(GetConfiguration(caseInsensitive: false, unqualifiedNameCall: false)));

            // Act
            HttpResponseMessage response = client.SendAsync(new HttpRequestMessage(
                new HttpMethod(httpMethod), "http://localhost/odata/" + odataPath)).Result;

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [PropertyData("PathSegmentIdentifierCaseInsensitiveCases")]
        public void ExtensionResolver_Works_CaseInsensitive(string httpMethod, string odataPath, string expect)
        {
            // Arrange
            HttpClient client =
                new HttpClient(new HttpServer(GetConfiguration(caseInsensitive: true, unqualifiedNameCall: true)));

            // Act
            HttpResponseMessage response = client.SendAsync(new HttpRequestMessage(
                new HttpMethod(httpMethod), "http://localhost/odata/" + odataPath)).Result;

            // Assert
            response.EnsureSuccessStatusCode();

            if (expect != null)
            {
                Assert.Equal(expect, (response.Content as ObjectContent<string>).Value);
            }
        }

        private static HttpConfiguration GetConfiguration(bool caseInsensitive, bool unqualifiedNameCall)
        {
            IEdmModel model = ODataRoutingModel.GetModel();
            HttpConfiguration config = new[]
            {
                typeof(MetadataController),
                typeof(ProductsController),
                typeof(RoutingCustomersController),
            }.GetHttpConfiguration();

            config.EnableCaseInsensitive(caseInsensitive);
            config.EnableUnqualifiedNameCall(unqualifiedNameCall);

            config.MapODataServiceRoute("odata", "odata", model);
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
        [PropertyData("QueryOptionCaseInsensitiveCases")]
        public void DefaultResolver_DoesnotWork_ForQueryOption(string queryOption, string caseInsensitive)
        {
            // Arrange
            HttpClient client = new HttpClient(new HttpServer(GetQueryOptionConfiguration(caseInsensitive: false)));

            // Act
            HttpResponseMessage response = client.SendAsync(new HttpRequestMessage(
                HttpMethod.Get, "http://localhost/query/ParserExtenstionCustomers?" + caseInsensitive)).Result;

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [PropertyData("QueryOptionCaseInsensitiveCases")]
        public void ExtensionResolver_Works_ForQueryOption(string queryOption, string caseInsensitive)
        {
            // Arrange
            HttpClient client = new HttpClient(new HttpServer(GetQueryOptionConfiguration(caseInsensitive: true)));

            // Act
            HttpResponseMessage response = client.SendAsync(new HttpRequestMessage(
                HttpMethod.Get, "http://localhost/query/ParserExtenstionCustomers?" + caseInsensitive)).Result;

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Theory]
        [PropertyData("QueryOptionCaseInsensitiveCases")]
        public void ExtensionResolver_ReturnsSameResult_ForCaseSensitiveAndCaseInsensitive(string queryOption, string caseInsensitive)
        {
            // Arrange
            HttpClient caseSensitiveclient = new HttpClient(new HttpServer(GetQueryOptionConfiguration(caseInsensitive: false)));
            HttpClient caseInsensitiveclient = new HttpClient(new HttpServer(GetQueryOptionConfiguration(caseInsensitive: true)));

            // Act
            HttpResponseMessage response = caseSensitiveclient.SendAsync(new HttpRequestMessage(
                HttpMethod.Get, "http://localhost/query/ParserExtenstionCustomers?" + queryOption)).Result;
            response.EnsureSuccessStatusCode(); // Guard
            string caseSensitivePayload = response.Content.ReadAsStringAsync().Result;

            response = caseInsensitiveclient.SendAsync(new HttpRequestMessage(
                HttpMethod.Get, "http://localhost/query/ParserExtenstionCustomers?" + caseInsensitive)).Result;
            response.EnsureSuccessStatusCode(); // Guard
            string caseInsensitivePayload = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(caseSensitivePayload, caseInsensitivePayload);
        }

        private static HttpConfiguration GetQueryOptionConfiguration(bool caseInsensitive)
        {
            HttpConfiguration config = new[] { typeof(ParserExtenstionCustomersController) }.GetHttpConfiguration();
            config.EnableCaseInsensitive(caseInsensitive);
            config.MapODataServiceRoute("query", "query", GetEdmModel());
            return config;
        }

        [Theory]
        [InlineData("gender='Male'", true, HttpStatusCode.OK)]
        [InlineData("gender='Male'", false, HttpStatusCode.NotFound)]
        [InlineData("gender=System.Web.OData.TestCommon.Models.Gender'Male'", true, HttpStatusCode.OK)]
        [InlineData("gender=System.Web.OData.TestCommon.Models.Gender'Male'", false, HttpStatusCode.OK)]
        [InlineData("gender='SomeUnknowValue'", true, HttpStatusCode.NotFound)]
        [InlineData("gender=System.Web.OData.TestCommon.Models.Gender'SomeUnknowValue'", true, HttpStatusCode.NotFound)]
        public void ExtensionResolver_Works_EnumPrefixFree(string parameter, bool enableEnumPrefix, HttpStatusCode statusCode)
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            HttpConfiguration config = new[] { typeof(ParserExtenstionCustomersController) }.GetHttpConfiguration();
            config.EnableEnumPrefixFree(enableEnumPrefix);
            config.MapODataServiceRoute("odata", "odata", model);
            HttpClient client = new HttpClient(new HttpServer(config));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                String.Format("http://localhost/odata/ParserExtenstionCustomers/Default.GetCustomerByGender({0})", parameter));
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(statusCode, response.StatusCode);

            if (statusCode == HttpStatusCode.OK)
            {
                Assert.Equal("GetCustomerByGender/Male", (response.Content as ObjectContent<string>).Value);
            }
        }

        [Theory]
        [InlineData("$filter=Gender eq 'Male'", true, HttpStatusCode.OK, "0,2,4,6,8")]
        [InlineData("$filter=Gender eq 'Male'", false, HttpStatusCode.BadRequest, null)]
        [InlineData("$filter=Gender eq 'Female'", true, HttpStatusCode.OK, "1,3,5,7,9")]
        [InlineData("$filter=Gender eq 'Female'", false, HttpStatusCode.BadRequest, null)]
        [InlineData("$filter=Gender eq System.Web.OData.TestCommon.Models.Gender'Male'", true, HttpStatusCode.OK, "0,2,4,6,8")]
        [InlineData("$filter=Gender eq System.Web.OData.TestCommon.Models.Gender'Male'", false, HttpStatusCode.OK, "0,2,4,6,8")]
        [InlineData("$filter=Gender eq System.Web.OData.TestCommon.Models.Gender'Female'", true, HttpStatusCode.OK, "1,3,5,7,9")]
        [InlineData("$filter=Gender eq System.Web.OData.TestCommon.Models.Gender'Female'", false, HttpStatusCode.OK, "1,3,5,7,9")]
        [InlineData("$filter=Gender eq 'SomeUnknowValue'", true, HttpStatusCode.BadRequest, null)]
        [InlineData("$filter=Gender eq System.Web.OData.TestCommon.Models.Gender'SomeUnknowValue'", true, HttpStatusCode.BadRequest, null)]
        [InlineData("$filter=NullableGender eq 'Male'", true, HttpStatusCode.OK, "")]
        [InlineData("$filter=NullableGender eq System.Web.OData.TestCommon.Models.Gender'Male'", true, HttpStatusCode.OK, "")]
        [InlineData("$filter=NullableGender eq 'Female'", true, HttpStatusCode.OK, "1,3,5,7,9")]
        [InlineData("$filter=NullableGender eq System.Web.OData.TestCommon.Models.Gender'Female'", true, HttpStatusCode.OK, "1,3,5,7,9")]
        [InlineData("$filter=NullableGender eq null", true, HttpStatusCode.OK, "0,2,4,6,8")]
        public void ExtensionResolver_Works_EnumPrefixFree_QueryOption(string query, bool enableEnumPrefix, HttpStatusCode statusCode, string output)
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            HttpConfiguration config = new[] { typeof(ParserExtenstionCustomersController) }.GetHttpConfiguration();
            config.EnableEnumPrefixFree(enableEnumPrefix);
            config.MapODataServiceRoute("odata", "odata", model);
            HttpClient client = new HttpClient(new HttpServer(config));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                String.Format("http://localhost/odata/ParserExtenstionCustomers?{0}", query));
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(statusCode, response.StatusCode);

            if (statusCode == HttpStatusCode.OK)
            {
                JObject content = response.Content.ReadAsAsync<JObject>().Result;
                Assert.Equal(output, String.Join(",", content["value"].Select(e => e["Id"])));
            }
        }

        [Fact]
        public void DefaultResolver_DoesnotWorks_UnqualifiedNameTemplate()
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            HttpConfiguration config = new[] { typeof(ParserExtenstionCustomers2Controller) }.GetHttpConfiguration();
            config.EnableUnqualifiedNameCall(false);
            config.MapODataServiceRoute("odata", "odata", model);
            HttpClient client = new HttpClient(new HttpServer(config));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                "http://localhost/odata/ParserExtenstionCustomers2");

            // Assert
            Assert.Throws<InvalidOperationException>(() => client.SendAsync(request).Result);
        }

        [Fact]
        public void ExtensionResolver_Works_UnqualifiedNameTemplate()
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            HttpConfiguration config = new[] { typeof(ParserExtenstionCustomers2Controller) }.GetHttpConfiguration();
            config.EnableUnqualifiedNameCall(true);
            config.MapODataServiceRoute("odata", "odata", model);
            HttpClient client = new HttpClient(new HttpServer(config));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                "http://localhost/odata/ParserExtenstionCustomers2/GetCustomerTitleById(id=32)");
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("GetCustomerTitleById/32", (response.Content as ObjectContent<string>).Value);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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
