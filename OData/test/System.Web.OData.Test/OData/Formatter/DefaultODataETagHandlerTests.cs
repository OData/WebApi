// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Web.OData.Formatter
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
                    (long)123123123123,
                    (float)123.123,
                    (double)123123123123.123,
                    Guid.Empty,
                    new DateTimeOffset(DateTime.FromBinary(0), TimeSpan.Zero),
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

        [Theory]
        [InlineData("1", new object[] { "any", 1 })]
        public void CreateETag_ETagCreatedAndParsed_GivenValues(string notUsed, object[] values)
        {
            // Arrange
            DefaultODataETagHandler handler = new DefaultODataETagHandler();
            Dictionary<string, object> properties = new Dictionary<string, object>();
            for (int i = 0; i < values.Length; i++)
            {
                properties.Add("Prop" + i, values[i]);
            }

            // Act
            EntityTagHeaderValue etagHeaderValue = handler.CreateETag(properties);
            IList<object> results = handler.ParseETag(etagHeaderValue).Select(p => p.Value).ToList();

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
