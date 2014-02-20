// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.OData.TestCommon;
using System.Web.OData.TestCommon.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.TestCommon;
using Moq;
using BuilderTestModels = System.Web.OData.Builder.TestModels;

namespace System.Web.OData.Builder
{
    public class ODataModelBuilderTest
    {
        [Fact]
        public void RemoveStructuralType_RemovesComplexType()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.AddComplexType(typeof(Customer));

            Assert.NotEmpty(builder.StructuralTypes);

            builder.RemoveStructuralType(typeof(Customer));
            Assert.Empty(builder.StructuralTypes);
        }

        [Fact]
        public void RemoveStructuralType_RemovesEntityType()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.AddEntityType(typeof(Customer));

            Assert.NotEmpty(builder.StructuralTypes);

            builder.RemoveStructuralType(typeof(Customer));
            Assert.Empty(builder.StructuralTypes);
        }

        [Fact]
        public void CanRemoveProcedureByName()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = builder.Action("Format");
            bool removed = builder.RemoveProcedure("Format");

            // Assert      
            Assert.Equal(0, builder.Procedures.Count());
        }

        [Fact]
        public void CanRemoveProcedure()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = builder.Action("Format");
            ProcedureConfiguration procedure = builder.Procedures.SingleOrDefault();
            bool removed = builder.RemoveProcedure(procedure);

            // Assert
            Assert.True(removed);
            Assert.Equal(0, builder.Procedures.Count());
        }

        [Fact]
        public void RemoveProcedureByNameThrowsWhenAmbiguous()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();

            ActionConfiguration action1 = builder.Action("Format");
            ActionConfiguration action2 = builder.Action("Format");
            action2.Parameter<int>("SegmentSize");

            Assert.Throws<InvalidOperationException>(() =>
            {
                builder.RemoveProcedure("Format");
            });
        }

        [Fact]
        public void BuilderIncludesMapFromEntityTypeToBindableProcedures()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntitySet<Customer>("Customers").EntityType;
            customer.HasKey(c => c.Id);
            customer.Property(c => c.Name);
            customer.Action("Reward");
            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType customerType = model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault();

            // Act
            BindableProcedureFinder finder = model.GetAnnotationValue<BindableProcedureFinder>(model);

            // Assert
            Assert.NotNull(finder);
            Assert.NotNull(finder.FindProcedures(customerType).SingleOrDefault());
            Assert.Equal("Reward", finder.FindProcedures(customerType).SingleOrDefault().Name);
        }

        [Fact]
        public void DataServiceVersion_RoundTrips()
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            Assert.Reflection.Property(builder, b => b.DataServiceVersion, new Version(4, 0), allowNull: false, roundTripTestValue: new Version(1, 0));
        }

        [Fact]
        public void MaxDataServiceVersion_RoundTrips()
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            Assert.Reflection.Property(builder, b => b.MaxDataServiceVersion, new Version(4, 0), allowNull: false, roundTripTestValue: new Version(1, 0));
        }

        [Fact]
        public void DataServiceVersion_Is_AppliedToTheResultingModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.DataServiceVersion = new Version(2, 2);

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(new Version(2, 2), model.GetDataServiceVersion());
        }

        [Fact]
        public void MaxDataServiceVersion_Is_AppliedToTheResultingModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.MaxDataServiceVersion = new Version(2, 2);

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(new Version(2, 2), model.GetMaxDataServiceVersion());
        }

        [Fact]
        public void EntityContainer_Is_Default()
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            IEdmModel model = builder.GetEdmModel();

            Assert.Same(model.EntityContainer, model.SchemaElements.OfType<IEdmEntityContainer>().Single());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ActionLink_PreservesFollowsConventions(bool value)
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            ActionConfiguration configuration = new ActionConfiguration(builder, "IgnoreAction");
            Mock<IEdmTypeConfiguration> bindingParameterTypeMock = new Mock<IEdmTypeConfiguration>();
            bindingParameterTypeMock.Setup(o => o.Kind).Returns(EdmTypeKind.Entity);
            Type entityType = typeof(object);
            bindingParameterTypeMock.Setup(o => o.ClrType).Returns(entityType);
            configuration.SetBindingParameter("IgnoreParameter", bindingParameterTypeMock.Object,
                alwaysBindable: false);
            configuration.HasActionLink((a) => { throw new NotImplementedException(); }, followsConventions: value);
            builder.AddProcedure(configuration);
            builder.AddEntityType(entityType);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var action = Assert.Single(model.SchemaElements.OfType<IEdmAction>());
            ActionLinkBuilder actionLinkBuilder = model.GetActionLinkBuilder(action);
            Assert.NotNull(actionLinkBuilder);
            Assert.Equal(value, actionLinkBuilder.FollowsConventions);
        }

        [Fact]
        public void GetEdmModel_PropertyWithETag_IsConcurrencyToken()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            customer.HasKey(c => c.Id);
            customer.Property(c => c.Id);
            customer.Property(c => c.Name).IsConcurrencyToken();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType type = model.AssertHasEntityType(typeof(Customer));
            IEdmStructuralProperty property =
                type.AssertHasPrimitiveProperty(model, "Name", EdmPrimitiveTypeKind.String, isNullable: true);
            Assert.Equal(EdmConcurrencyMode.Fixed, property.ConcurrencyMode);
        }

        [Fact]
        public void GetEdmModel_DoesntCreateOperationImport_For_BoundedOperations()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntitySetConfiguration<Customer> customers = builder.EntitySet<Customer>("Customers");
            customers.EntityType.HasKey(c => c.Id);
            customers.EntityType.Action("Action").Returns<bool>();
            customers.EntityType.Collection.Action("CollectionAction").Returns<bool>();
            customers.EntityType.Function("Function").Returns<bool>();
            customers.EntityType.Collection.Function("Function").Returns<bool>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(0, model.EntityContainer.OperationImports().Count());
        }

        [Fact]
        public void GetEdmModel_CreatesOperationImports_For_UnboundedOperations()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.Function("Function").Returns<bool>();
            builder.Action("Action").Returns<bool>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(2, model.EntityContainer.OperationImports().Count());
        }

        [Fact]
        public void Validate_Throws_If_Entity_Doesnt_Have_Key_Defined()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<Customer>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => builder.GetEdmModel(), "The entity 'Customer' does not have a key defined.");
        }

        [Fact]
        public void Validate_Doesnt_Throw_For_Derived_Entities()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<BuilderTestModels.Car>().DerivesFrom<BuilderTestModels.Vehicle>();
            builder.EntityType<BuilderTestModels.Vehicle>().HasKey(v => v.Name);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
        }
    }
}
