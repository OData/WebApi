// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Extensions;
using System.Web.OData.TestCommon;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;

namespace System.Web.OData.Routing
{
    public class ODataRoutingTest
    {
        private readonly HttpServer _nullPrefixServer;
        private readonly HttpClient _nullPrefixClient;
        private readonly HttpServer _fixedPrefixServer;
        private readonly HttpClient _fixedPrefixClient;
        private readonly HttpServer _parameterizedPrefixServer;
        private readonly HttpClient _parameterizedPrefixClient;

        public ODataRoutingTest()
        {
            var model = ODataRoutingModel.GetModel();

            // Separate clients and servers so routes are not ambiguous.
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.MapODataServiceRoute("NullPrefixRoute", null, model);

            _nullPrefixServer = CreateServer(configuration);
            _nullPrefixClient = new HttpClient(_nullPrefixServer);

            // FixedPrefixRoute has both a non-empty virtual path root and a fixed route prefix.
            configuration = new HttpConfiguration(new HttpRouteCollection("MyRoot"));
            configuration.MapODataServiceRoute("FixedPrefixRoute", "odata", model);

            _fixedPrefixServer = CreateServer(configuration);
            _fixedPrefixClient = new HttpClient(_fixedPrefixServer);

            configuration = new HttpConfiguration();
            configuration.MapODataServiceRoute("ParameterizedPrefixRoute", "{a}", model);

            _parameterizedPrefixServer = CreateServer(configuration);
            _parameterizedPrefixClient = new HttpClient(_parameterizedPrefixServer);
        }

        public static TheoryDataSet<string, string, string> ServiceAndMetadataRoutes
        {
            get
            {
                return new TheoryDataSet<string, string, string>
                {
                    // service document
                    { "GET", "", null },
                    { "GET", "?hello=goodbye", null },
                    { "GET", "?hello= good bye", null },
                    { "GET", "?hello= good+bye", null },
                    { "GET", "?hello = good%20bye", null },
                    // metadata document
                    { "GET", "$metadata", null },
                    { "GET", "$metadata?hello = good%20bye", null },
                };
            }
        }

