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
    }
}
