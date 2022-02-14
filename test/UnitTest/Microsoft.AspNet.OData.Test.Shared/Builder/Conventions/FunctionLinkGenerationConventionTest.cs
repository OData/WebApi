//-----------------------------------------------------------------------------
// <copyright file="FunctionLinkGenerationConventionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Builder.Conventions;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder.Conventions
{
    public class FunctionLinkGenerationConventionTest
    {
        [Fact]
        public void Apply_SetOperationLinkBuilder_ForFunctionBoundToEntity()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
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
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");
            var function = builder.EntityType<Customer>().Function("MyFunction").Returns<int>();
            function.Parameter<string>("param");
            IEdmModel model = builder.GetEdmModel();

            // Act
            var configuration = RoutingConfigurationFactory.Create();
            configuration.MapODataServiceRoute("odata", "odata", model);

            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost:123", configuration, "odata");

            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            var edmType = model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "Customer");
            var serializerContext = ODataSerializerContextFactory.Create(model, customers, request);
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
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
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

#if !NETCORE // TODO 939: This crashes on AspNetCore
        [Fact]
        public void Convention_GeneratesUri_ForFunctionBoundToCollectionOfEntity()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");
            var function = builder.EntityType<Customer>().Collection.Function("MyFunction").Returns<int>();
            function.Parameter<string>("param");
            IEdmModel model = builder.GetEdmModel();

            // Act
            var configuration = RoutingConfigurationFactory.Create();
            configuration.MapODataServiceRoute("odata", "odata", model);

            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost:123", configuration, "odata");

            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            var entityContext = ResourceSetContextFactory.Create(customers, request);

            // Assert
            var edmFunction = model.SchemaElements.OfType<IEdmFunction>().First(f => f.Name == "MyFunction");
            Assert.NotNull(edmFunction);

            OperationLinkBuilder functionLinkBuilder = model.GetOperationLinkBuilder(edmFunction);
            Uri link = functionLinkBuilder.BuildLink(entityContext);

            Assert.Equal("http://localhost:123/odata/Customers/Default.MyFunction(param=@param)",
                link.AbsoluteUri);
        }
#endif

        [Fact]
        public void Apply_Doesnot_Override_UserConfiguration()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
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

            string routeName = "OData";
            var configuration = RoutingConfigurationFactory.CreateWithRootContainer(routeName);
            configuration.MapODataServiceRoute(routeName, null, model);

            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost:123", configuration, routeName);

            OperationLinkBuilder fuinctionLinkBuilder = model.GetOperationLinkBuilder(edmFunction);

            var serializerContext = ODataSerializerContextFactory.Create(model, edmCustomers, request);
            var entityContext = new ResourceContext(serializerContext, edmType.AsReference(), new Customer { Id = 109 });

            // Assert
            Uri link = fuinctionLinkBuilder.BuildLink(entityContext);
            Assert.Equal("http://localhost/FunctionTestWorks", link.AbsoluteUri);
        }

        [Fact]
        public void Apply_SetsOperationLinkBuilder_OnlyIfFunctionIsBindable()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
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
