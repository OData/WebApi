//-----------------------------------------------------------------------------
// <copyright file="ActionLinkGenerationConventionTest.cs" company=".NET Foundation">
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
    public class ActionLinkGenerationConventionTest
    {
        [Fact]
        public void Apply_SetOperationLinkBuilder_ForActionBoundToEntity()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            var action = builder.EntityType<Customer>().Action("MyAction");

            // Act
            Assert.Null(action.GetActionLink()); // Guard
            Assert.Null(action.GetFeedActionLink()); // Guard

            ActionLinkGenerationConvention convention = new ActionLinkGenerationConvention();
            convention.Apply(action, builder);

            // Assert
            var actionLink = action.GetActionLink();
            Assert.NotNull(actionLink);
            Assert.IsType<Func<ResourceContext, Uri>>(actionLink);

            Assert.Null(action.GetFeedActionLink());
        }

        [Fact]
        public void Convention_GeneratesUri_ForActionBoundToEntity()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");
            var action = builder.EntityType<Customer>().Action("MyAction");
            action.Parameter<string>("param");
            IEdmModel model = builder.GetEdmModel();

            // Act
            var configuration = RoutingConfigurationFactory.Create();
            configuration.MapODataServiceRoute("odata", "odata", model);

            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost:123", configuration, "odata");

            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            var edmType = model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "Customer");
            var serializerContext = ODataSerializerContextFactory.Create(model, customers, request);
            var resourceContext = new ResourceContext(serializerContext, edmType.AsReference(), new Customer { Id = 109 });

            // Assert
            var edmAction = model.SchemaElements.OfType<IEdmAction>().First(f => f.Name == "MyAction");
            Assert.NotNull(edmAction);

            OperationLinkBuilder actionLinkBuilder = model.GetOperationLinkBuilder(edmAction);
            Uri link = actionLinkBuilder.BuildLink(resourceContext);

            Assert.Equal("http://localhost:123/odata/Customers(109)/Default.MyAction", link.AbsoluteUri);
        }

        [Fact]
        public void Convention_GeneratesUri_ForActionBoundToEntity_UsingCamelCase()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.OnModelCreating += ApplyLowerCamelCase;
            builder.EntitySet<Customer>("Customers");
            var action = builder.EntityType<Customer>().Action("simpleAction");
            action.Parameter<string>("param");
            IEdmModel model = builder.GetEdmModel();

            // Act
            var configuration = RoutingConfigurationFactory.Create();
            configuration.MapODataServiceRoute("odata", "odata", model);

            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost:123", configuration, "odata");

            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            var edmType = model.SchemaElements.OfType<IEdmEntityType>().FirstOrDefault(e => e.Name == "Customer");
            Assert.Null(edmType); // guard, since it's lower camel case, it should be null.
            edmType = model.SchemaElements.OfType<IEdmEntityType>().FirstOrDefault(e => e.Name == "customer");
            Assert.NotNull(edmType);

            var serializerContext = ODataSerializerContextFactory.Create(model, customers, request);
            var resourceContext = new ResourceContext(serializerContext, edmType.AsReference(), new Customer { Id = 109 });

            // Assert
            var edmAction = model.SchemaElements.OfType<IEdmAction>().First(f => f.Name == "simpleAction");
            Assert.NotNull(edmAction);

            OperationLinkBuilder actionLinkBuilder = model.GetOperationLinkBuilder(edmAction);
            Uri link = actionLinkBuilder.BuildLink(resourceContext);

            Assert.Equal("http://localhost:123/odata/Customers(109)/Default.simpleAction", link.AbsoluteUri);
        }

        internal static void ApplyLowerCamelCase(ODataConventionModelBuilder builder)
        {
            LowerCamelCaser lowerCamelCaser = new LowerCamelCaser();

            // handle structural types & their properties
            foreach (StructuralTypeConfiguration type in builder.StructuralTypes)
            {
                type.Name = lowerCamelCaser.ToLowerCamelCase(type.Name);
                foreach (PropertyConfiguration property in type.Properties)
                {
                    property.Name = lowerCamelCaser.ToLowerCamelCase(property.Name);
                }
            }
        }

        [Fact]
        public void Apply_WorksFor_ActionBoundToCollectionOfEntity()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            var action = builder.EntityType<Customer>().Collection.Action("MyFunction").Returns<int>();
            action.Parameter<string>("param");

            // Act
            Assert.Null(action.GetActionLink()); // Guard
            Assert.Null(action.GetFeedActionLink()); // Guard

            ActionLinkGenerationConvention convention = new ActionLinkGenerationConvention();
            convention.Apply(action, builder);

            // Assert
            var actionLink = action.GetFeedActionLink();
            Assert.NotNull(actionLink);
            Assert.IsType<Func<ResourceSetContext, Uri>>(actionLink);

            Assert.Null(action.GetActionLink());
        }

