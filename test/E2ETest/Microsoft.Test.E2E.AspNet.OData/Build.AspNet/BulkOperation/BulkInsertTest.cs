// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
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

namespace Microsoft.Test.E2E.AspNet.OData.BulkInsert1
{
    public class BulkInsertTest : WebHostTestBase
    {
        public BulkInsertTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(EmployeesController), typeof(MetadataController) };
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("convention", "convention", BulkInsertEdmModel.GetConventionModel(configuration));
            configuration.MapODataServiceRoute("explicit", "explicit", BulkInsertEdmModel.GetExplicitModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.EnsureInitialized();
        }


        #region Query

        [Theory]
        [InlineData("/convention/Employees/$count", 3)]
        [InlineData("/convention/Employees/$count?$filter=Name eq 'Name1'", 1)]
        public async Task QueryEntitySetCount(string url, int expectedCount)
        {
            // Arrange
            await ResetDatasource();
            string requestUri = this.BaseAddress + url;

            // Act
            HttpResponseMessage response = await this.Client.GetAsync(requestUri);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            string count = await response.Content.ReadAsStringAsync();
            Assert.Equal<int>(expectedCount, int.Parse(count));
        }

      
        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryEnumPropertyValueInEntityType(string format)
        {
            await ResetDatasource();
            var requestUri = this.BaseAddress + "/convention/Employees(1)/AccessLevel/$value?$format=" + format;
            var response = await this.Client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            AccessLevel actual;
            Assert.True(Enum.TryParse<AccessLevel>(content, out actual));
            Assert.Equal(AccessLevel.Execute, actual);
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryEnumPropertyInComplexType(string format)
        {
            await ResetDatasource();
            string requestUri = this.BaseAddress + "/convention/Employees(1)/FavoriteSports?$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.True(response.IsSuccessStatusCode);

            var result = await response.Content.ReadAsObject<JObject>();
            var value = result.GetValue("LikeMost").ToString();
            Assert.Equal("Pingpong", value);
            value = result.GetValue("Like").ToString();
            Assert.Equal(@"[""Pingpong"",""Basketball""]", value.Replace("\r\n", "").Replace(" ", ""));
            if (format == "application/json;odata.metadata=full")
            {
                var context = result.GetValue("@odata.context").ToString();
                Assert.True(context.IndexOf("/$metadata#Employees(1)/FavoriteSports") > 0);
            }

            requestUri = this.BaseAddress + "/convention/Employees(1)/FavoriteSports/LikeMost?$format=" + format;
            response = await this.Client.GetAsync(requestUri);
            result = await response.Content.ReadAsObject<JObject>();
            value = result.GetValue("value").ToString();
            Assert.Equal("Pingpong", value);
        }

    
        #endregion

        #region Update

       
        [Fact]
        public async Task PostToEnumCollection()
        {
            //Arrange
            await ResetDatasource();
            string requestUri = this.BaseAddress + "/convention/Employees/2/SkillSet?$format=application/json;odata.metadata=none";
            //Get the count before the post
            int count = 0;
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;
                count = result.Count;
            }

            //Set up the post request
            var requestForPost = new HttpRequestMessage(HttpMethod.Post, requestUri);
            requestForPost.Content = new StringContent(content: @"{
                    'value':'Sql'
                    }", encoding: Encoding.UTF8, mediaType: "application/json");

            //Act
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }

