// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class PlainTextODataQueryOptionsParserTest
    {
        private const string QueryOptionsString = "$filter=Id le 5";

        [Fact]
        public async Task Parse_WithQueryOptionsInStream()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(QueryOptionsString));

            var result = await new PlainTextODataQueryOptionsParser().ParseAsync(memoryStream);

            Assert.Equal('?' + QueryOptionsString, result);
        }

        [Fact]
        public async Task Parse_WithDisposedStream()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(QueryOptionsString));
            memoryStream.Dispose();

            await Assert.ThrowsAsync<ODataException>(async() => await new PlainTextODataQueryOptionsParser().ParseAsync(memoryStream));
        }

        [Fact]
        public async Task Parse_WithEmptyStream()
        {
            var memoryStream = new MemoryStream();

            var result = await new PlainTextODataQueryOptionsParser().ParseAsync(memoryStream);

            Assert.Equal("", result);
        }

        [Fact]
        public async Task Parse_WithQueryStringSeparatorIncludedInStream()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes('?' + QueryOptionsString));

            var result = await new PlainTextODataQueryOptionsParser().ParseAsync(memoryStream);

            Assert.Equal('?' + QueryOptionsString, result);
        }

        [Fact]
        public void PlainTextODataQueryOptionsParser_IsReturnedBy_ODataQueryOptionsParserFactory()
        {
            var parsers = ODataQueryOptionsParserFactory.Create();

            Assert.Contains(parsers, p => p.GetType().Equals(typeof(PlainTextODataQueryOptionsParser)));
        }
    }
}
