// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.SxS.ODataV3.Controllers;
using WebStack.QA.Test.OData.SxS.ODataV4.Controllers;
using Xunit;
using Xunit.Extensions;
using ODataV3Stack = System.Web.Http.OData;
using ODataV4Stack = Microsoft.AspNet.OData;


namespace WebStack.QA.Test.OData.SxS
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
                typeof(ProductsController), typeof(PartsController), typeof(ODataV3Stack.ODataMetadataController), 
                typeof(ProductsV2Controller), typeof(PartsV2Controller), typeof(ODataV4Stack.MetadataController) 
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
        [InlineData("SxSOData/Parts/?v=2", "Part")]
        [InlineData("SxSOData/Parts(1)/?v=2", "Part")]
        [InlineData("SxSOData/Products/?v=2", "Product")]
        [InlineData("SxSOData/Products(1)/?v=2", "Product")]
        public async Task ODataSxSV4QueryTest(string url, string expectedTypeName)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);

            //Act
            HttpResponseMessage responseMessage = await this.Client.GetAsync(requestUri);

            //Assert
            var jObject = await responseMessage.Content.ReadAsAsync<JObject>();

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.Equal("4.0", responseMessage.Headers.GetValues("OData-Version").ElementAt(0));
            Assert.True(jObject.Property("@odata.context").ToString().Contains(expectedTypeName));
        }

        [Theory]
        [InlineData("SxSOData/Parts/", "Part")]
        [InlineData("SxSOData/Parts/?v=1", "Part")]
        [InlineData("SxSOData/Products(1)/?v=1", "Product")]
        [InlineData("SxSOData/Products(1)", "Product")]
        public async Task ODataSxSV3QueryTest(string url, string expectedTypeName)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);

            //Act
            HttpResponseMessage responseMessage = await this.Client.GetAsync(requestUri);

            //Assert
            var jObject = await responseMessage.Content.ReadAsAsync<JObject>();

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.Equal("3.0", responseMessage.Headers.GetValues("DataServiceVersion").ElementAt(0));
            Assert.True(jObject.Property("odata.metadata").ToString().Contains(expectedTypeName));
        }

        [Theory]
        [InlineData("SxSOData/$metadata/", "Version=\"3.0\"")]
        [InlineData("SxSOData/$metadata/?v=1", "Version=\"3.0\"")]
        [InlineData("SxSOData/$metadata/?v=2", "Version=\"4.0\"")]
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
