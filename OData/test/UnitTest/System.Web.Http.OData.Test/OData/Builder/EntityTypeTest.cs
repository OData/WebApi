﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Builder
{
    public class EntityTypeTest
    {
        [Fact]
        public void CreateEntityType()
        {
            var builder = new ODataModelBuilder().Add_Customer_EntityType();
            var model = builder.GetServiceModel();
            var customerType = model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(t => t.Name == "Customer");
            Assert.NotNull(customerType);
            Assert.Equal("Customer", customerType.Name);
            Assert.Equal(typeof(Customer).Namespace, customerType.Namespace);
            Assert.Equal("CustomerId", customerType.DeclaredKey.Single().Name);
            Assert.Equal(5, customerType.DeclaredProperties.Count());
        }

        [Fact]
        public void CreateEntityTypeWithRelationship()
        {
            var builder = new ODataModelBuilder().Add_Customer_EntityType().Add_Order_EntityType().Add_OrderCustomer_Relationship();

            var model = builder.GetServiceModel();
            var orderType = model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(t => t.Name == "Order");
            Assert.NotNull(orderType);
            Assert.Equal("Order", orderType.Name);
            Assert.Equal(typeof(Order).Namespace, orderType.Namespace);
            Assert.Equal("OrderId", orderType.DeclaredKey.Single().Name);
            Assert.Equal(5, orderType.DeclaredProperties.Count());
            Assert.Equal(1, orderType.NavigationProperties().Count());
            var deliveryDateProperty = orderType.DeclaredProperties.Single(dp => dp.Name == "DeliveryDate");
            Assert.NotNull(deliveryDateProperty);
            Assert.True(deliveryDateProperty.Type.IsNullable);

            Assert.Equal("Customer", orderType.NavigationProperties().First().Name);
            Assert.Equal("Customer", orderType.NavigationProperties().First().ToEntityType().Name);

            var customerType = model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault(t => t.Name == "Customer");
            Assert.NotNull(customerType);
            Assert.Equal("Customer", customerType.Name);
            Assert.Equal(typeof(Customer).Namespace, customerType.Namespace);
            Assert.Equal("CustomerId", customerType.DeclaredKey.Single().Name);
            Assert.Equal(5, customerType.DeclaredProperties.Count());
        }

        [Fact]
        public void CanCreateEntityWithCompoundKey()
        {
            var builder = new ODataModelBuilder();
            var customer = builder.Entity<Customer>();
            customer.HasKey(c => new { c.CustomerId, c.Name });
            customer.Property(c => c.SharePrice);
            customer.Property(c => c.ShareSymbol);
            customer.Property(c => c.Website);

            var model = builder.GetServiceModel();
            var customerType = model.FindType(typeof(Customer).FullName) as IEdmEntityType;
            Assert.Equal(5, customerType.Properties().Count());
            Assert.Equal(2, customerType.DeclaredKey.Count());
            Assert.NotNull(customerType.DeclaredKey.SingleOrDefault(k => k.Name == "CustomerId"));
            Assert.NotNull(customerType.DeclaredKey.SingleOrDefault(k => k.Name == "Name"));
        }

        [Fact]
        public void CanCreateEntityWithCollectionProperties()
        {
            var builder = new ODataModelBuilder();
            var customer = builder.Entity<Customer>();
            customer.HasKey(c => c.CustomerId);
            customer.CollectionProperty(c => c.Aliases);
            customer.CollectionProperty(c => c.Addresses);


            var aliasesProperty = customer.Properties.OfType<CollectionPropertyConfiguration>().SingleOrDefault(p => p.Name == "Aliases");
            var addressesProperty = customer.Properties.OfType<CollectionPropertyConfiguration>().SingleOrDefault(p => p.Name == "Addresses");

            Assert.Equal(3, customer.Properties.Count());
            Assert.Equal(2, customer.Properties.OfType<CollectionPropertyConfiguration>().Count());
            Assert.NotNull(aliasesProperty);
            Assert.Equal(typeof(string), aliasesProperty.ElementType);
            Assert.NotNull(addressesProperty);
            Assert.Equal(typeof(Address), addressesProperty.ElementType);

            Assert.Equal(2, builder.StructuralTypes.Count());
            var addressType = builder.StructuralTypes.Skip(1).FirstOrDefault();
            Assert.NotNull(addressType);
            Assert.Equal(EdmTypeKind.Complex, addressType.Kind);
            Assert.Equal(typeof(Address).FullName, addressType.FullName);

            var model = builder.GetServiceModel();
            var edmCustomerType = model.FindType(typeof(Customer).FullName) as IEdmEntityType;
            var edmAddressType = model.FindType(typeof(Address).FullName) as IEdmComplexType;
        }

        [Fact]
        public void SimpleCollections_Are_NotNullable_ByDefault()
        {
            var builder = new ODataModelBuilder();

            var property =
                builder
                .Entity<Customer>()
                .CollectionProperty(c => c.Aliases);

            var model = builder.GetEdmModel();

            Assert.False(property.OptionalProperty);
            var edmCustomer = model.AssertHasEntityType(typeof(Customer));
            Assert.False(edmCustomer.FindProperty("Aliases").Type.IsNullable);
        }

        [Fact]
        public void ComplexCollections_Are_NotNullable_ByDefault()
        {
            var builder = new ODataModelBuilder();

            var property =
                builder
                .Entity<Customer>()
                .CollectionProperty(c => c.Addresses);

            var model = builder.GetEdmModel();

            Assert.False(property.OptionalProperty);
            var edmCustomer = model.AssertHasEntityType(typeof(Customer));
            Assert.False(edmCustomer.FindProperty("Addresses").Type.IsNullable);
        }

        [Fact]
        public void CanCreateAbstractEntityType()
        {
            var builder = new ODataModelBuilder();
            builder
                .Entity<Vehicle>()
                .Abstract();

            var model = builder.GetEdmModel();
            var edmCustomerType = model.FindType(typeof(Vehicle).FullName) as IEdmEntityType;
            Assert.True(edmCustomerType != null);
            Assert.True(edmCustomerType.IsAbstract);
        }

        [Fact]
        public void CanCreateDerivedtypes()
        {
            var builder = new ODataModelBuilder();

            builder
                .Entity<Vehicle>()
                .Abstract()
                .HasKey(v => v.Model)
                .HasKey(v => v.Name)
                .Property(v => v.WheelCount);

            builder
                .Entity<Motorcycle>()
                .DerivesFrom<Vehicle>()
                .Property(m => m.CanDoAWheelie);

            builder
                .Entity<Car>()
                .DerivesFrom<Vehicle>()
                .Property(c => c.SeatingCapacity);

            var model = builder.GetEdmModel();

            var edmVehicle = model.AssertHasEntityType(typeof(Vehicle));
            Assert.Null(edmVehicle.BaseEntityType());
            Assert.Equal(2, edmVehicle.Key().Count());
            edmVehicle.AssertHasKey(model, "Model", EdmPrimitiveTypeKind.Int32);
            edmVehicle.AssertHasKey(model, "Name", EdmPrimitiveTypeKind.String);
            Assert.Equal(3, edmVehicle.Properties().Count());
            edmVehicle.AssertHasPrimitiveProperty(model, "WheelCount", EdmPrimitiveTypeKind.Int32, isNullable: false);

            var edmMotorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            Assert.Equal(edmVehicle, edmMotorcycle.BaseEntityType());
            Assert.Equal(2, edmMotorcycle.Key().Count());
            edmMotorcycle.AssertHasKey(model, "Model", EdmPrimitiveTypeKind.Int32);
            edmMotorcycle.AssertHasKey(model, "Name", EdmPrimitiveTypeKind.String);
            Assert.Equal(4, edmMotorcycle.Properties().Count());
            edmMotorcycle.AssertHasPrimitiveProperty(model, "WheelCount", EdmPrimitiveTypeKind.Int32, isNullable: false);
            edmMotorcycle.AssertHasPrimitiveProperty(model, "CanDoAWheelie", EdmPrimitiveTypeKind.Boolean, isNullable: false);

            var edmCar = model.AssertHasEntityType(typeof(Car));
            Assert.Equal(edmVehicle, edmMotorcycle.BaseEntityType());
            Assert.Equal(2, edmCar.Key().Count());
            edmCar.AssertHasKey(model, "Model", EdmPrimitiveTypeKind.Int32);
            edmCar.AssertHasKey(model, "Name", EdmPrimitiveTypeKind.String);
            Assert.Equal(4, edmCar.Properties().Count());
            edmCar.AssertHasPrimitiveProperty(model, "WheelCount", EdmPrimitiveTypeKind.Int32, isNullable: false);
            edmCar.AssertHasPrimitiveProperty(model, "SeatingCapacity", EdmPrimitiveTypeKind.Int32, isNullable: false);
        }

        [Fact]
        public void CanCreateDerivedTypesInAnyOrder()
        {
            var builder = new ODataModelBuilder();

            builder
                .Entity<SportBike>()
                .DerivesFrom<Motorcycle>();

            builder
                .Entity<Car>()
                .DerivesFrom<Vehicle>();

            builder
                .Entity<Vehicle>();

            builder
                .Entity<Motorcycle>()
                .DerivesFrom<Vehicle>();

            IEdmModel model = builder.GetEdmModel();

            model.AssertHasEntityType(typeof(Vehicle));
            model.AssertHasEntityType(typeof(Car), typeof(Vehicle));
            model.AssertHasEntityType(typeof(Motorcycle), typeof(Vehicle));
            model.AssertHasEntityType(typeof(SportBike), typeof(Motorcycle));
        }

        [Fact]
        public void HasKeyOnDerivedTypes_Throws()
        {
            var builder = new ODataModelBuilder();

            Assert.Throws<InvalidOperationException>(
                () => builder
                        .Entity<Motorcycle>()
                        .DerivesFrom<Vehicle>()
                        .HasKey(m => m.ID),
                "Cannot define keys on type 'System.Web.Http.OData.Builder.TestModels.Motorcycle' deriving from 'System.Web.Http.OData.Builder.TestModels.Vehicle'. Only the root type in the entity inheritance hierarchy can contain keys.");
        }

        [Fact]
        public void CanDefinePropertyOnDerivedType_NotPresentInBaseEdmType_ButPresentInBaseClrType()
        {
            var builder = new ODataModelBuilder();

            builder
                .Entity<Motorcycle>()
                .DerivesFrom<Vehicle>()
                .Property(m => m.Model);

            var model = builder.GetEdmModel();

            var edmVehicle = model.AssertHasEntityType(typeof(Vehicle));
            Assert.Null(edmVehicle.BaseEntityType());
            Assert.Equal(0, edmVehicle.Properties().Count());

            var edmMotorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            Assert.Equal(edmVehicle, edmMotorcycle.BaseEntityType());
            Assert.Equal(1, edmMotorcycle.Properties().Count());
            edmMotorcycle.AssertHasPrimitiveProperty(model, "Model", EdmPrimitiveTypeKind.Int32, isNullable: false);
        }

        [Fact]
        public void RedefiningBaseTypeProperty_Throws()
        {
            var builder = new ODataModelBuilder();

            builder
                .Entity<Vehicle>()
                .Property(v => v.WheelCount);

            Assert.ThrowsArgument(
                () => builder
                        .Entity<Motorcycle>()
                        .DerivesFrom<Vehicle>()
                        .Property(m => m.WheelCount),
                "propertyInfo",
                "Cannot redefine property 'WheelCount' already defined on the base type 'System.Web.Http.OData.Builder.TestModels.Vehicle'.");
        }

        [Fact]
        public void DefiningPropertyOnBaseTypeAlreadyPresentInDerivedType_Throws()
        {
            var builder = new ODataModelBuilder();

            builder
                .Entity<Motorcycle>()
                .DerivesFrom<Vehicle>()
                .Property(m => m.Model);

            Assert.ThrowsArgument(
                () => builder
                        .Entity<Vehicle>()
                        .Property(v => v.Model),
                "propertyInfo",
                "Cannot define property 'Model' in the base entity type 'System.Web.Http.OData.Builder.TestModels.Vehicle' as the derived type 'System.Web.Http.OData.Builder.TestModels.Motorcycle' already defines it.");
        }

        [Fact]
        public void DerivesFrom_Throws_IfDerivedTypeHasKeys()
        {
            var builder = new ODataModelBuilder();

            Assert.Throws<InvalidOperationException>(
            () => builder
                    .Entity<Motorcycle>()
                    .HasKey(m => m.Model)
                    .DerivesFrom<Vehicle>(),
            "Cannot define keys on type 'System.Web.Http.OData.Builder.TestModels.Motorcycle' deriving from 'System.Web.Http.OData.Builder.TestModels.Vehicle'. Only the root type in the entity inheritance hierarchy can contain keys.");
        }

        [Fact]
        public void DerivesFrom_Throws_IfDerivedTypeDoesNotDeriveFromBaseType()
        {
            var builder = new ODataModelBuilder();

            Assert.ThrowsArgument(
                () => builder
                        .Entity<string>()
                        .DerivesFrom<Vehicle>(),
                "baseType",
                "'System.String' does not inherit from 'System.Web.Http.OData.Builder.TestModels.Vehicle'.");
        }

        [Fact]
        public void DerivesFrom_Throws_WhenSettingTheBaseType_IfDuplicatePropertyInBaseType()
        {
            var builder = new ODataModelBuilder();

            builder
                .Entity<Vehicle>()
                .Property(v => v.Model);

            var motorcycle = builder
                            .Entity<Motorcycle>();
            motorcycle.Property(m => m.Model);

            Assert.ThrowsArgument(
                () => motorcycle.DerivesFrom<Vehicle>(),
                "propertyInfo",
                "Cannot redefine property 'Model' already defined on the base type 'System.Web.Http.OData.Builder.TestModels.Vehicle'.");
        }

        [Fact]
        public void DerivesFrom_Throws_WhenSettingTheBaseType_IfDuplicatePropertyInDerivedType()
        {
            var builder = new ODataModelBuilder();

            builder
                .Entity<Vehicle>()
                .Property(v => v.Model);

            builder
                .Entity<SportBike>()
                .DerivesFrom<Motorcycle>()
                .Property(c => c.Model);

            Assert.ThrowsArgument(
                () => builder
                    .Entity<Motorcycle>()
                    .DerivesFrom<Vehicle>(),
                "propertyInfo",
                "Cannot define property 'Model' in the base entity type 'System.Web.Http.OData.Builder.TestModels.Motorcycle' as the derived type 'System.Web.Http.OData.Builder.TestModels.SportBike' already defines it.");
        }

        [Fact]
        public void CannotDeriveFromItself()
        {
            var builder = new ODataModelBuilder();

            Assert.ThrowsArgument(
            () => builder
                .Entity<Vehicle>()
                .DerivesFrom<Vehicle>(),
            "baseType",
            "'System.Web.Http.OData.Builder.TestModels.Vehicle' does not inherit from 'System.Web.Http.OData.Builder.TestModels.Vehicle'.");
        }

        [Fact]
        public void DerivesFrom_SetsBaseType()
        {
            var builder = new ODataModelBuilder();
            var motorcycle = builder.Entity<Motorcycle>();

            motorcycle.DerivesFrom<Vehicle>();

            Assert.NotNull(motorcycle.BaseType);
            Assert.Equal(typeof(Vehicle), motorcycle.BaseType.ClrType);
        }

        [Fact]
        public void CanDeriveFromNull()
        {
            var builder = new ODataModelBuilder();
            var motorcycle = builder.Entity<Motorcycle>();

            motorcycle.DerivesFromNothing();
            Assert.Null(motorcycle.BaseType);
        }

        [Fact]
        public void BaseTypeConfigured_IsFalseByDefault()
        {
            var builder = new ODataModelBuilder();
            var motorcycle = builder.AddEntity(typeof(Motorcycle));

            Assert.False(motorcycle.BaseTypeConfigured);
        }

        [Fact]
        public void SettingBaseType_UpdatesBaseTypeConfigured()
        {
            var builder = new ODataModelBuilder();
            var motorcycle = builder.AddEntity(typeof(Motorcycle));
            var vehicle = builder.AddEntity(typeof(Vehicle));

            motorcycle.DerivesFrom(vehicle);

            Assert.True(motorcycle.BaseTypeConfigured);
        }

        [Fact]
        public void SettingBaseTypeToNull_AlsoUpdatesBaseTypeConfigured()
        {
            var builder = new ODataModelBuilder();
            var motorcycle = builder.AddEntity(typeof(Motorcycle));
            var vehicle = builder.AddEntity(typeof(Vehicle));

            motorcycle.DerivesFromNothing();

            Assert.True(motorcycle.BaseTypeConfigured);
        }

        [Fact]
        public void RemoveKey_ThrowsArgumentNull()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var motorcycle = builder.AddEntity(typeof(Motorcycle));

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => motorcycle.RemoveKey(keyProperty: null),
                "keyProperty");
        }

        [Fact]
        public void RemoveKey_Removes_KeyProperty()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var motorcycle = builder.AddEntity(typeof(Motorcycle));
            PrimitivePropertyConfiguration modelProperty = motorcycle.AddProperty(typeof(Motorcycle).GetProperty("Model"));
            motorcycle.HasKey(typeof(Motorcycle).GetProperty("Model"));
            Assert.Equal(new[] { modelProperty }, motorcycle.Keys);

            // Act
            motorcycle.RemoveKey(modelProperty);

            // Assert
            Assert.Empty(motorcycle.Keys);
        }
    }
}
