// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Text;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class PlainTextODataQueryOptionsParserTest
    {
        private const string QueryOptionsString = "$filter=Id le 5";

        [Fact]
        public void Parse_WithQueryOptionsInStream()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(QueryOptionsString));

            var result = new PlainTextODataQueryOptionsParser().Parse(memoryStream);

            Assert.Equal('?' + QueryOptionsString, result);
        }

        [Fact]
        public void Parse_WithDisposedStream()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(QueryOptionsString));
            memoryStream.Dispose();

            Assert.Throws<ODataException>(() => new PlainTextODataQueryOptionsParser().Parse(memoryStream));
        }

        [Fact]
        public void Parse_WithReadStream()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(QueryOptionsString));
            var reader = new StreamReader(memoryStream);

            var result = new PlainTextODataQueryOptionsParser().Parse(memoryStream);
            
            Assert.Equal('?' + QueryOptionsString, result);
            reader.Dispose();
        }

        [Fact]
        public void Parse_WithEmptyStream()
        {
            var memoryStream = new MemoryStream();

            var result = new PlainTextODataQueryOptionsParser().Parse(memoryStream);

            Assert.Equal("", result);
        }

        [Fact]
        public void Parse_WithQueryStringSeparatorIncludedInStream()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes('?' + QueryOptionsString));

            var result = new PlainTextODataQueryOptionsParser().Parse(memoryStream);

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
