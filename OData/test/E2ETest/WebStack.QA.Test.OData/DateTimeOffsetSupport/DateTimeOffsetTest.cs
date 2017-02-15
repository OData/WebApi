using System;
using System.Collections.Generic;
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
using Newtonsoft.Json;
using Nuwa;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.ModelBuilder;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.DateTimeOffsetSupport
{
    [NuwaFramework]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class DateTimeOffsetTest
    {
        #region Configuration and Static Members
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
                model: DateTimeOffsetEdmModel.GetConventionModel());

            configuration.MapODataServiceRoute(
                routeName: "explicit",
                routePrefix: "explicit",
                model: DateTimeOffsetEdmModel.GetExplicitModel(),
                batchHandler: new DefaultODataBatchHandler(httpServer));

            configuration.EnsureInitialized();
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
        #endregion

        #region Helper methods
        private async Task<HttpResponseMessage> ResetDatasource(DateTimeOffset time)
        {
            var uriReset = string.Format("{0}/convention/ResetDataSource/?time={1}", this.BaseAddress, time.ToString("o"));
            var response = await this.Client.PostAsync(uriReset, null);
            return response;
        }

        private File CreateFile(DateTimeOffset createDate)
        {
            return new File() {
                FileId = 0,
                Name = "FileName",
                CreatedDate = createDate,
            };
        }

        private string Serialize(Object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        private File Deserialize(HttpResponseMessage response)
        {
            return JsonConvert.DeserializeObject<File>(response.Content.ReadAsStringAsync().Result);
        }

        private IList<File> DeserializeList(HttpResponseMessage response)
        {
            JObject json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            IList<JToken> value = json["value"].Children().ToList();
            IList<File> files = new List<File>();

            foreach (JToken token in value) 
            {
                var file = JsonConvert.DeserializeObject<File>(token.ToString());
                files.Add(file);
            }

            return files;
        }
        #endregion

        #region CRUD on DateTimeOffset related entity
        [Theory]
        [PropertyData("MediaTypes")]
        public async Task QueryFileEntityTest(string mode, string mime)
        {
            DateTimeOffset serverTime = DateTimeOffset.Now;
            await ResetDatasource(serverTime);

            var files = FilesController.CreateFiles(serverTime);
            for (int i = 1; i <= 5; ++i)
            {
                string requestUri = string.Format("{0}/{1}/Files({2})?$format={3}", BaseAddress, mode, i, mime);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

                var response = await Client.SendAsync(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                File file = Deserialize(response);
                Assert.Equal(files[i-1], file);
            }
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task UpdateFileEntityTestRoundTrip(string mode)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            await ResetDatasource(now);
            string requestUri = string.Format("{0}/{1}/Files(2)", BaseAddress, mode);

            // GET ~/Files(2)
            var response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            File file2 = Deserialize(response);

            var contentObject = new { DeleteDate = now };
            string content = Serialize(contentObject);

            // Patch ~/Files(2)
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // GET ~/Files(2)
            response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            file2.DeleteDate = now;
            File newFile2 = Deserialize(response);
            Assert.Equal(file2, newFile2);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task CreateDeleteFileEntityRoundTrip(string mode)
        {
            DateTimeOffset now = DateTimeOffset.Now;

            await ResetDatasource(now);
            string filesUri = string.Format("{0}/{1}/Files", BaseAddress, mode);
            string fileUri = filesUri + "(6)";

            File newFile = CreateFile(now);
            string content = Serialize(newFile);
            
            // POST ~/Files
            HttpRequestMessage postRequest = new HttpRequestMessage(HttpMethod.Post, filesUri);
            postRequest.Content = new StringContent(content);
            postRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var postResponse = await Client.SendAsync(postRequest);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

            var postResult = Deserialize(postResponse);
            newFile.FileId = postResult.FileId;
            Assert.NotNull(postResult.FileId);
            Assert.Equal(newFile, postResult);

            // GET ~/Files(6)
            HttpResponseMessage getResponse = await Client.GetAsync(fileUri);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getResult = JsonConvert.DeserializeObject<File>(getResponse.Content.ReadAsStringAsync().Result);
            Assert.NotNull(getResult.FileId);
            Assert.Equal(getResult, postResult);

            // Delete ~/Files(6)
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, fileUri);
            var deleteResponse = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // GET ~/Files(6)
            getResponse = this.Client.GetAsync(fileUri).Result;
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
        #endregion

        #region Query option on DateTime

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task CanSelectDateTimeProperty(string mode)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            await ResetDatasource(now);

            string requestUri = string.Format("{0}/{1}/Files(3)?$select=CreatedDate", BaseAddress, mode);

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var dateType = new { CreatedDate = now };
            var date = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, dateType);

            Assert.Equal(date.CreatedDate, now);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task CanFilterDateTimeProperty(string mode)
        {
            DateTimeOffset time =  new DateTimeOffset(2010, 12, 1, 12, 0, 0, TimeSpan.Zero);
            var fileList = FilesController.CreateFiles(time);
            fileList = fileList.GetRange(2, 3);
            await ResetDatasource(time);

            string requestUri = string.Format("{0}/{1}/Files?$filter=year(CreatedDate) eq 2010", BaseAddress, mode);

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseFileList = DeserializeList(response);
            Assert.Equal(3, fileList.Count());
            Assert.True(fileList.SequenceEqual(responseFileList));
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task CanOrderByDateTimeProperty(string mode)
        {
            DateTimeOffset time = DateTimeOffset.Now;
            var fileList = FilesController.CreateFiles(time);
            await ResetDatasource(time);

            string requestUri = string.Format("{0}/{1}/Files?$orderby=CreatedDate ", BaseAddress, mode);

            HttpResponseMessage response = await Client.GetAsync(requestUri + "desc");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseFileList = DeserializeList(response);

            Assert.True(fileList.SequenceEqual(responseFileList));

            response = await Client.GetAsync(requestUri + "asc");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            responseFileList = DeserializeList(response);

            fileList.Reverse();
            Assert.True(fileList.SequenceEqual(responseFileList));
        }
        #endregion

        #region now() Function Tests
        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task Now_FilterDateTimePropertyWithGt(string mode)
        {
            DateTimeOffset time = DateTimeOffset.Now.AddHours(1);
            var fileList = FilesController.CreateFiles(time);
            fileList = fileList.GetRange(0, 3);
            await ResetDatasource(time);

            string requestUri = string.Format("{0}/{1}/Files?$filter=CreatedDate gt now()", BaseAddress, mode);

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseFileList = DeserializeList(response);
            Assert.Equal(3, fileList.Count());
            Assert.True(fileList.SequenceEqual(responseFileList));
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task NowFilterDateTimePropertyWithLt(string mode)
        {
            DateTimeOffset time = DateTimeOffset.Now.AddHours(1);
            var fileList = FilesController.CreateFiles(time);
            fileList = fileList.GetRange(3, 2);
            await ResetDatasource(time);

            string requestUri = string.Format("{0}/{1}/Files?$filter=CreatedDate lt now()", BaseAddress, mode);

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseFileList = DeserializeList(response);
            Assert.Equal(2, fileList.Count());
            Assert.True(fileList.SequenceEqual(responseFileList));
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task NowFilterDateTimePropertyWithDayFunction(string mode)
        {
            DateTimeOffset time = DateTimeOffset.Now;
            var fileList = FilesController.CreateFiles(time);
            await ResetDatasource(time);

            string requestUri = string.Format("{0}/{1}/Files?$filter=day(CreatedDate) eq day(now())", BaseAddress, mode);

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseFileList = DeserializeList(response);
            Assert.Equal(5, fileList.Count());
            Assert.True(fileList.SequenceEqual(responseFileList));
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task NowFilterDateTimePropertyWithMonthFunction(string mode)
        {
            DateTimeOffset time = DateTimeOffset.Now;
            await ResetDatasource(time);
            var fileList = FilesController.CreateFiles(time);

            string requestUri = string.Format("{0}/{1}/Files?$filter=month(CreatedDate) eq month(now())", BaseAddress, mode);

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseFileList = DeserializeList(response);
            Assert.Contains(fileList[2], responseFileList);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task NowFilterDateTimePropertyWithYearFunction(string mode)
        {
            DateTimeOffset time = DateTimeOffset.Now;
            await ResetDatasource(time);
            var fileList = FilesController.CreateFiles(time);

            string requestUri = string.Format("{0}/{1}/Files?$filter=year(CreatedDate) ge year(now())", BaseAddress, mode);

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseFileList = DeserializeList(response);
            Assert.Contains(fileList[2], responseFileList);
            Assert.Contains(fileList[1], responseFileList);
            Assert.Contains(fileList[0], responseFileList);
        }

        #endregion
    }
}
