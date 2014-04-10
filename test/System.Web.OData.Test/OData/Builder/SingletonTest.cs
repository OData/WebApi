// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Formatter;
using System.Web.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder
{
    public class SingletonTest
    {
        [Fact]
        public void AddSingleton_ThrowException_IfSingletonNameNull()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var entityType = new Mock<EntityTypeConfiguration>().Object;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => builder.AddSingleton(null, entityType),
                "The argument 'name' is null or empty.\r\nParameter name: name");
        }

        [Fact]
        public void AddSingleton_ThrowException_IfEntityTypeNull()
        {
            // Arrange
            var builder = new ODataModelBuilder();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => builder.AddSingleton("OsCorp", entityType: null),
                "entityType");
        }

        [Fact]
        public void AddSingleton_ThrowException_IfSingletonNameContainsADot()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var entityType = new Mock<EntityTypeConfiguration>().Object;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => builder.AddSingleton("My.Singleton", entityType),
                "'My.Singleton' is not a valid singleton name. The singleton name cannot contain '.'.");
        }

        [Fact]
        public void AddSingleton_ThrowException_IfAddTheSameSingletonTwiceWithDifferentType()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var companyType = new Mock<EntityTypeConfiguration>();
            companyType.Setup(a => a.Name).Returns("Company");
            builder.AddSingleton("OsCorp", companyType.Object);
            var otherType = new Mock<EntityTypeConfiguration>().Object;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => builder.AddSingleton("OsCorp", otherType),
                "The singleton 'OsCorp' was already configured with a different EntityType ('Company').\r\nParameter name: entityType");
        }

        [Fact]
        public void AddSingleton_OnlyExistOne_IfAddTheSameSingletonTwiceWithSameType()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var entityType = new Mock<EntityTypeConfiguration>().Object;
            var config1 = builder.AddSingleton("Singleton", entityType);

            // Act
            var config2 = builder.AddSingleton("Singleton", entityType);

            // Assert
            Assert.Same(config1, config2);
            Assert.Equal(1, builder.Singletons.Count());
        }

        [Fact]
        public void AddSingleton_ByName()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            var singletonType = builder.Singleton<Company>("OsCorp");

            // Assert
            Assert.IsType(typeof(SingletonConfiguration<Company>), singletonType);
            var singleton = Assert.Single(builder.Singletons);
            Assert.Equal("OsCorp", singleton.Name);
            Assert.Equal(typeof(Company), singleton.ClrType);
        }

        [Fact]
        public void RemoveSingleton_ByName()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.Singleton<Car>("Contoso");

            // Act
            bool removed = builder.RemoveSingleton("Contoso");

            // Assert
            Assert.True(removed);
            Assert.Empty(builder.Singletons);
        }

        [Fact]
        public void CreateEdmModel_WithSingleton()
        {
            // Arrange
            var builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            var singleton = builder.Singleton<Company>("OsCorp");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);

            var container = model.SchemaElements.OfType<IEdmEntityContainer>().SingleOrDefault();
            Assert.NotNull(container);

            var osCorp = container.FindSingleton("OsCorp");
            Assert.NotNull(osCorp);
            Assert.Equal("Company", osCorp.EntityType().Name);
        }

        [Fact]
        public void CreateEdmModel_WithSingletonAndEntitySet_UnderTheSameEntityType()
        {
            // Arrange
            var builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            var entitySet = builder.EntitySet<Company>("Companies");
            var singleton = builder.Singleton<Company>("OsCorp");

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();
            Assert.NotNull(model);

            var container = model.SchemaElements.OfType<IEdmEntityContainer>().SingleOrDefault();
            Assert.NotNull(container);

            var osCorp = container.FindSingleton("OsCorp");
            Assert.NotNull(osCorp);
            Assert.Equal("System.Web.OData.Builder.TestModels.Company", osCorp.EntityType().FullName());

            var companies = container.FindEntitySet("Companies");
            Assert.NotNull(companies);
            Assert.Equal(osCorp.EntityType().FullName(), companies.EntityType().FullName());
        }

         [Fact]
        public void CreateEdmModel_WithSingletonAndEntitySet_AndEntitySetBindingToSingleton()
        {
            // Arrange
            var builder = new ODataModelBuilder()
                .Add_Company_EntityType()
                .Add_Employee_EntityType()
                .Add_CompanyEmployees_Relationship();

            // EntitySet -> Singleton
            builder.EntitySet<Company>("Companies").HasSingletonBinding(c => c.CEO, "Boss");

            // Act & Assert
            var model = builder.GetEdmModel();
            Assert.NotNull(model);

            var container = model.SchemaElements.OfType<IEdmEntityContainer>().SingleOrDefault();
            Assert.NotNull(container);

            var entitySet = container.FindEntitySet("Companies");
            Assert.NotNull(entitySet);

            var singleton = container.FindSingleton("Boss");
            Assert.NotNull(singleton);

            var ceo = entitySet.NavigationPropertyBindings.FirstOrDefault(nt => nt.NavigationProperty.Name == "CEO");
            Assert.NotNull(ceo);
            Assert.Same(singleton, ceo.Target);
            Assert.Equal("Boss", ceo.Target.Name);
        }

        [Fact]
        public void CreateEdmModel_WithSingletonAndEntitySet_AndSingletonBindingToEntitySet()
        {
            // Arrange
            var builder = new ODataModelBuilder()
                .Add_Company_EntityType()
                .Add_Employee_EntityType()
                .Add_CompanyEmployees_Relationship();

            // Singleton -> EntitySet
            builder.Singleton<Company>("OsCorp").HasManyBinding(c => c.ComplanyEmployees, "Employees");

            // Act & Assert
            var model = builder.GetEdmModel();
            Assert.NotNull(model);

            var container = model.SchemaElements.OfType<IEdmEntityContainer>().SingleOrDefault();
            Assert.NotNull(container);

            var osCorp = container.FindSingleton("OsCorp");
            Assert.NotNull(osCorp);

            var employees = container.FindEntitySet("Employees");
            Assert.NotNull(employees);

            var companyEmployees = osCorp.NavigationPropertyBindings.FirstOrDefault(nt => nt.NavigationProperty.Name == "ComplanyEmployees");
            Assert.NotNull(companyEmployees);
            Assert.Equal("Employees", companyEmployees.Target.Name);
        }

        [Fact]
        public void CreateEdmModel_WithSingletons_AndSingletonBindingToSingleton()
        {
            // Arrange
            var builder = new ODataModelBuilder()
                .Add_Company_EntityType()
                .Add_Employee_EntityType()
                .Add_CompanyEmployees_Relationship();

            // Singleton -> Singleton
            builder.Singleton<Company>("OsCorp").HasSingletonBinding(c => c.CEO, "Boss");

            // Act & Assert
            var model = builder.GetEdmModel();
            Assert.NotNull(model);

            var container = model.SchemaElements.OfType<IEdmEntityContainer>().SingleOrDefault();
            Assert.NotNull(container);

            var osCorp = container.FindSingleton("OsCorp");
            Assert.NotNull(osCorp);

            var gates = container.FindSingleton("Boss");
            Assert.NotNull(gates);

            var vipCompanyVipEmployee = osCorp.NavigationPropertyBindings.FirstOrDefault(nt => nt.NavigationProperty.Name == "CEO");
            Assert.NotNull(vipCompanyVipEmployee);
            Assert.Same(gates, vipCompanyVipEmployee.Target);
            Assert.Equal("Boss", vipCompanyVipEmployee.Target.Name);
        }

        [Fact]
        public void CreateEdmModel_WithSingleton_CanAddBindingToDerivedNavigationProperty()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();

            var vehicle = builder.AddEntityType(typeof(Vehicle));
            var motorcycle = builder.AddEntityType(typeof(Motorcycle)).DerivesFrom(vehicle);
            var manufacturer = builder.AddEntityType(typeof(MotorcycleManufacturer));
            var yamaha = builder.AddSingleton("Yamaha", manufacturer);
            var navProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.One);

            var r7 = builder.AddSingleton("Yamaha-R7", motorcycle);
            r7.AddBinding(navProperty, yamaha);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var motorcycleEdmType = model.AssertHasEntityType(typeof(Motorcycle));
            var edmNavProperty = motorcycleEdmType.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: false, multiplicity: EdmMultiplicity.One);
            Assert.Equal(
                "Yamaha",
                model.EntityContainer.FindSingleton("Yamaha-R7").FindNavigationTarget(edmNavProperty).Name);
        }

        [Fact]
        public void CreateEdmModel_WithSingleton_CanAddNavigationLinkToDerivedNavigationProperty()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();

            var vehicle = builder.AddEntityType(typeof(Vehicle));
            var car = builder.AddEntityType(typeof(Car)).DerivesFrom(vehicle);
            var manufacturer = builder.AddEntityType(typeof(CarManufacturer));
            var fordo = builder.AddSingleton("Fordo", manufacturer);
            var navProperty = car.AddNavigationProperty(typeof(Car).GetProperty("Manufacturer"), EdmMultiplicity.One);

            var contoso = builder.AddSingleton("Contoso", vehicle);
            var binding = contoso.AddBinding(navProperty, fordo);
            contoso.HasNavigationPropertyLink(navProperty, new NavigationLinkBuilder((ctxt, property) => new Uri("http://works/"), followsConventions: false));

            // Act & assert
            IEdmModel model = builder.GetEdmModel();
            var motorcycleEdmType = model.AssertHasEntityType(typeof(Car));
            var edmNavProperty = motorcycleEdmType.AssertHasNavigationProperty(model, "Manufacturer", typeof(CarManufacturer), isNullable: false, multiplicity: EdmMultiplicity.One);
            var edmContoso = model.EntityContainer.FindSingleton("Contoso");

            Assert.NotNull(model.GetNavigationSourceLinkBuilder(edmContoso));
            Assert.Equal(
                "http://works/",
                model.GetNavigationSourceLinkBuilder(edmContoso).BuildNavigationLink(new EntityInstanceContext(), edmNavProperty, ODataMetadataLevel.Default).AbsoluteUri);
        }

        [Fact]
        public void SingletonAddBinding_Throws_IfBindingNavigationPropertyIsNotPartOfEntityType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            var vehicleType = builder.AddEntityType(typeof(Vehicle));
            var carType = builder.AddEntityType(typeof(Car));
            var manufacturerType = builder.AddEntityType(typeof(CarManufacturer));
            var fordo = builder.AddSingleton("Fordo", manufacturerType);
            var navProperty = carType.AddNavigationProperty(typeof(Car).GetProperty("Manufacturer"), EdmMultiplicity.One);
            var myVehicle = builder.AddSingleton("MyVehicle", vehicleType);

            // Act & Assert
            Assert.ThrowsArgument(
                () => myVehicle.AddBinding(navProperty, fordo),
                "navigationConfiguration",
                "The declaring entity type 'System.Web.OData.Builder.TestModels.Car' of the given navigation property is not a part of " +
                "the entity type 'System.Web.OData.Builder.TestModels.Vehicle' hierarchy of the entity set or singleton 'MyVehicle'.");
        }

        [Fact]
        public void SingletonAddNavigationLink_Throws_IfNavigationPropertyInHierarchyIsNotPartOfEntityType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            var vehicleType = builder.AddEntityType(typeof(Vehicle));
            var carType = builder.AddEntityType(typeof(Car));
            var manufacturerType = builder.AddEntityType(typeof(CarManufacturer));
            var fordo = builder.AddSingleton("Fordo", manufacturerType);

            var navProperty = carType.AddNavigationProperty(typeof(Car).GetProperty("Manufacturer"), EdmMultiplicity.One);
            var myVehicle = builder.AddSingleton("MyVehicle", vehicleType);

            // Act & Assert
            Assert.ThrowsArgument(
                () => myVehicle.HasNavigationPropertyLink(navProperty, new NavigationLinkBuilder((ctxt, property) => new Uri("http://works/"), followsConventions: false)),
                "navigationProperty",
                "The declaring entity type 'System.Web.OData.Builder.TestModels.Car' of the given navigation property is not a part of the " +
                "entity type 'System.Web.OData.Builder.TestModels.Vehicle' hierarchy of the entity set or singleton 'MyVehicle'.");
        }

        [Fact]
        public void SingletonCanConfigureOptionalBinding_For_NavigationPropertiesInDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            builder.Singleton<Vehicle>("MyVehicle")
                .HasOptionalBinding((Car m) => m.Manufacturer, "manufacturers");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var myVehicle = model.EntityContainer.FindSingleton("MyVehicle");
            Assert.NotNull(myVehicle);

            var car = model.AssertHasEntityType(typeof(Car));
            var carManufacturerProperty = car.AssertHasNavigationProperty(model, "Manufacturer", typeof(CarManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);

            var carManufacturerPropertyTarget = myVehicle.FindNavigationTarget(carManufacturerProperty);
            Assert.NotNull(carManufacturerPropertyTarget);
            Assert.Equal("manufacturers", carManufacturerPropertyTarget.Name);
        }

        [Fact]
        public void SingletonCanConfigureRequiredBinding_For_NavigationPropertiesInDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            builder
                .Singleton<Vehicle>("MyVehicle")
                .HasRequiredBinding((Car m) => m.Manufacturer, "manufacturers");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var myVehicle = model.EntityContainer.FindSingleton("MyVehicle");
            Assert.NotNull(myVehicle);

            var car = model.AssertHasEntityType(typeof(Car));
            var carManufacturerProperty = car.AssertHasNavigationProperty(model, "Manufacturer", typeof(CarManufacturer), isNullable: false, multiplicity: EdmMultiplicity.One);

            var carManufacturerPropertyTarget = myVehicle.FindNavigationTarget(carManufacturerProperty);
            Assert.NotNull(carManufacturerPropertyTarget);
            Assert.Equal("manufacturers", carManufacturerPropertyTarget.Name);
        }

        [Fact]
        public void SingletonCanConfigureManyBinding_For_NavigationPropertiesInDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            builder
                .Singleton<Vehicle>("MyVehicle")
                .HasManyBinding((Motorcycle m) => m.Manufacturers, "manufacturers");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var myVehicle = model.EntityContainer.FindSingleton("MyVehicle");
            Assert.NotNull(myVehicle);

            var motorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            var motorcycleManufacturerProperty = motorcycle.AssertHasNavigationProperty(model, "Manufacturers", typeof(MotorcycleManufacturer), isNullable: false, multiplicity: EdmMultiplicity.Many);

            var motorcycleManufacturerPropertyTarget = myVehicle.FindNavigationTarget(motorcycleManufacturerProperty);
            Assert.NotNull(motorcycleManufacturerPropertyTarget);
            Assert.Equal("manufacturers", motorcycleManufacturerPropertyTarget.Name);
        }
    }
}
