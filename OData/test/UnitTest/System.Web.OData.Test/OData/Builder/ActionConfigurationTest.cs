// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder
{
    public class ActionConfigurationTest
    {
        [Fact]
        public void CanCreateActionWithNoArguments()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.Namespace = "MyNamespace";
            builder.ContainerName = "MyContainer";
            ActionConfiguration action = builder.Action("Format");
            ActionConfiguration actionII = builder.Action("FormatII");
            actionII.Namespace = "MyNamespaceII";

            // Assert
            Assert.Equal("Format", action.Name);
            Assert.Equal(OperationKind.Action, action.Kind);
            Assert.NotNull(action.Parameters);
            Assert.Empty(action.Parameters);
            Assert.Null(action.ReturnType);
            Assert.True(action.IsSideEffecting);
            Assert.False(action.IsComposable);
            Assert.False(action.IsBindable);
            Assert.Equal("MyNamespace", action.Namespace);
            Assert.Equal("MyNamespace.Format", action.FullyQualifiedName);
            Assert.Equal("MyNamespaceII", actionII.Namespace);
            Assert.Equal("MyNamespaceII.FormatII", actionII.FullyQualifiedName);
            Assert.NotNull(builder.Operations);
            Assert.Equal(2, builder.Operations.Count());
        }

        [Fact]
        public void AttemptToRemoveNonExistantEntityReturnsFalse()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            ODataModelBuilder builder2 = new ODataModelBuilder();
            OperationConfiguration toRemove = builder2.Action("ToRemove");

            // Act
            bool removedByName = builder.RemoveOperation("ToRemove");
            bool removed = builder.RemoveOperation(toRemove);

            //Assert
            Assert.False(removedByName);
            Assert.False(removed);
        }

        [Fact]
        public void CanCreateActionWithPrimitiveReturnType()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = builder.Action("CreateMessage");
            action.Returns<string>();

            // Assert
            Assert.NotNull(action.ReturnType);
            Assert.Equal("Edm.String", action.ReturnType.FullName);
        }

        [Fact]
        public void CanCreateActionWithPrimitiveCollectionReturnType()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = builder.Action("CreateMessages");
            action.ReturnsCollection<string>();

            // Assert
            Assert.NotNull(action.ReturnType);
            Assert.Equal("Collection(Edm.String)", action.ReturnType.FullName);
        }

        [Fact]
        public void CanCreateActionWithComplexReturnType()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();

            ActionConfiguration createAddress = builder.Action("CreateAddress").Returns<Address>();
            ActionConfiguration createAddresses = builder.Action("CreateAddresses").ReturnsCollection<Address>();

            // Assert
            ComplexTypeConfiguration address = createAddress.ReturnType as ComplexTypeConfiguration;
            Assert.NotNull(address);
            Assert.Equal(typeof(Address).FullName, address.FullName);
            Assert.Null(createAddress.NavigationSource);

            CollectionTypeConfiguration addresses = createAddresses.ReturnType as CollectionTypeConfiguration;
            Assert.NotNull(addresses);
            Assert.Equal(string.Format("Collection({0})", typeof(Address).FullName), addresses.FullName);
            address = addresses.ElementType as ComplexTypeConfiguration;
            Assert.NotNull(address);
            Assert.Equal(typeof(Address).FullName, address.FullName);
            Assert.Null(createAddresses.NavigationSource);
        }

        [Fact]
        public void CanCreateActionWithEntityReturnType()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();

            ActionConfiguration createGoodCustomer = builder.Action("CreateGoodCustomer").ReturnsFromEntitySet<Customer>("GoodCustomers");
            ActionConfiguration createBadCustomers = builder.Action("CreateBadCustomers").ReturnsCollectionFromEntitySet<Customer>("BadCustomers");

            // Assert
            EntityTypeConfiguration customer = createGoodCustomer.ReturnType as EntityTypeConfiguration;
            Assert.NotNull(customer);
            Assert.Equal(typeof(Customer).FullName, customer.FullName);
            EntitySetConfiguration goodCustomers = builder.EntitySets.SingleOrDefault(s => s.Name == "GoodCustomers");
            Assert.NotNull(goodCustomers);
            Assert.Same(createGoodCustomer.NavigationSource, goodCustomers);

            CollectionTypeConfiguration customers = createBadCustomers.ReturnType as CollectionTypeConfiguration;
            Assert.NotNull(customers);
            customer = customers.ElementType as EntityTypeConfiguration;
            Assert.NotNull(customer);
            EntitySetConfiguration badCustomers = builder.EntitySets.SingleOrDefault(s => s.Name == "BadCustomers");
            Assert.NotNull(badCustomers);
            Assert.Same(createBadCustomers.NavigationSource, badCustomers);
        }

        [Fact]
        public void CanCreateActionThatBindsToEntity()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            ActionConfiguration sendEmail = customer.Action("SendEmail");

            // Assert
            Assert.True(sendEmail.IsBindable);
            Assert.NotNull(sendEmail.Parameters);
            Assert.Equal(1, sendEmail.Parameters.Count());
            Assert.Equal(BindingParameterConfiguration.DefaultBindingParameterName, sendEmail.Parameters.Single().Name);
            Assert.Equal(typeof(Customer).FullName, sendEmail.Parameters.Single().TypeConfiguration.FullName);
        }

        [Fact]
        public void CanCreateActionThatBindsToEntityCollection()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            ActionConfiguration sendEmail = customer.Collection.Action("SendEmail");

            // Assert
            Assert.True(sendEmail.IsBindable);
            Assert.NotNull(sendEmail.Parameters);
            Assert.Equal(1, sendEmail.Parameters.Count());
            Assert.Equal(BindingParameterConfiguration.DefaultBindingParameterName, sendEmail.Parameters.Single().Name);
            Assert.Equal(string.Format("Collection({0})", typeof(Customer).FullName), sendEmail.Parameters.Single().TypeConfiguration.FullName);
        }

        [Fact]
        public void CanCreateActionWithNonbindingParameters_AddParameterGenericMethod()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = builder.Action("MyAction");
            action.Parameter<string>("p0");
            action.Parameter<int>("p1");
            action.Parameter<Address>("p2");
            ParameterConfiguration[] parameters = action.Parameters.ToArray();

            // Assert
            Assert.Equal(3, parameters.Length);
            Assert.Equal("p0", parameters[0].Name);
            Assert.Equal("Edm.String", parameters[0].TypeConfiguration.FullName);
            Assert.Equal("p1", parameters[1].Name);
            Assert.Equal("Edm.Int32", parameters[1].TypeConfiguration.FullName);
            Assert.Equal("p2", parameters[2].Name);
            Assert.Equal(typeof(Address).FullName, parameters[2].TypeConfiguration.FullName);
        }

        [Fact]
        public void CanCreateActionWithNonbindingParameters_AddParameterNonGenericMethod()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = builder.Action("MyAction");
            action.Parameter(typeof(string), "p0");
            action.Parameter(typeof(int), "p1");
            action.Parameter(typeof(Address), "p2");
            ParameterConfiguration[] parameters = action.Parameters.ToArray();

            // Assert
            Assert.Equal(3, parameters.Length);
            Assert.Equal("p0", parameters[0].Name);
            Assert.Equal("Edm.String", parameters[0].TypeConfiguration.FullName);
            Assert.Equal("p1", parameters[1].Name);
            Assert.Equal("Edm.Int32", parameters[1].TypeConfiguration.FullName);
            Assert.Equal("p2", parameters[2].Name);
            Assert.Equal(typeof(Address).FullName, parameters[2].TypeConfiguration.FullName);
        }

        [Fact]
        public void CanCreateActionWithNonbindingParameters()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = builder.Action("MyAction");
            action.Parameter<string>("p0");
            action.Parameter<int>("p1");
            action.Parameter<Address>("p2");
            action.CollectionParameter<string>("p3");
            action.CollectionParameter<int>("p4");
            action.CollectionParameter<ZipCode>("p5");
            action.EntityParameter<Customer>("p6");
            action.CollectionEntityParameter<Employee>("p7");
            ParameterConfiguration[] parameters = action.Parameters.ToArray();
            ComplexTypeConfiguration[] complexTypes = builder.StructuralTypes.OfType<ComplexTypeConfiguration>().ToArray();
            EntityTypeConfiguration[] entityTypes = builder.StructuralTypes.OfType<EntityTypeConfiguration>().ToArray();

            // Assert
            Assert.Equal(2, complexTypes.Length);
            Assert.Equal(typeof(Address).FullName, complexTypes[0].FullName);
            Assert.Equal(typeof(ZipCode).FullName, complexTypes[1].FullName);

            Assert.Equal(2, entityTypes.Length);
            Assert.Equal(typeof(Customer).FullName, entityTypes[0].FullName);
            Assert.Equal(typeof(Employee).FullName, entityTypes[1].FullName);

            Assert.Equal(8, parameters.Length);
            Assert.Equal("p0", parameters[0].Name);
            Assert.Equal("Edm.String", parameters[0].TypeConfiguration.FullName);
            Assert.Equal("p1", parameters[1].Name);
            Assert.Equal("Edm.Int32", parameters[1].TypeConfiguration.FullName);
            Assert.Equal("p2", parameters[2].Name);
            Assert.Equal(typeof(Address).FullName, parameters[2].TypeConfiguration.FullName);
            Assert.Equal("p3", parameters[3].Name);
            Assert.Equal("Collection(Edm.String)", parameters[3].TypeConfiguration.FullName);
            Assert.Equal("p4", parameters[4].Name);
            Assert.Equal("Collection(Edm.Int32)", parameters[4].TypeConfiguration.FullName);
            Assert.Equal("p5", parameters[5].Name);
            Assert.Equal(string.Format("Collection({0})", typeof(ZipCode).FullName), parameters[5].TypeConfiguration.FullName);

            Assert.Equal("p6", parameters[6].Name);
            Assert.Equal(typeof(Customer).FullName, parameters[6].TypeConfiguration.FullName);

            Assert.Equal("p7", parameters[7].Name);
            Assert.Equal(string.Format("Collection({0})", typeof(Employee).FullName), parameters[7].TypeConfiguration.FullName);
        }

        [Fact]
        public void CanCreateActionWithReturnTypeAsNullableByDefault()
        {
            // Arrange & Act
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = builder.Action("MyAction").Returns<Address>();

            // Assert
            Assert.True(action.OptionalReturn);
        }

        [Fact]
        public void CanCreateActionWithReturnTypeAsNullableByOptionalReturn()
        {
            // Arrange & Act
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = builder.Action("MyAction").Returns<Address>();
            action.OptionalReturn = false;

            // Assert
            Assert.False(action.OptionalReturn);
        }

        [Fact]
        public void CanCreateActionWithNonbindingParametersAsNullable()
        {
            // Arrange & Act
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = builder.Action("MyAction");
            action.Parameter<string>("p0");
            action.Parameter<string>("p1").OptionalParameter = false;
            action.Parameter<int>("p2").OptionalParameter = true;
            action.Parameter<int>("p3");
            action.Parameter<Address>("p4");
            action.Parameter<Address>("p5").OptionalParameter = false;

            action.CollectionParameter<ZipCode>("p6");
            action.CollectionParameter<ZipCode>("p7").OptionalParameter = false;

            action.EntityParameter<Customer>("p8");
            action.EntityParameter<Customer>("p9").OptionalParameter = false;

            action.CollectionEntityParameter<Customer>("p10");
            action.CollectionEntityParameter<Customer>("p11").OptionalParameter = false;
            Dictionary<string, ParameterConfiguration> parameters = action.Parameters.ToDictionary(e => e.Name, e => e);

            // Assert
            Assert.True(parameters["p0"].OptionalParameter);
            Assert.False(parameters["p1"].OptionalParameter);

            Assert.True(parameters["p2"].OptionalParameter);
            Assert.False(parameters["p3"].OptionalParameter);

            Assert.True(parameters["p4"].OptionalParameter);
            Assert.False(parameters["p5"].OptionalParameter);

            Assert.True(parameters["p6"].OptionalParameter);
            Assert.False(parameters["p7"].OptionalParameter);

            Assert.True(parameters["p8"].OptionalParameter);
            Assert.False(parameters["p9"].OptionalParameter);

            Assert.True(parameters["p10"].OptionalParameter);
            Assert.False(parameters["p11"].OptionalParameter);
        }

        [Fact]
        public void CanCreateEdmModel_WithBindableAction()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            customer.HasKey(c => c.CustomerId);
            customer.Property(c => c.Name);

            // Act
            customer.Action("ActionName");
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmAction action = Assert.Single(model.SchemaElements.OfType<IEdmAction>());
            Assert.True(action.IsBound);
            Assert.Equal("ActionName", action.Name);
            Assert.Null(action.ReturnType);
            Assert.Equal(1, action.Parameters.Count());
            Assert.Equal(BindingParameterConfiguration.DefaultBindingParameterName, action.Parameters.Single().Name);
            Assert.Equal(typeof(Customer).FullName, action.Parameters.Single().Type.FullName());
        }

        [Fact]
        public void CanCreateEdmModel_WithNonBindableAction()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();

            // Act
            ActionConfiguration actionConfiguration = builder.Action("ActionName");
            actionConfiguration.ReturnsFromEntitySet<Customer>("Customers");

            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityContainer container = model.EntityContainer;
            Assert.NotNull(container);
            Assert.Equal(1, container.Elements.OfType<IEdmActionImport>().Count());
            Assert.Equal(1, container.Elements.OfType<IEdmEntitySet>().Count());
            IEdmActionImport action = container.Elements.OfType<IEdmActionImport>().Single();
            Assert.False(action.Action.IsBound);
            Assert.Equal("ActionName", action.Name);
            Assert.NotNull(action.Action.ReturnType);
            Assert.NotNull(action.EntitySet);
            Assert.Equal("Customers", (action.EntitySet as IEdmPathExpression).Path);
            Assert.Empty(action.Action.Parameters);
        }

        [Fact]
        public void GetEdmModel_ThrowsException_WhenUnboundActionOverloaded()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.Action("ActionName").Parameter<int>("Param1");
            builder.Action("ActionName").Returns<string>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => builder.GetEdmModel(),
                "Found more than one unbound action with name 'ActionName'. " +
                "Each unbound action must have an unique action name.");
        }

        [Fact]
        public void GetEdmModel_ThrowsException_WhenBoundActionOverloaded()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            customer.HasKey(c => c.CustomerId);
            customer.Property(c => c.Name);
            customer.Action("ActionOnCustomer");
            customer.Action("ActionOnCustomer").Returns<string>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => builder.GetEdmModel(),
                "Found more than one action with name 'ActionOnCustomer' " +
                "bound to the same type 'System.Web.OData.Builder.TestModels.Customer'. " +
                "Each bound action must have a different binding type or name.");
        }

        [Fact]
        public void HasActionLink_ThrowsException_OnNonBindableActions()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = builder.Action("NoBindableAction");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => action.HasActionLink(ctx => new Uri("http://any"), followsConventions: false),
                "To register an action link factory, actions must be bindable to a single entity. " +
                "Action 'NoBindableAction' does not meet this requirement.");
        }

        [Fact]
        public void HasActionLink_ThrowsException_OnNoBoundToEntityActions()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            ActionConfiguration action = customer.Collection.Action("CollectionAction");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => action.HasActionLink(ctx => new Uri("http://any"), followsConventions: false),
                "To register an action link factory, actions must be bindable to a single entity. " +
                "Action 'CollectionAction' does not meet this requirement.");
        }

        [Fact]
        public void HasFeedActionLink_ThrowsException_OnNonBindableActions()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = builder.Action("NoBindableAction");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => action.HasFeedActionLink(ctx => new Uri("http://any"), followsConventions: false),
                "To register an action link factory, actions must be bindable to the collection of entity. " +
                "Action 'NoBindableAction' does not meet this requirement.");
        }

        [Fact]
        public void HasFeedActionLink_ThrowsException_OnNoBoundToCollectionEntityActions()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            ActionConfiguration action = customer.Action("NonCollectionAction");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => action.HasFeedActionLink(ctx => new Uri("http://any"), followsConventions: false),
                "To register an action link factory, actions must be bindable to the collection of entity. " +
                "Action 'NonCollectionAction' does not meet this requirement.");
        }

        [Fact]
        public void CanManuallyConfigureActionLinkFactory()
        {
            // Arrange
            string uriTemplate = "http://server/service/Customers({0})/Reward";
            Uri expectedUri = new Uri(string.Format(uriTemplate, 1));
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntitySet<Customer>("Customers").EntityType;
            customer.HasKey(c => c.CustomerId);
            customer.Property(c => c.Name);

            // Act
            ActionConfiguration reward = customer.Action("Reward");
            reward.HasActionLink(ctx => new Uri(string.Format(uriTemplate, ctx.GetPropertyValue("CustomerId"))),
                followsConventions: false);
            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType customerType = model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault();
            ODataSerializerContext serializerContext = new ODataSerializerContext { Model = model };

            ResourceContext context = new ResourceContext(serializerContext, customerType.AsReference(), new Customer { CustomerId = 1 });
            IEdmAction rewardAction = Assert.Single(model.SchemaElements.OfType<IEdmAction>()); // Guard
            OperationLinkBuilder actionLinkBuilder = model.GetAnnotationValue<OperationLinkBuilder>(rewardAction);

            //Assert
            Assert.Equal(expectedUri, reward.GetActionLink()(context));
            Assert.NotNull(actionLinkBuilder);
            Assert.Equal(expectedUri, actionLinkBuilder.BuildLink(context));
        }

        [Fact]
        public void CanManuallyConfigureFeedActionLinkFactory()
        {
            // Arrange
            Uri expectedUri = new Uri("http://localhost/service/Customers/Reward");
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntitySet<Customer>("Customers").EntityType;
            customer.HasKey(c => c.CustomerId);
            customer.Property(c => c.Name);
            ActionConfiguration reward = customer.Collection.Action("Reward");
            reward.HasFeedActionLink(ctx => expectedUri, followsConventions: false);
            IEdmModel model = builder.GetEdmModel();

            // Act
            IEdmAction rewardAction = Assert.Single(model.SchemaElements.OfType<IEdmAction>()); // Guard
            OperationLinkBuilder actionLinkBuilder = model.GetAnnotationValue<OperationLinkBuilder>(rewardAction);
            ResourceSetContext context = new ResourceSetContext();

            //Assert
            Assert.Equal(expectedUri, reward.GetFeedActionLink()(context));
            Assert.NotNull(actionLinkBuilder);
            Assert.Equal(expectedUri, actionLinkBuilder.BuildLink(context));
        }

        [Fact]
        public void WhenActionLinksNotManuallyConfigured_ConventionBasedBuilderUsesConventions()
        {
            // Arrange
            string uriTemplate = "http://server/Movies({0})/Default.Watch";
            Uri expectedUri = new Uri(string.Format(uriTemplate, 1));
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntityTypeConfiguration<Movie> movie = builder.EntitySet<Movie>("Movies").EntityType;
            ActionConfiguration watch = movie.Action("Watch");
            IEdmModel model = builder.GetEdmModel();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://server/Movies");
            HttpConfiguration configuration = new HttpConfiguration();
            string routeName = "Route";
            configuration.MapODataServiceRoute(routeName, null, model);
            request.SetConfiguration(configuration);
            request.EnableODataDependencyInjectionSupport(routeName);
            UrlHelper urlHelper = new UrlHelper(request);

            // Act
            IEdmEntityType movieType = model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault();
            IEdmEntityContainer container = model.SchemaElements.OfType<IEdmEntityContainer>().SingleOrDefault();
            IEdmAction watchAction = Assert.Single(model.SchemaElements.OfType<IEdmAction>()); // Guard
            IEdmEntitySet entitySet = container.EntitySets().SingleOrDefault();
            ODataSerializerContext serializerContext = new ODataSerializerContext { Model = model, NavigationSource = entitySet, Url = urlHelper };

            ResourceContext context = new ResourceContext(serializerContext, movieType.AsReference(), new Movie { ID = 1, Name = "Avatar" });
            OperationLinkBuilder actionLinkBuilder = model.GetAnnotationValue<OperationLinkBuilder>(watchAction);

            //Assert
            Assert.Equal(expectedUri, watch.GetActionLink()(context));
            Assert.NotNull(actionLinkBuilder);
            Assert.Equal(expectedUri, actionLinkBuilder.BuildLink(context));
        }

        [Fact]
        public void WhenFeedActionLinksNotManuallyConfigured_ConventionBasedBuilderUsesConventions()
        {
            // Arrange
            Uri expectedUri = new Uri("http://server/Movies/Default.Watch");
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntityTypeConfiguration<Movie> movie = builder.EntitySet<Movie>("Movies").EntityType;
            ActionConfiguration watch = movie.Collection.Action("Watch"); // action bound to collection
            IEdmModel model = builder.GetEdmModel();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://server/Movies");
            HttpConfiguration configuration = new HttpConfiguration();
            string routeName = "Route";
            configuration.MapODataServiceRoute(routeName, null, model);
            request.SetConfiguration(configuration);
            request.EnableODataDependencyInjectionSupport(routeName);
            request.ODataProperties().Model = model;
            UrlHelper urlHelper = new UrlHelper(request);

            // Act
            IEdmEntityContainer container = model.SchemaElements.OfType<IEdmEntityContainer>().SingleOrDefault();
            IEdmAction watchAction = Assert.Single(model.SchemaElements.OfType<IEdmAction>()); // Guard
            IEdmEntitySet entitySet = container.EntitySets().SingleOrDefault();

            ResourceSetContext context = new ResourceSetContext
            {
                EntitySetBase = entitySet,
                Url = urlHelper,
                Request = request
            };

            OperationLinkBuilder actionLinkBuilder = model.GetAnnotationValue<OperationLinkBuilder>(watchAction);

            //Assert
            Assert.Equal(expectedUri, watch.GetFeedActionLink()(context));
            Assert.NotNull(actionLinkBuilder);
            Assert.Equal(expectedUri, actionLinkBuilder.BuildLink(context));
        }

        [Fact]
        public void GetEdmModel_SetsNullableIfParameterTypeIsNullable()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            EntityTypeConfiguration<Movie> movie = builder.EntitySet<Movie>("Movies").EntityType;
            var actionBuilder = movie.Action("Watch");
            actionBuilder.Parameter<int>("int");
            actionBuilder.Parameter<Nullable<int>>("nullableOfInt");
            actionBuilder.Parameter<string>("string");

            // Act
            IEdmModel model = builder.GetEdmModel();

            //Assert
            IEdmOperation action = Assert.Single(model.SchemaElements.OfType<IEdmAction>());
            Assert.False(action.FindParameter("int").Type.IsNullable);
            Assert.True(action.FindParameter("nullableOfInt").Type.IsNullable);
            Assert.True(action.FindParameter("string").Type.IsNullable);
        }

        [Fact]
        public void GetEdmModel_SetsNullableIfParameterTypeIsReferenceType()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            EntityTypeConfiguration<Movie> movie = builder.EntitySet<Movie>("Movies").EntityType;
            var actionBuilder = movie.Action("Watch");

            actionBuilder.Parameter<string>("string").OptionalParameter = false;
            actionBuilder.Parameter<string>("nullaleString");

            actionBuilder.Parameter<Address>("address").OptionalParameter = false;
            actionBuilder.Parameter<Address>("nullableAddress");

            actionBuilder.EntityParameter<Customer>("customer").OptionalParameter = false;
            actionBuilder.EntityParameter<Customer>("nullableCustomer");

            actionBuilder.CollectionParameter<Address>("addresses").OptionalParameter = false;
            actionBuilder.CollectionParameter<Address>("nullableAddresses");

            actionBuilder.CollectionEntityParameter<Customer>("customers").OptionalParameter = false;
            actionBuilder.CollectionEntityParameter<Customer>("nullableCustomers");

            // Act
            IEdmModel model = builder.GetEdmModel();

            //Assert
            IEdmOperation action = Assert.Single(model.SchemaElements.OfType<IEdmAction>());

            Assert.False(action.FindParameter("string").Type.IsNullable);
            Assert.True(action.FindParameter("nullaleString").Type.IsNullable);

            Assert.False(action.FindParameter("address").Type.IsNullable);
            Assert.True(action.FindParameter("nullableAddress").Type.IsNullable);

            Assert.False(action.FindParameter("customer").Type.IsNullable);
            Assert.True(action.FindParameter("nullableCustomer").Type.IsNullable);

            Assert.False(action.FindParameter("addresses").Type.IsNullable);
            Assert.True(action.FindParameter("nullableAddresses").Type.IsNullable);

            // Follow up: https://github.com/OData/odata.net/issues/98
            // Assert.False(action.FindParameter("customers").Type.IsNullable);
            Assert.True(action.FindParameter("nullableCustomers").Type.IsNullable);
        }

        [Fact]
        public void GetEdmModel_SetReturnTypeAsNullable()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            EntityTypeConfiguration<Movie> movie = builder.EntitySet<Movie>("Movies").EntityType;
            movie.Action("Watch1").Returns<Address>();
            movie.Action("Watch2").Returns<Address>().OptionalReturn = false;

            // Act
            IEdmModel model = builder.GetEdmModel();

            //Assert
            IEdmOperation action = model.SchemaElements.OfType<IEdmAction>().First(e => e.Name == "Watch1");
            Assert.True(action.ReturnType.IsNullable);

            action = model.SchemaElements.OfType<IEdmAction>().First(e => e.Name == "Watch2");
            Assert.False(action.ReturnType.IsNullable);
        }

        [Fact]
        public void GetEdmModel_SetsDateTimeAsParameterType()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            EntityTypeConfiguration<Movie> movie = builder.EntitySet<Movie>("Movies").EntityType;
            var actionBuilder = movie.Action("DateTimeAction");
            actionBuilder.Parameter<DateTime>("dateTime");
            actionBuilder.Parameter<DateTime?>("nullableDateTime");
            actionBuilder.CollectionParameter<DateTime>("collectionDateTime");
            actionBuilder.CollectionParameter<DateTime?>("nullableCollectionDateTime");

            // Act
            IEdmModel model = builder.GetEdmModel();

            //Assert
            IEdmOperation action = Assert.Single(model.SchemaElements.OfType<IEdmAction>());
            Assert.Equal("DateTimeAction", action.Name);

            IEdmOperationParameter parameter = action.FindParameter("dateTime");
            Assert.Equal("Edm.DateTimeOffset", parameter.Type.FullName());
            Assert.False(parameter.Type.IsNullable);

            parameter = action.FindParameter("nullableDateTime");
            Assert.Equal("Edm.DateTimeOffset", parameter.Type.FullName());
            Assert.True(parameter.Type.IsNullable);

            parameter = action.FindParameter("collectionDateTime");
            Assert.Equal("Collection(Edm.DateTimeOffset)", parameter.Type.FullName());
            Assert.False(parameter.Type.IsNullable);

            parameter = action.FindParameter("nullableCollectionDateTime");
            Assert.Equal("Collection(Edm.DateTimeOffset)", parameter.Type.FullName());
            Assert.True(parameter.Type.IsNullable);
        }

        [Theory]
        [InlineData(typeof(Date), "Edm.Date")]
        [InlineData(typeof(Date?), "Edm.Date")]
        [InlineData(typeof(TimeOfDay), "Edm.TimeOfDay")]
        [InlineData(typeof(TimeOfDay?), "Edm.TimeOfDay")]
        public void CanCreateEdmModel_WithDateAndTimeOfDay_AsActionParameter(Type paramType, string expect)
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            EntityTypeConfiguration<Movie> movie = builder.EntitySet<Movie>("Movies").EntityType;
            var actionBuilder = movie.Action("ActionName");
            actionBuilder.Parameter(paramType, "p1");

            MethodInfo method = typeof(OperationConfiguration).GetMethod("CollectionParameter", BindingFlags.Instance | BindingFlags.Public);
            method.MakeGenericMethod(paramType).Invoke(actionBuilder, new[] { "p2" });

            // Act
            IEdmModel model = builder.GetEdmModel();

            //Assert
            IEdmOperation action = Assert.Single(model.SchemaElements.OfType<IEdmAction>());
            Assert.Equal("ActionName", action.Name);

            IEdmOperationParameter parameter = action.FindParameter("p1");
            Assert.Equal(expect, parameter.Type.FullName());
            Assert.Equal(paramType.IsNullable(), parameter.Type.IsNullable);

            parameter = action.FindParameter("p2");
            Assert.Equal("Collection(" + expect + ")", parameter.Type.FullName());
            Assert.Equal(paramType.IsNullable(), parameter.Type.IsNullable);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HasActionLink_SetsFollowsConventions(bool value)
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = new ActionConfiguration(builder, "IgnoreAction");
            Mock<IEdmTypeConfiguration> bindingParameterTypeMock = new Mock<IEdmTypeConfiguration>();
            bindingParameterTypeMock.Setup(o => o.Kind).Returns(EdmTypeKind.Entity);
            bindingParameterTypeMock.Setup(o => o.ClrType).Returns(typeof(int));
            IEdmTypeConfiguration bindingParameterType = bindingParameterTypeMock.Object;
            action.SetBindingParameter("IgnoreParameter", bindingParameterType);

            // Act
            action.HasActionLink((a) => { throw new NotImplementedException(); }, followsConventions: value);

            // Assert
            Assert.Equal(value, action.FollowsConventions);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HasFeedActionLink_SetsFollowsConventions(bool value)
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            var action = builder.EntityType<Customer>().Collection.Action("IgnoreAction");

            // Act
            action.HasFeedActionLink((a) => { throw new NotImplementedException(); }, followsConventions: value);

            // Assert
            Assert.Equal(value, action.FollowsConventions);
        }

        [Fact]
        public void ReturnsFromEntitySet_Sets_NavigationSourceAndReturnType()
        {
            // Arrange
            string entitySetName = "movies";
            ODataModelBuilder builder = new ODataModelBuilder();
            var movies = builder.EntitySet<Movie>(entitySetName);
            var action = builder.Action("Action");

            // Act
            action.ReturnsFromEntitySet(movies);

            // Assert
            Assert.Equal(entitySetName, action.NavigationSource.Name);
            Assert.Equal(typeof(Movie), action.ReturnType.ClrType);
        }

        [Fact]
        public void ReturnsFromEntitySet_ThrowsArgumentNull_EntitySetConfiguration()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            var action = builder.Action("action");

            // Act & Assert
            Assert.ThrowsArgumentNull(() => action.ReturnsFromEntitySet<Movie>(entitySetConfiguration: null),
                "entitySetConfiguration");
        }

        [Fact]
        public void ReturnsCollectionFromEntitySet_Sets_EntitySetAndReturnType()
        {
            // Arrange
            string entitySetName = "movies";
            ODataModelBuilder builder = new ODataModelBuilder();
            var movies = builder.EntitySet<Movie>(entitySetName);
            var action = builder.Action("Action");

            // Act
            action.ReturnsCollectionFromEntitySet(movies);

            // Assert
            Assert.Equal(entitySetName, action.NavigationSource.Name);
            Assert.Equal(typeof(IEnumerable<Movie>), action.ReturnType.ClrType);
        }

        [Fact]
        public void ReturnsCollectionFromEntitySet_ThrowsArgumentNull_EntitySetConfiguration()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            var action = builder.Action("action");

            // Act & Assert
            Assert.ThrowsArgumentNull(() => action.ReturnsCollectionFromEntitySet<Movie>(entitySetConfiguration: null),
                "entitySetConfiguration");
        }

        [Fact]
        public void Returns_ThrowsInvalidOperationException_IfReturnTypeIsEntity()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<Movie>();
            var action = builder.Action("action");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => action.Returns<Movie>(),
                "The EDM type 'System.Web.OData.Builder.Movie' is already declared as an entity type. Use the " +
                "method 'ReturnsFromEntitySet' if the return type is an entity.");
        }

        [Fact]
        public void ReturnsCollection_ThrowsInvalidOperationException_IfReturnTypeIsEntity()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<Movie>();
            var action = builder.Action("action");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => action.ReturnsCollection<Movie>(),
                "The EDM type 'System.Web.OData.Builder.Movie' is already declared as an entity type. Use the " +
                "method 'ReturnsCollectionFromEntitySet' if the return type is an entity collection.");
        }

        [Fact]
        public void Cant_SetActionTile_OnNonBindableActions()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = builder.Action("MyAction");
            action.Returns<int>();
            action.Title = "My Action";

            // Act
            IEdmModel model = builder.GetEdmModel();
            IEdmOperationImport actionImport = model.EntityContainer.OperationImports().OfType<IEdmActionImport>().Single();
            Assert.NotNull(actionImport);
            OperationTitleAnnotation actionTitleAnnotation = model.GetOperationTitleAnnotation(actionImport.Operation);

            // Assert
            Assert.Null(actionTitleAnnotation);
        }

        [Fact]
        public void Can_SetActionTitle_OnBindable_Actions()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntitySetConfiguration<Movie> movies = builder.EntitySet<Movie>("Movies");
            movies.EntityType.HasKey(m => m.ID);
            ActionConfiguration entityAction = movies.EntityType.Action("Checkout");
            entityAction.Returns<int>();
            entityAction.Title = "Check out";
            ActionConfiguration collectionAction = movies.EntityType.Collection.Action("RemoveOld");
            collectionAction.Returns<int>();
            collectionAction.Title = "Remove Old Movies";

            // Act
            IEdmModel model = builder.GetEdmModel();
            IEdmOperation checkout = model.FindOperations("Default.Checkout").Single();
            IEdmOperation removeOld = model.FindOperations("Default.RemoveOld").Single();
            OperationTitleAnnotation checkoutTitle = model.GetOperationTitleAnnotation(checkout);
            OperationTitleAnnotation removeOldTitle = model.GetOperationTitleAnnotation(removeOld);

            // Assert
            Assert.NotNull(checkoutTitle);
            Assert.Equal("Check out", checkoutTitle.Title);
            Assert.NotNull(removeOldTitle);
            Assert.Equal("Remove Old Movies", removeOldTitle.Title);
        }

        public class Movie
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }
    }
}
