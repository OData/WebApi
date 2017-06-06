// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter.Serialization;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder.Conventions
{
    public class FunctionLinkGenerationConventionTest
    {
        [Fact]
        public void Apply_SetOperationLinkBuilder_ForFunctionBoundToEntity()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var function = builder.EntityType<Customer>().Function("MyFunction").Returns<int>();
            function.Parameter<string>("param");

            // Act
            Assert.Null(function.GetFunctionLink()); // Guard
            Assert.Null(function.GetFeedFunctionLink()); // Guard

            FunctionLinkGenerationConvention convention = new FunctionLinkGenerationConvention();
            convention.Apply(function, builder);

            // Assert
            var functionLink = function.GetFunctionLink();
            Assert.NotNull(functionLink);
            Assert.IsType<Func<ResourceContext, Uri>>(functionLink);

            Assert.Null(function.GetFeedFunctionLink());
        }

        [Fact]
        public void Convention_GeneratesUri_ForFunctionBoundToEntity()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            var function = builder.EntityType<Customer>().Function("MyFunction").Returns<int>();
            function.Parameter<string>("param");
            IEdmModel model = builder.GetEdmModel();

            // Act
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.MapODataServiceRoute("odata", "odata", model);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:123");
            request.SetConfiguration(configuration);
            request.EnableODataDependencyInjectionSupport("odata");

            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            var edmType = model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "Customer");
            var serializerContext = new ODataSerializerContext { Model = model, NavigationSource = customers, Url = request.GetUrlHelper() };
            var entityContext = new ResourceContext(serializerContext, edmType.AsReference(), new Customer { Id = 109 });

            // Assert
            var edmFunction = model.SchemaElements.OfType<IEdmFunction>().First(f => f.Name == "MyFunction");
            Assert.NotNull(edmFunction);

            OperationLinkBuilder functionLinkBuilder = model.GetOperationLinkBuilder(edmFunction);
            Uri link = functionLinkBuilder.BuildLink(entityContext);

            Assert.Equal("http://localhost:123/odata/Customers(109)/Default.MyFunction(param=@param)",
                link.AbsoluteUri);
        }

        [Fact]
        public void Apply_WorksFor_FunctionBoundToCollectionOfEntity()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var function = builder.EntityType<Customer>().Collection.Function("MyFunction").Returns<int>();
            function.Parameter<string>("param");

            // Act
            Assert.Null(function.GetFunctionLink()); // Guard
            Assert.Null(function.GetFeedFunctionLink()); // Guard

            FunctionLinkGenerationConvention convention = new FunctionLinkGenerationConvention();
            convention.Apply(function, builder);

            // Assert
            var functionLink = function.GetFeedFunctionLink();
            Assert.NotNull(functionLink);
            Assert.IsType<Func<ResourceSetContext, Uri>>(functionLink);

            Assert.Null(function.GetFunctionLink());
        }

        [Fact]
        public void Convention_GeneratesUri_ForFunctionBoundToCollectionOfEntity()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            var function = builder.EntityType<Customer>().Collection.Function("MyFunction").Returns<int>();
            function.Parameter<string>("param");
            IEdmModel model = builder.GetEdmModel();

            // Act
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.MapODataServiceRoute("odata", "odata", model);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:123");
            request.SetConfiguration(configuration);
            request.EnableODataDependencyInjectionSupport("odata");

            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            var entityContext = new ResourceSetContext { EntitySetBase = customers, Request = request, Url = request.GetUrlHelper() };

            // Assert
            var edmFunction = model.SchemaElements.OfType<IEdmFunction>().First(f => f.Name == "MyFunction");
            Assert.NotNull(edmFunction);

            OperationLinkBuilder functionLinkBuilder = model.GetOperationLinkBuilder(edmFunction);
            Uri link = functionLinkBuilder.BuildLink(entityContext);

            Assert.Equal("http://localhost:123/odata/Customers/Default.MyFunction(param=@param)",
                link.AbsoluteUri);
        }

        [Fact]
        public void Apply_Doesnot_Override_UserConfiguration()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var customers = builder.EntitySet<Customer>("Customers");
            var function = customers.EntityType.Function("MyFunction").Returns<int>();
            function.HasFunctionLink(ctx => new Uri("http://localhost/FunctionTestWorks"), followsConventions: false);
            FunctionLinkGenerationConvention convention = new FunctionLinkGenerationConvention();
            convention.Apply(function, builder);

            // Act
            IEdmModel model = builder.GetEdmModel();
            var edmCustomers = model.EntityContainer.FindEntitySet("Customers");
            var edmType = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
            var edmFunction = model.SchemaElements.OfType<IEdmFunction>().Single(f => f.Name == "MyFunction");
            Assert.NotNull(edmFunction);

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.MapODataServiceRoute(model);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            request.SetConfiguration(configuration);

            OperationLinkBuilder fuinctionLinkBuilder = model.GetOperationLinkBuilder(edmFunction);

            var serializerContext = new ODataSerializerContext { Model = model, NavigationSource = edmCustomers, Url = request.GetUrlHelper() };
            var entityContext = new ResourceContext(serializerContext, edmType.AsReference(), new Customer { Id = 109 });

            // Assert
            Uri link = fuinctionLinkBuilder.BuildLink(entityContext);
            Assert.Equal("http://localhost/FunctionTestWorks", link.AbsoluteUri);
        }

        [Fact]
        public void Apply_SetsOperationLinkBuilder_OnlyIfFunctionIsBindable()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var function = builder.Function("MyFunction").Returns<int>();
            ActionLinkGenerationConvention convention = new ActionLinkGenerationConvention();

            // Act
            convention.Apply(function, builder);

            // Assert
            IEdmModel model = builder.GetEdmModel();
            var edmFunction = model.EntityContainer.Elements.OfType<IEdmFunctionImport>().Single();
            Assert.NotNull(edmFunction);

            OperationLinkBuilder linkBuilder = model.GetAnnotationValue<OperationLinkBuilder>(edmFunction);
            Assert.Null(linkBuilder);
        }

        [Fact]
        public void Apply_FollowsConventions()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            FunctionConfiguration function = new FunctionConfiguration(builder, "IgnoreAction");
            Mock<IEdmTypeConfiguration> mockBindingParameterType = new Mock<IEdmTypeConfiguration>();
            mockBindingParameterType.Setup(o => o.Kind).Returns(EdmTypeKind.Entity);
            mockBindingParameterType.Setup(o => o.ClrType).Returns(typeof(int));
            function.SetBindingParameter("IgnoreParameter", mockBindingParameterType.Object);
            FunctionLinkGenerationConvention convention = new FunctionLinkGenerationConvention();

            // Act
            convention.Apply(function, builder);

            // Assert
            Assert.True(function.FollowsConventions);
        }

        public class Customer
        {
            public int Id { get; set; }
        }
    }
}
