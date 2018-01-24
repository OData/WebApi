// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.DollarId
{
    public class DollarIdTest : WebHostTestBase
    {
        public DollarIdTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { typeof(SingersController), typeof(AlbumsController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();

            configuration.MapODataServiceRoute("Test", "", DollarIdEdmModel.GetModel());
            configuration.EnsureInitialized();
        }

        [Fact]
        public async Task DeleteNavigationLink()
        {
            var requestBaseUri = this.BaseAddress + "/Singers(0)/Albums";

            await ResetDataSource("Singers");
            await ResetDataSource("Albums");

            //DELETE Singers(0)/Albums/$ref?$id=BaseAddress/Albums(0)
            var response = await this.Client.DeleteAsync(string.Format(requestBaseUri + "/$ref?$id={0}/Albums(0)", this.BaseAddress));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            //GET Singers(0)/Albums
            response = await this.Client.GetAsync(requestBaseUri);
            var json = await response.Content.ReadAsAsync<JObject>();
            var result = json["value"] as JArray;
            Assert.Equal<int>(2, result.Count);

            //DELETE Singers(0)/Albums/$ref?$id=../../Albums(0)
            response = await this.Client.DeleteAsync(string.Format(requestBaseUri + "/$ref?$id=../../Albums(1)"));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            //GET Singers(0)/Albums
            response = await this.Client.GetAsync(requestBaseUri);
            json = await response.Content.ReadAsAsync<JObject>();
            result = json["value"] as JArray;
            Assert.Single(result);
        }

        [Fact]
        public async Task DeleteContainedNavigationLink()
        {
            var requestBaseUri = this.BaseAddress + "/Albums(5)/Sales";

            await ResetDataSource("Singers");
            await ResetDataSource("Albums");

            var response = await this.Client.DeleteAsync(string.Format(requestBaseUri + "/$ref?$id={0}/Albums(5)/Sales(6)", this.BaseAddress));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            //GET Singers(0)/Albums
            response = await this.Client.GetAsync(requestBaseUri);
            var json = await response.Content.ReadAsAsync<JObject>();
            var result = json["value"] as JArray;
            Assert.Single(result);

            response = await this.Client.DeleteAsync(string.Format(requestBaseUri + "/$ref?$id=../../Albums(5)/Sales(7)", this.BaseAddress));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await this.Client.GetAsync(requestBaseUri);
            json = await response.Content.ReadAsAsync<JObject>();
            result = json["value"] as JArray;
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSingersNameOfAlbum()
        {
            var requestBaseUri = this.BaseAddress + "/Albums(5)/Microsoft.Test.E2E.AspNet.OData.DollarId.GetSingers()?$filter=MasterPiece eq 'def'&$select=Name";

            var response = await this.Client.GetAsync(requestBaseUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await this.Client.GetAsync(requestBaseUri);
            var json = await response.Content.ReadAsAsync<JObject>();
            var result = json["value"];
            Assert.Equal("Name102", (string)result[0]["Name"]);
        }

        private async Task<HttpResponseMessage> ResetDataSource(string controller)
        {
            var uriReset = string.Format(this.BaseAddress + "/{0}/Microsoft.Test.E2E.AspNet.OData.DollarId.ResetDataSource", controller);
            var response = await this.Client.PostAsync(uriReset, null);

            return response;
        }
    }
}
