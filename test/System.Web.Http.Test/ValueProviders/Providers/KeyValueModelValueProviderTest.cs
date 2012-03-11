using System.Collections.Generic;
using System.Globalization;
using System.Net.Http.Formatting;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.ValueProviders.Providers
{
    public class KeyValueModelValueProviderTest
    {
        private readonly IKeyValueModel _keyValueModel;

        public KeyValueModelValueProviderTest()
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            values["foo"] = "fooValue";
            values["int"] = 55;
            values["bar.baz"] = "someOtherValue";

            _keyValueModel = new KeyValueModel(values);
        }

        [Fact]
        public void ContainsPrefix_GuardClauses()
        {
            // Arrange
            var valueProvider = new KeyValueModelValueProvider(_keyValueModel, null);

            // Act & assert
            Assert.ThrowsArgumentNull(() => valueProvider.ContainsPrefix(null), "prefix");
        }

        [Fact]
        public void ContainsPrefix_WithEmptyCollection_ReturnsFalseForEmptyPrefix()
        {
            // Arrange
            var valueProvider = new KeyValueModelValueProvider(new KeyValueModel(new Dictionary<string, object>()), null);

            // Act
            bool result = valueProvider.ContainsPrefix("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsPrefix_WithNonEmptyCollection_ReturnsTrueForEmptyPrefix()
        {
            // Arrange
            var valueProvider = new KeyValueModelValueProvider(_keyValueModel, null);

            // Act
            bool result = valueProvider.ContainsPrefix("");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainsPrefix_WithNonEmptyCollection_ReturnsTrueForKnownPrefixes()
        {
            // Arrange
            var valueProvider = new KeyValueModelValueProvider(_keyValueModel, null);

            // Act & Assert
            Assert.True(valueProvider.ContainsPrefix("foo"));
            Assert.True(valueProvider.ContainsPrefix("bar"));
            Assert.True(valueProvider.ContainsPrefix("bar.baz"));
        }

        [Fact]
        public void ContainsPrefix_WithNonEmptyCollection_ReturnsFalseForUnknownPrefix()
        {
            // Arrange
            var valueProvider = new KeyValueModelValueProvider(_keyValueModel, null);

            // Act
            bool result = valueProvider.ContainsPrefix("biff");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetKeysFromPrefix_GuardClauses()
        {
            // Arrange
            var valueProvider = new KeyValueModelValueProvider(_keyValueModel, null);

            // Act & assert
            Assert.ThrowsArgumentNull(() => valueProvider.GetKeysFromPrefix(null), "prefix");
        }

        [Fact]
        public void GetKeysFromPrefix_EmptyPrefix_ReturnsAllPrefixes()
        {
            // Arrange
            var valueProvider = new KeyValueModelValueProvider(_keyValueModel, null);

            // Act
            IDictionary<string, string> result = valueProvider.GetKeysFromPrefix("");

            // Assert
            Assert.Equal(5, result.Count);
            Assert.Equal("", result[""]);
            Assert.Equal("foo", result["foo"]);
            Assert.Equal("int", result["int"]);
            Assert.Equal("bar", result["bar"]);
            Assert.Equal("bar.baz", result["bar.baz"]);
        }

        [Fact]
        public void GetKeysFromPrefix_UnknownPrefix_ReturnsEmptyDictionary()
        {
            // Arrange
            var valueProvider = new KeyValueModelValueProvider(_keyValueModel, null);

            // Act
            IDictionary<string, string> result = valueProvider.GetKeysFromPrefix("abc");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetKeysFromPrefix_KnownPrefix_ReturnsMatchingItems()
        {
            // Arrange
            var valueProvider = new KeyValueModelValueProvider(_keyValueModel, null);

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
            var valueProvider = new KeyValueModelValueProvider(_keyValueModel, null);

            // Act & assert
            Assert.ThrowsArgumentNull(() => valueProvider.GetValue(null), "key"); ;
        }

        [Fact]
        public void GetValue_SingleValue_String()
        {
            // Arrange
            var culture = CultureInfo.GetCultureInfo("fr-FR");
            var valueProvider = new KeyValueModelValueProvider(_keyValueModel, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("bar.baz");

            // Assert
            Assert.NotNull(vpResult);
            Assert.Equal("someOtherValue", (string)vpResult.RawValue);
            Assert.Equal("someOtherValue", vpResult.AttemptedValue);
            Assert.Equal(culture, vpResult.Culture);
        }

        [Fact]
        public void GetValue_SingleValue_Not_String()
        {
            // Arrange
            var culture = CultureInfo.GetCultureInfo("fr-FR");
            var valueProvider = new KeyValueModelValueProvider(_keyValueModel, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("int");

            // Assert
            Assert.NotNull(vpResult);
            Assert.Equal(55, (int)vpResult.RawValue);
            Assert.Equal("55", vpResult.AttemptedValue);
            Assert.Equal(culture, vpResult.Culture);
        }

        [Fact]
        public void GetValue_ReturnsNullIfKeyNotFound()
        {
            // Arrange
            var valueProvider = new KeyValueModelValueProvider(_keyValueModel, null);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("bar");

            // Assert
            Assert.Null(vpResult);
        }

        class KeyValueModel : IKeyValueModel
        {
            private Dictionary<string, object> _values;

            public KeyValueModel(Dictionary<string, object> values)
            {
                _values = values;
            }

            public bool TryGetValue(string key, out object value)
            {
                return _values.TryGetValue(key, out value);
            }

            public IEnumerable<string> Keys
            {
                get { return _values.Keys; }
            }
        }
    }
}
