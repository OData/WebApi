// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.OData.Builder.TestModels;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Formatter.Serialization;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder
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

            // Assert
            Assert.Equal("Format", action.Name);
            Assert.Equal(ProcedureKind.Action, action.Kind);
            Assert.NotNull(action.Parameters);
            Assert.Empty(action.Parameters);
            Assert.Null(action.ReturnType);
            Assert.True(action.IsSideEffecting);
            Assert.False(action.IsComposable);
            Assert.False(action.IsBindable);
            Assert.Equal("MyContainer.Format", action.ContainerQualifiedName);
            Assert.Equal("MyContainer.Format", action.FullName);
            Assert.Equal("MyNamespace.MyContainer.Format", action.FullyQualifiedName);
            Assert.NotNull(builder.Procedures);
            Assert.Equal(1, builder.Procedures.Count());
        }

        [Fact]
        public void AttemptToRemoveNonExistantEntityReturnsFalse()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            ODataModelBuilder builder2 = new ODataModelBuilder();
            ProcedureConfiguration toRemove = builder2.Action("ToRemove");

            // Act
            bool removedByName = builder.RemoveProcedure("ToRemove");
            bool removed = builder.RemoveProcedure(toRemove);

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
            Assert.Null(createAddress.EntitySet);

            CollectionTypeConfiguration addresses = createAddresses.ReturnType as CollectionTypeConfiguration;
            Assert.NotNull(addresses);
            Assert.Equal(string.Format("Collection({0})", typeof(Address).FullName), addresses.FullName);
            address = addresses.ElementType as ComplexTypeConfiguration;
            Assert.NotNull(address);
            Assert.Equal(typeof(Address).FullName, address.FullName);
            Assert.Null(createAddresses.EntitySet);
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
            Assert.Same(createGoodCustomer.EntitySet, goodCustomers);

            CollectionTypeConfiguration customers = createBadCustomers.ReturnType as CollectionTypeConfiguration;
            Assert.NotNull(customers);
            customer = customers.ElementType as EntityTypeConfiguration;
            Assert.NotNull(customer);
            EntitySetConfiguration badCustomers = builder.EntitySets.SingleOrDefault(s => s.Name == "BadCustomers");
            Assert.NotNull(badCustomers);
            Assert.Same(createBadCustomers.EntitySet, badCustomers);
        }

        [Fact]
        public void CanCreateActionThatBindsToEntity()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.Entity<Customer>();
            ActionConfiguration sendEmail = customer.Action("SendEmail");

            // Assert
            Assert.True(sendEmail.IsBindable);
            Assert.True(sendEmail.IsAlwaysBindable);
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
            EntityTypeConfiguration<Customer> customer = builder.Entity<Customer>();
            ActionConfiguration sendEmail = customer.Collection.Action("SendEmail");

            // Assert
            Assert.True(sendEmail.IsBindable);
            Assert.True(sendEmail.IsAlwaysBindable);
            Assert.NotNull(sendEmail.Parameters);
            Assert.Equal(1, sendEmail.Parameters.Count());
            Assert.Equal(BindingParameterConfiguration.DefaultBindingParameterName, sendEmail.Parameters.Single().Name);
            Assert.Equal(string.Format("Collection({0})", typeof(Customer).FullName), sendEmail.Parameters.Single().TypeConfiguration.FullName);
        }

        [Fact]
        public void CanCreateTransientAction()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.Entity<Customer>();
            customer.TransientAction("Reward");

            ProcedureConfiguration action = builder.Procedures.SingleOrDefault();
            Assert.NotNull(action);
            Assert.True(action.IsBindable);
            Assert.False(action.IsAlwaysBindable);
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
            ParameterConfiguration[] parameters = action.Parameters.ToArray();
            ComplexTypeConfiguration[] complexTypes = builder.StructuralTypes.OfType<ComplexTypeConfiguration>().ToArray();

            // Assert
            Assert.Equal(2, complexTypes.Length);
            Assert.Equal(typeof(Address).FullName, complexTypes[0].FullName);
            Assert.Equal(typeof(ZipCode).FullName, complexTypes[1].FullName);
            Assert.Equal(6, parameters.Length);
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
        }

        [Fact]
        public void CanCreateEdmModel_WithBindableAction()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.Entity<Customer>();
            customer.HasKey(c => c.CustomerId);
            customer.Property(c => c.Name);
            // Act
            ActionConfiguration sendEmail = customer.Action("ActionName");
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityContainer container = model.EntityContainers().SingleOrDefault();
            Assert.NotNull(container);
            Assert.Equal(1, container.Elements.OfType<IEdmFunctionImport>().Count());
            IEdmFunctionImport action = container.Elements.OfType<IEdmFunctionImport>().Single();
            Assert.False(action.IsComposable);
            Assert.True(action.IsSideEffecting);
            Assert.True(action.IsBindable);
            Assert.True(model.IsAlwaysBindable(action));
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
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            ActionConfiguration actionConfiguration = builder.Action("ActionName");
            actionConfiguration.ReturnsFromEntitySet<Customer>("Customers");

            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityContainer container = model.EntityContainers().SingleOrDefault();
            Assert.NotNull(container);
            Assert.Equal(1, container.Elements.OfType<IEdmFunctionImport>().Count());
            Assert.Equal(1, container.Elements.OfType<IEdmEntitySet>().Count());
            IEdmFunctionImport action = container.Elements.OfType<IEdmFunctionImport>().Single();
            Assert.False(action.IsComposable);
            Assert.True(action.IsSideEffecting);
            Assert.False(action.IsBindable);
            Assert.False(model.IsAlwaysBindable(action));
            Assert.Equal("ActionName", action.Name);
            Assert.NotNull(action.ReturnType);
            Assert.NotNull(action.EntitySet);
            Assert.Equal("Customers", (action.EntitySet as IEdmEntitySetReferenceExpression).ReferencedEntitySet.Name);
            Assert.Equal(typeof(Customer).FullName, (action.EntitySet as IEdmEntitySetReferenceExpression).ReferencedEntitySet.ElementType.FullName());
            Assert.Empty(action.Parameters);
        }

        [Fact]
        public void CanCreateEdmModel_WithTransientBindableAction()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.Entity<Customer>();
            customer.HasKey(c => c.CustomerId);
            customer.Property(c => c.Name);
            // Act
            ActionConfiguration sendEmail = customer.TransientAction("ActionName");
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityContainer container = model.EntityContainers().SingleOrDefault();
            Assert.NotNull(container);
            Assert.Equal(1, container.Elements.OfType<IEdmFunctionImport>().Count());
            IEdmFunctionImport action = container.Elements.OfType<IEdmFunctionImport>().Single();
            Assert.True(action.IsBindable);
            Assert.False(model.IsAlwaysBindable(action));
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

            EntityInstanceContext context = new EntityInstanceContext(serializerContext, customerType.AsReference(), new Customer { CustomerId = 1 });
            IEdmFunctionImport rewardAction = model.SchemaElements.OfType<IEdmEntityContainer>().SingleOrDefault().FunctionImports().SingleOrDefault();
            ActionLinkBuilder actionLinkBuilder = model.GetAnnotationValue<ActionLinkBuilder>(rewardAction);

            //Assert
            Assert.Equal(expectedUri, reward.GetActionLink()(context));
            Assert.NotNull(actionLinkBuilder);
            Assert.Equal(expectedUri, actionLinkBuilder.BuildActionLink(context));
        }

        [Fact]
        public void WhenActionLinksNotManuallyConfigured_ConventionBasedBuilderUsesConventions()
        {
            // Arrange
            string uriTemplate = "http://server/Movies({0})/Watch";
            Uri expectedUri = new Uri(string.Format(uriTemplate, 1));
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntityTypeConfiguration<Movie> movie = builder.EntitySet<Movie>("Movies").EntityType;
            ActionConfiguration watch = movie.Action("Watch");
            IEdmModel model = builder.GetEdmModel();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://server/Movies");
            HttpConfiguration configuration = new HttpConfiguration();
            string routeName = "Route";
            configuration.Routes.MapODataServiceRoute(routeName, null, model);
            request.SetConfiguration(configuration);
            request.ODataProperties().RouteName = routeName;
            UrlHelper urlHelper = new UrlHelper(request);

            // Act
            IEdmEntityType movieType = model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault();
            IEdmEntityContainer container = model.SchemaElements.OfType<IEdmEntityContainer>().SingleOrDefault();
            IEdmFunctionImport watchAction = container.FunctionImports().SingleOrDefault();
            IEdmEntitySet entitySet = container.EntitySets().SingleOrDefault();
            ODataSerializerContext serializerContext = new ODataSerializerContext { Model = model, EntitySet = entitySet, Url = urlHelper };

            EntityInstanceContext context = new EntityInstanceContext(serializerContext, movieType.AsReference(), new Movie { ID = 1, Name = "Avatar" });
            ActionLinkBuilder actionLinkBuilder = model.GetAnnotationValue<ActionLinkBuilder>(watchAction);

            //Assert
            Assert.Equal(expectedUri, watch.GetActionLink()(context));
            Assert.NotNull(actionLinkBuilder);
            Assert.Equal(expectedUri, actionLinkBuilder.BuildActionLink(context));
        }

        [Fact]
        public void GetEdmModel_SetsNullableIffParameterTypeIsNullable()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Movie> movie = builder.EntitySet<Movie>("Movies").EntityType;
            var actionBuilder = movie.Action("Watch");
            actionBuilder.Parameter<int>("int");
            actionBuilder.Parameter<Nullable<int>>("nullableOfInt");
            actionBuilder.Parameter<DateTime>("dateTime");
            actionBuilder.Parameter<string>("string");

            // Act
            IEdmModel model = builder.GetEdmModel();

            //Assert
            IEdmEntityContainer container = model.SchemaElements.OfType<IEdmEntityContainer>().SingleOrDefault();
            IEdmFunctionImport action = container.FindFunctionImports("Watch").Single();
            Assert.False(action.FindParameter("int").Type.IsNullable);
            Assert.True(action.FindParameter("nullableOfInt").Type.IsNullable);
            Assert.False(action.FindParameter("dateTime").Type.IsNullable);
            Assert.True(action.FindParameter("string").Type.IsNullable);
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
            IEdmTypeConfiguration bindingParameterType = bindingParameterTypeMock.Object;
            action.SetBindingParameter("IgnoreParameter", bindingParameterType, alwaysBindable: false);

            // Act
            action.HasActionLink((a) => { throw new NotImplementedException(); }, followsConventions: value);

            // Assert
            Assert.Equal(value, action.FollowsConventions);
        }

        [Fact]
        public void ReturnsFromEntitySet_Sets_EntitySetAndReturnType()
        {
            // Arrange
            string entitySetName = "movies";
            ODataModelBuilder builder = new ODataModelBuilder();
            var movies = builder.EntitySet<Movie>(entitySetName);
            var action = builder.Action("Action");

            // Act
            action.ReturnsFromEntitySet(movies);

            // Assert
            Assert.Equal(entitySetName, action.EntitySet.Name);
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
            Assert.Equal(entitySetName, action.EntitySet.Name);
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
            builder.Entity<Movie>();
            var action = builder.Action("action");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => action.Returns<Movie>(),
                "The EDM type 'System.Web.Http.OData.Builder.Movie' is already declared as an entity type. Use the " +
                "method 'ReturnsFromEntitySet' if the return type is an entity.");
        }

        [Fact]
        public void ReturnsCollection_ThrowsInvalidOperationException_IfReturnTypeIsEntity()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.Entity<Movie>();
            var action = builder.Action("action");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => action.ReturnsCollection<Movie>(),
                "The EDM type 'System.Web.Http.OData.Builder.Movie' is already declared as an entity type. Use the " +
                "method 'ReturnsCollectionFromEntitySet' if the return type is an entity collection.");
        }

        public class Movie
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }
    }
}
