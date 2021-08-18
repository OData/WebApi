//-----------------------------------------------------------------------------
// <copyright file="EntityTypeTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
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
            Assert.Equal(6, customerType.DeclaredProperties.Count());
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
            Assert.Single(orderType.NavigationProperties());
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
            Assert.Equal(6, customerType.DeclaredProperties.Count());
        }

        [Fact]
        public void CanCreateEntityWithCompoundKey()
        {
            var builder = new ODataModelBuilder();
            var customer = builder.EntityType<Customer>();
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
        public void CanCreateEntityWithCompoundKey_ForDateAndTimeOfDay()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var entity = builder.EntityType<EntityTypeWithDateAndTimeOfDay>();
            entity.HasKey(e => new { e.Date, e.TimeOfDay });

            // Act
            var model = builder.GetServiceModel();

            // Assert
            var entityType =
                model.SchemaElements.OfType<IEdmEntityType>().Single(e => e.Name == "EntityTypeWithDateAndTimeOfDay");
            Assert.Equal(2, entityType.Properties().Count());
            Assert.Equal(2, entityType.DeclaredKey.Count());
            Assert.NotNull(entityType.DeclaredKey.SingleOrDefault(k => k.Name == "Date"));
            Assert.NotNull(entityType.DeclaredKey.SingleOrDefault(k => k.Name == "TimeOfDay"));
        }

        [Fact]
        public void CanCreateEntityWithEnumKey()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            var enumEntityType = builder.EntityType<EnumModel>();
            enumEntityType.HasKey(c => c.Simple);
            enumEntityType.Property(c => c.Id);

            // Act
            IEdmModel model = builder.GetServiceModel();

            // Assert
            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>()
                .FirstOrDefault(c => c.Name == "EnumModel");
            Assert.NotNull(entityType);
            Assert.Equal(2, entityType.Properties().Count());

            Assert.Single(entityType.DeclaredKey);
            IEdmStructuralProperty key = entityType.DeclaredKey.First();
            Assert.Equal(EdmTypeKind.Enum, key.Type.TypeKind());
            Assert.Equal("Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum", key.Type.Definition.FullTypeName());
        }

        [Fact]
        public void CanCreateEntityWithCompoundEnumKeys()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            var enumEntityType = builder.EntityType<EnumModel>();
            enumEntityType.HasKey(c => new { c.Simple, c.Long });

            // Act
            IEdmModel model = builder.GetServiceModel();

            // Assert
            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>()
                .FirstOrDefault(c => c.Name == "EnumModel");
            Assert.NotNull(entityType);
            Assert.Equal(2, entityType.Properties().Count());

            Assert.Equal(2, entityType.DeclaredKey.Count());
            IEdmStructuralProperty simpleKey = entityType.DeclaredKey.First(k => k.Name == "Simple");
            Assert.Equal(EdmTypeKind.Enum, simpleKey.Type.TypeKind());
            Assert.Equal("Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum", simpleKey.Type.Definition.FullTypeName());

            IEdmStructuralProperty longKey = entityType.DeclaredKey.First(k => k.Name == "Long");
            Assert.Equal(EdmTypeKind.Enum, longKey.Type.TypeKind());
            Assert.Equal("Microsoft.AspNet.OData.Test.Common.Types.LongEnum", longKey.Type.Definition.FullTypeName());
        }

        [Fact]
        public void CanCreateEntityWithPrimitiveAndEnumKey()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            var enumEntityType = builder.EntityType<EnumModel>();
            enumEntityType.HasKey(c => new { c.Simple, c.Id });

            // Act
            IEdmModel model = builder.GetServiceModel();

            // Assert
            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>()
                .FirstOrDefault(c => c.Name == "EnumModel");
            Assert.NotNull(entityType);
            Assert.Equal(2, entityType.Properties().Count());

            Assert.Equal(2, entityType.DeclaredKey.Count());
            IEdmStructuralProperty enumKey = entityType.DeclaredKey.First(k => k.Name == "Simple");
            Assert.Equal(EdmTypeKind.Enum, enumKey.Type.TypeKind());
            Assert.Equal("Microsoft.AspNet.OData.Test.Common.Types.SimpleEnum", enumKey.Type.Definition.FullTypeName());

            IEdmStructuralProperty primitiveKey = entityType.DeclaredKey.First(k => k.Name == "Id");
            Assert.Equal(EdmTypeKind.Primitive, primitiveKey.Type.TypeKind());
            Assert.Equal("Edm.Int32", primitiveKey.Type.Definition.FullTypeName());
        }

        [Fact]
        public void CanCreateEntityWithCollectionProperties()
        {
            var builder = new ODataModelBuilder();
            var customer = builder.EntityType<Customer>();
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
        public void SimpleCollections_Are_Nullable_ByDefault()
        {
            var builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();

            var property =
                builder
                .EntityType<Customer>()
                .CollectionProperty(c => c.Aliases);

            var model = builder.GetEdmModel();

            Assert.False(property.OptionalProperty);
            var edmCustomer = model.AssertHasEntityType(typeof(Customer));
            Assert.True(edmCustomer.FindProperty("Aliases").Type.IsNullable);
        }

        [Fact]
        public void ComplexCollections_Are_NotNullable_ByDefault()
        {
            var builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();

            var property =
                builder
                .EntityType<Customer>()
                .CollectionProperty(c => c.Addresses);

            var model = builder.GetEdmModel();

            Assert.False(property.OptionalProperty);
            var edmCustomer = model.AssertHasEntityType(typeof(Customer));
            Assert.False(edmCustomer.FindProperty("Addresses").Type.IsNullable);
        }

        [Fact]
        public void CanCreateAbstractEntityType()
        {
            var builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            builder
                .EntityType<Vehicle>()
                .Abstract();

            var model = builder.GetEdmModel();
            var edmCustomerType = model.FindType(typeof(Vehicle).FullName) as IEdmEntityType;
            Assert.True(edmCustomerType != null);
            Assert.True(edmCustomerType.IsAbstract);
        }

        [Fact]
        public void CanCreateMediaTypeEntity()
        {
            // Arrange
            var builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            builder
                .EntityType<Vehicle>()
                .MediaType();

            // Act
            var model = builder.GetEdmModel();
            var edmCustomerType = model.FindType(typeof(Vehicle).FullName) as IEdmEntityType;

            // Assert
            Assert.True(edmCustomerType != null);
            Assert.True(edmCustomerType.HasStream);
        }

        [Fact]
        public void CanCreateDerivedtypes()
        {
            var builder = new ODataModelBuilder();

            builder
                .EntityType<Vehicle>()
                .Abstract()
                .HasKey(v => v.Model)
                .HasKey(v => v.Name)
                .Property(v => v.WheelCount);

            builder
                .EntityType<Motorcycle>()
                .DerivesFrom<Vehicle>()
                .Property(m => m.CanDoAWheelie);

            builder
                .EntityType<Car>()
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
            var builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();

            builder
                .EntityType<SportBike>()
                .DerivesFrom<Motorcycle>();

            builder
                .EntityType<Car>()
                .DerivesFrom<Vehicle>();

            builder
                .EntityType<Vehicle>();

            builder
                .EntityType<Motorcycle>()
                .DerivesFrom<Vehicle>();

            IEdmModel model = builder.GetEdmModel();

            model.AssertHasEntityType(typeof(Vehicle));
            model.AssertHasEntityType(typeof(Car), typeof(Vehicle));
            model.AssertHasEntityType(typeof(Motorcycle), typeof(Vehicle));
            model.AssertHasEntityType(typeof(SportBike), typeof(Motorcycle));
        }

        [Fact]
        public void HasKeyOnDerivedTypes_Throws_IfCallDerivedFromFirst_ThenHasKey()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<Vehicle>().Abstract();
            builder.EntityType<Motorcycle>().DerivesFrom<Vehicle>().HasKey(m => m.ID);
            EntityTypeConfiguration<SportBike> sportBikeType = builder.EntityType<SportBike>();

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => sportBikeType.DerivesFrom<Motorcycle>().HasKey(s => s.SportBikeProperty_NotVisible),
                "Cannot define keys on type 'Microsoft.AspNet.OData.Test.Builder.TestModels.SportBike' deriving from 'Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle'. " +
                "The base type in the entity inheritance hierarchy already contains keys.");
        }

        [Fact]
        public void HasKeyOnDerivedTypes_Throws_IfCallHasKeyFirst_ThenDerivedFrom()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<Vehicle>().Abstract();
            builder.EntityType<Motorcycle>().DerivesFrom<Vehicle>().HasKey(m => m.ID);
            EntityTypeConfiguration<SportBike> sportBikeType = builder.EntityType<SportBike>();

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => sportBikeType.HasKey(s => s.SportBikeProperty_NotVisible).DerivesFrom<Motorcycle>(),
                "Cannot define keys on type 'Microsoft.AspNet.OData.Test.Builder.TestModels.SportBike' deriving from 'Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle'. " +
                "The base type in the entity inheritance hierarchy already contains keys.");
        }

        [Fact]
        public void HasKeyOnDerivedTypes_Works_ForBaseTypeWithoutKey()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            builder.EntityType<Vehicle>().Abstract();
            builder.EntityType<Motorcycle>().DerivesFrom<Vehicle>().HasKey(m => m.ID);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType vehicleType = model.AssertHasEntityType(typeof(Vehicle));
            Assert.True(vehicleType.IsAbstract);
            Assert.Null(vehicleType.DeclaredKey);

            IEdmEntityType motorCycleType = model.AssertHasEntityType(typeof(Motorcycle), typeof(Vehicle));
            Assert.False(motorCycleType.IsAbstract);
            IEdmStructuralProperty keyProperty = Assert.Single(motorCycleType.DeclaredKey);
            Assert.Equal("ID", keyProperty.Name);
        }

        [Fact]
        public void CanDefinePropertyOnDerivedType_NotPresentInBaseEdmType_ButPresentInBaseClrType()
        {
            var builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();

            builder
                .EntityType<Motorcycle>()
                .DerivesFrom<Vehicle>()
                .Property(m => m.Model);

            var model = builder.GetEdmModel();

            var edmVehicle = model.AssertHasEntityType(typeof(Vehicle));
            Assert.Null(edmVehicle.BaseEntityType());
            Assert.Empty(edmVehicle.Properties());

            var edmMotorcycle = model.AssertHasEntityType(typeof(Motorcycle));
            Assert.Equal(edmVehicle, edmMotorcycle.BaseEntityType());
            Assert.Single(edmMotorcycle.Properties());
            edmMotorcycle.AssertHasPrimitiveProperty(model, "Model", EdmPrimitiveTypeKind.Int32, isNullable: false);
        }

        [Fact]
        public void RedefiningBaseTypeProperty_Throws()
        {
            var builder = new ODataModelBuilder();

            builder
                .EntityType<Vehicle>()
                .Property(v => v.WheelCount);

            ExceptionAssert.ThrowsArgument(
                () => builder
                        .EntityType<Motorcycle>()
                        .DerivesFrom<Vehicle>()
                        .Property(m => m.WheelCount),
                "propertyInfo",
                "Cannot redefine property 'WheelCount' already defined on the base type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Vehicle'.");
        }

        [Fact]
        public void DefiningPropertyOnBaseTypeAlreadyPresentInDerivedType_Throws()
        {
            var builder = new ODataModelBuilder();

            builder
                .EntityType<Motorcycle>()
                .DerivesFrom<Vehicle>()
                .Property(m => m.Model);

            ExceptionAssert.ThrowsArgument(
                () => builder
                        .EntityType<Vehicle>()
                        .Property(v => v.Model),
                "propertyInfo",
                "Cannot define property 'Model' in the base type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Vehicle' as the derived type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle' already defines it.");
        }

        [Fact]
        public void DerivesFrom_DoesnotThrows_IfDerivedTypeHasKeys()
        {
            // Arrange
            var builder = new ODataModelBuilder();

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => builder.EntityType<Motorcycle>().HasKey(m => m.Model).DerivesFrom<Vehicle>());
        }

        [Fact]
        public void DerivesFrom_Throws_IfDerivedTypeDoesntDeriveFromBaseType()
        {
            var builder = new ODataModelBuilder();

            ExceptionAssert.ThrowsArgument(
                () => builder
                        .EntityType<string>()
                        .DerivesFrom<Vehicle>(),
                "baseType",
                "'System.String' does not inherit from 'Microsoft.AspNet.OData.Test.Builder.TestModels.Vehicle'.");
        }

        [Fact]
        public void DerivesFrom_Throws_WhenSettingTheBaseType_IfDuplicatePropertyInBaseType()
        {
            var builder = new ODataModelBuilder();

            builder
                .EntityType<Vehicle>()
                .Property(v => v.Model);

            var motorcycle = builder
                            .EntityType<Motorcycle>();
            motorcycle.Property(m => m.Model);

            ExceptionAssert.ThrowsArgument(
                () => motorcycle.DerivesFrom<Vehicle>(),
                "propertyInfo",
                "Cannot redefine property 'Model' already defined on the base type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Vehicle'.");
        }

        [Fact]
        public void DerivesFrom_Throws_WhenSettingTheBaseType_IfDuplicatePropertyInDerivedType()
        {
            var builder = new ODataModelBuilder();

            builder
                .EntityType<Vehicle>()
                .Property(v => v.Model);

            builder
                .EntityType<SportBike>()
                .DerivesFrom<Motorcycle>()
                .Property(c => c.Model);

            ExceptionAssert.ThrowsArgument(
                () => builder
                    .EntityType<Motorcycle>()
                    .DerivesFrom<Vehicle>(),
                "propertyInfo",
                "Cannot define property 'Model' in the base type 'Microsoft.AspNet.OData.Test.Builder.TestModels.Motorcycle' as the derived type 'Microsoft.AspNet.OData.Test.Builder.TestModels.SportBike' already defines it.");
        }

        [Fact]
        public void CannotDeriveFromItself()
        {
            var builder = new ODataModelBuilder();

            ExceptionAssert.ThrowsArgument(
            () => builder
                .EntityType<Vehicle>()
                .DerivesFrom<Vehicle>(),
            "baseType",
            "'Microsoft.AspNet.OData.Test.Builder.TestModels.Vehicle' does not inherit from 'Microsoft.AspNet.OData.Test.Builder.TestModels.Vehicle'.");
        }

        [Fact]
        public void DerivesFrom_SetsBaseType()
        {
            var builder = new ODataModelBuilder();
            var motorcycle = builder.EntityType<Motorcycle>();

            motorcycle.DerivesFrom<Vehicle>();

            Assert.NotNull(motorcycle.BaseType);
            Assert.Equal(typeof(Vehicle), motorcycle.BaseType.ClrType);
        }

        [Fact]
        public void CanDeriveFromNull()
        {
            var builder = new ODataModelBuilder();
            var motorcycle = builder.EntityType<Motorcycle>();

            motorcycle.DerivesFromNothing();
            Assert.Null(motorcycle.BaseType);
        }

        [Fact]
        public void BaseTypeConfigured_IsFalseByDefault()
        {
            var builder = new ODataModelBuilder();
            var motorcycle = builder.AddEntityType(typeof(Motorcycle));

            Assert.False(motorcycle.BaseTypeConfigured);
        }

        [Fact]
        public void SettingBaseType_UpdatesBaseTypeConfigured()
        {
            var builder = new ODataModelBuilder();
            var motorcycle = builder.AddEntityType(typeof(Motorcycle));
            var vehicle = builder.AddEntityType(typeof(Vehicle));

            motorcycle.DerivesFrom(vehicle);

            Assert.True(motorcycle.BaseTypeConfigured);
        }

        [Fact]
        public void SettingBaseTypeToNull_AlsoUpdatesBaseTypeConfigured()
        {
            var builder = new ODataModelBuilder();
            var motorcycle = builder.AddEntityType(typeof(Motorcycle));
            var vehicle = builder.AddEntityType(typeof(Vehicle));

            motorcycle.DerivesFromNothing();

            Assert.True(motorcycle.BaseTypeConfigured);
        }

        [Fact]
        public void RemoveKey_ThrowsArgumentNull()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var motorcycle = builder.AddEntityType(typeof(Motorcycle));

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => motorcycle.RemoveKey(keyProperty: null),
                "keyProperty");
        }

        [Fact]
        public void RemoveKey_Removes_KeyProperty()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var motorcycle = builder.AddEntityType(typeof(Motorcycle));
            PrimitivePropertyConfiguration modelProperty = motorcycle.AddProperty(typeof(Motorcycle).GetProperty("Model"));
            motorcycle.HasKey(typeof(Motorcycle).GetProperty("Model"));
            Assert.Equal(new[] { modelProperty }, motorcycle.Keys);

            // Act
            motorcycle.RemoveKey(modelProperty);

            // Assert
            Assert.Empty(motorcycle.Keys);
        }

        [Fact]
        public void RemoveEnumKey_ThrowsArgumentNull()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            var motorcycle = builder.AddEntityType(typeof(Motorcycle));

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => motorcycle.RemoveKey(enumKeyProperty: null),
                "enumKeyProperty");
        }

        [Fact]
        public void RemoveEnumKey_Removes_EnumKeyProperty()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            var enumEntityType = builder.AddEntityType(typeof(EnumModel));
            enumEntityType.HasKey(typeof(EnumModel).GetProperty("Simple"));

            EnumPropertyConfiguration enumProperty =
                enumEntityType.AddEnumProperty(typeof(EnumModel).GetProperty("Simple"));

            Assert.Equal(new[] { enumProperty }, enumEntityType.EnumKeys); // Guard

            // Act
            enumEntityType.RemoveKey(enumProperty);

            // Assert
            Assert.Empty(enumEntityType.EnumKeys);
        }


        [Fact]
        public void DynamicDictionaryProperty_Works_ToSetEntityTypeAsOpen()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            EntityTypeConfiguration<SimpleOpenEntityType> entityType = builder.EntityType<SimpleOpenEntityType>();
            entityType.HasKey(c => c.Id);
            entityType.Property(c => c.Name);
            entityType.HasDynamicProperties(c => c.DynamicProperties);

            // Act & Assert
            Assert.True(entityType.IsOpen);
        }

        [Fact]
        public void GetEdmModel_WorksOnModelBuilder_ForOpenEntityType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<SimpleOpenEntityType> entity = builder.EntityType<SimpleOpenEntityType>();
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name);
            entity.HasDynamicProperties(c => c.DynamicProperties);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType entityType = Assert.Single(model.SchemaElements.OfType<IEdmEntityType>());
            Assert.True(entityType.IsOpen);
            Assert.Equal(2, entityType.Properties().Count());

            Assert.True(entityType.Properties().Where(c => c.Name == "Id").Any());
            Assert.True(entityType.Properties().Where(c => c.Name == "Name").Any());
        }


        [Fact]
        public void GetEdmModel_WorksOnModelBuilder_WithDateTime()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<DateTimeModel> entity = builder.EntityType<DateTimeModel>();
            entity.HasKey(c => c.BirthdayA);
            entity.Property(c => c.BirthdayB);
            entity.CollectionProperty(c => c.BirthdayC);
            entity.CollectionProperty(c => c.BirthdayD);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType entityType = Assert.Single(model.SchemaElements.OfType<IEdmEntityType>());
            Assert.Equal("BirthdayA", entityType.DeclaredKey.Single().Name);

            IList<IEdmProperty> properties = entityType.Properties().ToList();
            Assert.Equal(4, properties.Count);
            Assert.Equal("BirthdayA", properties[0].Name);
            Assert.Equal("BirthdayB", properties[1].Name);
            Assert.Equal("BirthdayC", properties[2].Name);
            Assert.Equal("BirthdayD", properties[3].Name);
        }

        public class SimpleOpenEntityType
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public IDictionary<string, object> DynamicProperties { get; set; }
        }

        public class SimpleAnnotationEntityType
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public IODataInstanceAnnotationContainer InstanceAnnotations { get; set; }
        }
        public class EntityTypeWithDateAndTimeOfDay
        {
            public Date Date { get; set; }
            public TimeOfDay TimeOfDay { get; set; }
        }

        [Fact]
        public void CanCreateAbstractEntityTypeWithoutKey()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<BaseAbstractShape>().Abstract();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType baseShapeType =
                model.SchemaElements.OfType<IEdmEntityType>().FirstOrDefault(e => e.Name == "BaseAbstractShape");
            Assert.NotNull(baseShapeType);
            Assert.True(baseShapeType.IsAbstract);
            Assert.Null(baseShapeType.DeclaredKey);
            Assert.Empty(baseShapeType.Properties());
        }

        public abstract class BaseAbstractShape
        {
            public string ShapeName { get; set; }
        }

        [Fact]
        public void CanCreateDerivedEntityTypeWithOwnKeys()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<BaseShape>().Abstract();
            builder.EntityType<DerivedShape>().HasKey(d => d.ShapeId);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType baseShapeType =
                model.SchemaElements.OfType<IEdmEntityType>().FirstOrDefault(e => e.Name == "BaseShape");
            Assert.NotNull(baseShapeType);
            Assert.True(baseShapeType.IsAbstract);
            Assert.Null(baseShapeType.DeclaredKey);

            IEdmEntityType derivedShapeType =
                model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "DerivedShape");
            Assert.NotNull(derivedShapeType);
            Assert.False(derivedShapeType.IsAbstract);
            Assert.NotNull(derivedShapeType.DeclaredKey);

            IEdmStructuralProperty keyProperty = Assert.Single(derivedShapeType.DeclaredKey);
            Assert.Equal("ShapeId", keyProperty.Name);
        }

        public abstract class BaseShape
        {
            public string ShapeName { get; set; }
        }

        public class DerivedShape : BaseShape
        {
            public int ShapeId { get; set; }
        }
    }
}
