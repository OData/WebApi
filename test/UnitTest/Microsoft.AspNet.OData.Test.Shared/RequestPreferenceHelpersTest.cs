//-----------------------------------------------------------------------------
// <copyright file="RequestPreferenceHelpersTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Interfaces;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class RequestPreferenceHelpersTest
    {
        [Fact]
        public void RequestPrefersMaxPageSize_ReturnsFalse_WhenMaxPageSizeIsZero()
        {
            // Arrange
            var headers = new FakeWebApiHeaders("maxpagesize=0");

            // Act
            var result = RequestPreferenceHelpers.RequestPrefersMaxPageSize(headers, out int pageSize);

            // Assert - a client-supplied maxpagesize=0 must not disable server-driven paging
            Assert.False(result);
        }

        [Fact]
        public void RequestPrefersMaxPageSize_ReturnsFalse_WhenMaxPageSizeIsNegative()
        {
            // Arrange
            var headers = new FakeWebApiHeaders("maxpagesize=-5");

            // Act
            var result = RequestPreferenceHelpers.RequestPrefersMaxPageSize(headers, out int pageSize);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void RequestPrefersMaxPageSize_ReturnsTrue_WithPageSize_WhenMaxPageSizeIsPositive()
        {
            // Arrange
            var headers = new FakeWebApiHeaders("maxpagesize=10");

            // Act
            var result = RequestPreferenceHelpers.RequestPrefersMaxPageSize(headers, out int pageSize);

            // Assert
            Assert.True(result);
            Assert.Equal(10, pageSize);
        }

        [Fact]
        public void RequestPrefersMaxPageSize_IsCaseInsensitive()
        {
            // Arrange
            var headers = new FakeWebApiHeaders("MaxPageSize=15");

            // Act
            var result = RequestPreferenceHelpers.RequestPrefersMaxPageSize(headers, out int pageSize);

            // Assert
            Assert.True(result);
            Assert.Equal(15, pageSize);
        }

        [Fact]
        public void RequestPrefersMaxPageSize_ReturnsTrue_WithPageSize_ForODataMaxPageSize()
        {
            // Arrange
            var headers = new FakeWebApiHeaders("odata.maxpagesize=20");

            // Act
            var result = RequestPreferenceHelpers.RequestPrefersMaxPageSize(headers, out int pageSize);

            // Assert - back-compat: odata.maxpagesize is still honored when positive
            Assert.True(result);
            Assert.Equal(20, pageSize);
        }

        [Fact]
        public void RequestPrefersMaxPageSize_ReturnsFalse_ForBareMaxPageSizeToken_WithoutThrowing()
        {
            // Arrange - a bare "maxpagesize" token with no "=value" must not throw IndexOutOfRange
            var headers = new FakeWebApiHeaders("maxpagesize");

            // Act
            var result = RequestPreferenceHelpers.RequestPrefersMaxPageSize(headers, out int pageSize);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void RequestPrefersMaxPageSize_ReturnsFalse_WhenNoPreferHeaderPresent()
        {
            // Arrange
            var headers = new FakeWebApiHeaders();

            // Act
            var result = RequestPreferenceHelpers.RequestPrefersMaxPageSize(headers, out int pageSize);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void RequestPrefersMaxPageSize_MaxPageSizeZeroSupersedesODataMaxPageSize_ReturnsFalse()
        {
            // Arrange - maxpagesize supersedes odata.maxpagesize; the 0 must win and disable paging
            var headers = new FakeWebApiHeaders("maxpagesize=0, odata.maxpagesize=20");

            // Act
            var result = RequestPreferenceHelpers.RequestPrefersMaxPageSize(headers, out int pageSize);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void RequestPrefersMaxPageSize_MaxPageSizeSupersedesODataMaxPageSize_ReturnsMaxPageSizeValue()
        {
            // Arrange - when both are present and positive, maxpagesize wins
            var headers = new FakeWebApiHeaders("maxpagesize=5, odata.maxpagesize=20");

            // Act
            var result = RequestPreferenceHelpers.RequestPrefersMaxPageSize(headers, out int pageSize);

            // Assert
            Assert.True(result);
            Assert.Equal(5, pageSize);
        }

        private sealed class FakeWebApiHeaders : IWebApiHeaders
        {
            private readonly IDictionary<string, IEnumerable<string>> _headers =
                new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);

            public FakeWebApiHeaders(string preferHeaderValue = null)
            {
                if (preferHeaderValue != null)
                {
                    _headers[RequestPreferenceHelpers.PreferHeaderName] = new[] { preferHeaderValue };
                }
            }

            public bool TryGetValues(string key, out IEnumerable<string> values)
            {
                return _headers.TryGetValue(key, out values);
            }
        }
    }
}
