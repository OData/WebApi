// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ETags
{
    public class ETagsOtherTypesTest : WebHostTestBase
    {
        public ETagsOtherTypesTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("odata1", "double", GetDoubleETagEdmModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("odata2", "short", GetShortETagEdmModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetDoubleETagEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            var customer = builder.EntitySet<ETagsCustomer>("ETagsCustomers").EntityType;
            customer.Property(c => c.DoubleProperty).IsConcurrencyToken();
            customer.Ignore(c => c.StringWithConcurrencyCheckAttributeProperty);
            return builder.GetEdmModel();
        }

        private static IEdmModel GetShortETagEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            var customer = builder.EntitySet<ETagsCustomer>("ETagsCustomers").EntityType;
            customer.Ignore(c => c.StringWithConcurrencyCheckAttributeProperty);
            customer.Property(c => c.ShortProperty).IsConcurrencyToken();
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task GetEntryWithIfNoneMatchShouldReturnNotModifiedETagsTest_ForDouble()
        {
            string eTag;

            var getUri = this.BaseAddress + "/double/ETagsCustomers?$format=json";
            using (var response = await Client.GetAsync(getUri))
            {
                Assert.True(response.IsSuccessStatusCode);

                var json = await response.Content.ReadAsAsync<JObject>();
                var result = json.GetValue("value") as JArray;
                Assert.NotNull(result);

                // check the first
                eTag = result[0]["@odata.etag"].ToString();
                Assert.False(String.IsNullOrEmpty(eTag));
                Assert.Equal("W/\"Mi4w\"", eTag);

                EntityTagHeaderValue parsedValue;
                Assert.True(EntityTagHeaderValue.TryParse(eTag, out parsedValue));
                HttpConfiguration config = new HttpConfiguration();
                IETagHandler handler = config.GetETagHandler();
                IDictionary<string, object> tags = handler.ParseETag(parsedValue);
                KeyValuePair<string, object> pair = Assert.Single(tags);
                Single value = Assert.IsType<Single>(pair.Value);
                Assert.Equal((Single)2.0, value);
            }

            var getRequestWithEtag = new HttpRequestMessage(HttpMethod.Get, this.BaseAddress + "/double/ETagsCustomers(0)");
            getRequestWithEtag.Headers.IfNoneMatch.ParseAdd(eTag);
            using (var response = await Client.SendAsync(getRequestWithEtag))
            {
                Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetEntryWithIfNoneMatchShouldReturnNotModifiedETagsTest_ForShort()
        {
            string eTag;

            var getUri = this.BaseAddress + "/short/ETagsCustomers?$format=json";
            using (var response = await Client.GetAsync(getUri))
            {
                Assert.True(response.IsSuccessStatusCode);

                var json = await response.Content.ReadAsAsync<JObject>();
                var result = json.GetValue("value") as JArray;
                Assert.NotNull(result);

                // check the second
                eTag = result[1]["@odata.etag"].ToString();
                Assert.False(String.IsNullOrEmpty(eTag));
                Assert.Equal("W/\"MzI3NjY=\"", eTag);

                EntityTagHeaderValue parsedValue;
                Assert.True(EntityTagHeaderValue.TryParse(eTag, out parsedValue));
                HttpConfiguration config = new HttpConfiguration();
                IETagHandler handler = config.GetETagHandler();
                IDictionary<string, object> tags = handler.ParseETag(parsedValue);
                KeyValuePair<string, object> pair = Assert.Single(tags);
                int value = Assert.IsType<int>(pair.Value);
                Assert.Equal((short)32766, value);
            }

            var getRequestWithEtag = new HttpRequestMessage(HttpMethod.Get, this.BaseAddress + "/short/ETagsCustomers(1)");
            getRequestWithEtag.Headers.IfNoneMatch.ParseAdd(eTag);
            using (var response = await Client.SendAsync(getRequestWithEtag))
            {
                Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
            }
        }
    }
}