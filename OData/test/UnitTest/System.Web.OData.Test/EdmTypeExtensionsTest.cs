// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData
{
    public class EdmTypeExtensionsTest
    {
        [Fact]
        public void IsDeltaFeed_ThrowsArgumentNull_Type()
        {
            IEdmType type = null;
            Assert.ThrowsArgumentNull(
                () => type.IsDeltaFeed(),
                "type");
        }


        [Fact]
        public void CollectionType_IsDeltaFeed_ReturnsTrueForDeltaCollectionType()
        {
            IEdmEntityType _entityType = new EdmEntityType("NS", "Entity");
            EdmDeltaCollectionType _edmType = new EdmDeltaCollectionType(new EdmEntityTypeReference(_entityType, isNullable: true));
            IEdmCollectionTypeReference _edmTypeReference = new EdmCollectionTypeReference(_edmType);

            Assert.True(_edmTypeReference.Definition.IsDeltaFeed());
        }

        [Fact]
        public void CollectionType_IsDeltaFeed_ReturnsFalseForNonDeltaCollectionType()
        {
            IEdmEntityType _entityType = new EdmEntityType("NS", "Entity");
            EdmCollectionType _edmType = new EdmCollectionType(new EdmEntityTypeReference(_entityType, isNullable: true));
            IEdmCollectionTypeReference _edmTypeReference = new EdmCollectionTypeReference(_edmType);

            Assert.False(_edmTypeReference.Definition.IsDeltaFeed());
        }
    }
}
