//-----------------------------------------------------------------------------
// <copyright file="EdmChangedObjectCollectionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    internal class TypelessAPIHandlerFactory : ODataEdmAPIHandlerFactory
    {
        IEdmEntityType entityType;
        IEdmStructuredObject employee;

        protected TypelessAPIHandlerFactory(IEdmModel model): base(model)
        {
         
        }

        public TypelessAPIHandlerFactory(IEdmEntityType entityType, IEdmModel model): base(model)
        {
            this.entityType = entityType;
        }

        public TypelessAPIHandlerFactory(IEdmEntityType entityType, IEdmStructuredObject employee, IEdmModel model) : base(model)
        {
            this.entityType = entityType;
            this.employee = employee;
        }

        public override EdmODataAPIHandler GetHandler(NavigationPath navigationPath)
        {
            if (navigationPath != null)
            {
                var pathItems = navigationPath;
                
                if (pathItems == null)
                {
                    switch (navigationPath.Last().Name)
                    {                       
                        case "UnTypedFriend":
                        case "Friend":
                            return new FriendTypelessPatchHandler(entityType);

                        default:
                            return null;
                    }
                }
                               
            }

            return null;
        }

    }

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

        public static List<IEdmStructuredObject> friends = new List<IEdmStructuredObject>();

        internal void InitFriends()
        {
            friends = new List<IEdmStructuredObject>();
            EdmEntityType _entityType = new EdmEntityType("Microsoft.AspNet.OData.Test", "Friend");
            _entityType.AddKeys(_entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            _entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
         
            EdmEntityType _entityType1 = new EdmEntityType("Microsoft.AspNet.OData.Test", "NewFriend");
            _entityType1.AddKeys(_entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            _entityType1.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);

            var friend1 = new EdmEntityObject(_entityType);
            friend1.TrySetPropertyValue("Id", 1);
            friend1.TrySetPropertyValue("Name", "Test1");

            var friend2 = new EdmEntityObject(_entityType);
            friend2.TrySetPropertyValue("Id", 2);
            friend2.TrySetPropertyValue("Name", "Test2");

            var nfriend1 = new EdmEntityObject(_entityType1);
            nfriend1.TrySetPropertyValue("Id", 1);
            nfriend1.TrySetPropertyValue("Name", "Test1");

            var nfriend2 = new EdmEntityObject(_entityType1);
            nfriend2.TrySetPropertyValue("Id", 2);
            nfriend2.TrySetPropertyValue("Name", "Test2");

            var nfriends = new List<EdmStructuredObject>();
            nfriends.Add(nfriend1);
            nfriends.Add(nfriend2);

            friend1.TrySetPropertyValue("NewFriends", nfriends);

            friends.Add(friend1);
            friends.Add(friend2);
        }


        [Fact]
        public void EdmChangedObjectCollection_Patch()
        {
            //Assign
            InitFriends();
            EdmEntityType _entityType = new EdmEntityType("Microsoft.AspNet.OData.Test", "Friend");
            _entityType.AddKeys(_entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            _entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);

            var lstId = new List<string>();
            lstId.Add("Id");
            var deltaSet = new EdmChangedObjectCollection(_entityType);

            var edmChangedObj1 = new EdmDeltaEntityObject(_entityType);
            edmChangedObj1.TrySetPropertyValue("Id", 1);
            edmChangedObj1.TrySetPropertyValue("Name", "Friend1");

            var edmChangedObj2 = new EdmDeltaEntityObject(_entityType);
            edmChangedObj2.TrySetPropertyValue("Id", 2);
            edmChangedObj2.TrySetPropertyValue("Name", "Friend2");

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            var friendsSet = builder.EntitySet<Friend>("Friends");
            var model = builder.GetEdmModel();

            var keys = new[] { new KeyValuePair<string, object>("Id", 1) };
            var lst1 = new List<ODataPathSegment>();
            lst1.Add(new EntitySetSegment(model.EntityContainer.FindEntitySet("Friends")) { Identifier = "NewFriends" });
            lst1.Add(new KeySegment(keys, null, null));

            var keys2 = new[] { new KeyValuePair<string, object>("Id", 2) };
            var lst2 = new List<ODataPathSegment>();
            lst2.Add(new EntitySetSegment(model.EntityContainer.FindEntitySet("Friends")) { Identifier = "NewFriends" });
            lst2.Add(new KeySegment(keys2, null, null));

            edmChangedObj1.ODataPath = new ODataPath(lst1);
            edmChangedObj2.ODataPath = new ODataPath(lst2);

            deltaSet.Add(edmChangedObj1);
            deltaSet.Add(edmChangedObj2);

            //Act
            deltaSet.Patch(new FriendTypelessPatchHandler(_entityType), new TypelessAPIHandlerFactory(_entityType, null));

            //Assert
            Assert.Equal(2, friends.Count);
            object obj;
            friends[0].TryGetPropertyValue("Name", out obj);
            Assert.Equal("Friend1", obj );
            friends[1].TryGetPropertyValue("Name", out obj);
            Assert.Equal("Friend2", obj);

        }


        [Fact]
        public void EdmChangedObjectCollection_Patch_WithDeletes()
        {
            //Assign
            InitFriends();
            EdmEntityType _entityType = new EdmEntityType("Microsoft.AspNet.OData.Test", "Friend");
            _entityType.AddKeys(_entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            _entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);

            var changedObjCollection = new EdmChangedObjectCollection(_entityType);

            var edmChangedObj1 = new EdmDeltaEntityObject(_entityType);
            edmChangedObj1.TrySetPropertyValue("Id", 1);
            edmChangedObj1.TrySetPropertyValue("Name", "Friend1");

            var edmChangedObj2 = new EdmDeltaDeletedEntityObject(_entityType);
            edmChangedObj2.TrySetPropertyValue("Id", 2);
            edmChangedObj2.TrySetPropertyValue("Name", "Friend2");

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            var friendsSet = builder.EntitySet<Friend>("Friends");
            var model = builder.GetEdmModel();

            var keys = new[] { new KeyValuePair<string, object>("Id", 1) };
            var lst1 = new List<ODataPathSegment>();
            lst1.Add(new EntitySetSegment(model.EntityContainer.FindEntitySet("Friends")) { Identifier = "NewFriends" });
            lst1.Add(new KeySegment(keys, null, null));

            var keys2 = new[] { new KeyValuePair<string, object>("Id", 2) };
            var lst2 = new List<ODataPathSegment>();
            lst2.Add(new EntitySetSegment(model.EntityContainer.FindEntitySet("Friends")) { Identifier = "NewFriends" });
            lst2.Add(new KeySegment(keys2, null, null));

            edmChangedObj1.ODataPath = new ODataPath(lst1);
            edmChangedObj2.ODataPath = new ODataPath(lst2);

            changedObjCollection.Add(edmChangedObj1);
            changedObjCollection.Add(edmChangedObj2);

            //Act
            changedObjCollection.Patch(new FriendTypelessPatchHandler(_entityType), new TypelessAPIHandlerFactory(_entityType, null));

            //Assert
            Assert.Single(friends);
            object obj;
            friends[0].TryGetPropertyValue("Name", out obj);
            Assert.Equal("Friend1", obj);
      
        }

        [Fact]
        public void EdmChangedObjectCollection_Patch_WithInstanceAnnotations()
        {
            //Assign
            InitFriends();
            EdmEntityType _entityType = new EdmEntityType("Microsoft.AspNet.OData.Test", "Friend");
            _entityType.AddKeys(_entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            _entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);

     

            var changedObjCollection = new EdmChangedObjectCollection(_entityType);

            var edmChangedObj1 = new EdmDeltaEntityObject(_entityType);
            edmChangedObj1.TrySetPropertyValue("Id", 1);
            edmChangedObj1.TrySetPropertyValue("Name", "Friend1");
            edmChangedObj1.PersistentInstanceAnnotationsContainer = new ODataInstanceAnnotationContainer();
            edmChangedObj1.PersistentInstanceAnnotationsContainer.AddResourceAnnotation("NS.Test", 1);

            var edmChangedObj2 = new EdmDeltaEntityObject(_entityType);
            edmChangedObj2.TrySetPropertyValue("Id", 2);
            edmChangedObj2.TrySetPropertyValue("Name", "Friend2");

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            var friendsSet = builder.EntitySet<Friend>("Friends");
            var model = builder.GetEdmModel();

            var keys = new[] { new KeyValuePair<string, object>("Id", 1) };
            var lst1 = new List<ODataPathSegment>();
            lst1.Add(new EntitySetSegment(model.EntityContainer.FindEntitySet("Friends")) { Identifier = "NewFriends" });
            lst1.Add(new KeySegment(keys, null, null));

            var keys2 = new[] { new KeyValuePair<string, object>("Id", 2) };
            var lst2 = new List<ODataPathSegment>();
            lst2.Add(new EntitySetSegment(model.EntityContainer.FindEntitySet("Friends")) { Identifier = "NewFriends" });
            lst2.Add(new KeySegment(keys2, null, null));

            edmChangedObj1.ODataPath = new ODataPath(lst1);
            edmChangedObj2.ODataPath = new ODataPath(lst2);

            changedObjCollection.Add(edmChangedObj1);
            changedObjCollection.Add(edmChangedObj2);

            //Act
            var coll= changedObjCollection.Patch(new FriendTypelessPatchHandler(_entityType), new TypelessAPIHandlerFactory(_entityType, null));

            //Assert
            Assert.Equal(2, friends.Count);
            object obj;
            friends[0].TryGetPropertyValue("Name", out obj);
            Assert.Equal("Friend1", obj);

            var edmObj = coll[0] as EdmDeltaEntityObject;

            Assert.Equal("NS.Test", edmObj.PersistentInstanceAnnotationsContainer.GetResourceAnnotations().First().Key);
            Assert.Equal(1, edmObj.PersistentInstanceAnnotationsContainer.GetResourceAnnotations().First().Value);

            friends[1].TryGetPropertyValue("Name", out obj);
            Assert.Equal("Friend2", obj);
        }

    }

    public class Friend
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public List<NewFriend> NewFriends { get; set; }

        public IODataInstanceAnnotationContainer InstanceAnnotations { get; set; }
    }

    public class NewFriend
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    internal class FriendTypelessPatchHandler : EdmODataAPIHandler
    {
        IEdmEntityType entityType;

        public FriendTypelessPatchHandler(IEdmEntityType entityType)
        {            
            this.entityType = entityType;
        }

        public override ODataAPIResponseStatus TryCreate(IEdmChangedObject changedObject, out IEdmStructuredObject createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new EdmEntityObject(entityType);
  
                EdmChangedObjectCollectionTest.friends.Add(createdObject);
                
                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override ODataAPIResponseStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var id = keyValues.First().Value.ToString();
    
                foreach (var emp in EdmChangedObjectCollectionTest.friends)
                {
                    object id1;
                    emp.TryGetPropertyValue("Id", out id1);

                    if (id == id1.ToString())
                    {
                        EdmChangedObjectCollectionTest.friends.Remove(emp);

                        break;
                    }
                }


                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out IEdmStructuredObject originalObject, out string errorMessage)
        {
            ODataAPIResponseStatus status = ODataAPIResponseStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                var id = keyValues["Id"].ToString();

                foreach (var friend in EdmChangedObjectCollectionTest.friends)
                {
                    object id1;
                    friend.TryGetPropertyValue("Id", out id1);

                    if (id == id1.ToString())
                    {
                        originalObject = friend;
                        break;
                    }
                }


                if (originalObject == null)
                {
                    status = ODataAPIResponseStatus.NotFound;
                }

            }
            catch (Exception ex)
            {
                status = ODataAPIResponseStatus.Failure;
                errorMessage = ex.Message;
            }

            return status;
        }

        public override EdmODataAPIHandler GetNestedHandler(IEdmStructuredObject parent, string navigationPropertyName)
        {
            switch (navigationPropertyName)
            {
                case "NewFriends":
                    return new NewFriendTypelessPatchHandler(parent, entityType.DeclaredNavigationProperties().First().Type.Definition.AsElementType() as IEdmEntityType);
                default:
                    return null;
            }
        }

    }

    internal class NewFriendTypelessPatchHandler : EdmODataAPIHandler
    {
        IEdmEntityType entityType;
        EdmStructuredObject friend;

        public NewFriendTypelessPatchHandler(IEdmStructuredObject friend, IEdmEntityType entityType)
        {
            this.entityType = entityType;
            this.friend = friend as EdmStructuredObject;
        }

        public override ODataAPIResponseStatus TryCreate(IEdmChangedObject changedObject, out IEdmStructuredObject createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new EdmEntityObject(entityType);

                object obj;
                friend.TryGetPropertyValue("NewFriends", out obj);

                var nfriends = obj as List<IEdmStructuredObject>;

                nfriends.Add(createdObject);

                friend.TrySetPropertyValue("NewFriends", nfriends);

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override ODataAPIResponseStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                object obj;
                friend.TryGetPropertyValue("NewFriends", out obj);

                var nfriends = obj as List<EdmStructuredObject>;

                var id = keyValues.First().Value.ToString();

                foreach (var frnd in nfriends)
                {
                    object id1;
                    frnd.TryGetPropertyValue("Id", out id1);

                    if (id == id1.ToString())
                    {
                        nfriends.Remove(frnd);

                        break;
                    }
                }


                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out IEdmStructuredObject originalObject, out string errorMessage)
        {
            ODataAPIResponseStatus status = ODataAPIResponseStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                object obj;
                friend.TryGetPropertyValue("NewFriends", out obj);

                var nfriends = obj as List<IEdmStructuredObject>;

                var id = keyValues.First().Value.ToString();

                foreach (var frnd in nfriends)
                {
                    object id1;
                    frnd.TryGetPropertyValue("Id", out id1);

                    if (id == id1.ToString())
                    {
                        originalObject = frnd;

                        break;
                    }
                }



                if (originalObject == null)
                {
                    status = ODataAPIResponseStatus.NotFound;
                }

            }
            catch (Exception ex)
            {
                status = ODataAPIResponseStatus.Failure;
                errorMessage = ex.Message;
            }

            return status;
        }

        public override EdmODataAPIHandler GetNestedHandler(IEdmStructuredObject parent, string navigationPropertyName)
        {
            return null;
        }

    }

}
