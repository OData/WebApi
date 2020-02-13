// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ODataPathHandler
{
    public class LinkGeneration_Model_v1
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public LinkGeneration_Model_v2 NonContainedNavigationProperty { get; set; }
    }

    public class LinkGeneration_Model_v2
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class LinkGeneration_Model1Controller : TestODataController
    {
        public IQueryable<LinkGeneration_Model_v1> Get()
        {
            return new LinkGeneration_Model_v1[] 
            { 
                new LinkGeneration_Model_v1
                {
                    ID = 1,
                    Name = "One"
                }
            }.AsQueryable();
        }

        public LinkGeneration_Model_v2 GetNonContainedNavigationProperty(int key)
        {
            return new LinkGeneration_Model_v2
            {
                ID = 2,
                Name = "Test2"
            };
        }
    }
    public class LinkGeneration_Model2Controller : TestODataController
    {
        public IQueryable<LinkGeneration_Model_v2> Get()
        {
            return new LinkGeneration_Model_v2[] 
            { 
                new LinkGeneration_Model_v2
                {
                    ID = 1,
                    Name = "One"
                }
            }.AsQueryable();
        }
    }

    public class LinkGenerationTests : WebHostTestBase<LinkGenerationTests>
    {
        public LinkGenerationTests(WebHostTestFixture<LinkGenerationTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            var model1 = GetEdmModel1(configuration);
            var model2 = GetEdmModel2(configuration);
            configuration.MapODataServiceRoute("OData1", "v1", model1, new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("OData2", "v2", model2, new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
#if NETCORE
            configuration.MapHttpRoute("ApiDefault", "api/{controller}/{action}/{id?}");
#else
            configuration.MapHttpRoute("ApiDefault", "api/{controller}/{action}/{id}", new { id = System.Web.Http.RouteParameter.Optional });
#endif
        }

        protected static IEdmModel GetEdmModel1(WebRouteConfiguration configuration)
        {
            var mb = configuration.CreateConventionModelBuilder();
            mb.EntitySet<LinkGeneration_Model_v1>("LinkGeneration_Model1");
            mb.EntitySet<LinkGeneration_Model_v2>("LinkGeneration_Model2");
            return mb.GetEdmModel();
        }

        protected static IEdmModel GetEdmModel2(WebRouteConfiguration configuration)
        {
            var mb = configuration.CreateConventionModelBuilder();
            mb.EntitySet<LinkGeneration_Model_v2>("LinkGeneration_Model2");
            return mb.GetEdmModel();
        }

        [Fact]
        public async Task GeneratedLinkShouldMatchRequestRouting()
        {
            var content = await this.Client.GetStringAsync(this.BaseAddress + "/v1/LinkGeneration_Model1");
            Assert.DoesNotContain(@"/v2/LinkGeneration_Model1", content);

            content = await this.Client.GetStringAsync(this.BaseAddress + "/v2/LinkGeneration_Model2");
            Assert.DoesNotContain(@"/v1/LinkGeneration_Model2", content);
        }

        [Theory]
        [InlineData("/v1/LinkGeneration_Model1(1)/NonContainedNavigationProperty", "/v1/LinkGeneration_Model2(2)")]
        public async Task GeneratedLinkTestForNavigationProperty(string requestUrl, string expectLinkUrl)
        {
            string AcceptJsonFullMetadata = "application/json;odata.metadata=full";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                this.BaseAddress + requestUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(AcceptJsonFullMetadata));
            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(content);
            Assert.Equal(result["@odata.editLink"].ToString(), this.BaseAddress + expectLinkUrl, StringComparer.OrdinalIgnoreCase);
            Assert.Equal(result["@odata.id"].ToString(), this.BaseAddress + expectLinkUrl, StringComparer.OrdinalIgnoreCase);
        }
    }
}
