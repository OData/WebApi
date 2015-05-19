// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Tracing;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;
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
        public void DollarMetadata_Works_WithReferencialConstraint_IfForeignKeyAttributeOnNavigationProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<ForeignCustomer>("Customers");
            IEdmModel model = builder.GetEdmModel();

            HttpConfiguration config = new[] { typeof(MetadataController) }.GetHttpConfiguration();
            HttpServer server = new HttpServer(config);
            config.MapODataServiceRoute("odata", "odata", model);

            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync("http://localhost/odata/$metadata").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains("<ReferentialConstraint Property=\"CustomerId\" ReferencedProperty=\"ForeignCustomerId\" />",
                response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void DollarMetadata_Works_ForNullableReferencialConstraint_WithfForeignKeyAttribute()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<FkProduct>("Products");
            IEdmModel model = builder.GetEdmModel();

            HttpConfiguration config = new[] { typeof(MetadataController) }.GetHttpConfiguration();
            HttpServer server = new HttpServer(config);
            config.MapODataServiceRoute("odata", "odata", model);

            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync("http://localhost/odata/$metadata").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);

            string payload = response.Content.ReadAsStringAsync().Result;
            Assert.Contains("<Property Name=\"SupplierId\" Type=\"Edm.Int32\" />", payload);
            Assert.Contains("<ReferentialConstraint Property=\"SupplierId\" ReferencedProperty=\"Id\" />", payload);

            Assert.Contains("<Property Name=\"SupplierKey\" Type=\"Edm.Int32\" />", payload);
            Assert.Contains("<ReferentialConstraint Property=\"SupplierKey\" ReferencedProperty=\"Id\" />", payload);
        }

        [Fact]
        public void DollarMetadata_Works_ForNullableReferencialConstraint_WithCustomReferentialConstraints()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<FkSupplier>().HasKey(c => c.Id);

            var product = builder.EntityType<FkProduct>().HasKey(o => o.Id);
            product.HasOptional(o => o.Supplier, (o, c) => o.SupplierId == c.Id);
            product.HasRequired(o => o.SupplierNav, (o, c) => o.SupplierKey == c.Id);

            IEdmModel model = builder.GetEdmModel();

            HttpConfiguration config = new[] { typeof(MetadataController) }.GetHttpConfiguration();
            HttpServer server = new HttpServer(config);
            config.MapODataServiceRoute("odata", "odata", model);

            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync("http://localhost/odata/$metadata").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            string payload = response.Content.ReadAsStringAsync().Result;

            // non-nullable
            Assert.Contains("<Property Name=\"SupplierId\" Type=\"Edm.Int32\" />", payload);
            Assert.Contains("<NavigationProperty Name=\"Supplier\" Type=\"System.Web.OData.Formatter.FkSupplier\">", payload);
            Assert.Contains("<ReferentialConstraint Property=\"SupplierId\" ReferencedProperty=\"Id\" />", payload);

            // nullable
            Assert.Contains("<Property Name=\"SupplierKey\" Type=\"Edm.Int32\" Nullable=\"false\" />", payload);
            Assert.Contains("<NavigationProperty Name=\"SupplierNav\" Type=\"System.Web.OData.Formatter.FkSupplier\" Nullable=\"false\">", payload);
            Assert.Contains("<ReferentialConstraint Property=\"SupplierKey\" ReferencedProperty=\"Id\" />", payload);
        }

        [Fact]
        public void DollarMetadata_Works_ForNullableReferencialConstraint_WithForeignKeyAttributeAndRequiredAttribute()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<FkProduct2>("Products");
            IEdmModel model = builder.GetEdmModel();

            HttpConfiguration config = new[] { typeof(MetadataController) }.GetHttpConfiguration();
            HttpServer server = new HttpServer(config);
            config.MapODataServiceRoute("odata", "odata", model);

            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync("http://localhost/odata/$metadata").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);

            string payload = response.Content.ReadAsStringAsync().Result;

            // non-nullable
            Assert.Contains("<Property Name=\"SupplierId\" Type=\"Edm.String\" Nullable=\"false\" />", payload);
            Assert.Contains("<NavigationProperty Name=\"Supplier\" Type=\"System.Web.OData.Formatter.FkSupplier2\" Nullable=\"false\">", payload);
            Assert.Contains("<ReferentialConstraint Property=\"SupplierId\" ReferencedProperty=\"Id\" />", payload);

            // nullable
            Assert.Contains("<Property Name=\"SupplierKey\" Type=\"Edm.String\" />", payload);
            Assert.Contains("<NavigationProperty Name=\"SupplierNav\" Type=\"System.Web.OData.Formatter.FkSupplier2\">", payload);
            Assert.Contains("<ReferentialConstraint Property=\"SupplierKey\" ReferencedProperty=\"Id\" />", payload);
        }

        [Fact]
        public void DollarMetadata_Works_ForNullableReferencialConstraint_WithForeignKeyDiscovery()
        {
            // Arrange
            const string expect =
                "        <Property Name=\"FkSupplierId\" Type=\"Edm.Int32\" Nullable=\"false\" />\r\n" +
                "        <Property Name=\"FkSupplier2Id\" Type=\"Edm.String\" Nullable=\"false\" />\r\n" +
                "        <Property Name=\"FkSupplier3Id\" Type=\"Edm.Int32\" />\r\n" +
                "        <NavigationProperty Name=\"Supplier\" Type=\"System.Web.OData.Formatter.FkSupplier\" Nullable=\"false\">\r\n" +
                "          <ReferentialConstraint Property=\"FkSupplierId\" ReferencedProperty=\"Id\" />\r\n" +
                "        </NavigationProperty>\r\n" +
                "        <NavigationProperty Name=\"Supplier2\" Type=\"System.Web.OData.Formatter.FkSupplier2\" Nullable=\"false\">\r\n" +
                "          <ReferentialConstraint Property=\"FkSupplier2Id\" ReferencedProperty=\"Id\" />\r\n" +
                "        </NavigationProperty>\r\n" +
                "        <NavigationProperty Name=\"Supplier3\" Type=\"System.Web.OData.Formatter.FkSupplier3\">\r\n" +
                "          <ReferentialConstraint Property=\"FkSupplier3Id\" ReferencedProperty=\"Id\" />\r\n" +
                "        </NavigationProperty>";

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<FkProduct3>("Products");
            IEdmModel model = builder.GetEdmModel();

            HttpConfiguration config = new[] { typeof(MetadataController) }.GetHttpConfiguration();
            HttpServer server = new HttpServer(config);
            config.MapODataServiceRoute("odata", "odata", model);

            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync("http://localhost/odata/$metadata").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains(expect, response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void DollarMetadata_Works_WithReferencialConstraint_IfForeignKeyAttributeOnForeignKeyProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<ForeignCustomer2>("Customers");
            IEdmModel model = builder.GetEdmModel();

            HttpConfiguration config = new[] { typeof(MetadataController) }.GetHttpConfiguration();
            HttpServer server = new HttpServer(config);
            config.MapODataServiceRoute("odata", "odata", model);

            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync("http://localhost/odata/$metadata").Result;
            string payload = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);

            Assert.Contains("<OnDelete Action=\"Cascade\" />", payload);
            Assert.Contains("<ReferentialConstraint Property=\"CustomerId\" ReferencedProperty=\"Id\" />", payload);
        }

        [Fact]
        public void DollarMetadata_Works_WithCustomReferentialConstraints()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<ForeignCustomer>()
                .HasKey(c => c.ForeignCustomerId)
                .HasMany(c => c.Orders);

            builder.EntityType<ForeignOrder>()
                .HasKey(o => o.ForeignOrderId)
                .HasRequired(o => o.Customer, (o, c) => o.CustomerId == c.OtherCustomerKey);

            IEdmModel model = builder.GetEdmModel();

            HttpConfiguration config = new[] { typeof(MetadataController) }.GetHttpConfiguration();
            HttpServer server = new HttpServer(config);
            config.MapODataServiceRoute("odata", "odata", model);

            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync("http://localhost/odata/$metadata").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains("<ReferentialConstraint Property=\"CustomerId\" ReferencedProperty=\"OtherCustomerKey\" />",
                response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void DollarMetadata_Works_WithOnDeleteAction()
        {
            // Arrange
            const string expect =
                "        <Property Name=\"CustomerId\" Type=\"Edm.Int32\" Nullable=\"false\" />\r\n" +
                "        <NavigationProperty Name=\"Customer\" Type=\"System.Web.OData.Formatter.ForeignCustomer\" Nullable=\"false\">\r\n" +
                "          <OnDelete Action=\"Cascade\" />\r\n" +
                "          <ReferentialConstraint Property=\"CustomerId\" ReferencedProperty=\"ForeignCustomerId\" />\r\n" +
                "        </NavigationProperty>";

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<ForeignCustomer>().HasKey(c => c.ForeignCustomerId).HasMany(c => c.Orders);

            builder.EntityType<ForeignOrder>().HasKey(o => o.ForeignOrderId)
                .HasRequired(o => o.Customer, (o, c) => o.CustomerId == c.ForeignCustomerId)
                .CascadeOnDelete();

            IEdmModel model = builder.GetEdmModel();

            HttpConfiguration config = new[] { typeof(MetadataController) }.GetHttpConfiguration();
            HttpServer server = new HttpServer(config);
            config.MapODataServiceRoute("odata", "odata", model);

            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync("http://localhost/odata/$metadata").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains(expect, response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void DollarMetadata_Works_WithMultipleReferentialConstraints()
        {
            // Arrange
            const string expect =
                "        <NavigationProperty Name=\"Customer\" Type=\"System.Web.OData.Formatter.MultiForeignCustomer\" Nullable=\"false\">\r\n" +
                "          <ReferentialConstraint Property=\"CustomerForeignKey1\" ReferencedProperty=\"CustomerId1\" />\r\n" +
                "          <ReferentialConstraint Property=\"CustomerForeignKey2\" ReferencedProperty=\"CustomerId2\" />\r\n" +
                "        </NavigationProperty>";

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<MultiForeignCustomer>().HasKey(c => new {c.CustomerId1, c.CustomerId2}).HasMany(c => c.Orders);

            builder.EntityType<MultiForeignOrder>().HasKey(c => c.ForeignOrderId)
                .HasRequired(o => o.Customer,
                    (o, c) => o.CustomerForeignKey1 == c.CustomerId1 && o.CustomerForeignKey2 == c.CustomerId2);

            IEdmModel model = builder.GetEdmModel();

            HttpConfiguration config = new[] { typeof(MetadataController) }.GetHttpConfiguration();
            HttpServer server = new HttpServer(config);
            config.MapODataServiceRoute("odata", "odata", model);

            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync("http://localhost/odata/$metadata").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains(expect, response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void DollarMetadata_Works_WithPrincipalKeyOnBaseType_ButBaseTypeNotInEdmModel()
        {
            // Arrange
            const string expect =
                "        <NavigationProperty Name=\"DerivedProp\" Type=\"System.Web.OData.Formatter.DerivedPrincipalEntity\">\r\n" +
                "          <ReferentialConstraint Property=\"DerivedPrincipalEntityId\" ReferencedProperty=\"Id\" />\r\n" +
                "        </NavigationProperty>";

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<DerivedPrincipalEntity>("Principals");
            builder.EntitySet<DependentEntity>("Dependents");
            IEdmModel model = builder.GetEdmModel();

            HttpConfiguration config = new[] { typeof(MetadataController) }.GetHttpConfiguration();
            HttpServer server = new HttpServer(config);
            config.MapODataServiceRoute("odata", "odata", model);

            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync("http://localhost/odata/$metadata").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);

            Assert.Contains(expect, response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void DollarMetadata_Works_WithMultipleReferentialConstraints_ForUntypeModel()
        {
            // Arrange
            EdmModel model = new EdmModel();

            EdmEntityType customer = new EdmEntityType("DefaultNamespace", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("CustomerId", EdmCoreModel.Instance.GetInt32(isNullable: false)));
            customer.AddStructuralProperty("Name",
                EdmCoreModel.Instance.GetString(isUnbounded: false, maxLength: 100, isUnicode: null, isNullable: false));
            model.AddElement(customer);

            EdmEntityType order = new EdmEntityType("DefaultNamespace", "Order");
            order.AddKeys(order.AddStructuralProperty("OrderId", EdmCoreModel.Instance.GetInt32(false)));
            EdmStructuralProperty orderCustomerId = order.AddStructuralProperty("CustomerForeignKey", EdmCoreModel.Instance.GetInt32(true));
            model.AddElement(order);

            customer.AddBidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "Orders", Target = order, TargetMultiplicity = EdmMultiplicity.Many
                },
                new EdmNavigationPropertyInfo
                {
                    Name = "Customer", TargetMultiplicity = EdmMultiplicity.ZeroOrOne,
                    DependentProperties = new[] { orderCustomerId },
                    PrincipalProperties = customer.Key()
                });

            const string expect =
                "        <Property Name=\"CustomerForeignKey\" Type=\"Edm.Int32\" />\r\n" +
                "        <NavigationProperty Name=\"Customer\" Type=\"DefaultNamespace.Customer\" Partner=\"Orders\">\r\n" +
                "          <ReferentialConstraint Property=\"CustomerForeignKey\" ReferencedProperty=\"CustomerId\" />\r\n" +
                "        </NavigationProperty>";

            HttpConfiguration config = new[] { typeof(MetadataController) }.GetHttpConfiguration();
            HttpServer server = new HttpServer(config);
            config.MapODataServiceRoute("odata", "odata", model);

            HttpClient client = new HttpClient(server);

            // Act
            HttpResponseMessage response = client.GetAsync("http://localhost/odata/$metadata").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains(expect, response.Content.ReadAsStringAsync().Result);
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

        [Fact]
        public void DollarMetadata_Works_WithActionParameterNullable_ReturnTypeNullable()
        {
            // Arrange
            const string expectMetadata =
@"<Schema Namespace='Default' xmlns='http://docs.oasis-open.org/odata/ns/edm'>
      <Action Name='NullableAction' IsBound='true'>
        <Parameter Name='bindingParameter' Type='System.Web.OData.Formatter.FormatterPerson' />
        <Parameter Name='param' Type='Edm.String' Unicode='false' />
        <ReturnType Type='System.Web.OData.Formatter.FormatterAddress' />
      </Action>
      <Action Name='NonNullableAction' IsBound='true'>
        <Parameter Name='bindingParameter' Type='System.Web.OData.Formatter.FormatterPerson' />
        <Parameter Name='param' Type='Edm.String' Nullable='false' Unicode='false' />
        <ReturnType Type='System.Web.OData.Formatter.FormatterAddress' Nullable='false' />
      </Action>
      <EntityContainer Name='Container' />
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntityTypeConfiguration<FormatterPerson> person = builder.EntityType<FormatterPerson>();

            ActionConfiguration action = person.Action("NullableAction").Returns<FormatterAddress>();
            action.Parameter<string>("param");

            action = person.Action("NonNullableAction").Returns<FormatterAddress>();
            action.OptionalReturn = false;
            action.Parameter<string>("param").OptionalParameter = false;
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
            Assert.Contains(expectMetadata.Replace("'", "\""), response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void DollarMetadata_Works_WithFunctionParameterNullable_ReturnTypeNullable()
        {
            // Arrange
            const string expectMetadata =
@"<Schema Namespace='Default' xmlns='http://docs.oasis-open.org/odata/ns/edm'>
      <Function Name='NullableFunction' IsBound='true'>
        <Parameter Name='bindingParameter' Type='System.Web.OData.Formatter.FormatterPerson' />
        <Parameter Name='param' Type='Edm.String' Unicode='false' />
        <ReturnType Type='System.Web.OData.Formatter.FormatterAddress' />
      </Function>
      <Function Name='NonNullableFunction' IsBound='true'>
        <Parameter Name='bindingParameter' Type='System.Web.OData.Formatter.FormatterPerson' />
        <Parameter Name='param' Type='Edm.String' Nullable='false' Unicode='false' />
        <ReturnType Type='System.Web.OData.Formatter.FormatterAddress' Nullable='false' />
      </Function>
      <EntityContainer Name='Container' />
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntityTypeConfiguration<FormatterPerson> person = builder.EntityType<FormatterPerson>();

            FunctionConfiguration function = person.Function("NullableFunction").Returns<FormatterAddress>();
            function.Parameter<string>("param");

            function = person.Function("NonNullableFunction").Returns<FormatterAddress>();
            function.OptionalReturn = false;
            function.Parameter<string>("param").OptionalParameter = false;
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
            Assert.Contains(expectMetadata.Replace("'", "\""), response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void DollarMetadata_Works_WithAbstractEntityTypeWithoutKey()
        {
            // Arrange
            const string expectMetadata =
"      <EntityType Name=\"AbstractEntityType\" Abstract=\"true\">\r\n" +
"        <Property Name=\"IntProperty\" Type=\"Edm.Int32\" Nullable=\"false\" />\r\n" +
"      </EntityType>";

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<AbstractEntityType>().Abstract().Property(a => a.IntProperty);
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

            string payload = response.Content.ReadAsStringAsync().Result;
            Assert.Contains(expectMetadata, payload);
            Assert.DoesNotContain("<key>", payload);
        }

        [Fact]
        public void DollarMetadata_Works_WithDerivedEntityTypeWithOwnKeys()
        {
            // Arrange
            const string expectMetadata =
"    <Schema Namespace=\"System.Web.OData.Formatter\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">\r\n" +
"      <EntityType Name=\"AbstractEntityType\" Abstract=\"true\">\r\n" +
"        <Property Name=\"IntProperty\" Type=\"Edm.Int32\" Nullable=\"false\" />\r\n" +
"      </EntityType>\r\n" +
"      <EntityType Name=\"SubEntityType\" BaseType=\"System.Web.OData.Formatter.AbstractEntityType\">\r\n" +
"        <Key>\r\n" +
"          <PropertyRef Name=\"SubKey\" />\r\n" +
"        </Key>\r\n" +
"        <Property Name=\"SubKey\" Type=\"Edm.Int32\" Nullable=\"false\" />\r\n" +
"      </EntityType>\r\n" +
"      <EntityType Name=\"AnotherSubEntityType\" BaseType=\"System.Web.OData.Formatter.AbstractEntityType\">\r\n" +
"        <Key>\r\n" +
"          <PropertyRef Name=\"AnotherKey\" />\r\n" +
"        </Key>\r\n" +
"        <Property Name=\"AnotherKey\" Type=\"Edm.Double\" Nullable=\"false\" />\r\n" +
"      </EntityType>\r\n" +
"    </Schema>";

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<AbstractEntityType>().Abstract().Property(a => a.IntProperty);
            builder.EntityType<SubEntityType>().HasKey(b => b.SubKey).DerivesFrom<AbstractEntityType>();
            builder.EntityType<AnotherSubEntityType>().HasKey(d => d.AnotherKey).DerivesFrom<AbstractEntityType>();
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

            string payload = response.Content.ReadAsStringAsync().Result;
            Assert.Contains(expectMetadata, payload);
        }

        [Fact]
        public void DollarMetadata_Works_WithEntityTypeWithEnumKeys()
        {
            // Arrange
            const string expectMetadata =
                "      <EntityType Name=\"EnumModel\">\r\n" +
                "        <Key>\r\n" +
                "          <PropertyRef Name=\"Simple\" />\r\n" +
                "        </Key>\r\n" +
                "        <Property Name=\"Simple\" Type=\"NS.SimpleEnum\" Nullable=\"false\" />\r\n" +
                "      </EntityType>\r\n" +
                "      <EnumType Name=\"SimpleEnum\" />";

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<EnumModel>().HasKey(e => e.Simple).Namespace = "NS";
            builder.EnumType<SimpleEnum>().Namespace = "NS";
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

            string payload = response.Content.ReadAsStringAsync().Result;
            Assert.Contains(expectMetadata, payload);
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
