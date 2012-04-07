// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Moq;
using Xunit;

namespace System.Web.Mvc.Test
{
    public class GlobalFilterCollectionTest
    {
        [Fact]
        public void AddPlacesFilterInGlobalScope()
        {
            // Arrange
            var filterInstance = new object();
            var collection = new GlobalFilterCollection();

            // Act
            collection.Add(filterInstance);

            // Assert
            Filter filter = Assert.Single(collection);
            Assert.Same(filterInstance, filter.Instance);
            Assert.Equal(FilterScope.Global, filter.Scope);
            Assert.Equal(-1, filter.Order);
        }

        [Fact]
        public void AddWithOrderPlacesFilterInGlobalScope()
        {
            // Arrange
            var filterInstance = new object();
            var collection = new GlobalFilterCollection();

            // Act
            collection.Add(filterInstance, 42);

            // Assert
            Filter filter = Assert.Single(collection);
            Assert.Same(filterInstance, filter.Instance);
            Assert.Equal(FilterScope.Global, filter.Scope);
            Assert.Equal(42, filter.Order);
        }

        [Fact]
        public void ContainsFindsFilterByInstance()
        {
            // Arrange
            var filterInstance = new object();
            var collection = new GlobalFilterCollection();
            collection.Add(filterInstance);

            // Act
            bool result = collection.Contains(filterInstance);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void RemoveDeletesFilterByInstance()
        {
            // Arrange
            var filterInstance = new object();
            var collection = new GlobalFilterCollection();
            collection.Add(filterInstance);

            // Act
            collection.Remove(filterInstance);

            // Assert
            Assert.Empty(collection);
        }

        [Fact]
        public void CollectionIsIFilterProviderWhichReturnsAllFilters()
        {
            // Arrange
            var context = new ControllerContext();
            var descriptor = new Mock<ActionDescriptor>().Object;
            var filterInstance = new object();
            var collection = new GlobalFilterCollection();
            collection.Add(filterInstance);
            var provider = (IFilterProvider)collection;

            // Act
            IEnumerable<Filter> result = provider.GetFilters(context, descriptor);

            // Assert
            Filter filter = Assert.Single(result);
            Assert.Same(filterInstance, filter.Instance);
        }
    }
}
