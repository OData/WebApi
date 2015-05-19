// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Builder.Conventions.Attributes
{
    public class ForeignKeyDiscoveryConventionTest
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            Assert.DoesNotThrow(() => new ForeignKeyDiscoveryConvention());
        }

        [Theory]
        [InlineData("Key1", EdmMultiplicity.One, false)]
        [InlineData("Key2", EdmMultiplicity.One, false)]
        [InlineData("Key3", EdmMultiplicity.One, false)]
        [InlineData("Key1", EdmMultiplicity.ZeroOrOne, true)]
        [InlineData("Key2", EdmMultiplicity.ZeroOrOne, true)]
        [InlineData("Key3", EdmMultiplicity.ZeroOrOne, true)]
        public void Apply_TypeNameConventions_Works(string propertyName, EdmMultiplicity multiplicity, bool optional)
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

            EntityTypeConfiguration principalEntity = builder.AddEntityType(typeof(DiscoveryPrincipalEntity));
            PropertyInfo propertyInfo = typeof(DiscoveryPrincipalEntity).GetProperty(propertyName);
            principalEntity.HasKey(propertyInfo);

            EntityTypeConfiguration dependentEntity = builder.AddEntityType(typeof(DiscoveryDependentEntity));
            PropertyInfo expectPropertyInfo = typeof(DiscoveryDependentEntity).GetProperty("DiscoveryPrincipalEntity" + propertyName);
            var property = dependentEntity.AddProperty(expectPropertyInfo);

            PropertyInfo navigationPropertyInfo = typeof(DiscoveryDependentEntity).GetProperty("Principal");
            NavigationPropertyConfiguration navigation = dependentEntity.AddNavigationProperty(navigationPropertyInfo, multiplicity);

            // Act
            new ForeignKeyDiscoveryConvention().Apply(navigation, dependentEntity, builder);

            // Assert
            PropertyInfo actualPropertyInfo = Assert.Single(navigation.DependentProperties);
            Assert.Same(expectPropertyInfo, actualPropertyInfo);

            PropertyInfo principalProperty = Assert.Single(navigation.PrincipalProperties);
            Assert.Equal(propertyName, principalProperty.Name);

            Assert.Equal(optional, property.OptionalProperty);
        }

        [Fact]
        public void Apply_KeyNameConventions_Works()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

            EntityTypeConfiguration principalEntity = builder.AddEntityType(typeof(DiscoveryPrincipalEntity));
            PropertyInfo propertyInfo = typeof(DiscoveryPrincipalEntity).GetProperty("DiscoveryPrincipalEntityId");
            principalEntity.HasKey(propertyInfo);

            EntityTypeConfiguration dependentEntity = builder.AddEntityType(typeof(DiscoveryDependentEntity));
            PropertyInfo expectPropertyInfo = typeof(DiscoveryDependentEntity).GetProperty("DiscoveryPrincipalEntityId");
            dependentEntity.AddProperty(expectPropertyInfo);

            PropertyInfo navigationPropertyInfo = typeof(DiscoveryDependentEntity).GetProperty("Principal");
            NavigationPropertyConfiguration navigation = dependentEntity.AddNavigationProperty(navigationPropertyInfo,
                EdmMultiplicity.One);
            navigation.AddedExplicitly = false;

            // Act
            new ForeignKeyDiscoveryConvention().Apply(navigation, dependentEntity, builder);

            // Assert
            PropertyInfo actualPropertyInfo = Assert.Single(navigation.DependentProperties);
            Assert.Same(expectPropertyInfo, actualPropertyInfo);

            PropertyInfo principalProperty = Assert.Single(navigation.PrincipalProperties);
            Assert.Equal("DiscoveryPrincipalEntityId", principalProperty.Name);
        }

        private class DiscoveryPrincipalEntity
        {
            public string Key1 { get; set; }

            public Guid Key2 { get; set; }

            public DateTimeOffset Key3 { get; set; }

            public double DiscoveryPrincipalEntityId { get; set; }
        }

        private class DiscoveryDependentEntity
        {
            public string DiscoveryPrincipalEntityKey1 { get; set; }

            public Guid DiscoveryPrincipalEntityKey2 { get; set; }

            public DateTimeOffset DiscoveryPrincipalEntityKey3 { get; set; }

            public double DiscoveryPrincipalEntityId { get; set; }

            public DiscoveryPrincipalEntity Principal { get; set; }
        }
    }
}
