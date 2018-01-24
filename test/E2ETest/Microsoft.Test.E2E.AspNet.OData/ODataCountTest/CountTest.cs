// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ODataCountTest
{
    public class ODataCountTest : WebHostTestBase
    {
        public ODataCountTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { typeof(HeroesController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));
            configuration.Services.Replace(typeof (IAssembliesResolver), resolver);

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            configuration.Routes.Clear();
            HttpServer httpServer = configuration.GetHttpServer();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute(
                routeName: "odata",
                routePrefix: "odata",
                model: CountEdmModel.GetModel());

            configuration.EnsureInitialized();
        }

        [Theory]
        [InlineData("{0}/odata/Heroes/$count", 1)]
        [InlineData("{0}/odata/Heroes/Default.GetWeapons()/$count", 5)]
        [InlineData("{0}/odata/Heroes/Default.GetNames()/$count", 2)]
        public async Task DollarCountWorksWithEF(string url, int expectedCount)
        {   // Arrange
            string requestUri = string.Format(url, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            int actualCount = Int32.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(expectedCount, actualCount);
        }

        [Theory]
        [InlineData("{0}/odata/Heroes?$count=true", 1)]
        [InlineData("{0}/odata/Heroes/Default.GetWeapons()?$count=true", 5)]
        [InlineData("{0}/odata/Heroes/Default.GetNames()?$count=true", 2)]
        public async Task CountQueryOptionWorksWithEF(string url, int expectedCount)
        {   // Arrange
            string requestUri = string.Format(url, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsAsync<JObject>();
            Assert.Equal(expectedCount, result["@odata.count"]);
        }

        [Theory]
        [InlineData("{0}/odata/Heroes")]
        [InlineData("{0}/odata/Heroes/Default.GetWeapons()")]
        [InlineData("{0}/odata/Heroes/Default.GetNames()")]
        [InlineData("{0}/odata/Heroes?$count=false")]
        [InlineData("{0}/odata/Heroes/Default.GetWeapons()?$count=false")]
        [InlineData("{0}/odata/Heroes/Default.GetNames()?$count=false")]
        public async Task NegativeCountTest(string url)
        {   // Arrange
            string requestUri = string.Format(url, BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string result = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("@odata.count", result);
        }
    }
}