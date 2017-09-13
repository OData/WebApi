// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using Xunit;

namespace WebStack.QA.Test.OData.ETags
{
    [NuwaFramework]
    public class PutUpdatedEntryWithIfMatchETagsTest
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
            configuration.MessageHandlers.Add(new ETagMessageHandler());
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
        public void PutUpdatedEntryWithIfMatchShouldReturnPreconditionFailed()
        {
            string requestUri = this.BaseAddress + "/odata/ETagsCustomers(0)?$format=json";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = this.Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);

            JObject result = response.Content.ReadAsAsync<JObject>().Result;
            var etagInHeader = response.Headers.ETag.ToString();
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
            request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Content = new StringContent(string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}"",""Notes"":[""{3}""]}}", typeof(ETagsCustomer), 0, "Customer Name 0 updated again", "This is note 0 updated again"));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Headers.IfMatch.ParseAdd(etagInPayload);
            response = this.Client.SendAsync(request).Result;
            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
        }
    }
}