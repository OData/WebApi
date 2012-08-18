// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    public class AttributeEdmTypeConventionOfTEdmTypeConfigurationAndTAttributeTests
    {
        [Theory]
        [InlineData(typeof(DataContractAttribute), typeof(DataContractAttribute), true)]
        [InlineData(typeof(Attribute), typeof(DataContractAttribute), true)]
        [InlineData(typeof(DataContractAttribute), typeof(IgnoreTypeAttribute), false)]
        public void FilterMatchesSpecifiedAttributeType(Type conventionType, Type attributeType, bool expectedMatchResult)
        {
            MethodInfo createMethod = GetType().GetMethods(BindingFlags.Static | BindingFlags.Public).Single();
            Func<Attribute, bool> filter = createMethod.MakeGenericMethod(conventionType).Invoke(null, null) as Func<Attribute, bool>;
            Assert.Equal(expectedMatchResult, filter(Activator.CreateInstance(attributeType) as Attribute));
        }

        public static Func<Attribute, bool> GetFilter<TAttribute>()
            where TAttribute : Attribute
        {
            AttributeEdmTypeConvention<IStructuralTypeConfiguration, TAttribute> convention = new Mock<AttributeEdmTypeConvention<IStructuralTypeConfiguration, TAttribute>>(MockBehavior.Loose, false) { CallBase = true }.Object;
            return convention.AttributeFilter;
        }

        private class IgnoreTypeAttribute : Attribute
        {
        }
    }
}
