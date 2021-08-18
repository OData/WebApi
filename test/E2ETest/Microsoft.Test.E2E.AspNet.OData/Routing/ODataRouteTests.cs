//-----------------------------------------------------------------------------
// <copyright file="ODataRouteTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Routing
{
    public class ODataRouteTests : WebHostTestBase
    {
        public ODataRouteTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.MapODataServiceRoute("noPrefix", "", GetEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("prefix", "prefix", GetEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("oneParameterInPrefix", "{a}", GetEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataModelBuilder builder = configuration.CreateConventionModelBuilder();
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("")]
        [InlineData("prefix/")]
        [InlineData("parameter/")]
        public async Task UrlsGeneratedByFastPathAreConsistentWithUrlsGeneratedWithSlowPath(string requestPath)
        {
            Uri serviceUrl = new Uri(BaseAddress + "/" + requestPath);
            var request = new HttpRequestMessage(HttpMethod.Get, serviceUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            var response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string content = await response.Content.ReadAsStringAsync();

            Assert.Contains("odata.context\":\"" + serviceUrl + "$metadata\"", content);
        }
    }
}
