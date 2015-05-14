using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData;
using System.Web.OData.Batch;
using System.Web.OData.Extensions;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.ModelBuilder;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.DateTimeSupport
{
    [NuwaFramework]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class DateTimeTest
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { typeof(FilesController), typeof(MetadataController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"); // -8:00
            configuration.SetTimeZoneInfo(timeZoneInfo);

            configuration.Routes.Clear();
            HttpServer httpServer = configuration.GetHttpServer();
            configuration.MapODataServiceRoute(
                routeName: "convention",
                routePrefix: "convention",
                model: DateTimeEdmModel.GetConventionModel());

            configuration.MapODataServiceRoute(
                routeName: "explicit",
                routePrefix: "explicit",
                model: DateTimeEdmModel.GetExplicitModel(),
                batchHandler: new DefaultODataBatchHandler(httpServer));

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

        #region CRUD on DateTime related entity
        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task CreateFileTest(string mode)
        {
            await ResetDatasource();

            string expect = "{\r\n" + 
                            "  \"@odata.context\":\"{XXXX}\",\"FileId\":6,\"Name\":\"FileName\",\"CreatedDate\":\"2014-12-30T15:01:03-08:00\",\"DeleteDate\":null,\"ModifiedDates\":[\r\n" + 
                            "    \"2014-12-24T01:02:03-08:00\"\r\n" + 
                            "  ]\r\n" +  
                            "}";
            expect = expect.Replace("{XXXX}", string.Format("{0}/{1}/$metadata#Files/$entity", BaseAddress.ToLowerInvariant(), mode));

            string requestUri = string.Format("{0}/{1}/Files", BaseAddress, mode);

            string content = @"{'FileId':0,'Name':'FileName','CreatedDate':'2014-12-31T07:01:03+08:00','ModifiedDates':[]}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(expect, response.Content.ReadAsStringAsync().Result);
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
            JObject content = await response.Content.ReadAsAsync<JObject>();

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
        [PropertyData("MediaTypes")]
        public async Task QueryFileEntityTest(string mode, string mime)
        {
            await ResetDatasource();

            string requestUri = string.Format("{0}/{1}/Files(2)?$format={2}", BaseAddress, mode, mime);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject content = await response.Content.ReadAsAsync<JObject>();

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
            Assert.Contains("\"DeleteDate\":null", response.Content.ReadAsStringAsync().Result);

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
            Assert.Contains("\"DeleteDate\":\"2014-12-30T15:01:03-08:00\"", response.Content.ReadAsStringAsync().Result);
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
            Assert.Contains("\"CreatedDate\":\"2014-12-30T15:01:03-08:00\"", response.Content.ReadAsStringAsync().Result);

            // Delete ~/Files(6)
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // GET ~/Files(2)
            response = this.Client.GetAsync(requestUri).Result;
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
            Assert.Contains("\"CreatedDate\":\"2017-12-23T17:02:03-08:00\"", response.Content.ReadAsStringAsync().Result);
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

            JObject content = await response.Content.ReadAsAsync<JObject>();
            Assert.Equal(1, content["value"].Count());

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

            JObject content = await response.Content.ReadAsAsync<JObject>();

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

            JObject content = await response.Content.ReadAsAsync<JObject>();

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
            Assert.Contains("\"value\":\"2014-12-24T09:02:03+06:00\"", response.Content.ReadAsStringAsync().Result);
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
