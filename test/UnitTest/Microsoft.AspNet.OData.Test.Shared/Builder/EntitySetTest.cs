// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Formatter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
{
    public class EntitySetTest
    {
        [Fact]
        [Trait("Description", "ODataModelBuilder can create an Entity and EntitySet")]
        public void CreateModelWithEntitySet()
        {
            var builder = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet();
            builder.EntitySet<Customer>("Customers");

            var model = builder.GetServiceModel();
            var container = model.SchemaElements.OfType<IEdmEntityContainer>().SingleOrDefault();
            Assert.NotNull(container);
            var customers = container.EntitySets().SingleOrDefault(e => e.Name == "Customers");
            Assert.NotNull(customers);
            Assert.Equal("Customer", customers.EntityType().Name);
        }

        [Fact]
        [Trait("Description", "ODataModelBuilder can create two Entities, two Entities sets and a binding")]
        public void CreateModelWithTwoEntitySetsAndABinding()
        {
            var builder = new ODataModelBuilder()
                            .Add_Customer_EntityType()
                            .Add_Order_EntityType()
                            .Add_CustomerOrders_Relationship()
                            .Add_CustomerOrders_Binding();

            var model = builder.GetServiceModel();
            var container = model.SchemaElements.OfType<IEdmEntityContainer>().SingleOrDefault();
            Assert.NotNull(container);
            var customers = container.EntitySets().SingleOrDefault(e => e.Name == "Customers");
            Assert.NotNull(customers);
            var orders = container.EntitySets().SingleOrDefault(e => e.Name == "Orders");
            Assert.NotNull(orders);
            var customerOrders = customers.NavigationPropertyBindings.FirstOrDefault(nt => nt.NavigationProperty.Name == "Orders");
            Assert.NotNull(customerOrders);
            Assert.Equal("Orders", customerOrders.Target.Name);
        }

        [Fact]
        public void CanAddBinding_For_NavigationProperty()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();

            var motorcycle = builder.AddEntityType(typeof(Motorcycle));
            var manufacturer = builder.AddEntityType(typeof(MotorcycleManufacturer));
            var manufacturers = builder.AddEntitySet("manufacturers", manufacturer);
            var navProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.One);

            var motors = builder.AddEntitySet("Motorcycles", motorcycle);
            motors.AddBinding(navProperty, manufacturers);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var motorcycleEdmType = model.AssertHasEntityType(typeof(Motorcycle));
            var edmNavProperty = motorcycleEdmType.AssertHasNavigationProperty(model, "Manufacturer",
                typeof(MotorcycleManufacturer), isNullable: false, multiplicity: EdmMultiplicity.One);

            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Motorcycles");
            Assert.NotNull(entitySet);

            var target = entitySet.FindNavigationTarget(edmNavProperty);
            Assert.NotNull(target);
            Assert.Equal("manufacturers", target.Name);

            var binding = Assert.Single(entitySet.FindNavigationPropertyBindings(edmNavProperty));
            Assert.Same(target, binding.Target);
            Assert.Equal("Manufacturer", binding.Path.Path);
        }

        [Fact]
        public void CanAddBinding_For_DerivedNavigationProperty()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();

            var vehicle = builder.AddEntityType(typeof(Vehicle));
            var motorcycle = builder.AddEntityType(typeof(Motorcycle)).DerivesFrom(vehicle);
            var manufacturer = builder.AddEntityType(typeof(MotorcycleManufacturer));
            var manufacturers = builder.AddEntitySet("manufacturers", manufacturer);
            var navProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.One);

            var vehicles = builder.AddEntitySet("vehicles", vehicle);
            vehicles.AddBinding(navProperty, manufacturers);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var motorcycleEdmType = model.AssertHasEntityType(typeof(Motorcycle));
            var edmNavProperty = motorcycleEdmType.AssertHasNavigationProperty(model, "Manufacturer",
                typeof(MotorcycleManufacturer), isNullable: false, multiplicity: EdmMultiplicity.One);

            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("vehicles");
            Assert.NotNull(entitySet);

            var target = entitySet.FindNavigationTarget(edmNavProperty);
            Assert.NotNull(target);
            Assert.Equal("manufacturers", target.Name);

            var binding = Assert.Single(entitySet.FindNavigationPropertyBindings(edmNavProperty));
            Assert.Same(target, binding.Target);
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle/Manufacturer", binding.Path.Path);
        }

        [Fact]
        public void CanAddNavigationLink_For_DerivedNavigationProperty()
        {
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();

            var vehicle = builder.AddEntityType(typeof(Vehicle));
            var motorcycle = builder.AddEntityType(typeof(Motorcycle)).DerivesFrom(vehicle);
            var manufacturer = builder.AddEntityType(typeof(MotorcycleManufacturer));
            var manufacturers = builder.AddEntitySet("manufacturers", manufacturer);
            var navProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.One);

            var vehicles = builder.AddEntitySet("vehicles", vehicle);
            var binding = vehicles.AddBinding(navProperty, manufacturers);
            vehicles.HasNavigationPropertyLink(navProperty, new NavigationLinkBuilder((ctxt, property) => new Uri("http://works/"), followsConventions: false));

            IEdmModel model = builder.GetEdmModel();
            var motorcycleEdmType = model.AssertHasEntityType(typeof(Motorcycle));
            var edmNavProperty = motorcycleEdmType.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: false, multiplicity: EdmMultiplicity.One);
            var vehiclesEdmSet = model.EntityContainer.FindEntitySet("vehicles");

            Assert.NotNull(model.GetNavigationSourceLinkBuilder(vehiclesEdmSet));
            Assert.Equal(
                "http://works/",
                model.GetNavigationSourceLinkBuilder(vehiclesEdmSet).BuildNavigationLink(new ResourceContext(), edmNavProperty, ODataMetadataLevel.MinimalMetadata).AbsoluteUri);
        }

        [Fact]
        public void AddBinding_For_NavigationPropertyInHierarchy_DoesnotThrows()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            var vehicle = builder.AddEntityType(typeof(Vehicle));
            var motorcycle = builder.AddEntityType(typeof(Motorcycle));
            var manufacturer = builder.AddEntityType(typeof(MotorcycleManufacturer));
            var manufacturers = builder.AddEntitySet("manufacturers", manufacturer);

            var navProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.One);

            var vehicles = builder.AddEntitySet("vehicles", vehicle);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => vehicles.AddBinding(navProperty, manufacturers));
        }

        [Fact]
        public void AddNavigationLink_For_NavigationPropertyInHierarchy_Throws()
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            var vehicle = builder.AddEntityType(typeof(Vehicle));
            var motorcycle = builder.AddEntityType(typeof(Motorcycle));
            var manufacturer = builder.AddEntityType(typeof(MotorcycleManufacturer));
            var manufacturers = builder.AddEntitySet("manufacturers", manufacturer);

            var navProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.One);

            var vehicles = builder.AddEntitySet("vehicles", vehicle);

            ExceptionAssert.ThrowsArgument(
                () => vehicles.HasNavigationPropertyLink(navProperty, new NavigationLinkBuilder((ctxt, property) => new Uri("http://works/"), followsConventions: false)),
                "navigationProperty",
                "The declaring entity type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle' " +
                "of the given navigation property is not a part of the entity type " +
                "'Microsoft.AspNet.OData.Test.Builder.TestModels.Vehicle' hierarchy of the entity set or singleton 'vehicles'.");
        }

        [Fact]
        public void CanConfigureOptionalBinding_For_NavigationPropertiesInDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            builder
                .EntitySet<Vehicle>("vehicles")
                .HasOptionalBinding((Motorcycle m) => m.Manufacturer, "manufacturers");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var vehicles = model.EntityContainer.FindEntitySet("vehicles");
            Assert.NotNull(vehicles);

            var motorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            var motorcycleManufacturerProperty = motorcycle.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);

            var motorcycleManufacturerPropertyTargetSet = vehicles.FindNavigationTarget(motorcycleManufacturerProperty);
            Assert.NotNull(motorcycleManufacturerPropertyTargetSet);
            Assert.Equal("manufacturers", motorcycleManufacturerPropertyTargetSet.Name);

            IEdmNavigationPropertyBinding binding = Assert.Single(vehicles.FindNavigationPropertyBindings(motorcycleManufacturerProperty));
            Assert.Same(motorcycleManufacturerPropertyTargetSet, binding.Target);
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle/Manufacturer", binding.Path.Path);
        }

        [Fact]
        public void CanConfigureRequiredBinding_For_NavigationPropertiesInDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            builder
                .EntitySet<Vehicle>("vehicles")
                .HasRequiredBinding((Motorcycle m) => m.Manufacturer, "manufacturers");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var vehicles = model.EntityContainer.FindEntitySet("vehicles");
            Assert.NotNull(vehicles);

            var motorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            var motorcycleManufacturerProperty = motorcycle.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: false, multiplicity: EdmMultiplicity.One);

            var motorcycleManufacturerPropertyTargetSet = vehicles.FindNavigationTarget(motorcycleManufacturerProperty);
            Assert.NotNull(motorcycleManufacturerPropertyTargetSet);
            Assert.Equal("manufacturers", motorcycleManufacturerPropertyTargetSet.Name);

            IEdmNavigationPropertyBinding binding = Assert.Single(vehicles.FindNavigationPropertyBindings(motorcycleManufacturerProperty));
            Assert.Same(motorcycleManufacturerPropertyTargetSet, binding.Target);
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle/Manufacturer", binding.Path.Path);
        }

        [Fact]
        public void CanConfigureManyBinding_For_NavigationPropertiesInDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            builder
                .EntitySet<Vehicle>("vehicles")
                .HasManyBinding((Motorcycle m) => m.Manufacturers, "manufacturers");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var vehicles = model.EntityContainer.FindEntitySet("vehicles");
            Assert.NotNull(vehicles);

            var motorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            var motorcycleManufacturerProperty = motorcycle.AssertHasNavigationProperty(model, "Manufacturers", typeof(MotorcycleManufacturer), isNullable: false, multiplicity: EdmMultiplicity.Many);

            var motorcycleManufacturerPropertyTargetSet = vehicles.FindNavigationTarget(motorcycleManufacturerProperty);
            Assert.NotNull(motorcycleManufacturerPropertyTargetSet);
            Assert.Equal("manufacturers", motorcycleManufacturerPropertyTargetSet.Name);

            IEdmNavigationPropertyBinding binding = Assert.Single(vehicles.FindNavigationPropertyBindings(motorcycleManufacturerProperty));
            Assert.Same(motorcycleManufacturerPropertyTargetSet, binding.Target);
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle/Manufacturers", binding.Path.Path);
        }

        [Fact]
        public void CanConfigureLinks_For_NavigationPropertiesInDerivedType()
        {
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();

            var vehiclesSet = builder.EntitySet<Vehicle>("vehicles");

            vehiclesSet.HasNavigationPropertyLink(
                vehiclesSet.HasOptionalBinding((Motorcycle m) => m.Manufacturer, "manufacturers").NavigationProperty,
                (ctxt, property) =>
                    new Uri(String.Format("http://localhost/vehicles/{0}/{1}/{2}",
                        ctxt.GetPropertyValue("Model"), ctxt.GetPropertyValue("Name"), property.Name)), followsConventions: false);

            IEdmModel model = builder.GetEdmModel();
            var vehicles = model.EntityContainer.FindEntitySet("vehicles");
            var motorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            var motorcycleManufacturerProperty =
                motorcycle.AssertHasNavigationProperty(
                model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);

            var serializerContext = new ODataSerializerContext { Model = model, NavigationSource = vehicles };
            var entityContext = new ResourceContext(serializerContext, motorcycle.AsReference(), new Motorcycle { Name = "Motorcycle1", Model = 2009 });

            Uri link = model.GetNavigationSourceLinkBuilder(vehicles).BuildNavigationLink(entityContext, motorcycleManufacturerProperty, ODataMetadataLevel.MinimalMetadata);

            Assert.Equal("http://localhost/vehicles/2009/Motorcycle1/Manufacturer", link.AbsoluteUri);
        }

        [Fact]
        public void CannotBindNavigationPropertyAutmatically_WhenMultipleEntitySetsOfPropertyType_Exist()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            builder.EntitySet<Motorcycle>("motorcycles1").HasRequiredBinding(m => m.Manufacturer, "NorthWestMotorcycleManufacturers");
            builder.EntitySet<Motorcycle>("motorcycles2");
            builder.EntitySet<MotorcycleManufacturer>("NorthWestMotorcycleManufacturers");
            builder.EntitySet<MotorcycleManufacturer>("SouthWestMotorcycleManufacturers");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntitySet motorcycles1 = model.EntityContainer.FindEntitySet("motorcycles1");
            Assert.NotNull(motorcycles1);

            var motorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            var manufacturerNav = motorcycle.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: false, multiplicity: EdmMultiplicity.One);
            var bindings = motorcycles1.FindNavigationPropertyBindings(manufacturerNav);
            IEdmNavigationPropertyBinding binding = Assert.Single(bindings);
            Assert.Equal("Manufacturer", binding.NavigationProperty.Name);
            Assert.Equal("NorthWestMotorcycleManufacturers", binding.Target.Name);
            Assert.Equal("Manufacturer", binding.Path.Path);

            IEdmEntitySet motorcycles2 = model.EntityContainer.FindEntitySet("motorcycles2");
            Assert.Null(motorcycles2.FindNavigationPropertyBindings(manufacturerNav));
        }

        [Fact]
        public void CreateEdmModelWithEntitySetFromAbstractEntityTypeWithoutKey_Throws()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<Customer>().Abstract().Property(c => c.CustomerId);
            builder.EntitySet<Customer>("Customers");

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => builder.GetEdmModel(),
                "The entity set 'Customers' is based on type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Customer' that has no keys defined.");
        }

        [Fact]
        public void CanConfigureSingleProperty_MultipleBindingPath_For_NavigationProperties_WithComplex()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            builder
                .EntitySet<BindingCustomer>("Customers")
                .Binding
                .HasSinglePath(c => c.Location)
                .HasRequiredBinding(a => a.City, "Cities");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var customers = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers);

            // "BindingCustomer" entity type
            var customer = model.AssertHasEntityType(typeof(BindingCustomer));

            Assert.Empty(customer.NavigationProperties());
            IEdmProperty locationProperty = Assert.Single(customer.Properties());

            Assert.Equal("Location", locationProperty.Name);
            Assert.Equal(EdmPropertyKind.Structural, locationProperty.PropertyKind);

            // "BindingAddress" complex type
            var address = model.AssertHasComplexType(typeof(BindingAddress));
            var cityProperty = address.AssertHasNavigationProperty(model, "City", typeof(BindingCity), isNullable: false, multiplicity: EdmMultiplicity.One);
            var bindings = customers.FindNavigationPropertyBindings(cityProperty);
            IEdmNavigationPropertyBinding binding = Assert.Single(bindings);
            Assert.Equal("City", binding.NavigationProperty.Name);
            Assert.Equal("Cities", binding.Target.Name);
            Assert.Equal("Location/City", binding.Path.Path);

            IEdmNavigationSource navSource = customers.FindNavigationTarget(cityProperty, binding.Path);
            Assert.Same(navSource, binding.Target);

            // "BindingCity" entity type
            model.AssertHasEntityType(typeof(BindingCity));
        }

        [Fact]
        public void CanConfigureSingleProperty_MultipleBindingPath_For_NavigationProperties_WithComplex_Multiple()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            builder
                .EntitySet<BindingCustomer>("Customers")
                .Binding
                .HasSinglePath(c => c.Location)
                .HasRequiredBinding(a => a.City, "Cities_A");

            builder
                .EntitySet<BindingCustomer>("Customers")
                .Binding
                .HasSinglePath(c => c.Address)
                .HasRequiredBinding(a => a.City, "Cities_B");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var customers = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers);

            // "BindingCustomer" entity type
            var customer = model.AssertHasEntityType(typeof(BindingCustomer));
            Assert.Empty(customer.NavigationProperties());

            // "BindingAddress" complex type
            var address = model.AssertHasComplexType(typeof(BindingAddress));
            var cityProperty = address.AssertHasNavigationProperty(model, "City", typeof(BindingCity), isNullable: false, multiplicity: EdmMultiplicity.One);
            var bindings = customers.FindNavigationPropertyBindings(cityProperty).ToList();
            Assert.Equal(2, bindings.Count());

            Assert.Equal("City, City", String.Join(", ", bindings.Select(e => e.NavigationProperty.Name)));

            Assert.NotNull(bindings.SingleOrDefault(c => c.Target.Name == "Cities_A"));
            Assert.NotNull(bindings.SingleOrDefault(c => c.Target.Name == "Cities_B"));

            Assert.NotNull(bindings.SingleOrDefault(c => c.Path.Path == "Location/City"));
            Assert.NotNull(bindings.SingleOrDefault(c => c.Path.Path == "Address/City"));

            // "BindingCity" entity type
            model.AssertHasEntityType(typeof(BindingCity));
        }

        [Fact]
        public void CanConfigureManyProperty_MultipleBindingPath_For_NavigationProperties_WithComplex()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            builder
                .EntitySet<BindingCustomer>("Customers")
                .Binding
                .HasManyPath(c => c.Addresses)
                .HasManyBinding(a => a.Cities, "Cities");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var customers = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers);

            // "BindingCustomer" entity type
            var customer = model.AssertHasEntityType(typeof(BindingCustomer));

            Assert.Empty(customer.NavigationProperties());
            IEdmProperty addressesProperty = Assert.Single(customer.Properties());

            Assert.Equal("Addresses", addressesProperty.Name);
            Assert.Equal(EdmPropertyKind.Structural, addressesProperty.PropertyKind);
            Assert.True(addressesProperty.Type.IsCollection());

            // "BindingAddress" complex type
            var address = model.AssertHasComplexType(typeof(BindingAddress));
            var citiesProperty = address.AssertHasNavigationProperty(model, "Cities", typeof(BindingCity), isNullable: true, multiplicity: EdmMultiplicity.Many);
            var bindings = customers.FindNavigationPropertyBindings(citiesProperty);
            IEdmNavigationPropertyBinding binding = Assert.Single(bindings);
            Assert.Equal("Cities", binding.NavigationProperty.Name);
            Assert.Equal("Cities", binding.Target.Name);
            Assert.Equal("Addresses/Cities", binding.Path.Path);

            IEdmNavigationSource navSource = customers.FindNavigationTarget(citiesProperty, binding.Path);
            Assert.Same(navSource, binding.Target);

            // "BindingCity" entity type
            model.AssertHasEntityType(typeof(BindingCity));
        }

        [Fact]
        public void CanConfigureBindingPath_NavigationProperties_WithDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();

            var bindingConfiguration = builder
                .EntitySet<BindingCustomer>("Customers")
                .Binding
                .HasSinglePath((BindingVipCustomer v) => v.VipLocation);

            bindingConfiguration.HasOptionalBinding((BindingUsAddress u) => u.UsCity, "Cities_A");
            bindingConfiguration.HasManyBinding((BindingUsAddress u) => u.UsCities, "Cities_B");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var customers = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers);

            // "BindingVipCustomer" entity type
            var vipCustomer = model.AssertHasEntityType(typeof(BindingVipCustomer), typeof(BindingCustomer));

            Assert.Empty(vipCustomer.NavigationProperties());
            IEdmProperty vipLocationProperty = Assert.Single(vipCustomer.Properties());

            Assert.Equal("VipLocation", vipLocationProperty.Name);
            Assert.Equal(EdmPropertyKind.Structural, vipLocationProperty.PropertyKind);
            Assert.False(vipLocationProperty.Type.IsCollection());

            // "BindingUsAddress" complex type
            var usAddress = model.AssertHasComplexType(typeof(BindingUsAddress), typeof(BindingAddress));
            var cityProperty = usAddress.AssertHasNavigationProperty(model, "UsCity", typeof(BindingCity), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);
            var bindings = customers.FindNavigationPropertyBindings(cityProperty);
            IEdmNavigationPropertyBinding binding = Assert.Single(bindings);
            Assert.Equal("UsCity", binding.NavigationProperty.Name);
            Assert.Equal("Cities_A", binding.Target.Name);
            Assert.Equal("Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipLocation/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/UsCity", binding.Path.Path);

            IEdmNavigationSource navSource = customers.FindNavigationTarget(cityProperty, binding.Path);
            Assert.Same(navSource, binding.Target);

            var citiesProperty = usAddress.AssertHasNavigationProperty(model, "UsCities", typeof(BindingCity), isNullable: true, multiplicity: EdmMultiplicity.Many);
            bindings = customers.FindNavigationPropertyBindings(citiesProperty);
            binding = Assert.Single(bindings);
            Assert.Equal("UsCities", binding.NavigationProperty.Name);
            Assert.Equal("Cities_B", binding.Target.Name);
            Assert.Equal("Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipLocation/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/UsCities", binding.Path.Path);

            navSource = customers.FindNavigationTarget(citiesProperty, binding.Path);
            Assert.Same(navSource, binding.Target);

            // "BindingCity" entity type
            model.AssertHasEntityType(typeof(BindingCity));
        }

        [Fact]
        public void CanConfigureDerived()
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();

            builder.EntityType<Animal>().DerivesFrom<Creature>();
            builder.EntityType<Human>().DerivesFrom<Creature>();
            var creaturesConfiguration = builder.EntitySet<Creature>("Creatures");


            creaturesConfiguration.HasDerivedTypeConstraint<Animal>().HasDerivedTypeConstraint<Human>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            
        }
    }
}
