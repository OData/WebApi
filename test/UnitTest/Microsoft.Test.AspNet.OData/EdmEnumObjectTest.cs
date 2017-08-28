// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.TestCommon;

namespace Microsoft.Test.AspNet.OData
{
    public class EdmEnumObjectTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmTypeOfTypeIEdmEnumType()
        {
            Assert.ThrowsArgumentNull(() => new TestEdmEnumObject((IEdmEnumType)null, "test"), "edmType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmTypeOfTypeIEdmEnumTypeReference()
        {
            Assert.ThrowsArgumentNull(() => new TestEdmEnumObject((IEdmEnumTypeReference)null, "test"), "type");
        }

        [Fact]
        public void Property_IsNullable()
        {
            TestEdmEnumObject edmObject = new TestEdmEnumObject(new EdmEnumType("NS", "Enum"), "test");

            Assert.Reflection.BooleanProperty(edmObject, e => e.IsNullable, expectedDefaultValue: false);
        }

        [Fact]
        public void GetEdmType_HasSameDefinition_AsInitializedEdmType()
        {
            var enumType = new EdmEnumType("NS", "Enum");
            var edmObject = new TestEdmEnumObject(enumType, "test");

            Assert.Equal(enumType, edmObject.GetEdmType().Definition);
        }

        [Fact]
        public void GetEdmType_AgreesWithPropertyIsNullable()
        {
            var enumType = new EdmEnumType("NS", "Enum");
            var edmObject = new TestEdmEnumObject(enumType, "test");
            edmObject.IsNullable = true;

            Assert.True(edmObject.GetEdmType().IsNullable);
        }

        private class TestEdmEnumObject : EdmEnumObject
        {
            public TestEdmEnumObject(IEdmEnumType edmType, string value)
                : base(edmType, value)
            {
            }

            public TestEdmEnumObject(IEdmEnumTypeReference edmType, string value)
                : base(edmType, value)
            {
            }
        }
    }
}
