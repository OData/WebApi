//-----------------------------------------------------------------------------
// <copyright file="MetadataControllerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common.Types;
using Microsoft.AspNet.OData.Test.Formatter;
using Microsoft.OData.Edm;
using Xunit;
#else
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Tracing;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common.Types;
using Microsoft.AspNet.OData.Test.Formatter;
using Microsoft.OData.Edm;
using Moq;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test
{
    public class MetadataControllerTest
    {
        [Fact]
        public async Task DollarMetaData_Works_WithoutAcceptHeader()
        {
            // Arrange
            HttpClient client = GetClient();

            // Act
            var response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains("<edmx:Edmx", await response.Content.ReadAsStringAsync());
        }

#if NETFX // the following test cases only apply for Asp.Net so far.
        [Fact]
        public void GetMetadata_Returns_EdmModelFromRequest()
        {
            IEdmModel model = new EdmModel();

            MetadataController controller = new MetadataController();
            controller.Request = new HttpRequestMessage();
            controller.Request.EnableHttpDependencyInjectionSupport(model);

            IEdmModel responseModel = controller.GetMetadata();
            Assert.Equal(model, responseModel);
        }

        [Fact]
        public async Task DollarMetaDataWorks_AfterTracingIsEnabled()
        {
            HttpServer server = new HttpServer(GetConfiguration());
            server.Configuration.Services.Replace(typeof(ITraceWriter), new Mock<ITraceWriter>().Object);

            HttpClient client = new HttpClient(server);
            var response = await client.GetAsync("http://localhost/$metadata");

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains("<edmx:Edmx", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task ServiceDocumentWorks_AfterTracingIsEnabled_IfModelIsSetOnConfiguration()
        {
            HttpServer server = new HttpServer(GetConfiguration());
            server.Configuration.Services.Replace(typeof(ITraceWriter), new Mock<ITraceWriter>().Object);

            HttpClient client = new HttpClient(server);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            var response = await client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            Assert.Contains("\"@odata.context\":\"http://localhost/$metadata\"", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("application/xml")]
        [InlineData("application/abcd")]
        public async Task ServiceDocument_Returns_NotAcceptable_ForNonJsonMediaType(string mediaType)
        {
            // Arrange
            HttpServer server = new HttpServer(GetConfiguration());
            server.Configuration.Services.Replace(typeof(IContentNegotiator), new DefaultContentNegotiator(true));

            HttpClient client = new HttpClient(server);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(mediaType));

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);
        }

        [Fact]
        public void Controller_DoesNotAppear_InApiDescriptions()
        {
            var config = RoutingConfigurationFactory.CreateWithTypes(new[] { typeof(MetadataController) });
            config.Routes.MapHttpRoute("Default", "{controller}/{action}");
            config.MapODataServiceRoute(ODataConventionModelBuilderFactory.Create().GetEdmModel());
            config.EnsureInitialized();
            var explorer = config.Services.GetApiExplorer();

            var apis = explorer.ApiDescriptions.Select(api => api.ActionDescriptor.ControllerDescriptor.ControllerName);

            Assert.DoesNotContain("ODataMetadata", apis);
        }

        private HttpConfiguration GetConfiguration()
        {
            var config = RoutingConfigurationFactory.CreateWithTypes(new[] { typeof(MetadataController) });
            config.MapODataServiceRoute(ODataTestUtil.GetEdmModel());
            return config;
        }
#endif

        [Fact]
        public async Task DollarMetadata_Works_WithMultipleModels()
        {
            ODataConventionModelBuilder builder1 = ODataConventionModelBuilderFactory.Create();
            builder1.EntitySet<FormatterPerson>("People1");
            var model1 = builder1.GetEdmModel();

            ODataConventionModelBuilder builder2 = ODataConventionModelBuilderFactory.Create();
            builder2.EntitySet<FormatterPerson>("People2");
            var model2 = builder2.GetEdmModel();

            var server = TestServerFactory.Create(new[] { typeof(MetadataController) }, (config) =>
            {
                config.MapODataServiceRoute("OData1", "v1", model1);
                config.MapODataServiceRoute("OData2", "v2", model2);
            });

            HttpClient client = TestServerFactory.CreateClient(server);
            await AssertHasEntitySet(client, "http://localhost/v1/$metadata", "People1");
            await AssertHasEntitySet(client, "http://localhost/v2/$metadata", "People2");
        }

        [Fact]
        public async Task DollarMetadata_Works_WithReferencialConstraint_IfForeignKeyAttributeOnNavigationProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<ForeignCustomer>("Customers");
            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            HttpResponseMessage response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains("<ReferentialConstraint Property=\"CustomerId\" ReferencedProperty=\"ForeignCustomerId\" />",
                await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task DollarMetadata_Works_ForNullableReferencialConstraint_WithfForeignKeyAttribute()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<FkProduct>("Products");
            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            HttpResponseMessage response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Contains("<Property Name=\"SupplierId\" Type=\"Edm.Int32\" />", payload);
            Assert.Contains("<ReferentialConstraint Property=\"SupplierId\" ReferencedProperty=\"Id\" />", payload);

            Assert.Contains("<Property Name=\"SupplierKey\" Type=\"Edm.Int32\" />", payload);
            Assert.Contains("<ReferentialConstraint Property=\"SupplierKey\" ReferencedProperty=\"Id\" />", payload);
        }

        [Fact]
        public async Task DollarMetadata_Works_ForNullableReferencialConstraint_WithCustomReferentialConstraints()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<FkSupplier>().HasKey(c => c.Id);

            var product = builder.EntityType<FkProduct>().HasKey(o => o.Id);
            product.HasOptional(o => o.Supplier, (o, c) => o.SupplierId == c.Id);
            product.HasRequired(o => o.SupplierNav, (o, c) => o.SupplierKey == c.Id);

            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            HttpResponseMessage response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            string payload = await response.Content.ReadAsStringAsync();

            // non-nullable
            Assert.Contains("<Property Name=\"SupplierId\" Type=\"Edm.Int32\" />", payload);
            Assert.Contains("<NavigationProperty Name=\"Supplier\" Type=\"Microsoft.AspNet.OData.Test.Formatter.FkSupplier\">", payload);
            Assert.Contains("<ReferentialConstraint Property=\"SupplierId\" ReferencedProperty=\"Id\" />", payload);

            // nullable
            Assert.Contains("<Property Name=\"SupplierKey\" Type=\"Edm.Int32\" Nullable=\"false\" />", payload);
            Assert.Contains("<NavigationProperty Name=\"SupplierNav\" Type=\"Microsoft.AspNet.OData.Test.Formatter.FkSupplier\" Nullable=\"false\">", payload);
            Assert.Contains("<ReferentialConstraint Property=\"SupplierKey\" ReferencedProperty=\"Id\" />", payload);
        }

        [Fact]
        public async Task DollarMetadata_Works_ForNullableReferencialConstraint_WithForeignKeyAttributeAndRequiredAttribute()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<FkProduct2>("Products");
            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            HttpResponseMessage response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);

            string payload = await response.Content.ReadAsStringAsync();

            // non-nullable
            Assert.Contains("<Property Name=\"SupplierId\" Type=\"Edm.String\" Nullable=\"false\" />", payload);
            Assert.Contains("<NavigationProperty Name=\"Supplier\" Type=\"Microsoft.AspNet.OData.Test.Formatter.FkSupplier2\" Nullable=\"false\">", payload);
            Assert.Contains("<ReferentialConstraint Property=\"SupplierId\" ReferencedProperty=\"Id\" />", payload);

            // nullable
            Assert.Contains("<Property Name=\"SupplierKey\" Type=\"Edm.String\" />", payload);
            Assert.Contains("<NavigationProperty Name=\"SupplierNav\" Type=\"Microsoft.AspNet.OData.Test.Formatter.FkSupplier2\">", payload);
            Assert.Contains("<ReferentialConstraint Property=\"SupplierKey\" ReferencedProperty=\"Id\" />", payload);
        }

        [Fact]
        public async Task DollarMetadata_Works_ForNullableReferencialConstraint_WithForeignKeyDiscovery()
        {
            // Arrange
            const string expect =
                "<Property Name=\"FkSupplierId\" Type=\"Edm.Int32\" Nullable=\"false\" />" +
                "<Property Name=\"FkSupplier2Id\" Type=\"Edm.String\" Nullable=\"false\" />" +
                "<Property Name=\"FkSupplier3Id\" Type=\"Edm.Int32\" />" +
                "<NavigationProperty Name=\"Supplier\" Type=\"Microsoft.AspNet.OData.Test.Formatter.FkSupplier\" Nullable=\"false\">" +
                    "<ReferentialConstraint Property=\"FkSupplierId\" ReferencedProperty=\"Id\" />" +
                "</NavigationProperty>" +
                "<NavigationProperty Name=\"Supplier2\" Type=\"Microsoft.AspNet.OData.Test.Formatter.FkSupplier2\" Nullable=\"false\">" +
                    "<ReferentialConstraint Property=\"FkSupplier2Id\" ReferencedProperty=\"Id\" />" +
                "</NavigationProperty>" +
                "<NavigationProperty Name=\"Supplier3\" Type=\"Microsoft.AspNet.OData.Test.Formatter.FkSupplier3\">" +
                    "<ReferentialConstraint Property=\"FkSupplier3Id\" ReferencedProperty=\"Id\" />" +
                "</NavigationProperty>";

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<FkProduct3>("Products");
            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            HttpResponseMessage response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains(expect, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task DollarMetadata_Works_WithReferencialConstraint_IfForeignKeyAttributeOnForeignKeyProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<ForeignCustomer2>("Customers");
            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            HttpResponseMessage response = await client.GetAsync("http://localhost/odata/$metadata");
            string payload = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);

            Assert.Contains("<OnDelete Action=\"Cascade\" />", payload);
            Assert.Contains("<ReferentialConstraint Property=\"CustomerId\" ReferencedProperty=\"Id\" />", payload);
        }

        [Fact]
        public async Task DollarMetadata_Works_WithCustomReferentialConstraints()
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
            HttpClient client = GetClient(model);

            // Act
            HttpResponseMessage response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains("<ReferentialConstraint Property=\"CustomerId\" ReferencedProperty=\"OtherCustomerKey\" />",
                await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task DollarMetadata_Works_WithOnDeleteAction()
        {
            // Arrange
            const string expect =
                "<Property Name=\"CustomerId\" Type=\"Edm.Int32\" Nullable=\"false\" />" +
                "<NavigationProperty Name=\"Customer\" Type=\"Microsoft.AspNet.OData.Test.Formatter.ForeignCustomer\" Nullable=\"false\">" +
                    "<OnDelete Action=\"Cascade\" />" +
                    "<ReferentialConstraint Property=\"CustomerId\" ReferencedProperty=\"ForeignCustomerId\" />" +
                "</NavigationProperty>";

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<ForeignCustomer>().HasKey(c => c.ForeignCustomerId).HasMany(c => c.Orders);

            builder.EntityType<ForeignOrder>().HasKey(o => o.ForeignOrderId)
                .HasRequired(o => o.Customer, (o, c) => o.CustomerId == c.ForeignCustomerId)
                .CascadeOnDelete();

            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            HttpResponseMessage response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains(expect, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task DollarMetadata_Works_WithMultipleReferentialConstraints()
        {
            // Arrange
            const string expect =
                "<NavigationProperty Name=\"Customer\" Type=\"Microsoft.AspNet.OData.Test.Formatter.MultiForeignCustomer\" Nullable=\"false\">" +
                    "<ReferentialConstraint Property=\"CustomerForeignKey1\" ReferencedProperty=\"CustomerId1\" />" +
                    "<ReferentialConstraint Property=\"CustomerForeignKey2\" ReferencedProperty=\"CustomerId2\" />" +
                "</NavigationProperty>";

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<MultiForeignCustomer>().HasKey(c => new {c.CustomerId1, c.CustomerId2}).HasMany(c => c.Orders);

            builder.EntityType<MultiForeignOrder>().HasKey(c => c.ForeignOrderId)
                .HasRequired(o => o.Customer,
                    (o, c) => o.CustomerForeignKey1 == c.CustomerId1 && o.CustomerForeignKey2 == c.CustomerId2);

            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            HttpResponseMessage response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains(expect, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task DollarMetadata_Works_WithPrincipalKeyOnBaseType_ButBaseTypeNotInEdmModel()
        {
            // Arrange
            const string expect =
                "<NavigationProperty Name=\"DerivedProp\" Type=\"Microsoft.AspNet.OData.Test.Formatter.DerivedPrincipalEntity\">" +
                    "<ReferentialConstraint Property=\"DerivedPrincipalEntityId\" ReferencedProperty=\"Id\" />" +
                "</NavigationProperty>";

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<DerivedPrincipalEntity>("Principals");
            builder.EntitySet<DependentEntity>("Dependents");
            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            HttpResponseMessage response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);

            Assert.Contains(expect, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task DollarMetadata_Works_WithMultipleReferentialConstraints_ForUntypeModel()
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
                "<Property Name=\"CustomerForeignKey\" Type=\"Edm.Int32\" />" +
                "<NavigationProperty Name=\"Customer\" Type=\"DefaultNamespace.Customer\" Partner=\"Orders\">" +
                    "<ReferentialConstraint Property=\"CustomerForeignKey\" ReferencedProperty=\"CustomerId\" />" +
                "</NavigationProperty>";
            HttpClient client = GetClient(model);

            // Act
            HttpResponseMessage response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains(expect, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task DollarMetadata_Works_WithOpenComplexType()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<FormatterAddress>();
            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            var response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains("<ComplexType Name=\"FormatterAddress\" OpenType=\"true\">",
                await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task DollarMetadata_Works_WithInheritanceOpenComplexType()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<FormatterAddress>();
            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            var response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains("<ComplexType Name=\"FormatterUsAddress\" BaseType=\"Microsoft.AspNet.OData.Test.Formatter.FormatterAddress\" OpenType=\"true\">",
                await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task DollarMetadata_Works_WithDerivedOpenComplexType()
        {
            // Arrange
            const string expectMetadata =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<edmx:Edmx Version=\"4.0\" xmlns:edmx=\"http://docs.oasis-open.org/odata/ns/edmx\">" +
            "<edmx:DataServices>" +
                "<Schema Namespace=\"Microsoft.AspNet.OData.Test.Formatter\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">" +
                    "<ComplexType Name=\"ComplexBaseType\">" +
                        "<Property Name=\"BaseProperty\" Type=\"Edm.String\" />" +
                    "</ComplexType>" +
                    "<ComplexType Name=\"ComplexDerivedOpenType\" BaseType=\"Microsoft.AspNet.OData.Test.Formatter.ComplexBaseType\" OpenType=\"true\">" +
                        "<Property Name=\"DerivedProperty\" Type=\"Edm.String\" />" +
                    "</ComplexType>" +
                "</Schema>" +
                "<Schema Namespace=\"Default\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">" +
                    "<EntityContainer Name=\"Container\" />" +
                "</Schema>" +
            "</edmx:DataServices>" +
            "</edmx:Edmx>";
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<ComplexBaseType>();
            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            var response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Equal(expectMetadata.Replace("'", "\""), await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task DollarMetadata_Works_WithActionParameterNullable_ReturnTypeNullable()
        {
            // Arrange
            const string expectMetadata =
            "<Schema Namespace=\"Default\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">" +
                "<Action Name=\"NullableAction\" IsBound=\"true\">" +
                    "<Parameter Name=\"bindingParameter\" Type=\"Microsoft.AspNet.OData.Test.Formatter.FormatterPerson\" />" +
                    "<Parameter Name=\"param\" Type=\"Edm.String\" />" +
                    "<ReturnType Type=\"Microsoft.AspNet.OData.Test.Formatter.FormatterAddress\" />" +
                "</Action>" +
                "<Action Name=\"NonNullableAction\" IsBound=\"true\">" +
                    "<Parameter Name=\"bindingParameter\" Type=\"Microsoft.AspNet.OData.Test.Formatter.FormatterPerson\" />" +
                    "<Parameter Name=\"param\" Type=\"Edm.String\" Nullable=\"false\" />" +
                    "<ReturnType Type=\"Microsoft.AspNet.OData.Test.Formatter.FormatterAddress\" Nullable=\"false\" />" +
                "</Action>" +
                "<EntityContainer Name=\"Container\" /></Schema>" +
            "</edmx:DataServices>" +
            "</edmx:Edmx>";

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            EntityTypeConfiguration<FormatterPerson> person = builder.EntityType<FormatterPerson>();

            ActionConfiguration action = person.Action("NullableAction").Returns<FormatterAddress>();
            action.Parameter<string>("param");

            action = person.Action("NonNullableAction").Returns<FormatterAddress>();
            action.ReturnNullable = false;
            action.Parameter<string>("param").Nullable = false;
            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            var response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            var result = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectMetadata.Replace("'", "\""), result);
        }

        [Fact]
        public async Task DollarMetadata_Works_WithFunctionParameterNullable_ReturnTypeNullable()
        {
            // Arrange
            const string expectMetadata =
            "<Schema Namespace=\"Default\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">" +
                "<Function Name=\"NullableFunction\" IsBound=\"true\">" +
                    "<Parameter Name=\"bindingParameter\" Type=\"Microsoft.AspNet.OData.Test.Formatter.FormatterPerson\" />" +
                    "<Parameter Name=\"param\" Type=\"Edm.String\" />" +
                    "<ReturnType Type=\"Microsoft.AspNet.OData.Test.Formatter.FormatterAddress\" />" +
                "</Function>" +
                "<Function Name=\"NonNullableFunction\" IsBound=\"true\">" +
                    "<Parameter Name=\"bindingParameter\" Type=\"Microsoft.AspNet.OData.Test.Formatter.FormatterPerson\" />" +
                    "<Parameter Name=\"param\" Type=\"Edm.String\" Nullable=\"false\" />" +
                    "<ReturnType Type=\"Microsoft.AspNet.OData.Test.Formatter.FormatterAddress\" Nullable=\"false\" />" +
                "</Function>" +
                "<EntityContainer Name=\"Container\" />" +
            "</Schema>" +
            "</edmx:DataServices>" +
            "</edmx:Edmx>";

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            EntityTypeConfiguration<FormatterPerson> person = builder.EntityType<FormatterPerson>();

            FunctionConfiguration function = person.Function("NullableFunction").Returns<FormatterAddress>();
            function.Parameter<string>("param");

            function = person.Function("NonNullableFunction").Returns<FormatterAddress>();
            function.ReturnNullable = false;
            function.Parameter<string>("param").Nullable = false;
            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            var response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains(expectMetadata.Replace("'", "\""), await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task DollarMetadata_Works_WithActionOptionalParameter()
        {
            // Arrange
            const string expectMetadata =
            "<Schema Namespace=\"Default\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">" +
                "<Action Name=\"ActionWithOptional\" IsBound=\"true\">" +
                    "<Parameter Name=\"bindingParameter\" Type=\"Microsoft.AspNet.OData.Test.Formatter.FormatterPerson\" />" +
                    "<Parameter Name=\"param1\" Type=\"Edm.String\">" +
                      "<Annotation Term=\"Org.OData.Core.V1.OptionalParameter\">" +
                        "<Record>" +
                          "<PropertyValue Property=\"DefaultValue\" String=\"A default value\" />" +
                        "</Record>" +
                      "</Annotation>" +
                    "</Parameter>" +
                    "<Parameter Name=\"param2\" Type=\"Edm.String\">" +
                      "<Annotation Term=\"Org.OData.Core.V1.OptionalParameter\" />" +
                    "</Parameter>" +
                "</Action>" +
                "<EntityContainer Name=\"Container\" /></Schema>" +
            "</edmx:DataServices>" +
            "</edmx:Edmx>";

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            EntityTypeConfiguration<FormatterPerson> person = builder.EntityType<FormatterPerson>();

            ActionConfiguration action = person.Action("ActionWithOptional");
            action.Parameter<string>("param1").HasDefaultValue("A default value");
            action.Parameter<string>("param2").Optional();
            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            var response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            var result = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectMetadata.Replace("'", "\""), result);
        }

        [Fact]
        public async Task DollarMetadata_Works_WithFunctionOptionalParameter()
        {
            // Arrange
            const string expectMetadata =
            "<Schema Namespace=\"Default\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">" +
                "<Function Name=\"FunctionWithoutOptional\" IsBound=\"true\">" +
                    "<Parameter Name=\"bindingParameter\" Type=\"Microsoft.AspNet.OData.Test.Formatter.FormatterPerson\" />" +
                    "<Parameter Name=\"param\" Type=\"Edm.String\" />" +
                    "<ReturnType Type=\"Microsoft.AspNet.OData.Test.Formatter.FormatterAddress\" />" +
                "</Function>" +
                "<Function Name=\"FunctionWithOptional\" IsBound=\"true\">" +
                    "<Parameter Name=\"bindingParameter\" Type=\"Microsoft.AspNet.OData.Test.Formatter.FormatterPerson\" />" +
                    "<Parameter Name=\"param1\" Type=\"Edm.String\">" +
                      "<Annotation Term=\"Org.OData.Core.V1.OptionalParameter\">" +
                        "<Record>" +
                          "<PropertyValue Property=\"DefaultValue\" String=\"A default value\" />" +
                        "</Record>" +
                      "</Annotation>" +
                    "</Parameter>" +
                    "<Parameter Name=\"param2\" Type=\"Edm.String\">" +
                      "<Annotation Term=\"Org.OData.Core.V1.OptionalParameter\" />" +
                    "</Parameter>" +
                    "<ReturnType Type=\"Microsoft.AspNet.OData.Test.Formatter.FormatterAddress\" />" +
                "</Function>" +
                "<EntityContainer Name=\"Container\" />" +
            "</Schema>" +
            "</edmx:DataServices>" +
            "</edmx:Edmx>";

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            EntityTypeConfiguration<FormatterPerson> person = builder.EntityType<FormatterPerson>();

            FunctionConfiguration function = person.Function("FunctionWithoutOptional").Returns<FormatterAddress>();
            function.Parameter<string>("param");

            function = person.Function("FunctionWithOptional").Returns<FormatterAddress>();
            function.Parameter<string>("param1").HasDefaultValue("A default value");
            function.Parameter<string>("param2").Optional();
            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            var response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            string a = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectMetadata.Replace("'", "\""), await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task DollarMetadata_Works_WithAbstractEntityTypeWithoutKey()
        {
            // Arrange
            const string expectMetadata =
            "<EntityType Name=\"AbstractEntityType\" Abstract=\"true\">" +
                "<Property Name=\"IntProperty\" Type=\"Edm.Int32\" Nullable=\"false\" />" +
            "</EntityType>";

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<AbstractEntityType>().Abstract().Property(a => a.IntProperty);
            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            var response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectMetadata, payload);
            Assert.DoesNotContain("<key>", payload);
        }

        [Fact]
        public async Task DollarMetadata_Works_WithDerivedEntityTypeWithOwnKeys()
        {
            // Arrange
            const string expectMetadata =
            "<Schema Namespace=\"Microsoft.AspNet.OData.Test.Formatter\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">" +
                "<EntityType Name=\"AbstractEntityType\" Abstract=\"true\">" +
                    "<Property Name=\"IntProperty\" Type=\"Edm.Int32\" Nullable=\"false\" />" +
                "</EntityType>" +
                "<EntityType Name=\"SubEntityType\" BaseType=\"Microsoft.AspNet.OData.Test.Formatter.AbstractEntityType\">" +
                    "<Key>" +
                        "<PropertyRef Name=\"SubKey\" />" +
                    "</Key>" +
                    "<Property Name=\"SubKey\" Type=\"Edm.Int32\" Nullable=\"false\" />" +
                "</EntityType>" +
                "<EntityType Name=\"AnotherSubEntityType\" BaseType=\"Microsoft.AspNet.OData.Test.Formatter.AbstractEntityType\">" +
                    "<Key>" +
                        "<PropertyRef Name=\"AnotherKey\" />" +
                    "</Key>" +
                    "<Property Name=\"AnotherKey\" Type=\"Edm.Double\" Nullable=\"false\" />" +
                "</EntityType>" +
            "</Schema>";

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<AbstractEntityType>().Abstract().Property(a => a.IntProperty);
            builder.EntityType<SubEntityType>().HasKey(b => b.SubKey).DerivesFrom<AbstractEntityType>();
            builder.EntityType<AnotherSubEntityType>().HasKey(d => d.AnotherKey).DerivesFrom<AbstractEntityType>();
            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            var response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectMetadata, payload);
        }

        [Fact]
        public async Task DollarMetadata_Works_WithEntityTypeWithEnumKeys()
        {
            // Arrange
            const string expectMetadata =
                "<EntityType Name=\"EnumModel\">" +
                    "<Key>" +
                        "<PropertyRef Name=\"Simple\" />" +
                    "</Key>" +
                    "<Property Name=\"Simple\" Type=\"NS.SimpleEnum\" Nullable=\"false\" />" +
                "</EntityType>" +
                "<EnumType Name=\"SimpleEnum\" />";

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<EnumModel>().HasKey(e => e.Simple).Namespace = "NS";
            builder.EnumType<SimpleEnum>().Namespace = "NS";
            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            var response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectMetadata, payload);
        }

        [Fact]
        public async Task DollarMetadata_Works_WithConcurrencyVocabuaryAnnotation()
        {
            // Arrange
            const string expectMetadata =
                "<EntitySet Name=\"Customers\" EntityType=\"Microsoft.AspNet.OData.Test.Formatter.CustomerWithConcurrencyAttribute\">" +
                    "<Annotation Term=\"Org.OData.Core.V1.OptimisticConcurrency\">" +
                        "<Collection>" +
                            "<PropertyPath>Name</PropertyPath>" +
                            "<PropertyPath>Birthday</PropertyPath>" +
                        "</Collection>" +
                    "</Annotation>" +
                "</EntitySet>";

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<CustomerWithConcurrencyAttribute>("Customers");
            IEdmModel model = builder.GetEdmModel();
            HttpClient client = GetClient(model);

            // Act
            var response = await client.GetAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectMetadata, payload);
        }

        [Fact]
        public async Task ServiceDocumentWorks_OutputSingleton()
        {
            // Arrange
            HttpClient client = GetClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/odata");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            var response = await client.SendAsync(request);
            var repsoneString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("\"name\":\"President\",\"kind\":\"Singleton\",\"url\":\"President\"",
                repsoneString);
        }

        [Fact]
        public async Task ServiceDocument_ContainsFunctonImport()
        {
            // Arrange
            HttpClient client = GetClient();

            // Act
            var responseString = await client.GetStringAsync("http://localhost/odata");

            // Assert
            Assert.Contains("\"name\":\"GetPerson\",\"kind\":\"FunctionImport\",\"url\":\"GetPerson\"", responseString);
            Assert.Contains("\"name\":\"GetVipPerson\",\"kind\":\"FunctionImport\",\"url\":\"GetVipPerson\"", responseString);
        }

        [Fact]
        public async Task ServiceDocument_DoesNotContainFunctonImport_IfWithParameters()
        {
            // Arrange
            IEdmModel model = ODataTestUtil.GetEdmModel();
            IEnumerable<IEdmFunctionImport> functionImports = model.EntityContainer.Elements.OfType<IEdmFunctionImport>()
                .Where(f => f.Name == "GetSalary");

            HttpClient client = GetClient(model);

            // Act
            var responseString = await client.GetStringAsync("http://localhost/odata/");

            // Assert
            var functionImport = Assert.Single(functionImports);
            Assert.Equal("Default.GetSalary", functionImport.Function.FullName());
            Assert.True(functionImport.IncludeInServiceDocument);
            Assert.Contains("\"@odata.context\":\"http://localhost/odata/$metadata\"", responseString);
            Assert.DoesNotContain("\"name\":\"GetSalary\",\"kind\":\"FunctionImport\",\"url\":\"GetSalary\"", responseString);
        }

        [Fact]
        public async Task ServiceDocument_DoesNotContainFunctonImport_IfNotIncludeInServiceDocument()
        {
            // Arrange
            IEdmModel model = ODataTestUtil.GetEdmModel();
            IEdmFunctionImport[] functionImports = model.EntityContainer.Elements.OfType<IEdmFunctionImport>()
                .Where(f => f.Name == "GetAddress").ToArray();

            HttpClient client = GetClient(model);

            // Act
            var responseString = await client.GetStringAsync("http://localhost/odata/");

            // Assert
            Assert.Equal(2, functionImports.Length);

            Assert.Equal("Default.GetAddress", functionImports[0].Function.FullName());
            Assert.Equal("Default.GetAddress", functionImports[1].Function.FullName());

            Assert.False(functionImports[0].IncludeInServiceDocument);
            Assert.False(functionImports[1].IncludeInServiceDocument);

            Assert.Empty(functionImports[0].Function.Parameters);
            Assert.Equal("AddressId", functionImports[1].Function.Parameters.First().Name);

            Assert.Contains("\"@odata.context\":\"http://localhost/odata/$metadata\"", responseString);
            Assert.DoesNotContain("\"name\":\"GetAddress\",\"kind\":\"FunctionImport\",\"url\":\"GetAddress\"", responseString);
        }

        [Fact]
        public async Task ServiceDocument_OnlyContainOneFunctonImport_ForOverloadFunctions()
        {
            // Arrange
            IEdmModel model = ODataTestUtil.GetEdmModel();
            IEdmFunctionImport[] functionImports = model.EntityContainer.Elements.OfType<IEdmFunctionImport>()
                .Where(f => f.Name == "GetVipPerson").ToArray();

            HttpClient client = GetClient(model);

            // Act
            var responseString = await client.GetStringAsync("http://localhost/odata/");

            // Assert
            Assert.Equal(3, functionImports.Length);

            Assert.Equal("Default.GetVipPerson", functionImports[0].Function.FullName());
            Assert.Equal("Default.GetVipPerson", functionImports[1].Function.FullName());
            Assert.Equal("Default.GetVipPerson", functionImports[2].Function.FullName());

            Assert.Single(functionImports[0].Function.Parameters);
            Assert.Empty(functionImports[1].Function.Parameters);
            Assert.Equal(2, functionImports[2].Function.Parameters.Count());

            Assert.Contains("\"@odata.context\":\"http://localhost/odata/$metadata\"", responseString);
            Assert.Contains("\"name\":\"GetVipPerson\",\"kind\":\"FunctionImport\",\"url\":\"GetVipPerson\"", responseString);
        }

        [Fact]
        public async Task ServiceDocument_FunctionNamespace_Configuration()
        {
            // Arrange
            HttpClient client = GetClient();

            // Act
            var response = await client.GetAsync("http://localhost/odata/$metadata");
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("CustomizeNamepace.GetNS", responseString);
            Assert.Contains("Namespace=\"CustomizeNamepace\"", responseString);
        }

        [Fact]
        public async Task RequiredAttribute_Works_OnComplexTypeProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<FormatterAccount>("Accounts");

            HttpClient client = GetClient(builder.GetEdmModel());

            // Act
            var responseString = await client.GetStringAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.Contains(
                "<Property Name=\"Address\" Type=\"Microsoft.AspNet.OData.Test.Formatter.FormatterAddress\" Nullable=\"false\" />",
                responseString);

            Assert.Contains(
                "<Property Name=\"Addresses\" Type=\"Collection(Microsoft.AspNet.OData.Test.Formatter.FormatterAddress)\" Nullable=\"false\" />",
                responseString);
        }

        [Fact]
        public async Task DollarMetadata_Works_WithNavigationPropertyBindingOnMultiplePath()
        {
            // Arrange
          const string expectMetadata =
            "<EntityContainer Name=\"Container\">" +
                "<EntitySet Name=\"Customers\" EntityType=\"Microsoft.AspNet.OData.Test.Formatter.BindingCustomer\">" +
                    "<NavigationPropertyBinding Path=\"Location/City\" Target=\"Cities\" />" +
                "</EntitySet>" +
                "<EntitySet Name=\"Cities\" EntityType=\"Microsoft.AspNet.OData.Test.Formatter.BindingCity\" />" +
            "</EntityContainer>";

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<BindingCustomer>().HasKey(c => c.Id);
            builder.EntityType<BindingCity>().HasKey(c => c.Id);

            builder
                .EntitySet<BindingCustomer>("Customers")
                .Binding
                .HasSinglePath(c => c.Location)
                .HasRequiredBinding(a => a.City, "Cities");

            HttpClient client = GetClient(builder.GetEdmModel());

            // Act
            var responseString = await client.GetStringAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.Contains(expectMetadata, responseString);
        }

        [Fact]
        public async Task DollarMetadata_Works_WithNavigationPropertyBindingOnMultiplePath_WithDerived()
        {
            // Arrange
            const string expectMetadata =
              "<EntityContainer Name=\"Container\">" +
                  "<EntitySet Name=\"Customers\" EntityType=\"Microsoft.AspNet.OData.Test.Formatter.BindingCustomer\">" +
                      "<NavigationPropertyBinding Path=\"Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipLocation/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/UsCities\" Target=\"Cities_B\" />" +
                      "<NavigationPropertyBinding Path=\"Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipLocation/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/UsCity\" Target=\"Cities_A\" />" +
                  "</EntitySet>" +
                  "<EntitySet Name=\"Cities_A\" EntityType=\"Microsoft.AspNet.OData.Test.Formatter.BindingCity\" />" +
                  "<EntitySet Name=\"Cities_B\" EntityType=\"Microsoft.AspNet.OData.Test.Formatter.BindingCity\" />" +
              "</EntityContainer>";

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<BindingCustomer>().HasKey(c => c.Id);
            builder.EntityType<BindingCity>().HasKey(c => c.Id);

            var bindingConfiguration = builder
                .EntitySet<BindingCustomer>("Customers")
                .Binding
                .HasSinglePath((BindingVipCustomer v) => v.VipLocation);

            bindingConfiguration.HasOptionalBinding((BindingUsAddress u) => u.UsCity, "Cities_A");
            bindingConfiguration.HasManyBinding((BindingUsAddress u) => u.UsCities, "Cities_B");

            HttpClient client = GetClient(builder.GetEdmModel());

            // Act
            var responseString = await client.GetStringAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.Contains(expectMetadata, responseString);
        }

        [Fact]
        public async Task DollarMetadata_Works_WithNavigationPropertyBindingOnMultiplePath_ConventionModelBuilder()
        {
            // Arrange
            const string expectMetadata =
              "<EntityContainer Name=\"Container\">" +
                  "<EntitySet Name=\"Customers\" EntityType=\"Microsoft.AspNet.OData.Test.Formatter.BindingCustomer\">" +
                      "<NavigationPropertyBinding Path=\"Address/Cities\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Address/City\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Address/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/UsCities\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Address/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/UsCity\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Addresses/Cities\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Addresses/City\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Addresses/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/UsCities\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Addresses/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/UsCity\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Location/Cities\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Location/City\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Location/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/UsCities\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Location/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/UsCity\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipAddresses/Cities\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipAddresses/City\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipAddresses/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/UsCities\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipAddresses/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/UsCity\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipLocation/Cities\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipLocation/City\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipLocation/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/UsCities\" Target=\"Cities\" />" +
                      "<NavigationPropertyBinding Path=\"Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipLocation/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/UsCity\" Target=\"Cities\" />" +
                  "</EntitySet>" +
                  "<EntitySet Name=\"Cities\" EntityType=\"Microsoft.AspNet.OData.Test.Formatter.BindingCity\" />" +
              "</EntityContainer>";

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<BindingCustomer>("Customers");
            builder.EntitySet<BindingCity>("Cities");

            HttpClient client = GetClient(builder.GetEdmModel());

            // Act
            var responseString = await client.GetStringAsync("http://localhost/odata/$metadata");

            // Assert
            Assert.Contains(expectMetadata, responseString);
        }

        private static async Task AssertHasEntitySet(HttpClient client, string uri, string entitySetName)
        {
            var response = await client.GetAsync(uri);
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Contains(entitySetName, await response.Content.ReadAsStringAsync());
        }

        private HttpClient GetClient(IEdmModel model = null)
        {
            var server = TestServerFactory.Create(new[] { typeof(MetadataController) }, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", model ?? ODataTestUtil.GetEdmModel());
            });
            return TestServerFactory.CreateClient(server);
        }
    }
}
