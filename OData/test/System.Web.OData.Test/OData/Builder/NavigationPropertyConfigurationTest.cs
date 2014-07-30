// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder
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
    }
}