        public static TheoryDataSet<string, string, string> ControllerRoutes
        {
            get
            {
                return new TheoryDataSet<string, string, string>
                {
                    // entity set defaults
                    { "GET", "Products", "Get" },
                    { "POST", "Products", "Post" },
                    // entity set
                    { "GET", "RoutingCustomers", "GetRoutingCustomers" },
                    { "POST", "RoutingCustomers", "PostRoutingCustomer" },
                    { "GET", "RoutingCustomers?hello = good bye", "GetRoutingCustomers" },
                    { "GET", "RoutingCustomers/", "GetRoutingCustomers" },
                    { "GET", "RoutingCustomers/?hello=goodbye", "GetRoutingCustomers" },
                    // entity set / cast
                    { "GET", "RoutingCustomers/System.Web.OData.Routing.VIP", "GetRoutingCustomersFromVIP" },
                    { "POST", "RoutingCustomers/System.Web.OData.Routing.VIP", "PostRoutingCustomerFromVIP" },
                    // entity by key defaults
                    { "GET", "Products(10)", "Get(10)" },
                    { "PUT", "Products(10)", "Put(10)" },
                    { "PATCH", "Products(10)", "Patch(10)" },
                    { "MERGE", "Products(10)", "Patch(10)" },
                    { "DELETE", "Products(10)", "Delete(10)" },
                    // entity by key
                    { "GET", "RoutingCustomers(10)", "GetRoutingCustomer(10)" },
                    { "PUT", "RoutingCustomers(10)", "PutRoutingCustomer(10)" },
                    { "PATCH", "RoutingCustomers(10)", "PatchRoutingCustomer(10)" },
                    { "MERGE", "RoutingCustomers(10)", "PatchRoutingCustomer(10)" },
                    { "DELETE", "RoutingCustomers(10)", "DeleteRoutingCustomer(10)" },
                    // navigation properties
                    { "GET", "RoutingCustomers(10)/Products", "GetProducts(10)" },
                    { "GET", "RoutingCustomers(10)/System.Web.OData.Routing.VIP/Products", "GetProducts(10)" },
                    { "GET",
                        "RoutingCustomers(10)/System.Web.OData.Routing.VIP/RelationshipManager",
                        "GetRelationshipManagerFromVIP(10)" },
                    // structural properties
                    { "GET", "RoutingCustomers(10)/Name", "GetName(10)" },
                    { "GET", "RoutingCustomers(10)/Address", "GetAddress(10)" },
                    { "GET", "RoutingCustomers(10)/System.Web.OData.Routing.VIP/Name", "GetName(10)" },
                    { "GET", "RoutingCustomers(10)/System.Web.OData.Routing.VIP/Company", "GetCompanyFromVIP(10)" },
                    // refs
                    { "PUT", "RoutingCustomers(1)/Products/$ref", "CreateRef(1)(Products)" },
                    { "POST", "RoutingCustomers(1)/Products/$ref", "CreateRef(1)(Products)" },
                    { "DELETE", "RoutingCustomers(1)/Products/$ref", "DeleteRef(1)(Products)" },
                    { "DELETE", "RoutingCustomers(1)/Products(5)/$ref", "DeleteRef(1)(5)(Products)" },
                    { "DELETE", "RoutingCustomers(1)/Products/$ref?$id=../../Products(5)", "DeleteRef(1)(5)(Products)" },
                    // raw value
                    { "GET", "RoutingCustomers(10)/Name/$value", "GetName(10)" },
                    { "GET", "RoutingCustomers(10)/System.Web.OData.Routing.VIP/Name/$value", "GetName(10)" },
                    { "GET", "RoutingCustomers(10)/System.Web.OData.Routing.VIP/Company/$value", "GetCompanyFromVIP(10)" },
                    // actions on entities by key
                    { "POST", "RoutingCustomers(1)/Default.GetRelatedRoutingCustomers", "GetRelatedRoutingCustomers(1)" },
                    { "POST",
                        "RoutingCustomers(1)/System.Web.OData.Routing.VIP/Default.GetRelatedRoutingCustomers",
                        "GetRelatedRoutingCustomers(1)" },
                    { "POST",
                        "RoutingCustomers(1)/System.Web.OData.Routing.VIP/Default.GetSalesPerson",
                        "GetSalesPersonOnVIP(1)" },
                    // actions on entity sets
                    { "POST", "RoutingCustomers/Default.GetProducts", "GetProducts" },
                    { "POST", "RoutingCustomers/System.Web.OData.Routing.VIP/Default.GetProducts", "GetProducts" },
                    { "POST",
                        "RoutingCustomers/System.Web.OData.Routing.VIP/Default.GetMostProfitable",
                        "GetMostProfitableOnCollectionOfVIP" },
                    // functions on entities by key
                    { "GET", "Products(1)/Default.TopProductId", "TopProductId(1)" },
                    { "GET", "Products(1)/Default.TopProductIdByCity(city='any')", "TopProductIdByCity(1, any)" },
                    { "GET",
                        "Products(1)/Default.TopProductIdByCity(city=@city)?@city='any'",
                        "TopProductIdByCity(1, any)" },
                    { "GET",
                        "Products(1)/Default.TopProductIdByCityAndModel(city='any',model=2)",
                        "TopProductIdByCityAndModel(1, any, 2)" },
                    { "GET",
                        "Products(1)/Default.TopProductIdByCityAndModel(city=@city,model=@model)?@city='any'&@model=2",
                        "TopProductIdByCityAndModel(1, any, 2)" },
                    { "GET",
                        "Products(1)/System.Web.OData.Routing.ImportantProduct/Default.TopProductId",
                        "TopProductId(1)" },
                    // functions on entity sets
                    { "GET", "Products/Default.TopProductOfAll", "TopProductOfAll" },
                    { "GET", "Products/Default.TopProductOfAllByCity(city='any')", "TopProductOfAllByCity(any)" },
                    { "GET", "Products/Default.TopProductOfAllByCity(city='some%23hashes')", "TopProductOfAllByCity(some#hashes)" },
                    { "GET", "Products/Default.TopProductOfAllByCity(city='some%2fslashes')", "TopProductOfAllByCity(some/slashes)" },
                    { "GET", "Products/Default.TopProductOfAllByCity(city='some%3Fquestion%3Fmarks')", "TopProductOfAllByCity(some?question?marks)" },
                    { "GET", "Products/Default.TopProductOfAllByCity(city='some%3flower%23escapes')", "TopProductOfAllByCity(some?lower#escapes)" },
                    { "GET", "Products/Default.TopProductOfAllByCity(city='some%20spaces')", "TopProductOfAllByCity(some spaces)" },
                    { "GET", "Products/Default.TopProductOfAllByCity(city='_some+plus+signs_')", "TopProductOfAllByCity(_some+plus+signs_)" },
                    { "GET", "Products/Default.TopProductOfAllByCity(city='some(sub)and&other=delims')", "TopProductOfAllByCity(some(sub)and&other=delims)" },
                    { "GET", "Products/Default.TopProductOfAllByCity(city='some(delims)but%2Bupper:escaped')", "TopProductOfAllByCity(some(delims)but+upper:escaped)" },
                    { "GET", "Products/Default.TopProductOfAllByCity(city='some(delims)but%2blower:escaped')", "TopProductOfAllByCity(some(delims)but+lower:escaped)" },
                    { "GET", "Products/Default.TopProductOfAllByCity(city=':@')", "TopProductOfAllByCity(:@)" },
                    { "GET", "Products/Default.TopProductOfAllByCity(city='Chinese%E8%A5%BF%E9%9B%85%E5%9B%BEChars')", "TopProductOfAllByCity(Chinese西雅图Chars)" },
                    { "GET", "Products/Default.TopProductOfAllByCity(city='Unicode%D8%83Format%D8%83Char')", "TopProductOfAllByCity(Unicode؃Format؃Char)" },
                    { "GET", "Products/Default.TopProductOfAllByCity(city='Unicode%E1%BF%BCTitlecase%E1%BF%BCChar')", "TopProductOfAllByCity(UnicodeῼTitlecaseῼChar)" },
                    { "GET", "Products/Default.TopProductOfAllByCity(city='Unicode%E0%A4%83Combining%E0%A4%83Char')", "TopProductOfAllByCity(UnicodeःCombiningःChar)" },
                    { "GET",
                        "Products/Default.TopProductOfAllByCity(city=@city)?@city='any'",
                        "TopProductOfAllByCity(any)" },
                    { "GET",
                        "Products/Default.TopProductOfAllByCityAndModel(city='any',model=2)",
                        "TopProductOfAllByCityAndModel(any, 2)" },
                    { "GET",
                        "Products/Default.TopProductOfAllByCityAndModel(city=@city,model=@model)?@city='any'&@model=2",
                        "TopProductOfAllByCityAndModel(any, 2)" },
                    { "GET",
                        "Products/System.Web.OData.Routing.ImportantProduct/Default.TopProductOfAllByCity(city='any')",
                        "TopProductOfAllByCity(any)" },
                    // functions bound to the base and derived type
                    { "GET", "RoutingCustomers(4)/Default.GetOrdersCount()", "GetOrdersCount_4" },
                    { "GET",
                        "RoutingCustomers(5)/System.Web.OData.Routing.VIP/Default.GetOrdersCount()",
                        "GetOrdersCountOnVIP_5" },
                    { "GET",
                        "RoutingCustomers(6)/System.Web.OData.Routing.SpecialVIP/Default.GetOrdersCount()",
                        "GetOrdersCountOnVIP_6" },
                    { "GET", "RoutingCustomers(7)/Default.GetOrdersCount(factor=3)", "GetOrdersCount_(7,3)" },
                    { "GET",
                        "RoutingCustomers(8)/System.Web.OData.Routing.VIP/Default.GetOrdersCount(factor=4)",
                        "GetOrdersCount_(8,4)" },
                    { "GET",
                        "RoutingCustomers(9)/System.Web.OData.Routing.SpecialVIP/Default.GetOrdersCount(factor=5)",
                        "GetOrdersCount_(9,5)" },
                    // functions bound to the collection of the base and the derived type
                    { "GET", "RoutingCustomers/Default.GetAllEmployees()", "GetAllEmployees" },
                    { "GET",
                        "RoutingCustomers/System.Web.OData.Routing.VIP/Default.GetAllEmployees()",
                        "GetAllEmployeesOnCollectionOfVIP" },
                    { "GET",
                        "RoutingCustomers/System.Web.OData.Routing.SpecialVIP/Default.GetAllEmployees()",
                        "GetAllEmployeesOnCollectionOfVIP" },
                    // functions only bound to derived type
                    { "GET", "RoutingCustomers(5)/Default.GetSpecialGuid()", "~/entityset/key/unresolved" },
                    { "GET",
                        "RoutingCustomers(5)/System.Web.OData.Routing.VIP/Default.GetSpecialGuid()",
                        "~/entityset/key/cast/unresolved" },
                    { "GET",
                        "RoutingCustomers(5)/System.Web.OData.Routing.SpecialVIP/Default.GetSpecialGuid()",
                        "GetSpecialGuid_5" },
                    // functions with enum type parameter
                    { "GET",
                        "RoutingCustomers/Default.BoundFuncWithEnumParameters(" +
                            "SimpleEnum=Microsoft.TestCommon.Types.SimpleEnum'1'," +
                            "FlagsEnum=Microsoft.TestCommon.Types.FlagsEnum'One, Four')",
                        "BoundFuncWithEnumParameters(Second,One, Four)" },
                    { "GET",
                        "RoutingCustomers/Default.BoundFuncWithEnumParameterForAttributeRouting(" +
                            "SimpleEnum=Microsoft.TestCommon.Types.SimpleEnum'First')",
                        "BoundFuncWithEnumParameterForAttributeRouting(First)" },
                    { "GET",
                        "UnboundFuncWithEnumParameters(LongEnum=Microsoft.TestCommon.Types.LongEnum'ThirdLong'," +
                            "FlagsEnum=Microsoft.TestCommon.Types.FlagsEnum'7')",
                        "UnboundFuncWithEnumParameters(ThirdLong,One, Two, Four)" },
                    // The OData ABNF doesn't allow spaces within enum literals. But ODL _requires_ spaces after commas.
                    { "GET",
                        "UnboundFuncWithEnumParameters(LongEnum=Microsoft.TestCommon.Types.LongEnum'ThirdLong'," +
                            "FlagsEnum=Microsoft.TestCommon.Types.FlagsEnum'One, Two, Four')",
                        "UnboundFuncWithEnumParameters(ThirdLong,One, Two, Four)" },
                    { "GET",
                        "UnboundFuncWithEnumParameters(LongEnum=@long,FlagsEnum=@flags)?" +
                            "@long=Microsoft.TestCommon.Types.LongEnum'ThirdLong'&" +
                            "@flags=Microsoft.TestCommon.Types.FlagsEnum'7'",
                        "UnboundFuncWithEnumParameters(ThirdLong,One, Two, Four)" },
                    { "GET",
                        "UnboundFuncWithEnumParameters(LongEnum=@long,FlagsEnum=@flags)?" +
                            "@long=Microsoft.TestCommon.Types.LongEnum'ThirdLong'&" +
                            "@flags=Microsoft.TestCommon.Types.FlagsEnum'One, Two, Four'",
                        "UnboundFuncWithEnumParameters(ThirdLong,One, Two, Four)" },
                    // unmapped requests
                    { "GET", "RoutingCustomers(10)/Products(1)", "~/entityset/key/navigation/key" },
                    { "CUSTOM", "RoutingCustomers(10)", "~/entityset/key" },
                    // entity by key with type DateTimeOffset
                    { "GET",
                        "DateTimeOffsetKeyCustomers(2001-01-01T12:00:00.000+08:00)",
                        "GetDateTimeOffsetKeyCustomer(01/01/2001 12:00:00 +08:00)" },
                };
            }
        }

