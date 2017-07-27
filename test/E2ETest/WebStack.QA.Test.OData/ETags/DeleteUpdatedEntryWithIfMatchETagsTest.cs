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
    public class DeleteUpdatedEntryWithIfMatchETagsTest
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<ETagsCustomer> eTagsCustomersSet = builder.EntitySet<ETagsCustomer>("ETagsCustomers");
            EntityTypeConfiguration<ETagsCustomer> eTagsCustomers = eTagsCustomersSet.EntityType;
            eTagsCustomers.Property(c => c.Id).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.Name).IsConcurrencyToken();
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task DeleteUpdatedEntryWithIfMatchShouldReturnPreconditionFailed()
        {
            string eTag;

            var getUri = this.BaseAddress + "/odata/ETagsCustomers?$format=json";
            using (HttpResponseMessage getResponse = await this.Client.GetAsync(getUri))
            {
                Assert.True(getResponse.IsSuccessStatusCode);
                var json = await getResponse.Content.ReadAsAsync<JObject>();
                var result = json.GetValue("value") as JArray;

                eTag = result[0]["@odata.etag"].ToString();
                Assert.False(String.IsNullOrEmpty(eTag));
            }

            var putUri = this.BaseAddress + "/odata/ETagsCustomers(0)";
            var putContent = JObject.Parse(string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}"",""Notes"":[""{3}""]}}", typeof(ETagsCustomer), 0, "Customer Name 0 updated", "This is note 0 updated"));
            using (HttpResponseMessage response = await Client.PutAsJsonAsync(putUri, putContent))
            {
                response.EnsureSuccessStatusCode();
            }

            var deleteUri = this.BaseAddress + "/odata/ETagsCustomers(0)";
            var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, deleteUri);
            deleteRequest.Headers.IfMatch.ParseAdd(eTag);
            using (HttpResponseMessage response = await Client.SendAsync(deleteRequest))
            {
                Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
            }
        }
    }
}