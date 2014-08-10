// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder
{
    public class EdmTypeConfigurationExtensionsTest
    {
        [Fact]
        public void EntityType_DerivedProperties_ReturnsAllDerivedProperties()
        {
            // Arrange
            Mock<EntityTypeConfiguration> entityA = new Mock<EntityTypeConfiguration>();
            entityA.Object.ExplicitProperties.Add(new MockPropertyInfo(), MockProperty("A1", entityA.Object));
            entityA.Object.ExplicitProperties.Add(new MockPropertyInfo(), MockProperty("A2", entityA.Object));

            Mock<EntityTypeConfiguration> entityB = new Mock<EntityTypeConfiguration>();
            entityB.Object.ExplicitProperties.Add(new MockPropertyInfo(), MockProperty("B1", entityB.Object));
            entityB.Object.ExplicitProperties.Add(new MockPropertyInfo(), MockProperty("B2", entityB.Object));
            entityB.Setup(e => e.BaseType).Returns(entityA.Object);

            Mock<EntityTypeConfiguration> entityC = new Mock<EntityTypeConfiguration>();
            entityC.Object.ExplicitProperties.Add(new MockPropertyInfo(), MockProperty("C1", entityC.Object));
            entityC.Object.ExplicitProperties.Add(new MockPropertyInfo(), MockProperty("C2", entityC.Object));
            entityC.Setup(e => e.BaseType).Returns(entityB.Object);

            // Act & Assert
            Assert.Equal(
                new[] { "A1", "A2", "B1", "B2" },
                entityC.Object.DerivedProperties().Select(p => p.Name).OrderBy(s => s));
        }

        [Fact]
        public void ComplexType_DerivedProperties_ReturnsAllDerivedProperties()
        {
            // Arrange
            Mock<ComplexTypeConfiguration> complexA = new Mock<ComplexTypeConfiguration>();
            complexA.Object.ExplicitProperties.Add(new MockPropertyInfo(), MockProperty("A1", complexA.Object));
            complexA.Object.ExplicitProperties.Add(new MockPropertyInfo(), MockProperty("A2", complexA.Object));

            Mock<ComplexTypeConfiguration> complexB = new Mock<ComplexTypeConfiguration>();
            complexB.Object.ExplicitProperties.Add(new MockPropertyInfo(), MockProperty("B1", complexB.Object));
            complexB.Object.ExplicitProperties.Add(new MockPropertyInfo(), MockProperty("B2", complexB.Object));
            complexB.Setup(e => e.BaseType).Returns(complexA.Object);

            Mock<ComplexTypeConfiguration> complexC = new Mock<ComplexTypeConfiguration>();
            complexC.Object.ExplicitProperties.Add(new MockPropertyInfo(), MockProperty("C1", complexC.Object));
            complexC.Object.ExplicitProperties.Add(new MockPropertyInfo(), MockProperty("C2", complexC.Object));
            complexC.Setup(e => e.BaseType).Returns(complexB.Object);

            // Act & Assert
            Assert.Equal(
                new[] { "A1", "A2", "B1", "B2" },
                complexC.Object.DerivedProperties().Select(p => p.Name).OrderBy(s => s));
        }

        [Fact]
        public void Keys_Returns_DeclaredKeys_IfNoBaseType()
        {
            // Arrange
            PrimitivePropertyConfiguration[] keys = new PrimitivePropertyConfiguration[0];

            Mock<EntityTypeConfiguration> entity = new Mock<EntityTypeConfiguration>();
            entity.Setup(e => e.BaseType).Returns<EntityTypeConfiguration>(null);
            entity.Setup(e => e.Keys).Returns(keys);

            // Act & Assert
            Assert.ReferenceEquals(keys, entity.Object.Keys());
        }

        [Fact]
        public void Keys_Returns_DerivedKeys_IfBaseTypePresent()
        {
            // Arrange
            PrimitivePropertyConfiguration[] keys = new PrimitivePropertyConfiguration[0];


            Mock<EntityTypeConfiguration> baseBaseEntity = new Mock<EntityTypeConfiguration>();
            baseBaseEntity.Setup(e => e.Keys).Returns(keys);
            baseBaseEntity.Setup(e => e.BaseType).Returns<EntityTypeConfiguration>(null);

            Mock<EntityTypeConfiguration> baseEntity = new Mock<EntityTypeConfiguration>();
            baseEntity.Setup(e => e.BaseType).Returns(baseBaseEntity.Object);

            Mock<EntityTypeConfiguration> entity = new Mock<EntityTypeConfiguration>();
            baseEntity.Setup(e => e.BaseType).Returns(baseEntity.Object);

            // Act & Assert
            Assert.ReferenceEquals(keys, entity.Object.Keys());
        }

        [Fact]
        public void EntityType_ThisAndBaseAndDerivedTypes_Works()
        {
            // Arrange
            ODataModelBuilder builder = GetMockVehicleModel();
            EntityTypeConfiguration vehicle = builder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Single(e => e.Name == "vehicle");

            // Act & Assert
            Assert.Equal(
                new[] {"car", "motorcycle", "sportbike", "vehicle"}.OrderBy(e => e),
                builder.ThisAndBaseAndDerivedTypes(vehicle).Select(e => e.Name).OrderBy(name => name));
        }

        [Fact]
        public void ComplexType_ThisAndBaseAndDerivedTypes_Works()
        {
            // Arrange
            ODataModelBuilder builder = GetMockVehicleModel();
            ComplexTypeConfiguration address = builder.StructuralTypes
                .OfType<ComplexTypeConfiguration>().Single(e => e.Name == "Address");

            // Act & Assert
            Assert.Equal(
                new[] { "Address", "CarAddress", "MotorcycleAddress", "SportbikeAddress"}.OrderBy(name => name),
                builder.ThisAndBaseAndDerivedTypes(address).Select(e => e.Name).OrderBy(name => name));
        }

        [Fact]
        public void EntityType_BaseTypes_Works()
        {
            // Arrange
            ODataModelBuilder builder = GetMockVehicleModel();

            EntityTypeConfiguration sportbike = builder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Single(e => e.Name == "sportbike");

            // Act & Assert
            Assert.Equal(
                new[] { "vehicle", "motorcycle" }.OrderBy(name => name),
                sportbike.BaseTypes().Select(e => e.Name).OrderBy(name => name));
        }

        [Fact]
        public void ComplexType_BaseTypes_Works()
        {
            // Arrange
            ODataModelBuilder builder = GetMockVehicleModel();

            ComplexTypeConfiguration sportbikeAddress = builder.StructuralTypes
                .OfType<ComplexTypeConfiguration>().Single(e => e.Name == "SportbikeAddress");

            // Act & Assert
            Assert.Equal(
                new[] { "Address", "MotorcycleAddress" }.OrderBy(name => name),
                sportbikeAddress.BaseTypes().Select(e => e.Name).OrderBy(name => name));
        }

        [Fact]
        public void EntityType_DerivedTypes_Works()
        {
            // Arrange
            ODataModelBuilder builder = GetMockVehicleModel();

            EntityTypeConfiguration vehicle = builder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Single(e => e.Name == "vehicle");

            // Act & Assert
            Assert.Equal(
                new[] { "car", "motorcycle", "sportbike" }.OrderBy(name => name),
                builder.DerivedTypes(vehicle).Select(e => e.Name).OrderBy(name => name));
        }

        [Fact]
        public void ComplexType_DerivedTypes_Works()
        {
            // Arrange
            ODataModelBuilder builder = GetMockVehicleModel();

            ComplexTypeConfiguration address = builder.StructuralTypes
                .OfType<ComplexTypeConfiguration>().Single(e => e.Name == "Address");

            // Act & Assert
            Assert.Equal(
                new[] { "CarAddress", "MotorcycleAddress", "SportbikeAddress" }.OrderBy(name => name),
                builder.DerivedTypes(address).Select(e => e.Name).OrderBy(name => name));
        }

        [Fact]
        public void EntityType_IsAssignableFrom_ReturnsTrueForDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = GetMockVehicleModel();

            EntityTypeConfiguration vehicle = builder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Single(e => e.Name == "vehicle");
            EntityTypeConfiguration car = builder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Single(e => e.Name == "car");
            EntityTypeConfiguration motorcycle = builder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Single(e => e.Name == "motorcycle");
            EntityTypeConfiguration sportbike = builder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Single(e => e.Name == "sportbike");

            // Act & Assert
            Assert.True(vehicle.IsAssignableFrom(vehicle));
            Assert.True(vehicle.IsAssignableFrom(car));
            Assert.True(vehicle.IsAssignableFrom(motorcycle));
            Assert.True(vehicle.IsAssignableFrom(sportbike));
        }

        [Fact]
        public void ComplexType_IsAssignableFrom_ReturnsTrueForDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = GetMockVehicleModel();

            ComplexTypeConfiguration address = builder.StructuralTypes
                .OfType<ComplexTypeConfiguration>().Single(e => e.Name == "Address");
            ComplexTypeConfiguration carAddress = builder.StructuralTypes
                .OfType<ComplexTypeConfiguration>().Single(e => e.Name == "CarAddress");
            ComplexTypeConfiguration motorcycleAddress = builder.StructuralTypes
                .OfType<ComplexTypeConfiguration>().Single(e => e.Name == "MotorcycleAddress");
            ComplexTypeConfiguration sportbikeAddress = builder.StructuralTypes
                .OfType<ComplexTypeConfiguration>().Single(e => e.Name == "SportbikeAddress");

            // Act & Assert
            Assert.True(address.IsAssignableFrom(address));
            Assert.True(address.IsAssignableFrom(carAddress));
            Assert.True(address.IsAssignableFrom(motorcycleAddress));
            Assert.True(address.IsAssignableFrom(sportbikeAddress));
        }

        [Fact]
        public void EntityType_IsAssignableFrom_ReturnsFalseForBaseType()
        {
            // Arrange
            ODataModelBuilder builder = GetMockVehicleModel();

            EntityTypeConfiguration vehicle = builder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Single(e => e.Name == "vehicle");
            EntityTypeConfiguration car = builder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Single(e => e.Name == "car");
            EntityTypeConfiguration motorcycle = builder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Single(e => e.Name == "motorcycle");
            EntityTypeConfiguration sportbike = builder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Single(e => e.Name == "sportbike");

            // Act & Assert
            Assert.False(car.IsAssignableFrom(vehicle));
            Assert.False(motorcycle.IsAssignableFrom(vehicle));
            Assert.False(sportbike.IsAssignableFrom(vehicle));
        }

        [Fact]
        public void ComplexType_IsAssignableFrom_ReturnsFalseForBaseType()
        {
            // Arrange
            ODataModelBuilder builder = GetMockVehicleModel();

            ComplexTypeConfiguration address = builder.StructuralTypes
                .OfType<ComplexTypeConfiguration>().Single(e => e.Name == "Address");
            ComplexTypeConfiguration carAddress = builder.StructuralTypes
                .OfType<ComplexTypeConfiguration>().Single(e => e.Name == "CarAddress");
            ComplexTypeConfiguration motorcycleAddress = builder.StructuralTypes
                .OfType<ComplexTypeConfiguration>().Single(e => e.Name == "MotorcycleAddress");
            ComplexTypeConfiguration sportbikeAddress = builder.StructuralTypes
                .OfType<ComplexTypeConfiguration>().Single(e => e.Name == "SportbikeAddress");

            // Act & Assert
            Assert.False(carAddress.IsAssignableFrom(address));
            Assert.False(motorcycleAddress.IsAssignableFrom(address));
            Assert.False(sportbikeAddress.IsAssignableFrom(address));
        }

        [Fact]
        public void EntityType_IsAssignableFrom_ReturnsFalseForUnRelatedTypes()
        {
            // Arrange
            ODataModelBuilder builder = GetMockVehicleModel();

            EntityTypeConfiguration car = builder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Single(e => e.Name == "car");
            EntityTypeConfiguration motorcycle = builder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Single(e => e.Name == "motorcycle");
            EntityTypeConfiguration sportbike = builder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Single(e => e.Name == "sportbike");

            // Act & Assert
            Assert.False(motorcycle.IsAssignableFrom(car));
            Assert.False(car.IsAssignableFrom(motorcycle));
            Assert.False(sportbike.IsAssignableFrom(car));
            Assert.False(car.IsAssignableFrom(sportbike));
        }

        [Fact]
        public void ComplexType_IsAssignableFrom_ReturnsFalseForUnRelatedTypes()
        {
            // Arrange
            ODataModelBuilder builder = GetMockVehicleModel();

            ComplexTypeConfiguration carAddress = builder.StructuralTypes
                .OfType<ComplexTypeConfiguration>().Single(e => e.Name == "CarAddress");
            ComplexTypeConfiguration motorcycleAddress = builder.StructuralTypes
                .OfType<ComplexTypeConfiguration>().Single(e => e.Name == "MotorcycleAddress");
            ComplexTypeConfiguration sportbikeAddress = builder.StructuralTypes
                .OfType<ComplexTypeConfiguration>().Single(e => e.Name == "SportbikeAddress");

            // Act & Assert
            Assert.False(motorcycleAddress.IsAssignableFrom(carAddress));
            Assert.False(carAddress.IsAssignableFrom(motorcycleAddress));
            Assert.False(sportbikeAddress.IsAssignableFrom(carAddress));
            Assert.False(carAddress.IsAssignableFrom(sportbikeAddress));
        }

        [Fact]
        public void EntityType_ThisAndBaseTypes_ReturnsThisType()
        {
            // Arrange
            ODataModelBuilder builder = GetMockVehicleModel();
            EntityTypeConfiguration sportbike = builder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Single(e => e.Name == "sportbike");

            // Act & Assert
            Assert.Contains(sportbike, sportbike.ThisAndBaseTypes());
        }

        [Fact]
        public void EntityType_ThisAndBaseTypes_ReturnsAllTheBaseTypes()
        {
            // Arrange
            ODataModelBuilder builder = GetMockVehicleModel();

            EntityTypeConfiguration vehicle = builder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Single(e => e.Name == "vehicle");
            EntityTypeConfiguration motorcycle = builder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Single(e => e.Name == "motorcycle");
            EntityTypeConfiguration sportbike = builder.StructuralTypes
                .OfType<EntityTypeConfiguration>().Single(e => e.Name == "sportbike");

            // Act
            IEnumerable<StructuralTypeConfiguration> thisAndBaseTypes = sportbike.ThisAndBaseTypes();

            // Assert
            Assert.Contains(vehicle, thisAndBaseTypes);
            Assert.Contains(motorcycle, thisAndBaseTypes);
            Assert.Contains(sportbike, thisAndBaseTypes);
        }

        [Fact]
        public void ComplexType_ThisAndBaseTypes_ReturnsThisType()
        {
            // Arrange
            ODataModelBuilder builder = GetMockVehicleModel();
            ComplexTypeConfiguration sportbikeAddress = builder.StructuralTypes
                .OfType<ComplexTypeConfiguration>().Where(e => e.Name == "SportbikeAddress").Single();

            // Act & Assert
            Assert.Contains(sportbikeAddress, sportbikeAddress.ThisAndBaseTypes());
        }

        [Fact]
        public void ComplexType_ThisAndBaseTypes_ReturnsAllTheBaseTypes()
        {
            // Arrange
            ODataModelBuilder builder = GetMockVehicleModel();

            ComplexTypeConfiguration address = builder.StructuralTypes
                .OfType<ComplexTypeConfiguration>().Single(e => e.Name == "Address");
            ComplexTypeConfiguration motorcycleAddress = builder.StructuralTypes
                .OfType<ComplexTypeConfiguration>().Single(e => e.Name == "MotorcycleAddress");
            ComplexTypeConfiguration sportbikeAddress = builder.StructuralTypes
                .OfType<ComplexTypeConfiguration>().Single(e => e.Name == "SportbikeAddress");

            // Act
            IEnumerable<StructuralTypeConfiguration> thisAndBaseTypes = sportbikeAddress.ThisAndBaseTypes();

            // Assert
            Assert.Contains(address, thisAndBaseTypes);
            Assert.Contains(motorcycleAddress, thisAndBaseTypes);
            Assert.Contains(sportbikeAddress, thisAndBaseTypes);
        }

        private static ODataModelBuilder GetMockVehicleModel()
        {
            // Entity types
            Mock<EntityTypeConfiguration> vehicle = new Mock<EntityTypeConfiguration>();
            vehicle.Setup(c => c.Name).Returns("vehicle");
            vehicle.SetupGet(c => c.Kind).Returns(EdmTypeKind.Entity);

            Mock<EntityTypeConfiguration> car = new Mock<EntityTypeConfiguration>();
            car.Setup(c => c.Name).Returns("car");
            car.Setup(c => c.BaseType).Returns(vehicle.Object);
            car.SetupGet(c => c.Kind).Returns(EdmTypeKind.Entity);

            Mock<EntityTypeConfiguration> motorcycle = new Mock<EntityTypeConfiguration>();
            motorcycle.Setup(c => c.Name).Returns("motorcycle");
            motorcycle.Setup(c => c.BaseType).Returns(vehicle.Object);
            motorcycle.SetupGet(c => c.Kind).Returns(EdmTypeKind.Entity);

            Mock<EntityTypeConfiguration> sportbike = new Mock<EntityTypeConfiguration>();
            sportbike.Setup(c => c.Name).Returns("sportbike");
            sportbike.Setup(c => c.BaseType).Returns(motorcycle.Object);
            sportbike.SetupGet(c => c.Kind).Returns(EdmTypeKind.Entity);

            // Complex Types
            Mock<ComplexTypeConfiguration> address = new Mock<ComplexTypeConfiguration>();
            address.Setup(c => c.Name).Returns("Address");
            address.SetupGet(c => c.Kind).Returns(EdmTypeKind.Complex);

            Mock<ComplexTypeConfiguration> carAddress = new Mock<ComplexTypeConfiguration>();
            carAddress.Setup(c => c.Name).Returns("CarAddress");
            carAddress.Setup(c => c.BaseType).Returns(address.Object);
            carAddress.SetupGet(c => c.Kind).Returns(EdmTypeKind.Complex);

            Mock<ComplexTypeConfiguration> motorcycleAddress = new Mock<ComplexTypeConfiguration>();
            motorcycleAddress.Setup(c => c.Name).Returns("MotorcycleAddress");
            motorcycleAddress.Setup(c => c.BaseType).Returns(address.Object);
            motorcycleAddress.SetupGet(c => c.Kind).Returns(EdmTypeKind.Complex);

            Mock<ComplexTypeConfiguration> sportbikeAddress = new Mock<ComplexTypeConfiguration>();
            sportbikeAddress.Setup(c => c.Name).Returns("SportbikeAddress");
            sportbikeAddress.Setup(c => c.BaseType).Returns(motorcycleAddress.Object);
            sportbikeAddress.SetupGet(c => c.Kind).Returns(EdmTypeKind.Complex);

            Mock<ODataModelBuilder> modelBuilder = new Mock<ODataModelBuilder>();
            modelBuilder.Setup(m => m.StructuralTypes).Returns(
                new StructuralTypeConfiguration[] 
                {
                    vehicle.Object, motorcycle.Object, car.Object, sportbike.Object,
                    address.Object, carAddress.Object, motorcycleAddress.Object, sportbikeAddress.Object
                });

            return modelBuilder.Object;
        }

        private static PropertyConfiguration MockProperty(string name, StructuralTypeConfiguration declaringType)
        {
            Mock<PropertyInfo> propertyInfo = new Mock<PropertyInfo>();
            propertyInfo.Setup(p => p.Name).Returns(name);

            Mock<PropertyConfiguration> property = new Mock<PropertyConfiguration>(propertyInfo.Object, declaringType);
            return property.Object;
        }
    }
}
