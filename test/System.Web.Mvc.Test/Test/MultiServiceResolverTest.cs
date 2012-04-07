// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    public class MultiServiceResolverTest
    {
        [Fact]
        public void ConstructorWithNullThunkArgumentThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { new MultiServiceResolver<TestProvider>(null); },
                "itemsThunk");
        }

        [Fact]
        public void CurrentPrependsFromResolver()
        {
            // Arrange
            IEnumerable<TestProvider> providersFromServiceLocation = GetProvidersFromService();
            IEnumerable<TestProvider> providersFromItemsThunk = GetProvidersFromItemsThunk();
            IEnumerable<TestProvider> expectedProviders = providersFromServiceLocation.Concat(providersFromItemsThunk);

            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetServices(typeof(TestProvider)))
                .Returns(providersFromServiceLocation);

            MultiServiceResolver<TestProvider> multiResolver = new MultiServiceResolver<TestProvider>(() => providersFromItemsThunk, resolver.Object);

            // Act
            IEnumerable<TestProvider> returnedProviders = multiResolver.Current;

            // Assert
            Assert.Equal(expectedProviders.ToList(), returnedProviders.ToList());
        }

        [Fact]
        public void CurrentCachesResolverResult()
        {
            // Arrange
            IEnumerable<TestProvider> providersFromServiceLocation = GetProvidersFromService();
            IEnumerable<TestProvider> providersFromItemsThunk = GetProvidersFromItemsThunk();
            IEnumerable<TestProvider> expectedProviders = providersFromServiceLocation.Concat(providersFromItemsThunk);

            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetServices(typeof(TestProvider)))
                .Returns(providersFromServiceLocation);

            MultiServiceResolver<TestProvider> multiResolver = new MultiServiceResolver<TestProvider>(() => providersFromItemsThunk, resolver.Object);

            // Act
            IEnumerable<TestProvider> returnedProviders = multiResolver.Current;
            IEnumerable<TestProvider> cachedProviders = multiResolver.Current;

            // Assert
            Assert.Equal(expectedProviders.ToList(), returnedProviders.ToList());
            Assert.Equal(expectedProviders.ToList(), cachedProviders.ToList());
            resolver.Verify(r => r.GetServices(typeof(TestProvider)), Times.Exactly(1));
        }

        [Fact]
        public void CurrentReturnsCurrentItemsWhenResolverReturnsNoInstances()
        {
            // Arrange
            IEnumerable<TestProvider> providersFromItemsThunk = GetProvidersFromItemsThunk();
            MultiServiceResolver<TestProvider> resolver = new MultiServiceResolver<TestProvider>(() => providersFromItemsThunk);

            // Act
            IEnumerable<TestProvider> returnedProviders = resolver.Current;

            // Assert
            Assert.Equal(providersFromItemsThunk.ToList(), returnedProviders.ToList());
        }

        [Fact]
        public void CurrentDoesNotQueryResolverAfterNoInstancesAreReturned()
        {
            // Arrange
            IEnumerable<TestProvider> providersFromItemsThunk = GetProvidersFromItemsThunk();
            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetServices(typeof(TestProvider)))
                .Returns(new TestProvider[0]);
            MultiServiceResolver<TestProvider> multiResolver = new MultiServiceResolver<TestProvider>(() => providersFromItemsThunk, resolver.Object);

            // Act
            IEnumerable<TestProvider> returnedProviders = multiResolver.Current;
            IEnumerable<TestProvider> cachedProviders = multiResolver.Current;

            // Assert
            Assert.Equal(providersFromItemsThunk.ToList(), returnedProviders.ToList());
            Assert.Equal(providersFromItemsThunk.ToList(), cachedProviders.ToList());
            resolver.Verify(r => r.GetServices(typeof(TestProvider)), Times.Exactly(1));
        }

        [Fact]
        public void CurrentPropagatesExceptionWhenResolverThrowsNonActivationException()
        {
            // Arrange
            Mock<IDependencyResolver> resolver = new Mock<IDependencyResolver>(MockBehavior.Strict);
            MultiServiceResolver<TestProvider> multiResolver = new MultiServiceResolver<TestProvider>(() => null, resolver.Object);

            // Act & Assert
            Assert.Throws<MockException>(
                () => multiResolver.Current,
                @"IDependencyResolver.GetServices(System.Web.Mvc.Test.MultiServiceResolverTest+TestProvider) invocation failed with mock behavior Strict.
All invocations on the mock must have a corresponding setup."
                );
        }

        private class TestProvider
        {
        }

        private IEnumerable<TestProvider> GetProvidersFromService()
        {
            return new TestProvider[]
            {
                new TestProvider(),
                new TestProvider()
            };
        }

        private IEnumerable<TestProvider> GetProvidersFromItemsThunk()
        {
            return new TestProvider[]
            {
                new TestProvider(),
                new TestProvider()
            };
        }
    }
}
