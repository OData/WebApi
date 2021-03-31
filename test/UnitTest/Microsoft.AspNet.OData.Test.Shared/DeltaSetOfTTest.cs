// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class DeltaSetOfTTest
    {
       public static List<Friend> friends;

       [Fact]
        public void DeltaSet_Patch()
        {
            //Assign
               var lstId = new List<string>();
            lstId.Add("Id");
            var deltaSet = new DeltaSet<Friend>(lstId);

            var edmChangedObj1 = new Delta<Friend>();
            edmChangedObj1.TrySetPropertyValue("Id", 1);
            edmChangedObj1.TrySetPropertyValue("Name", "Friend1");

            var edmChangedObj2 = new Delta<Friend>();
            edmChangedObj2.TrySetPropertyValue("Id", 2);
            edmChangedObj2.TrySetPropertyValue("Name", "Friend2");
                        
            deltaSet.Add(edmChangedObj1);
            deltaSet.Add(edmChangedObj2);

            var friends = new List<Friend>();
            friends.Add(new Friend { Id = 1, Name = "Test1" });
            friends.Add(new Friend { Id = 2, Name = "Test2" });

            //Act
            deltaSet.Patch(friends);

            //Assert
            Assert.Equal(2, friends.Count);
            Assert.Equal("Friend1", friends[0].Name);
            Assert.Equal("Friend2", friends[1].Name);

        }


        [Fact]
        public void DeltaSet_Patch_WithDeletes()
        {
            //Assign
            var deltaSet = new DeltaSet<Friend>(new List<string>() { "Id" });


            var edmChangedObj1 = new Delta<Friend>();
            edmChangedObj1.TrySetPropertyValue("Id", 1);
            edmChangedObj1.TrySetPropertyValue("Name", "Friend1");

            var edmChangedObj2 = new DeltaDeletedEntityObject<Friend>();
            edmChangedObj2.TrySetPropertyValue("Id", 2);
            

            deltaSet.Add(edmChangedObj1);
            deltaSet.Add(edmChangedObj2);

            friends = new List<Friend>();
            friends.Add(new Friend { Id = 1, Name = "Test1" });
            friends.Add(new Friend { Id = 2, Name = "Test2" });

            //Act
            deltaSet.Patch(new FriendPatchHandler());

            //Assert
            Assert.Single(friends);
            Assert.Equal("Friend1", friends[0].Name);
        }

        [Fact]
        public void DeltaSet_Patch_WithInstanceAnnotations()
        {
            //Assign
            
            var deltaSet = new DeltaSet<Friend>((new List<string>() { "Id" }));


            var edmChangedObj1 = new Delta<Friend>();
            edmChangedObj1.TrySetPropertyValue("Id", 1);
            edmChangedObj1.TrySetPropertyValue("Name", "Friend1");

            var annotation = new ODataInstanceAnnotationContainer();
            annotation.AddResourceAnnotation("NS.Test1", 1);
            edmChangedObj1.TrySetPropertyValue("InstanceAnnotations", annotation);

            var edmChangedObj2 = new DeltaDeletedEntityObject<Friend>();
            edmChangedObj2.TrySetPropertyValue("Id", 2);
        
            edmChangedObj2.TransientInstanceAnnotationContainer = new ODataInstanceAnnotationContainer();
            edmChangedObj2.TransientInstanceAnnotationContainer.AddResourceAnnotation("Core.ContentID", 3);

            deltaSet.Add(edmChangedObj1);
            deltaSet.Add(edmChangedObj2);

            friends = new List<Friend>();
            friends.Add(new Friend { Id = 1, Name = "Test1" });
            friends.Add(new Friend { Id = 2, Name = "Test2" });

            //Act
            var coll = deltaSet.Patch(new FriendPatchHandler()).ToArray();

            //Assert
            Assert.Single(friends);
            Assert.Equal("Friend1", friends[0].Name);
            var changedObj = coll[0] as Delta<Friend>;
            Assert.NotNull(changedObj);
            
            object obj;
            changedObj.TryGetPropertyValue("InstanceAnnotations",out obj);
            var annotations = (obj as IODataInstanceAnnotationContainer).GetResourceAnnotations();
            Assert.Equal("NS.Test1", annotations.First().Key);
            Assert.Equal(1, annotations.First().Value);

            DeltaDeletedEntityObject<Friend> changedObj1 = coll[1] as DeltaDeletedEntityObject<Friend>;
            Assert.NotNull(changedObj1);

            annotations = changedObj1.TransientInstanceAnnotationContainer.GetResourceAnnotations();
            Assert.Equal("Core.ContentID", annotations.First().Key);
            Assert.Equal(3, annotations.First().Value);
        }

        [Fact]
        public void DeltaSet_Patch_WithNestedDelta()
        {
            //Assign

            var lstId = new List<string>();
            lstId.Add("Id");

            var deltaSet = new DeltaSet<Friend>(lstId);

            var deltaSet1 = new DeltaSet<NewFriend>(lstId);

            var edmNewObj1 = new Delta<NewFriend>();
            edmNewObj1.TrySetPropertyValue("Id", 1);
            edmNewObj1.TrySetPropertyValue("Name", "NewFriend1");

            var edmNewObj2 = new Delta<NewFriend>();
            edmNewObj2.TrySetPropertyValue("Id", 2);
            edmNewObj2.TrySetPropertyValue("Name", "NewFriend2");

            deltaSet1.Add(edmNewObj1);
            deltaSet1.Add(edmNewObj2);

            var deltaSet2 = new DeltaSet<NewFriend>(lstId);

            var edmNewObj21 = new Delta<NewFriend>();
            edmNewObj21.TrySetPropertyValue("Id", 3);
            edmNewObj21.TrySetPropertyValue("Name", "NewFriend3");

            var edmNewObj22 = new Delta<NewFriend>();
            edmNewObj22.TrySetPropertyValue("Id", 4);
            edmNewObj22.TrySetPropertyValue("Name", "NewFriend4");

            deltaSet2.Add(edmNewObj21);
            deltaSet2.Add(edmNewObj22);

            var edmChangedObj1 = new Delta<Friend>();
            edmChangedObj1.TrySetPropertyValue("Id", 1);
            edmChangedObj1.TrySetPropertyValue("Name", "Friend1");
            edmChangedObj1.TrySetPropertyValue("NewFriends", deltaSet1);

            var edmChangedObj2 = new Delta<Friend>();
            edmChangedObj2.TrySetPropertyValue("Id", 2);
            edmChangedObj2.TrySetPropertyValue("Name", "Friend2");
            edmChangedObj2.TrySetPropertyValue("NewFriends", deltaSet2);

            deltaSet.Add(edmChangedObj1);
            deltaSet.Add(edmChangedObj2);

            friends = new List<Friend>();
            friends.Add(new Friend { Id = 1, Name = "Test1" });
            friends.Add(new Friend { Id = 2, Name = "Test2", NewFriends= new List<NewFriend>() { new NewFriend {Id=3, Name="Test33" }, new NewFriend { Id = 4, Name = "Test44" } } });

            //Act
            deltaSet.Patch(new FriendPatchHandler());

            //Assert
            Assert.Equal(2, friends.Count);
            Assert.Equal("Friend1", friends[0].Name);
            Assert.Equal("Friend2", friends[1].Name);

            Assert.Equal(2, friends[0].NewFriends.Count);
            Assert.Equal(2, friends[1].NewFriends.Count);

            Assert.Equal("NewFriend1", friends[0].NewFriends[0].Name);
            Assert.Equal("NewFriend2", friends[0].NewFriends[1].Name);
            Assert.Equal("NewFriend3", friends[1].NewFriends[0].Name);
            Assert.Equal("NewFriend4", friends[1].NewFriends[1].Name);
        }

    }

    public class FriendPatchHandler : PatchMethodHandler<Friend>
    {
        public override IPatchMethodHandler GetNestedPatchHandler(Friend parent, string navigationPropertyName)
        {
            return new NewFriendPatchHandler(parent);
        }

        public override PatchStatus TryCreate(Delta<Friend> deltaFriend, out Friend createdObject, out string errorMessage)
        {
            createdObject = new Friend();
            DeltaSetOfTTest.friends.Add(createdObject);
            errorMessage = string.Empty;
            return PatchStatus.Success;
        }

        public override PatchStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage)
        {
            int id = Int32.Parse( keyValues.First().Value.ToString());

            DeltaSetOfTTest.friends.Remove(DeltaSetOfTTest.friends.First(x => x.Id == id));
            errorMessage = string.Empty;

            return PatchStatus.Success;
        }

        public override PatchStatus TryGet(IDictionary<string, object> keyValues, out Friend originalObject, out string errorMessage)
        {
            int id = Int32.Parse(keyValues.First().Value.ToString());
            originalObject = DeltaSetOfTTest.friends.First(x => x.Id == id);
            errorMessage = string.Empty;

            return PatchStatus.Success;
        }
    }

    public class NewFriendPatchHandler : PatchMethodHandler<NewFriend>
    {
        Friend parent;
        public NewFriendPatchHandler(Friend parent)
        {
            this.parent = parent;
        }

        public override IPatchMethodHandler GetNestedPatchHandler(NewFriend parent, string navigationPropertyName)
        {
            throw new NotImplementedException();
        }

        public override PatchStatus TryCreate(Delta<NewFriend> deltaFriend, out NewFriend createdObject, out string errorMessage)
        {
            createdObject = new NewFriend();
            if(parent.NewFriends == null)
            {
                parent.NewFriends = new List<NewFriend>();
            }

            parent.NewFriends.Add(createdObject);
            errorMessage = string.Empty;
            return PatchStatus.Success;
        }

        public override PatchStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage)
        {
            int id = Int32.Parse(keyValues.First().Value.ToString());

            parent.NewFriends.Remove(parent.NewFriends.First(x => x.Id == id));
            errorMessage = string.Empty;

            return PatchStatus.Success;
        }

        public override PatchStatus TryGet(IDictionary<string, object> keyValues, out NewFriend originalObject, out string errorMessage)
        {
            errorMessage = string.Empty;
            originalObject = null;

            if(parent.NewFriends == null)
            {
                return PatchStatus.NotFound;
            }

            int id = Int32.Parse(keyValues.First().Value.ToString());
            originalObject = parent.NewFriends.FirstOrDefault(x => x.Id == id);
            errorMessage = string.Empty;

            return originalObject!=null? PatchStatus.Success : PatchStatus.NotFound;
        }
    }
}
