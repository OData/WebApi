//-----------------------------------------------------------------------------
// <copyright file="ForeignKeyAttributeConventionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Builder.Conventions.Attributes;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder.Conventions.Attributes
{
    public class ForeignKeyAttributeConventionTest
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            ExceptionAssert.DoesNotThrow(() => new ForeignKeyAttributeConvention());
        }

        [Fact]
        public void Apply_SingleForeignKeyOnNavigationProperty_Works()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();

            builder.EntityType<PrincipalEntity>().HasKey(p => p.PrincipalStringId);
            EntityTypeConfiguration dependentEntity = builder.AddEntityType(typeof(SingleDependentEntity));

            PropertyInfo expectPrincipal = typeof(PrincipalEntity).GetProperty("PrincipalStringId");
            PropertyInfo expectDependent = typeof(SingleDependentEntity).GetProperty("PrincipalId");
            PrimitivePropertyConfiguration primitiveProperty = dependentEntity.AddProperty(expectDependent);

            PropertyInfo propertyInfo = typeof(SingleDependentEntity).GetProperty("Principal");
            NavigationPropertyConfiguration navigation = dependentEntity.AddNavigationProperty(propertyInfo,
                EdmMultiplicity.One);
            navigation.AddedExplicitly = false;

            // Act
            new ForeignKeyAttributeConvention().Apply(navigation, dependentEntity, builder);

            // Assert
            PropertyInfo actualPropertyInfo = Assert.Single(navigation.DependentProperties);
            Assert.Same(expectDependent, actualPropertyInfo);

            actualPropertyInfo = Assert.Single(navigation.PrincipalProperties);
            Assert.Same(expectPrincipal, actualPropertyInfo);

            Assert.False(primitiveProperty.OptionalProperty);
        }

        private class PrincipalEntity
        {
            public string PrincipalStringId { get; set; }

            public int PrincipalIntId { get; set; }
        }

        private class SingleDependentEntity
        {
            public string PrincipalId { get; set; }

            [ForeignKey("PrincipalId")]
            public PrincipalEntity Principal { get; set; }
        }


        [Fact]
        public void Apply_SingleNullableForeignKeyOnNavigationProperty_Works()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();

            builder.EntityType<PrincipalEntity>().HasKey(p => p.PrincipalIntId);
            EntityTypeConfiguration dependentEntity = builder.AddEntityType(typeof(SingleNullableDependentEntity));

            PropertyInfo expectPrincipal = typeof(PrincipalEntity).GetProperty("PrincipalIntId");
            PropertyInfo expectDependent = typeof(SingleNullableDependentEntity).GetProperty("PrincipalId");
            PrimitivePropertyConfiguration primitiveProperty = dependentEntity.AddProperty(expectDependent);

            PropertyInfo propertyInfo = typeof(SingleNullableDependentEntity).GetProperty("Principal");
            NavigationPropertyConfiguration navigation = dependentEntity.AddNavigationProperty(propertyInfo,
                EdmMultiplicity.ZeroOrOne);
            navigation.AddedExplicitly = false;

            // Act
            new ForeignKeyAttributeConvention().Apply(navigation, dependentEntity, builder);

            // Assert
            PropertyInfo actualPropertyInfo = Assert.Single(navigation.DependentProperties);
            Assert.Same(expectDependent, actualPropertyInfo);

            actualPropertyInfo = Assert.Single(navigation.PrincipalProperties);
            Assert.Same(expectPrincipal, actualPropertyInfo);

            Assert.True(primitiveProperty.OptionalProperty);
        }

        private class SingleNullableDependentEntity
        {
            public int? PrincipalId { get; set; }

            [ForeignKey("PrincipalId")]
            public PrincipalEntity Principal { get; set; }
        }

        [Fact]
        public void Apply_SingleForeignKeyOnForeignKeyProperty_Works()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<PrincipalEntity>().HasKey(p => p.PrincipalStringId);
            EntityTypeConfiguration dependentEntity = builder.AddEntityType(typeof(SingleDependentEntity2));

            PropertyInfo expectPrincipal = typeof(PrincipalEntity).GetProperty("PrincipalStringId");
            PropertyInfo expectDependent = typeof(SingleDependentEntity2).GetProperty("PrincipalId");
            PrimitivePropertyConfiguration primitiveProperty = dependentEntity.AddProperty(expectDependent);

            PropertyInfo propertyInfo = typeof(SingleDependentEntity2).GetProperty("Principal");
            NavigationPropertyConfiguration navigation = dependentEntity.AddNavigationProperty(propertyInfo,
                EdmMultiplicity.ZeroOrOne);
            navigation.AddedExplicitly = false;

            // Act
            new ForeignKeyAttributeConvention().Apply(primitiveProperty, dependentEntity, builder);

            // Assert
            PropertyInfo actualPropertyInfo = Assert.Single(navigation.DependentProperties);
            Assert.Same(expectDependent, actualPropertyInfo);

            actualPropertyInfo = Assert.Single(navigation.PrincipalProperties);
            Assert.Same(expectPrincipal, actualPropertyInfo);

            Assert.True(primitiveProperty.OptionalProperty);
        }

        private class SingleDependentEntity2
        {
            [ForeignKey("Principal")]
            public string PrincipalId { get; set; }

            public PrincipalEntity Principal { get; set; }
        }

        [Fact]
        public void Apply_MultiForeignKeysOnNavigationProperty_Works()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<PrincipalEntity>().HasKey(p => new { p.PrincipalStringId, p.PrincipalIntId });
            EntityTypeConfiguration dependentEntity = builder.AddEntityType(typeof(MultiDependentEntity));

            PropertyInfo expectPropertyInfo1 = typeof(MultiDependentEntity).GetProperty("PrincipalId1");
            dependentEntity.AddProperty(expectPropertyInfo1);

            PropertyInfo expectPropertyInfo2 = typeof(MultiDependentEntity).GetProperty("PrincipalId2");
            dependentEntity.AddProperty(expectPropertyInfo2);

            PropertyInfo propertyInfo = typeof(MultiDependentEntity).GetProperty("Principal");
            NavigationPropertyConfiguration navigation = dependentEntity.AddNavigationProperty(propertyInfo,
                EdmMultiplicity.One);
            navigation.AddedExplicitly = false;

            // Act
            new ForeignKeyAttributeConvention().Apply(navigation, dependentEntity, builder);

            // Assert
            Assert.Equal(2, navigation.DependentProperties.Count());
            Assert.Same(expectPropertyInfo1, navigation.DependentProperties.First());
            Assert.Same(expectPropertyInfo2, navigation.DependentProperties.Last());

            Assert.Equal(2, navigation.PrincipalProperties.Count());
            Assert.Equal("PrincipalIntId", navigation.PrincipalProperties.First().Name);
            Assert.Equal("PrincipalStringId", navigation.PrincipalProperties.Last().Name);
        }

        private class MultiDependentEntity
        {
            public int PrincipalId1 { get; set; }

            public string PrincipalId2 { get; set; }

            [ForeignKey("PrincipalId1 , PrincipalId2 ")]
            public PrincipalEntity Principal { get; set; }
        }

        [Fact]
        public void Apply_MultiForeignKeysOnForeignKeyProperty_Works()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<PrincipalEntity>().HasKey(p => new { p.PrincipalStringId, p.PrincipalIntId });
            EntityTypeConfiguration dependentEntity = builder.AddEntityType(typeof(MultiDependentEntity2));

            PropertyInfo expectPropertyInfo1 = typeof(MultiDependentEntity2).GetProperty("PrincipalId1");
            PrimitivePropertyConfiguration propertyConfig1 = dependentEntity.AddProperty(expectPropertyInfo1);

            PropertyInfo expectPropertyInfo2 = typeof(MultiDependentEntity2).GetProperty("PrincipalId2");
            PrimitivePropertyConfiguration propertyConfig2 = dependentEntity.AddProperty(expectPropertyInfo2);

            PropertyInfo propertyInfo = typeof(MultiDependentEntity2).GetProperty("Principal");
            NavigationPropertyConfiguration navigation = dependentEntity.AddNavigationProperty(propertyInfo,
                EdmMultiplicity.One);
            navigation.AddedExplicitly = false;

            // Act
            ForeignKeyAttributeConvention convention = new ForeignKeyAttributeConvention();
            convention.Apply(propertyConfig1, dependentEntity, builder);
            convention.Apply(propertyConfig2, dependentEntity, builder);

            // Assert
            Assert.Equal(2, navigation.DependentProperties.Count());
            Assert.Same(expectPropertyInfo1, navigation.DependentProperties.First());
            Assert.Same(expectPropertyInfo2, navigation.DependentProperties.Last());

            Assert.Equal(2, navigation.PrincipalProperties.Count());
            Assert.Equal("PrincipalIntId", navigation.PrincipalProperties.First().Name);
            Assert.Equal("PrincipalStringId", navigation.PrincipalProperties.Last().Name);
        }

        private class MultiDependentEntity2
        {
            [ForeignKey("Principal")]
            public int PrincipalId1 { get; set; }

            [ForeignKey("Principal")]
            public string PrincipalId2 { get; set; }

            public PrincipalEntity Principal { get; set; }
        }
    }
}
