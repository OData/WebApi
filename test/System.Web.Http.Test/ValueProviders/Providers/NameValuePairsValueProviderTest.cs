// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.ValueProviders.Providers
{
    public class NameValuePairsValueProviderTest
    {
        private static readonly IEnumerable<KeyValuePair<string, string>> _backingStore = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("foo", "fooValue1"),
            new KeyValuePair<string, string>("foo", "fooValue2"),
            new KeyValuePair<string, string>("bar.baz", "someOtherValue")
        };

        [Fact]
        public void Constructor_GuardClauses()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new NameValuePairsValueProvider(values: null, culture: CultureInfo.InvariantCulture),
                "values");

            Assert.ThrowsArgumentNull(
                () => new NameValuePairsValueProvider(valuesFactory: null, culture: CultureInfo.InvariantCulture),
                "valuesFactory");
        }

        [Fact]
        public void ContainsPrefix_GuardClauses()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

            // Act & assert
            Assert.ThrowsArgumentNull(
                () => valueProvider.ContainsPrefix(null),
                "prefix");
        }

        [Fact]
        public void ContainsPrefix_WithEmptyCollection_ReturnsFalseForEmptyPrefix()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(Enumerable.Empty<KeyValuePair<string, string>>(), null);

            // Act
            bool result = valueProvider.ContainsPrefix("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsPrefix_WithNonEmptyCollection_ReturnsTrueForEmptyPrefix()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

            // Act
            bool result = valueProvider.ContainsPrefix("");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainsPrefix_WithNonEmptyCollection_ReturnsTrueForKnownPrefixes()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

            // Act & Assert
            Assert.True(valueProvider.ContainsPrefix("foo"));
            Assert.True(valueProvider.ContainsPrefix("bar"));
            Assert.True(valueProvider.ContainsPrefix("bar.baz"));
        }

        [Fact]
        public void ContainsPrefix_WithNonEmptyCollection_ReturnsFalseForUnknownPrefix()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

            // Act
            bool result = valueProvider.ContainsPrefix("biff");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetKeysFromPrefix_GuardClauses()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

            // Act & assert
            Assert.ThrowsArgumentNull(
                () => valueProvider.GetKeysFromPrefix(null),
                "prefix");
        }

        [Fact]
        public void GetKeysFromPrefix_EmptyPrefix_ReturnsAllPrefixes()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

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
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

            // Act
            IDictionary<string, string> result = valueProvider.GetKeysFromPrefix("abc");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetKeysFromPrefix_KnownPrefix_ReturnsMatchingItems()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

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
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

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
            var valueProvider = new NameValuePairsValueProvider(_backingStore, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("bar.baz");

            // Assert
            Assert.NotNull(vpResult);
            Assert.Equal("someOtherValue", vpResult.RawValue);
            Assert.Equal("someOtherValue", vpResult.AttemptedValue);
            Assert.Equal(culture, vpResult.Culture);
        }

        [Fact]
        public void GetValue_MultiValue()
        {
            // Arrange
            var culture = CultureInfo.GetCultureInfo("fr-FR");
            var valueProvider = new NameValuePairsValueProvider(_backingStore, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("foo");

            // Assert
            Assert.NotNull(vpResult);
            Assert.Equal(new List<string>() { "fooValue1", "fooValue2" }, (List<string>)vpResult.RawValue);
            Assert.Equal("fooValue1,fooValue2", vpResult.AttemptedValue);
            Assert.Equal(culture, vpResult.Culture);
        }

        [Fact]
        public void GetValue_ReturnsNullIfKeyNotFound()
        {
            // Arrange
            var valueProvider = new NameValuePairsValueProvider(_backingStore, null);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("bar");

            // Assert
            Assert.Null(vpResult);
        }
    }
}
