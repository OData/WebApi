using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.SxS2.ODataV3.Controllers;
using WebStack.QA.Test.OData.SxS2.ODataV4.Controllers;
using Xunit;
using Xunit.Extensions;
using ODataV3Stack = System.Web.Http.OData;
using ODataV4Stack = System.Web.OData;

namespace WebStack.QA.Test.OData.SxS2
{
    [NuwaFramework]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class SxSODataV3AndV4Test
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[]
            {
                typeof(ProductsController), typeof(ODataV3Stack.ODataMetadataController), 
                typeof(ProductsV2Controller), typeof(ODataV4Stack.MetadataController) 
            };

            var resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            configuration.Routes.Clear();

            ODataV3.ODataV3WebApiConfig.Register(configuration);
            ODataV4.ODataV4WebApiConfig.Register(configuration);

            configuration.EnsureInitialized();
        }

        [Theory]
        [InlineData("SxSOData/Products", "Product")]
        [InlineData("SxSOData/Products(1)", "Product")]
        public async Task ODataSxSV4QueryTest(string url, string expectedTypeName)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("OData-Version", "4.0");
            request.Headers.Add("OData-MaxVersion", "4.0");
            request.Headers.Add("MaxDataServiceVersion", "3.0");

            //Act
            HttpResponseMessage responseMessage = await this.Client.SendAsync(request);

            //Assert
            var jObject = await responseMessage.Content.ReadAsAsync<JObject>();

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.Equal("4.0", responseMessage.Headers.GetValues("OData-Version").ElementAt(0));
            Assert.True(jObject.Property("@odata.context").ToString().Contains(expectedTypeName));
        }

        [Theory]
        [InlineData("SxSOData/Products", "Product")]
        [InlineData("SxSOData/Products(1)", "Product")]
        public async Task ODataSxSV3QueryTest(string url, string expectedTypeName)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("DataServiceVersion", "3.0");
            request.Headers.Add("MaxDataServiceVersion", "3.0");
            request.Headers.Add("OData-MaxVersion", "4.0");

            //Act
            HttpResponseMessage responseMessage = await this.Client.SendAsync(request);

            //Assert
            var jObject = await responseMessage.Content.ReadAsAsync<JObject>();

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.Equal("3.0", responseMessage.Headers.GetValues("DataServiceVersion").ElementAt(0));
            Assert.True(jObject.Property("odata.metadata").ToString().Contains(expectedTypeName));
        }

        [Theory]
        [InlineData("SxSOData/$metadata/", "Version=\"3.0\"")]
        public async Task ODataSxSMetadataQueryTest(string url, string expectedVersion)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);

            //Act
            HttpResponseMessage responseMessage = await this.Client.GetAsync(requestUri);

            //Assert
            var metaData = await responseMessage.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.True(metaData.Contains(expectedVersion));
        }
    }
}
