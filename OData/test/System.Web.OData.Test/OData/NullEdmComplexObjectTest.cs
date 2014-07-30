// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.OData
{
    public class NullEdmComplexObjectTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmType()
        {
            Assert.ThrowsArgumentNull(() => new NullEdmComplexObject(edmType: null), "edmType");
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

            Assert.Throws<InvalidOperationException>(() => nullComplexObject.TryGetPropertyValue("property", out propertyValue),
                "Cannot get property 'property' of a null EDM object of type '[NS.ComplexType Nullable=True]'.");
        }
    }
}
