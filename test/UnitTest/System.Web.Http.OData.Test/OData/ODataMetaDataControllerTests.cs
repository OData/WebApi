// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
        public void DollarMetaData_Works_WithConcurrencyCheckAttribute()
        {
            // Arrange
            string expectedStringInMetadata = "<Property Name=\"Name\" Type=\"Edm.String\" ConcurrencyMode=\"Fixed\" />";
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<FormatterPersonWithConcurrencyCheck>("People");

            HttpServer server = new HttpServer();
            server.Configuration.Routes.MapODataServiceRoute(builder.GetEdmModel());

            // Act
            HttpClient client = new HttpClient(server);
            var response = client.GetAsync("http://localhost/$metadata").Result;

            // Assert            
            Assert.Contains(expectedStringInMetadata, response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void DollarMetaData_Works_WithDatabaseGeneratedAttribute()
        {
            // Arrange
            string expectedString1 = string.Format(":{0}=\"Identity\"", StoreGeneratedPatternAnnotation.AnnotationName);
            string expectedString2 = string.Format("=\"{0}\"", StoreGeneratedPatternAnnotation.AnnotationsNamespace);

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<FormatterPersonWithDatabaseGeneratedOption>("People");

            HttpServer server = new HttpServer();
            server.Configuration.Routes.MapODataServiceRoute(builder.GetEdmModel());
            HttpClient client = new HttpClient(server);

            // Act
            var response = client.GetAsync("http://localhost/$metadata").Result;

            // Assert
            string result = response.Content.ReadAsStringAsync().Result;
            Assert.Contains(expectedString1, result);
            Assert.Contains(expectedString2, result);
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
        public void DollarMetadata_Works_WithReferentialConstraint()
        {
            // Arrange
            const string expect =
                "        <ReferentialConstraint>\r\n" +
                "          <Principal Role=\"Customer\">\r\n" +
                "            <PropertyRef Name=\"ForeignCustomerId\" />\r\n" +
                "          </Principal>\r\n" +
                "          <Dependent Role=\"CustomerPartner\">\r\n" +
                "            <PropertyRef Name=\"CustomerId\" />\r\n" +
                "          </Dependent>\r\n" +
                "        </ReferentialConstraint>";

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<ForeignCustomer>("Customers");
            IEdmModel model = builder.GetEdmModel();

            HttpServer server = new HttpServer();
            server.Configuration.Routes.MapODataServiceRoute("odata", "odata", model);

            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync("http://localhost/odata/$metadata").Result;
            string payload = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains("<OnDelete Action=\"Cascade\" />", payload);
            Assert.Contains(expect, payload);
        }

        [Fact]
        public void DollarMetadata_Works_WithMultipleReferentialConstraints_WithModelBuilder()
        {
            // Arrange
            const string expect =
                "      <Association Name=\"System_Web_Http_OData_Formatter_MultiForeignOrder_Customer_System_Web_Http_OData_Formatter_MultiForeignCustomer_CustomerPartner\">\r\n" +
                "        <End Type=\"System.Web.Http.OData.Formatter.MultiForeignCustomer\" Role=\"Customer\" Multiplicity=\"1\" />\r\n" +
                "        <End Type=\"System.Web.Http.OData.Formatter.MultiForeignOrder\" Role=\"CustomerPartner\" Multiplicity=\"0..1\">\r\n" +
                "          <OnDelete Action=\"Cascade\" />\r\n" +
                "        </End>\r\n" +
                "        <ReferentialConstraint>\r\n" +
                "          <Principal Role=\"Customer\">\r\n" +
                "            <PropertyRef Name=\"CustomerId2\" />\r\n" +
                "            <PropertyRef Name=\"CustomerId1\" />\r\n" +
                "          </Principal>\r\n" +
                "          <Dependent Role=\"CustomerPartner\">\r\n" +
                "            <PropertyRef Name=\"CustomerForeignKey2\" />\r\n" +
                "            <PropertyRef Name=\"CustomerForeignKey1\" />\r\n" +
                "          </Dependent>\r\n" +
                "        </ReferentialConstraint>\r\n" +
                "      </Association>";

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.Entity<MultiForeignCustomer>()
                .HasKey(c => new { c.CustomerId2, c.CustomerId1 })
                .HasMany(c => c.Orders);

            builder.Entity<MultiForeignOrder>()
                .HasKey(o => o.ForeignOrderId)
                .HasRequired(o => o.Customer, (o, c) => o.CustomerForeignKey2 == c.CustomerId2 && o.CustomerForeignKey1 == c.CustomerId1)
                .CascadeOnDelete();

            IEdmModel model = builder.GetEdmModel();

            HttpServer server = new HttpServer();
            server.Configuration.Routes.MapODataServiceRoute("odata", "odata", model);

            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync("http://localhost/odata/$metadata").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);

            Assert.Contains(expect, response.Content.ReadAsStringAsync().Result);
        }

        [Theory]
        [InlineData(typeof(BasePrincipalEntity))]
        [InlineData(typeof(DerivedPrincipalEntity))]
        public void DollarMetadata_Works_WithPrincipalKeyOnBaseType_ButBaseTypeNotInEdmModel(Type entityType)
        {
            // Arrange
            const string expect =
               "      <Association Name=\"System_Web_Http_OData_Formatter_DependentEntity_DerivedProp_System_Web_Http_OData_Formatter_DerivedPrincipalEntity_DerivedPropPartner\">\r\n" +
               "        <End Type=\"System.Web.Http.OData.Formatter.DerivedPrincipalEntity\" Role=\"DerivedProp\" Multiplicity=\"0..1\" />\r\n" +
               "        <End Type=\"System.Web.Http.OData.Formatter.DependentEntity\" Role=\"DerivedPropPartner\" Multiplicity=\"0..1\" />\r\n" +
               "        <ReferentialConstraint>\r\n" +
               "          <Principal Role=\"DerivedProp\">\r\n" +
               "            <PropertyRef Name=\"Id\" />\r\n" +
               "          </Principal>\r\n" +
               "          <Dependent Role=\"DerivedPropPartner\">\r\n" +
               "            <PropertyRef Name=\"DerivedPrincipalEntityId\" />\r\n" +
               "          </Dependent>\r\n" +
               "        </ReferentialConstraint>\r\n" +
               "      </Association>";

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntity(entityType);
            builder.Entity<DependentEntity>();
            IEdmModel model = builder.GetEdmModel();

            HttpServer server = new HttpServer();
            server.Configuration.Routes.MapODataServiceRoute("odata", "odata", model);

            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync("http://localhost/odata/$metadata").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);

            Assert.Contains(expect, response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void DollarMetadata_Works_WithMultipleReferentialConstraints_ForUntypedModel()
        {
            // Arrange
            EdmModel model = new EdmModel();

            EdmEntityType customer = new EdmEntityType("DefaultNamespace", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("CustomerId", EdmCoreModel.Instance.GetInt32(false)));
            customer.AddStructuralProperty("Name", EdmCoreModel.Instance.GetString(false));
            model.AddElement(customer);

            EdmEntityType order = new EdmEntityType("DefaultNamespace", "Order");
            order.AddKeys(order.AddStructuralProperty("OrderId", EdmCoreModel.Instance.GetInt32(false)));
            EdmStructuralProperty orderCustomerId = order.AddStructuralProperty("CustomerForeignKey", EdmCoreModel.Instance.GetInt32(true));
            model.AddElement(order);

            customer.AddBidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "Orders",
                    Target = order,
                    TargetMultiplicity = EdmMultiplicity.Many
                },
                new EdmNavigationPropertyInfo
                {
                    Name = "Customer",
                    TargetMultiplicity = EdmMultiplicity.ZeroOrOne,
                    DependentProperties = new[] { orderCustomerId },
                });

            const string expect =
                "        <ReferentialConstraint>\r\n" +
                "          <Principal Role=\"Customer\">\r\n" +
                "            <PropertyRef Name=\"CustomerId\" />\r\n" +
                "          </Principal>\r\n" +
                "          <Dependent Role=\"Orders\">\r\n" +
                "            <PropertyRef Name=\"CustomerForeignKey\" />\r\n" +
                "          </Dependent>\r\n" +
                "        </ReferentialConstraint>";

            HttpServer server = new HttpServer();
            server.Configuration.Routes.MapODataServiceRoute("odata", "odata", model);

            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync("http://localhost/odata/$metadata").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains(expect, response.Content.ReadAsStringAsync().Result);
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
