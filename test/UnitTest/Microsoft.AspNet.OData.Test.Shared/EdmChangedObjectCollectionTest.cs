// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class EdmChangedObjectCollectionTest
    {
         [Fact]
         public void Ctor_ThrowsArgumentNull_EdmType()
         {
             ExceptionAssert.ThrowsArgumentNull(() => new EdmChangedObjectCollection(entityType: null), "entityType");
         }

         [Fact]
         public void Ctor_ThrowsArgumentNull_List()
         {
             IEdmEntityType entityType = new Mock<IEdmEntityType>().Object;
             ExceptionAssert.ThrowsArgumentNull(() => new EdmChangedObjectCollection(entityType, changedObjectList: null), "list");
         }

         [Fact]
         public void GetEdmType_Returns_EdmTypeInitializedByCtor()
         {
             IEdmEntityType _entityType = new EdmEntityType("NS", "Entity");
             var edmObject = new EdmChangedObjectCollection(_entityType);
             IEdmCollectionTypeReference collectionTypeReference = (IEdmCollectionTypeReference)edmObject.GetEdmType();

             Assert.Same(_entityType, collectionTypeReference.ElementType().Definition);
         }
   }
}
