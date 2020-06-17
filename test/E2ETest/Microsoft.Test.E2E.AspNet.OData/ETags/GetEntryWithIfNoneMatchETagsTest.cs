// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
    public class GetEntryWithIfNoneMatchETagsTest : WebHostTestBase
    {
        public GetEntryWithIfNoneMatchETagsTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();

            builder.EntitySet<ETagsCustomer>("ETagsCustomers")
                   .EntityType
                   .Property(c => c.Name)
                   .IsConcurrencyToken();

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task GetEntryWithIfNoneMatchShouldReturnNotModifiedETagsTest()
        {
            string eTag;

             // DeleteUpdatedEntryWithIfMatchETagsTests will change #"0" customer
             // PutUpdatedEntryWithIfMatchETagsTests will change #"1"customer
             // PatchUpdatedEntryWithIfMatchETagsTest will change #"2" customer
             // So, this case uses "4"
            int customerId = 4;
            var getUri = this.BaseAddress + "/odata/ETagsCustomers?$format=json";
            using (var response = await Client.GetAsync(getUri))
            {
                Assert.True(response.IsSuccessStatusCode);

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;
                Assert.NotNull(result);

                eTag = result[customerId]["@odata.etag"].ToString();
                Assert.False(String.IsNullOrEmpty(eTag));
            }

            var getRequestWithEtag = new HttpRequestMessage(HttpMethod.Get, this.BaseAddress + "/odata/ETagsCustomers(" + customerId + ")");
            getRequestWithEtag.Headers.IfNoneMatch.ParseAdd(eTag);
            using (var response = await Client.SendAsync(getRequestWithEtag))
            {
                Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
            }
        }
    }
}