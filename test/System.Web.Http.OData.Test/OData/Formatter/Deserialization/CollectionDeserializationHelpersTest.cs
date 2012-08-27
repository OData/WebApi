// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
