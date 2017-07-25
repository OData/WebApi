using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Xunit;

namespace WebStack.QA.Test.OData.ETags
{
    [NuwaFramework]
    public class DerivedETagTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MessageHandlers.Add(new ETagMessageHandler());
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<ETagsCustomer> eTagsCustomersSet = builder.EntitySet<ETagsCustomer>("ETagsCustomers");
            eTagsCustomersSet.HasRequiredBinding(c => c.RelatedCustomer, eTagsCustomersSet);
            eTagsCustomersSet.HasRequiredBinding(c => c.ContainedCustomer, eTagsCustomersSet);
            EntitySetConfiguration<ETagsCustomer> eTagsDerivedCustomersSet = builder.EntitySet<ETagsCustomer>("ETagsDerivedCustomers");
            eTagsDerivedCustomersSet.HasRequiredBinding(c => c.RelatedCustomer, eTagsCustomersSet);
            eTagsDerivedCustomersSet.HasRequiredBinding(c => c.ContainedCustomer, eTagsCustomersSet);
            SingletonConfiguration<ETagsCustomer> eTagsCustomerSingleton = builder.Singleton<ETagsCustomer>("ETagsDerivedCustomersSingleton");
            eTagsCustomerSingleton.HasRequiredBinding(c => c.RelatedCustomer, eTagsCustomersSet);
            eTagsCustomerSingleton.HasRequiredBinding(c => c.ContainedCustomer, eTagsCustomersSet);
            return builder.GetEdmModel();
        }

        [Fact]
        public void DerivedTypesHaveSameETagsTest()
        {
            string requestUri = this.BaseAddress + "/odata/ETagsCustomers?$select=Id";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json");
            HttpResponseMessage response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);
            var jsonResult = response.Content.ReadAsAsync<JObject>().Result;
            var jsonETags = jsonResult.GetValue("value").Select(e => e["@odata.etag"].ToString());

            requestUri = this.BaseAddress + "/odata/ETagsDerivedCustomers?$select=Id";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json");
            response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);
            jsonResult = response.Content.ReadAsAsync<JObject>().Result;
            var derivedEtags = jsonResult.GetValue("value").Select(e => e["@odata.etag"].ToString());
            
            Assert.True(String.Concat(jsonETags) == String.Concat(derivedEtags), "Derived Types has different etags than base type");
        }

        [Fact(Skip = "VSTS AX: Model Container removed")]
        public void SingletonsHaveSameETagsTest()
        {
            string requestUri = this.BaseAddress + "/odata/ETagsCustomers?$select=Id";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json");
            HttpResponseMessage response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);
            var jsonResult = response.Content.ReadAsAsync<JObject>().Result;
            var jsonETags = jsonResult.GetValue("value").Select(e => e["@odata.etag"].ToString());

            requestUri = this.BaseAddress + "/odata/ETagsDerivedCustomersSingleton?$select=Id";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json");
            response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);
            jsonResult = response.Content.ReadAsAsync<JObject>().Result;
            var singletonEtag = jsonResult.GetValue("@odata.etag").ToString();

            Assert.True(jsonETags.FirstOrDefault() == singletonEtag, "Singleton has different etags than Set");
        }
    }
}
