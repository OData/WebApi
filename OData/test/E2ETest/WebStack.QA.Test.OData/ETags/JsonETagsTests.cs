using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData;
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
    public class JsonETagsTests
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
            configuration.MessageHandlers.Add(new ETagMessageHandler());
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<ETagsCustomer> eTagsCustomersSet = builder.EntitySet<ETagsCustomer>("ETagsCustomers");
            EntityTypeConfiguration<ETagsCustomer> eTagsCustomers = eTagsCustomersSet.EntityType;
            eTagsCustomers.Property(c => c.Id).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.Name).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.BoolProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.ByteProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.CharProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.DecimalProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.DoubleProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.ShortProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.LongProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.SbyteProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.FloatProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.UshortProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.UintProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.UlongProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.GuidProperty).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.DateTimeOffsetProperty).IsConcurrencyToken();
            return builder.GetEdmModel();
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
            var jsonETags = jsonResult.GetValue("value").Select(e => e["@odata.etag"].ToString());
            Assert.Equal(jsonETags.Count(), jsonETags.Distinct().Count());

            requestUri = this.BaseAddress + "/odata/ETagsCustomers";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json;odata=nometadata");
            response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);
            var jsonWithNometadataResult = response.Content.ReadAsAsync<JObject>().Result;
            var jsonWithNometadataETags = jsonWithNometadataResult.GetValue("value").Select(e => e["@odata.etag"].ToString());
            Assert.Equal(jsonWithNometadataETags.Count(), jsonWithNometadataETags.Distinct().Count());
            Assert.Equal(jsonETags, jsonWithNometadataETags);

            requestUri = this.BaseAddress + "/odata/ETagsCustomers";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json;odata=fullmetadata");
            response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);
            var jsonWithFullmetadataResult = response.Content.ReadAsAsync<JObject>().Result;
            var jsonWithFullmetadataETags = jsonWithFullmetadataResult.GetValue("value").Select(e => e["@odata.etag"].ToString());
            Assert.Equal(jsonWithFullmetadataETags.Count(), jsonWithFullmetadataETags.Distinct().Count());
            Assert.Equal(jsonETags, jsonWithFullmetadataETags);

            requestUri = this.BaseAddress + "/odata/ETagsCustomers";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json;odata=minimalmetadata");
            response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);
            var jsonWithMinimalmetadataResult = response.Content.ReadAsAsync<JObject>().Result;
            var jsonWithMinimalmetadataETags = jsonWithMinimalmetadataResult.GetValue("value").Select(e => e["@odata.etag"].ToString());
            Assert.Equal(jsonWithMinimalmetadataETags.Count(), jsonWithMinimalmetadataETags.Distinct().Count());
            Assert.Equal(jsonETags, jsonWithMinimalmetadataETags);
        }
    }
}