        [Theory]
        [PropertyData("ControllerRoutes")]
        [InlineData("DELETE",
            "RoutingCustomers(1)/Products/$ref?$id=http://localhost/Products(5)",
            "DeleteRef(1)(5)(Products)")]
        public async Task RoutesCorrectly(string httpMethod, string uri, string expectedResponse)
        {
            // Arrange & Act
            HttpResponseMessage response = await _nullPrefixClient.SendAsync(new HttpRequestMessage(
                new HttpMethod(httpMethod), "http://localhost/" + uri));

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(expectedResponse, (response.Content as ObjectContent<string>).Value);
        }

        [Theory]
        [PropertyData("ServiceAndMetadataRoutes")]
        public async Task RoutesCorrectly_WithServiceAndMetadataRoutes(string httpMethod, string uri, string unused)
        {
            // Arrange & Act
            HttpResponseMessage response = await _nullPrefixClient.SendAsync(new HttpRequestMessage(
                new HttpMethod(httpMethod), "http://localhost/" + uri));

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Theory]
        [PropertyData("ServiceAndMetadataRoutes")]
        [PropertyData("ControllerRoutes")]
        [InlineData("DELETE",
            "RoutingCustomers(1)/Products/$ref?$id=http://localhost/MyRoot/odata/Products(5)",
            "DeleteRef(1)(5)(Products)")]
        public async Task RoutesCorrectly_WithFixedPrefix(string httpMethod, string uri, string unused)
        {
            // Arrange & Act
            HttpResponseMessage response = await _fixedPrefixClient.SendAsync(new HttpRequestMessage(
                new HttpMethod(httpMethod), "http://localhost/MyRoot/odata/" + uri));

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Theory]
        [PropertyData("ServiceAndMetadataRoutes")]
        [PropertyData("ControllerRoutes")]
        [InlineData("DELETE",
            "RoutingCustomers(1)/Products/$ref?$id=http://localhost/parameter/Products(5)",
            "DeleteRef(1)(5)(Products)")]
        public async Task RoutesCorrectly_WithParameterizedPrefix(string httpMethod, string uri, string unused)
        {
            // Arrange & Act
            HttpResponseMessage response = await _parameterizedPrefixClient.SendAsync(new HttpRequestMessage(
                new HttpMethod(httpMethod), "http://localhost/parameter/" + uri));

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Theory]
        [InlineData("parameter with spaces")]
        [InlineData("parameter with+spaces")]
        [InlineData("parameter with%20spaces")]
        [InlineData("parameter with spaces/?hello=goodbye")]
        [InlineData("parameter+with spaces/RoutingCustomers")]
        [InlineData("parameter%20with spaces/RoutingCustomers/?hello = good bye")]
        public async Task RoutesCorrectly_WithSpacesInPrefixParameter(string uri)
        {
            // Arrange & Act
            HttpResponseMessage response = await _parameterizedPrefixClient.GetAsync("http://localhost/" + uri);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Theory]
        [InlineData("EnumCustomers?$filter=System.Web.OData.Builder.TestModels.Color'Unknown' eq null", "The string 'System.Web.OData.Builder.TestModels.Color'Unknown'' is not a valid enumeration type constant.")]
        [InlineData("EnumCustomers?$filter=geo.length(null) eq null", "Unknown function 'geo.length'.")]
        [InlineData("EnumCustomers?$filter=Default.OverloadUnboundFunction() eq null", "Unknown function 'Default.OverloadUnboundFunction'.")]
        public async Task BadQueryString_ReturnsBadRequest(string uri, string expectedError)
        {
            // Arrange & Act
            HttpResponseMessage response = await _nullPrefixClient.GetAsync("http://localhost/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            string responseString = response.Content.ReadAsStringAsync().Result;
            Assert.Contains(expectedError, responseString);
        }

        private static HttpServer CreateServer(HttpConfiguration configuration)
        {
            // Need the MetadataController to resolve the service document as well as $metadata.
            var controllers = new[]
            {
                typeof(DateTimeOffsetKeyCustomersController),
                typeof(MetadataController),
                typeof(RoutingCustomersController),
                typeof(ProductsController),
                typeof(EnumCustomersController)
            };

            TestAssemblyResolver resolver = new TestAssemblyResolver(new MockAssembly(controllers));
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            return new HttpServer(configuration);
        }
    }

