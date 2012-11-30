// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Routing
{
    public class ODataRoutingTest
    {
        HttpServer _server;
        HttpClient _client;

        public ODataRoutingTest()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.EnableOData(ODataRoutingModel.GetModel());

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
        // links
        [InlineData("PUT", "RoutingCustomers(1)/$links/Products", "CreateLink(1)(Products)")]
        [InlineData("POST", "RoutingCustomers(1)/$links/Products", "CreateLink(1)(Products)")]
        [InlineData("DELETE", "RoutingCustomers(1)/$links/Products", "DeleteLink(1)(Products)")]
        [InlineData("DELETE", "RoutingCustomers(1)/$links/Products(5)", "DeleteLink(1)(5)(Products)")]
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

    public class RoutingCustomersController : ApiController
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
            return String.Format("GetRoutingCustomer({0})", key);
        }

        public string PutRoutingCustomer(int key)
        {
            return String.Format("PutRoutingCustomer({0})", key);
        }

        [AcceptVerbs("PATCH", "MERGE")]
        public string PatchRoutingCustomer(int key)
        {
            return String.Format("PatchRoutingCustomer({0})", key);
        }

        public string DeleteRoutingCustomer(int key)
        {
            return String.Format("DeleteRoutingCustomer({0})", key);
        }

        public string GetProducts(int key)
        {
            return String.Format("GetProducts({0})", key);
        }

        public string GetRelationshipManagerFromVIP(int key)
        {
            return String.Format("GetRelationshipManagerFromVIP({0})", key);
        }

        [AcceptVerbs("POST", "PUT")]
        public string CreateLink(int key, string navigationProperty)
        {
            return String.Format("CreateLink({0})({1})", key, navigationProperty);
        }

        public string DeleteLink(int key, string navigationProperty)
        {
            return String.Format("DeleteLink({0})({1})", key, navigationProperty);
        }

        public string DeleteLink(int key, int relatedKey, string navigationProperty)
        {
            return String.Format("DeleteLink({0})({1})({2})", key, relatedKey, navigationProperty);
        }

        [AcceptVerbs("POST")]
        public string GetRelatedRoutingCustomers(int key)
        {
            return String.Format("GetRelatedRoutingCustomers({0})", key);
        }

        [AcceptVerbs("POST")]
        public string GetSalesPersonOnVIP(int key)
        {
            return String.Format("GetSalesPersonOnVIP({0})", key);
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

    public class ProductsController : ApiController
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
            return String.Format("Get({0})", key);
        }

        public string Put(int key)
        {
            return String.Format("Put({0})", key);
        }

        [AcceptVerbs("PATCH", "MERGE")]
        public string Patch(int key)
        {
            return String.Format("Patch({0})", key);
        }

        public string Delete(int key)
        {
            return String.Format("Delete({0})", key);
        }
    }
}