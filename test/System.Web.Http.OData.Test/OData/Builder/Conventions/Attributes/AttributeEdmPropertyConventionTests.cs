// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    public class AttributeEdmPropertyConventionOfTPropertyConfigurationTests
    {
        [Theory]
        [InlineData(typeof(PropertyConfiguration), typeof(PropertyConfiguration), true)]
        [InlineData(typeof(PropertyConfiguration), typeof(PrimitivePropertyConfiguration), true)]
        [InlineData(typeof(PropertyConfiguration), typeof(ComplexPropertyConfiguration), true)]
        [InlineData(typeof(PropertyConfiguration), typeof(NavigationPropertyConfiguration), true)]
        [InlineData(typeof(PrimitivePropertyConfiguration), typeof(PrimitivePropertyConfiguration), true)]
        [InlineData(typeof(NavigationPropertyConfiguration), typeof(NavigationPropertyConfiguration), true)]
        [InlineData(typeof(ComplexPropertyConfiguration), typeof(ComplexPropertyConfiguration), true)]
        [InlineData(typeof(PrimitivePropertyConfiguration), typeof(PropertyConfiguration), false)]
        [InlineData(typeof(PrimitivePropertyConfiguration), typeof(ComplexPropertyConfiguration), false)]
        [InlineData(typeof(PrimitivePropertyConfiguration), typeof(NavigationPropertyConfiguration), false)]
        [InlineData(typeof(ComplexPropertyConfiguration), typeof(PropertyConfiguration), false)]
        [InlineData(typeof(ComplexPropertyConfiguration), typeof(NavigationPropertyConfiguration), false)]
        [InlineData(typeof(ComplexPropertyConfiguration), typeof(PrimitivePropertyConfiguration), false)]
        [InlineData(typeof(NavigationPropertyConfiguration), typeof(PropertyConfiguration), false)]
        [InlineData(typeof(NavigationPropertyConfiguration), typeof(ComplexPropertyConfiguration), false)]
        [InlineData(typeof(NavigationPropertyConfiguration), typeof(PrimitivePropertyConfiguration), false)]
        public void Apply_AppliesOnly_ForMatchingTPropertyConfiguration(Type tPropertyConfiguration, Type edmPropertyType, bool shouldbeApplied)
        {
            MethodInfo applyMethod = GetType().GetMethods(BindingFlags.Static | BindingFlags.Public).Single();
            Assert.Equal(shouldbeApplied, (bool)applyMethod.MakeGenericMethod(tPropertyConfiguration, edmPropertyType).Invoke(null, null));
        }

        public static bool Apply<TPropertyConfiguration, TProperty>()
            where TPropertyConfiguration : PropertyConfiguration
            where TProperty : PropertyConfiguration
        {
            bool applyCalled = false;
            Func<Attribute, bool> matchAllFilter = a => true;

            // build the type
            Mock<IStructuralTypeConfiguration> structuralType = new Mock<IStructuralTypeConfiguration>(MockBehavior.Strict);

            // build the property
            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            Attribute attribute = new Mock<Attribute>().Object;
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { attribute });

            Mock<TProperty> propertyConfiguration;
            if (typeof(TProperty) == typeof(NavigationPropertyConfiguration))
            {
                propertyConfiguration = new Mock<TProperty>(property.Object, EdmMultiplicity.ZeroOrOne);
            }
            else
            {
                propertyConfiguration = new Mock<TProperty>(property.Object);
            }

            // build the convention
            Mock<AttributeEdmPropertyConvention<TPropertyConfiguration>> convention = new Mock<AttributeEdmPropertyConvention<TPropertyConfiguration>>(matchAllFilter, false);
            convention.Setup(c => c.Apply(It.IsAny<TPropertyConfiguration>(), It.IsAny<IStructuralTypeConfiguration>(), It.IsAny<Attribute>())).Callback(() => { applyCalled = true; });

            // Apply
            (convention.Object as IEdmPropertyConvention).Apply(propertyConfiguration.Object, structuralType.Object);

            return applyCalled;
        }
    }
}
