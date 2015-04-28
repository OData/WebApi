using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using Microsoft.Data.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using Xunit;

namespace WebStack.QA.Test.OData.ETags
{
    [NuwaFramework]
    public class CRUDEntryWithETagsTest
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.Routes.MapODataServiceRoute("odata", "odata", GetEdmModel(), new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault());
            configuration.MessageHandlers.Add(new ETagMessageHandler());
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
        public async Task GetEntryWithIfNoneMatchShouldReturnNotModifiedTest()
        {
            string eTag;

            var getUri = this.BaseAddress + "/odata/ETagsCustomers";
            using (var response = await Client.GetAsync(getUri))
            {
                Assert.True(response.IsSuccessStatusCode);

                var json = await response.Content.ReadAsAsync<JObject>();
                var result = json.GetValue("value") as JArray;
                Assert.NotNull(result);

                eTag = result[0]["odata.etag"].ToString();
                Assert.False(String.IsNullOrEmpty(eTag));
            }

            var getRequestWithEtag = new HttpRequestMessage(HttpMethod.Get,
                this.BaseAddress + "/odata/ETagsCustomers(0)");
            getRequestWithEtag.Headers.IfNoneMatch.ParseAdd(eTag);
            using (var response = await Client.SendAsync(getRequestWithEtag))
            {
                Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetEntryWithIfMatchShouldReturnTest()
        {
            string eTag;

            var getUri = this.BaseAddress + "/odata/ETagsCustomers";
            using (var response = await Client.GetAsync(getUri))
            {
                Assert.True(response.IsSuccessStatusCode);

                var json = await response.Content.ReadAsAsync<JObject>();
                var result = json.GetValue("value") as JArray;
                Assert.NotNull(result);

                eTag = result[0]["odata.etag"].ToString();
                Assert.False(string.IsNullOrEmpty(eTag));
            }

            var getRequestWithEtag = new HttpRequestMessage(HttpMethod.Get,
                this.BaseAddress + "/odata/ETagsCustomers(0)");
            getRequestWithEtag.Headers.IfMatch.ParseAdd(eTag);
            using (var response = await Client.SendAsync(getRequestWithEtag))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetEntryWithIfMatchStarShouldReturn()
        {
            string eTag = "*";
            var getRequestWithEtag = new HttpRequestMessage(HttpMethod.Get,
                this.BaseAddress + "/odata/ETagsCustomers(0)");
            getRequestWithEtag.Headers.IfMatch.ParseAdd(eTag);
            using (var response = await Client.SendAsync(getRequestWithEtag))
            {
                Assert.True(response.IsSuccessStatusCode);
            }
        }

        [Fact]
        public async Task DeleteUpdatedEntryWithIfMatchShouldReturnPreconditionFailed()
        {
            string eTag;

            var getUri = this.BaseAddress + "/odata/ETagsCustomers";
            using (HttpResponseMessage getResponse = await this.Client.GetAsync(getUri))
            {
                Assert.True(getResponse.IsSuccessStatusCode);
                var json = await getResponse.Content.ReadAsAsync<JObject>();
                var result = json.GetValue("value") as JArray;

                eTag = result[0]["odata.etag"].ToString();
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

        [Fact]
        public async Task PutUpdatedEntryWithIfMatchShouldReturnPreconditionFailed()
        {
            ETagsCustomersController.ResetCustomers();

            string etagInPayload;
            var getUri = this.BaseAddress + "/odata/ETagsCustomers(2)";
            using (HttpResponseMessage getResponse = await this.Client.GetAsync(getUri))
            {
                Assert.True(getResponse.IsSuccessStatusCode);
                JObject result = getResponse.Content.ReadAsAsync<JObject>().Result;
                var etagInHeader = getResponse.Headers.ETag.ToString();
                etagInPayload = (string)result["odata.etag"];
                Assert.True(etagInPayload == etagInHeader,
                    string.Format("The etag value in payload is not the same as the one in Header, in payload it is: {0}, but in header, {1}.", etagInPayload, etagInHeader));
            }

            var putUri1 = this.BaseAddress + "/odata/ETagsCustomers(2)";
            var putContent1 = JObject.Parse(string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}"",""Notes"":[""{3}""]}}", typeof(ETagsCustomer), 2, "Customer Name 2 updated", "This is note 2 updated"));
            using (HttpResponseMessage putResponse = await this.Client.PutAsJsonAsync(putUri1, putContent1))
            {
                Assert.True(putResponse.IsSuccessStatusCode);
            }

            var putUri2 = this.BaseAddress + "/odata/ETagsCustomers(2)";
            var putRequest = new HttpRequestMessage(HttpMethod.Put, putUri2);
            putRequest.Content = new StringContent(string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}"",""Notes"":[""{3}""]}}", typeof(ETagsCustomer), 2, "Customer Name 2 updated again", "This is note 2 updated again"));
            putRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            putRequest.Headers.IfMatch.ParseAdd(etagInPayload);
            using (HttpResponseMessage putResponse = await this.Client.SendAsync(putRequest))
            {
                Assert.Equal(HttpStatusCode.PreconditionFailed, putResponse.StatusCode);
            }
        }

        [Fact]
        public async Task PatchUpdatedEntryWithIfMatchShouldReturnPreconditionFailed()
        {
            ETagsCustomersController.ResetCustomers();

            string etagInPayload;
            var getUri = this.BaseAddress + "/odata/ETagsCustomers(1)";
            using (HttpResponseMessage getResponse = await this.Client.GetAsync(getUri))
            {
                Assert.True(getResponse.IsSuccessStatusCode);
                var etagInHeader = getResponse.Headers.ETag.ToString();
                JObject result = getResponse.Content.ReadAsAsync<JObject>().Result;
                etagInPayload = (string)result["odata.etag"];
                Assert.True(etagInPayload == etagInHeader,
                    string.Format("The etag value in payload is not the same as the one in Header, in payload it is: {0}, but in header, {1}.", etagInPayload, etagInHeader));
            }

            var putUri = this.BaseAddress + "/odata/ETagsCustomers(1)";
            var putPayload = JObject.Parse(string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}"",""Notes"":[""{3}""]}}", typeof(ETagsCustomer), 1, "Customer Name 1 updated", "This is note 1 updated"));
            using (HttpResponseMessage putResponse = await this.Client.PutAsJsonAsync(putUri, putPayload))
            {
                Assert.True(putResponse.IsSuccessStatusCode);
            }

