// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;
using System.Net.Http;
using System.Web.Http.OData.Extensions;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Routing
{
    public class ODataRoutingTest
    {
        private HttpServer _server;
        private HttpClient _client;

        public ODataRoutingTest()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Routes.MapODataServiceRoute(ODataRoutingModel.GetModel());

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
        [InlineData("GET", "RoutingCustomers(10)/System.Web.Http.OData.Routing.VIP/Products", "GetProducts(10)")]
        [InlineData("GET", "RoutingCustomers(10)/System.Web.Http.OData.Routing.VIP/RelationshipManager", "GetRelationshipManagerFromVIP(10)")]
        // structural properties
        [InlineData("GET", "RoutingCustomers(10)/Name", "GetName(10)")]
        [InlineData("GET", "RoutingCustomers(10)/Address", "GetAddress(10)")]
        [InlineData("GET", "RoutingCustomers(10)/System.Web.Http.OData.Routing.VIP/Name", "GetName(10)")]
        [InlineData("GET", "RoutingCustomers(10)/System.Web.Http.OData.Routing.VIP/Company", "GetCompanyFromVIP(10)")]
        // links
        [InlineData("PUT", "RoutingCustomers(1)/$links/Products", "CreateLink(1)(Products)")]
        [InlineData("POST", "RoutingCustomers(1)/$links/Products", "CreateLink(1)(Products)")]
        [InlineData("DELETE", "RoutingCustomers(1)/$links/Products", "DeleteLink(1)(Products)")]
        [InlineData("DELETE", "RoutingCustomers(1)/$links/Products(5)", "DeleteLink(1)(5)(Products)")]
        // raw value
        [InlineData("GET", "RoutingCustomers(10)/Name/$value", "GetName(10)")]
        [InlineData("GET", "RoutingCustomers(10)/System.Web.Http.OData.Routing.VIP/Name/$value", "GetName(10)")]
        [InlineData("GET", "RoutingCustomers(10)/System.Web.Http.OData.Routing.VIP/Company/$value", "GetCompanyFromVIP(10)")]
        // actions on entities by key
        [InlineData("POST", "RoutingCustomers(1)/GetRelatedRoutingCustomers", "GetRelatedRoutingCustomers(1)")]
        [InlineData("POST", "RoutingCustomers(1)/System.Web.Http.OData.Routing.VIP/GetRelatedRoutingCustomers", "GetRelatedRoutingCustomers(1)")]
        [InlineData("POST", "RoutingCustomers(1)/System.Web.Http.OData.Routing.VIP/GetSalesPerson", "GetSalesPersonOnVIP(1)")]
        // actions on entity sets
        [InlineData("POST", "RoutingCustomers/GetProducts", "GetProducts")]
        [InlineData("POST", "RoutingCustomers/System.Web.Http.OData.Routing.VIP/GetProducts", "GetProducts")]
        [InlineData("POST", "RoutingCustomers/System.Web.Http.OData.Routing.VIP/GetMostProfitable", "GetMostProfitableOnCollectionOfVIP")]
        // unmapped requests
        [InlineData("GET", "RoutingCustomers(10)/Products(1)", "~/entityset/key/navigation/key")]
        [InlineData("CUSTOM", "RoutingCustomers(10)", "~/entityset/key")]
        public void RoutesCorrectly(string httpMethod, string uri, string expectedResponse)
        {
            HttpResponseMessage response = _client.SendAsync(new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/" + uri)).Result;

            response.EnsureSuccessStatusCode();
            Assert.Equal(expectedResponse, (response.Content as ObjectContent<string>).Value);
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
        public string CreateLink(int key, string navigationProperty)
        {
            return String.Format(CultureInfo.InvariantCulture, "CreateLink({0})({1})", key, navigationProperty);
        }

        public string DeleteLink(int key, string navigationProperty)
        {
            return String.Format(CultureInfo.InvariantCulture, "DeleteLink({0})({1})", key, navigationProperty);
        }

        public string DeleteLink(int key, int relatedKey, string navigationProperty)
        {
            return String.Format(CultureInfo.InvariantCulture, "DeleteLink({0})({1})({2})", key, relatedKey, navigationProperty);
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
    }
}