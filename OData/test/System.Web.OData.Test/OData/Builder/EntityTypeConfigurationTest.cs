// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder.Test
{
    public class EntityTypeConfigurationTest
    {
        [Theory]
        [InlineData(typeof(IEnumerable<DateTime>), EdmMultiplicity.Many)]
        [InlineData(typeof(IEnumerable<DateTime?>), EdmMultiplicity.Unknown)]
        [InlineData(typeof(DateTime), EdmMultiplicity.One)]
        [InlineData(typeof(DateTime?), EdmMultiplicity.ZeroOrOne)]
        public void AddNavigationProperty_ThrowsIfTypeIsDateTime(Type propertyType, EdmMultiplicity multiplicity)
        {
            // Arrange
            MockType type = new MockType("Customer", @namespace: "Contoso");
            MockPropertyInfo property = new MockPropertyInfo(propertyType, "Birthday");
            property.SetupGet(p => p.ReflectedType).Returns(type);
            property.SetupGet(p => p.DeclaringType).Returns(type);

            Mock<EntityTypeConfiguration> mock = new Mock<EntityTypeConfiguration> { CallBase = true };
            EntityTypeConfiguration configuration = mock.Object;
            mock.SetupGet(c => c.ClrType).Returns(type);

            // Act & Assert
            Assert.ThrowsArgument(
                () => configuration.AddNavigationProperty(property, multiplicity),
                "navigationProperty",
                string.Format(
                    "The type '{0}' of property 'Birthday' in the 'Contoso.Customer' type is not a supported type.",
                    propertyType.FullName));
        }

        [Theory]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(DateTime?))]
        public void Ctor_ThrowsIfPropertyIsDateTime(Type type)
        {
            // Act & Assert
            Assert.ThrowsArgument(() =>
                new EntityTypeConfiguration(Mock.Of<ODataModelBuilder>(), type),
                "clrType",
                string.Format("The type '{0}' is not a supported type.", type.FullName));
        }
    }
}