    public class DateTimeOffsetKeyCustomersController : ODataController
    {
        public string GetDateTimeOffsetKeyCustomer(DateTimeOffset key)
        {
            return String.Format(CultureInfo.InvariantCulture, "GetDateTimeOffsetKeyCustomer({0})", key);
        }
    }

    public class RoutingCustomersController : ODataController
    {
        public string GetRoutingCustomers()
        {
            return "GetRoutingCustomers";
        }

        public string PostRoutingCustomer()
        {
            return "PostRoutingCustomer";
        }

        public string GetRoutingCustomersFromVIP()
        {
            return "GetRoutingCustomersFromVIP";
        }

        public string PostRoutingCustomerFromVIP()
        {
            return "PostRoutingCustomerFromVIP";
        }

        public string GetRoutingCustomer(int key)
        {
            return String.Format(CultureInfo.InvariantCulture, "GetRoutingCustomer({0})", key);
        }

        public string PutRoutingCustomer(int key)
        {
            return String.Format(CultureInfo.InvariantCulture, "PutRoutingCustomer({0})", key);
        }

        [AcceptVerbs("PATCH", "MERGE")]
        public string PatchRoutingCustomer(int key)
        {
            return String.Format(CultureInfo.InvariantCulture, "PatchRoutingCustomer({0})", key);
        }

