// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.OData.Builder.Conventions.Attributes
{
    public class AttributeConventionTests
    {
        [Fact]
        public void Ctor_ThrowsFor_Null_attributeFilter()
        {
            Assert.ThrowsArgumentNull(() => { AttributeConvention convention = new TestAttributeConvention(null, true); }, "attributeFilter");
        }

        [Fact]
        public void GetAttributes_InvokesFilter()
        {
            // Arrange
            int filterInvoked = 0;
            Func<Attribute, bool> filter = attribute =>
            {
                filterInvoked++;
                return true;
            };

            AttributeConvention convention = new TestAttributeConvention(filter, true);

            // Act & Assert
            Assert.Equal(
                new[] { "FactAttribute" },
                convention.GetAttributes(GetType().GetMethod("GetAttributes_InvokesFilter")).Select(a => a.GetType().Name));
            Assert.Equal(1, filterInvoked);
        }

        [Fact]
        [Sample]
        [Sample]
        public void GetAttributes_Throws_IfMultipleAttributesPresentAndAllowMultipleIsFalse()
        {
            Func<Attribute, bool> filter = attribute => attribute.GetType() == typeof(SampleAttribute);
            AttributeConvention convention = new TestAttributeConvention(filter, false);

            Assert.ThrowsArgument(
                () => convention.GetAttributes(GetType().GetMethod("GetAttributes_Throws_IfMultipleAttributesPresentAndAllowMultipleIsFalse")),
                "member",
                "The member 'GetAttributes_Throws_IfMultipleAttributesPresentAndAllowMultipleIsFalse' on type 'AttributeConventionTests' contains multiple instances of the attribute 'SampleAttribute'.");
        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
        internal class SampleAttribute : Attribute
        {
        }

        private class TestAttributeConvention : AttributeConvention
        {
            public TestAttributeConvention(Func<Attribute, bool> attributeFilter, bool allowMultiple)
                : base(attributeFilter, allowMultiple)
            {
            }
        }
    }
}
