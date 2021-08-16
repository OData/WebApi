//-----------------------------------------------------------------------------
// <copyright file="ETagsOtherTypesTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
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

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("odata1", "double", GetDoubleETagEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("odata2", "short", GetShortETagEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetDoubleETagEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            var customer = builder.EntitySet<ETagsCustomer>("ETagsCustomers").EntityType;
            customer.Property(c => c.DoubleProperty).IsConcurrencyToken();
            customer.Ignore(c => c.StringWithConcurrencyCheckAttributeProperty);
            return builder.GetEdmModel();
        }

        private static IEdmModel GetShortETagEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
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

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;
                Assert.NotNull(result);

                // check the first unchanged,
                // because #0, #1, #2 will change potentially running in parallel by other tests.
                eTag = result[3]["@odata.etag"].ToString();
                Assert.False(String.IsNullOrEmpty(eTag));
                Assert.Equal("W/\"OC4w\"", eTag);

                EntityTagHeaderValue parsedValue;
                Assert.True(EntityTagHeaderValue.TryParse(eTag, out parsedValue));
                IDictionary<string, object> tags = this.ParseETag(parsedValue);
                KeyValuePair<string, object> pair = Assert.Single(tags);
                Single value = Assert.IsType<Single>(pair.Value);
                Assert.Equal((Single)8.0, value);
            }

            var getRequestWithEtag = new HttpRequestMessage(HttpMethod.Get, this.BaseAddress + "/double/ETagsCustomers(3)");
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

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;
                Assert.NotNull(result);

                // check the first unchanged,
                // because #0, #1, #2 will change potentially running in parallel by other tests.
                eTag = result[3]["@odata.etag"].ToString();
                Assert.False(String.IsNullOrEmpty(eTag));
                Assert.Equal("W/\"MzI3NjQ=\"", eTag);

                EntityTagHeaderValue parsedValue;
                Assert.True(EntityTagHeaderValue.TryParse(eTag, out parsedValue));
                IDictionary<string, object> tags = this.ParseETag(parsedValue);
                KeyValuePair<string, object> pair = Assert.Single(tags);
                int value = Assert.IsType<int>(pair.Value);
                Assert.Equal((short)32764, value);
            }

            var getRequestWithEtag = new HttpRequestMessage(HttpMethod.Get, this.BaseAddress + "/short/ETagsCustomers(3)");
            getRequestWithEtag.Headers.IfNoneMatch.ParseAdd(eTag);
            using (var response = await Client.SendAsync(getRequestWithEtag))
            {
                Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
            }
        }

        private IDictionary<string, object> ParseETag(EntityTagHeaderValue etagHeaderValue)
        {
            string tag = etagHeaderValue.Tag.Trim('\"');

            // split etag
            string[] rawValues = tag.Split(',');
            IDictionary<string, object> properties = new Dictionary<string, object>();
            for (int index = 0; index < rawValues.Length; index++)
            {
                string rawValue = rawValues[index];

                // base64 decode
                byte[] bytes = Convert.FromBase64String(rawValue);
                string valueString = Encoding.UTF8.GetString(bytes);
                object obj = ODataUriUtils.ConvertFromUriLiteral(valueString, ODataVersion.V4);
                if (obj is ODataNullValue)
                {
                    obj = null;
                }
                properties.Add(index.ToString(CultureInfo.InvariantCulture), obj);
            }

            return properties;
        }
    }
}
