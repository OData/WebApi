//-----------------------------------------------------------------------------
// <copyright file="DeltaSetOfTTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class DeltaSetOfTTest
    {
       public static List<Friend> friends;

        [Fact]
        public void DeltaSet_Patch()
        {
            //Arrange
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
        public void DeltaSet_Add_WrongItem_ThrowsError()
        {
            //Assign
            
            var edmChangedObjectcollection = new DeltaSet<Friend>(new List<string>() { "Id" });

            var edmChangedObj1 = new Delta<NewFriend>();
            edmChangedObj1.TrySetPropertyValue("Id", 1);
            edmChangedObj1.TrySetPropertyValue("Name", "Friend1");

            //Act & Assert
            Assert.Throws<ArgumentException>(() => edmChangedObjectcollection.Add(edmChangedObj1));
        }



        [Fact]
        public void DeltaSet_Patch_WithDeletes()
        {
            //Arrange
            var deltaSet = new DeltaSet<Friend>(new List<string>() { "Id" });


            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            var friendsSet = builder.EntitySet<Friend>("Friends");
            var model = builder.GetEdmModel();

            var edmChangedObj1 = new Delta<Friend>();
            edmChangedObj1.TrySetPropertyValue("Id", 1);
            edmChangedObj1.TrySetPropertyValue("Name", "Friend1");

            var edmChangedObj2 = new DeltaDeletedEntityObject<Friend>();
            edmChangedObj2.TrySetPropertyValue("Id", 2);

            var lst = new List<ODataPathSegment>();
            lst.Add(new EntitySetSegment(model.EntityContainer.FindEntitySet("Friends")) { Identifier = "Friends" });

            edmChangedObj1.ODataPath = new ODataPath(lst) ;
            edmChangedObj2.ODataPath = new ODataPath(lst);


            deltaSet.Add(edmChangedObj1);
            deltaSet.Add(edmChangedObj2);

            friends = new List<Friend>();
            friends.Add(new Friend { Id = 1, Name = "Test1" });
            friends.Add(new Friend { Id = 2, Name = "Test2" });


            //Act
            deltaSet.Patch(new FriendPatchHandler(), new APIHandlerFactory(model));

            //Assert
            Assert.Single(friends);
            Assert.Equal("Friend1", friends[0].Name);
        }

        [Fact]
        public void DeltaSet_Patch_WithInstanceAnnotations()
        {
            //Arrange
            
            var deltaSet = new DeltaSet<Friend>((new List<string>() { "Id" }));
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            var friendsSet = builder.EntitySet<Friend>("Friends");
            var model = builder.GetEdmModel();

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

            var lst = new List<ODataPathSegment>();
            lst.Add(new EntitySetSegment(model.EntityContainer.FindEntitySet("Friends")) { Identifier = "Friends" });

            edmChangedObj1.ODataPath = new ODataPath(lst);
            edmChangedObj2.ODataPath = new ODataPath(lst);

            deltaSet.Add(edmChangedObj1);
            deltaSet.Add(edmChangedObj2);

            friends = new List<Friend>();
            friends.Add(new Friend { Id = 1, Name = "Test1" });
            friends.Add(new Friend { Id = 2, Name = "Test2" });
 
            //Act
            var coll = deltaSet.Patch(new FriendPatchHandler(), new APIHandlerFactory(model)).ToArray();

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
            //Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            var friendsSet = builder.EntitySet<Friend>("Friends");
            var model = builder.GetEdmModel();

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

          

            var edmChangedObj1 = new Delta<Friend>();
            edmChangedObj1.TrySetPropertyValue("Id", 1);
            edmChangedObj1.TrySetPropertyValue("Name", "Friend1");
            edmChangedObj1.TrySetPropertyValue("NewFriends", deltaSet1);

            var edmChangedObj2 = new Delta<Friend>();
            edmChangedObj2.TrySetPropertyValue("Id", 2);
            edmChangedObj2.TrySetPropertyValue("Name", "Friend2");
            edmChangedObj2.TrySetPropertyValue("NewFriends", deltaSet2);

            var lst1 = new List<ODataPathSegment>();
            lst1.Add(new EntitySetSegment(model.EntityContainer.FindEntitySet("Friends")) { Identifier = "NewFriends" });

            edmNewObj21.ODataPath = new ODataPath(lst1);
            edmNewObj22.ODataPath = new ODataPath(lst1);

            edmNewObj1.ODataPath = new ODataPath(lst1);
            edmNewObj2.ODataPath = new ODataPath(lst1);

            deltaSet2.Add(edmNewObj21);
            deltaSet2.Add(edmNewObj22);

            var lst = new List<ODataPathSegment>();
            lst.Add(new EntitySetSegment(model.EntityContainer.FindEntitySet("Friends")) { Identifier = "Friends" });

            edmChangedObj1.ODataPath = new ODataPath(lst);
            edmChangedObj2.ODataPath = new ODataPath(lst);

            deltaSet.Add(edmChangedObj1);
            deltaSet.Add(edmChangedObj2);

            friends = new List<Friend>();
            friends.Add(new Friend { Id = 1, Name = "Test1" });
            friends.Add(new Friend { Id = 2, Name = "Test2", NewFriends= new List<NewFriend>() { new NewFriend {Id=3, Name="Test33" }, new NewFriend { Id = 4, Name = "Test44" } } });

 

            //Act
            deltaSet.Patch(new FriendPatchHandler(), new APIHandlerFactory(model));

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

    internal class APIHandlerFactory : ODataAPIHandlerFactory
    {
        public APIHandlerFactory(IEdmModel model): base(model)
        {

        }

        public override IODataAPIHandler GetHandler(NavigationPath navigationPath)
        {
            if (navigationPath != null)
            {
                var pathItems = navigationPath;

                if (pathItems == null)
                {
                    switch (pathItems.Last().Name)
                    {   
                        case "Friend":
                            return new FriendPatchHandler();
                       
                        default:
                            return null;
                    }
                }
            }

            return null;
        }
    }
    internal class FriendPatchHandler : ODataAPIHandler<Friend>
    {
        public override IODataAPIHandler GetNestedHandler(Friend parent, string navigationPropertyName)
        {
            return new NewFriendPatchHandler(parent);
        }

        public override ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out Friend createdObject, out string errorMessage)
        {
            createdObject = new Friend();
            DeltaSetOfTTest.friends.Add(createdObject);
            errorMessage = string.Empty;
            return ODataAPIResponseStatus.Success;
        }

        public override ODataAPIResponseStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage)
        {
            int id = Int32.Parse( keyValues.First().Value.ToString());

            DeltaSetOfTTest.friends.Remove(DeltaSetOfTTest.friends.First(x => x.Id == id));
            errorMessage = string.Empty;

            return ODataAPIResponseStatus.Success;
        }

        public override ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out Friend originalObject, out string errorMessage)
        {
            int id = Int32.Parse(keyValues.First().Value.ToString());
            originalObject = DeltaSetOfTTest.friends.First(x => x.Id == id);
            errorMessage = string.Empty;

            return ODataAPIResponseStatus.Success;
        }
    }

    internal class NewFriendPatchHandler : ODataAPIHandler<NewFriend>
    {
        Friend parent;
        public NewFriendPatchHandler(Friend parent)
        {
            this.parent = parent;
        }

        public override IODataAPIHandler GetNestedHandler(NewFriend parent, string navigationPropertyName)
        {
            throw new NotImplementedException();
        }

        public override ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out NewFriend createdObject, out string errorMessage)
        {
            createdObject = new NewFriend();
            if(parent.NewFriends == null)
            {
                parent.NewFriends = new List<NewFriend>();
            }

            parent.NewFriends.Add(createdObject);
            errorMessage = string.Empty;
            return ODataAPIResponseStatus.Success;
        }

        public override ODataAPIResponseStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage)
        {
            int id = Int32.Parse(keyValues.First().Value.ToString());

            parent.NewFriends.Remove(parent.NewFriends.First(x => x.Id == id));
            errorMessage = string.Empty;

            return ODataAPIResponseStatus.Success;
        }

        public override ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out NewFriend originalObject, out string errorMessage)
        {
            errorMessage = string.Empty;
            originalObject = null;

            if(parent.NewFriends == null)
            {
                return ODataAPIResponseStatus.NotFound;
            }

            int id = Int32.Parse(keyValues.First().Value.ToString());
            originalObject = parent.NewFriends.FirstOrDefault(x => x.Id == id);
            errorMessage = string.Empty;

            return originalObject!=null? ODataAPIResponseStatus.Success : ODataAPIResponseStatus.NotFound;
        }
    }
}
