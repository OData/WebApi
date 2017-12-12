// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.TestCommon;
using Xunit;

namespace Microsoft.Test.AspNet.OData
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
