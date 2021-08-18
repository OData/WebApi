//-----------------------------------------------------------------------------
// <copyright file="NullEdmComplexObjectTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class NullEdmComplexObjectTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmType()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new NullEdmComplexObject(edmType: null), "edmType");
        }

        [Fact]
        public void GetEdmType_Returns_CtorInitializedValue()
        {
            IEdmComplexTypeReference edmType = new EdmComplexTypeReference(new EdmComplexType("NS", "ComplexType"), isNullable: true);
            NullEdmComplexObject nullComplexObject = new NullEdmComplexObject(edmType);

            IEdmTypeReference result = nullComplexObject.GetEdmType();

            Assert.Same(edmType, result);
        }

        [Fact]
        public void TryGetValue_ThrowsInvalidOperation_EdmComplexObjectNullRef()
        {
            IEdmComplexTypeReference edmType = new EdmComplexTypeReference(new EdmComplexType("NS", "ComplexType"), isNullable: true);
            NullEdmComplexObject nullComplexObject = new NullEdmComplexObject(edmType);
            object propertyValue;

            ExceptionAssert.Throws<InvalidOperationException>(() => nullComplexObject.TryGetPropertyValue("property", out propertyValue),
                "Cannot get property 'property' of a null EDM object of type '[NS.ComplexType Nullable=True]'.");
        }
    }
}
