// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder
{
    public class EdmTypeConfigurationExtensionsTest
    {
        [Fact]
        public void DerivedProperties_ReturnsAllDerivedProperties()
        {
            Mock<IEntityTypeConfiguration> entityA = new Mock<IEntityTypeConfiguration>();
            entityA.Setup(e => e.Properties).Returns(new[] { MockProperty("A1"), MockProperty("A2") });

            Mock<IEntityTypeConfiguration> entityB = new Mock<IEntityTypeConfiguration>();
            entityB.Setup(e => e.Properties).Returns(new[] { MockProperty("B1"), MockProperty("B2") });
            entityB.Setup(e => e.BaseType).Returns(entityA.Object);

            Mock<IEntityTypeConfiguration> entityC = new Mock<IEntityTypeConfiguration>();
            entityC.Setup(e => e.Properties).Returns(new[] { MockProperty("C1"), MockProperty("C2") });
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

            Mock<IEntityTypeConfiguration> entity = new Mock<IEntityTypeConfiguration>();
            entity.Setup(e => e.BaseType).Returns<IEntityTypeConfiguration>(null);
            entity.Setup(e => e.Keys).Returns(keys);

            // Act & Assert
            Assert.ReferenceEquals(keys, entity.Object.Keys());
        }

        [Fact]
        public void Keys_Returns_DerivedKeys_IfBaseTypePresent()
        {
            // Arrange
            PrimitivePropertyConfiguration[] keys = new PrimitivePropertyConfiguration[0];


            Mock<IEntityTypeConfiguration> baseBaseEntity = new Mock<IEntityTypeConfiguration>();
            baseBaseEntity.Setup(e => e.Keys).Returns(keys);
            baseBaseEntity.Setup(e => e.BaseType).Returns<IEntityTypeConfiguration>(null);

            Mock<IEntityTypeConfiguration> baseEntity = new Mock<IEntityTypeConfiguration>();
            baseEntity.Setup(e => e.BaseType).Returns(baseBaseEntity.Object);

            Mock<IEntityTypeConfiguration> entity = new Mock<IEntityTypeConfiguration>();
            baseEntity.Setup(e => e.BaseType).Returns(baseEntity.Object);

            // Act & Assert
            Assert.ReferenceEquals(keys, entity.Object.Keys());
        }

        [Fact]
        public void ThisAndBaseAndDerivedTypes_Works()
        {
            ODataModelBuilder builder = GetMockVehicleModel();
            IEntityTypeConfiguration vehicle = builder.StructuralTypes.OfType<IEntityTypeConfiguration>().Where(e => e.Name == "vehicle").Single();

            Assert.Equal(
                builder.ThisAndBaseAndDerivedTypes(vehicle).Select(e => e.Name).OrderBy(name => name),
                builder.StructuralTypes.Select(e => e.Name).OrderBy(name => name));
        }

        [Fact]
        public void BaseTypes_Works()
        {
            ODataModelBuilder builder = GetMockVehicleModel();

            IEntityTypeConfiguration vehicle = builder.StructuralTypes.OfType<IEntityTypeConfiguration>().Where(e => e.Name == "vehicle").Single();
            IEntityTypeConfiguration motorcycle = builder.StructuralTypes.OfType<IEntityTypeConfiguration>().Where(e => e.Name == "motorcycle").Single();
            IEntityTypeConfiguration sportbike = builder.StructuralTypes.OfType<IEntityTypeConfiguration>().Where(e => e.Name == "sportbike").Single();

            Assert.Equal(
                sportbike.BaseTypes().Select(e => e.Name).OrderBy(name => name),
                new[] { vehicle, motorcycle }.Select(e => e.Name).OrderBy(name => name));
        }

        [Fact]
        public void DerivedTypes_Works()
        {
            ODataModelBuilder builder = GetMockVehicleModel();
            IEntityTypeConfiguration vehicle = builder.StructuralTypes.OfType<IEntityTypeConfiguration>().Where(e => e.Name == "vehicle").Single();
            IEntityTypeConfiguration car = builder.StructuralTypes.OfType<IEntityTypeConfiguration>().Where(e => e.Name == "car").Single();
            IEntityTypeConfiguration motorcycle = builder.StructuralTypes.OfType<IEntityTypeConfiguration>().Where(e => e.Name == "motorcycle").Single();
            IEntityTypeConfiguration sportbike = builder.StructuralTypes.OfType<IEntityTypeConfiguration>().Where(e => e.Name == "sportbike").Single();

            Assert.Equal(
                builder.DerivedTypes(vehicle).Select(e => e.Name).OrderBy(name => name),
                new[] { sportbike, motorcycle, car }.Select(e => e.Name).OrderBy(name => name));
        }

        private static ODataModelBuilder GetMockVehicleModel()
        {
            Mock<IEntityTypeConfiguration> vehicle = new Mock<IEntityTypeConfiguration>();
            vehicle.Setup(c => c.Name).Returns("vehicle");

            Mock<IEntityTypeConfiguration> car = new Mock<IEntityTypeConfiguration>();
            car.Setup(c => c.Name).Returns("car");
            car.Setup(e => e.BaseType).Returns(vehicle.Object);

            Mock<IEntityTypeConfiguration> motorcycle = new Mock<IEntityTypeConfiguration>();
            motorcycle.Setup(c => c.Name).Returns("motorcycle");
            motorcycle.Setup(e => e.BaseType).Returns(vehicle.Object);

            Mock<IEntityTypeConfiguration> sportbike = new Mock<IEntityTypeConfiguration>();
            sportbike.Setup(c => c.Name).Returns("sportbike");
            sportbike.Setup(e => e.BaseType).Returns(motorcycle.Object);

            Mock<ODataModelBuilder> modelBuilder = new Mock<ODataModelBuilder>();
            modelBuilder.Setup(m => m.StructuralTypes).Returns(
                new IStructuralTypeConfiguration[] { vehicle.Object, motorcycle.Object, car.Object, sportbike.Object });

            return modelBuilder.Object;
        }

        private static PropertyConfiguration MockProperty(string name)
        {
            Mock<PropertyInfo> propertyInfo = new Mock<PropertyInfo>();
            propertyInfo.Setup(p => p.Name).Returns(name);

            Mock<PropertyConfiguration> property = new Mock<PropertyConfiguration>(propertyInfo.Object);
            return property.Object;
        }
    }
}
