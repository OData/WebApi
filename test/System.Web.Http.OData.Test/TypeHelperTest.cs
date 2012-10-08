// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using Microsoft.TestCommon;

namespace System.Web.Http.OData
{
    public class TypeHelperTest
    {
        public static TheoryDataSet<Type, Type> CollectionTypesData
        {
            get
            {
                return new TheoryDataSet<Type, Type>
                {
                    { typeof(ICollection<string>), typeof(string) },
                    { typeof(IList<string>), typeof(string) },
                    { typeof(List<int>), typeof(int) },
                    { typeof(IsCollection_with_Collections_TestClass), typeof(bool) },
                    { typeof(IEnumerable<int>), typeof(int) },
                    { typeof(int[]), typeof(int) },
                    { typeof(MyCustomCollection), typeof(int) },
                };
            }
        }

        [Theory]
        [PropertyData("CollectionTypesData")]
        public void IsCollection_with_Collections(Type collectionType, Type elementType)
        {
            Type type;
            Assert.True(collectionType.IsCollection(out type));
            Assert.Equal(elementType, type);
        }

        [Theory]
        [PropertyData("CollectionTypesData")]
        public void GetInnerElementType(Type collectionType, Type elementType)
        {
            Assert.Equal(elementType, collectionType.GetInnerElementType());
        }

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(ICollection))]
        [InlineData(typeof(IEnumerable))]
        [InlineData(typeof(string))]
        public void IsCollection_with_NonCollections(Type type)
        {
            Assert.False(type.IsCollection());
        }

        private sealed class IsCollection_with_Collections_TestClass : List<bool>
        {
        }

        private class MyCustomCollection : List<int>
        {
        }
    }
}