        public string DeleteRoutingCustomer(int key)
        {
            return String.Format(CultureInfo.InvariantCulture, "DeleteRoutingCustomer({0})", key);
        }

        public string GetProducts(int key)
        {
            return String.Format(CultureInfo.InvariantCulture, "GetProducts({0})", key);
        }

        public string GetRelationshipManagerFromVIP(int key)
        {
            return String.Format(CultureInfo.InvariantCulture, "GetRelationshipManagerFromVIP({0})", key);
        }

        public string GetName(int key)
        {
            return String.Format(CultureInfo.InvariantCulture, "GetName({0})", key);
        }

        public string GetAddress(int key)
        {
            return String.Format(CultureInfo.InvariantCulture, "GetAddress({0})", key);
        }

        public string GetCompanyFromVIP(int key)
        {
            return String.Format(CultureInfo.InvariantCulture, "GetCompanyFromVIP({0})", key);
        }

        [AcceptVerbs("POST", "PUT")]
        public string CreateRef(int key, string navigationProperty)
        {
            return String.Format(CultureInfo.InvariantCulture, "CreateRef({0})({1})", key, navigationProperty);
        }

        public string DeleteRef(int key, string navigationProperty)
        {
            return String.Format(CultureInfo.InvariantCulture, "DeleteRef({0})({1})", key, navigationProperty);
        }

