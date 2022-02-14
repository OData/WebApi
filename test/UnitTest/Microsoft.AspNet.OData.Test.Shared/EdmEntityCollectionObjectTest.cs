//-----------------------------------------------------------------------------
// <copyright file="EdmEntityCollectionObjectTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class EdmEntityCollectionObjectTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmType()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new EdmEntityObjectCollection(edmType: null), "edmType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_List()
        {
            IEdmCollectionTypeReference edmType = new Mock<IEdmCollectionTypeReference>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => new EdmEntityObjectCollection(edmType, list: null), "list");
        }

        [Fact]
        public void Ctor_ThrowsArgument_UnexpectedElementType()
        {
            IEdmTypeReference elementType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: true);
            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(new EdmCollectionType(elementType));

            ExceptionAssert.ThrowsArgument(() => new EdmEntityObjectCollection(collectionType), "edmType",
            "The element type '[Edm.Int32 Nullable=True]' of the given collection type '[Collection([Edm.Int32 Nullable=True]) Nullable=True]' " +
            "is not of the type 'IEdmEntityType'.");
        }

        [Fact]
        public void GetEdmType_Returns_EdmTypeInitializedByCtor()
        {
            IEdmTypeReference elementType = new EdmEntityTypeReference(new EdmEntityType("NS", "Entity"), isNullable: false);
            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(new EdmCollectionType(elementType));

            var edmObject = new EdmEntityObjectCollection(collectionType);
            Assert.Same(collectionType, edmObject.GetEdmType());
        }
    }
}
