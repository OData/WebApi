// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Collections.Generic
{
    public class CollectionExtensionsTest
    {
        [Fact]
        public void AsArray_Array_ReturnsSameInstance()
        {
            object[] array = new object[] { new object(), new object() };

            object[] arrayAsArray = ((IEnumerable<object>)array).AsArray();

            Assert.Same(array, arrayAsArray);
        }

        [Fact]
        public void AsArray_Enumerable_Copies()
        {
            IList<object> list = new List<object>() { new object(), new object() };
            object[] listToArray = list.ToArray();

            object[] listAsArray = ((IEnumerable<object>)list).AsArray();

            Assert.Equal(listToArray, listAsArray);
        }

        [Fact]
        public void AsCollection_Collection_ReturnsSameInstance()
        {
            Collection<object> collection = new Collection<object>() { new object(), new object() };

            Collection<object> collectionAsCollection = ((IEnumerable<object>)collection).AsCollection();

            Assert.Same(collection, collectionAsCollection);
        }

        [Fact]
        public void AsCollection_Enumerable_Copies()
        {
            IEnumerable<object> enumerable = new LinkedList<object>(new object[] { new object(), new object() });

            Collection<object> enumerableAsCollection = ((IEnumerable<object>)enumerable).AsCollection();

            Assert.Equal(enumerable, ((IEnumerable<object>)enumerableAsCollection));
        }

        [Fact]
        public void AsCollection_IList_Wraps()
        {
            IList<object> list = new List<object>() { new object(), new object() };

            Collection<object> listAsCollection = list.AsCollection();
            list.Add(new object());

            Assert.Equal(list, listAsCollection.ToList());
        }

        [Fact]
        public void AsIList_IList_ReturnsSameInstance()
        {
            List<object> list = new List<object> { new object(), new object() };

            IList<object> listAsIList = ((IEnumerable<object>)list).AsIList();

            Assert.Same(list, listAsIList);
        }

        [Fact]
        public void AsIList_Enumerable_Copies()
        {
            LinkedList<object> enumerable = new LinkedList<object>();
            enumerable.AddLast(new object());
            enumerable.AddLast(new object());
            List<object> expected = enumerable.ToList();

            IList<object> enumerableAsIList = ((IEnumerable<object>)enumerable).AsIList();

            Assert.Equal(expected, enumerableAsIList);
            Assert.NotSame(expected, enumerableAsIList);
        }
        
        [Fact]
        public void AsList_List_ReturnsSameInstance()
        {
            List<object> list = new List<object> { new object(), new object() };

            List<object> listAsList = ((IEnumerable<object>)list).AsList();

            Assert.Same(list, listAsList);
        }

        [Fact]
        public void AsList_Enumerable_Copies()
        {
            List<object> list = new List<object>() { new object(), new object() };
            object[] array = list.ToArray();

            List<object> arrayAsList = ((IEnumerable<object>)array).AsList();

            Assert.Equal(list, arrayAsList);
            Assert.NotSame(list, arrayAsList);
            Assert.NotSame(array, arrayAsList);
        }

        public void AsList_ListWrapperCollection_ReturnsSameInstance()
        {
            List<object> list = new List<object> { new object(), new object() };
            ListWrapperCollection<object> listWrapper = new ListWrapperCollection<object>(list);

            List<object> listWrapperAsList = ((IEnumerable<object>)listWrapper).AsList();

            Assert.Same(list, listWrapperAsList);
        }

        [Fact]
        public void SingleDefaultOrErrorIListEmptyReturnsNull()
        {
            IList<object> empty = new List<object>();
            object errorArgument = new object();
            Action<object> errorAction = (object argument) =>
            {
                throw new InvalidOperationException();
            };

            Assert.Null(empty.SingleDefaultOrError(errorAction, errorArgument));
        }

        [Fact]
        public void SingleDefaultOrErrorIListSingleReturns()
        {
            IList<object> single = new List<object>() { new object() };
            object errorArgument = new object();
            Action<object> errorAction = (object argument) =>
            {
                throw new InvalidOperationException();
            };

            Assert.Equal(single[0], single.SingleDefaultOrError(errorAction, errorArgument));
        }

        [Fact]
        public void SingleDefaultOrErrorIListMultipleThrows()
        {
            IList<object> multiple = new List<object>() { new object(), new object() };
            object errorArgument = new object();
            Action<object> errorAction = (object argument) =>
            {
                Assert.Equal(errorArgument, argument);
                throw new InvalidOperationException();
            };

            Assert.Throws<InvalidOperationException>(() => multiple.SingleDefaultOrError(errorAction, errorArgument));
        }

        [Fact]
        public void SingleOfTypeDefaultOrErrorIListNoMatchReturnsNull()
        {
            IList<object> noMatch = new List<object>() { new object(), new object() };
            object errorArgument = new object();
            Action<object> errorAction = (object argument) =>
            {
                throw new InvalidOperationException();
            };

            Assert.Null(noMatch.SingleOfTypeDefaultOrError<object, string, object>(errorAction, errorArgument));
        }

        [Fact]
        public void SingleOfTypeDefaultOrErrorIListOneMatchReturns()
        {
            IList<object> singleMatch = new List<object>() { new object(), "Match", new object() };
            object errorArgument = new object();
            Action<object> errorAction = (object argument) =>
            {
                throw new InvalidOperationException();
            };

            Assert.Equal("Match", singleMatch.SingleOfTypeDefaultOrError<object, string, object>(errorAction, errorArgument));
        }

        [Fact]
        public void SingleOfTypeDefaultOrErrorIListMultipleMatchesThrows()
        {
            IList<object> multipleMatch = new List<object>() { new object(), "Match1", new object(), "Match2" };
            object errorArgument = new object();
            Action<object> errorAction = (object argument) =>
            {
                Assert.Equal(errorArgument, argument);
                throw new InvalidOperationException();
            };

            Assert.Throws<InvalidOperationException>(() => multipleMatch.SingleOfTypeDefaultOrError<object, string, object>(errorAction, errorArgument));
        }
    }
}
