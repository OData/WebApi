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
using Xunit.Extensions;
using System.Web.OData;

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
            configuration.Services.Replace(
                typeof(System.Web.Http.Dispatcher.IAssembliesResolver),
                new Common.TestAssemblyResolver(typeof(ETagsCustomerController), typeof(ETagsCustomersController),typeof(MetadataController)));
            configuration.
                MapODataServiceRoute(
                    routeName: "odata",
                    routePrefix: "odata",
                    model: model,
                    pathHandler: new DefaultODataPathHandler(),
                    routingConventions: ODataRoutingConventions.CreateDefaultWithAttributeRouting(configuration,model),
                    defaultHandler: HttpClientFactory.CreatePipeline(
                        innerHandler: new HttpControllerDispatcher(configuration),
                        handlers: new[] { new System.Web.OData.ETagMessageHandler() }));
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<ETagsCustomer> eTagsCustomersSet = builder.EntitySet<ETagsCustomer>("ETagsCustomers");
            EntityTypeConfiguration<ETagsCustomer> eTagsCustomers = eTagsCustomersSet.EntityType;
            SingletonConfiguration<ETagsCustomer> eTagsCustomerSingleton = builder.Singleton<ETagsCustomer>("ETagsCustomer");
            eTagsCustomers.Property(c => c.Id).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.Name).IsConcurrencyToken();
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("/odata/ETagsCustomers(0)")]
        [InlineData("/odata/ETagsCustomers(0)/RelatedCustomer")]
        [InlineData("/odata/ETagsCustomers(0)/ContainedCustomer")]
        [InlineData("/odata/ETagsCustomer")]
        [InlineData("/odata/ETagsCustomer/RelatedCustomer")]
        [InlineData("/odata/ETagsCustomer/ContainedCustomer")]
        public void PatchUpdatedEntryWithIfMatchShouldReturnPreconditionFailed(string target)
        {
            string requestUri = this.BaseAddress + target;

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode, string.Format("Get Request failed for {0}", requestUri));
            var etagInHeader = response.Headers.ETag.ToString();
            JObject result = response.Content.ReadAsAsync<JObject>().Result;
            var etagInPayload = (string)result["@odata.etag"];
            Assert.True(etagInPayload == etagInHeader,
                string.Format("The etag value in payload is not the same as the one in Header, in payload it is: {0}, but in header, {1}.", etagInPayload, etagInHeader));

            request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Content = new StringContent(string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}"",""Notes"":[""{3}""]}}", typeof(ETagsCustomer), 0, "Customer Name 0 updated", "This is note 0 updated"));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode, string.Format("PUT Request failed for {0}", requestUri));

            request = new HttpRequestMessage(new HttpMethod("Patch"), requestUri);
            request.Content = new StringContent(string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}""}}", typeof(ETagsCustomer), 0, "Customer Name 0 updated again"));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Headers.IfMatch.ParseAdd(etagInPayload);
            response = this.Client.SendAsync(request).Result;
            Assert.True(HttpStatusCode.PreconditionFailed == response.StatusCode, string.Format("Expected PreconditionFailed from Patch:{0}, instead received {1}", requestUri, response.StatusCode.ToString()));
        }
    }
}