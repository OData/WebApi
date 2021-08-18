//-----------------------------------------------------------------------------
// <copyright file="EdmTypeExtensionsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class EdmTypeExtensionsTest
    {
        [Fact]
        public void IsDeltaFeed_ThrowsArgumentNull_Type()
        {
            IEdmType type = null;
            ExceptionAssert.ThrowsArgumentNull(
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

        [Fact]
        public void IsDeltaObject_ThrowsArgumentNull_Type()
        {
            IEdmObject instance = null;
            ExceptionAssert.ThrowsArgumentNull(
                () => instance.IsDeltaResource(),
                "resource");
        }

        [Fact]
        public void EdmDeltaEntityObject_IsDeltaObject_ReturnsTrueForDeltaObject()
        {
            IEdmEntityType _type = new EdmEntityType("NS", "Entity");
            EdmDeltaEntityObject _edmObject = new EdmDeltaEntityObject(_type);

            Assert.True(_edmObject.IsDeltaResource());
        }

        [Fact]
        public void EdmDeltaComplexObject_IsDeltaObject_ReturnsTrueForDeltaObject()
        {
            IEdmComplexType _type = new EdmComplexType("NS", "Entity");
            EdmDeltaComplexObject _edmObject = new EdmDeltaComplexObject(_type);

            Assert.True(_edmObject.IsDeltaResource());
        }

        [Fact]
        public void EdmEntityObject_IsDeltaFeed_ReturnsFalseForNonDeltaObject()
        {
            IEdmEntityType _type = new EdmEntityType("NS", "Entity");
            EdmEntityObject _edmObject = new EdmEntityObject(_type);

            Assert.False(_edmObject.IsDeltaResource());
        }

        [Fact]
        public void EdmComplexObject_IsDeltaFeed_ReturnsFalseForNonDeltaObject()
        {
            IEdmComplexType _type = new EdmComplexType("NS", "Entity");
            EdmComplexObject _edmObject = new EdmComplexObject(_type);

            Assert.False(_edmObject.IsDeltaResource());
        }
    }
}
