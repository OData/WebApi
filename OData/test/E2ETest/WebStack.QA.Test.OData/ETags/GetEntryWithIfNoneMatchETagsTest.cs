using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using Xunit;

namespace WebStack.QA.Test.OData.ETags
{
    [NuwaFramework]
    public class GetEntryWithIfNoneMatchETagsTest
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<ETagsCustomer>("ETagsCustomers")
                   .EntityType
                   .Property(c => c.Name)
                   .IsConcurrencyToken();

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task GetEntryWithIfNoneMatchShouldReturnNotModifiedETagsTest()
        {
            string eTag;

            var getUri = this.BaseAddress + "/odata/ETagsCustomers?$format=json";
            using (var response = await Client.GetAsync(getUri))
            {
                Assert.True(response.IsSuccessStatusCode);

                var json = await response.Content.ReadAsAsync<JObject>();
                var result = json.GetValue("value") as JArray;
                Assert.NotNull(result);

                eTag = result[0]["@odata.etag"].ToString();
                Assert.False(String.IsNullOrEmpty(eTag));
            }

            var getRequestWithEtag = new HttpRequestMessage(HttpMethod.Get, this.BaseAddress + "/odata/ETagsCustomers(0)");
            getRequestWithEtag.Headers.IfNoneMatch.ParseAdd(eTag);
            using (var response = await Client.SendAsync(getRequestWithEtag))
            {
                Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
            }
        }
    }
}