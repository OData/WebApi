// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Builder.Conventions
{
    public class ForeignKeyDiscoveryConventionTest
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            Assert.DoesNotThrow(() => new ForeignKeyDiscoveryConvention());
        }

        [Theory]
        [InlineData("Key1")]
        [InlineData("Key2")]
        [InlineData("Key3")]
        public void Apply_EntityTypeNamePlusKeyNameConventions_Works(string propertyName)
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            EntityTypeConfiguration principalEntity = builder.AddEntity(typeof(DiscoveryPrincipalEntity));
            PropertyInfo propertyInfo = typeof(DiscoveryPrincipalEntity).GetProperty(propertyName);
            principalEntity.HasKey(propertyInfo);

            EntityTypeConfiguration dependentEntity = builder.AddEntity(typeof(DiscoveryDependentEntity));
            PropertyInfo expectPropertyInfo = typeof(DiscoveryDependentEntity).GetProperty("DiscoveryPrincipalEntity" + propertyName);
            dependentEntity.AddProperty(expectPropertyInfo);

            PropertyInfo navigationPropertyInfo = typeof(DiscoveryDependentEntity).GetProperty("Principal");
            NavigationPropertyConfiguration navigation = dependentEntity.AddNavigationProperty(navigationPropertyInfo,
                EdmMultiplicity.One);
            navigation.AddedExplicitly = false;

            // Act
            new ForeignKeyDiscoveryConvention().Apply(navigation, dependentEntity);

            // Assert
            PropertyInfo actualPropertyInfo = Assert.Single(navigation.DependentProperties);
            Assert.Same(expectPropertyInfo, actualPropertyInfo);

            PropertyInfo principalProperty = Assert.Single(navigation.PrincipalProperties);
            Assert.Equal(propertyName, principalProperty.Name);
        }

        [Fact]
        public void Apply_KeyNameConventions_Works()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            EntityTypeConfiguration principalEntity = builder.AddEntity(typeof(DiscoveryPrincipalEntity));
            PropertyInfo propertyInfo = typeof(DiscoveryPrincipalEntity).GetProperty("DiscoveryPrincipalEntityId");
            principalEntity.HasKey(propertyInfo);

            EntityTypeConfiguration dependentEntity = builder.AddEntity(typeof(DiscoveryDependentEntity));
            PropertyInfo expectPropertyInfo = typeof(DiscoveryDependentEntity).GetProperty("DiscoveryPrincipalEntityId");
            dependentEntity.AddProperty(expectPropertyInfo);

            PropertyInfo navigationPropertyInfo = typeof(DiscoveryDependentEntity).GetProperty("Principal");
            NavigationPropertyConfiguration navigation = dependentEntity.AddNavigationProperty(navigationPropertyInfo,
                EdmMultiplicity.One);
            navigation.AddedExplicitly = false;

            // Act
            new ForeignKeyDiscoveryConvention().Apply(navigation, dependentEntity);

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
