// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Moq;
using System;
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


        [Fact]
        public void EdmChangedObjectCollection_Add_WrongItem_ThrowsError()
        {
            //Assign
            IEdmEntityType _entityType = new EdmEntityType("namespace Microsoft.AspNet.OData.Test", "Friend");
            var edmChangedObjectcollection = new EdmChangedObjectCollection<Friend>(_entityType);

            var edmChangedObj1 = new Delta<NewFriend>();
            edmChangedObj1.TrySetPropertyValue("Id", 1);
            edmChangedObj1.TrySetPropertyValue("Name", "Friend1");

            //Act & Assert
            Assert.Throws<ArgumentException>(() => edmChangedObjectcollection.Add(edmChangedObj1));
        }

        [Fact]
        public void EdmChangedObjectCollection_Add()
        {
            //Assign
            IEdmEntityType _entityType = new EdmEntityType("namespace Microsoft.AspNet.OData.Test", "Friend");
            var edmChangedObjectcollection = new EdmChangedObjectCollection<Friend>(_entityType);

            var edmChangedObj1 = new Delta<Friend>();
            edmChangedObj1.TrySetPropertyValue("Id", 1);
            edmChangedObj1.TrySetPropertyValue("Name", "Friend1");

            var edmChangedObj2 = new Delta<Friend>();
            edmChangedObj2.TrySetPropertyValue("Id", 2);
            edmChangedObj2.TrySetPropertyValue("Name", "Friend2");

            //Act
            edmChangedObjectcollection.Add(edmChangedObj1);
            edmChangedObjectcollection.Add(edmChangedObj2);

            //Assert
            Assert.Equal(2, edmChangedObjectcollection.Count);
            object id;
            edmChangedObjectcollection[0].TryGetPropertyValue("Id", out id);
            Assert.Equal(1, id);
            object name;
            edmChangedObjectcollection[0].TryGetPropertyValue("Name", out name);
            Assert.Equal("Friend1", name);

            edmChangedObjectcollection[1].TryGetPropertyValue("Id", out id);
            Assert.Equal(2, id);

            edmChangedObjectcollection[1].TryGetPropertyValue("Name", out name);
            Assert.Equal("Friend2", name);
        }

        [Fact]
        public void EdmChangedObjectCollection_Add_ConvertWithBase()
        {
            //Assign
            IEdmEntityType _entityType = new EdmEntityType("namespace Microsoft.AspNet.OData.Test", "Friend");
            var edmChangedObjectcollection = new EdmChangedObjectCollection<Friend>(_entityType);

            var edmChangedObj1 = new Delta<Friend>();
            edmChangedObj1.TrySetPropertyValue("Id", 1);
            edmChangedObj1.TrySetPropertyValue("Name", "Friend1");

            var edmChangedObj2 = new Delta<Friend>();
            edmChangedObj2.TrySetPropertyValue("Id", 2);
            edmChangedObj2.TrySetPropertyValue("Name", "Friend2");

            //Act
            var edmchanged1 = edmChangedObj1 as IEdmChangedObject;
            var edmchanged2 = edmChangedObj2 as IEdmChangedObject;

            var edmObjColl = edmChangedObjectcollection as EdmChangedObjectCollection;

            edmObjColl.Add(edmchanged1);
            edmObjColl.Add(edmchanged2);

            //Assert
            Assert.Equal(2, edmChangedObjectcollection.Count);
            object id;
            edmChangedObjectcollection[0].TryGetPropertyValue("Id", out id);
            Assert.Equal(1, id);
            object name;
            edmChangedObjectcollection[0].TryGetPropertyValue("Name", out name);
            Assert.Equal("Friend1", name);

            edmChangedObjectcollection[1].TryGetPropertyValue("Id", out id);
            Assert.Equal(2, id);

            edmChangedObjectcollection[1].TryGetPropertyValue("Name", out name);
            Assert.Equal("Friend2", name);
        }

        [Fact]
        public void EdmChangedObjectCollection_Add_Remove_Contains_Enumerate()
        {
            //Assign
            IEdmEntityType _entityType = new EdmEntityType("namespace Microsoft.AspNet.OData.Test", "Friend");
            var edmChangedObjectcollection = new EdmChangedObjectCollection<Friend>(_entityType);

            var edmChangedObj1 = new Delta<Friend>();
            edmChangedObj1.TrySetPropertyValue("Id", 1);
            edmChangedObj1.TrySetPropertyValue("Name", "Friend1");

            //Act & Assert
            edmChangedObjectcollection.Add(edmChangedObj1);
            Assert.Single(edmChangedObjectcollection);

            Assert.True(edmChangedObjectcollection.Contains(edmChangedObj1));

            var enumerator = edmChangedObjectcollection.GetEnumerator();
            Assert.NotNull(enumerator);
            Assert.True(enumerator.MoveNext());

            edmChangedObjectcollection.Remove(edmChangedObj1);
            Assert.Empty(edmChangedObjectcollection);
        }

        [Fact]
        public void EdmChangedObjectCollection_Add_Remove_Contains_Enumerate_CastAfter()
        {
            //Assign
            IEdmEntityType _entityType = new EdmEntityType("namespace Microsoft.AspNet.OData.Test", "Friend");
            var edmChangedObjectcollection = new EdmChangedObjectCollection<Friend>(_entityType);

            var edmChangedObj1 = new Delta<Friend>();
            edmChangedObj1.TrySetPropertyValue("Id", 1);
            edmChangedObj1.TrySetPropertyValue("Name", "Friend1");

            var edmChangedObj2 = new Delta<Friend>();
            edmChangedObj2.TrySetPropertyValue("Id", 2);
            edmChangedObj2.TrySetPropertyValue("Name", "Friend2");

            //Act & Assert
            edmChangedObjectcollection.Add(edmChangedObj1);
            edmChangedObjectcollection.Add(edmChangedObj2);
            Assert.Equal(2,edmChangedObjectcollection.Count);

            Assert.True(edmChangedObjectcollection.Contains(edmChangedObj1));

            var enumerator = edmChangedObjectcollection.GetEnumerator();
            Assert.NotNull(enumerator);
            Assert.True(enumerator.MoveNext());

            edmChangedObjectcollection.Remove(edmChangedObj1);
            Assert.Single(edmChangedObjectcollection);

            var edmObjColl = edmChangedObjectcollection as EdmChangedObjectCollection;
            Assert.Single(edmObjColl);

            var hasEdmObj = edmChangedObjectcollection.Contains(edmChangedObj2);
            Assert.True(hasEdmObj);

            object id;
            edmObjColl[0].TryGetPropertyValue("Id", out id);
            Assert.Equal(2, id);

            object name;
            edmObjColl[0].TryGetPropertyValue("Name", out name);
            Assert.Equal("Friend2", name);
        }


        [Fact]
        public void EdmChangedObjectCollection_Add_Remove_Contains_Enumerate_CastBefore()
        {
            //Assign
            IEdmEntityType _entityType = new EdmEntityType("namespace Microsoft.AspNet.OData.Test", "Friend");
            var edmChangedObjectcollection = new EdmChangedObjectCollection<Friend>(_entityType);

            var edmChangedObj1 = new Delta<Friend>();
            edmChangedObj1.TrySetPropertyValue("Id", 1);
            edmChangedObj1.TrySetPropertyValue("Name", "Friend1");

            //Act & Assert
            var edmCollFriend = edmChangedObjectcollection as EdmChangedObjectCollection;

            edmCollFriend.Add(edmChangedObj1);            
            
            Assert.Single(edmCollFriend);

            var edmObj = edmCollFriend.Contains(edmChangedObj1);
            Assert.True(edmObj);

            var enumerator = edmCollFriend.GetEnumerator();
            Assert.NotNull(enumerator);
            Assert.True(enumerator.MoveNext());

            edmCollFriend.Remove(edmChangedObj1);
            Assert.Empty(edmCollFriend);
            Assert.Empty(edmCollFriend);
        }

        [Fact]
        public void EdmChangedObjectCollection_Add_Remove_Contains_Enumerate_CastWithBaseEdm()
        {
            //Assign
            IEdmEntityType _entityType = new EdmEntityType("namespace Microsoft.AspNet.OData.Test", "Friend");
            var edmChangedObjectcollection = new EdmChangedObjectCollection<Friend>(_entityType);

            var edmChangedObj1 = new Delta<Friend>();
            edmChangedObj1.TrySetPropertyValue("Id", 1);
            edmChangedObj1.TrySetPropertyValue("Name", "Friend1");

            var edmChangedObj = edmChangedObj1 as IEdmChangedObject;
            //Act & Assert
            edmChangedObjectcollection.Add(edmChangedObj);
            Assert.Single(edmChangedObjectcollection);
                        
            var enumerator = edmChangedObjectcollection.GetEnumerator();
            Assert.NotNull(enumerator);
            Assert.True(enumerator.MoveNext());

            var edmObjColl = edmChangedObjectcollection as EdmChangedObjectCollection;
            Assert.Single(edmObjColl);

            var hasedmObj = edmChangedObjectcollection.Contains(edmChangedObj);
            Assert.True(hasedmObj);

            object id;
            edmObjColl[0].TryGetPropertyValue("Id", out id);
            Assert.Equal(1, id);

            object name;
            edmObjColl[0].TryGetPropertyValue("Name", out name);
            Assert.Equal("Friend1", name);

            edmObjColl.Remove(edmChangedObj);
            Assert.Empty(edmChangedObjectcollection);

        }
    }

    public class Friend
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }

    public class NewFriend
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }
}
