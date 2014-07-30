// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder.Conventions.Attributes
{
    public class AttributeEdmTypeConventionOfTEdmTypeConfigurationTests
    {
        [Theory]
        [InlineData(typeof(StructuralTypeConfiguration), typeof(StructuralTypeConfiguration), true)]
        [InlineData(typeof(StructuralTypeConfiguration), typeof(StructuralTypeConfiguration), true)]
        [InlineData(typeof(StructuralTypeConfiguration), typeof(ComplexTypeConfiguration), false)]
        [InlineData(typeof(StructuralTypeConfiguration), typeof(EntityTypeConfiguration), false)]
        [InlineData(typeof(ComplexTypeConfiguration), typeof(StructuralTypeConfiguration), true)]
        [InlineData(typeof(ComplexTypeConfiguration), typeof(StructuralTypeConfiguration), true)]
        [InlineData(typeof(ComplexTypeConfiguration), typeof(ComplexTypeConfiguration), true)]
        [InlineData(typeof(ComplexTypeConfiguration), typeof(EntityTypeConfiguration), false)]
        [InlineData(typeof(EntityTypeConfiguration), typeof(StructuralTypeConfiguration), true)]
        [InlineData(typeof(EntityTypeConfiguration), typeof(StructuralTypeConfiguration), true)]
        [InlineData(typeof(EntityTypeConfiguration), typeof(ComplexTypeConfiguration), false)]
        [InlineData(typeof(EntityTypeConfiguration), typeof(EntityTypeConfiguration), true)]
        public void Apply_AppliesOnly_ForMatchingTEdmTypeConfiguration(Type tEdmTypeConfiguration, Type tConventionType, bool shouldbeApplied)
        {
            MethodInfo applyMethod = GetType().GetMethods(BindingFlags.Static | BindingFlags.Public).Single();
            Assert.Equal(shouldbeApplied, (bool)applyMethod.MakeGenericMethod(tEdmTypeConfiguration, tConventionType).Invoke(null, null));
        }

        public static bool Apply<TEdmTypeConfiguration, TConventionType>()
            where TEdmTypeConfiguration : StructuralTypeConfiguration
            where TConventionType : StructuralTypeConfiguration
        {
            Func<Attribute, bool> matchAllFilter = a => true;

            ODataConventionModelBuilder builder = new Mock<ODataConventionModelBuilder>().Object;
            Attribute attribute = new Mock<Attribute>().Object;

            // build the type
            Mock<Type> type = new Mock<Type>();
            type.Setup(t => t.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { attribute });
            Mock<TEdmTypeConfiguration> structuralType = new Mock<TEdmTypeConfiguration>(MockBehavior.Strict);
            structuralType.Setup(t => t.ClrType).Returns(type.Object);

            // build the convention
            SpyAttributeEdmTypeConvention<TConventionType> spy =
                new SpyAttributeEdmTypeConvention<TConventionType>(matchAllFilter, allowMultiple: false);

            // Apply
            (spy as IEdmTypeConvention).Apply(structuralType.Object, builder);

            return object.ReferenceEquals(builder, spy.ModelBuilder) &&
                object.ReferenceEquals(attribute, spy.Attribute);
        }

        private class SpyAttributeEdmTypeConvention<TConventionType> : AttributeEdmTypeConvention<TConventionType>
            where TConventionType : StructuralTypeConfiguration
        {
            public SpyAttributeEdmTypeConvention(Func<Attribute, bool> attributeFilter, bool allowMultiple)
                : base(attributeFilter, allowMultiple)
            {
            }

            public ODataModelBuilder ModelBuilder { get; private set; }

            public Attribute Attribute { get; private set; }

            public override void Apply(TConventionType edmTypeConfiguration, ODataConventionModelBuilder model,
                Attribute attribute)
            {
                ModelBuilder = model;
                Attribute = attribute;
            }
        }
    }
}
