// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.InstanceAnnotations
{
    public class AnnotationTest : WebHostTestBase
    {
        public AnnotationTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(EmployeesController), typeof(MetadataController) };
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("convention", "convention", AnnotationEdmModel.GetConventionModel(configuration));
            configuration.MapODataServiceRoute("explicit", "explicit", AnnotationEdmModel.GetExplicitModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.EnsureInitialized();
        }

        #region InstanceAnnotation



        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task InstanceAnnotationOnTypeTest(string format)
        {
            await ResetDatasource();
            string postUri = this.BaseAddress + "/convention/Employees?$format="+format;

            var postContent = JObject.Parse(@"{""ID"":1,
                    ""Name"":""Name1"",
                    ""SkillSet"":[""Sql""],
                    ""Gender"":""Female"",
                    ""AccessLevel"":""Read,Write"",
                    ""FavoriteSports"":{
                            ""LikeMost"":""Pingpong"",
                            ""Like"":[""Pingpong"",""Basketball""]
                    },
                    ""@NS.Test"":1}");

            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            using (HttpResponseMessage response = await this.Client.PostAsJsonAsync(postUri, postContent))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);

                var json = await response.Content.ReadAsObject<JObject>();
                VerifyInstanceAnnotations("Name1", json.ToString());
            }

        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task InstanceAnnotationOnPropertyTest(string format)
        {
            await ResetDatasource();
            string postUri = this.BaseAddress + "/convention/Employees?$format=" + format;

            var postContent = JObject.Parse(@"{""ID"":1,                                       
                    ""Name"":""Name2"",
                    ""SkillSet"":[""Sql""],
                    ""Gender@NS.TestGender"":100,
                    ""Gender"":""Female"",
                    ""AccessLevel"":""Read,Write"",
                    ""FavoriteSports"":{
                            ""LikeMost"":""Pingpong"",
                            ""Like"":[""Pingpong"",""Basketball""]
                    },
                    }");

            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");

            using (HttpResponseMessage response = await this.Client.PostAsJsonAsync(postUri, postContent))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var json = await response.Content.ReadAsObject<JObject>();
                VerifyInstanceAnnotations("Name2", json.ToString());
            }
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task InstanceAnnotationOnTypeAndPropertyTest(string format)
        {
            await ResetDatasource();
            string postUri = this.BaseAddress + "/convention/Employees?$format=" + format;

            var postContent = JObject.Parse(@"{""ID"":1,                                       
                    ""Name"":""Name3"",
                    ""SkillSet"":[""Sql""],
                    ""Gender@NS.TestGender"":100,
                    ""Gender"":""Female"",
                    ""AccessLevel"":""Read,Write"",
                    ""FavoriteSports"":{
                            ""LikeMost"":""Pingpong"",
                            ""Like"":[""Pingpong"",""Basketball""]
                    },
                    ""@NS.Test"":1}");

            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");
            using (HttpResponseMessage response = await this.Client.PostAsJsonAsync(postUri, postContent))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var json = await response.Content.ReadAsObject<JObject>();
                VerifyInstanceAnnotations("Name3", json.ToString());
            }
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task InstanceAnnotationMultipleTest(string format)
        {
            await ResetDatasource();
            string postUri = this.BaseAddress + "/convention/Employees?$format=" + format;

            var postContent = JObject.Parse(@"{""ID"":1,                                       
                    ""Name@NS.TestName"":""TestName1"",                    
                    ""Name"":""Name4"",
                    ""SkillSet"":[""Sql""],
                    ""Gender@NS.TestGender"":500,
                    ""Gender"":""Female"",
                    ""AccessLevel"":""Read,Write"",
                    ""FavoriteSports"":{
                            ""LikeMost"":""Pingpong"",
                            ""Like"":[""Pingpong"",""Basketball""]
                    },
                    ""@NS.Test1"":100,
                    ""@NS.Test2"":""Testing""}");

            Client.DefaultRequestHeaders.Add("Prefer", @"odata.include-annotations=""*""");
            using (HttpResponseMessage response = await this.Client.PostAsJsonAsync(postUri, postContent))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var json = await response.Content.ReadAsObject<JObject>();
                VerifyInstanceAnnotations("Name4", json.ToString());
            }
        }
        #endregion


        private async Task<HttpResponseMessage> ResetDatasource()
        {
            var uriReset = this.BaseAddress + "/convention/ResetDataSource";
            var response = await this.Client.PostAsync(uriReset, null);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            return response;
        }

        private void VerifyInstanceAnnotations(string name, string json)
        {
            switch (name)
            {
                case "Name1":
                    Assert.Contains(@"""@NS.Test"": 1", json);
                    break;
                case "Name2":
                    Assert.Contains(@"""Gender@NS.TestGender"": 100", json);
                    break;
                case "Name3":
                    Assert.Contains(@"""@NS.Test"": 1", json);
                    Assert.Contains(@"""Gender@NS.TestGender"": 100", json);
                    break;
                case "Name4":
                    Assert.Contains(@"""@NS.Test1"": 100", json);
                    Assert.Contains(@"""Gender@NS.TestGender"": 500", json);
                    Assert.Contains(@"""@NS.Test2"": ""Testing""", json);
                    Assert.Contains(@"""Name@NS.TestName"": ""TestName1""", json);
                    break;
                default:
                    break;
            }
        }
    }
}