// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Types;
using Xunit;
#else
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Types;
using Microsoft.AspNet.OData.Test.Extensions;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Routing
{
    public class ODataRoutingTest
    {
        private readonly HttpClient _nullPrefixClient;
        private readonly HttpClient _fixedPrefixClient;
        private readonly HttpClient _parameterizedPrefixClient;

        public ODataRoutingTest()
        {
            var model = ODataRoutingModel.GetModel();

            var controllers = new[]
            {
                typeof(DateTimeOffsetKeyCustomersController),
                typeof(MetadataController),
                typeof(RoutingCustomersController),
                typeof(ProductsController),
                typeof(EnumCustomersController),
                typeof(DestinationsController),
                typeof(IncidentsController),
                typeof(NotFoundWithIdCustomersController),
                typeof(NotFoundCustomersController)
            };

            // Separate clients and servers so routes are not ambiguous.
            var nullPrefixServer = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("NullPrefixRoute", null, model);
            });

            _nullPrefixClient = TestServerFactory.CreateClient(nullPrefixServer);

            // FixedPrefixRoute has both a non-empty virtual path root and a fixed route prefix.
            var fixedPrefixServer = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("FixedPrefixRoute", "MyRoot/odata", model);
            });

            _fixedPrefixClient = TestServerFactory.CreateClient(fixedPrefixServer);

            var parameterizedPrefixServer = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("ParameterizedPrefixRoute", "{a}", model);
            });

            _parameterizedPrefixClient = TestServerFactory.CreateClient(parameterizedPrefixServer);
        }

        public static TheoryDataSet<string, string, string> ServiceAndMetadataRoutes
        {
            get
            {
                return new TheoryDataSet<string, string, string>
                {
                    // service document
                    { "GET", "", "" },
                    { "GET", "?hello=goodbye", "" },
                    { "GET", "?hello= good bye", "" },
                    { "GET", "?hello= good+bye", "" },
                    { "GET", "?hello = good%20bye", "" },
                    // metadata document
                    { "GET", "$metadata", "" },
                    { "GET", "$metadata?hello = good%20bye", "" },
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
                    { "GET", "RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP", "GetRoutingCustomersFromVIP" },
                    { "POST", "RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP", "PostRoutingCustomerFromVIP" },
                    // entity by key defaults
                    { "GET", "Products(10)", "Get(10)" },
                    { "PUT", "Products(10)", "Put(10)" },
                    { "PATCH", "Products(10)", "Patch(10)" },
                    { "MERGE", "Products(10)", "Patch(10)" },
                    { "DELETE", "Products(10)", "Delete(10)" },
                    // entity by key  defaults using keyID as param name
                    { "GET", "Incidents(10)", "Get(10) with keyID" },
                    { "PUT", "Incidents(10)", "Put(10) with keyID" },
                    { "PATCH", "Incidents(10)", "Patch(10) with keyID" },
                    { "MERGE", "Incidents(10)", "Patch(10) with keyID" },
                    { "DELETE", "Incidents(10)", "Delete(10) with keyID" },
                    // entity by key
                    { "GET", "RoutingCustomers(10)", "GetRoutingCustomer(10)" },
                    { "PUT", "RoutingCustomers(10)", "PutRoutingCustomer(10)" },
                    { "PATCH", "RoutingCustomers(10)", "PatchRoutingCustomer(10)" },
                    { "MERGE", "RoutingCustomers(10)", "PatchRoutingCustomer(10)" },
                    { "DELETE", "RoutingCustomers(10)", "DeleteRoutingCustomer(10)" },
                    // navigation properties
                    { "GET", "RoutingCustomers(10)/Products", "GetProducts(10)" },
                    { "GET", "RoutingCustomers(10)/Microsoft.AspNet.OData.Test.Routing.VIP/Products", "GetProducts(10)" },
                    { "GET",
                        "RoutingCustomers(10)/Microsoft.AspNet.OData.Test.Routing.VIP/RelationshipManager",
                        "GetRelationshipManagerFromVIP(10)" },
                    // structural properties
                    { "GET", "RoutingCustomers(10)/Name", "GetName(10)" },
                    { "GET", "RoutingCustomers(10)/Address", "GetAddress(10)" },
                    { "GET", "RoutingCustomers(10)/Microsoft.AspNet.OData.Test.Routing.VIP/Name", "GetName(10)" },
                    { "GET", "RoutingCustomers(10)/Microsoft.AspNet.OData.Test.Routing.VIP/Company", "GetCompanyFromVIP(10)" },
                    { "GET", "Incidents(10)/Name", "GetName(10) with keyID" },
                    // refs
                    { "PUT", "RoutingCustomers(1)/Products/$ref", "CreateRef(1)(Products)" },
                    { "POST", "RoutingCustomers(1)/Products/$ref", "CreateRef(1)(Products)" },
                    { "DELETE", "RoutingCustomers(1)/Products/$ref", "DeleteRef(1)(Products)" },
                    { "DELETE", "RoutingCustomers(1)/Products(5)/$ref", "DeleteRef(1)(5)(Products)" },
                    { "DELETE", "RoutingCustomers(1)/Products/$ref?$id=../../Products(5)", "DeleteRef(1)(5)(Products)" },
                    // raw value
                    { "GET", "RoutingCustomers(10)/Name/$value", "GetName(10)" },
                    { "GET", "RoutingCustomers(10)/Microsoft.AspNet.OData.Test.Routing.VIP/Name/$value", "GetName(10)" },
                    { "GET", "RoutingCustomers(10)/Microsoft.AspNet.OData.Test.Routing.VIP/Company/$value", "GetCompanyFromVIP(10)" },
                    // actions on entities by key
                    { "POST", "RoutingCustomers(1)/Default.GetRelatedRoutingCustomers", "GetRelatedRoutingCustomers(1)" },
                    { "POST",
                        "RoutingCustomers(1)/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetRelatedRoutingCustomers",
                        "GetRelatedRoutingCustomers(1)" },
                    { "POST",
                        "RoutingCustomers(1)/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetSalesPerson",
                        "GetSalesPersonOnVIP(1)" },
                    // actions on entity sets
                    { "POST", "RoutingCustomers/Default.GetProducts", "GetProducts" },
                    { "POST", "RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetProducts", "GetProducts" },
                    { "POST",
                        "RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetMostProfitable",
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
                        "Products(1)/Microsoft.AspNet.OData.Test.Routing.ImportantProduct/Default.TopProductId",
                        "TopProductId(1)" },
                    // functions on entity sets
                    { "GET", "Products/Default.TopProductOfAll", "TopProductOfAll" },
                    { "GET", "Products/Default.TopProductOfAllByCity(city='any')", "TopProductOfAllByCity(any)" },
                    { "GET", "Products/Default.TopProductOfAllByCity(city='some%23hashes')", "TopProductOfAllByCity(some#hashes)" },
#if NETFX // ASP.NET Core can't get the raw request Uri string now. See githu issue: https://github.com/aspnet/Mvc/issues/6892
                    { "GET", "Products/Default.TopProductOfAllByCity(city='some%2fslashes')", "TopProductOfAllByCity(some/slashes)" },
#endif
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
                        "Products/Microsoft.AspNet.OData.Test.Routing.ImportantProduct/Default.TopProductOfAllByCity(city='any')",
                        "TopProductOfAllByCity(any)" },
                    // functions bound to the base and derived type
                    { "GET", "RoutingCustomers(4)/Default.GetOrdersCount()", "GetOrdersCount_4" },
                    { "GET",
                        "RoutingCustomers(5)/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetOrdersCount()",
                        "GetOrdersCountOnVIP_5" },
                    { "GET",
                        "RoutingCustomers(6)/Microsoft.AspNet.OData.Test.Routing.SpecialVIP/Default.GetOrdersCount()",
                        "GetOrdersCountOnVIP_6" },
                    { "GET", "RoutingCustomers(7)/Default.GetOrdersCount(factor=3)", "GetOrdersCount_(7,3)" },
                    { "GET",
                        "RoutingCustomers(8)/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetOrdersCount(factor=4)",
                        "GetOrdersCount_(8,4)" },
                    { "GET",
                        "RoutingCustomers(9)/Microsoft.AspNet.OData.Test.Routing.SpecialVIP/Default.GetOrdersCount(factor=5)",
                        "GetOrdersCount_(9,5)" },
                    // functions bound to the collection of the base and the derived type
                    { "GET", "RoutingCustomers/Default.GetAllEmployees()", "GetAllEmployees" },
                    { "GET",
                        "RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetAllEmployees()",
                        "GetAllEmployeesOnCollectionOfVIP" },
                    { "GET",
                        "RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.SpecialVIP/Default.GetAllEmployees()",
                        "GetAllEmployeesOnCollectionOfVIP" },
                    // functions only bound to derived type
                    { "GET", "RoutingCustomers(5)/Default.GetSpecialGuid()", "~/entityset/key/unresolved" },
                    { "GET",
                        "RoutingCustomers(5)/Microsoft.AspNet.OData.Test.Routing.VIP/Default.GetSpecialGuid()",
                        "~/entityset/key/cast/unresolved" },
                    { "GET",
                        "RoutingCustomers(5)/Microsoft.AspNet.OData.Test.Routing.SpecialVIP/Default.GetSpecialGuid()",
                        "GetSpecialGuid_5" },
                    // functions with enum type parameter
                    { "GET",
                        "RoutingCustomers/Default.BoundFuncWithEnumParameters(" +
                            "SimpleEnum=Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum'1'," +
                            "FlagsEnum=Microsoft.AspNet.OData.Test.Common.Types.FlagsEnum'One, Four')",
                        "BoundFuncWithEnumParameters(Second,One, Four)" },
                    { "GET",
                        "RoutingCustomers/Default.BoundFuncWithEnumParameterForAttributeRouting(" +
                            "SimpleEnum=Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum'First')",
                        "BoundFuncWithEnumParameterForAttributeRouting(First)" },
                    { "GET",
                        "UnboundFuncWithEnumParameters(LongEnum=Microsoft.AspNet.OData.Test.Common.Types.LongEnum'ThirdLong'," +
                            "FlagsEnum=Microsoft.AspNet.OData.Test.Common.Types.FlagsEnum'7')",
                        "UnboundFuncWithEnumParameters(ThirdLong,One, Two, Four)" },
                    // The OData ABNF doesn't allow spaces within enum literals. But ODL _requires_ spaces after commas.
                    { "GET",
                        "UnboundFuncWithEnumParameters(LongEnum=Microsoft.AspNet.OData.Test.Common.Types.LongEnum'ThirdLong'," +
                            "FlagsEnum=Microsoft.AspNet.OData.Test.Common.Types.FlagsEnum'One, Two, Four')",
                        "UnboundFuncWithEnumParameters(ThirdLong,One, Two, Four)" },
                    { "GET",
                        "UnboundFuncWithEnumParameters(LongEnum=@long,FlagsEnum=@flags)?" +
                            "@long=Microsoft.AspNet.OData.Test.Common.Types.LongEnum'ThirdLong'&" +
                            "@flags=Microsoft.AspNet.OData.Test.Common.Types.FlagsEnum'7'",
                        "UnboundFuncWithEnumParameters(ThirdLong,One, Two, Four)" },
                    { "GET",
                        "UnboundFuncWithEnumParameters(LongEnum=@long,FlagsEnum=@flags)?" +
                            "@long=Microsoft.AspNet.OData.Test.Common.Types.LongEnum'ThirdLong'&" +
                            "@flags=Microsoft.AspNet.OData.Test.Common.Types.FlagsEnum'One, Two, Four'",
                        "UnboundFuncWithEnumParameters(ThirdLong,One, Two, Four)" },
                    // unmapped requests
                    { "GET", "RoutingCustomers(10)/Products(1)", "~/entityset/key/navigation/key" },
                    { "CUSTOM", "RoutingCustomers(10)", "~/entityset/key" },
                    // entity by key with type DateTimeOffset
                    { "GET",
                        "DateTimeOffsetKeyCustomers(2001-01-01T12:00:00.000+08:00)",
                        "GetDateTimeOffsetKeyCustomer(01/01/2001 12:00:00 +08:00)" },

                    // test string parameter for [FromODataUri]
                    { "GET", "Products(1)/Default.CopyProductByCity(city='123')", "CopyProductByCity(1, 123)" },
                    { "GET", "Products(1)/Default.CopyProductByCity(city='any')", "CopyProductByCity(1, any)" },
                    { "GET", "Products(1)/Default.CopyProductByCity(city='123any')", "CopyProductByCity(1, 123any)" },
                    { "GET", "Products(1)/Default.CopyProductByCity(city='any123')",  "CopyProductByCity(1, any123)" },

                    // test optional parameter routing (white space is intentional.)
                    { "GET", "Products/Default.GetCount(minSalary=1.1)",                               "GetCount(1.1, 0, 1200.99)" },
                    { "GET", "Products/Default.GetCount(minSalary=1.2, maxSalary=2.9)",                "GetCount(1.2, 2.9, 1200.99)" },
                    { "GET", "Products/Default.GetCount(minSalary=1.3, maxSalary=3.4, aveSalary=4.5)", "GetCount(1.3, 3.4, 4.5)" }
                };
            }
        }

        public static TheoryDataSet<string, string, string> MoreFunctionRouteData
        {
            get
            {
                return new TheoryDataSet<string, string, string>
                {
                    // Collection of primitive
                    {
                        "GET",
                        "RoutingCustomers(10)/Default.CollectionOfPrimitiveTypeFunction(intValues=[1,2,4,7,8])",
                        "CollectionOfPrimitiveTypeFunction([1,2,4,7,8])"
                    },

                    {
                        "GET",
                        "RoutingCustomers(10)/Default.CollectionOfPrimitiveTypeFunction(intValues=@p)?@p=[1,2,4,7,8]",
                        "CollectionOfPrimitiveTypeFunction([1,2,4,7,8])"
                    },

                    // Complex
                    {
                        "GET",
                        "RoutingCustomers(10)/Default.CanMoveToAddress(address=@p)?@p={\"@odata.type\":\"Microsoft.AspNet.OData.Test.Routing.Address\",\"Street\":\"NE 24th St.\",\"City\":\"Redmond\",\"ZipCode\":\"9001\"}",
                        "CanMoveToAddress(Street=NE 24th St.,City=Redmond,ZipCode=9001)"
                    },

                    // Collection of complex
                    {
                        "GET", "RoutingCustomers(10)/Default.MoveToAddresses(addresses=@p)?" +
                               "@p=[{\"@odata.type\":\"Microsoft.AspNet.OData.Test.Routing.Address\",\"Street\":\"NE 24th St.\",\"City\":\"Redmond\",\"ZipCode\":\"9001\"}," +
                               "{\"@odata.type\":\"Microsoft.AspNet.OData.Test.Routing.Address\",\"Street\":\"NE 24th St.\",\"City\":\"Redmond\",\"ZipCode\":\"9001\"}]",
                        "MoveToAddresses(addresses={2})"
                    },

                    // Entity
                    {
                        "GET",
                        "RoutingCustomers(10)/Default.EntityTypeFunction(product=@p)?@p={\"@odata.type\":\"Microsoft.AspNet.OData.Test.Routing.Product\",\"ID\":9,\"Name\":\"Phone\"}",
                        "EntityTypeFunction(ID=9,Name=Phone)"
                    },

                    // Feed (Collection of entity)
                    {
                        "GET", "RoutingCustomers(10)/Default.CollectionEntityTypeFunction(products=@p)" +
                               "?@p=[{\"@odata.type\":\"Microsoft.AspNet.OData.Test.Routing.Product\",\"ID\":9,\"Name\":\"Phone\"}," +
                               "{\"@odata.type\":\"Microsoft.AspNet.OData.Test.Routing.SpecialProduct\",\"ID\":9,\"Name\":\"Phone\",\"Value\":9}]",
                        "CollectionEntityTypeFunction(products={2})"
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(ControllerRoutes))]
        [InlineData("DELETE",
            "RoutingCustomers(1)/Products/$ref?$id=http://localhost/Products(5)",
            "DeleteRef(1)(5)(Products)")]
        [MemberData(nameof(MoreFunctionRouteData))]
        [InlineData("GET", "RoutingCustomers(11)/Default.EntityTypeFunction(product=@p)?@p={\"@odata.id\":\"http://localhost/Products(5)\"}",
            "EntityTypeFunctionWithEntityReference(ID=5,Name=--)")]
        [InlineData("GET", "RoutingCustomers(11)/Default.CollectionEntityTypeFunction(products=@p)" +
            "?@p=[{\"@odata.id\":\"http://localhost/Products(5)\"},{\"@odata.id\":\"http://localhost/Products(6)\"}]",
            "CollectionEntityTypeFunctionWithEntityReference(products=[{ID=5,Name=--},{ID=6,Name=--}])")]
        public async Task RoutesCorrectly(string httpMethod, string uri, string expectedResponse)
        {
            // Arrange & Act
            HttpResponseMessage response = await _nullPrefixClient.SendAsync(new HttpRequestMessage(
                new HttpMethod(httpMethod), "http://localhost/" + uri));

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            Assert.Equal(expectedResponse, response.Content.AsObjectContentValue());
        }

        [Theory]
        [InlineData("CollectionOfPrimitiveTypeFunction(intValues=@p)?@p=[1,2,4,7,8")] // missing "]"
        [InlineData("CanMoveToAddress(address=@p)?@p={\"@odata.")] // not valid complex payload
        [InlineData("EntityTypeFunction(product=@p)?@p={\"@odata.type\":\"Microsoft.AspNet.OData.Test.Routing.Product\",\"id\":9,\"Name\":\"Phone\"}")] // should be "ID"
        public async Task RoutesIncorrectly_ForBadFunctionParameters(string uri)
        {
            // Arrange & Act
            HttpResponseMessage response = await _nullPrefixClient.GetAsync("http://localhost/RoutingCustomers(10)/Default." + uri);

            // Assert
            Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(ServiceAndMetadataRoutes))]
        public async Task RoutesCorrectly_WithServiceAndMetadataRoutes(string httpMethod, string uri, string unused)
        {
            // Arrange & Act
            HttpResponseMessage response = await _nullPrefixClient.SendAsync(new HttpRequestMessage(
                new HttpMethod(httpMethod), "http://localhost/" + uri));

            // Assert
            Assert.NotNull(unused);
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
        }

        [Theory]
        [MemberData(nameof(ServiceAndMetadataRoutes))]
        [MemberData(nameof(ControllerRoutes))]
        [MemberData(nameof(MoreFunctionRouteData))]
        [InlineData("DELETE",
            "RoutingCustomers(1)/Products/$ref?$id=http://localhost/MyRoot/odata/Products(5)",
            "DeleteRef(1)(5)(Products)")]
        [InlineData("GET", "RoutingCustomers(11)/Default.EntityTypeFunction(product=@p)?@p={\"@odata.id\":\"http://localhost/MyRoot/odata/Products(5)\"}",
            "EntityTypeFunctionWithEntityReference(ID=5,Name=--)")]
        [InlineData("GET", "RoutingCustomers(11)/Default.CollectionEntityTypeFunction(products=@p)" +
            "?@p=[{\"@odata.id\":\"http://localhost/MyRoot/odata/Products(5)\"},{\"@odata.id\":\"http://localhost/MyRoot/odata/Products(6)\"}]",
            "CollectionEntityTypeFunctionWithEntityReference(products=[{ID=5,Name=--},{ID=6,Name=--}])")]
        public async Task RoutesCorrectly_WithFixedPrefix(string httpMethod, string uri, string unused)
        {
            // Arrange & Act
            HttpResponseMessage response = await _fixedPrefixClient.SendAsync(new HttpRequestMessage(
                new HttpMethod(httpMethod), "http://localhost/MyRoot/odata/" + uri));

            // Assert
            Assert.NotNull(unused);
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
        }

        [Theory]
        [InlineData("DELETE", "Destinations(1)/Parents/$ref?$id=http://localhost/MyRoot/odata/Destinations(5)")]
        public async Task RoutesCorrectly_DeleteRefWithDerivedNavigationProperty(string httpMethod, string uri)
        {
            // Arrange & Act
            HttpResponseMessage response = await _fixedPrefixClient.SendAsync(new HttpRequestMessage(
                new HttpMethod(httpMethod), "http://localhost/MyRoot/odata/" + uri));

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
        }

        [Theory]
        [MemberData(nameof(ServiceAndMetadataRoutes))]
        [MemberData(nameof(ControllerRoutes))]
        [MemberData(nameof(MoreFunctionRouteData))]
        [InlineData("DELETE",
            "RoutingCustomers(1)/Products/$ref?$id=http://localhost/parameter/Products(5)",
            "DeleteRef(1)(5)(Products)")]
        [InlineData("GET", "RoutingCustomers(11)/Default.EntityTypeFunction(product=@p)?@p={\"@odata.id\":\"http://localhost/parameter/Products(5)\"}",
            "EntityTypeFunctionWithEntityReference(ID=5,Name=--)")]
        [InlineData("GET", "RoutingCustomers(11)/Default.CollectionEntityTypeFunction(products=@p)" +
            "?@p=[{\"@odata.id\":\"http://localhost/parameter/Products(5)\"},{\"@odata.id\":\"http://localhost/parameter/Products(6)\"}]",
            "CollectionEntityTypeFunctionWithEntityReference(products=[{ID=5,Name=--},{ID=6,Name=--}])")]
        public async Task RoutesCorrectly_WithParameterizedPrefix(string httpMethod, string uri, string unused)
        {
            // Arrange & Act
            HttpResponseMessage response = await _parameterizedPrefixClient.SendAsync(new HttpRequestMessage(
                new HttpMethod(httpMethod), "http://localhost/parameter/" + uri));

            // Assert
            Assert.NotNull(unused);
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
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
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
        }

        [Theory]
        [InlineData("EnumCustomers?$filter=Microsoft.AspNet.OData.Test.Builder.TestModels.Color'Unknown' eq null", "The string 'Microsoft.AspNet.OData.Test.Builder.TestModels.Color'Unknown'' is not a valid enumeration type constant.")]
        [InlineData("EnumCustomers?$filter=geo.length(null) eq null", "Unknown function 'geo.length'.")]
        [InlineData("EnumCustomers?$filter=Default.OverloadUnboundFunction() eq null", "Unknown function 'Default.OverloadUnboundFunction'.")]
        public async Task BadQueryString_ReturnsBadRequest(string uri, string expectedError)
        {
            // Arrange & Act
            HttpResponseMessage response = await _nullPrefixClient.GetAsync("http://localhost/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            string responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedError, responseString);
        }

        [Theory]
        [InlineData("OnlyGetByIdRoutingCustomers", "GET")]
        [InlineData("OnlyGetRoutingCustomers(10)", "GET")]
        [InlineData("OnlyGetRoutingCustomers(10)", "DELETE")]
        [InlineData("OnlyGetRoutingCustomers(10)", "PUT")]
        [InlineData("OnlyGetRoutingCustomers(10)", "PATCH")]
        public async Task ActionsDontMatch_ReturnsNotFound(string uri, string httpMethod)
        {
            // Arrange & Act
            HttpResponseMessage response = await _nullPrefixClient.SendAsync(new HttpRequestMessage(
                new HttpMethod(httpMethod), "http://localhost/" + uri));

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    public class DateTimeOffsetKeyCustomersController : ODataController
    {
        public string GetDateTimeOffsetKeyCustomer(DateTimeOffset key)
        {
            return String.Format(CultureInfo.InvariantCulture, "GetDateTimeOffsetKeyCustomer({0})", key);
        }
    }

    public class RoutingCustomersController : TestODataController
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
        // ~/RoutingCustomers(7)/Microsoft.AspNet.OData.Test.Routing.VIP/GetOrdersCount(factor=2)
        // ~/RoutingCustomers(9)/Microsoft.AspNet.OData.Test.Routing.SpecialVIP/GetOrdersCount(factor=5)
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

        [HttpGet]
        public string CanMoveToAddress(int key, [FromODataUri] ODataRoutingModel.Address address)
        {
            return "CanMoveToAddress(Street=" + address.Street + ",City=" + address.City + ",ZipCode=" + address.ZipCode + ")";
        }

        [HttpGet]
        public string MoveToAddresses(int key, [FromODataUri] IEnumerable<ODataRoutingModel.Address> addresses)
        {
            return "MoveToAddresses(addresses={" + addresses.Count() + "})";
        }

        [HttpGet]
        public string CollectionOfPrimitiveTypeFunction(int key, [FromODataUri] IEnumerable<int> intValues)
        {
            return "CollectionOfPrimitiveTypeFunction([" + String.Join(",", intValues.Select(e => e)) + "])";
        }

        [HttpGet]
        public ITestActionResult EntityTypeFunction(int key, [FromODataUri] ODataRoutingModel.Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // entity call
            if (key == 10)
            {
                return Ok("EntityTypeFunction(ID=" + product.ID + ",Name=" + product.Name + ")");
            }
            else if (key == 11)
            {
                // entity reference call
                return Ok("EntityTypeFunctionWithEntityReference(ID=" + product.ID + ",Name=" + (product.Name ?? "--") + ")");
            }

            return Ok("BadRequest");
        }

        [HttpGet]
        public string CollectionEntityTypeFunction(int key, [FromODataUri] IEnumerable<ODataRoutingModel.Product> products)
        {
            // entity call
            if (key == 10)
            {
                return "CollectionEntityTypeFunction(products={" + products.Count() + "})";
            }
            else if (key == 11)
            {
                // entity reference call
                IList<string> msg = new List<string>();
                foreach (var p in products)
                {
                    msg.Add("{ID=" + p.ID + ",Name=" + (p.Name ?? "--") + "}");
                }
                return "CollectionEntityTypeFunctionWithEntityReference(products=[" + String.Join(",", msg) + "])";
            }

            return "BadRequest";
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
        public string GetCount(double minSalary, double maxSalary = 0.0, double aveSalary = 1200.99)
        {
            return String.Format(CultureInfo.InvariantCulture, "GetCount({0}, {1}, {2})", minSalary, maxSalary, aveSalary);
        }

#if NETCORE
        // Use this method to make sure it can't be picked up by OData action selector.
        // However, the multiple functions in same controller is not allowed in ASP.NET.
        [AcceptVerbs("GET")]
        public string GetCount(string minSalary)
        {
            return String.Format(CultureInfo.InvariantCulture, "GetCount({0})", minSalary);
        }
#endif

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
        public string CopyProductByCity(int key, [FromODataUri]string city)
        {
            return String.Format(CultureInfo.InvariantCulture, "CopyProductByCity({0}, {1})", key, city);
        }

        [AcceptVerbs("GET")]
        public string TopProductOfAllByCityAndModel(string city, int model)
        {
            return String.Format(CultureInfo.InvariantCulture, "TopProductOfAllByCityAndModel({0}, {1})", city, model);
        }
    }

    public class DestinationsController : ODataController
    {
        public string Get(int key)
        {
            return String.Format(CultureInfo.InvariantCulture, "Get({0})", key);
        }

        public string DeleteRef(int key, int relatedKey, string navigationProperty)
        {
            return String.Format(CultureInfo.InvariantCulture, "DeleteRef({0})({1})({2})", key, relatedKey, navigationProperty);
        }
    }

    public class EnumCustomersController : TestODataController
    {
        [EnableQuery]
        public ITestActionResult Get()
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

    public class IncidentsController : TestODataController
    {
        public string Get(int keyID)
        {
            return String.Format(CultureInfo.InvariantCulture, "Get({0}) with keyID", keyID);
        }

        public string Put(int keyID)
        {
            return String.Format(CultureInfo.InvariantCulture, "Put({0}) with keyID", keyID);
        }

        [AcceptVerbs("PATCH", "MERGE")]
        public string Patch(int keyID)
        {
            return String.Format(CultureInfo.InvariantCulture, "Patch({0}) with keyID", keyID);
        }

        public string Delete(int keyID)
        {
            return String.Format(CultureInfo.InvariantCulture, "Delete({0}) with keyID", keyID);
        }

        public string GetName(int keyID)
        {
            return String.Format(CultureInfo.InvariantCulture, "GetName({0}) with keyID", keyID);
        }
    }

    public class NotFoundWithIdCustomersController: TestODataController
    {
        public string Get(int key)
        {
            return String.Format(CultureInfo.InvariantCulture, "Get({0})", key);
        }
    }

    public class NotFoundCustomersController: TestODataController
    {
        public string Get()
        {
            return "Get()";
        }

        public string Put()
        {
            return "Put()";
        }

        public string Patch()
        {
            return "Patch()";
        }

        public string Post()
        {
            return "Post()";
        }

        public string Delete()
        {
            return "Delete()";
        }
    }
}
