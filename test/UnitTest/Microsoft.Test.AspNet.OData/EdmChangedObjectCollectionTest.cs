// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.TestCommon;
using Moq;

namespace Microsoft.Test.AspNet.OData
{
    public class EdmChangedObjectCollectionTest
    {
         [Fact]
         public void Ctor_ThrowsArgumentNull_EdmType()
         {
             Assert.ThrowsArgumentNull(() => new EdmChangedObjectCollection(entityType: null), "entityType");
         }

         [Fact]
         public void Ctor_ThrowsArgumentNull_List()
         {
             IEdmEntityType entityType = new Mock<IEdmEntityType>().Object;
             Assert.ThrowsArgumentNull(() => new EdmChangedObjectCollection(entityType, changedObjectList: null), "list");
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
