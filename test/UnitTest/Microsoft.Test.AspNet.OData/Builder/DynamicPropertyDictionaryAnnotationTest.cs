// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNet.OData.Builder;
using Microsoft.Test.AspNet.OData.TestCommon;
using Moq;

namespace Microsoft.Test.AspNet.OData.Builder
{
    public class DynamicPropertyDictionaryAnnotationTest
    {
        [Fact]
        public void Ctor_ThrowsForNullPropertyInfo()
        {
            Assert.ThrowsArgumentNull(
                () => new DynamicPropertyDictionaryAnnotation(propertyInfo: null),
                "propertyInfo");
        }

        [Fact]
        public void Ctor_ThrowsForNotIDictionaryPropretyInfo()
        {
            // Arrange
            Mock<PropertyInfo> propertyInfo = new Mock<PropertyInfo>();
            propertyInfo.Setup(p => p.PropertyType).Returns(typeof(int));

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new DynamicPropertyDictionaryAnnotation(
                propertyInfo: propertyInfo.Object),
                "Type 'Int32' is not supported as dynamic property annotation. " +
                "Referenced property must be of type 'IDictionary<string, object>'." +
                "\r\nParameter name: propertyInfo");
        }
    }
}