            //Assert
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;
                Assert.True(count + 1 == result.Count,
                    String.Format("\nExpected count: {0},\n actual: {1},\n request uri: {2}",
                    count + 1,
                    result.Count,
                    requestUri));
            }
        }
                   
        [Fact]
        public async Task PatchEmployee_WithUpdates()
        {
            //Arrange
            await ResetDatasource();
            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'Sql'  ,
                    'Friends@odata.delta':[{'Id':1,'Name':'Test2'},{'Id':2,'Name':'Test3'}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Assert
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;

                Assert.Equal(2, result.Count);
                Assert.Contains("Test2", result.ToString());
            }

        }

        [Fact]
        public async Task PatchEmployee_WithDelete()
        {
            //Arrange
            await ResetDatasource();
            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'Sql'  ,
                    'Friends@odata.delta':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Assert
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;

                Assert.Single(result);
                Assert.DoesNotContain("Test0", result.ToString());
            }
        }


        [Fact]
        public async Task PatchEmployee_WithAddUpdateAndDelete()
        {
            //Arrange
            await ResetDatasource();
            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'Sql'  ,
                    'Friends@odata.delta':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{'Id':2,'Name':'Test3'},{'Id':3,'Name':'Test4'}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Assert
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;

                Assert.Equal(2, result.Count);
                Assert.DoesNotContain("Test0", result.ToString());
                Assert.Contains("Test3", result.ToString());
                Assert.Contains("Test4", result.ToString());
            }
        }


        [Fact]
        public async Task PatchEmployee_WithMultipleUpdatesinOrder1()
        {
            //Arrange
            await ResetDatasource();
            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'Sql'  ,
                    'Friends@odata.delta':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{'Id':1,'Name':'Test_1'},{'Id':2,'Name':'Test3'},{'Id':3,'Name':'Test4'}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Assert
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;

                Assert.Equal(3, result.Count);
                Assert.DoesNotContain("Test0", result.ToString());
                Assert.Contains("Test_1", result.ToString());
                Assert.Contains("Test3", result.ToString());
                Assert.Contains("Test4", result.ToString());
            }
        }



        [Fact]
        public async Task PatchEmployee_WithMultipleUpdatesinOrder2()
        {
            //Arrange
            await ResetDatasource();
            string requestUri = this.BaseAddress + "/convention/Employees(1)";

            var content = @"{
                    'Name':'Sql'  ,
                    'Friends@odata.delta':[{ '@odata.removed' : {'reason':'changed'}, 'Id':1},{'Id':1,'Name':'Test_1'},{'Id':2,'Name':'Test3'},{'Id':3,'Name':'Test4'},{ '@odata.removed' : {'reason':'changed'}, 'Id':1}]
                     }";

            var requestForPost = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);

            StringContent stringContent = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json");
            requestForPost.Content = stringContent;

            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            //Assert
            requestUri = this.BaseAddress + "/convention/Employees(1)/Friends";
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;

                Assert.Equal(2, result.Count);
                Assert.DoesNotContain("Test0", result.ToString());
                Assert.DoesNotContain("Test_1", result.ToString());
                Assert.Contains("Test3", result.ToString());
                Assert.Contains("Test4", result.ToString());
            }
        }


        [Fact]
        public async Task UpdateEntity()
        {
            await ResetDatasource();
            string getUri = this.BaseAddress + "/convention/Employees(2)?$format=application/json;odata.metadata=none";

            using (HttpResponseMessage response = await Client.GetAsync(getUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var accessLevel = json.GetValue("AccessLevel").ToString();
                Assert.Equal("Read", accessLevel);

                var skillSet = json.GetValue("SkillSet").ToString();
                Assert.Equal("[]", skillSet);

                var favoriteSport = json["FavoriteSports"]["LikeMost"].ToString();
                Assert.Equal("Pingpong", favoriteSport);

                var sports = json["FavoriteSports"]["Like"].ToString();
                Assert.Equal(@"[""Pingpong"",""Basketball""]", sports.Replace("\r\n", "").Replace(" ", ""));
            }

            var putUri = this.BaseAddress + "/convention/Employees(2)";
            var putContent = JObject.Parse(@"{""ID"":2,
                    ""Name"":""Name2"",
                    ""SkillSet"":[""Sql""],
                    ""Gender"":""Female"",
                    ""AccessLevel"":""Execute,Write"",
                    ""FavoriteSports"":{
                            ""LikeMost"":""Basketball"",
                            ""Like"":[""Pingpong"",""Basketball""]
                    }}");
            using (HttpResponseMessage response = await Client.PutAsJsonAsync(putUri, putContent))
            {
                response.EnsureSuccessStatusCode();
            }

            using (HttpResponseMessage response = await Client.GetAsync(getUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();

                var accessLevel = json.GetValue("AccessLevel");
                Assert.Equal("Write, Execute", accessLevel);

                var skillSet = json.GetValue("SkillSet").ToString();
                Assert.Equal(@"[""Sql""]", skillSet.Replace("\r\n", "").Replace(" ", ""));

                var favoriteSport = json["FavoriteSports"]["LikeMost"].ToString();
                Assert.Equal("Basketball", favoriteSport);

                var sports = json["FavoriteSports"]["Like"].ToString();
                Assert.Equal(@"[""Pingpong"",""Basketball""]", sports.Replace("\r\n", "").Replace(" ", ""));
            }
        }

        [Theory]
        [InlineData("PUT")]
        [InlineData("PATCH")]
        public async Task UpsertEntity(string method)
        {
            await ResetDatasource();

            var requestUri = this.BaseAddress + "/convention/Employees(20)";
            var requestContent = JObject.Parse(@"{""ID"":20,
                    ""Name"":""Name2"",
                    ""SkillSet"":[""Sql""],
                    ""Gender"":""Female"",
                    ""AccessLevel"":""Execute,Write"",
                    ""FavoriteSports"":{
                            ""LikeMost"":""Basketball"",
                            ""Like"":[""Pingpong"",""Basketball""]
                    }}");
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), requestUri);
            request.Content = new StringContent(requestContent.ToString());
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Headers.Add("Prefer", "return=minimal");
            using (HttpResponseMessage response = await Client.SendAsync(request))
            {
                Assert.True(HttpStatusCode.NoContent == response.StatusCode,
                    string.Format("Response code is not right, expected: {0}, actual: {1}", HttpStatusCode.NoContent, response.StatusCode));
                Assert.True(response.Headers.Contains("OData-EntityId"), "The response should contain Header 'OData-EntityId'");
                Assert.True(response.Headers.Contains("Location"), "The response should contain Header 'Location'");
                Assert.True(response.Headers.Contains("OData-Version"), "The response should contain Header 'OData-Version'");
            }
        }

        #endregion

        #region Delete

        [Fact]
        public async Task DeleteEntity()
        {
            await ResetDatasource();
            string uriGet = this.BaseAddress + "/convention/Employees?$format=application/json;odata.metadata=none";

            using (HttpResponseMessage response = await Client.GetAsync(uriGet))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var values = json.GetValue("value") as JArray;
                Assert.Equal<int>(3, values.Count);
            }

            var uriDelete = this.BaseAddress + "/convention/Employees(1)";
            using (HttpResponseMessage response = await Client.DeleteAsync(uriDelete))
            {
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }

            using (HttpResponseMessage response = await Client.GetAsync(uriGet))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var values = json.GetValue("value") as JArray;
                Assert.Equal<int>(2, values.Count);
            }
        }

        #endregion

        #region Enum with action

        [Fact]
        public async Task EnumInActionOutput()
        {
            await ResetDatasource();
            var postUri = this.BaseAddress + "/convention/SetAccessLevel";
            var postContent = JObject.Parse(@"{""accessLevel"":""Read,Execute"",""ID"":1}");

            var response = await Client.PostAsJsonAsync(postUri, postContent);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsObject<JObject>();
            var value = json["value"].ToString();

            Assert.Equal("Read, Execute", value);
        }

        #endregion

        #region Enum with function



        #endregion

        private async Task<HttpResponseMessage> ResetDatasource()
        {
            var uriReset = this.BaseAddress + "/convention/ResetDataSource";
            var response = await this.Client.PostAsync(uriReset, null);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            return response;
        }
    }
}