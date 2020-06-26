#if !NETCOREAPP2_0
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.AspNet.OData.Test.Routing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Authorization
{
    public class ODataAuthorizationTest
    {
        private readonly HttpClient _client;

        public ODataAuthorizationTest()
        {
            var model = ODataRoutingModel.GetModel() as EdmModel;
            AddPermissions(model as EdmModel);

            var controllers = new[]
            {
                typeof(ProductsController),
                typeof(MyProductController),
                typeof(RoutingCustomersController),
                typeof(VipCustomerController),
                typeof(SalesPeopleController)
            };

            var server = TestServerFactory.CreateWithEndpointRouting(controllers, endpoints =>
            {
                endpoints.MapODataRoute("odata", "odata", model);
            }, services =>
            {
                services.AddAuthorization();
                services.AddODataAuthorization((context) =>
                {
                    var perm = context.User?.FindFirst("Permission")?.Value;
                    if (perm == null)
                    {
                        return Task.FromResult(Enumerable.Empty<string>());
                    }

                    return Task.FromResult(new string[] { perm }.AsEnumerable());
                });
                services.AddRouting();
                services.AddAuthentication("AuthScheme").AddScheme<CustomAuthOptions, CustomAuthHandler>("AuthScheme", options => { });
            }, app =>
            {
                app.UseODataAuthorization();
            });

            _client = TestServerFactory.CreateClient(server);
        }

        void AddPermissions(EdmModel model)
        {
            var readRestrictions = "Org.OData.Capabilities.V1.ReadRestrictions";
            var insertRestrictions = "Org.OData.Capabilities.V1.InsertRestrictions";
            var updateRestrictions = "Org.OData.Capabilities.V1.UpdateRestrictions";
            var deleteRestrictions = "Org.OData.Capabilities.V1.DeleteRestrictions";
            var operationRestrictions = "Org.OData.Capabilities.V1.OperationRestrictions";

            var product = model.FindDeclaredType("Microsoft.AspNet.OData.Test.Routing.Product") as IEdmEntityType;
            var products = model.FindDeclaredEntitySet("Products");
            var myProduct = model.FindDeclaredSingleton("MyProduct");
            var customers = model.FindDeclaredEntitySet("RoutingCustomers");
            var vipCustomer = model.FindDeclaredSingleton("VipCustomer");
            var salesPeople = model.FindDeclaredEntitySet("SalesPeople");
            var productFunction = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "FunctionBoundToProduct");
            var topProduct = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "TopProductOfAll");
            var getProducts = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "GetProducts");
            var getFavoriteProduct = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "GetFavoriteProduct");
            var getSalesPerson = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "GetSalesPerson");
            var getSalesPeople = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "GetSalesPeople");
            var getVIPRoutingCustomers = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "GetVIPRoutingCustomers");
            var getRoutingCustomerById = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "GetRoutingCustomerById");
            var unboundFunction = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "UnboundFunction");

            model.AddVocabularyAnnotation(new EdmVocabularyAnnotation(
                products,
                model.FindTerm(readRestrictions),
                new EdmRecordExpression(
                    CreatePermissionProperty(new string[] { "Product.Read", "Product.ReadAll" }),
                    new EdmPropertyConstructor("ReadByKeyRestrictions", CreatePermission(new[] { "Product.ReadByKey" })))));


            AddPermissionsTo(model, products, insertRestrictions, "Product.Insert");
            AddPermissionsTo(model, products, deleteRestrictions, "Product.Delete");
            AddPermissionsTo(model, products, updateRestrictions, "Product.Update");

            AddPermissionsTo(model, myProduct, readRestrictions, "MyProduct.Read");
            AddPermissionsTo(model, myProduct, deleteRestrictions, "MyProduct.Delete");
            AddPermissionsTo(model, myProduct, updateRestrictions, "MyProduct.Update");

            AddPermissionsTo(model, productFunction, operationRestrictions, "Product.Function");
            AddPermissionsTo(model, topProduct, operationRestrictions, "Product.Top");
            AddPermissionsTo(model, getRoutingCustomerById, operationRestrictions, "GetRoutingCustomerById");
            AddPermissionsTo(model, unboundFunction, operationRestrictions, "UnboundFunction");

            model.AddVocabularyAnnotation(new EdmVocabularyAnnotation(
                customers,
                model.FindTerm(readRestrictions),
                new EdmRecordExpression(
                    CreatePermissionProperty(new string[] { "Customer.Read", "Customer.ReadAll" }),
                    new EdmPropertyConstructor("ReadByKeyRestrictions", CreatePermission(new[] { "Customer.ReadByKey" })))));

            AddPermissionsTo(model, customers, insertRestrictions, "Customer.Insert");

            AddPermissionsTo(model, vipCustomer, readRestrictions, "VipCustomer.Read");

            model.AddVocabularyAnnotation(new EdmVocabularyAnnotation(
                salesPeople,
                model.FindTerm(readRestrictions),
                new EdmRecordExpression(
                    CreatePermissionProperty(new string[] { "SalesPerson.Read", "SalesPerson.ReadAll" }),
                    new EdmPropertyConstructor("ReadByKeyRestrictions", CreatePermission(new[] { "SalesPerson.ReadByKey" })))));

            AddPermissionsTo(model, getProducts, operationRestrictions, "Customer.GetProducts");
            AddPermissionsTo(model, getFavoriteProduct, operationRestrictions, "Customer.GetFavoriteProduct");
            AddPermissionsTo(model, getSalesPerson, operationRestrictions, "Customer.GetSalesPerson");
            AddPermissionsTo(model, getSalesPeople, operationRestrictions, "Customer.GetSalesPeople");
            AddPermissionsTo(model, getVIPRoutingCustomers, operationRestrictions, "SalesPerson.GetVip");
        }

        void AddPermissionsTo(EdmModel model, IEdmVocabularyAnnotatable target, string restrictionName, params string[] scopes)
        {
            model.AddVocabularyAnnotation(new EdmVocabularyAnnotation(
                target,
                model.FindTerm(restrictionName),
                CreatePermission(scopes)));
        }

        [Theory]
        // GET /entityset
        [InlineData("GET", "Products", "Product.Read", "GET Products")]
        [InlineData("GET", "Products", "Product.ReadAll", "GET Products")]
        [InlineData("GET", "Products/$count", "Product.Read", "GET Products")]
        [InlineData("GET", "Products/Microsoft.AspNet.OData.Test.Routing.SpecialProduct", "Product.Read", "GET SpecialProducts")]
        [InlineData("GET", "Products/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/$count", "Product.Read", "GET SpecialProducts")]
        // POST /entityset
        [InlineData("POST", "Products", "Product.Insert", "POST Products")]
        [InlineData("POST", "Products/Microsoft.AspNet.OData.Test.Routing.SpecialProduct", "Product.Insert", "POST SpecialProduct")]
        // GET /entityset/key
        [InlineData("GET", "Products(10)", "Product.ReadByKey", "GET Products(10)")]
        [InlineData("GET", "Products(10)/Microsoft.AspNet.OData.Test.Routing.SpecialProduct", "Product.ReadByKey", "GET SpecialProduct(10)")]
        // DELETE /entityset/key
        [InlineData("DELETE", "Products(10)", "Product.Delete", "DELETE Products(10)")]
        [InlineData("DELETE", "Products(10)/Microsoft.AspNet.OData.Test.Routing.SpecialProduct", "Product.Delete", "DELETE SpecialProduct(10)")]
        // PUT /entityset/key
        [InlineData("PUT", "Products(10)", "Product.Update", "PUT Products(10)")]
        [InlineData("PUT", "Products(10)/Microsoft.AspNet.OData.Test.Routing.SpecialProduct", "Product.Update", "PUT SpecialProduct(10)")]
        // PATCH /entityset/key
        [InlineData("PATCH", "Products(10)", "Product.Update", "PATCH Products(10)")]
        [InlineData("PATCH", "Products(10)/Microsoft.AspNet.OData.Test.Routing.SpecialProduct", "Product.Update", "PATCH SpecialProduct(10)")]
        [InlineData("MERGE", "Products(10)", "Product.Update", "PATCH Products(10)")]
        [InlineData("MERGE", "Products(10)/Microsoft.AspNet.OData.Test.Routing.SpecialProduct", "Product.Update", "PATCH SpecialProduct(10)")]
        // /singleton and /singleton/cast
        [InlineData("GET", "MyProduct", "MyProduct.Read", "GET MyProduct")]
        [InlineData("GET", "MyProduct/Microsoft.AspNet.OData.Test.Routing.SpecialProduct", "MyProduct.Read", "GET MySpecialProduct")]
        [InlineData("PUT", "MyProduct", "MyProduct.Update", "PUT MyProduct")]
        [InlineData("PUT", "MyProduct/Microsoft.AspNet.OData.Test.Routing.SpecialProduct", "MyProduct.Update", "PUT MySpecialProduct")]
        [InlineData("PATCH", "MyProduct", "MyProduct.Update", "PATCH MyProduct")]
        [InlineData("PATCH", "MyProduct/Microsoft.AspNet.OData.Test.Routing.SpecialProduct", "MyProduct.Update", "PATCH MySpecialProduct")]
        [InlineData("MERGE", "MyProduct", "MyProduct.Update", "PATCH MyProduct")]
        [InlineData("MERGE", "MyProduct/Microsoft.AspNet.OData.Test.Routing.SpecialProduct", "MyProduct.Update", "PATCH MySpecialProduct")]
        // bound functions
        // TODO should support different function overloads with different permissions
        // TODO create test with functions bound to derived types
        // TODO figure out why /$count returns a 404
        [InlineData("GET", "Products(10)/FunctionBoundToProduct", "Product.Function", "FunctionBoundToProduct(10)")]
        [InlineData("GET", "Products(10)/FunctionBoundToProduct(P1=1)", "Product.Function", "FunctionBoundToProduct(10, 1)")]
        [InlineData("GET", "Products(10)/FunctionBoundToProduct(P1=1, P2=2, P3='3')", "Product.Function", "FunctionBoundToProduct(10, 1, 2, 3)")]
        //[InlineData("GET", "Products(10)/FunctionBoundToProduct/$count", "Product.Function", "FunctionBoundToProduct(10)")]
        [InlineData("GET", "Products(10)/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/FunctionBoundToProduct", "Product.Function", "FunctionBoundToProduct(10)")]
        //[InlineData("GET", "Products(10)/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/FunctionBoundToProduct/$count", "Product.Function", "FunctionBoundToProduct(10)")]
        // entityset functions
        [InlineData("GET", "Products/TopProductOfAll", "Product.Top", "TopProductOfAll()")]
        //[InlineData("GET", "Products/TopProductOfAll/$count", "Product.Top", "TopProductOfAll()")]
        [InlineData("GET", "Products/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/TopProductOfAll", "Product.Top", "TopProductOfAll()")]
        //[InlineData("GET", "Products/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/TopProductOfAll/$count", "Product.Top", "TopProductOfAll()")]
        // singleton functions
        [InlineData("GET", "MyProduct/FunctionBoundToProduct", "Product.Function", "FunctionBoundToProduct()")]
        //[InlineData("GET", "MyProduct/FunctionBoundToProduct/$count", "Product.Function", "FunctionBoundToProduct()")]
        [InlineData("GET", "MyProduct/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/FunctionBoundToProduct", "Product.Function", "FunctionBoundToProduct()")]
        //[InlineData("GET", "MyProduct/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/FunctionBoundtoProduct/$count", "Product.Function", "FunctionBoundToProduct()")]
        // entity actions
        [InlineData("POST", "SalesPeople(10)/GetVIPRoutingCustomers", "SalesPerson.GetVip", "GetVIPRoutingCustomers(10)")]
        [InlineData("POST", "SalesPeople/GetVIPRoutingCustomers", "SalesPerson.GetVip", "GetVIPRoutingCustomers()")]
        [InlineData("POST", "RoutingCustomers(10)/Microsoft.AspNet.OData.Test.Routing.VIP/GetSalesPerson", "Customer.GetSalesPerson", "GetSalesPersonOnVIP(10)")]
        // entityset actions
        [InlineData("POST", "RoutingCustomers/GetProducts", "Customer.GetProducts", "GetProducts()")]
        [InlineData("POST", "RoutingCustomers/Microsoft.AspNet.OData.Test.Routing.VIP/GetSalesPeople", "Customer.GetSalesPeople", "GetSalesPeopleOnVIP()")]
        // singleton actions
        [InlineData("POST", "VipCustomer/Microsoft.AspNet.OData.Test.Routing.VIP/GetSalesPerson", "Customer.GetSalesPerson", "GetSalesPerson()")]
        [InlineData("POST", "VipCustomer/GetFavoriteProduct", "Customer.GetFavoriteProduct", "GetFavoriteProduct()")]
        // entityset/key/property
        [InlineData("GET", "Products(10)/Name", "Product.ReadByKey", "GetProductName(10)")]
        [InlineData("GET", "Products(10)/Name/$value", "Product.ReadByKey", "GetProductName(10)")]
        [InlineData("GET", "Products(10)/Tags/$count", "Product.ReadByKey", "GetProductTags(10)")]
        [InlineData("DELETE", "Products(10)/Name", "Product.Update", "DeleteProductName(10)")]
        [InlineData("PATCH", "Products(10)/Name", "Product.Update", "PatchProductName(10)")]
        [InlineData("PUT", "Products(10)/Name", "Product.Update", "PutProductName(10)")]
        [InlineData("POST", "Products(10)/Tags", "Product.Update", "PostProductTags(10)")]
        // entityset/key/cast/property
        [InlineData("GET", "Products(10)/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/Name", "Product.ReadByKey", "GetProductName(10)")]
        [InlineData("GET", "Products(10)/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/Name/$value", "Product.ReadByKey", "GetProductName(10)")]
        [InlineData("GET", "Products(10)/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/Tags/$count", "Product.ReadByKey", "GetProductTags(10)")]
        [InlineData("DELETE", "Products(10)/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/Name", "Product.Update", "DeleteProductName(10)")]
        [InlineData("PATCH", "Products(10)/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/Name", "Product.Update", "PatchProductName(10)")]
        [InlineData("PUT", "Products(10)/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/Name", "Product.Update", "PutProductName(10)")]
        [InlineData("POST", "Products(10)/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/Tags", "Product.Update", "PostProductTags(10)")]
        // singleton/property
        [InlineData("GET", "MyProduct/Name", "MyProduct.Read", "GetMyProductName")]
        [InlineData("GET", "MyProduct/Name/$value", "MyProduct.Read", "GetMyProductName")]
        [InlineData("GET", "MyProduct/Tags/$count", "MyProduct.Read", "GetMyProductTags")]
        [InlineData("DELETE", "MyProduct/Name", "MyProduct.Update", "DeleteMyProductName")]
        [InlineData("PATCH", "MyProduct/Name", "MyProduct.Update", "PatchMyProductName")]
        [InlineData("PUT", "MyProduct/Name", "MyProduct.Update", "PutMyProductName")]
        [InlineData("POST", "MyProduct/Tags", "MyProduct.Update", "PostMyProductTags")]
        // singleton/cast/property
        [InlineData("GET", "MyProduct/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/Name", "MyProduct.Read", "GetMyProductName")]
        [InlineData("GET", "MyProduct/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/Name/$value", "MyProduct.Read", "GetMyProductName")]
        [InlineData("GET", "MyProduct/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/Tags/$count", "MyProduct.Read", "GetMyProductTags")]
        [InlineData("DELETE", "MyProduct/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/Name", "MyProduct.Update", "DeleteMyProductName")]
        [InlineData("PATCH", "MyProduct/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/Name", "MyProduct.Update", "PatchMyProductName")]
        [InlineData("PUT", "MyProduct/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/Name", "MyProduct.Update", "PutMyProductName")]
        [InlineData("POST", "MyProduct/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/Tags", "MyProduct.Update", "PostMyProductTags")]
        // dynamic properties
        [InlineData("GET", "SalesPeople(10)/SomeProperty", "SalesPerson.ReadByKey", "GetSalesPersonDynamicProperty(10, SomeProperty)")]
        // navigation properties
        [InlineData("GET", "Products(10)/RoutingCustomers", "Customer.Read", "GetProductCustomers(10)")]
        [InlineData("POST", "MyProduct/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/RoutingCustomers", "Customer.Insert", "PostMyProductCustomer")]
        // $ref
        [InlineData("GET", "Products(10)/RoutingCustomers(20)/$ref","Product.ReadByKey", "GetProductCustomerRef(10, 20)")]
        [InlineData("POST", "MyProduct/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/RoutingCustomers/$ref", "MyProduct.Update", "CreateMyProductCustomerRef")]
        [InlineData("DELETE", "Products(10)/Microsoft.AspNet.OData.Test.Routing.SpecialProduct/RoutingCustomers(20)/$ref", "Product.Update", "DeleteProductCustomerRef(10, 20)")]
        [InlineData("DELETE", "MyProduct/RoutingCustomers(20)/$ref", "MyProduct.Update", "DeleteMyProductCustomerRef(20)")]
        [InlineData("PUT", "Products(10)/RoutingCustomers/$ref", "Product.Update", "CreateProductCustomerRef(10)")]
        // unbound action
        [InlineData("POST", "GetRoutingCustomerById", "GetRoutingCustomerById", "GetRoutingCustomerById")]
        // unbound function
        [InlineData("GET", "UnboundFunction", "UnboundFunction", "UnboundFunction")]
        // complex routes requiring ODataRoute attribute
        [InlineData("GET", "Products(10)/RoutingCustomers(20)/Address/Street", "Customer.ReadByKey", "GetProductRoutingCustomerAddressStreet")]
        public async void RestrictsPermissions(string method, string endpoint, string permission, string expectedResponse)
        {
            var uri = $"http://localhost/odata/{endpoint}";
            // permission forbidden if auth not provided
            HttpResponseMessage response = await _client.SendAsync(new HttpRequestMessage(
                new HttpMethod(method), uri));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            // request succeeds if permission is correct
            var message = new HttpRequestMessage(new HttpMethod(method), uri);
            message.Headers.Add("Scope", permission);

            response = await _client.SendAsync(message);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedResponse, response.Content.AsObjectContentValue());
        }

        static IEdmExpression CreatePermission(params string[] scopeNames)
        {
            var restriction = new EdmRecordExpression(
                CreatePermissionProperty(scopeNames));
            
            return restriction;
        }

        static IEdmPropertyConstructor CreatePermissionProperty(params string[] scopeNames)
        {
            var scopes = scopeNames.Select(scope => new EdmRecordExpression(
                   new EdmPropertyConstructor("Scope", new EdmStringConstant(scope)),
                   new EdmPropertyConstructor("RestrictedProperties", new EdmStringConstant("*"))));

            var permission = new EdmRecordExpression(
                new EdmPropertyConstructor("SchemeName", new EdmStringConstant("AuthScheme")),
                new EdmPropertyConstructor("Scopes", new EdmCollectionExpression(scopes)));

            var property = new EdmPropertyConstructor("Permissions", new EdmCollectionExpression(permission));
            return property;
        }
    }

    internal class CustomAuthHandler : AuthenticationHandler<CustomAuthOptions>
    {
        public CustomAuthHandler(IOptionsMonitor<CustomAuthOptions> options, ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new System.Security.Principal.GenericIdentity("Me");
            var scopeValues = Request.Headers["Scope"];
            if (scopeValues.Count != 0)
            {
                var scope = scopeValues.ToArray()[0];
                identity.AddClaim(new System.Security.Claims.Claim("Permission", scope));
            }

            var principal = new System.Security.Principal.GenericPrincipal(identity, Array.Empty<string>());
            var ticket = new AuthenticationTicket(principal, "AuthScheme");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    internal class CustomAuthOptions : AuthenticationSchemeOptions
    {
    }

    public class ProductsController: TestODataController
    {
        public string Get()
        {
            return "GET Products";
        }

        public string GetProductsFromSpecialProduct()
        {
            return "GET SpecialProducts";
        }

        public string Post()
        {
            return "POST Products";
        }

        public string PostFromSpecialProduct()
        {
            return "POST SpecialProduct";
        }

        public string Get(int key)
        {
            return $"GET Products({key})";
        }

        public string GetSpecialProduct(int key)
        {
            return $"GET SpecialProduct({key})";
        }

        public string Delete(int key)
        {
            return $"DELETE Products({key})";
        }

        public string DeleteSpecialProduct(int key)
        {
            return $"DELETE SpecialProduct({key})";
        }

        public string Put(int key)
        {
            return $"PUT Products({key})";
        }

        public string PutSpecialProduct(int key)
        {
            return $"PUT SpecialProduct({key})";
        }

        public string Patch(int key)
        {
            return $"PATCH Products({key})";
        }

        public string PatchSpecialProduct(int key)
        {
            return $"PATCH SpecialProduct({key})";
        }

        public string FunctionBoundToProduct(int key)
        {
            return $"FunctionBoundToProduct({key})";
        }

        public string FunctionBoundToProduct(int key, int P1)
        {
            return $"FunctionBoundToProduct({key}, {P1})";
        }

        public string FunctionBoundToProduct(int key, int P1, int P2, string P3)
        {
            return $"FunctionBoundToProduct({key}, {P1}, {P2}, {P3})";
        }

        public string FunctionBoundToProductOnSpecialProduct(int key)
        {
            return $"FunctionBoundToSpecialProduct({key})";
        }

        public string TopProductOfAll()
        {
            return "TopProductOfAll()";
        }

        public string GetName(int key)
        {
            return $"GetProductName({key})";
        }

        public string PutToName(int key)
        {
            return $"PutProductName({key})";
        }

        public string PatchToName(int key)
        {
            return $"PatchProductName({key})";
        }

        public string DeleteToName(int key)
        {
            return $"DeleteProductName({key})";
        }

        public string PostToTags(int key)
        {
            return $"PostProductTags({key})";
        }

        public string GetTags(int key)
        {
            return $"GetProductTags({key})";
        }

        public string GetRoutingCustomers(int key)
        {
            return $"GetProductCustomers({key})";
        }

        [ODataRoute("Products({key})/RoutingCustomers({relatedKey})/$ref")]
        public string GetRefToRoutingCustomers(int key, int relatedKey)
        {
            return $"GetProductCustomerRef({key}, {relatedKey})";
        }

        public string DeleteRefToRoutingCustomers(int key, int relatedKey)
        {
            return $"DeleteProductCustomerRef({key}, {relatedKey})";
        }

        public string CreateRefToRoutingCustomers(int key)
        {
            return $"CreateProductCustomerRef({key})";
        }

        [HttpGet]
        [ODataRoute("Products({key})/RoutingCustomers({relatedKey})/Address/Street")]
        public string GetProductRoutingCustomerAddressStreet(int key, int relatedKey)
        {
            return "GetProductRoutingCustomerAddressStreet";
        }
    }

    public class MyProductController: TestODataController
    {
        public string Get()
        {
            return "GET MyProduct";
        }

        public string GetFromSpecialProduct()
        {
            return "GET MySpecialProduct";
        }

        public string Put()
        {
            return "PUT MyProduct";
        }

        public string PutFromSpecialProduct()
        {
            return "PUT MySpecialProduct";
        }

        public string Patch()
        {
            return "PATCH MyProduct";
        }

        public string PatchFromSpecialProduct()
        {
            return "PATCH MySpecialProduct";
        }

        public string FunctionBoundToProduct()
        {
            return "FunctionBoundToProduct()";
        }

        public string GetName()
        {
            return "GetMyProductName";
        }

        public string PutToName()
        {
            return "PutMyProductName";
        }

        public string PatchToName()
        {
            return "PatchMyProductName";
        }

        public string DeleteToName()
        {
            return "DeleteMyProductName";
        }

        public string PostToTags()
        {
            return "PostMyProductTags";
        }

        public string GetTags()
        {
            return "GetMyProductTags";
        }

        public string PostToRoutingCustomers()
        {
            return "PostMyProductCustomer";
        }

        public string CreateRefToRoutingCustomers()
        {
            return $"CreateMyProductCustomerRef";
        }

        public string DeleteRefToRoutingCustomers(int relatedKey)
        {
            return $"DeleteMyProductCustomerRef({relatedKey})";
        }
    }

    public class RoutingCustomersController: TestODataController
    {
        public string GetProducts()
        {
            return "GetProducts()";
        }
        public string GetSalesPersonOnVIP(int key)
        {
            return $"GetSalesPersonOnVIP({key})";
        }

        public string GetSalesPeopleOnCollectionOfVIP()
        {
            return "GetSalesPeopleOnVIP()";
        }

        [HttpPost]
        [ODataRoute("GetRoutingCustomerById")]
        public string GetRoutingCustomerById()
        {
            return "GetRoutingCustomerById";
        }

        [HttpGet]
        [ODataRoute("UnboundFunction")]
        public string UnboundFunction()
        {
            return "UnboundFunction";
        }
    }

    public class VipCustomerController: TestODataController
    {
        public string GetSalesPerson()
        {
            return "GetSalesPerson()";
        }

        public string GetFavoriteProduct()
        {
            return "GetFavoriteProduct()";
        }

        public string GetName()
        {
            return "GetName()";
        }
    }

    public class SalesPeopleController: TestODataController
    {
        public string GetVIPRoutingCustomers(int key)
        {
            return $"GetVIPRoutingCustomers({key})";
        }

        public string GetVIPRoutingCustomers()
        {
            return "GetVIPRoutingCustomers()";
        }

        public string GetDynamicProperty(int key, string dynamicProperty)
        {
            return $"GetSalesPersonDynamicProperty({key}, {dynamicProperty})";
        }
    }
}
#endif