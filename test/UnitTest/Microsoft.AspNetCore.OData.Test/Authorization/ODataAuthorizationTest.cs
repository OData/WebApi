#if !NETCOREAPP2_0
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.AspNet.OData.Test.Routing;
using Microsoft.AspNetCore.Authentication;
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
                services.AddODataAuthorization();
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

            var product = model.FindDeclaredType("Microsoft.AspNet.OData.Test.Routing.Product");
            var products = model.FindDeclaredEntitySet("Products");
            var myProduct = model.FindDeclaredSingleton("MyProduct");
            var productFunction = model.FindDeclaredBoundOperations("Default.FunctionBoundToProduct", product).First(f => f.Parameters.Count() == 1);
            var topProduct = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "TopProductOfAll");
            var getProducts = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "GetProducts");
            var getFavoriteProduct = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "GetFavoriteProduct");
            var getSalesPerson = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "GetSalesPerson");
            var getSalesPeople = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "GetSalesPeople");
            var getVIPRoutingCustomers = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "GetVIPRoutingCustomers");

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

        static IEdmExpression CreatePermission(string[] scopeNames)
        {
            var restriction = new EdmRecordExpression(
                CreatePermissionProperty(scopeNames));
            
            return restriction;
        }

        static IEdmPropertyConstructor CreatePermissionProperty(string[] scopeNames)
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
            var scopes = Request.Headers["Scope"];
            if (scopes.Count != 0)
            {
                identity.AddClaim(new System.Security.Claims.Claim("Scope", scopes.ToArray()[0]));
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
    }
}
#endif