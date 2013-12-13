// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.Builder.TestModels;
using System.Web.Http.OData.Formatter.Serialization;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Expressions;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder
{
    public class FunctionConfigurationTest
    {
        [Fact]
        public void CanCreateFunctionWithNoArguments()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.Namespace = "MyNamespace";
            builder.ContainerName = "MyContainer";
            FunctionConfiguration function = builder.Function("Format");

            // Assert
            Assert.Equal("Format", function.Name);
            Assert.Equal(ProcedureKind.Function, function.Kind);
            Assert.NotNull(function.Parameters);
            Assert.Empty(function.Parameters);
            Assert.Null(function.ReturnType);
            Assert.False(function.IsSideEffecting);
            Assert.False(function.IsComposable);
            Assert.False(function.IsBindable);
            Assert.False(function.SupportedInFilter);
            Assert.False(function.SupportedInOrderBy);
            Assert.Equal("MyContainer.Format", function.ContainerQualifiedName);
            Assert.Equal("MyContainer.Format", function.FullName);
            Assert.Equal("MyNamespace.MyContainer.Format", function.FullyQualifiedName);
            Assert.NotNull(builder.Procedures);
            Assert.Equal(1, builder.Procedures.Count());
        }

        [Fact]
        public void AttemptToRemoveNonExistentEntityReturnsFalse()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            ODataModelBuilder builder2 = new ODataModelBuilder();
            ProcedureConfiguration toRemove = builder2.Function("ToRemove");

            // Act
            bool removedByName = builder.RemoveProcedure("ToRemove");
            bool removed = builder.RemoveProcedure(toRemove);

            //Assert
            Assert.False(removedByName);
            Assert.False(removed);
        }

        [Fact]
        public void CanCreateFunctionWithPrimitiveReturnType()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            FunctionConfiguration function = builder.Function("CreateMessage");
            function.Returns<string>();

            // Assert
            Assert.NotNull(function.ReturnType);
            Assert.Equal("Edm.String", function.ReturnType.FullName);
        }

        [Fact]
        public void CanCreateFunctionWithPrimitiveCollectionReturnType()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            FunctionConfiguration function = builder.Function("CreateMessages");
            function.ReturnsCollection<string>();

            // Assert
            Assert.NotNull(function.ReturnType);
            Assert.Equal("Collection(Edm.String)", function.ReturnType.FullName);
        }

        [Fact]
        public void CanCreateFunctionWithComplexReturnType()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();

            FunctionConfiguration createAddress = builder.Function("CreateAddress").Returns<Address>();
            FunctionConfiguration createAddresses = builder.Function("CreateAddresses").ReturnsCollection<Address>();

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
        public void CanCreateFunctionWithEntityReturnType()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();

            FunctionConfiguration createGoodCustomer = builder.Function("CreateGoodCustomer").ReturnsFromEntitySet<Customer>("GoodCustomers");
            FunctionConfiguration createBadCustomers = builder.Function("CreateBadCustomers").ReturnsCollectionFromEntitySet<Customer>("BadCustomers");

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
        public void CanCreateFunctionWithEntityReturnTypeViaEntitySetPath()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.Entity<Customer>();
            EntityTypeConfiguration<Order> order = builder.Entity<Order>();

            order.HasRequired<Customer>(o => o.Customer);
            FunctionConfiguration getOrderCustomer = order.Function("GetOrderCustomer").ReturnsEntityViaEntitySetPath<Customer>("bindingParameter/Customer");
            IEdmModel model = builder.GetEdmModel();
            IEdmEntityContainer container = model.SchemaElements.OfType<IEdmEntityContainer>().SingleOrDefault();
            IEdmFunctionImport function = container.FindOperationImports("GetOrderCustomer").Single() as IEdmFunctionImport;

            // Assert
            Assert.NotNull(function);
            Assert.Equal(function.EntitySet.ExpressionKind, EdmExpressionKind.Path);
            Assert.Equal(string.Join("/", ((IEdmPathExpression)(function.EntitySet)).Path), "bindingParameter/Customer");
        }

        [Fact]
        public void CanCreateFunctionWithCollectionReturnTypeViaEntitySetPath()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.Entity<Customer>();
            EntityTypeConfiguration<Order> order = builder.Entity<Order>();

            customer.HasMany<Order>(c => c.Orders);
            FunctionConfiguration getOrders = customer.Function("GetOrders").ReturnsCollectionViaEntitySetPath<Order>("bindingParameter/Orders");
            IEdmModel model = builder.GetEdmModel();
            IEdmEntityContainer container = model.SchemaElements.OfType<IEdmEntityContainer>().SingleOrDefault();
            IEdmFunctionImport function = container.FindOperationImports("GetOrders").Single() as IEdmFunctionImport;

            // Assert
            Assert.NotNull(function);
            Assert.Equal(function.EntitySet.ExpressionKind, EdmExpressionKind.Path);
            Assert.Equal(string.Join("/", ((IEdmPathExpression)(function.EntitySet)).Path), "bindingParameter/Orders");
        }

        [Fact]
        public void CanCreateFunctionThatBindsToEntity()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.Entity<Customer>();
            FunctionConfiguration sendEmail = customer.Function("SendEmail");

            // Assert
            Assert.True(sendEmail.IsBindable);
            Assert.True(sendEmail.IsAlwaysBindable);
            Assert.NotNull(sendEmail.Parameters);
            Assert.Equal(1, sendEmail.Parameters.Count());
            Assert.Equal(BindingParameterConfiguration.DefaultBindingParameterName, sendEmail.Parameters.Single().Name);
            Assert.Equal(typeof(Customer).FullName, sendEmail.Parameters.Single().TypeConfiguration.FullName);
        }

        [Fact]
        public void CanCreateFunctionThatBindsToEntityCollection()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.Entity<Customer>();
            FunctionConfiguration sendEmail = customer.Collection.Function("SendEmail");

            // Assert
            Assert.True(sendEmail.IsBindable);
            Assert.True(sendEmail.IsAlwaysBindable);
            Assert.NotNull(sendEmail.Parameters);
            Assert.Equal(1, sendEmail.Parameters.Count());
            Assert.Equal(BindingParameterConfiguration.DefaultBindingParameterName, sendEmail.Parameters.Single().Name);
            Assert.Equal(string.Format("Collection({0})", typeof(Customer).FullName), sendEmail.Parameters.Single().TypeConfiguration.FullName);
        }

        [Fact]
        public void CanCreateTransientFunction()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.Entity<Customer>();
            customer.TransientFunction("Reward");

            ProcedureConfiguration function = builder.Procedures.SingleOrDefault();
            Assert.NotNull(function);
            Assert.True(function.IsBindable);
            Assert.False(function.IsAlwaysBindable);
        }

        [Fact]
        public void CanCreateFunctionWithNonbindingParameters()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            FunctionConfiguration function = builder.Function("MyFunction");
            function.Parameter<string>("p0");
            function.Parameter<int>("p1");
            function.Parameter<Address>("p2");
            function.CollectionParameter<string>("p3");
            function.CollectionParameter<int>("p4");
            function.CollectionParameter<ZipCode>("p5");
            ParameterConfiguration[] parameters = function.Parameters.ToArray();
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
        public void CanCreateEdmModel_WithBindableFunction()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.Entity<Customer>();
            customer.HasKey(c => c.CustomerId);
            customer.Property(c => c.Name);
            // Act
            FunctionConfiguration sendEmail = customer.Function("FunctionName");
            sendEmail.Returns<bool>();
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityContainer container = model.EntityContainers().SingleOrDefault();
            Assert.NotNull(container);
            Assert.Equal(1, container.Elements.OfType<IEdmFunctionImport>().Count());
            IEdmFunction function = container.Elements.OfType<IEdmFunctionImport>().Single().Function;
            Assert.False(function.IsComposable);
            Assert.True(function.IsBound);
            Assert.True(model.IsAlwaysBindable(function));
            Assert.Equal("FunctionName", function.Name);
            Assert.NotNull(function.ReturnType);
            Assert.Equal(1, function.Parameters.Count());
            Assert.Equal(BindingParameterConfiguration.DefaultBindingParameterName, function.Parameters.Single().Name);
            Assert.Equal(typeof(Customer).FullName, function.Parameters.Single().Type.FullName());
        }

        [Fact]
        public void CanCreateEdmModel_WithNonBindableFunction()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            FunctionConfiguration functionConfiguration = builder.Function("FunctionName");
            functionConfiguration.ReturnsFromEntitySet<Customer>("Customers");

            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityContainer container = model.EntityContainers().SingleOrDefault();
            Assert.NotNull(container);
            Assert.Equal(1, container.Elements.OfType<IEdmFunctionImport>().Count());
            Assert.Equal(1, container.Elements.OfType<IEdmEntitySet>().Count());
            IEdmFunctionImport functionImport = container.Elements.OfType<IEdmFunctionImport>().Single();
            IEdmFunction function = functionImport.Function;
            Assert.False(function.IsComposable);
            Assert.False(function.IsBound);
            Assert.False(model.IsAlwaysBindable(function));
            Assert.Equal("FunctionName", function.Name);
            Assert.NotNull(function.ReturnType);
            Assert.NotNull(functionImport.EntitySet);
            Assert.Equal("Customers", (functionImport.EntitySet as IEdmEntitySetReferenceExpression).ReferencedEntitySet.Name);
            Assert.Equal(typeof(Customer).FullName, (functionImport.EntitySet as IEdmEntitySetReferenceExpression).ReferencedEntitySet.ElementType.FullName());
            Assert.Empty(function.Parameters);
        }

        [Fact]
        public void CanCreateEdmModel_WithTransientBindableFunction()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.Entity<Customer>();
            customer.HasKey(c => c.CustomerId);
            customer.Property(c => c.Name);
            // Act
            FunctionConfiguration sendEmail = customer.TransientFunction("FunctionName");
            sendEmail.Returns<bool>();
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityContainer container = model.EntityContainers().SingleOrDefault();
            Assert.NotNull(container);
            Assert.Equal(1, container.Elements.OfType<IEdmFunctionImport>().Count());
            IEdmFunction function = container.Elements.OfType<IEdmFunctionImport>().Single().Function;
            Assert.True(function.IsBound);
            Assert.False(model.IsAlwaysBindable(function));
        }

        [Fact]
        public void CanManuallyConfigureFunctionLinkFactory()
        {
            // Arrange
            string uriTemplate = "http://server/service/Customers({0})/Reward";
            Uri expectedUri = new Uri(string.Format(uriTemplate, 1));
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntitySet<Customer>("Customers").EntityType;
            customer.HasKey(c => c.CustomerId);
            customer.Property(c => c.Name);

            // Act
            FunctionConfiguration reward = customer.Function("Reward");
            reward.HasFunctionLink(ctx => new Uri(string.Format(uriTemplate, ctx.GetPropertyValue("CustomerId"))),
                followsConventions: false);
            reward.Returns<bool>();
            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType customerType = model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault();
            ODataSerializerContext serializerContext = new ODataSerializerContext { Model = model };

            EntityInstanceContext context = new EntityInstanceContext(serializerContext, customerType.AsReference(), new Customer { CustomerId = 1 });
            IEdmFunction rewardFunction = model.SchemaElements.OfType<IEdmEntityContainer>()
                .SingleOrDefault()
                .OperationImports()
                .SingleOrDefault().Operation as IEdmFunction;
            Assert.NotNull(rewardFunction);
            FunctionLinkBuilder functionLinkBuilder = model.GetAnnotationValue<FunctionLinkBuilder>(rewardFunction);

            //Assert
            Assert.Equal(expectedUri, reward.GetFunctionLink()(context));
            Assert.NotNull(functionLinkBuilder);
            Assert.Equal(expectedUri, functionLinkBuilder.BuildFunctionLink(context));
        }

        [Fact]
        public void GetEdmModel_SetsNullableIffParameterTypeIsNullable()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Movie> movie = builder.EntitySet<Movie>("Movies").EntityType;
            var functionBuilder = movie.Function("Watch");
            functionBuilder.Parameter<int>("int");
            functionBuilder.Parameter<Nullable<int>>("nullableOfInt");
            functionBuilder.Parameter<DateTime>("dateTime");
            functionBuilder.Parameter<string>("string");
            functionBuilder.Returns<int>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            //Assert
            IEdmEntityContainer container = model.SchemaElements.OfType<IEdmEntityContainer>().SingleOrDefault();
            IEdmOperation function = container.FindOperationImports("Watch").Single().Operation as IEdmFunction;
            Assert.NotNull(function);
            Assert.False(function.FindParameter("int").Type.IsNullable);
            Assert.True(function.FindParameter("nullableOfInt").Type.IsNullable);
            Assert.False(function.FindParameter("dateTime").Type.IsNullable);
            Assert.True(function.FindParameter("string").Type.IsNullable);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HasFunctionLink_SetsFollowsConventions(bool value)
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            FunctionConfiguration function = new FunctionConfiguration(builder, "IgnoreFunction");
            Mock<IEdmTypeConfiguration> bindingParameterTypeMock = new Mock<IEdmTypeConfiguration>();
            bindingParameterTypeMock.Setup(o => o.Kind).Returns(EdmTypeKind.Entity);
            IEdmTypeConfiguration bindingParameterType = bindingParameterTypeMock.Object;
            function.SetBindingParameter("IgnoreParameter", bindingParameterType, alwaysBindable: false);

            // Act
            function.HasFunctionLink((a) => { throw new NotImplementedException(); }, followsConventions: value);

            // Assert
            Assert.Equal(value, function.FollowsConventions);
        }

        public class Movie
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }
    }
}
