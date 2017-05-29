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
    public class ActionLinkGenerationConventionTest
    {
        [Fact]
        public void Apply_SetFunctionLinkBuilder_ForActionBoundToEntity()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var action = builder.EntityType<Customer>().Action("MyAction");

            // Act
            Assert.Null(action.GetActionLink()); // Guard
            Assert.Null(action.GetFeedActionLink()); // Guard

            ActionLinkGenerationConvention convention = new ActionLinkGenerationConvention();
            convention.Apply(action, builder);

            // Assert
            var actionLink = action.GetActionLink();
            Assert.NotNull(actionLink);
            Assert.IsType<Func<EntityInstanceContext, Uri>>(actionLink);

            Assert.Null(action.GetFeedActionLink());
        }

        [Fact]
        public void Convention_GeneratesUri_ForActionBoundToEntity()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            var action = builder.EntityType<Customer>().Action("MyAction");
            action.Parameter<string>("param");
            IEdmModel model = builder.GetEdmModel();

            // Act
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.MapODataServiceRoute("odata", "odata", model);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:123");
            request.SetConfiguration(configuration);
            request.ODataProperties().RouteName = "odata";

            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            var edmType = model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "Customer");
            var serializerContext = new ODataSerializerContext { Model = model, NavigationSource = customers, Url = request.GetUrlHelper() };
            var entityContext = new EntityInstanceContext(serializerContext, edmType.AsReference(), new Customer { Id = 109 });

            // Assert
            var edmAction = model.SchemaElements.OfType<IEdmAction>().First(f => f.Name == "MyAction");
            Assert.NotNull(edmAction);

            ActionLinkBuilder actionLinkBuilder = model.GetActionLinkBuilder(edmAction);
            Uri link = actionLinkBuilder.BuildActionLink(entityContext);

            Assert.Equal("http://localhost:123/odata/Customers(109)/Default.MyAction", link.AbsoluteUri);
        }

        [Fact]
        public void Apply_WorksFor_ActionBoundToCollectionOfEntity()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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
            Assert.IsType<Func<FeedContext, Uri>>(actionLink);

            Assert.Null(action.GetActionLink());
        }

        [Fact]
        public void Convention_GeneratesUri_ForActionBoundToCollectionOfEntity()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            var action = builder.EntityType<Customer>().Collection.Action("MyAction").Returns<int>();
            action.Parameter<string>("param");
            IEdmModel model = builder.GetEdmModel();

            // Act
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.MapODataServiceRoute("odata", "odata", model);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:123");
            request.SetConfiguration(configuration);
            request.ODataProperties().RouteName = "odata";

            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            var entityContext = new FeedContext { EntitySetBase = customers, Request = request, Url = request.GetUrlHelper() };

            // Assert
            var edmAction = model.SchemaElements.OfType<IEdmAction>().First(f => f.Name == "MyAction");
            Assert.NotNull(edmAction);

            ActionLinkBuilder actionLinkBuilder = model.GetActionLinkBuilder(edmAction);
            Uri link = actionLinkBuilder.BuildActionLink(entityContext);

            Assert.Equal("http://localhost:123/odata/Customers/Default.MyAction", link.AbsoluteUri);
        }

        [Fact]
        public void Apply_Doesnot_Override_UserConfiguration()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.MapODataServiceRoute(model);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            request.SetConfiguration(configuration);

            ActionLinkBuilder actionLinkBuilder = model.GetActionLinkBuilder(edmAction);

            var serializerContext = new ODataSerializerContext { Model = model, NavigationSource = edmCustomers, Url = request.GetUrlHelper() };
            var entityContext = new EntityInstanceContext(serializerContext, edmCustomer.AsReference(), new Customer { Id = 2009 });

            // Assert
            Uri link = actionLinkBuilder.BuildActionLink(entityContext);
            Assert.Equal("http://localhost/ActionTestWorks", link.AbsoluteUri);
        }

        [Fact]
        public void Apply_SetsActionLinkBuilder_OnlyIfActionIsBindable()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            var paintAction = builder.Action("Paint");
            ActionLinkGenerationConvention convention = new ActionLinkGenerationConvention();

            // Act
            convention.Apply(paintAction, builder);

            // Assert
            IEdmModel model = builder.GetEdmModel();
            var paintEdmAction = model.EntityContainer.Elements.OfType<IEdmActionImport>().Single();

            ActionLinkBuilder actionLinkBuilder = model.GetAnnotationValue<ActionLinkBuilder>(paintEdmAction);

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
