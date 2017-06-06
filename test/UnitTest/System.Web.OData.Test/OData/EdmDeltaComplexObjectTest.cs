// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData
{
    public class EdmDeltaComplexObjectTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmTypeOfTypeIEdmComplexType()
        {
            Assert.ThrowsArgumentNull(() => new EdmDeltaComplexObject((IEdmComplexType)null), "edmType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmTypeOfTypeIEdmComplexTypeReference()
        {
            Assert.ThrowsArgumentNull(() => new EdmDeltaComplexObject((IEdmComplexTypeReference)null), "type");
        }

        [Fact]
        public void Property_IsNullable()
        {
            EdmDeltaComplexObject edmObject = new EdmDeltaComplexObject(new EdmComplexType("NS", "Complex"));

            Assert.Reflection.BooleanProperty(edmObject, e => e.IsNullable, expectedDefaultValue: false);
        }
    }
}
