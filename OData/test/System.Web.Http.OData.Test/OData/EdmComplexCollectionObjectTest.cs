// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData
{
    public class EdmComplexCollectionObjectTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmType()
        {
            Assert.ThrowsArgumentNull(() => new EdmComplexObjectCollection(edmType: null), "edmType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_List()
        {
            IEdmCollectionTypeReference edmType = new Mock<IEdmCollectionTypeReference>().Object;
            Assert.ThrowsArgumentNull(() => new EdmComplexObjectCollection(edmType, list: null), "list");
        }

        [Fact]
        public void Ctor_ThrowsArgument_UnexpectedElementType()
        {
            IEdmTypeReference elementType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: true);
            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(new EdmCollectionType(elementType), isNullable: false);

            Assert.ThrowsArgument(() => new EdmComplexObjectCollection(collectionType), "edmType",
            "The element type '[Edm.Int32 Nullable=True]' of the given collection type '[Collection([Edm.Int32 Nullable=True]) Nullable=False]' " +
            "is not of the type 'IEdmComplexType'.");
        }

        [Fact]
        public void GetEdmType_Returns_EdmTypeInitializedByCtor()
        {
            IEdmTypeReference elementType = new EdmComplexTypeReference(new EdmComplexType("NS", "Complex"), isNullable: false);
            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(new EdmCollectionType(elementType), isNullable: false);

            var edmObject = new EdmComplexObjectCollection(collectionType);
            Assert.Same(collectionType, edmObject.GetEdmType());
        }
    }
}