            var patchUri = this.BaseAddress + "/odata/ETagsCustomers(1)";
            var patchRequest = new HttpRequestMessage(new HttpMethod("PATCH"), patchUri);
            patchRequest.Content = new StringContent(string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}""}}", typeof(ETagsCustomer), 1, "Customer Name 1 updated again"));
            patchRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            patchRequest.Headers.IfMatch.ParseAdd(etagInPayload);
            using (HttpResponseMessage patchReponse = await this.Client.SendAsync(patchRequest))
            {
                Assert.Equal(HttpStatusCode.PreconditionFailed, patchReponse.StatusCode);
            }
        }

        [Fact]
        public void JsonWithDifferentMetadataLevelsHaveSameETagsTest()
        {
            string requestUri = this.BaseAddress + "/odata/ETagsCustomers";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json");
            HttpResponseMessage response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);
            var jsonResult = response.Content.ReadAsAsync<JObject>().Result;
            var jsonETags = jsonResult.GetValue("value").Select(e => e["odata.etag"].ToString());
            Assert.Equal(jsonETags.Count(), jsonETags.Distinct().Count());

            requestUri = this.BaseAddress + "/odata/ETagsCustomers";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json;odata=nometadata");
            response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);
            var jsonWithNometadataResult = response.Content.ReadAsAsync<JObject>().Result;
            var jsonWithNometadataETags = jsonWithNometadataResult.GetValue("value").Select(e => e["odata.etag"].ToString());
            Assert.Equal(jsonWithNometadataETags.Count(), jsonWithNometadataETags.Distinct().Count());
            Assert.Equal(jsonETags, jsonWithNometadataETags);

            requestUri = this.BaseAddress + "/odata/ETagsCustomers";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json;odata=fullmetadata");
            response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);
            var jsonWithFullmetadataResult = response.Content.ReadAsAsync<JObject>().Result;
            var jsonWithFullmetadataETags = jsonWithFullmetadataResult.GetValue("value").Select(e => e["odata.etag"].ToString());
            Assert.Equal(jsonWithFullmetadataETags.Count(), jsonWithFullmetadataETags.Distinct().Count());
            Assert.Equal(jsonETags, jsonWithFullmetadataETags);

            requestUri = this.BaseAddress + "/odata/ETagsCustomers";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json;odata=minimalmetadata");
            response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);
            var jsonWithMinimalmetadataResult = response.Content.ReadAsAsync<JObject>().Result;
            var jsonWithMinimalmetadataETags = jsonWithMinimalmetadataResult.GetValue("value").Select(e => e["odata.etag"].ToString());
            Assert.Equal(jsonWithMinimalmetadataETags.Count(), jsonWithMinimalmetadataETags.Distinct().Count());
            Assert.Equal(jsonETags, jsonWithMinimalmetadataETags);
        }
    }
}
