//-----------------------------------------------------------------------------
// <copyright file="DateTimeTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.DateTimeSupport
{
    [Collection("TimeZoneTests")] // TimeZoneInfo is not thread-safe. Tests in this collection will be executed sequentially 
    public class DateTimeTest : WebHostTestBase
    {
        public DateTimeTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(FilesController), typeof(MetadataController) };
            configuration.AddControllers(controllers);

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"); // -8:00
            configuration.SetTimeZoneInfo(timeZoneInfo);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute(
                routeName: "convention",
                routePrefix: "convention",
                model: DateTimeEdmModel.GetConventionModel(configuration));

            configuration.MapODataServiceRoute(
                routeName: "explicit",
                routePrefix: "explicit",
                model: DateTimeEdmModel.GetExplicitModel(),
                batchHandler: configuration.CreateDefaultODataBatchHandler());

            configuration.EnsureInitialized();
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task ModelBuilderTest(string modelMode)
        {
            string requestUri = string.Format("{0}/{1}/$metadata", this.BaseAddress, modelMode);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            var stream = await response.Content.ReadAsStreamAsync();
            IODataResponseMessage message = new ODataMessageWrapper(stream, response.Content.Headers);
            var reader = new ODataMessageReader(message);
            var edmModel = reader.ReadMetadataDocument();

            var fileType = edmModel.SchemaElements.OfType<IEdmEntityType>().Single(et => et.Name == "File");
            Assert.Equal(5, fileType.Properties().Count());

            var createdDateProperty = fileType.DeclaredProperties.Single(p => p.Name == "CreatedDate");
            Assert.Equal(EdmTypeKind.Primitive, createdDateProperty.Type.TypeKind());
            Assert.Equal("Edm.DateTimeOffset", createdDateProperty.Type.Definition.FullTypeName());
            Assert.False(createdDateProperty.Type.IsNullable);

            var deleteDateProperty = fileType.DeclaredProperties.Single(p => p.Name == "DeleteDate");
            Assert.Equal(EdmTypeKind.Primitive, deleteDateProperty.Type.TypeKind());
            Assert.Equal("Edm.DateTimeOffset", deleteDateProperty.Type.Definition.FullTypeName());
            Assert.True(deleteDateProperty.Type.IsNullable);

            var modifiedDates = fileType.DeclaredProperties.Single(p => p.Name == "ModifiedDates");
            Assert.Equal(EdmTypeKind.Collection, modifiedDates.Type.TypeKind());
            Assert.Equal("Collection(Edm.DateTimeOffset)", modifiedDates.Type.Definition.FullTypeName());
            Assert.False(modifiedDates.Type.IsNullable);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task PostToDateTimeCollection(string modelMode)
        {
            //Arrange
            await ResetDatasource();
            string requestUri = string.Format("{0}/{1}/Files/1/ModifiedDates", this.BaseAddress, modelMode);

            //Get the count of elements in the collection
            int count = 0;
            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;
                count = result.Count;
            }

            //Set up the post request 
            var requestForPost = new HttpRequestMessage(HttpMethod.Post, requestUri);
            requestForPost.Content = new StringContent(content: @"{
                    'value':'2014-10-16T01:02:03Z'
                    }", encoding: Encoding.UTF8, mediaType: "application/json");
            
            //Act & Assert
            using (HttpResponseMessage response = await this.Client.SendAsync(requestForPost))
            {
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }

            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsObject<JObject>();
                var result = json.GetValue("value") as JArray;
                Assert.Equal(count+1,result.Count);
            }
        }

        #region CRUD on DateTime related entity
        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task CreateFileTest(string mode)
        {
            await ResetDatasource();

            string expect = "{" + 
                            "\"@odata.context\":\"{XXXX}\",\"FileId\":6,\"Name\":\"FileName\",\"CreatedDate\":\"2014-12-30T15:01:03-08:00\",\"DeleteDate\":null,\"ModifiedDates\":[" + 
                            "\"2014-12-24T01:02:03-08:00\"" + 
                            "]" +  
                            "}";
            expect = expect.Replace("{XXXX}", string.Format("{0}/{1}/$metadata#Files/$entity", BaseAddress.ToLowerInvariant(), mode));

            string requestUri = string.Format("{0}/{1}/Files", BaseAddress, mode);

            string content = @"{'FileId':0,'Name':'FileName','CreatedDate':'2014-12-31T07:01:03+08:00','ModifiedDates':[]}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(expect, await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task QueryFileEntitySetTest(string mode)
        {
            await ResetDatasource();

            DateTime expect = new DateTime(2014, 12, 24, 1, 2, 3, DateTimeKind.Utc);
            string requestUri = string.Format("{0}/{1}/Files", BaseAddress, mode);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject content = await response.Content.ReadAsObject<JObject>();

            Assert.Equal(5, content["value"].Count());
            for (int i = 1; i <= 5; i++)
            {
                Assert.Equal(new DateTimeOffset(expect.AddYears(i)), content["value"][i - 1]["CreatedDate"]);
            }
        }

        public static TheoryDataSet<string, string> MediaTypes
        {
            get
            {
                string[] modes = new string[] { "convention", "explicit" };
                string[] mimes = new string[]{
                    "json",
                    "application/json",
                    "application/json;odata.metadata=none",
                    "application/json;odata.metadata=minimal",
                    "application/json;odata.metadata=full"};
                TheoryDataSet<string, string> data = new TheoryDataSet<string, string>();
                foreach (string mode in modes)
                {
                    foreach (string mime in mimes)
                    {
                        data.Add(mode, mime);
                    }
                }
                return data;
            }
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryFileEntityTest(string mode, string mime)
        {
            await ResetDatasource();

            string requestUri = string.Format("{0}/{1}/Files(2)?$format={2}", BaseAddress, mode, mime);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject content = await response.Content.ReadAsObject<JObject>();

            Assert.Equal(2, content["FileId"]);
            Assert.Equal("File #2", content["Name"]);
            Assert.Equal(DateTimeOffset.Parse("2016-12-23T17:02:03-08:00"), content["CreatedDate"]);
            Assert.Null((DateTime?)(content["DeleteDate"]));

            Assert.Equal(3, content["ModifiedDates"].Count());
            Assert.Equal(DateTimeOffset.Parse("2014-12-23T17:02:03-08:00"), content["ModifiedDates"][0]);
            Assert.Equal(new DateTimeOffset(new DateTime(2014, 12, 24, 1, 2, 3, DateTimeKind.Local)), content["ModifiedDates"][1]);
            Assert.Equal(DateTimeOffset.Parse("2014-12-24T01:02:03-08:00"), content["ModifiedDates"][2]);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task UpdateFileEntityTestRoundTrip(string mode)
        {
            await ResetDatasource();

            string requestUri = string.Format("{0}/{1}/Files(2)", BaseAddress, mode);

            // GET ~/Files(2)
            var response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"DeleteDate\":null", await response.Content.ReadAsStringAsync());

            // Patch ~/Files(2)
            const string content = @"{'DeleteDate':'2014-12-31T07:01:03+08:00'}";
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri)
            {
                Content = new StringContent(content)
            };
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // GET ~/Files(2)
            response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"DeleteDate\":\"2014-12-30T15:01:03-08:00\"", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task CreateDeleteFileEntityRoundTrip(string mode)
        {
            string requestUri = string.Format("{0}/{1}/Files(6)", BaseAddress, mode);

            // POST ~/Files
            await CreateFileTest(mode);

            // GET ~/Files(6)
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"CreatedDate\":\"2014-12-30T15:01:03-08:00\"", await response.Content.ReadAsStringAsync());

            // Delete ~/Files(6)
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // GET ~/Files(2)
            response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region Query option on DateTime

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task CanSelectDateTimeProperty(string mode)
        {
            await ResetDatasource();

            string requestUri = string.Format("{0}/{1}/Files(3)?$select=CreatedDate", BaseAddress, mode);

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"CreatedDate\":\"2017-12-23T17:02:03-08:00\"", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task CanFilterDateTimeProperty(string mode)
        {
            await ResetDatasource();

            string requestUri = string.Format("{0}/{1}/Files?$filter=year(CreatedDate) eq 2018", BaseAddress, mode);

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject content = await response.Content.ReadAsObject<JObject>();
            Assert.Single(content["value"]);

            Assert.Equal(4, content["value"][0]["FileId"]);
            Assert.Equal("File #4", content["value"][0]["Name"]);
            Assert.Equal(DateTimeOffset.Parse("2018-12-23T17:02:03-08:00"), content["value"][0]["CreatedDate"]);
            Assert.Null((DateTime?)(content["value"][0]["DeleteDate"]));

            Assert.Equal(3, content["value"][0]["ModifiedDates"].Count());
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task CanOrderByDateTimeProperty(string mode)
        {
            await ResetDatasource();

            string requestUri = string.Format("{0}/{1}/Files?$orderby=CreatedDate desc", BaseAddress, mode);

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject content = await response.Content.ReadAsObject<JObject>();

            Assert.Equal(5, content["value"].Count());

            // desc
            for (int i = 5; i >= 1; i--)
            {
                Assert.Equal("File #" + i, content["value"][5 - i]["Name"]);
            }
        }
        #endregion

        #region function/action on DateTime

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task CanCallFunctionOnDateTime(string mode)
        {
            await ResetDatasource();

            string requestUri = string.Format("{0}/{1}/Files/Default.GetFilesModifiedAt(modifiedDate=2014-12-24T09:02:03+08:00)", BaseAddress, mode);

            HttpResponseMessage response = await Client.GetAsync(requestUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject content = await response.Content.ReadAsObject<JObject>();

            Assert.Equal(5, content["value"].Count());
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task CanCallActionOnDateTime(string mode)
        {
            await ResetDatasource();

            string requestUri = string.Format("{0}/{1}/Files(2)/Default.CopyFiles", BaseAddress, mode);
            string content = "{'createdDate':'2014-12-24T09:02:03+06:00'}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(content)
            };

            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            HttpResponseMessage response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"value\":\"2014-12-24T09:02:03+06:00\"", await response.Content.ReadAsStringAsync());
        }

        #endregion
        private async Task<HttpResponseMessage> ResetDatasource()
        {
            var uriReset = this.BaseAddress + "/convention/ResetDataSource";
            var response = await this.Client.PostAsync(uriReset, null);
            return response;
        }
    }
}
