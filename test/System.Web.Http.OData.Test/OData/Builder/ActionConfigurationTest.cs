// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.TestCommon;

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
            ActionConfiguration action = new ActionConfiguration(builder, "Format");

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
            ProcedureConfiguration toRemove = new ActionConfiguration(builder2, "ToRemove");
            
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
            ActionConfiguration action = new ActionConfiguration(builder, "CreateMessage");
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
            ActionConfiguration action = new ActionConfiguration(builder, "CreateMessages");
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
            
            ActionConfiguration createAddress = new ActionConfiguration(builder, "CreateAddress");
            createAddress.Returns<Address>();
            
            ActionConfiguration createAddresses = new ActionConfiguration(builder, "CreateAddresses");
            createAddresses.ReturnsCollection<Address>();

            // Assert
            IComplexTypeConfiguration address = createAddress.ReturnType as IComplexTypeConfiguration;
            Assert.NotNull(address);
            Assert.Equal(typeof(Address).FullName, address.FullName);
            Assert.Null(createAddress.EntitySet);

            ICollectionTypeConfiguration addresses = createAddresses.ReturnType as ICollectionTypeConfiguration;
            Assert.NotNull(addresses);
            Assert.Equal(string.Format("Collection({0})", typeof(Address).FullName), addresses.FullName);
            address = addresses.ElementType as IComplexTypeConfiguration;
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

            ActionConfiguration createGoodCustomer = new ActionConfiguration(builder, "CreateGoodCustomer");
            createGoodCustomer.ReturnsFromEntitySet<Customer>("GoodCustomers");

            ActionConfiguration createBadCustomers = new ActionConfiguration(builder, "CreateBadCustomers");
            createBadCustomers.ReturnsCollectionFromEntitySet<Customer>("BadCustomers");

            // Assert
            IEntityTypeConfiguration customer = createGoodCustomer.ReturnType as IEntityTypeConfiguration;
            Assert.NotNull(customer);
            Assert.Equal(typeof(Customer).FullName, customer.FullName);
            IEntitySetConfiguration goodCustomers = builder.EntitySets.SingleOrDefault(s => s.Name == "GoodCustomers");
            Assert.NotNull(goodCustomers);
            Assert.Same(createGoodCustomer.EntitySet, goodCustomers);

            ICollectionTypeConfiguration customers = createBadCustomers.ReturnType as ICollectionTypeConfiguration;
            Assert.NotNull(customers);
            customer = customers.ElementType as IEntityTypeConfiguration;
            Assert.NotNull(customer);
            IEntitySetConfiguration badCustomers = builder.EntitySets.SingleOrDefault(s => s.Name == "BadCustomers");
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
            Assert.NotNull(sendEmail.Parameters);
            Assert.Equal(1, sendEmail.Parameters.Count());
            Assert.Equal(BindingParameterConfiguration.DefaultBindingParameterName, sendEmail.Parameters.Single().Name);
            Assert.Equal(string.Format("Collection({0})", typeof(Customer).FullName), sendEmail.Parameters.Single().TypeConfiguration.FullName);
        }

        [Fact]
        public void CanCreateActionWithNonBindingParameters()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = new ActionConfiguration(builder, "MyAction");
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
            ActionConfiguration actionConfiguration = new ActionConfiguration(builder, "ActionName");
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
            Assert.Equal("ActionName", action.Name);
            Assert.NotNull(action.ReturnType);
            Assert.NotNull(action.EntitySet);
            Assert.Equal("Customers", (action.EntitySet as IEdmEntitySetReferenceExpression).ReferencedEntitySet.Name);
            Assert.Equal(typeof(Customer).FullName, (action.EntitySet as IEdmEntitySetReferenceExpression).ReferencedEntitySet.ElementType.FullName());
            Assert.Empty(action.Parameters);
        }
    }
}
