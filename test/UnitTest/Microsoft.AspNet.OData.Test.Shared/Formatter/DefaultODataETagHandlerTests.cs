//-----------------------------------------------------------------------------
// <copyright file="DefaultODataETagHandlerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter
{
    [Collection("TimeZoneTests")] // TimeZoneInfo is not thread-safe. Tests in this collection will be executed sequentially 
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

                    // ODL has bug in ConvertFromUriLiteral, please uncomment it after fix https://github.com/OData/odata.net/issues/77.
                    // new Date(1997, 7, 1), 
                    new TimeOfDay(10, 11, 12, 13),
                };
            }
        }

        [Theory]
        [MemberData(nameof(CreateAndParseETagForValue_DataSet))]
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
            Assert.Single(values);
            Assert.Equal(value, values[0]);
        }

        [Theory]
        [InlineData("UTC")] // +0:00
        [InlineData("Pacific Standard Time")] // -8:00
        [InlineData("China Standard Time")] // +8:00
        public void DefaultODataETagHandler_DateTime_RoundTrips(string timeZoneId)
        {
            // Arrange
            TimeZoneInfoHelper.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime value = new DateTime(2015, 2, 17, 1, 2, 3, DateTimeKind.Utc);

            DefaultODataETagHandler handler = new DefaultODataETagHandler();
            Dictionary<string, object> properties = new Dictionary<string, object> { { "Any", value } };

            // Act
            EntityTagHeaderValue etagHeaderValue = handler.CreateETag(properties);
            IList<object> values = handler.ParseETag(etagHeaderValue).Select(p => p.Value).ToList();

            // Assert
            Assert.True(etagHeaderValue.IsWeak);
            Assert.Single(values);
            DateTimeOffset result = Assert.IsType<DateTimeOffset>(values[0]);

            Assert.Equal(TimeZoneInfo.ConvertTime(new DateTimeOffset(value), TimeZoneInfoHelper.TimeZone), result);
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
            Assert.NotNull(notUsed);
            Assert.True(etagHeaderValue.IsWeak);
            Assert.Equal(values.Length, results.Count);
            for (int i = 0; i < values.Length; i++)
            {
                Assert.Equal(values[i], results[i]);
            }
        }
    }
}