        public string DeleteRef(int key, int relatedKey, string navigationProperty)
        {
            return String.Format(CultureInfo.InvariantCulture, "DeleteRef({0})({1})({2})", key, relatedKey, navigationProperty);
        }

        [AcceptVerbs("POST")]
        public string GetRelatedRoutingCustomers(int key)
        {
            return String.Format(CultureInfo.InvariantCulture, "GetRelatedRoutingCustomers({0})", key);
        }

        [AcceptVerbs("POST")]
        public string GetSalesPersonOnVIP(int key)
        {
            return String.Format(CultureInfo.InvariantCulture, "GetSalesPersonOnVIP({0})", key);
        }

        [AcceptVerbs("POST")]
        public string GetProducts()
        {
            return "GetProducts";
        }

        [AcceptVerbs("POST")]
        public string GetMostProfitableOnCollectionOfVIP()
        {
            return "GetMostProfitableOnCollectionOfVIP";
        }

        [AcceptVerbs("GET", "POST", "PUT", "PATCH", "DELETE", "CUSTOM")]
        public string HandleUnmappedRequest(ODataPath path)
        {
            return path.PathTemplate;
        }

        public string GetOrdersCount(int key)
        {
            return "GetOrdersCount_" + key;
        }

        public string GetOrdersCountOnVIP(int key)
        {
            return "GetOrdersCountOnVIP_" + key;
        }

        public string GetOrdersCount(int key, int factor)
        {
            return "GetOrdersCount_(" + key + "," + factor + ")";
        }

