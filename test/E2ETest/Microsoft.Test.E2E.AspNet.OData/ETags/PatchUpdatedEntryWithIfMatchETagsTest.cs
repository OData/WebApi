// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
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
#else
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
#endif

namespace Microsoft.Test.E2E.AspNet.OData.ETags
{
    public class PatchUpdatedEntryWithIfMatchETagsTest : WebHostTestBase<PatchUpdatedEntryWithIfMatchETagsTest>
    {
        public PatchUpdatedEntryWithIfMatchETagsTest(WebHostTestFixture<PatchUpdatedEntryWithIfMatchETagsTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.Routes.Clear();
            var model = GetEdmModel(configuration);
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.
                MapODataServiceRoute(
                    routeName: "odata",
                    routePrefix: "odata",
                    model: model,
                    pathHandler: new DefaultODataPathHandler(),
                    routingConventions: ODataRoutingConventions.CreateDefault());

            //, defaultHandler: HttpClientFactory.CreatePipeline(
            //    innerHandler: new HttpControllerDispatcher(configuration),
            //    handlers: new[] { new Microsoft.AspNet.OData.ETagMessageHandler() })
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
        public async Task PatchUpdatedEntryWithIfMatchShouldReturnPreconditionFailed()
        {
            string requestUri = this.BaseAddress + "/odata/ETagsCustomers(2)?$format=json";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);
            var etagInHeader = response.Headers.ETag.ToString();
            JObject result = await response.Content.ReadAsObject<JObject>();
            var etagInPayload = (string)result["@odata.etag"];
            Assert.True(etagInPayload == etagInHeader,
                string.Format("The etag value in payload is not the same as the one in Header, in payload it is: {0}, but in header, {1}.", etagInPayload, etagInHeader));

            requestUri = this.BaseAddress + "/odata/ETagsCustomers(2)";
            request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Content = new StringContent(string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}"",""Notes"":[""{3}""]}}", typeof(ETagsCustomer), 2, "Customer Name 2 updated", "This is note 2 updated"));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);

            requestUri = this.BaseAddress + "/odata/ETagsCustomers(2)";
            request = new HttpRequestMessage(new HttpMethod("Patch"), requestUri);
            request.Content = new StringContent(string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}""}}", typeof(ETagsCustomer), 2, "Customer Name 2 updated again"));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Headers.IfMatch.ParseAdd(etagInPayload);
            response = await this.Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
        }
    }
}