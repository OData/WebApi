using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Dispatcher;
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
    public class PatchUpdatedEntryWithIfMatchETagsTest
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Routes.Clear();
            var model = GetEdmModel();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.
                MapODataServiceRoute(
                    routeName: "odata",
                    routePrefix: "odata",
                    model: model,
                    pathHandler: new DefaultODataPathHandler(),
                    routingConventions: ODataRoutingConventions.CreateDefault(),
                    defaultHandler: HttpClientFactory.CreatePipeline(
                        innerHandler: new HttpControllerDispatcher(configuration),
                        handlers: new[] { new System.Web.OData.ETagMessageHandler() }));
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

        [Fact(Skip = "VSTS AX: Model Container removed")]
        public void PatchUpdatedEntryWithIfMatchShouldReturnPreconditionFailed()
        {
            string requestUri = this.BaseAddress + "/odata/ETagsCustomers(0)?$format=json";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);
            var etagInHeader = response.Headers.ETag.ToString();
            JObject result = response.Content.ReadAsAsync<JObject>().Result;
            var etagInPayload = (string)result["@odata.etag"];
            Assert.True(etagInPayload == etagInHeader,
                string.Format("The etag value in payload is not the same as the one in Header, in payload it is: {0}, but in header, {1}.", etagInPayload, etagInHeader));

            requestUri = this.BaseAddress + "/odata/ETagsCustomers(0)";
            request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Content = new StringContent(string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}"",""Notes"":[""{3}""]}}", typeof(ETagsCustomer), 0, "Customer Name 0 updated", "This is note 0 updated"));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);

            requestUri = this.BaseAddress + "/odata/ETagsCustomers(0)";
            request = new HttpRequestMessage(new HttpMethod("Patch"), requestUri);
            request.Content = new StringContent(string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}""}}", typeof(ETagsCustomer), 0, "Customer Name 0 updated again"));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Headers.IfMatch.ParseAdd(etagInPayload);
            response = this.Client.SendAsync(request).Result;
            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
        }
    }
}