// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Globalization;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
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
        public void Constructor_ThrowsIfCollectionIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new NameValueCollectionValueProvider(null, CultureInfo.InvariantCulture); }, "collection");
        }

        [Fact]
        public void ContainsPrefix()
        {
            // Arrange
            NameValueCollectionValueProvider valueProvider = new NameValueCollectionValueProvider(_backingStore, null);

            // Act
            bool result = valueProvider.ContainsPrefix("bar");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainsPrefix_DoesNotContainEmptyPrefixIfBackingStoreIsEmpty()
        {
            // Arrange
            NameValueCollectionValueProvider valueProvider = new NameValueCollectionValueProvider(new NameValueCollection(), null);

            // Act
            bool result = valueProvider.ContainsPrefix("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsPrefix_ThrowsIfPrefixIsNull()
        {
            // Arrange
            NameValueCollectionValueProvider valueProvider = new NameValueCollectionValueProvider(_backingStore, null);

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { valueProvider.ContainsPrefix(null); }, "prefix");
        }

        [Fact]
        public void GetValue()
        {
            // Arrange
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            NameValueCollectionValueProvider valueProvider = new NameValueCollectionValueProvider(_backingStore, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("foo");

            // Assert
            Assert.NotNull(vpResult);
            Assert.Equal(_backingStore.GetValues("foo"), (string[])vpResult.RawValue);
            Assert.Equal("fooValue1,fooValue2", vpResult.AttemptedValue);
            Assert.Equal(culture, vpResult.Culture);
        }

        [Fact]
        public void GetValue_NonValidating()
        {
            // Arrange
            NameValueCollection unvalidatedCollection = new NameValueCollection()
            {
                { "foo", "fooValue3" },
                { "foo", "fooValue4" }
            };

            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            NameValueCollectionValueProvider valueProvider = new NameValueCollectionValueProvider(_backingStore, unvalidatedCollection, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("foo", skipValidation: true);

            // Assert
            Assert.NotNull(vpResult);
            Assert.Equal(new[] { "fooValue3", "fooValue4" }, (string[])vpResult.RawValue);
            Assert.Equal("fooValue3,fooValue4", vpResult.AttemptedValue);
            Assert.Equal(culture, vpResult.Culture);
        }

        [Fact]
        public void GetValue_NonValidating_NoUnvalidatedCollectionSpecified_UsesDefaultCollectionValue()
        {
            // Arrange
            CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
            NameValueCollectionValueProvider valueProvider = new NameValueCollectionValueProvider(_backingStore, null, culture);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("foo", skipValidation: true);

            // Assert
            Assert.NotNull(vpResult);
            Assert.Equal(_backingStore.GetValues("foo"), (string[])vpResult.RawValue);
            Assert.Equal("fooValue1,fooValue2", vpResult.AttemptedValue);
            Assert.Equal(culture, vpResult.Culture);
        }

        [Fact]
        public void GetValue_ReturnsNullIfKeyNotFound()
        {
            // Arrange
            NameValueCollectionValueProvider valueProvider = new NameValueCollectionValueProvider(_backingStore, null);

            // Act
            ValueProviderResult vpResult = valueProvider.GetValue("bar");

            // Assert
            Assert.Null(vpResult);
        }

        [Fact]
        public void GetValue_ThrowsIfKeyIsNull()
        {
            // Arrange
            NameValueCollectionValueProvider valueProvider = new NameValueCollectionValueProvider(_backingStore, null);

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { valueProvider.GetValue(null); }, "key");
        }
    }
}
