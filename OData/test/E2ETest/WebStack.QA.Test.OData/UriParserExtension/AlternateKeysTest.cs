using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData;
using System.Web.OData.Extensions;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.UriParserExtension
{
    [NuwaFramework]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class AlternateKeysTest
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { typeof(CustomersController), typeof(OrdersController), typeof(MetadataController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            configuration.EnableAlternateKeys(true);

            configuration.Routes.Clear();

            configuration.MapODataServiceRoute(routeName: "odata",
                routePrefix: "odata", model: UriParserExtenstionEdmModel.GetEdmModelWithAlternateKeys());

            configuration.EnsureInitialized();
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper webConfig)
        {
            webConfig.AddRAMFAR(true);
        }

        public static TheoryDataSet<string, string, string> CaseInsensitiveCases
        {
            get
            {
                return new TheoryDataSet<string, string, string>()
                {
                    // method, requestPath, response
                    { "Get", "$metadata", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<edmx:Edmx Version=\"4.0\" xmlns:edmx=\"http://docs.oasis-open.org/odata/ns/edmx\">\r\n  <edmx:DataServices>\r\n    <Schema Namespace=\"WebStack.QA.Test.OData.UriParserExtension\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">\r\n      <EntityType Name=\"Customer\">\r\n        <Key>\r\n          <PropertyRef Name=\"Id\" />\r\n        </Key>\r\n        <Property Name=\"Id\" Type=\"Edm.Int32\" Nullable=\"false\" />\r\n        <Property Name=\"Name\" Type=\"Edm.String\" />\r\n        <Property Name=\"Gender\" Type=\"WebStack.QA.Test.OData.UriParserExtension.Gender\" Nullable=\"false\" />\r\n        <NavigationProperty Name=\"Orders\" Type=\"Collection(WebStack.QA.Test.OData.UriParserExtension.Order)\" />\r\n        <Annotation Term=\"OData.Community.Keys.V1.AlternateKeys\">\r\n          <Collection>\r\n            <Record Type=\"OData.Community.Keys.V1.AlternateKey\">\r\n              <PropertyValue Property=\"Key\">\r\n                <Collection>\r\n                  <Record Type=\"OData.Community.Keys.V1.PropertyRef\">\r\n                    <PropertyValue Property=\"Alias\" String=\"Name\" />\r\n                    <PropertyValue Property=\"Name\" PropertyPath=\"Name\" />\r\n                  </Record>\r\n                </Collection>\r\n              </PropertyValue>\r\n            </Record>\r\n          </Collection>\r\n        </Annotation>\r\n      </EntityType>\r\n      <EntityType Name=\"Order\">\r\n        <Key>\r\n          <PropertyRef Name=\"Id\" />\r\n        </Key>\r\n        <Property Name=\"Id\" Type=\"Edm.Int32\" Nullable=\"false\" />\r\n        <Property Name=\"Title\" Type=\"Edm.String\" />\r\n      </EntityType>\r\n      <EntityType Name=\"VipCustomer\" BaseType=\"WebStack.QA.Test.OData.UriParserExtension.Customer\">\r\n        <Property Name=\"VipProperty\" Type=\"Edm.String\" />\r\n      </EntityType>\r\n      <EnumType Name=\"Gender\">\r\n        <Member Name=\"Male\" Value=\"1\" />\r\n        <Member Name=\"Female\" Value=\"2\" />\r\n      </EnumType>\r\n    </Schema>\r\n    <Schema Namespace=\"Default\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">\r\n      <EntityContainer Name=\"Container\">\r\n        <EntitySet Name=\"Customers\" EntityType=\"WebStack.QA.Test.OData.UriParserExtension.Customer\">\r\n          <NavigationPropertyBinding Path=\"Orders\" Target=\"Orders\" />\r\n        </EntitySet>\r\n        <EntitySet Name=\"Orders\" EntityType=\"WebStack.QA.Test.OData.UriParserExtension.Order\" />\r\n      </EntityContainer>\r\n    </Schema>\r\n  </edmx:DataServices>\r\n</edmx:Edmx>"},
                    { "Get", "Customers(1)/Name/$value", "Customer #1"}
                };
            }
        }

        [Theory]
        [PropertyData("CaseInsensitiveCases")]
        public async Task EnableAlternateKeysTest(string method, string requestPath, string comparisonValue)
        {
            var requestUri = string.Format("{0}/odata/{1}", this.BaseAddress, requestPath);
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), requestUri);
            HttpResponseMessage response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();

            Assert.Equal(responseContent, comparisonValue);
        }
    }
}
