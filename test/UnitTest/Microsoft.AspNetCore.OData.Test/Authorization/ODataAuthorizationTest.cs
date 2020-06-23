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
                typeof(ProductsController)
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
            var entitySet = model.FindDeclaredEntitySet("Products");

            model.AddVocabularyAnnotation(new EdmVocabularyAnnotation(
                entitySet,
                model.FindTerm("Org.OData.Capabilities.V1.ReadRestrictions"),
                new EdmRecordExpression(
                    CreatePermissionProperty(new string[] { "Product.Read", "Product.ReadAll" }),
                    new EdmPropertyConstructor("ReadByKeyRestrictions", CreatePermission(new[] { "Product.ReadByKey" })))));

            model.AddVocabularyAnnotation(new EdmVocabularyAnnotation(
                entitySet,
                model.FindTerm("Org.OData.Capabilities.V1.InsertRestrictions"),
                CreatePermission(new[] { "Product.Insert" })));

            model.AddVocabularyAnnotation(new EdmVocabularyAnnotation(
                entitySet,
                model.FindTerm("Org.OData.Capabilities.V1.DeleteRestrictions"),
                CreatePermission(new[] { "Product.Delete" })));

            model.AddVocabularyAnnotation(new EdmVocabularyAnnotation(
                entitySet,
                model.FindTerm("Org.OData.Capabilities.V1.UpdateRestrictions"),
                CreatePermission(new[] { "Product.Update" })));
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
    }
}
#endif