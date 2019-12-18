// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
{
    public class NavigationPropertyConfigurationTest
    {
        [Fact]
        public void Ctor_Throws_ArgumentNull_Property()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new NavigationPropertyConfiguration(property: null, multiplicity: EdmMultiplicity.One, declaringType: new EntityTypeConfiguration()),
                "property");
        }

        [Fact]
        public void Ctor_ThrowsArgument_IfMultiplicityIsManyAndPropertyIsNotCollection()
        {
            MockType mockEntity = new MockType().Property<int>("ID");

#if NETCOREAPP3_0
            ExceptionAssert.Throws<ArgumentException>(
                () => new NavigationPropertyConfiguration(mockEntity.GetProperty("ID"), EdmMultiplicity.Many, new EntityTypeConfiguration()),
                "The property 'ID' on the type 'T' is being configured as a Many-to-Many navigation property. Many to Many navigation properties must be collections. (Parameter 'property')");
#else
            ExceptionAssert.Throws<ArgumentException>(
                () => new NavigationPropertyConfiguration(mockEntity.GetProperty("ID"), EdmMultiplicity.Many, new EntityTypeConfiguration()),
                "The property 'ID' on the type 'T' is being configured as a Many-to-Many navigation property. Many to Many navigation properties must be collections.\r\nParameter name: property");
#endif
        }

        [Fact]
        public void Ctor_MultiplicityProperty_IsInitializedProperly()
        {
            NavigationPropertyConfiguration navigationProperty = new NavigationPropertyConfiguration(new MockPropertyInfo(), EdmMultiplicity.ZeroOrOne, new EntityTypeConfiguration());

            Assert.Equal(EdmMultiplicity.ZeroOrOne, navigationProperty.Multiplicity);
        }

        [Fact]
        public void Property_DeclaringType_Returns_DeclaredType()
        {
            Mock<EntityTypeConfiguration> mockDeclaringType = new Mock<EntityTypeConfiguration>();
            NavigationPropertyConfiguration navigationProperty = new NavigationPropertyConfiguration(new MockPropertyInfo(), EdmMultiplicity.One, mockDeclaringType.Object);

            Assert.Equal(mockDeclaringType.Object, navigationProperty.DeclaringType);
        }

        [Fact]
        public void Optional_ModifiesMultiplicityToZeroOrOne()
        {
            NavigationPropertyConfiguration navigationProperty = new NavigationPropertyConfiguration(new MockPropertyInfo(), EdmMultiplicity.One, new EntityTypeConfiguration());

            navigationProperty.Optional();

            Assert.Equal(EdmMultiplicity.ZeroOrOne, navigationProperty.Multiplicity);
        }

        [Fact]
        public void Required_ModifiesMultiplicityToOne()
        {
            NavigationPropertyConfiguration navigationProperty = new NavigationPropertyConfiguration(new MockPropertyInfo(), EdmMultiplicity.ZeroOrOne, new EntityTypeConfiguration());

            navigationProperty.Required();

            Assert.Equal(EdmMultiplicity.One, navigationProperty.Multiplicity);
        }

        [Fact]
        public void Required_Throws_IfMultiplicityIsMany()
        {
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(new MockPropertyInfo(typeof(int[]), "P"), EdmMultiplicity.Many, new EntityTypeConfiguration());

            ExceptionAssert.Throws<InvalidOperationException>(
                () => navigationProperty.Required(),
                "Cannot change multiplicity of the collection navigation property 'P'.");
        }

        [Fact]
        public void Optional_Throws_IfMultiplicityIsMany()
        {
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(new MockPropertyInfo(typeof(int[]), "P"), EdmMultiplicity.Many, new EntityTypeConfiguration());

            ExceptionAssert.Throws<InvalidOperationException>(
                () => navigationProperty.Optional(),
                "Cannot change multiplicity of the collection navigation property 'P'.");
        }

        [Fact]
        public void ContainsTarget_DefaultsToFalse()
        {
            // Arrange & Act
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(
                    new MockPropertyInfo(),
                    EdmMultiplicity.ZeroOrOne,
                    new EntityTypeConfiguration());

            // Assert
            Assert.False(navigationProperty.ContainsTarget);
        }

        [Fact]
        public void Contained_ModifiesContainsTargetToTrue()
        {
            // Arrange
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(
                    new MockPropertyInfo(),
                    EdmMultiplicity.ZeroOrOne,
                    new EntityTypeConfiguration());

            // Act
            navigationProperty.Contained();

            // Assert
            Assert.True(navigationProperty.ContainsTarget);
        }

        [Fact]
        public void NonContained_ModifiesContainsTargetToFalse()
        {
            // Arrange
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(
                    new MockPropertyInfo(),
                    EdmMultiplicity.ZeroOrOne,
                    new EntityTypeConfiguration());

            // Act
            navigationProperty.Contained().NonContained();

            // Assert
            Assert.False(navigationProperty.ContainsTarget);
        }

        [Fact]
        public void DependentProperties_ReturnsEmpty_ByDefault()
        {
            // Arrange
            NavigationPropertyConfiguration navigationProperty = new NavigationPropertyConfiguration(
                    new MockPropertyInfo(), EdmMultiplicity.ZeroOrOne, new EntityTypeConfiguration());

            // Act & Assert
            Assert.Empty(navigationProperty.DependentProperties);
        }

        [Fact]
        public void PrincipalProperties_ReturnsEmpty_ByDefault()
        {
            // Arrange
            NavigationPropertyConfiguration navigationProperty = new NavigationPropertyConfiguration(
                    new MockPropertyInfo(), EdmMultiplicity.ZeroOrOne, new EntityTypeConfiguration());

            // Act & Assert
            Assert.Empty(navigationProperty.PrincipalProperties);
        }

        [Fact]
        public void OnDeleteAction_Returns_NoneByDefault()
        {
            // Arrange
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(new MockPropertyInfo(), EdmMultiplicity.ZeroOrOne,
                    new EntityTypeConfiguration());

            // Act & Assert
            Assert.Equal(EdmOnDeleteAction.None, navigationProperty.OnDeleteAction);
        }

        [Fact]
        public void CascadeOnDelete_ModifiesOnDelete()
        {
            // Arrange
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(new MockPropertyInfo(), EdmMultiplicity.ZeroOrOne,
                    new EntityTypeConfiguration());

            // Act
            navigationProperty.CascadeOnDelete();

            // Assert
            Assert.Equal(EdmOnDeleteAction.Cascade, navigationProperty.OnDeleteAction);
        }

        [Fact]
        public void CascadeOnDelete_ModifiesOnDelete_WithParameter()
        {
            // Arrange
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(new MockPropertyInfo(), EdmMultiplicity.ZeroOrOne,
                    new EntityTypeConfiguration());

            // Act
            navigationProperty.CascadeOnDelete(cascade: false);

            // Assert
            Assert.Equal(EdmOnDeleteAction.None, navigationProperty.OnDeleteAction);
        }

        [Fact]
        public void HasConstraint_CanSetDependentAndPrincipalProperty()
        {
            // Arrange
            PropertyInfo principalPropertyInfo = typeof(Principal).GetProperty("PrincipalKey1");
            PropertyInfo dependentPropertyInfo = typeof(Dependent).GetProperty("DependentKey1");
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.AddEntityType(typeof(Principal));
            EntityTypeConfiguration dependentEntity = builder.AddEntityType(typeof(Dependent));
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(typeof(Dependent).GetProperty("Principal"), EdmMultiplicity.One,
                    dependentEntity);

            // Act
            navigationProperty.HasConstraint(dependentPropertyInfo, principalPropertyInfo);

            // Assert
            Assert.Same(principalPropertyInfo, Assert.Single(navigationProperty.PrincipalProperties));
            Assert.Same(dependentPropertyInfo, Assert.Single(navigationProperty.DependentProperties));
        }

        [Fact]
        public void HasConstraint_CanSetDependentAndPrincipalProperty_OnlyOnceForTheSameConstraint()
        {
            // Arrange
            PropertyInfo principalPropertyInfo = typeof(Principal).GetProperty("PrincipalKey1");
            PropertyInfo dependentPropertyInfo = typeof(Dependent).GetProperty("DependentKey1");
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.AddEntityType(typeof(Principal));
            EntityTypeConfiguration dependentEntity = builder.AddEntityType(typeof(Dependent));
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(typeof(Dependent).GetProperty("Principal"), EdmMultiplicity.One,
                    dependentEntity);

            // Act
            navigationProperty.HasConstraint(dependentPropertyInfo, principalPropertyInfo);
            navigationProperty.HasConstraint(dependentPropertyInfo, principalPropertyInfo);
            navigationProperty.HasConstraint(new KeyValuePair<PropertyInfo, PropertyInfo>(dependentPropertyInfo,
                principalPropertyInfo));

            // Assert
            Assert.Single(navigationProperty.PrincipalProperties);
            Assert.Single(navigationProperty.DependentProperties);
        }

        [Fact]
        public void HasConstraint_ThrowsArgumentNull_ForNullDependentPropertyInfo()
        {
            // Arrange
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(new MockPropertyInfo(), EdmMultiplicity.ZeroOrOne,
                    new EntityTypeConfiguration());

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => navigationProperty.HasConstraint(dependentPropertyInfo: null,
                    principalPropertyInfo: new MockPropertyInfo()),
                "dependentPropertyInfo");
        }

        [Fact]
        public void HasConstraint_ThrowsArgumentNull_ForNullPrincipalPropertyInfo()
        {
            // Arrange
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(new MockPropertyInfo(), EdmMultiplicity.ZeroOrOne,
                    new EntityTypeConfiguration());

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => navigationProperty.HasConstraint(new MockPropertyInfo(), principalPropertyInfo: null),
                "principalPropertyInfo");
        }

        [Fact]
        public void HasConstraint_Throws_MultiplicityMany()
        {
            // Arrange
            Mock<EntityTypeConfiguration> entityType = new Mock<EntityTypeConfiguration>();
            entityType.Setup(c => c.ClrType).Returns(typeof(NavigationPropertyConfigurationTest));
            NavigationPropertyConfiguration navigationProperty = new NavigationPropertyConfiguration(
                new MockPropertyInfo(typeof(List<NavigationPropertyConfigurationTest>), "Navigation"),
                EdmMultiplicity.Many, entityType.Object);

            // Act & Assert
            ExceptionAssert.Throws<NotSupportedException>(
                () => navigationProperty.HasConstraint(new MockPropertyInfo(), new MockPropertyInfo()),
                String.Format(SRResources.ReferentialConstraintOnManyNavigationPropertyNotSupported,
                "Navigation", typeof(NavigationPropertyConfigurationTest).FullName));
        }

        [Fact]
        public void HasConstraint_Throws_ReferentialConstraintAlreadyConfigured_Dependent()
        {
            // Arrange
            PropertyInfo principalPropertyInfo = typeof(Principal).GetProperty("PrincipalKey1");
            PropertyInfo dependentPropertyInfo = typeof(Dependent).GetProperty("DependentKey1");
            PropertyInfo otherPrincipalPropertyInfo = typeof(Principal).GetProperty("PrincipalKey2");

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.AddEntityType(typeof(Principal));
            EntityTypeConfiguration dependentEntity = builder.AddEntityType(typeof(Dependent));
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(typeof(Dependent).GetProperty("Principal"), EdmMultiplicity.One,
                    dependentEntity);

            navigationProperty.HasConstraint(dependentPropertyInfo, principalPropertyInfo);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => navigationProperty.HasConstraint(dependentPropertyInfo, otherPrincipalPropertyInfo),
                String.Format(SRResources.ReferentialConstraintAlreadyConfigured,
                "dependent", "DependentKey1", "principal", "PrincipalKey1"));
        }

        [Fact]
        public void HasConstraint_Throws_ReferentialConstraintAlreadyConfigured_Principal()
        {
            // Arrange
            PropertyInfo principalPropertyInfo = typeof(Principal).GetProperty("PrincipalKey1");
            PropertyInfo dependentPropertyInfo = typeof(Dependent).GetProperty("DependentKey1");
            PropertyInfo otherPrincipalPropertyInfo = typeof(Dependent).GetProperty("DependentKey2");

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.AddEntityType(typeof(Principal));
            EntityTypeConfiguration dependentEntity = builder.AddEntityType(typeof(Dependent));
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(typeof(Dependent).GetProperty("Principal"), EdmMultiplicity.One,
                    dependentEntity);

            navigationProperty.HasConstraint(dependentPropertyInfo, principalPropertyInfo);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => navigationProperty.HasConstraint(otherPrincipalPropertyInfo, principalPropertyInfo),
                String.Format(SRResources.ReferentialConstraintAlreadyConfigured,
                "principal", "PrincipalKey1", "dependent", "DependentKey1"));
        }

        [Fact]
        public void HasConstraint_Throws_DependentAndPrincipalTypeNotMatch()
        {
            // Arrange
            PropertyInfo principalPropertyInfo = typeof(Principal).GetProperty("PrincipalKey2");
            PropertyInfo dependentPropertyInfo = typeof(Dependent).GetProperty("DependentKey1");

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.AddEntityType(typeof(Principal));
            EntityTypeConfiguration dependentEntity = builder.AddEntityType(typeof(Dependent));
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(typeof(Dependent).GetProperty("Principal"), EdmMultiplicity.One,
                    dependentEntity);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => navigationProperty.HasConstraint(dependentPropertyInfo, principalPropertyInfo),
                String.Format(SRResources.DependentAndPrincipalTypeNotMatch, "System.Int32", "System.String"));
        }

        [Fact]
        public void HasConstraint_Throws_ReferentialConstraintPropertyTypeNotValid()
        {
            // Arrange
            PropertyInfo principalPropertyInfo = typeof(Principal).GetProperty("MockPrincipal");
            PropertyInfo dependentPropertyInfo = typeof(Dependent).GetProperty("MockDependent");

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.AddEntityType(typeof(Principal));
            EntityTypeConfiguration dependentEntity = builder.AddEntityType(typeof(Dependent));
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(typeof(Dependent).GetProperty("Principal"), EdmMultiplicity.One,
                    dependentEntity);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => navigationProperty.HasConstraint(dependentPropertyInfo, principalPropertyInfo),
                String.Format(SRResources.ReferentialConstraintPropertyTypeNotValid, "Microsoft.AspNet.OData.Test.Common.MockType"));
        }

        class Principal
        {
            public int PrincipalKey1 { get; set; }

            public string PrincipalKey2 { get; set; }

            public MockType MockPrincipal { get; set; }
        }

        class Dependent
        {
            public int DependentKey1 { get; set; }

            public string DependentKey2 { get; set; }

            public MockType MockDependent { get; set; }

            public Principal Principal { get; set; }
        }
    }
}
