// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder
{
    public class NavigationPropertyConfigurationTest
    {
        [Fact]
        public void Ctor_Throws_ArgumentNull_Property()
        {
            Assert.ThrowsArgumentNull(
                () => new NavigationPropertyConfiguration(property: null, multiplicity: EdmMultiplicity.One, declaringType: new EntityTypeConfiguration()),
                "property");
        }

        [Fact]
        public void Ctor_ThrowsArgument_IfMultiplicityIsManyAndPropertyIsNotCollection()
        {
            MockType mockEntity = new MockType().Property<int>("ID");

            Assert.Throws<ArgumentException>(
                () => new NavigationPropertyConfiguration(mockEntity.GetProperty("ID"), EdmMultiplicity.Many, new EntityTypeConfiguration()),
                "The property 'ID' on the type 'T' is being configured as a Many-to-Many navigation property. Many to Many navigation properties must be collections.\r\nParameter name: property");
        }

        [Fact]
        public void Ctor_MultiplicityProperty_IsInitializedProperly()
        {
            NavigationPropertyConfiguration navigationProperty = new NavigationPropertyConfiguration(new MockPropertyInfo(), EdmMultiplicity.ZeroOrOne, new EntityTypeConfiguration());

            Assert.Equal(EdmMultiplicity.ZeroOrOne, navigationProperty.Multiplicity);
        }

        [Fact]
        public void Property_DeclaringEntityType_Returns_DeclaredType()
        {
            Mock<EntityTypeConfiguration> mockDeclaringType = new Mock<EntityTypeConfiguration>();
            NavigationPropertyConfiguration navigationProperty = new NavigationPropertyConfiguration(new MockPropertyInfo(), EdmMultiplicity.One, mockDeclaringType.Object);

            Assert.Equal(mockDeclaringType.Object, navigationProperty.DeclaringEntityType);
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

            Assert.Throws<InvalidOperationException>(
                () => navigationProperty.Required(),
                "Cannot change multiplicity of the collection navigation property 'P'.");
        }

        [Fact]
        public void Optional_Throws_IfMultiplicityIsMany()
        {
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(new MockPropertyInfo(typeof(int[]), "P"), EdmMultiplicity.Many, new EntityTypeConfiguration());

            Assert.Throws<InvalidOperationException>(
                () => navigationProperty.Optional(),
                "Cannot change multiplicity of the collection navigation property 'P'.");
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
        public void HasConstraint_Throws_NotSupported_ForMany()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration entityType = builder.AddEntity(typeof(Dependent));
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(new MockPropertyInfo(typeof(IList<int>), "Users"),
                    EdmMultiplicity.Many, entityType);

            // Assert & Act
            Assert.Throws<NotSupportedException>(
                () => navigationProperty.HasConstraint(new MockPropertyInfo(), new MockPropertyInfo()),
                String.Format(SRResources.ReferentialConstraintOnManyNavigationPropertyNotSupported,
                    navigationProperty.Name, navigationProperty.DeclaringEntityType.ClrType.FullName));
        }

        [Fact]
        public void HasConstraint_ThrowsArgumentNull_ForNullDependentProperty()
        {
            // Arrange
            Mock<EntityTypeConfiguration> mockDeclaringType = new Mock<EntityTypeConfiguration>();
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(new MockPropertyInfo(), EdmMultiplicity.One,
                    mockDeclaringType.Object);

            // Act & Assert
            Assert.ThrowsArgumentNull(() => navigationProperty.HasConstraint(dependentPropertyInfo: null,
                principalPropertyInfo: new MockPropertyInfo()),
                "dependentPropertyInfo");
        }

        [Fact]
        public void HasConstraint_ThrowsArgumentNull_ForNullPrincipalProperty()
        {
            // Arrange
            Mock<EntityTypeConfiguration> mockDeclaringType = new Mock<EntityTypeConfiguration>();
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(new MockPropertyInfo(), EdmMultiplicity.One,
                    mockDeclaringType.Object);

            // Act & Assert
            Assert.ThrowsArgumentNull(() => navigationProperty.HasConstraint(new MockPropertyInfo(),
                    principalPropertyInfo: null),
                "principalPropertyInfo");
        }

        [Fact]
        public void HasConstraint_Works_SetupReferentialConstraint()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration entity = builder.AddEntity(typeof(Dependent));
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(typeof(Dependent).GetProperty("Principal"),
                    EdmMultiplicity.ZeroOrOne, entity);

            PropertyInfo principalProperty = typeof(Principal).GetProperty("PrincipalKey1");
            PropertyInfo dependentProperty = typeof(Dependent).GetProperty("DependentKey1");

            // Act
            navigationProperty.HasConstraint(dependentProperty, principalProperty);

            // Assert
            PropertyInfo actualDependentInfo = Assert.Single(navigationProperty.DependentProperties);
            Assert.Same(dependentProperty, actualDependentInfo);

            PropertyInfo actualPrincipalInfo = Assert.Single(navigationProperty.PrincipalProperties);
            Assert.Same(principalProperty, actualPrincipalInfo);
        }

        [Fact]
        public void HasConstraint_DoesnotAddTwice_ForSameConstraint()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration entity = builder.AddEntity(typeof(Dependent));
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(typeof(Dependent).GetProperty("Principal"),
                    EdmMultiplicity.ZeroOrOne, entity);

            PropertyInfo principalProperty = typeof(Principal).GetProperty("PrincipalKey1");
            PropertyInfo dependentProperty = typeof(Dependent).GetProperty("DependentKey1");

            // Act
            navigationProperty.HasConstraint(dependentProperty, principalProperty);
            navigationProperty.HasConstraint(dependentProperty, principalProperty);

            // Assert
            PropertyInfo actualInfo = Assert.Single(navigationProperty.DependentProperties);
            Assert.Same(dependentProperty, actualInfo);

            PropertyInfo actualPrincipalInfo = Assert.Single(navigationProperty.PrincipalProperties);
            Assert.Same(principalProperty, actualPrincipalInfo);
        }

        [Fact]
        public void HasConstraint_Throws_ReferentialConstraintAlreadyConfigured_Dependent()
        {
            // Arrange
            PropertyInfo principalPropertyInfo = typeof(Principal).GetProperty("PrincipalKey1");
            PropertyInfo dependentPropertyInfo = typeof(Dependent).GetProperty("DependentKey1");
            PropertyInfo otherPrincipalPropertyInfo = typeof(Principal).GetProperty("PrincipalKey2");

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.AddEntity(typeof(Principal));
            EntityTypeConfiguration dependentEntity = builder.AddEntity(typeof(Dependent));
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(typeof(Dependent).GetProperty("Principal"), EdmMultiplicity.One,
                    dependentEntity);

            navigationProperty.HasConstraint(dependentPropertyInfo, principalPropertyInfo);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
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
            builder.AddEntity(typeof(Principal));
            EntityTypeConfiguration dependentEntity = builder.AddEntity(typeof(Dependent));
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(typeof(Dependent).GetProperty("Principal"), EdmMultiplicity.One,
                    dependentEntity);

            navigationProperty.HasConstraint(dependentPropertyInfo, principalPropertyInfo);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
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
            builder.AddEntity(typeof(Principal));
            EntityTypeConfiguration dependentEntity = builder.AddEntity(typeof(Dependent));
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(typeof(Dependent).GetProperty("Principal"), EdmMultiplicity.One,
                    dependentEntity);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
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
            builder.AddEntity(typeof(Principal));
            EntityTypeConfiguration dependentEntity = builder.AddEntity(typeof(Dependent));
            NavigationPropertyConfiguration navigationProperty =
                new NavigationPropertyConfiguration(typeof(Dependent).GetProperty("Principal"), EdmMultiplicity.One,
                    dependentEntity);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => navigationProperty.HasConstraint(dependentPropertyInfo, principalPropertyInfo),
                String.Format(SRResources.ReferentialConstraintPropertyTypeNotValid, "System.Web.Http.OData.MockType"));
        }

        class Principal
        {
            public int PrincipalKey1 { get; set; }

            public String PrincipalKey2 { get; set; }

            public MockType MockPrincipal { get; set; }
        }

        class Dependent
        {
            public int DependentKey1 { get; set; }

            public String DependentKey2 { get; set; }

            public MockType MockDependent { get; set; }

            public Principal Principal { get; set; }
        }
    }
}
