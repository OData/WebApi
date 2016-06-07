// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData
{
    public class EdmDeltaCollectionTypeTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EntityTypeReference()
        {
            Assert.ThrowsArgumentNull(() => new EdmDeltaCollectionType((IEdmTypeReference)null), "entityTypeReference");
        }


        [Fact]
        public void GetEdmType_Returns_EdmTypeInitializedByCtor()
        {
            IEdmEntityTypeReference _entityReferenceType = new EdmEntityTypeReference(new EdmEntityType("NS", "Entity"), isNullable: false);
            var edmObject = new EdmDeltaCollectionType(_entityReferenceType);

            Assert.Same(_entityReferenceType, edmObject.ElementType);
        }
    }
}
