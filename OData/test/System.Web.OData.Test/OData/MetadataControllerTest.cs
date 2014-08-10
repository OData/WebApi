// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Tracing;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder
{
    public class MetadataControllerTest
    {
        [Fact]
        public void DollarMetaData_Works_WithoutAcceptHeader()
        {
            // Arrange
            HttpServer server = new HttpServer(GetConfiguration());
            HttpClient client = new HttpClient(server);

            // Act
            var response = client.GetAsync("http://localhost/$metadata").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains("<edmx:Edmx", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void GetMetadata_Returns_EdmModelFromRequest()
        {
            IEdmModel model = new EdmModel();

            MetadataController controller = new MetadataController();
            controller.Request = new HttpRequestMessage();
            controller.Request.ODataProperties().Model = model;

            IEdmModel responseModel = controller.GetMetadata();
            Assert.Equal(model, responseModel);
        }

        [Fact]
        public void GetMetadata_Throws_IfModelIsNotSetOnRequest()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            MetadataController controller = new MetadataController();
            controller.Request = new HttpRequestMessage();

            Assert.Throws<InvalidOperationException>(() => controller.GetMetadata(),
                "The request must have an associated EDM model. Consider using the extension method " +
                "HttpConfiguration.Routes.MapODataServiceRoute to register a route that parses the OData URI and " +
                "attaches the model information.");
        }

        [Fact]
        public void DollarMetaDataWorks_AfterTracingIsEnabled()
        {
            HttpServer server = new HttpServer(GetConfiguration());
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

            var config = new[] { typeof(MetadataController) }.GetHttpConfiguration();
            HttpServer server = new HttpServer(config);
            config.MapODataServiceRoute("OData1", "v1", model1);
            config.MapODataServiceRoute("OData2", "v2", model2);

            HttpClient client = new HttpClient(server);
            AssertHasEntitySet(client, "http://localhost/v1/$metadata", "People1");
            AssertHasEntitySet(client, "http://localhost/v2/$metadata", "People2");
        }

        [Fact]
        public void DollarMetadata_Works_WithOpenComplexType()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<FormatterAddress>();
            IEdmModel model = builder.GetEdmModel();

            var config = new[] { typeof(MetadataController) }.GetHttpConfiguration();
            config.MapODataServiceRoute(model);
            HttpServer server = new HttpServer(config);
            HttpClient client = new HttpClient(server);

            // Act
            var response = client.GetAsync("http://localhost/$metadata").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains("<ComplexType Name=\"FormatterAddress\" OpenType=\"true\">",
                response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void DollarMetadata_Works_WithInheritanceOpenComplexType()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<FormatterAddress>();
            IEdmModel model = builder.GetEdmModel();

            var config = new[] { typeof(MetadataController) }.GetHttpConfiguration();
            config.MapODataServiceRoute(model);
            HttpServer server = new HttpServer(config);
            HttpClient client = new HttpClient(server);

            // Act
            var response = client.GetAsync("http://localhost/$metadata").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains("<ComplexType Name=\"FormatterUsAddress\" BaseType=\"System.Web.OData.Formatter.FormatterAddress\" OpenType=\"true\">",
                response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void DollarMetadata_Works_WithDerivedOpenComplexType()
        {
            // Arrange
            const string expectMetadata =
@"<?xml version='1.0' encoding='utf-8'?>
<edmx:Edmx Version='4.0' xmlns:edmx='http://docs.oasis-open.org/odata/ns/edmx'>
  <edmx:DataServices>
    <Schema Namespace='System.Web.OData.Formatter' xmlns='http://docs.oasis-open.org/odata/ns/edm'>
      <ComplexType Name='ComplexBaseType'>
        <Property Name='BaseProperty' Type='Edm.String' />
      </ComplexType>
      <ComplexType Name='ComplexDerivedOpenType' BaseType='System.Web.OData.Formatter.ComplexBaseType' OpenType='true'>
        <Property Name='DerivedProperty' Type='Edm.String' />
      </ComplexType>
    </Schema>
    <Schema Namespace='Default' xmlns='http://docs.oasis-open.org/odata/ns/edm'>
      <EntityContainer Name='Container' />
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<ComplexBaseType>();
            IEdmModel model = builder.GetEdmModel();

            var config = new[] { typeof(MetadataController) }.GetHttpConfiguration();
            config.MapODataServiceRoute(model);
            HttpServer server = new HttpServer(config);
            HttpClient client = new HttpClient(server);

            // Act
            var response = client.GetAsync("http://localhost/$metadata").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Equal(expectMetadata.Replace("'", "\""), response.Content.ReadAsStringAsync().Result);
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
            HttpServer server = new HttpServer(GetConfiguration());
            server.Configuration.Services.Replace(typeof(ITraceWriter), new Mock<ITraceWriter>().Object);

            HttpClient client = new HttpClient(server);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            var response = client.SendAsync(request).Result;

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            Assert.Contains("\"@odata.context\":\"http://localhost/$metadata\"", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void ServiceDocumentWorks_OutputSingleton()
        {
            // Arrange
            HttpServer server = new HttpServer(GetConfiguration());

            HttpClient client = new HttpClient(server);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            var response = client.SendAsync(request).Result;
            var repsoneString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("\"name\":\"President\",\"kind\":\"Singleton\",\"url\":\"President\"",
                repsoneString);
        }

        [Theory]
        [InlineData("application/xml")]
        [InlineData("application/abcd")]
        public void ServiceDocument_Returns_NotAcceptable_ForNonJsonMediaType(string mediaType)
        {
            // Arrange
            HttpServer server = new HttpServer(GetConfiguration());
            server.Configuration.Services.Replace(typeof(IContentNegotiator), new DefaultContentNegotiator(true));

            HttpClient client = new HttpClient(server);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(mediaType));

            // Act
            var response = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);
        }

        [Fact]
        public void ServiceDocument_ContainsFunctonImport()
        {
            // Arrange
            HttpServer server = new HttpServer(GetConfiguration());
            HttpClient client = new HttpClient(server);

            // Act
            var responseString = client.GetStringAsync("http://localhost/").Result;

            // Assert
            Assert.Contains("\"name\":\"GetPerson\",\"kind\":\"FunctionImport\",\"url\":\"GetPerson\"", responseString);
            Assert.Contains("\"name\":\"GetVipPerson\",\"kind\":\"FunctionImport\",\"url\":\"GetVipPerson\"", responseString);
        }

        [Fact]
        public void ServiceDocument_DoesNotContainFunctonImport_IfWithParameters()
        {
            // Arrange
            IEdmModel model = ODataTestUtil.GetEdmModel();
            IEnumerable<IEdmFunctionImport> functionImports = model.EntityContainer.Elements.OfType<IEdmFunctionImport>()
                .Where(f => f.Name == "GetSalary");

            HttpServer server = new HttpServer(GetConfiguration());
            HttpClient client = new HttpClient(server);

            // Act
            var responseString = client.GetStringAsync("http://localhost/").Result;

            // Assert
            var functionImport = Assert.Single(functionImports);
            Assert.Equal("Default.GetSalary", functionImport.Function.FullName());
            Assert.True(functionImport.IncludeInServiceDocument);
            Assert.Contains("\"@odata.context\":\"http://localhost/$metadata\"", responseString);
            Assert.DoesNotContain("\"name\":\"GetSalary\",\"kind\":\"FunctionImport\",\"url\":\"GetSalary\"", responseString);
        }

        [Fact]
        public void ServiceDocument_DoesNotContainFunctonImport_IfNotIncludeInServiceDocument()
        {
            // Arrange
            IEdmModel model = ODataTestUtil.GetEdmModel();
            IEdmFunctionImport[] functionImports = model.EntityContainer.Elements.OfType<IEdmFunctionImport>()
                .Where(f => f.Name == "GetAddress").ToArray();

            HttpServer server = new HttpServer(GetConfiguration());
            HttpClient client = new HttpClient(server);

            // Act
            var responseString = client.GetStringAsync("http://localhost/").Result;

            // Assert
            Assert.Equal(2, functionImports.Length);

            Assert.Equal("Default.GetAddress", functionImports[0].Function.FullName());
            Assert.Equal("Default.GetAddress", functionImports[1].Function.FullName());

            Assert.False(functionImports[0].IncludeInServiceDocument);
            Assert.False(functionImports[1].IncludeInServiceDocument);

            Assert.Empty(functionImports[0].Function.Parameters);
            Assert.Equal("AddressId", functionImports[1].Function.Parameters.First().Name);

            Assert.Contains("\"@odata.context\":\"http://localhost/$metadata\"", responseString);
            Assert.DoesNotContain("\"name\":\"GetAddress\",\"kind\":\"FunctionImport\",\"url\":\"GetAddress\"", responseString);
        }

        [Fact]
        public void ServiceDocument_OnlyContainOneFunctonImport_ForOverloadFunctions()
        {
            // Arrange
            IEdmModel model = ODataTestUtil.GetEdmModel();
            IEdmFunctionImport[] functionImports = model.EntityContainer.Elements.OfType<IEdmFunctionImport>()
                .Where(f => f.Name == "GetVipPerson").ToArray();

            HttpServer server = new HttpServer(GetConfiguration());
            HttpClient client = new HttpClient(server);

            // Act
            var responseString = client.GetStringAsync("http://localhost/").Result;

            // Assert
            Assert.Equal(3, functionImports.Length);

            Assert.Equal("Default.GetVipPerson", functionImports[0].Function.FullName());
            Assert.Equal("Default.GetVipPerson", functionImports[1].Function.FullName());
            Assert.Equal("Default.GetVipPerson", functionImports[2].Function.FullName());

            Assert.Single(functionImports[0].Function.Parameters);
            Assert.Empty(functionImports[1].Function.Parameters);
            Assert.Equal(2, functionImports[2].Function.Parameters.Count());

            Assert.Contains("\"@odata.context\":\"http://localhost/$metadata\"", responseString);
            Assert.Contains("\"name\":\"GetVipPerson\",\"kind\":\"FunctionImport\",\"url\":\"GetVipPerson\"", responseString);
        }

        [Fact]
        public void Controller_DoesNotAppear_InApiDescriptions()
        {
            var config = new[] { typeof(MetadataController) }.GetHttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{action}");
            config.MapODataServiceRoute(new ODataConventionModelBuilder().GetEdmModel());
            config.EnsureInitialized();
            var explorer = config.Services.GetApiExplorer();

            var apis = explorer.ApiDescriptions.Select(api => api.ActionDescriptor.ControllerDescriptor.ControllerName);

            Assert.DoesNotContain("ODataMetadata", apis);
        }

        [Fact]
        public void RequiredAttribute_Works_OnComplexTypeProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<FormatterAccount>("Accounts");

            var config = new[] { typeof(MetadataController) }.GetHttpConfiguration();
            config.MapODataServiceRoute(builder.GetEdmModel());

            HttpServer server = new HttpServer(config);
            HttpClient client = new HttpClient(server);

            // Act
            var responseString = client.GetStringAsync("http://localhost/$metadata").Result;

            // Assert
            Assert.Contains(
                "<Property Name=\"Address\" Type=\"System.Web.OData.Formatter.FormatterAddress\" Nullable=\"false\" />",
                responseString);

            Assert.Contains(
                "<Property Name=\"Addresses\" Type=\"Collection(System.Web.OData.Formatter.FormatterAddress)\" Nullable=\"false\" />",
                responseString);
        }

        private HttpConfiguration GetConfiguration()
        {
            var config = new[] { typeof(MetadataController) }.GetHttpConfiguration();
            config.MapODataServiceRoute(ODataTestUtil.GetEdmModel());
            return config;
        }
    }
}
