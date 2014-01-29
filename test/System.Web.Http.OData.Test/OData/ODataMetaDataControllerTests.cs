// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Formatter;
using System.Web.Http.Tracing;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder
{
    public class ODataMetaDataControllerTests
    {
        [Fact]
        public void DollarMetaData_Works_WithoutAcceptHeader()
        {
            HttpServer server = new HttpServer();
            server.Configuration.Routes.MapODataServiceRoute(ODataTestUtil.GetEdmModel());

            HttpClient client = new HttpClient(server);
            var response = client.GetAsync("http://localhost/$metadata").Result;

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains("<edmx:Edmx", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void GetMetadata_Returns_EdmModelFromRequest()
        {
            IEdmModel model = new EdmModel();

            ODataMetadataController controller = new ODataMetadataController();
            controller.Request = new HttpRequestMessage();
            controller.Request.ODataProperties().Model = model;

            IEdmModel responseModel = controller.GetMetadata();
            Assert.Equal(model, responseModel);
        }

        [Fact]
        public void GetMetadata_Throws_IfModelIsNotSetOnRequest()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            ODataMetadataController controller = new ODataMetadataController();
            controller.Request = new HttpRequestMessage();

            Assert.Throws<InvalidOperationException>(() => controller.GetMetadata(),
                "The request must have an associated EDM model. Consider using the extension method " +
                "HttpConfiguration.Routes.MapODataServiceRoute to register a route that parses the OData URI and " +
                "attaches the model information.");
        }

        [Fact]
        public void DollarMetaDataWorks_AfterTracingIsEnabled()
        {
            HttpServer server = new HttpServer();
            server.Configuration.Routes.MapODataServiceRoute(ODataTestUtil.GetEdmModel());
            server.Configuration.Services.Replace(typeof(ITraceWriter), new Mock<ITraceWriter>().Object);

            HttpClient client = new HttpClient(server);
            var response = client.GetAsync("http://localhost/$metadata").Result;

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains("<edmx:Edmx", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void DollarMetadata_Works_WithMultipleModels()
        {
            ODataConventionModelBuilder builder1 = new ODataConventionModelBuilder();
            builder1.EntitySet<FormatterPerson>("People1");
            var model1 = builder1.GetEdmModel();

            ODataConventionModelBuilder builder2 = new ODataConventionModelBuilder();
            builder2.EntitySet<FormatterPerson>("People2");
            var model2 = builder2.GetEdmModel();

            HttpServer server = new HttpServer();
            server.Configuration.Routes.MapODataServiceRoute("OData1", "v1", model1);
            server.Configuration.Routes.MapODataServiceRoute("OData2", "v2", model2);

            HttpClient client = new HttpClient(server);
            AssertHasEntitySet(client, "http://localhost/v1/$metadata", "People1");
            AssertHasEntitySet(client, "http://localhost/v2/$metadata", "People2");
        }

        private static void AssertHasEntitySet(HttpClient client, string uri, string entitySetName)
        {
            var response = client.GetAsync(uri).Result;
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains(entitySetName, response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void ServiceDocumentWorks_AfterTracingIsEnabled_IfModelIsSetOnConfiguration()
        {
            HttpServer server = new HttpServer();
            server.Configuration.Routes.MapODataServiceRoute(ODataTestUtil.GetEdmModel());
            server.Configuration.Services.Replace(typeof(ITraceWriter), new Mock<ITraceWriter>().Object);

            HttpClient client = new HttpClient(server);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            var response = client.SendAsync(request).Result;

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/atomsvc+xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains("<workspace>", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Controller_DoesNotAppear_InApiDescriptions()
        {
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{action}");
            config.Routes.MapODataServiceRoute(new ODataConventionModelBuilder().GetEdmModel());
            var explorer = config.Services.GetApiExplorer();

            var apis = explorer.ApiDescriptions.Select(api => api.ActionDescriptor.ControllerDescriptor.ControllerName);

            Assert.DoesNotContain("ODataMetadata", apis);
        }

        [Fact]
        public void GetMetadata_Doesnot_Change_DataServiceVersion()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            model.SetDataServiceVersion(new Version(0, 42));

            ODataMetadataController controller = new ODataMetadataController();
            controller.Request = new HttpRequestMessage();
            controller.Request.ODataProperties().Model = model;

            // Act
            IEdmModel controllerModel = controller.GetMetadata();

            // Assert
            Assert.Equal(new Version(0, 42), controllerModel.GetDataServiceVersion());
        }
    }
}