        // Writing this function here is used as guard. In the model, we doesn't define GetOrdersCount(int factor)
        // function on VIP entity type. So, the following two requests:
        // ~/RoutingCustomers(7)/System.Web.OData.Routing.VIP/GetOrdersCount(factor=2)
        // ~/RoutingCustomers(9)/System.Web.OData.Routing.SpecialVIP/GetOrdersCount(factor=5)
        // will never be routed into this function. Otherwise, it routes to the above function as
        // public string GetOrdersCount(int key, int factor)
        public string GetOrdersCountOnVIP(int key, int factor)
        {
            return "GetOrdersCountOnVIP_(" + key + "," + factor + ")";
        }

        public string GetSpecialGuid(int key)
        {
            return "GetSpecialGuid_" + key;
        }

        public string GetAllEmployees()
        {
            return "GetAllEmployees";
        }

        public string GetAllEmployeesOnCollectionOfVIP()
        {
            return "GetAllEmployeesOnCollectionOfVIP";
        }

        [HttpGet]
        public string BoundFuncWithEnumParametersOnCollectionOfRoutingCustomer(SimpleEnum simpleEnum, FlagsEnum flagsEnum)
        {
            return "BoundFuncWithEnumParameters(" + simpleEnum + "," + flagsEnum + ")";
        }

        [HttpGet]
        [ODataRoute("RoutingCustomers/Default.BoundFuncWithEnumParameterForAttributeRouting(SimpleEnum={p1})")]
        public string BoundFuncWithEnumParameter(SimpleEnum p1)
        {
            return "BoundFuncWithEnumParameterForAttributeRouting(" + p1 + ")";
        }

        [HttpGet]
        [ODataRoute("UnboundFuncWithEnumParameters(LongEnum={p1},FlagsEnum={p2})")]
        public string UnoundFuncWithEnumParameter(LongEnum p1, FlagsEnum p2)
        {
            return "UnboundFuncWithEnumParameters(" + p1 + "," + p2 + ")";
        }
    }

    public class ProductsController : ODataController
    {
        public string Get()
        {
            return "Get";
        }

        public string Post()
        {
            return "Post";
        }

        public string Get(int key)
        {
            return String.Format(CultureInfo.InvariantCulture, "Get({0})", key);
        }

        public string Put(int key)
        {
            return String.Format(CultureInfo.InvariantCulture, "Put({0})", key);
        }

        [AcceptVerbs("PATCH", "MERGE")]
        public string Patch(int key)
        {
            return String.Format(CultureInfo.InvariantCulture, "Patch({0})", key);
        }

        public string Delete(int key)
        {
            return String.Format(CultureInfo.InvariantCulture, "Delete({0})", key);
        }

        [AcceptVerbs("GET")]
        public string TopProductId(int key)
        {
            return String.Format(CultureInfo.InvariantCulture, "TopProductId({0})", key);
        }

        [AcceptVerbs("GET")]
        public string TopProductIdByCity(int key, string city)
        {
            return String.Format(CultureInfo.InvariantCulture, "TopProductIdByCity({0}, {1})", key, city);
        }

        [AcceptVerbs("GET")]
        public string TopProductIdByCityAndModel(int key, string city, int model)
        {
            return String.Format(CultureInfo.InvariantCulture, "TopProductIdByCityAndModel({0}, {1}, {2})", key, city, model);
        }

        [AcceptVerbs("GET")]
        public string TopProductOfAll()
        {
            return String.Format(CultureInfo.InvariantCulture, "TopProductOfAll");
        }

        [AcceptVerbs("GET")]
        public string TopProductOfAllByCity(string city)
        {
            return String.Format(CultureInfo.InvariantCulture, "TopProductOfAllByCity({0})", city);
        }

        [AcceptVerbs("GET")]
        public string TopProductOfAllByCityAndModel(string city, int model)
        {
            return String.Format(CultureInfo.InvariantCulture, "TopProductOfAllByCityAndModel({0}, {1})", city, model);
        }
    }

    public class EnumCustomersController : ODataController
    {
        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(new[]
            {
                new ODataRoutingModel.EnumCustomer
                {
                    ID = 1,
                    Color = Color.Green
                },
                new ODataRoutingModel.EnumCustomer
                {
                    ID = 2,
                    Color = Color.Red | Color.Blue
                }
            });
        }
    }
}
