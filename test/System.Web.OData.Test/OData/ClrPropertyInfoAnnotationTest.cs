// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData
{
    public class ClrPropertyInfoAnnotationTest
    {
        [Fact]
        public void Ctor_ThrowsForNullPropertyInfo()
        {
            Assert.ThrowsArgumentNull(
                () => new ClrPropertyInfoAnnotation(clrPropertyInfo: null),
                "clrPropertyInfo");
        }

        [Fact]
        public void Property_ClrPropertyInfo_RoundTrips()
        {
            Mock<PropertyInfo> mockPropertyInfo = new Mock<PropertyInfo>();
            PropertyInfo defaultPropertyInfo = mockPropertyInfo.Object;
            ClrPropertyInfoAnnotation _clrPropertyInfoAnnotation = new ClrPropertyInfoAnnotation(defaultPropertyInfo);
            PropertyInfo propertyInfo = new Mock<PropertyInfo>().Object;
            Assert.Reflection.Property(
                _clrPropertyInfoAnnotation,
                c => c.ClrPropertyInfo,
                defaultPropertyInfo,
                allowNull: false,
                roundTripTestValue: propertyInfo);
        }
    }
}
