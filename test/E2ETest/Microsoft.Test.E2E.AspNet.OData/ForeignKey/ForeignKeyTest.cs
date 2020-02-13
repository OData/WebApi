// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ForeignKey
{
    public class ForeignKeyTest : WebHostTestBase<ForeignKeyTest>
    {
        public ForeignKeyTest(WebHostTestFixture<ForeignKeyTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(ForeignKeyCustomersController),
                typeof(ForeignKeyOrdersController),
                typeof(ForeignKeyCustomersNoCascadeController),
                typeof(ForeignKeyOrdersNoCascadeController),
                typeof(MetadataController) };
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute(routeName: "explicit", routePrefix: "explicit",
                model: ForeignKeyEdmModel.GetExplicitModel(foreignKey: true));

            configuration.MapODataServiceRoute(routeName: "convention", routePrefix: "convention",
                model: ForeignKeyEdmModel.GetConventionModel(configuration));

            configuration.MapODataServiceRoute(routeName: "noncascade", routePrefix: "noncascade",
                model: ForeignKeyEdmModel.GetExplicitModel(foreignKey: false));

            configuration.EnsureInitialized();
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task ReferentialConstraintModelBuilderTest(string modelMode)
        {
            string requestUri = string.Format("{0}/{1}/$metadata", this.BaseAddress, modelMode);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);

            var stream = await response.Content.ReadAsStreamAsync();
            IODataResponseMessage message = new ODataMessageWrapper(stream, response.Content.Headers);
            var reader = new ODataMessageReader(message);
            var edmModel = reader.ReadMetadataDocument();

            var customer = edmModel.SchemaElements.OfType<IEdmEntityType>()
                .Single(et => et.Name == "ForeignKeyCustomer");
            Assert.Equal(3, customer.Properties().Count());

            var order = edmModel.SchemaElements.OfType<IEdmEntityType>().Single(et => et.Name == "ForeignKeyOrder");
            Assert.Equal(4, order.Properties().Count());

            var idProperty = customer.DeclaredProperties.Single(p => p.Name == "Id");
            var customerIdProperty = order.DeclaredProperties.Single(p => p.Name == "CustomerId");

            var navProperty = order.DeclaredNavigationProperties().Single(p => p.Name == "Customer");
            Assert.Equal(EdmOnDeleteAction.Cascade, navProperty.OnDelete);

            var dependentProperty = Assert.Single(navProperty.DependentProperties());
            Assert.Same(customerIdProperty, dependentProperty);

            var principalProperty = Assert.Single(navProperty.PrincipalProperties());
            Assert.Same(idProperty, principalProperty);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task ReferentialConstraintPresentsOnMetadataDocument(string modelMode)
        {
            string expect = "<NavigationProperty Name=\"Customer\" Type=\"Microsoft.Test.E2E.AspNet.OData.ForeignKey.ForeignKeyCustomer\">\r\n" + 
                            "          <OnDelete Action=\"Cascade\" />\r\n" + 
                            "          <ReferentialConstraint Property=\"CustomerId\" ReferencedProperty=\"Id\" />\r\n" + 
                            "        </NavigationProperty>";
            // Remove indentation
            expect = Regex.Replace(expect, @"\r\n\s*<", @"<");

            string requestUri = string.Format("{0}/{1}/$metadata", this.BaseAddress, modelMode);
            HttpResponseMessage response = await Client.GetAsync(requestUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expect, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task ReferentialConstraintDoesnotPresentsOnMetadataDocument()
        {
            string requestUri = string.Format("{0}/noncascade/$metadata", this.BaseAddress);
            HttpResponseMessage response = await Client.GetAsync(requestUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.DoesNotContain("ReferentialConstraint", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task CanDeleteTheRelatedOrders_IfDeleteTheCustomer(string modelMode)
        {
            await ResetDatasource("/convention/ResetDataSource");

            string requestCustomerUri = string.Format("{0}/{1}/ForeignKeyCustomers(2)", this.BaseAddress, modelMode);
            string requestOrderUri = string.Format("{0}/{1}/ForeignKeyOrders(5)", this.BaseAddress, modelMode);

            // GET ~/ForeignKeyCustomers(2)
            HttpResponseMessage response = await Client.GetAsync(requestCustomerUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            dynamic payload = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(2, (int)payload["Id"]);
            Assert.Equal("Customer #2", (string)payload["Name"]);

            // GET ~/ForeignKeyOrders(5)
            response = await Client.GetAsync(requestOrderUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            payload = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(5, (int)payload["OrderId"]);
            Assert.Equal("Order #5", (string)payload["OrderName"]);
            Assert.Equal(2, (int)payload["CustomerId"]);

            // DELETE ~/ForeignKeyCustomers(2)
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, requestCustomerUri);
            response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // GET ~/ForeignKeyCustomers(2)
            response = await Client.GetAsync(requestCustomerUri);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            // GET ~/ForeignKeyOrders(5)
            response = await Client.GetAsync(requestOrderUri);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CannotDeleteTheRelatedOrders_IfDeleteTheCustomer()
        {
            await ResetDatasource("/noncascade/ResetDataSourceNonCacade");

            string requestCustomerUri = BaseAddress + "/noncascade/ForeignKeyCustomersNoCascade(2)";
            string requestOrderUri = BaseAddress + "/noncascade/ForeignKeyOrdersNoCascade(5)";

            // GET ~/ForeignKeyCustomersNoCascade(2)
            HttpResponseMessage response = await Client.GetAsync(requestCustomerUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            dynamic payload = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(2, (int)payload["Id"]);
            Assert.Equal("Customer #2", (string)payload["Name"]);

            // GET ~/ForeignKeyOrdersNoCascade(5)
            response = await Client.GetAsync(requestOrderUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            payload = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(5, (int)payload["OrderId"]);
            Assert.Equal("Order #5", (string)payload["OrderName"]);
            Assert.Equal(2, (int)payload["CustomerId"]);

            // DELETE ~/ForeignKeyCustomersNoCascade(2)  will fail.
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, requestCustomerUri);
            response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            // DELETE ~/ForeignKeyOrdersNoCascade(4,5,6)
            for (int i = 4; i <= 6; i++)
            {
                string subOrderUri = BaseAddress + "/noncascade/ForeignKeyOrdersNoCascade(" + i + ")";
                request = new HttpRequestMessage(HttpMethod.Delete, subOrderUri);
                response = await Client.SendAsync(request);
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }

            // Re-DELETE ~/ForeignKeyCustomersNoCascade(2)
            request = new HttpRequestMessage(HttpMethod.Delete, requestCustomerUri);
            response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // GET ~/ForeignKeyCustomersNoCascade(2)
            response = await Client.GetAsync(requestCustomerUri);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            // GET ~/ForeignKeyOrdersNoCascade(5)
            response = await Client.GetAsync(requestOrderUri);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task<HttpResponseMessage> ResetDatasource(string uri)
        {
            var uriReset = this.BaseAddress + uri;
            var response = await this.Client.PostAsync(uriReset, null);
            return response;
        }
    }
}
