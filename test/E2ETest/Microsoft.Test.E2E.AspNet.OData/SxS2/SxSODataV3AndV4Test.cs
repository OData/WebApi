// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.SxS2.ODataV3.Controllers;
using Microsoft.Test.E2E.AspNet.OData.SxS2.ODataV4.Controllers;
using Newtonsoft.Json.Linq;
using Xunit;
using ODataV3Stack = System.Web.Http.OData;
using ODataV4Stack = Microsoft.AspNet.OData;

namespace Microsoft.Test.E2E.AspNet.OData.SxS2
{
    public class SxSODataV3AndV4Test : WebHostTestBase<SxSODataV3AndV4Test>
    {
        public SxSODataV3AndV4Test(WebHostTestFixture<SxSODataV3AndV4Test> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            var controllers = new[]
            {
                typeof(ProductsController), typeof(ODataV3Stack.ODataMetadataController),
                typeof(ProductsV2Controller), typeof(ODataV4Stack.MetadataController)
            };

            configuration.AddControllers(controllers);

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
            Assert.Contains(expectedTypeName, jObject.Property("@odata.context").ToString());
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
            Assert.Contains(expectedTypeName, jObject.Property("odata.metadata").ToString());
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
            Assert.Contains(expectedVersion, metaData);
        }
    }
}
