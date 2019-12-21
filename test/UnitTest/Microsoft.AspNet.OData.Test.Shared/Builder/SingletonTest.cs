// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
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
#if NETCOREAPP3_1
            ExceptionAssert.Throws<ArgumentException>(() => builder.AddSingleton(null, entityType),
                "The argument 'name' is null or empty. (Parameter 'name')");
#else
            ExceptionAssert.Throws<ArgumentException>(() => builder.AddSingleton(null, entityType),
                "The argument 'name' is null or empty.\r\nParameter name: name");
#endif
        }

        [Fact]
        public void AddSingleton_ThrowException_IfEntityTypeNull()
        {
            // Arrange
            var builder = new ODataModelBuilder();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => builder.AddSingleton("OsCorp", entityType: null),
                "entityType");
        }

        [Fact]
        public void AddSingleton_ThrowException_IfSingletonNameContainsADot()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var entityType = new Mock<EntityTypeConfiguration>().Object;

            // Act & Assert
            ExceptionAssert.Throws<NotSupportedException>(() => builder.AddSingleton("My.Singleton", entityType),
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
#if NETCOREAPP3_1
            ExceptionAssert.Throws<ArgumentException>(() => builder.AddSingleton("OsCorp", otherType),
                "The singleton 'OsCorp' was already configured with a different EntityType ('Company'). (Parameter 'entityType')");
#else
            ExceptionAssert.Throws<ArgumentException>(() => builder.AddSingleton("OsCorp", otherType),
                "The singleton 'OsCorp' was already configured with a different EntityType ('Company').\r\nParameter name: entityType");
#endif
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
            Assert.Single(builder.Singletons);
        }

        [Fact]
        public void AddSingleton_ByName()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            var singletonType = builder.Singleton<Company>("OsCorp");

            // Assert
            Assert.IsType<SingletonConfiguration<Company>>(singletonType);
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
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Company", osCorp.EntityType().FullName());

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
        public void CreateEdmModel_WithSingleton_CanAddBindingPath_ToNavigationProperty_WithComplex()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();

            var motorcycle = builder.AddEntityType(typeof(Motorcycle));
            var myMotor = builder.AddSingleton("MyMotor", motorcycle);

            var manufacturer = builder.AddComplexType(typeof(MotorcycleManufacturer));
            var address = builder.AddEntityType(typeof(ManufacturerAddress));

            motorcycle.AddComplexProperty(typeof(Motorcycle).GetProperty("Manufacturer"));
            var navProperty = manufacturer.AddNavigationProperty(typeof(Manufacturer).GetProperty("Address"), EdmMultiplicity.One);

            var addresses = builder.AddEntitySet("Addresses", address);
            myMotor.AddBinding(navProperty, addresses, new List<MemberInfo>
            {
                typeof(Motorcycle).GetProperty("Manufacturer"),
                typeof(Manufacturer).GetProperty("Address")
            });

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var motorcycleEdmType = model.AssertHasEntityType(typeof(Motorcycle));
            Assert.Empty(motorcycleEdmType.NavigationProperties());

            var manufacturerEdmType = model.AssertHasComplexType(typeof(MotorcycleManufacturer));

            var edmNavProperty = manufacturerEdmType.AssertHasNavigationProperty(model, "Address",
                typeof(ManufacturerAddress), isNullable: false, multiplicity: EdmMultiplicity.One);

            var myMotorSingleton = model.EntityContainer.FindSingleton("MyMotor");
            Assert.NotNull(myMotorSingleton);

            var bindings = myMotorSingleton.FindNavigationPropertyBindings(edmNavProperty);
            var binding = Assert.Single(bindings);

            Assert.Equal("Address", binding.NavigationProperty.Name);
            Assert.Equal("Addresses", binding.Target.Name);
            Assert.Equal("Manufacturer/Address", binding.Path.Path);
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
                model.GetNavigationSourceLinkBuilder(edmContoso).BuildNavigationLink(new ResourceContext(), edmNavProperty, ODataMetadataLevel.MinimalMetadata).AbsoluteUri);
        }

        [Fact]
        public void SingletonAddBinding_DoesnotThrows_IfBindingNavigationPropertyIsNotPartOfEntityType()
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
            ExceptionAssert.DoesNotThrow(() => myVehicle.AddBinding(navProperty, fordo));
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
            ExceptionAssert.ThrowsArgument(
                () => myVehicle.HasNavigationPropertyLink(navProperty, new NavigationLinkBuilder((ctxt, property) => new Uri("http://works/"), followsConventions: false)),
                "navigationProperty",
                "The declaring entity type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Car' of the given navigation property is not a part of the " +
                "entity type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Vehicle' hierarchy of the entity set or singleton 'MyVehicle'.");
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
