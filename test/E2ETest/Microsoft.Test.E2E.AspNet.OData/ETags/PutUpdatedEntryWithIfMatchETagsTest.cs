// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ETags
{
    public class PutUpdatedEntryWithIfMatchETagsTest : WebHostTestBase<PutUpdatedEntryWithIfMatchETagsTest>
    {
        public PutUpdatedEntryWithIfMatchETagsTest(WebHostTestFixture<PutUpdatedEntryWithIfMatchETagsTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.AddETagMessageHandler(new ETagMessageHandler());
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
            EntitySetConfiguration<ETagsCustomer> eTagsCustomersSet = builder.EntitySet<ETagsCustomer>("ETagsCustomers");
            EntityTypeConfiguration<ETagsCustomer> eTagsCustomers = eTagsCustomersSet.EntityType;
            eTagsCustomers.Property(c => c.Id).IsConcurrencyToken();
            eTagsCustomers.Property(c => c.Name).IsConcurrencyToken();
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task PutUpdatedEntryWithIfMatchShouldReturnPreconditionFailed()
        {
            string requestUri = this.BaseAddress + "/odata/ETagsCustomers(1)?$format=json";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);

            JObject result = await response.Content.ReadAsObject<JObject>();
            var etagInHeader = response.Headers.ETag.ToString();
            var etagInPayload = (string)result["@odata.etag"];
            Assert.True(etagInPayload == etagInHeader,
                string.Format("The etag value in payload is not the same as the one in Header, in payload it is: {0}, but in header, {1}.", etagInPayload, etagInHeader));

            requestUri = this.BaseAddress + "/odata/ETagsCustomers(1)";
            request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Content = new StringContent(string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}"",""Notes"":[""{3}""]}}", typeof(ETagsCustomer), 1, "Customer Name 1 updated", "This is note 1 updated"));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);

            requestUri = this.BaseAddress + "/odata/ETagsCustomers(1)";
            request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Content = new StringContent(string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}"",""Notes"":[""{3}""]}}", typeof(ETagsCustomer), 1, "Customer Name 1 updated again", "This is note 1 updated again"));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Headers.IfMatch.ParseAdd(etagInPayload);
            response = await this.Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
        }
    }
}