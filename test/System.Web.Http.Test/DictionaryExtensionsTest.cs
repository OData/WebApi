// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http
{
    public class DictionaryExtensionsTest
    {
        [Fact]
        public void IsCorrectType()
        {
            Assert.Type.HasProperties(typeof(DictionaryExtensions), TypeAssert.TypeProperties.IsStatic | TypeAssert.TypeProperties.IsClass);
        }

        [Fact]
        public void TryGetValueThrowsOnNullCollection()
        {
            string value;
            Assert.ThrowsArgumentNull(() => DictionaryExtensions.TryGetValue<string>(null, String.Empty, out value), "collection");
        }

        [Fact]
        public void TryGetValueThrowsOnNullKey()
        {
            IDictionary<string, object> dict = new Dictionary<string, object>();
            string value;
            Assert.ThrowsArgumentNull(() => dict.TryGetValue<string>(null, out value), "key");
        }

        public static TheoryDataSet<object> DictionaryValues
        {
            get
            {
                return new TheoryDataSet<object>
                {
                    "test",
                    new string[] { "A", "B", "C" },
                    8,
                    new List<int> {1, 2, 3},
                    1D,
                    (IEnumerable<double>)new List<double> { 1D, 2D, 3D },
                    new Uri("http://some.host"),
                    Guid.NewGuid(),
                    HttpStatusCode.NotImplemented,
                    new HttpStatusCode[] { HttpStatusCode.Accepted, HttpStatusCode.Ambiguous, HttpStatusCode.BadGateway }
                };
            }
        }

        [Fact]
        public void TryGetValueReturnsFalse()
        {
            // Arrange
            IDictionary<string, object> dict = new Dictionary<string, object>();

            // Act
            string resultValue = null;
            bool result = dict.TryGetValue("notfound", out resultValue);

            // Assert
            Assert.False(result);
            Assert.Null(resultValue);
        }

        [Theory]
        [PropertyData("DictionaryValues")]
        public void TryGetValueReturnsTrue<T>(T value)
        {
            // Arrange
            IDictionary<string, object> dict = new Dictionary<string, object>()
            {
                { "key", value }
            };


            // Act
            T resultValue;
            bool result = DictionaryExtensions.TryGetValue(dict, "key", out resultValue);

            // Assert
            Assert.True(result);
            Assert.Equal(typeof(T), resultValue.GetType());
            Assert.Equal(value, resultValue);
        }

        [Fact]
        public void GetValueThrowsOnNullCollection()
        {
            Assert.ThrowsArgumentNull(() => DictionaryExtensions.GetValue<string>(null, String.Empty), "collection");
        }

        [Fact]
        public void GetValueThrowsOnNullKey()
        {
            IDictionary<string, object> dict = new Dictionary<string, object>();
            Assert.ThrowsArgumentNull(() => dict.GetValue<string>(null), "key");
        }

        [Fact]
        public void GetValueThrowsOnNotFound()
        {
            IDictionary<string, object> dict = new Dictionary<string, object>();
            Assert.Throws<InvalidOperationException>(() => dict.GetValue<string>("notfound"));
        }

        [Theory]
        [PropertyData("DictionaryValues")]
        public void GetValueReturnsValue<T>(T value)
        {
            // Arrange
            IDictionary<string, object> dict = new Dictionary<string, object>()
            {
                { "key", value }
            };

            // Act
            T resultValue = DictionaryExtensions.GetValue<T>(dict, "key");

            // Assert
            Assert.Equal(typeof(T), resultValue.GetType());
            Assert.Equal(value, resultValue);
        }

        [Fact]
        public void FindKeysWithPrefixRecognizesRootChilden()
        {
            // Arrange
            IDictionary<string, int> dict = new Dictionary<string, int>()
            {
                { "[0]", 1 },
                { "Name", 2 },
                { "Address.Street", 3 },
                { "", 4 }
            };

            // Act
            List<int> results = DictionaryExtensions.FindKeysWithPrefix<int>(dict, "").Select(kvp => kvp.Value).ToList();

            // Assert
            Assert.Equal(4, results.Count);
            Assert.Contains(1, results);
            Assert.Contains(2, results);
            Assert.Contains(3, results);
            Assert.Contains(4, results);
        }
    }
}
