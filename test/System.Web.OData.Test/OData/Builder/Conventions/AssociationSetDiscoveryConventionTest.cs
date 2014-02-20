// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.OData.Builder.TestModels;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder.Conventions
{
    public class AssociationSetDiscoveryConventionTest
    {
        private AssociationSetDiscoveryConvention _convention = new AssociationSetDiscoveryConvention();

        [Fact]
        public void AssociationSetDiscoveryConvention_AddsBindingForBaseAndDerivedNavigationProperties()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            EntityTypeConfiguration vehicle = builder.AddEntityType(typeof(Vehicle));

            EntityTypeConfiguration car = builder.AddEntityType(typeof(Car)).DerivesFrom(vehicle);
            NavigationPropertyConfiguration carNavigationProperty = car.AddNavigationProperty(typeof(Car).GetProperty("Manufacturer"), EdmMultiplicity.ZeroOrOne);

            EntityTypeConfiguration motorcycle = builder.AddEntityType(typeof(Motorcycle)).DerivesFrom(vehicle);
            NavigationPropertyConfiguration motorcycleNavigationProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.ZeroOrOne);

            EntityTypeConfiguration manufacturer = builder.AddEntityType(typeof(Manufacturer));
            EntityTypeConfiguration motorcycleManufacturer = builder.AddEntityType(typeof(MotorcycleManufacturer)).DerivesFrom(manufacturer);
            EntityTypeConfiguration carManufacturer = builder.AddEntityType(typeof(CarManufacturer)).DerivesFrom(manufacturer);

            EntitySetConfiguration manufacturers = builder.AddEntitySet("manufacturers", manufacturer);


            Mock<EntitySetConfiguration> entitySet = new Mock<EntitySetConfiguration>(MockBehavior.Strict);
            entitySet.Setup(v => v.EntityType).Returns(vehicle);
            entitySet.Setup(v => v.AddBinding(motorcycleNavigationProperty, manufacturers)).Returns<NavigationPropertyConfiguration>(null);
            entitySet.Setup(v => v.AddBinding(carNavigationProperty, manufacturers)).Returns<NavigationPropertyConfiguration>(null);

            // Act
            _convention.Apply(entitySet.Object, builder);

            // Assert
            entitySet.VerifyAll();
        }

        [Fact]
        public void GetTargetEntitySet_Throws_IfTargetEntityTypeIsMissing()
        {
            // Arrange
            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.PropertyType).Returns(typeof(Vehicle));
            property.Setup(p => p.ReflectedType).Returns(typeof(AssociationSetDiscoveryConventionTest));
            property.Setup(p => p.Name).Returns("SamplePropertyName");

            Mock<EntityTypeConfiguration> entityTypeConfiguration = new Mock<EntityTypeConfiguration>();
            NavigationPropertyConfiguration config = new NavigationPropertyConfiguration(property.Object, EdmMultiplicity.ZeroOrOne, entityTypeConfiguration.Object);

            Mock<ODataModelBuilder> modelBuilder = new Mock<ODataModelBuilder>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => AssociationSetDiscoveryConvention.GetTargetEntitySet(config, modelBuilder.Object),
                "Could not find the target entity type for the navigation property 'SamplePropertyName' on entity type 'System.Web.OData.Builder.Conventions.AssociationSetDiscoveryConventionTest'.");
        }

        [Fact]
        public void GetTargetEntitySet_Returns_Null_IfNoMatchingTargetEntitySet()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration motorcycle = builder.AddEntityType(typeof(Motorcycle));
            NavigationPropertyConfiguration navigationProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.ZeroOrOne);

            // Act
            EntitySetConfiguration targetEntitySet = AssociationSetDiscoveryConvention.GetTargetEntitySet(navigationProperty, builder);

            // Assert
            Assert.Null(targetEntitySet);
        }

        [Fact]
        public void GetTargetEntitySet_Returns_TargetEntitySet()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration motorcycle = builder.AddEntityType(typeof(Motorcycle));
            EntityTypeConfiguration manufacturer = builder.AddEntityType(typeof(MotorcycleManufacturer));
            NavigationPropertyConfiguration navigationProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.ZeroOrOne);
            EntitySetConfiguration manufacturers = builder.AddEntitySet("manufacturers", manufacturer);

            // Act
            EntitySetConfiguration targetEntitySet = AssociationSetDiscoveryConvention.GetTargetEntitySet(navigationProperty, builder);

            // Assert
            Assert.Equal(manufacturers, targetEntitySet);
        }

        [Fact]
        public void GetTargetEntitySet_Returns_Null_IfMultipleMatchingTargetEntitySet()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration motorcycle = builder.AddEntityType(typeof(Motorcycle));
            EntityTypeConfiguration manufacturer = builder.AddEntityType(typeof(MotorcycleManufacturer));
            NavigationPropertyConfiguration navigationProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.ZeroOrOne);
            EntitySetConfiguration manufacturers1 = builder.AddEntitySet("manufacturers1", manufacturer);
            EntitySetConfiguration manufacturers2 = builder.AddEntitySet("manufacturers2", manufacturer);

            // Act
            EntitySetConfiguration targetEntitySet = AssociationSetDiscoveryConvention.GetTargetEntitySet(navigationProperty, builder);

            // Assert
            Assert.Null(targetEntitySet);
        }

        [Fact]
        public void GetTargetEntitySet_Returns_BaseTypeEntitySet_IfNoMatchingEntitysetForCurrentType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration motorcycle = builder.AddEntityType(typeof(Motorcycle));
            EntityTypeConfiguration manufacturer = builder.AddEntityType(typeof(Manufacturer));
            EntityTypeConfiguration motorcycleManufacturer = builder.AddEntityType(typeof(MotorcycleManufacturer)).DerivesFrom(manufacturer);

            NavigationPropertyConfiguration navigationProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.ZeroOrOne);
            EntitySetConfiguration manufacturers = builder.AddEntitySet("manufacturers", manufacturer);

            // Act
            EntitySetConfiguration targetEntitySet = AssociationSetDiscoveryConvention.GetTargetEntitySet(navigationProperty, builder);

            // Assert
            Assert.Equal(manufacturers, targetEntitySet);
        }
    }
}
