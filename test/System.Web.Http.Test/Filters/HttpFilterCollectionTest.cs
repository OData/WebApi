// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Filters
{
    public class HttpFilterCollectionTest
    {
        private readonly IFilter _filter = new Mock<IFilter>().Object;
        private readonly HttpFilterCollection _collection = new HttpFilterCollection();

        [Fact]
        public void Add_WhenFilterParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => _collection.Add(filter: null), "filter");
        }

        [Fact]
        public void Add_AddsFilterWithGlobalScope()
        {
            _collection.Add(_filter);

            Assert.Same(_filter, _collection.First().Instance);
            Assert.Equal(FilterScope.Global, _collection.First().Scope);
        }

        [Fact]
        public void Add_AllowsAddingSameInstanceMultipleTimes()
        {
            _collection.Add(_filter);
            _collection.Add(_filter);

            Assert.Equal(2, _collection.Count);
        }

        [Fact]
        public void Clear_EmptiesCollection()
        {
            _collection.Add(_filter);

            _collection.Clear();

            Assert.Equal(0, _collection.Count);
        }

        [Fact]
        public void Contains_WhenFilterNotInCollection_ReturnsFalse()
        {
            Assert.False(_collection.Contains(_filter));
        }

        [Fact]
        public void Contains_WhenFilterInCollection_ReturnsTrue()
        {
            _collection.Add(_filter);

            Assert.True(_collection.Contains(_filter));
        }

        [Fact]
        public void Count_WhenCollectionIsEmpty_ReturnsZero()
        {
            Assert.Equal(0, _collection.Count);
        }

        [Fact]
        public void Count_WhenItemAddedToCollection_ReturnsOne()
        {
            _collection.Add(_filter);

            Assert.Equal(1, _collection.Count);
        }

        [Fact]
        public void Remove_WhenCollectionDoesNotHaveFilter_DoesNothing()
        {
            _collection.Remove(_filter);

            Assert.Equal(0, _collection.Count);
        }

        [Fact]
        public void Remove_WhenCollectionHasFilter_RemovesIt()
        {
            _collection.Add(_filter);
            _collection.Add(_filter);

            _collection.Remove(_filter);

            Assert.Equal(0, _collection.Count);
        }
    }
}
