// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class EdmDeltaComplexObjectTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmTypeOfTypeIEdmComplexType()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new EdmDeltaComplexObject((IEdmComplexType)null), "edmType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmTypeOfTypeIEdmComplexTypeReference()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new EdmDeltaComplexObject((IEdmComplexTypeReference)null), "type");
        }

        [Fact]
        public void Property_IsNullable()
        {
            EdmDeltaComplexObject edmObject = new EdmDeltaComplexObject(new EdmComplexType("NS", "Complex"));

            ReflectionAssert.BooleanProperty(edmObject, e => e.IsNullable, expectedDefaultValue: false);
        }
    }
}
