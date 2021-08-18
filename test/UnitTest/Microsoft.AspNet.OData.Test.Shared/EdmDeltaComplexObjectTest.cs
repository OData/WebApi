//-----------------------------------------------------------------------------
// <copyright file="EdmDeltaComplexObjectTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
