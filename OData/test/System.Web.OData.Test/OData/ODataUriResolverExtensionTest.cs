// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

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

        private static HttpConfiguration GetQueryOptionConfiguration(bool caseInsensitive)
        {
            HttpConfiguration config = new[] { typeof(ParserExtenstionCustomersController) }.GetHttpConfiguration();
            config.EnableCaseInsensitive(caseInsensitive);
            config.MapODataServiceRoute("query", "query", GetEdmModel());
            return config;
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<ParserExtenstionCustomer>("ParserExtenstionCustomers");
            builder.EntitySet<ParserExtenstionOrder>("ParserExtenstionOrders");
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
                        Price = i*j
                    }).ToList()
            }).ToList();
    }

    public class ParserExtenstionCustomer
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public IList<ParserExtenstionOrder> Orders { get; set; }
    }

    public class ParserExtenstionOrder
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
    }
}
