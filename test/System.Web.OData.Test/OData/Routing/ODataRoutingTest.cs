// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Extensions;
using System.Web.OData.TestCommon;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;

namespace System.Web.OData.Routing
{
    public class ODataRoutingTest
    {
        private HttpServer _server;
        private HttpClient _client;

        public ODataRoutingTest()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.MapODataServiceRoute("RouteName", null, ODataRoutingModel.GetModel());

            var controllers = new[]
            {
                typeof(DateTimeOffsetKeyCustomersController),
                typeof(RoutingCustomersController),
                typeof(ProductsController)
            };

            TestAssemblyResolver resolver = new TestAssemblyResolver(new MockAssembly(controllers));
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            _server = new HttpServer(configuration);
            _client = new HttpClient(_server);
        }

        [Theory]
        // entity set defaults
        [InlineData("GET", "Products", "Get")]
        [InlineData("POST", "Products", "Post")]
        // entity set
        [InlineData("GET", "RoutingCustomers", "GetRoutingCustomers")]
        [InlineData("POST", "RoutingCustomers", "PostRoutingCustomer")]
        // entity set / cast
        [InlineData("GET", "RoutingCustomers/System.Web.OData.Routing.VIP", "GetRoutingCustomersFromVIP")]
        [InlineData("POST", "RoutingCustomers/System.Web.OData.Routing.VIP", "PostRoutingCustomerFromVIP")]
        // entity by key defaults
        [InlineData("GET", "Products(10)", "Get(10)")]
        [InlineData("PUT", "Products(10)", "Put(10)")]
        [InlineData("PATCH", "Products(10)", "Patch(10)")]
        [InlineData("MERGE", "Products(10)", "Patch(10)")]
        [InlineData("DELETE", "Products(10)", "Delete(10)")]
        // entity by key
        [InlineData("GET", "RoutingCustomers(10)", "GetRoutingCustomer(10)")]
        [InlineData("PUT", "RoutingCustomers(10)", "PutRoutingCustomer(10)")]
        [InlineData("PATCH", "RoutingCustomers(10)", "PatchRoutingCustomer(10)")]
        [InlineData("MERGE", "RoutingCustomers(10)", "PatchRoutingCustomer(10)")]
        [InlineData("DELETE", "RoutingCustomers(10)", "DeleteRoutingCustomer(10)")]
        // navigation properties
        [InlineData("GET", "RoutingCustomers(10)/Products", "GetProducts(10)")]
        [InlineData("GET", "RoutingCustomers(10)/System.Web.OData.Routing.VIP/Products", "GetProducts(10)")]
        [InlineData("GET", "RoutingCustomers(10)/System.Web.OData.Routing.VIP/RelationshipManager", "GetRelationshipManagerFromVIP(10)")]
        // structural properties
        [InlineData("GET", "RoutingCustomers(10)/Name", "GetName(10)")]
        [InlineData("GET", "RoutingCustomers(10)/Address", "GetAddress(10)")]
        [InlineData("GET", "RoutingCustomers(10)/System.Web.OData.Routing.VIP/Name", "GetName(10)")]
        [InlineData("GET", "RoutingCustomers(10)/System.Web.OData.Routing.VIP/Company", "GetCompanyFromVIP(10)")]
        // refs
        [InlineData("PUT", "RoutingCustomers(1)/Products/$ref", "CreateRef(1)(Products)")]
        [InlineData("POST", "RoutingCustomers(1)/Products/$ref", "CreateRef(1)(Products)")]
        [InlineData("DELETE", "RoutingCustomers(1)/Products/$ref", "DeleteRef(1)(Products)")]
        [InlineData("DELETE", "RoutingCustomers(1)/Products(5)/$ref", "DeleteRef(1)(5)(Products)")]
        [InlineData("DELETE", "RoutingCustomers(1)/Products/$ref?$id=http://localhost/Products(5)", "DeleteRef(1)(5)(Products)")]
        [InlineData("DELETE", "RoutingCustomers(1)/Products/$ref?$id=../../Products(5)", "DeleteRef(1)(5)(Products)")]
        // raw value
        [InlineData("GET", "RoutingCustomers(10)/Name/$value", "GetName(10)")]
        [InlineData("GET", "RoutingCustomers(10)/System.Web.OData.Routing.VIP/Name/$value", "GetName(10)")]
        [InlineData("GET", "RoutingCustomers(10)/System.Web.OData.Routing.VIP/Company/$value", "GetCompanyFromVIP(10)")]
        // actions on entities by key
        [InlineData("POST", "RoutingCustomers(1)/Default.GetRelatedRoutingCustomers", "GetRelatedRoutingCustomers(1)")]
        [InlineData("POST", "RoutingCustomers(1)/System.Web.OData.Routing.VIP/Default.GetRelatedRoutingCustomers", "GetRelatedRoutingCustomers(1)")]
        [InlineData("POST", "RoutingCustomers(1)/System.Web.OData.Routing.VIP/Default.GetSalesPerson", "GetSalesPersonOnVIP(1)")]
        // actions on entity sets
        [InlineData("POST", "RoutingCustomers/Default.GetProducts", "GetProducts")]
        [InlineData("POST", "RoutingCustomers/System.Web.OData.Routing.VIP/Default.GetProducts", "GetProducts")]
        [InlineData("POST", "RoutingCustomers/System.Web.OData.Routing.VIP/Default.GetMostProfitable", "GetMostProfitableOnCollectionOfVIP")]
        // functions on entities by key
        [InlineData("GET", "Products(1)/Default.TopProductId", "TopProductId(1)")]
        [InlineData("GET", "Products(1)/Default.TopProductIdByCity(city='any')", "TopProductIdByCity(1, any)")]
        [InlineData("GET", "Products(1)/Default.TopProductIdByCity(city=@city)?@city='any'", "TopProductIdByCity(1, any)")]
        [InlineData("GET", "Products(1)/Default.TopProductIdByCityAndModel(city='any',model=2)", "TopProductIdByCityAndModel(1, any, 2)")]
        [InlineData("GET", "Products(1)/Default.TopProductIdByCityAndModel(city=@city,model=@model)?@city='any'&@model=2", "TopProductIdByCityAndModel(1, any, 2)")]
        [InlineData("GET", "Products(1)/System.Web.OData.Routing.ImportantProduct/Default.TopProductId", "TopProductId(1)")]
        // functions on entity sets
        [InlineData("GET", "Products/Default.TopProductOfAll", "TopProductOfAll")]
        [InlineData("GET", "Products/Default.TopProductOfAllByCity(city='any')", "TopProductOfAllByCity(any)")]
        [InlineData("GET", "Products/Default.TopProductOfAllByCity(city=@city)?@city='any'", "TopProductOfAllByCity(any)")]
        [InlineData("GET", "Products/Default.TopProductOfAllByCityAndModel(city='any',model=2)", "TopProductOfAllByCityAndModel(any, 2)")]
        [InlineData("GET", "Products/Default.TopProductOfAllByCityAndModel(city=@city,model=@model)?@city='any'&@model=2", "TopProductOfAllByCityAndModel(any, 2)")]
        [InlineData("GET", "Products/System.Web.OData.Routing.ImportantProduct/Default.TopProductOfAllByCity(city='any')", "TopProductOfAllByCity(any)")]
        // functions bound to the base and derived type
        [InlineData("GET", "RoutingCustomers(4)/Default.GetOrdersCount()", "GetOrdersCount_4")]
        [InlineData("GET", "RoutingCustomers(5)/System.Web.OData.Routing.VIP/Default.GetOrdersCount()", "GetOrdersCountOnVIP_5")]
        [InlineData("GET", "RoutingCustomers(6)/System.Web.OData.Routing.SpecialVIP/Default.GetOrdersCount()", "GetOrdersCountOnVIP_6")]
        [InlineData("GET", "RoutingCustomers(7)/Default.GetOrdersCount(factor=3)", "GetOrdersCount_(7,3)")]
        [InlineData("GET", "RoutingCustomers(8)/System.Web.OData.Routing.VIP/Default.GetOrdersCount(factor=4)", "GetOrdersCount_(8,4)")]
        [InlineData("GET", "RoutingCustomers(9)/System.Web.OData.Routing.SpecialVIP/Default.GetOrdersCount(factor=5)", "GetOrdersCount_(9,5)")]
        // functions bound to the collection of the base and the derived type
        [InlineData("GET", "RoutingCustomers/Default.GetAllEmployees()", "GetAllEmployees")]
        [InlineData("GET", "RoutingCustomers/System.Web.OData.Routing.VIP/Default.GetAllEmployees()", "GetAllEmployeesOnCollectionOfVIP")]
        [InlineData("GET", "RoutingCustomers/System.Web.OData.Routing.SpecialVIP/Default.GetAllEmployees()", "GetAllEmployeesOnCollectionOfVIP")]
        // functions only bound to derived type
        [InlineData("GET", "RoutingCustomers(5)/Default.GetSpecialGuid()", "~/entityset/key/unresolved")]
        [InlineData("GET", "RoutingCustomers(5)/System.Web.OData.Routing.VIP/Default.GetSpecialGuid()", "~/entityset/key/cast/unresolved")]
        [InlineData("GET", "RoutingCustomers(5)/System.Web.OData.Routing.SpecialVIP/Default.GetSpecialGuid()", "GetSpecialGuid_5")]
        // functions with enum type parameter
        [InlineData("GET", "RoutingCustomers/Default.BoundFuncWithEnumParameters(SimpleEnum=Microsoft.TestCommon.Types.SimpleEnum'1'," +
            "FlagsEnum=Microsoft.TestCommon.Types.FlagsEnum'One, Four')", "BoundFuncWithEnumParameters(Second,One, Four)")]
        [InlineData("GET", "RoutingCustomers/Default.BoundFuncWithEnumParameterForAttributeRouting(SimpleEnum=Microsoft.TestCommon.Types.SimpleEnum'First')",
            "BoundFuncWithEnumParameterForAttributeRouting(First)")]
        [InlineData("GET", "UnboundFuncWithEnumParameters(LongEnum=Microsoft.TestCommon.Types.LongEnum'ThirdLong'," +
            "FlagsEnum=Microsoft.TestCommon.Types.FlagsEnum'7')", "UnboundFuncWithEnumParameters(ThirdLong,One, Two, Four)")]
        // unmapped requests
        [InlineData("GET", "RoutingCustomers(10)/Products(1)", "~/entityset/key/navigation/key")]
        [InlineData("CUSTOM", "RoutingCustomers(10)", "~/entityset/key")]
        // entity by key with type DateTimeOffset
        [InlineData("GET", "DateTimeOffsetKeyCustomers(2001-01-01T12:00:00.000+08:00)", "GetDateTimeOffsetKeyCustomer(01/01/2001 12:00:00 +08:00)")]
        public void RoutesCorrectly(string httpMethod, string uri, string expectedResponse)
        {
            // Arrange
            HttpResponseMessage response = _client.SendAsync(new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/" + uri)).Result;

            // Act & Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(expectedResponse, (response.Content as ObjectContent<string>).Value);
        }

        [Fact]
        public void RoutesCorrectly_WithParameterInRoutePrefix()
        {
            // Arrange
            _server.Configuration.MapODataServiceRoute("parameterInPrefix", "{a}", ODataRoutingModel.GetModel());

            // Act
            HttpResponseMessage response = _client.GetAsync("http://localhost/parameter/RoutingCustomers").Result;

            // Assert
            response.EnsureSuccessStatusCode();
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
}
