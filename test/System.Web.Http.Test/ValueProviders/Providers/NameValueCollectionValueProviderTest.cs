// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.ValueProviders.Providers
{
    public class NameValueCollectionValueProviderTest
    {
        private static readonly NameValueCollection _backingStore = new NameValueCollection()
        {
            { "foo", "fooValue1" },
            { "foo", "fooValue2" },
            { "bar.baz", "someOtherValue" }
        };

        [Fact]
        public void Constructor_GuardClauses()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new NameValueCollectionValueProvider(values: null, culture: CultureInfo.InvariantCulture),
                "values");

            Assert.ThrowsArgumentNull(
                () => new NameValueCollectionValueProvider(valuesFactory: null, culture: CultureInfo.InvariantCulture),
                "valuesFactory");
        }

        [Fact]
        public void ContainsPrefix_GuardClauses()
        {
            // Arrange
            var valueProvider = new NameValueCollectionValueProvider(_backingStore, null);

            // Act & assert
            Assert.ThrowsArgumentNull(
                () => valueProvider.ContainsPrefix(null),
                "prefix");
        }

        [Fact]
        public void ContainsPrefix_WithEmptyCollection_ReturnsFalseForEmptyPrefix()
        {
            // Arrange
            var valueProvider = new NameValueCollectionValueProvider(new NameValueCollection(), null);

            // Act
            bool result = valueProvider.ContainsPrefix("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsPrefix_WithNonEmptyCollection_ReturnsTrueForEmptyPrefix()
        {
            // Arrange
            var valueProvider = new NameValueCollectionValueProvider(_backingStore, null);

            // Act
            bool result = valueProvider.ContainsPrefix("");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainsPrefix_WithNonEmptyCollection_ReturnsTrueForKnownPrefixes()
        {
            // Arrange
            var valueProvider = new NameValueCollectionValueProvider(_backingStore, null);

            // Act & Assert
            Assert.True(valueProvider.ContainsPrefix("foo"));
            Assert.True(valueProvider.ContainsPrefix("bar"));
            Assert.True(valueProvider.ContainsPrefix("bar.baz"));
        }

        [Fact]
        public void ContainsPrefix_WithNonEmptyCollection_ReturnsFalseForUnknownPrefix()
        {
            // Arrange
            var valueProvider = new NameValueCollectionValueProvider(_backingStore, null);

            // Act
            bool result = valueProvider.ContainsPrefix("biff");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetKeysFromPrefix_GuardClauses()
        {
            // Arrange
            var valueProvider = new NameValueCollectionValueProvider(_backingStore, null);

            // Act & assert
            Assert.ThrowsArgumentNull(
                () => valueProvider.GetKeysFromPrefix(null),
                "prefix");
        }

        [Fact]
        public void GetKeysFromPrefix_EmptyPrefix_ReturnsAllPrefixes()
        {
            // Arrange
            var valueProvider = new NameValueCollectionValueProvider(_backingStore, null);

            // Act
            IDictionary<string, string> result = valueProvider.GetKeysFromPrefix("");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("foo", result["foo"]);
            Assert.Equal("bar", result["bar"]);
        }

        [Fact]
        public void GetKeysFromPrefix_UnknownPrefix_ReturnsEmptyDictionary()
        {
            // Arrange
            var valueProvider = new NameValueCollectionValueProvider(_backingStore, null);

            // Act
            IDictionary<string, string> result = valueProvider.GetKeysFromPrefix("abc");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetKeysFromPrefix_KnownPrefix_ReturnsMatchingItems()
        {
            // Arrange
            var valueProvider = new NameValueCollectionValueProvider(_backingStore, null);

            // Act
            IDictionary<string, string> result = valueProvider.GetKeysFromPrefix("bar");

            // Assert
            KeyValuePair<string, string> kvp = Assert.Single(result);
            Assert.Equal("baz", kvp.Key);
            Assert.Equal("bar.baz", kvp.Value);
        }

        [Fact]
        public void GetValue_GuardClauses()
        {
            // Arrange
            var valueProvider = new NameValueCollectionValueProvider(_backingStore, null);

            // Act & assert
            Assert.ThrowsArgumentNull(
                () => valueProvider.GetValue(null),
                "key");
        }

        [Fact]
        public void GetValue_SingleValue()
        {
            // Arrange
            var culture = CultureInfo.GetCultureInfo("fr-FR");
            var valueProvider = new NameValueCollectionValueProvider(_backingStore, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("bar.baz");

            // Assert
            Assert.NotNull(vpResult);
            Assert.Equal(new[] { "someOtherValue" }, (string[])vpResult.RawValue);
            Assert.Equal("someOtherValue", vpResult.AttemptedValue);
            Assert.Equal(culture, vpResult.Culture);
        }

        [Fact]
        public void GetValue_MultiValue()
        {
            // Arrange
            var culture = CultureInfo.GetCultureInfo("fr-FR");
            var valueProvider = new NameValueCollectionValueProvider(_backingStore, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("foo");

            // Assert
            Assert.NotNull(vpResult);
            Assert.Equal(new[] { "fooValue1", "fooValue2" }, (string[])vpResult.RawValue);
            Assert.Equal("fooValue1,fooValue2", vpResult.AttemptedValue);
            Assert.Equal(culture, vpResult.Culture);
        }

        [Fact]
        public void GetValue_ReturnsNullIfKeyNotFound()
        {
            // Arrange
            var valueProvider = new NameValueCollectionValueProvider(_backingStore, null);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("bar");

            // Assert
            Assert.Null(vpResult);
        }
    }
}
