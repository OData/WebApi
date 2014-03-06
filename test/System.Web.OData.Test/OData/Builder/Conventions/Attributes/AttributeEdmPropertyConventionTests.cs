// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder.Conventions.Attributes
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
            Func<Attribute, bool> matchAllFilter = a => true;

            // build the type
            Mock<EntityTypeConfiguration> structuralType = new Mock<EntityTypeConfiguration>(MockBehavior.Strict);
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            structuralType.Setup(s => s.ModelBuilder).Returns(builder);

            // build the property
            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns("Property");
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            Attribute attribute = new Mock<Attribute>().Object;
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { attribute });

            Mock<TProperty> propertyConfiguration;
            if (typeof(TProperty) == typeof(NavigationPropertyConfiguration))
            {
                propertyConfiguration = new Mock<TProperty>(property.Object, EdmMultiplicity.ZeroOrOne, structuralType.Object);
            }
            else
            {
                propertyConfiguration = new Mock<TProperty>(property.Object, structuralType.Object);
            }

            // build the convention
            SpyAttributeEdmPropertyConvention<TPropertyConfiguration> spy =
                new SpyAttributeEdmPropertyConvention<TPropertyConfiguration>(matchAllFilter, allowMultiple: false);

            // Apply
            (spy as IEdmPropertyConvention).Apply(propertyConfiguration.Object, structuralType.Object, builder);

            return spy.Called;
        }

        private class SpyAttributeEdmPropertyConvention<TPropertyConfiguration> :
            AttributeEdmPropertyConvention<TPropertyConfiguration>
            where TPropertyConfiguration : PropertyConfiguration
        {
            public SpyAttributeEdmPropertyConvention(Func<Attribute, bool> attributeFilter, bool allowMultiple)
                : base(attributeFilter, allowMultiple)
            {
            }

            public bool Called { get; private set; }

            public override void Apply(TPropertyConfiguration edmProperty,
                StructuralTypeConfiguration structuralTypeConfiguration,
                Attribute attribute,
                ODataConventionModelBuilder model)
            {
                Called = true;
            }
        }
    }
}
