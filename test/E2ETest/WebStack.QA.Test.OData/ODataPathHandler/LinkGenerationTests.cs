﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
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
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.ODataPathHandler
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

    public class LinkGeneration_Model1Controller : ODataController
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
    public class LinkGeneration_Model2Controller : ODataController
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

    public class LinkGenerationTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            var model1 = GetEdmModel1(configuration);
            var model2 = GetEdmModel2(configuration);
            configuration.MapODataServiceRoute("OData1", "v1", model1, new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("OData2", "v2", model2, new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.Routes.MapHttpRoute("ApiDefault", "api/{controller}/{action}/{id}", new { id = RouteParameter.Optional });
        }

        protected static IEdmModel GetEdmModel1(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            mb.EntitySet<LinkGeneration_Model_v1>("LinkGeneration_Model1");
            mb.EntitySet<LinkGeneration_Model_v2>("LinkGeneration_Model2");
            return mb.GetEdmModel();
        }

        protected static IEdmModel GetEdmModel2(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            mb.EntitySet<LinkGeneration_Model_v2>("LinkGeneration_Model2");
            return mb.GetEdmModel();
        }

        [Fact]
        public void GeneratedLinkShouldMatchRequestRouting()
        {
            var content = this.Client.GetStringAsync(this.BaseAddress + "/v1/LinkGeneration_Model1").Result;
            Assert.DoesNotContain(@"/v2/LinkGeneration_Model1", content);

            content = this.Client.GetStringAsync(this.BaseAddress + "/v2/LinkGeneration_Model2").Result;
            Assert.DoesNotContain(@"/v1/LinkGeneration_Model2", content);
        }

        [Theory]
        [InlineData("/v1/LinkGeneration_Model1(1)/NonContainedNavigationProperty", "/v1/LinkGeneration_Model2(2)")]
        public void GeneratedLinkTestForNavigationProperty(string requestUrl, string expectLinkUrl)
        {
            string AcceptJsonFullMetadata = "application/json;odata.metadata=full";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                this.BaseAddress + requestUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(AcceptJsonFullMetadata));
            var content = this.Client.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
            JObject result = JObject.Parse(content);
            Assert.Equal(result["@odata.editLink"].ToString(), this.BaseAddress + expectLinkUrl, StringComparer.OrdinalIgnoreCase);
            Assert.Equal(result["@odata.id"].ToString(), this.BaseAddress + expectLinkUrl, StringComparer.OrdinalIgnoreCase);
        }
    }
}
