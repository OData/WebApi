// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class CollectionDeserializationHelpersTest
    {
        public static TheoryDataSet<IList, IEnumerable> CopyItemsToCollectionData
        {
            get
            {
                IList source = new List<int> { 1, 2, 3 };
                return new TheoryDataSet<IList, IEnumerable>
                {
                    { source, new List<int>() },
                    { source, new Collection<int>() },
                    { source, new CustomCollectionWithAdd<int>() },
                };
            }
        }

        [Theory]
        [PropertyData("CopyItemsToCollectionData")]
        public void CopyItemsToCollection(IList oldCollection, IEnumerable newCollection)
        {
            oldCollection.AddToCollection(newCollection, typeof(int), typeof(CollectionDeserializationHelpersTest), "PropertyName", newCollection.GetType());

            Assert.Equal(
                new[] { 1, 2, 3 },
                newCollection as IEnumerable<int>);
        }

        [Fact]
        public void CopyItemsToCollection_CanConvertToNonStandardEdm()
        {
            IList source = new List<string> { SimpleEnum.First.ToString(), SimpleEnum.Second.ToString(), SimpleEnum.Third.ToString() };
            IEnumerable newCollection = new CustomCollectionWithAdd<SimpleEnum>();

            source.AddToCollection(newCollection, typeof(SimpleEnum), typeof(CollectionDeserializationHelpersTest), "PropertyName", newCollection.GetType());

            Assert.Equal(new[] { SimpleEnum.First, SimpleEnum.Second, SimpleEnum.Third }, newCollection as IEnumerable<SimpleEnum>);
        }

        [Theory]
        [InlineData(typeof(IEnumerable<int>), typeof(int))]
        [InlineData(typeof(ICollection<int>), typeof(int))]
        [InlineData(typeof(IList<int>), typeof(int))]
        [InlineData(typeof(Collection<int>), typeof(int))]
        [InlineData(typeof(List<int>), typeof(int))]
        [InlineData(typeof(LinkedList<int>), typeof(int))]
        public void TryCreateInstance_Creates_AppropriateCollectionObject(Type collectionType, Type elementType)
        {
            IEnumerable result;
            bool created = CollectionDeserializationHelpers.TryCreateInstance(collectionType, null, elementType, out result);

            Assert.True(created);
            Assert.IsAssignableFrom(collectionType, result);
        }

        [Fact]
        public void TryCreateInstance_EdmComplexObjectCollection_SetsEdmType()
        {
            EdmComplexType complexType = new EdmComplexType("NS", "ComplexType");
            IEdmCollectionTypeReference complexCollectionType = 
                new EdmCollectionType(complexType.ToEdmTypeReference(true))
                .ToEdmTypeReference(true).AsCollection();
            
            IEnumerable result;
            CollectionDeserializationHelpers.TryCreateInstance(typeof(EdmComplexObjectCollection), complexCollectionType, typeof(EdmComplexObject), out result);

            var edmObject = Assert.IsType<EdmComplexObjectCollection>(result);
            Assert.Equal(edmObject.GetEdmType(), complexCollectionType, new EdmTypeReferenceEqualityComparer());
        }

        [Fact]
        public void TryCreateInstance_EdmEntityObjectCollection_SetsEdmType()
        {
            EdmEntityType entityType = new EdmEntityType("NS", "EntityType");
            IEdmCollectionTypeReference entityCollectionType =
                new EdmCollectionType(entityType.ToEdmTypeReference(true))
                .ToEdmTypeReference(true).AsCollection();

            IEnumerable result;
            CollectionDeserializationHelpers.TryCreateInstance(typeof(EdmEntityObjectCollection), entityCollectionType, typeof(EdmComplexObject), out result);

            var edmObject = Assert.IsType<EdmEntityObjectCollection>(result);
            Assert.Equal(edmObject.GetEdmType(), entityCollectionType, new EdmTypeReferenceEqualityComparer());
        }

        private class CustomCollectionWithAdd<T> : IEnumerable<T>
        {
            List<T> _list = new List<T>();

            public void Add(T item)
            {
                _list.Add(item);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }
    }
}
