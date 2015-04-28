using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData.Extensions;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.ModelBuilder;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.ForeignKey
{
    [NuwaFramework]
    [NwHost(HostType.KatanaSelf)]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class ForeignKeyTest
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            configuration.Routes.Clear();
            HttpServer httpServer = configuration.GetHttpServer();
            configuration.Routes.MapODataServiceRoute(routeName: "explicit", routePrefix: "explicit",
                model: ForeignKeyEdmModel.GetExplicitModel(foreignKey: true));

            configuration.Routes.MapODataServiceRoute(routeName: "convention", routePrefix: "convention",
                model: ForeignKeyEdmModel.GetConventionModel());

            configuration.Routes.MapODataServiceRoute(routeName: "noncascade", routePrefix: "noncascade",
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

            var customerIdProperty = order.DeclaredProperties.Single(p => p.Name == "CustomerId");

            var navProperty = order.DeclaredNavigationProperties().Single(p => p.Name == "Customer");
            Assert.Equal(EdmOnDeleteAction.Cascade, navProperty.OnDelete);
            Assert.Equal(1, navProperty.DependentProperties.Count());
            var dependentProperty = navProperty.DependentProperties.Single();

            Assert.Same(customerIdProperty, dependentProperty);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task ReferentialConstraintPresentsOnMetadataDocument(string modelMode)
        {
            string expect = "<ReferentialConstraint>\r\n" + 
                            "          <Principal Role=\"Customer\">\r\n" +
                            "            <PropertyRef Name=\"Id\" />\r\n" + 
                            "          </Principal>\r\n" + 
                            "          <Dependent Role=\"CustomerPartner\">\r\n" + 
                            "            <PropertyRef Name=\"CustomerId\" />\r\n" + 
                            "          </Dependent>\r\n" + 
                            "        </ReferentialConstraint>";

            string requestUri = string.Format("{0}/{1}/$metadata", this.BaseAddress, modelMode);
            HttpResponseMessage response = await Client.GetAsync(requestUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expect, response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public async Task ReferentialConstraintDoesnotPresentsOnMetadataDocument()
        {
            string requestUri = string.Format("{0}/noncascade/$metadata", this.BaseAddress);
            HttpResponseMessage response = await Client.GetAsync(requestUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.DoesNotContain("ReferentialConstraint", response.Content.ReadAsStringAsync().Result);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task CanDeleteTheRelatedOrders_IfDeleteTheCustomer(string modelMode)
        {
            await ResetDatasource("/convention/ForeignKeyCustomers/ResetDataSource");

            string requestCustomerUri = string.Format("{0}/{1}/ForeignKeyCustomers(2)", this.BaseAddress, modelMode);
            string requestOrderUri = string.Format("{0}/{1}/ForeignKeyOrders(5)", this.BaseAddress, modelMode);

            // GET ~/ForeignKeyCustomers(2)
            HttpResponseMessage response = await Client.GetAsync(requestCustomerUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            dynamic payload = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(2, (int) payload["Id"]);
            Assert.Equal("Customer #2", (string) payload["Name"]);

            // GET ~/ForeignKeyOrders(5)
            response = await Client.GetAsync(requestOrderUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            payload = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(5, (int) payload["OrderId"]);
            Assert.Equal("Order #5", (string) payload["OrderName"]);
            Assert.Equal(2, (int) payload["CustomerId"]);

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
            await ResetDatasource("/noncascade/ForeignKeyCustomersNoCascade/ResetDataSource");

            string requestCustomerUri = BaseAddress + "/noncascade/ForeignKeyCustomersNoCascade(2)";
            string requestOrderUri = BaseAddress + "/noncascade/ForeignKeyOrdersNoCascade(5)";

            // GET ~/ForeignKeyCustomersNoCascade(2)
            HttpResponseMessage response = await Client.GetAsync(requestCustomerUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            dynamic payload = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(2, (int) payload["Id"]);
            Assert.Equal("Customer #2", (string) payload["Name"]);

            // GET ~/ForeignKeyOrdersNoCascade(5)
            response = await Client.GetAsync(requestOrderUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            payload = JObject.Parse(response.Content.ReadAsStringAsync().Result);
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