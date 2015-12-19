// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Expressions;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder
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
            ActionConfiguration functionII = builder.Action("FormatII");
            functionII.Namespace = "MyNamespaceII";

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
            Assert.Equal("MyNamespace", function.Namespace);
            Assert.Equal("MyNamespace.Format", function.FullyQualifiedName);
            Assert.Equal("MyNamespaceII", functionII.Namespace);
            Assert.Equal("MyNamespaceII.FormatII", functionII.FullyQualifiedName);
            Assert.NotNull(builder.Procedures);
            Assert.Equal(2, builder.Procedures.Count());
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
        public void CanCreateFunctionWithEntityReturnTypeViaEntitySetPath()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            EntityTypeConfiguration<Order> order = builder.EntityType<Order>();

            order.HasRequired<Customer>(o => o.Customer);
            FunctionConfiguration getOrderCustomer = order.Function("GetOrderCustomer").ReturnsEntityViaEntitySetPath<Customer>("bindingParameter/Customer");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmFunction function = Assert.Single(model.SchemaElements.OfType<IEdmFunction>());
            Assert.Equal(EdmExpressionKind.Path, function.EntitySetPath.ExpressionKind);
            Assert.Equal("bindingParameter/Customer", string.Join("/", ((IEdmPathExpression)(function.EntitySetPath)).Path));
        }

        [Fact]
        public void CanCreateFunctionWithCollectionReturnTypeViaEntitySetPath()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            EntityTypeConfiguration<Order> order = builder.EntityType<Order>();

            customer.HasMany<Order>(c => c.Orders);
            FunctionConfiguration getOrders = customer.Function("GetOrders").ReturnsCollectionViaEntitySetPath<Order>("bindingParameter/Orders");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmFunction function = Assert.Single(model.SchemaElements.OfType<IEdmFunction>());
            Assert.Equal(EdmExpressionKind.Path, function.EntitySetPath.ExpressionKind);
            Assert.Equal("bindingParameter/Orders", string.Join("/", ((IEdmPathExpression)(function.EntitySetPath)).Path));
        }

        [Fact]
        public void CanCreateFunctionThatBindsToEntity()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            FunctionConfiguration sendEmail = customer.Function("SendEmail");

            // Assert
            Assert.True(sendEmail.IsBindable);
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
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            FunctionConfiguration sendEmail = customer.Collection.Function("SendEmail");

            // Assert
            Assert.True(sendEmail.IsBindable);
            Assert.NotNull(sendEmail.Parameters);
            Assert.Equal(1, sendEmail.Parameters.Count());
            Assert.Equal(BindingParameterConfiguration.DefaultBindingParameterName, sendEmail.Parameters.Single().Name);
            Assert.Equal(string.Format("Collection({0})", typeof(Customer).FullName), sendEmail.Parameters.Single().TypeConfiguration.FullName);
        }

        [Fact]
        public void CanCreateFunctionWithNonbindingParameters_AddParameterGenericMethod()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            FunctionConfiguration function = builder.Function("MyFunction");
            function.Parameter<string>("p0");
            function.Parameter<int>("p1");
            function.Parameter<Address>("p2");
            ParameterConfiguration[] parameters = function.Parameters.ToArray();

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
        public void CanCreateFunctionWithNonbindingParameters_AddParameterNonGenericMethod()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            FunctionConfiguration function = builder.Function("MyFunction");
            function.Parameter(typeof(string), "p0");
            function.Parameter(typeof(int), "p1");
            function.Parameter(typeof(Address), "p2");
            ParameterConfiguration[] parameters = function.Parameters.ToArray();

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
            function.EntityParameter<Customer>("p6");
            function.CollectionEntityParameter<Employee>("p7");
            ParameterConfiguration[] parameters = function.Parameters.ToArray();
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
        public void CanCreateFunctionWithReturnTypeAsNullableByDefault()
        {
            // Arrange & Act
            ODataModelBuilder builder = new ODataModelBuilder();
            FunctionConfiguration function = builder.Function("MyFunction").Returns<Address>();

            // Assert
            Assert.True(function.OptionalReturn);
        }

        [Fact]
        public void CanCreateFunctionWithReturnTypeAsNullableByOptionalReturn()
        {
            // Arrange & Act
            ODataModelBuilder builder = new ODataModelBuilder();
            FunctionConfiguration function = builder.Function("MyFunction").Returns<Address>();
            function.OptionalReturn = false;

            // Assert
            Assert.False(function.OptionalReturn);
        }

        [Fact]
        public void CanCreateFunctionWithNonbindingParametersAsNullable()
        {
            // Arrange & Act
            ODataModelBuilder builder = new ODataModelBuilder();
            FunctionConfiguration function = builder.Function("MyFunction");
            function.Parameter<string>("p0");
            function.Parameter<string>("p1").OptionalParameter = false;
            function.Parameter<int>("p2").OptionalParameter = true;
            function.Parameter<int>("p3");
            function.Parameter<Address>("p4");
            function.Parameter<Address>("p5").OptionalParameter = false;

            function.CollectionParameter<ZipCode>("p6");
            function.CollectionParameter<ZipCode>("p7").OptionalParameter = false;

            Dictionary<string, ParameterConfiguration> parameters = function.Parameters.ToDictionary(e => e.Name, e => e);

            // Assert
            Assert.True(parameters["p0"].OptionalParameter);
            Assert.False(parameters["p1"].OptionalParameter);

            Assert.True(parameters["p2"].OptionalParameter);
            Assert.False(parameters["p3"].OptionalParameter);

            Assert.True(parameters["p4"].OptionalParameter);
            Assert.False(parameters["p5"].OptionalParameter);

            Assert.True(parameters["p6"].OptionalParameter);
            Assert.False(parameters["p7"].OptionalParameter);
        }

        [Fact]
        public void CanCreateEdmModel_WithBindableFunction()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            customer.HasKey(c => c.CustomerId);
            customer.Property(c => c.Name);
            // Act
            FunctionConfiguration sendEmail = customer.Function("FunctionName");
            sendEmail.Returns<bool>();
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(1, model.SchemaElements.OfType<IEdmFunction>().Count());
            IEdmFunction function = Assert.Single(model.SchemaElements.OfType<IEdmFunction>());
            Assert.False(function.IsComposable);
            Assert.True(function.IsBound);
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
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();

            // Act
            FunctionConfiguration functionConfiguration = builder.Function("FunctionName");
            functionConfiguration.ReturnsFromEntitySet<Customer>("Customers");

            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityContainer container = model.EntityContainer;
            Assert.NotNull(container);
            Assert.Equal(1, container.Elements.OfType<IEdmFunctionImport>().Count());
            Assert.Equal(1, container.Elements.OfType<IEdmEntitySet>().Count());
            IEdmFunctionImport functionImport = Assert.Single(container.Elements.OfType<IEdmFunctionImport>());
            IEdmFunction function = functionImport.Function;
            Assert.False(function.IsComposable);
            Assert.False(function.IsBound);
            Assert.Equal("FunctionName", function.Name);
            Assert.NotNull(function.ReturnType);
            Assert.NotNull(functionImport.EntitySet);
            Assert.Equal("Customers", (functionImport.EntitySet as IEdmEntitySetReferenceExpression).ReferencedEntitySet.Name);
            Assert.Equal(
                typeof(Customer).FullName,
                (functionImport.EntitySet as IEdmEntitySetReferenceExpression).ReferencedEntitySet.EntityType().FullName());
            Assert.Empty(function.Parameters);
        }

        [Fact]
        public void CanCreateEdmModel_ForBindableFunction_WithSupportedParameterType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            customer.HasKey(c => c.CustomerId);
            customer.Property(c => c.Name);

            // Act
            FunctionConfiguration functionBuilder = customer.Function("FunctionName");
            functionBuilder.Parameter<int>("primitive");
            functionBuilder.CollectionParameter<int>("collectionPrimitive");

            functionBuilder.Parameter<bool?>("nullablePrimitive");
            functionBuilder.CollectionParameter<bool?>("nullableCollectionPrimitive");

            functionBuilder.Parameter<Color>("enum");
            functionBuilder.CollectionParameter<Color>("collectionEnum");

            functionBuilder.Parameter<Color?>("nullableEnum");
            functionBuilder.CollectionParameter<Color?>("nullableCollectionEnum");

            functionBuilder.Parameter<Address>("complex");
            functionBuilder.CollectionParameter<Address>("collectionComplex");

            functionBuilder.EntityParameter<Customer>("entity");
            functionBuilder.CollectionEntityParameter<Customer>("collectionEntity");

            functionBuilder.Returns<bool>();

            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(1, model.SchemaElements.OfType<IEdmFunction>().Count());
            IEdmFunction function = Assert.Single(model.SchemaElements.OfType<IEdmFunction>());
            Assert.False(function.IsComposable);
            Assert.True(function.IsBound);
            Assert.Equal("FunctionName", function.Name);
            Assert.NotNull(function.ReturnType);

            Assert.Equal(13, function.Parameters.Count());

            function.AssertHasParameter(model, BindingParameterConfiguration.DefaultBindingParameterName, typeof(Customer), true);

            function.AssertHasParameter(model, parameterName: "primitive", parameterType: typeof(int), isNullable: false);
            function.AssertHasParameter(model, parameterName: "collectionPrimitive", parameterType: typeof(IList<int>), isNullable: false);

            function.AssertHasParameter(model, parameterName: "nullablePrimitive", parameterType: typeof(bool?), isNullable: true);
            function.AssertHasParameter(model, parameterName: "nullableCollectionPrimitive", parameterType: typeof(IList<bool?>), isNullable: true);

            function.AssertHasParameter(model, parameterName: "enum", parameterType: typeof(Color), isNullable: false);
            function.AssertHasParameter(model, parameterName: "collectionEnum", parameterType: typeof(IList<Color>), isNullable: false);

            function.AssertHasParameter(model, parameterName: "nullableEnum", parameterType: typeof(Color?), isNullable: true);
            function.AssertHasParameter(model, parameterName: "nullableCollectionEnum", parameterType: typeof(IList<Color?>), isNullable: true);

            function.AssertHasParameter(model, parameterName: "complex", parameterType: typeof(Address), isNullable: true);
            function.AssertHasParameter(model, parameterName: "collectionComplex", parameterType: typeof(IList<Address>), isNullable: true);

            function.AssertHasParameter(model, parameterName: "entity", parameterType: typeof(Customer), isNullable: true);
            function.AssertHasParameter(model, parameterName: "collectionEntity", parameterType: typeof(IList<Customer>), isNullable: true);
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
            IEdmFunction rewardFunction = Assert.Single(model.SchemaElements.OfType<IEdmFunction>()); // Guard
            FunctionLinkBuilder functionLinkBuilder = model.GetAnnotationValue<FunctionLinkBuilder>(rewardFunction);

            //Assert
            Assert.Equal(expectedUri, reward.GetFunctionLink()(context));
            Assert.NotNull(functionLinkBuilder);
            Assert.Equal(expectedUri, functionLinkBuilder.BuildFunctionLink(context));
        }

        [Fact]
        public void GetEdmModel_SetsNullableIfParameterTypeIsNullable()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            EntityTypeConfiguration<Movie> movie = builder.EntitySet<Movie>("Movies").EntityType;
            var functionBuilder = movie.Function("Watch");
            functionBuilder.Parameter<int>("int");
            functionBuilder.Parameter<Nullable<int>>("nullableOfInt");
            functionBuilder.Parameter<string>("string");
            functionBuilder.EntityParameter<Customer>("customer");
            functionBuilder.CollectionEntityParameter<Customer>("customers");
            functionBuilder.Returns<int>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            //Assert
            var function = Assert.Single(model.SchemaElements.OfType<IEdmFunction>());
            Assert.False(function.FindParameter("int").Type.IsNullable);
            Assert.True(function.FindParameter("nullableOfInt").Type.IsNullable);
            Assert.True(function.FindParameter("string").Type.IsNullable);

            Assert.True(function.FindParameter("customer").Type.IsNullable);
            Assert.True(function.FindParameter("customers").Type.IsNullable);
        }

        [Fact]
        public void GetEdmModel_SetsNullableIfParameterTypeIsReferenceType()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            EntityTypeConfiguration<Movie> movie = builder.EntitySet<Movie>("Movies").EntityType;
            var functionBuilder = movie.Function("Watch");

            functionBuilder.Parameter<string>("string").OptionalParameter = false;
            functionBuilder.Parameter<string>("nullaleString");

            functionBuilder.Parameter<Address>("address").OptionalParameter = false;
            functionBuilder.Parameter<Address>("nullableAddress");

            functionBuilder.CollectionParameter<Address>("addresses").OptionalParameter = false;
            functionBuilder.CollectionParameter<Address>("nullableAddresses");
            functionBuilder.Returns<int>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            //Assert
            IEdmOperation function = Assert.Single(model.SchemaElements.OfType<IEdmFunction>());

            Assert.False(function.FindParameter("string").Type.IsNullable);
            Assert.True(function.FindParameter("nullaleString").Type.IsNullable);

            Assert.False(function.FindParameter("address").Type.IsNullable);
            Assert.True(function.FindParameter("nullableAddress").Type.IsNullable);

            Assert.False(function.FindParameter("addresses").Type.IsNullable);
            Assert.True(function.FindParameter("nullableAddresses").Type.IsNullable);
        }

        [Fact]
        public void GetEdmModel_SetReturnTypeAsNullable()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            EntityTypeConfiguration<Movie> movie = builder.EntitySet<Movie>("Movies").EntityType;
            movie.Function("Watch1").Returns<Address>();
            movie.Function("Watch2").Returns<Address>().OptionalReturn = false;

            // Act
            IEdmModel model = builder.GetEdmModel();

            //Assert
            IEdmOperation function = model.SchemaElements.OfType<IEdmFunction>().First(e => e.Name == "Watch1");
            Assert.True(function.ReturnType.IsNullable);

            function = model.SchemaElements.OfType<IEdmFunction>().First(e => e.Name == "Watch2");
            Assert.False(function.ReturnType.IsNullable);
        }

        [Fact]
        public void GetEdmModel_SetsDateTimeAsParameterType_WorksForDefaultConverter()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            EntityTypeConfiguration<Movie> movie = builder.EntitySet<Movie>("Movies").EntityType;
            var functionBuilder = movie.Function("DateTimeFunction");
            functionBuilder.Parameter<DateTime>("dateTime");
            functionBuilder.Parameter<DateTime?>("nullableDateTime");
            functionBuilder.CollectionParameter<DateTime>("collectionDateTime");
            functionBuilder.CollectionParameter<DateTime?>("nullableCollectionDateTime");
            functionBuilder.Returns<DateTime>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            //Assert
            IEdmOperation function = Assert.Single(model.SchemaElements.OfType<IEdmFunction>());
            Assert.Equal("DateTimeFunction", function.Name);

            Assert.Equal("Edm.DateTimeOffset", function.ReturnType.FullName());
            Assert.False(function.ReturnType.IsNullable);

            IEdmOperationParameter parameter = function.FindParameter("dateTime");
            Assert.Equal("Edm.DateTimeOffset", parameter.Type.FullName());
            Assert.False(parameter.Type.IsNullable);

            parameter = function.FindParameter("nullableDateTime");
            Assert.Equal("Edm.DateTimeOffset", parameter.Type.FullName());
            Assert.True(parameter.Type.IsNullable);

            parameter = function.FindParameter("collectionDateTime");
            Assert.Equal("Collection(Edm.DateTimeOffset)", parameter.Type.FullName());
            Assert.False(parameter.Type.IsNullable);

            parameter = function.FindParameter("nullableCollectionDateTime");
            Assert.Equal("Collection(Edm.DateTimeOffset)", parameter.Type.FullName());
            Assert.True(parameter.Type.IsNullable);
        }

        [Theory]
        [InlineData(typeof(Date), "Edm.Date")]
        [InlineData(typeof(Date?), "Edm.Date")]
        [InlineData(typeof(TimeOfDay), "Edm.TimeOfDay")]
        [InlineData(typeof(TimeOfDay?), "Edm.TimeOfDay")]
        public void CanCreateEdmModel_WithDateAndTimeOfDay_AsFunctionParameter(Type paramType, string expect)
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            EntityTypeConfiguration<Movie> movie = builder.EntitySet<Movie>("Movies").EntityType;
            var functionBuilder = movie.Function("FunctionName").Returns<int>();
            functionBuilder.Parameter(paramType, "p1");

            MethodInfo method = typeof(ProcedureConfiguration).GetMethod("CollectionParameter", BindingFlags.Instance | BindingFlags.Public);
            method.MakeGenericMethod(paramType).Invoke(functionBuilder, new[] { "p2" });

            // Act
            IEdmModel model = builder.GetEdmModel();

            //Assert
            IEdmOperation function = Assert.Single(model.SchemaElements.OfType<IEdmFunction>());
            Assert.Equal("FunctionName", function.Name);

            IEdmOperationParameter parameter = function.FindParameter("p1");
            Assert.Equal(expect, parameter.Type.FullName());
            Assert.Equal(paramType.IsNullable(), parameter.Type.IsNullable);

            parameter = function.FindParameter("p2");
            Assert.Equal("Collection(" + expect + ")", parameter.Type.FullName());
            Assert.Equal(paramType.IsNullable(), parameter.Type.IsNullable);
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
            bindingParameterTypeMock.Setup(o => o.ClrType).Returns(typeof(int));
            IEdmTypeConfiguration bindingParameterType = bindingParameterTypeMock.Object;
            function.SetBindingParameter("IgnoreParameter", bindingParameterType);

            // Act
            function.HasFunctionLink((a) => { throw new NotImplementedException(); }, followsConventions: value);

            // Assert
            Assert.Equal(value, function.FollowsConventions);
        }

        [Fact]
        public void Cant_SetFunctionTitle_OnNonBindableFunctions()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            FunctionConfiguration function = builder.Function("MyFunction");
            function.Returns<int>();
            function.Title = "My Function";

            // Act
            IEdmModel model = builder.GetEdmModel();
            IEdmOperationImport functionImport = model.EntityContainer.OperationImports().OfType<IEdmFunctionImport>().Single();
            Assert.NotNull(functionImport);
            OperationTitleAnnotation functionTitleAnnotation = model.GetOperationTitleAnnotation(functionImport.Operation);

            // Assert
            Assert.Null(functionTitleAnnotation);
        }

        [Fact]
        public void Can_SetFunctionTitle_OnBindable_Functions()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntitySetConfiguration<Movie> movies = builder.EntitySet<Movie>("Movies");
            movies.EntityType.HasKey(m => m.ID);
            FunctionConfiguration entityAction = movies.EntityType.Function("Checkout");
            entityAction.Returns<int>();
            entityAction.Title = "Check out";
            FunctionConfiguration collectionAction = movies.EntityType.Collection.Function("RemoveOld");
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
