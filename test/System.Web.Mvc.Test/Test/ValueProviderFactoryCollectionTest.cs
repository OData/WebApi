// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    public class ValueProviderFactoryCollectionTest
    {
        [Fact]
        public void ListWrappingConstructor()
        {
            // Arrange
            List<ValueProviderFactory> list = new List<ValueProviderFactory>()
            {
                new FormValueProviderFactory()
            };

            // Act
            ValueProviderFactoryCollection collection = new ValueProviderFactoryCollection(list);

            // Assert
            Assert.Equal(list, collection.ToList());
        }

        [Fact]
        public void ListWrappingConstructorThrowsIfListIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { new ValueProviderFactoryCollection(null, null); },
                "list");
        }

        [Fact]
        public void DefaultConstructor()
        {
            // Act
            ValueProviderFactoryCollection collection = new ValueProviderFactoryCollection();

            // Assert
            Assert.Empty(collection);
        }

        [Fact]
        public void AddNullValueProviderFactoryThrows()
        {
            // Arrange
            ValueProviderFactoryCollection collection = new ValueProviderFactoryCollection();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { collection.Add(null); },
                "item");
        }

        [Fact]
        public void GetValueProvider()
        {
            // Arrange
            ControllerContext controllerContext = new ControllerContext();
            IValueProvider[] expectedValueProviders = new[]
            {
                new Mock<IValueProvider>().Object,
                new Mock<IValueProvider>().Object
            };

            Mock<ValueProviderFactory> mockFactory1 = new Mock<ValueProviderFactory>();
            mockFactory1.Setup(o => o.GetValueProvider(controllerContext)).Returns(expectedValueProviders[0]);
            Mock<ValueProviderFactory> mockFactory2 = new Mock<ValueProviderFactory>();
            mockFactory2.Setup(o => o.GetValueProvider(controllerContext)).Returns(expectedValueProviders[1]);

            ValueProviderFactoryCollection factories = new ValueProviderFactoryCollection()
            {
                mockFactory1.Object,
                mockFactory2.Object
            };

            // Act
            ValueProviderCollection valueProviders = (ValueProviderCollection)factories.GetValueProvider(controllerContext);

            // Assert
            Assert.Equal(expectedValueProviders, valueProviders.ToArray());
        }

        [Fact]
        public void GetValueProviderDelegatesToResolver()
        {
            //Arrange
            ControllerContext controllerContext = new ControllerContext();
            IValueProvider[] expectedValueProviders = new[]
            {
                new Mock<IValueProvider>().Object,
                new Mock<IValueProvider>().Object
            };

            Mock<ValueProviderFactory> mockFactory1 = new Mock<ValueProviderFactory>();
            mockFactory1.Setup(o => o.GetValueProvider(controllerContext)).Returns(expectedValueProviders[0]);
            Mock<ValueProviderFactory> mockFactory2 = new Mock<ValueProviderFactory>();
            mockFactory2.Setup(o => o.GetValueProvider(controllerContext)).Returns(expectedValueProviders[1]);

            Resolver<IEnumerable<ValueProviderFactory>> resolver = new Resolver<IEnumerable<ValueProviderFactory>> { Current = new[] { mockFactory1.Object, mockFactory2.Object } };
            ValueProviderFactoryCollection factories = new ValueProviderFactoryCollection(resolver);

            // Act
            ValueProviderCollection valueProviders = (ValueProviderCollection)factories.GetValueProvider(controllerContext);

            // Assert
            Assert.Equal(expectedValueProviders, valueProviders.ToArray());
        }

        [Fact]
        public void SetItem()
        {
            // Arrange
            ValueProviderFactoryCollection collection = new ValueProviderFactoryCollection();
            collection.Add(new Mock<ValueProviderFactory>().Object);

            ValueProviderFactory newFactory = new Mock<ValueProviderFactory>().Object;

            // Act
            collection[0] = newFactory;

            // Assert
            Assert.Single(collection);
            Assert.Equal(newFactory, collection[0]);
        }

        [Fact]
        public void SetNullValueProviderFactoryThrows()
        {
            // Arrange
            ValueProviderFactoryCollection collection = new ValueProviderFactoryCollection();
            collection.Add(new Mock<ValueProviderFactory>().Object);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { collection[0] = null; },
                "item");
        }
    }
}
