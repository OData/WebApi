// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
    public class DeleteUpdatedEntryWithIfMatchETagsTest : WebHostTestBase<DeleteUpdatedEntryWithIfMatchETagsTest>
    {
        public DeleteUpdatedEntryWithIfMatchETagsTest(WebHostTestFixture<DeleteUpdatedEntryWithIfMatchETagsTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
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
        public async Task DeleteUpdatedEntryWithIfMatchShouldReturnPreconditionFailed()
        {
            string eTag;

            var getUri = this.BaseAddress + "/odata/ETagsCustomers?$format=json";
            using (HttpResponseMessage getResponse = await this.Client.GetAsync(getUri))
            {
                Assert.True(getResponse.IsSuccessStatusCode);
                var json = await getResponse.Content.ReadAsObject<JObject>();
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