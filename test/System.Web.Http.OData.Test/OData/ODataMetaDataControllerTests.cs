// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Formatter;
using System.Web.Http.Tracing;
using Microsoft.Data.Edm;
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
            ODataMediaTypeFormatter odataFormatter = new ODataMediaTypeFormatter(ODataTestUtil.GetEdmModel());
            server.Configuration.Formatters.Insert(0, odataFormatter);
            server.Configuration.Routes.MapHttpRoute(ODataRouteNames.Metadata, "$metadata", new { Controller = "ODataMetadata", Action = "GetMetadata" });

            HttpClient client = new HttpClient(server);
            var response = client.GetAsync("http://localhost/$metadata").Result;

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(response.Content.Headers.ContentType.MediaType, "application/xml");
            Assert.Contains("<edmx:Edmx", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void GetMetadata_Returns_EdmModelFromSetODataFormatter()
        {
            IEdmModel model = new EdmModel();
            ODataMediaTypeFormatter oDataFormatter = new ODataMediaTypeFormatter(model);
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Formatters.Insert(0, oDataFormatter);

            ODataMetadataController controller = new ODataMetadataController();
            controller.Request = new HttpRequestMessage();
            controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            IEdmModel responseModel;
            Assert.True(controller.GetMetadata().TryGetContentValue<IEdmModel>(out responseModel));
            Assert.Equal(model, responseModel);
        }

        [Fact]
        public void GetMetadata_Throws_IfModelIsNotSetOnConfiguration_And_ODataFormatterIsNotPresent()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            ODataMetadataController controller = new ODataMetadataController();
            controller.Request = new HttpRequestMessage();
            controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            Assert.Throws<InvalidOperationException>(
                () => controller.GetMetadata(),
                "No OData formatter was found to write the OData metadata. Consider registering an appropriate ODataMediaTypeFormatter on the configuration's formatter collection.");
        }

        [Fact]
        public void DollarMetaDataWorks_AfterTracingIsEnabled_IfModelIsSetOnConfiguration()
        {
            HttpServer server = new HttpServer();
            ODataMediaTypeFormatter odataFormatter = new ODataMediaTypeFormatter(ODataTestUtil.GetEdmModel());
            server.Configuration.Formatters.Insert(0, odataFormatter);
            server.Configuration.Routes.MapHttpRoute(ODataRouteNames.Metadata, "$metadata", new { Controller = "ODataMetadata", Action = "GetMetadata" });
            server.Configuration.Services.Replace(typeof(ITraceWriter), new Mock<ITraceWriter>().Object);

            HttpClient client = new HttpClient(server);
            var response = client.GetAsync("http://localhost/$metadata").Result;

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(response.Content.Headers.ContentType.MediaType, "application/xml");
            Assert.Contains("<edmx:Edmx", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void ServiceDocumentWorks_AfterTracingIsEnabled_IfModelIsSetOnConfiguration()
        {
            HttpServer server = new HttpServer();
            ODataMediaTypeFormatter odataFormatter = new ODataMediaTypeFormatter(ODataTestUtil.GetEdmModel());
            server.Configuration.Formatters.Insert(0, odataFormatter);
            server.Configuration.Routes.MapHttpRoute(ODataRouteNames.ServiceDocument, "", new { Controller = "ODataMetadata", Action = "GetServiceDocument" });
            server.Configuration.Services.Replace(typeof(ITraceWriter), new Mock<ITraceWriter>().Object);

            HttpClient client = new HttpClient(server);
            var response = client.GetAsync("http://localhost/").Result;

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(response.Content.Headers.ContentType.MediaType, "application/xml");
            Assert.Contains("<workspace>", response.Content.ReadAsStringAsync().Result);
        }
    }
}
