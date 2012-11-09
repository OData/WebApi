// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
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
            bool applyCalled = false;
            Func<Attribute, bool> matchAllFilter = a => true;

            ODataModelBuilder builder = new Mock<ODataModelBuilder>().Object;
            Attribute attribute = new Mock<Attribute>().Object;

            // build the type
            Mock<Type> type = new Mock<Type>();
            type.Setup(t => t.GetCustomAttributes(It.IsAny<bool>())).Returns(new[] { attribute });
            Mock<TEdmTypeConfiguration> structuralType = new Mock<TEdmTypeConfiguration>(MockBehavior.Strict);

            structuralType.Setup(t => t.ClrType).Returns(type.Object);

            // build the convention
            Mock<AttributeEdmTypeConvention<TConventionType>> convention = new Mock<AttributeEdmTypeConvention<TConventionType>>(matchAllFilter, false);
            convention.Setup(c => c.Apply(It.IsAny<TConventionType>(), builder, attribute)).Callback(() => { applyCalled = true; });

            // Apply
            (convention.Object as IEdmTypeConvention).Apply(structuralType.Object, builder);

            return applyCalled;
        }
    }
}
