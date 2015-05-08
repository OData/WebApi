﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.Builder.TestModels;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Formatter.Serialization;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Builder
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
            Assert.Equal("Customer", customers.ElementType.Name);
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
            var customerOrders = customers.NavigationTargets.First(nt => nt.NavigationProperty.Name == "Orders");
            Assert.NotNull(customerOrders);
            Assert.Equal("Orders", customerOrders.TargetEntitySet.Name);
        }

        [Fact]
        public void CanAddBinding_For_DerivedNavigationProperty()
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            var vehicle = builder.AddEntity(typeof(Vehicle));
            var motorcycle = builder.AddEntity(typeof(Motorcycle)).DerivesFrom(vehicle);
            var manufacturer = builder.AddEntity(typeof(MotorcycleManufacturer));
            var manufacturers = builder.AddEntitySet("manufacturers", manufacturer);
            var navProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.One);

            var vehicles = builder.AddEntitySet("vehicles", vehicle);
            vehicles.AddBinding(navProperty, manufacturers);

            IEdmModel model = builder.GetEdmModel();
            var motorcycleEdmType = model.AssertHasEntityType(typeof(Motorcycle));
            var edmNavProperty = motorcycleEdmType.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: false, multiplicity: EdmMultiplicity.One);

            Assert.Equal(
                "manufacturers",
                model.EntityContainers().Single().FindEntitySet("vehicles").FindNavigationTarget(edmNavProperty).Name);
        }

        [Fact]
        public void CanAddNavigationLink_For_DerivedNavigationProperty()
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            var vehicle = builder.AddEntity(typeof(Vehicle));
            var motorcycle = builder.AddEntity(typeof(Motorcycle)).DerivesFrom(vehicle);
            var manufacturer = builder.AddEntity(typeof(MotorcycleManufacturer));
            var manufacturers = builder.AddEntitySet("manufacturers", manufacturer);
            var navProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.One);

            var vehicles = builder.AddEntitySet("vehicles", vehicle);
            var binding = vehicles.AddBinding(navProperty, manufacturers);
            vehicles.HasNavigationPropertyLink(navProperty, new NavigationLinkBuilder((ctxt, property) => new Uri("http://works/"), followsConventions: false));

            IEdmModel model = builder.GetEdmModel();
            var motorcycleEdmType = model.AssertHasEntityType(typeof(Motorcycle));
            var edmNavProperty = motorcycleEdmType.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: false, multiplicity: EdmMultiplicity.One);
            var vehiclesEdmSet = model.EntityContainers().Single().FindEntitySet("vehicles");

            Assert.NotNull(model.GetEntitySetLinkBuilder(vehiclesEdmSet));
            Assert.Equal(
                "http://works/",
                model.GetEntitySetLinkBuilder(vehiclesEdmSet).BuildNavigationLink(new EntityInstanceContext(), edmNavProperty, ODataMetadataLevel.Default).AbsoluteUri);
        }

        [Fact]
        public void AddBinding_For_NavigationPropertyInHierarchy_Throws()
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            var vehicle = builder.AddEntity(typeof(Vehicle));
            var motorcycle = builder.AddEntity(typeof(Motorcycle));
            var manufacturer = builder.AddEntity(typeof(MotorcycleManufacturer));
            var manufacturers = builder.AddEntitySet("manufacturers", manufacturer);

            var navProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.One);

            var vehicles = builder.AddEntitySet("vehicles", vehicle);

            Assert.ThrowsArgument(
                () => vehicles.AddBinding(navProperty, manufacturers),
                "navigationConfiguration",
                "The declaring entity type 'System.Web.Http.OData.Builder.TestModels.Motorcycle' of the given navigation property is not a part of the entity type 'System.Web.Http.OData.Builder.TestModels.Vehicle' hierarchy of the entity set 'vehicles'.");
        }

        [Fact]
        public void AddNavigationLink_For_NavigationPropertyInHierarchy_Throws()
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            var vehicle = builder.AddEntity(typeof(Vehicle));
            var motorcycle = builder.AddEntity(typeof(Motorcycle));
            var manufacturer = builder.AddEntity(typeof(MotorcycleManufacturer));
            var manufacturers = builder.AddEntitySet("manufacturers", manufacturer);

            var navProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.One);

            var vehicles = builder.AddEntitySet("vehicles", vehicle);

            Assert.ThrowsArgument(
                () => vehicles.HasNavigationPropertyLink(navProperty, new NavigationLinkBuilder((ctxt, property) => new Uri("http://works/"), followsConventions: false)),
                "navigationProperty",
                "The declaring entity type 'System.Web.Http.OData.Builder.TestModels.Motorcycle' of the given navigation property is not a part of the entity type 'System.Web.Http.OData.Builder.TestModels.Vehicle' hierarchy of the entity set 'vehicles'.");
        }

        [Fact]
        public void CanConfigureOptionalBinding_For_NavigationPropertiesInDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder
                .EntitySet<Vehicle>("vehicles")
                .HasOptionalBinding((Motorcycle m) => m.Manufacturer, "manufacturers");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var vehicles = model.EntityContainers().Single().FindEntitySet("vehicles");
            Assert.NotNull(vehicles);

            var motorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            var motorcycleManufacturerProperty = motorcycle.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);

            var motorcycleManufacturerPropertyTargetSet = vehicles.FindNavigationTarget(motorcycleManufacturerProperty);
            Assert.NotNull(motorcycleManufacturerPropertyTargetSet);
            Assert.Equal("manufacturers", motorcycleManufacturerPropertyTargetSet.Name);
        }

        [Fact]
        public void CanConfigureRequiredBinding_For_NavigationPropertiesInDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder
                .EntitySet<Vehicle>("vehicles")
                .HasRequiredBinding((Motorcycle m) => m.Manufacturer, "manufacturers");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var vehicles = model.EntityContainers().Single().FindEntitySet("vehicles");
            Assert.NotNull(vehicles);

            var motorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            var motorcycleManufacturerProperty = motorcycle.AssertHasNavigationProperty(model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: false, multiplicity: EdmMultiplicity.One);

            var motorcycleManufacturerPropertyTargetSet = vehicles.FindNavigationTarget(motorcycleManufacturerProperty);
            Assert.NotNull(motorcycleManufacturerPropertyTargetSet);
            Assert.Equal("manufacturers", motorcycleManufacturerPropertyTargetSet.Name);
        }

        [Fact]
        public void CanConfigureManyBinding_For_NavigationPropertiesInDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder
                .EntitySet<Vehicle>("vehicles")
                .HasManyBinding((Motorcycle m) => m.Manufacturers, "manufacturers");

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var vehicles = model.EntityContainers().Single().FindEntitySet("vehicles");
            Assert.NotNull(vehicles);

            var motorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            var motorcycleManufacturerProperty = motorcycle.AssertHasNavigationProperty(model, "Manufacturers", typeof(MotorcycleManufacturer), isNullable: false, multiplicity: EdmMultiplicity.Many);

            var motorcycleManufacturerPropertyTargetSet = vehicles.FindNavigationTarget(motorcycleManufacturerProperty);
            Assert.NotNull(motorcycleManufacturerPropertyTargetSet);
            Assert.Equal("manufacturers", motorcycleManufacturerPropertyTargetSet.Name);
        }

        [Fact]
        public void CanConfigureLinks_For_NavigationPropertiesInDerivedType()
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            var vehiclesSet = builder.EntitySet<Vehicle>("vehicles");

            vehiclesSet.HasNavigationPropertyLink(
                vehiclesSet.HasOptionalBinding((Motorcycle m) => m.Manufacturer, "manufacturers").NavigationProperty,
                (ctxt, property) =>
                    new Uri(String.Format("http://localhost/vehicles/{0}/{1}/{2}",
                        ctxt.GetPropertyValue("Model"), ctxt.GetPropertyValue("Name"), property.Name)), followsConventions: false);

            IEdmModel model = builder.GetEdmModel();
            var vehicles = model.EntityContainers().Single().FindEntitySet("vehicles");
            var motorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            var motorcycleManufacturerProperty =
                motorcycle.AssertHasNavigationProperty(
                model, "Manufacturer", typeof(MotorcycleManufacturer), isNullable: true, multiplicity: EdmMultiplicity.ZeroOrOne);

            var serializerContext = new ODataSerializerContext { Model = model, EntitySet = vehicles };
            var entityContext = new EntityInstanceContext(serializerContext, motorcycle.AsReference(), new Motorcycle { Name = "Motorcycle1", Model = 2009 });

            Uri link = model.GetEntitySetLinkBuilder(vehicles).BuildNavigationLink(entityContext, motorcycleManufacturerProperty, ODataMetadataLevel.Default);

            Assert.Equal("http://localhost/vehicles/2009/Motorcycle1/Manufacturer", link.AbsoluteUri);
        }

        [Fact]
        public void CannotBindNavigationPropertyAutmatically_WhenMultipleEntitySetsOfPropertyType_Exist()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntitySet<Motorcycle>("motorcycles1").HasRequiredBinding(m => m.Manufacturer, "NorthWestMotorcycleManufacturers");
            builder.EntitySet<Motorcycle>("motorcycles2");
            builder.EntitySet<MotorcycleManufacturer>("NorthWestMotorcycleManufacturers");
            builder.EntitySet<MotorcycleManufacturer>("SouthWestMotorcycleManufacturers");

            Assert.Throws<NotSupportedException>(
            () => builder.GetEdmModel(),
            "Cannot automatically bind the navigation property 'Manufacturer' on entity type 'System.Web.Http.OData.Builder.TestModels.Motorcycle' for the source entity set 'motorcycles2' because there are two or more matching target entity sets. " +
            "The matching entity sets are: NorthWestMotorcycleManufacturers, SouthWestMotorcycleManufacturers.");
        }
    }
}
