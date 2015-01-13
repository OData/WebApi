// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    public class ForeignKeyAttributeConventionTest
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            Assert.DoesNotThrow(() => new ForeignKeyAttributeConvention());
        }

        [Fact]
        public void Apply_SingleForeignKeyOnNavigationProperty_Works()
        {
            // Arrange
            Type dependentType = typeof(SingleDependentEntity);

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.Entity<PrincipalEntity>().HasKey(p => p.PrincipalStringId);

            EntityTypeConfiguration dependentEntity = builder.AddEntity(dependentType);

            PropertyInfo expectPropertyInfo = dependentType.GetProperty("PrincipalStringKey");
            PrimitivePropertyConfiguration primitiveProperty = dependentEntity.AddProperty(expectPropertyInfo);

            PropertyInfo propertyInfo = dependentType.GetProperty("Principal");
            NavigationPropertyConfiguration navigation = dependentEntity.AddNavigationProperty(propertyInfo,
                EdmMultiplicity.One);
            navigation.AddedExplicitly = false;

            // Act
            new ForeignKeyAttributeConvention().Apply(navigation, dependentEntity);

            // Assert
            PropertyInfo actualPropertyInfo = Assert.Single(navigation.DependentProperties);
            Assert.Same(expectPropertyInfo, actualPropertyInfo);

            Assert.Equal("PrincipalStringId", Assert.Single(navigation.PrincipalProperties).Name);
            Assert.False(primitiveProperty.OptionalProperty);
        }

        [Fact]
        public void Apply_SingleForeignKeyOnForeignKeyProperty_Works()
        {
            // Arrange
            Type dependentType = typeof(SingleDependentEntity2);

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.Entity<PrincipalEntity>().HasKey(p => p.PrincipalIntId);

            EntityTypeConfiguration dependentEntity = builder.AddEntity(dependentType);

            PropertyInfo expectPropertyInfo = dependentType.GetProperty("PrincipalId");
            PrimitivePropertyConfiguration primitiveProperty = dependentEntity.AddProperty(expectPropertyInfo);

            PropertyInfo propertyInfo = dependentType.GetProperty("Principal");
            NavigationPropertyConfiguration navigation = dependentEntity.AddNavigationProperty(propertyInfo,
                EdmMultiplicity.ZeroOrOne);
            navigation.AddedExplicitly = false;

            // Act
            new ForeignKeyAttributeConvention().Apply(primitiveProperty, dependentEntity);

            // Assert
            PropertyInfo actualPropertyInfo = Assert.Single(navigation.DependentProperties);
            Assert.Same(expectPropertyInfo, actualPropertyInfo);

            Assert.Equal("PrincipalIntId", Assert.Single(navigation.PrincipalProperties).Name);
            Assert.False(primitiveProperty.OptionalProperty);
        }

        [Fact]
        public void Apply_MultiForeignKeysOnNavigationProperty_Works()
        {
            // Arrange
            Type dependentType = typeof(MultiDependentEntity);

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.Entity<PrincipalEntity>().HasKey(p => new { p.PrincipalIntId, p.PrincipalStringId });

            EntityTypeConfiguration dependentEntity = builder.AddEntity(dependentType);

            PropertyInfo expectPropertyInfo1 = dependentType.GetProperty("PrincipalId1");
            dependentEntity.AddProperty(expectPropertyInfo1);

            PropertyInfo expectPropertyInfo2 = dependentType.GetProperty("PrincipalId2");
            dependentEntity.AddProperty(expectPropertyInfo2);

            PropertyInfo propertyInfo = typeof(MultiDependentEntity).GetProperty("Principal");
            NavigationPropertyConfiguration navigation = dependentEntity.AddNavigationProperty(propertyInfo,
                EdmMultiplicity.One);
            navigation.AddedExplicitly = false;

            // Act
            new ForeignKeyAttributeConvention().Apply(navigation, dependentEntity);

            // Assert
            Assert.Equal(2, navigation.DependentProperties.Count());
            Assert.Same(expectPropertyInfo1, navigation.DependentProperties.First());
            Assert.Same(expectPropertyInfo2, navigation.DependentProperties.Last());

            Assert.Equal(2, navigation.PrincipalProperties.Count());
            Assert.Equal("PrincipalIntId", navigation.PrincipalProperties.First().Name);
            Assert.Equal("PrincipalStringId", navigation.PrincipalProperties.Last().Name);
        }

        [Fact]
        public void Apply_MultiForeignKeysOnForeignKeyProperty_Works()
        {
            // Arrange
            Type dependentType = typeof(MultiDependentEntity2);
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.Entity<PrincipalEntity>().HasKey(p => new { p.PrincipalIntId, p.PrincipalStringId });

            EntityTypeConfiguration dependentEntity = builder.AddEntity(dependentType);

            PropertyInfo expectPropertyInfo1 = dependentType.GetProperty("PrincipalId1");
            PrimitivePropertyConfiguration propertyConfig1 = dependentEntity.AddProperty(expectPropertyInfo1);

            PropertyInfo expectPropertyInfo2 = dependentType.GetProperty("PrincipalId2");
            PrimitivePropertyConfiguration propertyConfig2 = dependentEntity.AddProperty(expectPropertyInfo2);

            PropertyInfo propertyInfo = dependentType.GetProperty("Principal");
            NavigationPropertyConfiguration navigation = dependentEntity.AddNavigationProperty(propertyInfo,
                EdmMultiplicity.One);
            navigation.AddedExplicitly = false;

            // Act
            ForeignKeyAttributeConvention convention = new ForeignKeyAttributeConvention();
            convention.Apply(propertyConfig1, dependentEntity);
            convention.Apply(propertyConfig2, dependentEntity);

            // Assert
            Assert.Equal(2, navigation.DependentProperties.Count());
            Assert.Same(expectPropertyInfo1, navigation.DependentProperties.First());
            Assert.Same(expectPropertyInfo2, navigation.DependentProperties.Last());

            Assert.Equal(2, navigation.PrincipalProperties.Count());
            Assert.Equal("PrincipalIntId", navigation.PrincipalProperties.First().Name);
            Assert.Equal("PrincipalStringId", navigation.PrincipalProperties.Last().Name);
        }

        [Fact]
        public void Apply_IgnoreEmptyAndUnknownForeignKeyProperty()
        {
            // Arrange
            Type dependentType = typeof(InvalidDependentEntity);

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.Entity<PrincipalEntity>().HasKey(p => new { p.PrincipalIntId, p.PrincipalStringId });

            EntityTypeConfiguration dependentEntity = builder.AddEntity(dependentType);
            PropertyInfo expectPropertyInfo1 = dependentType.GetProperty("PrincipalId1");
            dependentEntity.AddProperty(expectPropertyInfo1);

            PropertyInfo expectPropertyInfo2 = dependentType.GetProperty("PrincipalId2");
            dependentEntity.AddProperty(expectPropertyInfo2);

            PropertyInfo propertyInfo = dependentType.GetProperty("Principal");
            NavigationPropertyConfiguration navigation = dependentEntity.AddNavigationProperty(propertyInfo,
                EdmMultiplicity.One);
            navigation.AddedExplicitly = false;

            // Act & Assert
            new ForeignKeyAttributeConvention().Apply(navigation, dependentEntity);

            // Assert
            Assert.Equal(2, navigation.DependentProperties.Count());
            Assert.Same(expectPropertyInfo1, navigation.DependentProperties.First());
            Assert.Same(expectPropertyInfo2, navigation.DependentProperties.Last());
        }

        private class PrincipalEntity
        {
            public string PrincipalStringId { get; set; }

            public int PrincipalIntId { get; set; }
        }

        private class SingleDependentEntity
        {
            public string PrincipalStringKey { get; set; }

            [ForeignKey("PrincipalStringKey")]
            public PrincipalEntity Principal { get; set; }
        }

        private class SingleDependentEntity2
        {
            [ForeignKey("Principal")]
            public int PrincipalId { get; set; }

            public PrincipalEntity Principal { get; set; }
        }

        private class MultiDependentEntity
        {
            public int PrincipalId1 { get; set; }

            public string PrincipalId2 { get; set; }

            [ForeignKey("PrincipalId1,PrincipalId2")]
            public PrincipalEntity Principal { get; set; }
        }

        private class MultiDependentEntity2
        {
            [ForeignKey("Principal")]
            public int PrincipalId1 { get; set; }

            [ForeignKey("Principal")]
            public string PrincipalId2 { get; set; }

            public PrincipalEntity Principal { get; set; }
        }

        private class InvalidDependentEntity
        {
            public int PrincipalId1 { get; set; }

            public string PrincipalId2 { get; set; }

            [ForeignKey("PrincipalId1,PrincipalId#,,PrincipalId2")]
            public PrincipalEntity Principal { get; set; }
        }
    }
}
