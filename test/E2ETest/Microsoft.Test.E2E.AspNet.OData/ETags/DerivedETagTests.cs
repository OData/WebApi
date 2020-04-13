// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.Http;
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
    public class DerivedETagTests : WebHostTestBase<DerivedETagTests>
    {
        public DerivedETagTests(WebHostTestFixture<DerivedETagTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("derivedEtag", "derivedEtag", GetDerivedEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.AddETagMessageHandler(new ETagMessageHandler());
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
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

        private static IEdmModel GetDerivedEdmModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
            EntitySetConfiguration<ETagsCustomer> eTagsCustomersSet = builder.EntitySet<ETagsCustomer>("ETagsCustomers");
            eTagsCustomersSet.HasRequiredBinding(c => c.RelatedCustomer, eTagsCustomersSet);
            eTagsCustomersSet.HasRequiredBinding(c => c.ContainedCustomer, eTagsCustomersSet);
            EntitySetConfiguration<ETagsDerivedCustomer> eTagsDerivedCustomersSet = builder.EntitySet<ETagsDerivedCustomer>("ETagsDerivedCustomers");
            eTagsDerivedCustomersSet.HasRequiredBinding(c => c.RelatedCustomer, eTagsCustomersSet);
            eTagsDerivedCustomersSet.HasRequiredBinding(c => c.ContainedCustomer, eTagsCustomersSet);
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task DerivedTypesHaveSameETagsTest()
        {
            string requestUri = this.BaseAddress + "/odata/ETagsCustomers?$select=Id";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json");
            HttpResponseMessage response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);
            var jsonResult = await response.Content.ReadAsObject<JObject>();
            var jsonETags = jsonResult.GetValue("value").Select(e => e["@odata.etag"].ToString());

            requestUri = this.BaseAddress + "/odata/ETagsDerivedCustomers?$select=Id";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json");
            response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);
            jsonResult = await response.Content.ReadAsObject<JObject>();
            var derivedEtags = jsonResult.GetValue("value").Select(e => e["@odata.etag"].ToString());
            
            Assert.True(String.Concat(jsonETags) == String.Concat(derivedEtags), "Derived Types has different etags than base type");
        }

        [Fact]
        public async Task SingletonsHaveSameETagsTest()
        {
            string requestUri = this.BaseAddress + "/odata/ETagsCustomers?$select=Id";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json");
            HttpResponseMessage response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);
            var jsonResult = await response.Content.ReadAsObject<JObject>();
            var jsonETags = jsonResult.GetValue("value").Select(e => e["@odata.etag"].ToString());

            requestUri = this.BaseAddress + "/odata/ETagsDerivedCustomersSingleton?$select=Id";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json");
            response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);
            jsonResult = await response.Content.ReadAsObject<JObject>();
            var singletonEtag = jsonResult.GetValue("@odata.etag").ToString();

            Assert.True(jsonETags.FirstOrDefault() == singletonEtag, "Singleton has different etags than Set");
        }

        [Fact]
        public async Task DerivedEntitySetsHaveETagsTest()
        {
            string requestUri = this.BaseAddress + "/derivedEtag/ETagsDerivedCustomers?$select=Id";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json");
            HttpResponseMessage response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);
            var jsonResult = await response.Content.ReadAsObject<JObject>();
            var derivedEtags = jsonResult.GetValue("value").Select(e => e["@odata.etag"].ToString());
            Assert.Equal(10, derivedEtags.Count());
            Assert.Equal("W/\"bnVsbA==\"", derivedEtags.First());
        }
    }
}
