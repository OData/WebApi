// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder.Conventions
{
    public class NavigationLinksGenerationConventionTest
    {
        private NavigationLinksGenerationConvention _convention = new NavigationLinksGenerationConvention();

        [Fact]
        public void Apply_AddsNavigationLinkFor_AllBaseDeclaredAndDerivedProperties()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            IEntityTypeConfiguration vehicle = builder.AddEntity(typeof(Vehicle));

            IEntityTypeConfiguration car = builder.AddEntity(typeof(Car)).DerivesFrom(vehicle);
            NavigationPropertyConfiguration carNavigationProperty = car.AddNavigationProperty(typeof(Car).GetProperty("Manufacturer"), EdmMultiplicity.ZeroOrOne);

            IEntityTypeConfiguration motorcycle = builder.AddEntity(typeof(Motorcycle)).DerivesFrom(vehicle);
            NavigationPropertyConfiguration motorcycleNavigationProperty = motorcycle.AddNavigationProperty(typeof(Motorcycle).GetProperty("Manufacturer"), EdmMultiplicity.ZeroOrOne);

            IEntityTypeConfiguration manufacturer = builder.AddEntity(typeof(Manufacturer));
            IEntityTypeConfiguration motorcycleManufacturer = builder.AddEntity(typeof(MotorcycleManufacturer)).DerivesFrom(manufacturer);
            IEntityTypeConfiguration carManufacturer = builder.AddEntity(typeof(CarManufacturer)).DerivesFrom(manufacturer);

            IEntitySetConfiguration manufacturers = builder.AddEntitySet("manufacturers", manufacturer);


            Mock<IEntitySetConfiguration> entitySet = new Mock<IEntitySetConfiguration>(MockBehavior.Strict);
            entitySet.Setup(v => v.EntityType).Returns(vehicle);
            entitySet.Setup(v => v.GetNavigationPropertyLink(motorcycleNavigationProperty)).Returns<NavigationPropertyConfiguration>(null);
            entitySet.Setup(v => v.GetNavigationPropertyLink(carNavigationProperty)).Returns<NavigationPropertyConfiguration>(null);

            entitySet
                .Setup(v => v.HasNavigationPropertyLink(motorcycleNavigationProperty, It.IsAny<Func<EntityInstanceContext, IEdmNavigationProperty, Uri>>()))
                .Returns<IEntitySetConfiguration>(null);
            entitySet
                .Setup(v => v.HasNavigationPropertyLink(carNavigationProperty, It.IsAny<Func<EntityInstanceContext, IEdmNavigationProperty, Uri>>()))
                .Returns<IEntitySetConfiguration>(null);

            // Act
            _convention.Apply(entitySet.Object, builder);

            // Assert
            entitySet.VerifyAll();
        }
    }
}
