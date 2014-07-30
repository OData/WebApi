// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder
{
    public class EdmTypeConfigurationExtensionsTest
    {
        [Fact]
        public void DerivedProperties_ReturnsAllDerivedProperties()
        {
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

            Assert.Equal(
                new[] { "A1", "A2", "B1", "B2" },
                entityC.Object.DerivedProperties().Select(p => p.Name).OrderBy(s => s));
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
        public void ThisAndBaseAndDerivedTypes_Works()
        {
            ODataModelBuilder builder = GetMockVehicleModel();
            EntityTypeConfiguration vehicle = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "vehicle").Single();

            Assert.Equal(
                builder.ThisAndBaseAndDerivedTypes(vehicle).Select(e => e.Name).OrderBy(name => name),
                builder.StructuralTypes.Select(e => e.Name).OrderBy(name => name));
        }

        [Fact]
        public void BaseTypes_Works()
        {
            ODataModelBuilder builder = GetMockVehicleModel();

            EntityTypeConfiguration vehicle = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "vehicle").Single();
            EntityTypeConfiguration motorcycle = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "motorcycle").Single();
            EntityTypeConfiguration sportbike = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "sportbike").Single();

            Assert.Equal(
                sportbike.BaseTypes().Select(e => e.Name).OrderBy(name => name),
                new[] { vehicle, motorcycle }.Select(e => e.Name).OrderBy(name => name));
        }

        [Fact]
        public void DerivedTypes_Works()
        {
            ODataModelBuilder builder = GetMockVehicleModel();
            EntityTypeConfiguration vehicle = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "vehicle").Single();
            EntityTypeConfiguration car = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "car").Single();
            EntityTypeConfiguration motorcycle = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "motorcycle").Single();
            EntityTypeConfiguration sportbike = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "sportbike").Single();

            Assert.Equal(
                builder.DerivedTypes(vehicle).Select(e => e.Name).OrderBy(name => name),
                new[] { sportbike, motorcycle, car }.Select(e => e.Name).OrderBy(name => name));
        }

        [Fact]
        public void IsAssignableFrom_ReturnsTrueForDerivedType()
        {
            ODataModelBuilder builder = GetMockVehicleModel();
            EntityTypeConfiguration vehicle = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "vehicle").Single();
            EntityTypeConfiguration car = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "car").Single();
            EntityTypeConfiguration motorcycle = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "motorcycle").Single();
            EntityTypeConfiguration sportbike = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "sportbike").Single();

            Assert.True(vehicle.IsAssignableFrom(vehicle));
            Assert.True(vehicle.IsAssignableFrom(car));
            Assert.True(vehicle.IsAssignableFrom(motorcycle));
            Assert.True(vehicle.IsAssignableFrom(sportbike));
        }

        [Fact]
        public void IsAssignableFrom_ReturnsFalseForBaseType()
        {
            ODataModelBuilder builder = GetMockVehicleModel();
            EntityTypeConfiguration vehicle = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "vehicle").Single();
            EntityTypeConfiguration car = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "car").Single();
            EntityTypeConfiguration motorcycle = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "motorcycle").Single();
            EntityTypeConfiguration sportbike = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "sportbike").Single();

            Assert.False(car.IsAssignableFrom(vehicle));
            Assert.False(motorcycle.IsAssignableFrom(vehicle));
            Assert.False(sportbike.IsAssignableFrom(vehicle));
        }

        [Fact]
        public void IsAssignableFrom_ReturnsFalseForUnRelatedTypes()
        {
            ODataModelBuilder builder = GetMockVehicleModel();
            EntityTypeConfiguration vehicle = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "vehicle").Single();
            EntityTypeConfiguration car = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "car").Single();
            EntityTypeConfiguration motorcycle = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "motorcycle").Single();
            EntityTypeConfiguration sportbike = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "sportbike").Single();

            Assert.False(motorcycle.IsAssignableFrom(car));
            Assert.False(car.IsAssignableFrom(motorcycle));
            Assert.False(sportbike.IsAssignableFrom(car));
            Assert.False(car.IsAssignableFrom(sportbike));
        }

        [Fact]
        public void ThisAndBaseTypes_ReturnsThisType()
        {
            ODataModelBuilder builder = GetMockVehicleModel();
            EntityTypeConfiguration sportbike = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "sportbike").Single();

            Assert.Contains(sportbike, sportbike.ThisAndBaseTypes());
        }

        [Fact]
        public void ThisAndBaseTypes_ReturnsAllTheBaseTypes()
        {
            ODataModelBuilder builder = GetMockVehicleModel();
            EntityTypeConfiguration vehicle = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "vehicle").Single();
            EntityTypeConfiguration motorcycle = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "motorcycle").Single();
            EntityTypeConfiguration sportbike = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "sportbike").Single();

            Assert.Contains(vehicle, sportbike.ThisAndBaseTypes());
            Assert.Contains(motorcycle, sportbike.ThisAndBaseTypes());
            Assert.Contains(sportbike, sportbike.ThisAndBaseTypes());
        }

        private static ODataModelBuilder GetMockVehicleModel()
        {
            Mock<EntityTypeConfiguration> vehicle = new Mock<EntityTypeConfiguration>();
            vehicle.Setup(c => c.Name).Returns("vehicle");

            Mock<EntityTypeConfiguration> car = new Mock<EntityTypeConfiguration>();
            car.Setup(c => c.Name).Returns("car");
            car.Setup(e => e.BaseType).Returns(vehicle.Object);

            Mock<EntityTypeConfiguration> motorcycle = new Mock<EntityTypeConfiguration>();
            motorcycle.Setup(c => c.Name).Returns("motorcycle");
            motorcycle.Setup(e => e.BaseType).Returns(vehicle.Object);

            Mock<EntityTypeConfiguration> sportbike = new Mock<EntityTypeConfiguration>();
            sportbike.Setup(c => c.Name).Returns("sportbike");
            sportbike.Setup(e => e.BaseType).Returns(motorcycle.Object);

            Mock<ODataModelBuilder> modelBuilder = new Mock<ODataModelBuilder>();
            modelBuilder.Setup(m => m.StructuralTypes).Returns(
                new StructuralTypeConfiguration[] { vehicle.Object, motorcycle.Object, car.Object, sportbike.Object });

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
