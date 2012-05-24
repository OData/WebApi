// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    public class GlobalFilterCollectionTest
    {
        private GlobalFilterCollection _collection = new GlobalFilterCollection();
        private object _filterInstance = GetFilterInstance<IActionFilter>();

        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [CLSCompliant(false)]
        public void AddRejectsNonFilterInstances(object instance)
        {
            // Act + Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                _collection.Add(instance);
            }, "The given filter instance must implement one or more of the following filter interfaces: IAuthorizationFilter, IActionFilter, IResultFilter, IExceptionFilter.");
        }

        [Fact]
        public void AddAcceptsFilterInstances()
        {
            // Arrange
            var filters = new object[] {
                GetFilterInstance<IActionFilter>(),
                GetFilterInstance<IAuthorizationFilter>(),
                GetFilterInstance<IResultFilter>(),
                GetFilterInstance<IExceptionFilter>() 
            }.ToList();

            // Act
            filters.ForEach(f => _collection.Add(f));

            // Assert
            Assert.Equal(filters, _collection.Select(i => i.Instance));
        }

        [Fact]
        public void AddPlacesFilterInGlobalScope()
        {
            // Act
            _collection.Add(_filterInstance);

            // Assert
            Filter filter = Assert.Single(_collection);
            Assert.Same(_filterInstance, filter.Instance);
            Assert.Equal(FilterScope.Global, filter.Scope);
            Assert.Equal(-1, filter.Order);
        }

        [Fact]
        public void AddWithOrderPlacesFilterInGlobalScope()
        {
            // Act
            _collection.Add(_filterInstance, 42);

            // Assert
            Filter filter = Assert.Single(_collection);
            Assert.Same(_filterInstance, filter.Instance);
            Assert.Equal(FilterScope.Global, filter.Scope);
            Assert.Equal(42, filter.Order);
        }

        [Fact]
        public void ContainsFindsFilterByInstance()
        {
            // Arrange
            _collection.Add(_filterInstance);

            // Act
            bool result = _collection.Contains(_filterInstance);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void RemoveDeletesFilterByInstance()
        {
            // Arrange
            _collection.Add(_filterInstance);

            // Act
            _collection.Remove(_filterInstance);

            // Assert
            Assert.Empty(_collection);
        }

        [Fact]
        public void CollectionIsIFilterProviderWhichReturnsAllFilters()
        {
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            _collection.Add(_filterInstance);
            var provider = (IFilterProvider)_collection;

            // Act
            IEnumerable<Filter> result = provider.GetFilters(context, descriptor);

            // Assert
            Filter filter = Assert.Single(result);
            Assert.Same(_filterInstance, filter.Instance);
        }

        private static TFilter GetFilterInstance<TFilter>() where TFilter : class
        {
            return new Mock<TFilter>().Object;
        }
    }
}
