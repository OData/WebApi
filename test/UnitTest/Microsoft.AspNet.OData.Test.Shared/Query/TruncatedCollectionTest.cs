//-----------------------------------------------------------------------------
// <copyright file="TruncatedCollectionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Common;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class TruncatedCollectionTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Collection_Enumerable_Source()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new TruncatedCollection<int>(source: null, pageSize: 10), "source");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_Collection_Queryable_Source()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new TruncatedCollection<int>(source: null, pageSize: 10, parameterize: false), "source");
        }

        [Fact]
        public void Ctor_ThrowsArgumentGreater_Collection()
        {
            ExceptionAssert.ThrowsArgumentGreaterThanOrEqualTo(
                () => new TruncatedCollection<int>(source: new int[0], pageSize: 0), "pageSize", "1", "0");
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(3, false)]
        [InlineData(10, false)]
        public void Property_IsTruncated(int pageSize, bool expectedResult)
        {
            TruncatedCollection<int> collection = new TruncatedCollection<int>(new[] { 1, 2, 3 }, pageSize);
            Assert.Equal(expectedResult, collection.IsTruncated);
        }

        [Fact]
        public void Property_PageSize()
        {
            int pageSize = 42;
            TruncatedCollection<int> collection = new TruncatedCollection<int>(new[] { 1, 2, 3 }, pageSize);
            Assert.Equal(pageSize, collection.PageSize);
        }

        [Fact]
        public void GetEnumerator_Truncates_IfPageSizeIsLessThanCollectionSize()
        {
            TruncatedCollection<int> collection = new TruncatedCollection<int>(new[] { 1, 2, 3 }, pageSize: 2);

            Assert.Equal(new[] { 1, 2 }, collection);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(42)]
        public void GetEnumerator_DoesNotTruncate_IfPageSizeIsGreaterThanOrEqualToCollectionSize(int pageSize)
        {
            TruncatedCollection<int> collection = new TruncatedCollection<int>(new[] { 1, 2, 3 }, pageSize);

            Assert.Equal(new[] { 1, 2, 3 }, collection);
        }
    }
}
