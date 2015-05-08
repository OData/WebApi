// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    public class DefaultODataETagHandlerTests
    {
        public static TheoryDataSet<object> CreateAndParseETagForValue_DataSet
        {
            get
            {
                return new TheoryDataSet<object>
                {
                    (bool)true,
                    (string)"123",
                    (int)123,
                    (long)123,
                    (float)123.123,
                    (double)123.123,
                    (decimal)123.123,
                    Guid.Empty,
                    DateTime.FromBinary(0),
                    TimeSpan.FromSeconds(86456),
                    DateTimeOffset.FromFileTime(0).ToUniversalTime(),
                };
            }
        }

        [Theory]
        [PropertyData("CreateAndParseETagForValue_DataSet")]
        public void DefaultODataETagHandler_RoundTrips(object value)
        {
            // Arrange
            DefaultODataETagHandler handler = new DefaultODataETagHandler();
            Dictionary<string, object> properties = new Dictionary<string, object> { { "Any", value } };

            // Act
            EntityTagHeaderValue etagHeaderValue = handler.CreateETag(properties);
            IList<object> values = handler.ParseETag(etagHeaderValue).Select(p => p.Value).ToList();

            // Assert
            Assert.True(etagHeaderValue.IsWeak);
            Assert.Equal(1, values.Count);
            Assert.Equal(value, values[0]);
        }

        [Fact]
        public void CreateETag_ETagCreatedAndParsed_GivenValues()
        {
            // Arrange
            object[] values = new object[] { "any", 1 };
            DefaultODataETagHandler handler = new DefaultODataETagHandler();
            Dictionary<string, object> properties = new Dictionary<string, object>();
            for (int i = 0; i < values.Length; i++)
            {
                properties.Add("Prop" + i, values[i]);
            }

            // Act
            EntityTagHeaderValue etagHeaderValue = handler.CreateETag(properties);
            IList<object> results = handler.ParseETag(etagHeaderValue).OrderBy(p => p.Key).Select(p => p.Value).ToList();

            // Assert
            Assert.True(etagHeaderValue.IsWeak);
            Assert.Equal(values.Length, results.Count);
            for (int i = 0; i < values.Length; i++)
            {
                Assert.Equal(values[i], results[i]);
            }
        }
    }
}
