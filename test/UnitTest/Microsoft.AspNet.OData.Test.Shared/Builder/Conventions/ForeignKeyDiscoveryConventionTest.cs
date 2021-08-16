//-----------------------------------------------------------------------------
// <copyright file="ForeignKeyDiscoveryConventionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Reflection;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Builder.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder.Conventions
{
    public class ForeignKeyDiscoveryConventionTest
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            ExceptionAssert.DoesNotThrow(() => new ForeignKeyDiscoveryConvention());
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
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();

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
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();

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
