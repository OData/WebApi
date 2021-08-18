//-----------------------------------------------------------------------------
// <copyright file="DollarIdTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
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

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(SingersController), typeof(AlbumsController) };
            configuration.AddControllers(controllers);

            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();

            configuration.MapODataServiceRoute("Test", "", DollarIdEdmModel.GetModel(configuration));
            configuration.EnsureInitialized();
        }

        [Fact]
        public async Task DeleteNavigationLink()
        {
            // For parallel running and no side effect, pay attention that key value is 1.
            // And for this test cases only, we don't need to reset the data source because it's only run once.
            var requestBaseUri = this.BaseAddress + "/Singers(1)/Albums";

            //DELETE Singers(1)/Albums/$ref?$id=BaseAddress/Albums(0)
            var response = await this.Client.DeleteAsync(string.Format(requestBaseUri + "/$ref?$id={0}/Albums(0)", this.BaseAddress));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            //GET Singers(1)/Albums
            response = await this.Client.GetAsync(requestBaseUri);
            var json = await response.Content.ReadAsObject<JObject>();
            var result = json["value"] as JArray;
            Assert.Equal<int>(2, result.Count);

            //DELETE Singers(1)/Albums/$ref?$id=../../Albums(1)
            response = await this.Client.DeleteAsync(string.Format(requestBaseUri + "/$ref?$id=../../Albums(1)"));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            //GET Singers(1)/Albums
            response = await this.Client.GetAsync(requestBaseUri);
            json = await response.Content.ReadAsObject<JObject>();
            result = json["value"] as JArray;
            Assert.Single(result);
        }

        [Fact]
        public async Task DeleteContainedNavigationLink()
        {
            // For parallel running and no side effect, pay attention that key value is 1.
            // And for this test cases only, we don't need to reset the data source because it's only run once.
            var requestBaseUri = this.BaseAddress + "/Albums(1)/Sales";

            var response = await this.Client.DeleteAsync(string.Format(requestBaseUri + "/$ref?$id={0}/Albums(1)/Sales(2)", this.BaseAddress));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            //GET Singers(1)/Albums
            response = await this.Client.GetAsync(requestBaseUri);
            var json = await response.Content.ReadAsObject<JObject>();
            var result = json["value"] as JArray;
            Assert.Single(result);

            response = await this.Client.DeleteAsync(string.Format(requestBaseUri + "/$ref?$id=../../Albums(1)/Sales(3)", this.BaseAddress));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            //GET Singers(1)/Albums
            response = await this.Client.GetAsync(requestBaseUri);
            json = await response.Content.ReadAsObject<JObject>();
            result = json["value"] as JArray;
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSingersNameOfAlbum()
        {
            // 3 is a magic test value and is verified at controller.
            var requestBaseUri = this.BaseAddress + "/Albums(3)/Microsoft.Test.E2E.AspNet.OData.DollarId.GetSingers()?$filter=MasterPiece eq 'def'&$select=Name";

            var response = await this.Client.GetAsync(requestBaseUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            var result = json["value"];
            Assert.Equal("Name102", (string)result[0]["Name"]);
        }
    }
}