#if !NETCORE // TODO 939: This crashes on AspNetCore
        [Fact]
        public void Convention_GeneratesUri_ForActionBoundToCollectionOfEntity()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");
            var action = builder.EntityType<Customer>().Collection.Action("MyAction").Returns<int>();
            action.Parameter<string>("param");
            IEdmModel model = builder.GetEdmModel();

            // Act
            var configuration = RoutingConfigurationFactory.Create();
            configuration.MapODataServiceRoute("odata", "odata", model);

            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost:123", configuration, "odata");

            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            var entityContext = ResourceSetContextFactory.Create(customers, request);

            // Assert
            var edmAction = model.SchemaElements.OfType<IEdmAction>().First(f => f.Name == "MyAction");
            Assert.NotNull(edmAction);

            OperationLinkBuilder actionLinkBuilder = model.GetOperationLinkBuilder(edmAction);
            Uri link = actionLinkBuilder.BuildLink(entityContext);

            Assert.Equal("http://localhost:123/odata/Customers/Default.MyAction", link.AbsoluteUri);
        }
#endif

        [Fact]
        public void Apply_Doesnot_Override_UserConfiguration()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            var customers = builder.EntitySet<Customer>("Customers");
            var paintAction = customers.EntityType.Action("Paint");
            paintAction.HasActionLink(ctxt => new Uri("http://localhost/ActionTestWorks"), followsConventions: false);
            ActionLinkGenerationConvention convention = new ActionLinkGenerationConvention();

            // Act
            convention.Apply(paintAction, builder);

            IEdmModel model = builder.GetEdmModel();
            var edmCustomers = model.EntityContainer.FindEntitySet("Customers");
            var edmCustomer = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
            var edmAction = model.SchemaElements.OfType<IEdmAction>().First(a => a.Name == "Paint");
            Assert.NotNull(edmAction);

            string routeName = "OData";
            var configuration = RoutingConfigurationFactory.CreateWithRootContainer(routeName);
            configuration.MapODataServiceRoute(routeName, null, model);

            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost", configuration, routeName);

            OperationLinkBuilder actionLinkBuilder = model.GetOperationLinkBuilder(edmAction);

            var serializerContext = ODataSerializerContextFactory.Create(model, edmCustomers, request);
            var entityContext = new ResourceContext(serializerContext, edmCustomer.AsReference(), new Customer { Id = 2009 });

            // Assert
            Uri link = actionLinkBuilder.BuildLink(entityContext);
            Assert.Equal("http://localhost/ActionTestWorks", link.AbsoluteUri);
        }

        [Fact]
        public void Apply_SetsOperationLinkBuilder_OnlyIfActionIsBindable()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");
            var paintAction = builder.Action("Paint");
            ActionLinkGenerationConvention convention = new ActionLinkGenerationConvention();

            // Act
            convention.Apply(paintAction, builder);

            // Assert
            IEdmModel model = builder.GetEdmModel();
            var paintEdmAction = model.EntityContainer.Elements.OfType<IEdmActionImport>().Single();

            OperationLinkBuilder actionLinkBuilder = model.GetAnnotationValue<OperationLinkBuilder>(paintEdmAction);

            Assert.Null(actionLinkBuilder);
        }

        [Fact]
        public void Apply_FollowsConventions()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = new ActionConfiguration(builder, "IgnoreAction");
            Mock<IEdmTypeConfiguration> mockBindingParameterType = new Mock<IEdmTypeConfiguration>();
            mockBindingParameterType.Setup(o => o.Kind).Returns(EdmTypeKind.Entity);
            mockBindingParameterType.Setup(o => o.ClrType).Returns(typeof(int));
            action.SetBindingParameter("IgnoreParameter", mockBindingParameterType.Object);
            ActionLinkGenerationConvention convention = new ActionLinkGenerationConvention();

            // Act
            convention.Apply(action, builder);

            // Assert
            Assert.True(action.FollowsConventions);
        }

        public class Customer
        {
            public int Id { get; set; }
        }
    }
}
