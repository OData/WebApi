// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Core;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing
{
    public class KeyValueParserTest
    {
        [Theory]
        [InlineData("", new string[0])]
        [InlineData("      ", new string[0])]
        [InlineData("123", new[] { ":123" })]
        [InlineData("'123'", new[] { ":'123'" })]
        [InlineData("@1", new[] { ":@1" })]
        [InlineData("id=1", new[] { "id:1" })]
        [InlineData("id1=,id2=12", new[] { "id1:", "id2:12" })]
        [InlineData("id1=1,id2=2", new[] { "id1:1", "id2:2" })]
        [InlineData("id1='1',id2=2", new[] { "id1:'1'", "id2:2" })]
        [InlineData("id1='12''3',id2=2", new[] { "id1:'12''3'", "id2:2" })]
        [InlineData("id1='12''3'''", new[] { "id1:'12''3'''" })]
        [InlineData("id1=''''''", new[] { "id1:''''''" })]
        [InlineData("id1=',,==''',id2=2", new[] { "id1:',,=='''", "id2:2" })]
        [InlineData("id1='1',id2='2'", new[] { "id1:'1'", "id2:'2'" })]
        [InlineData("id1=guid'1',id2='2'", new[] { "id1:guid'1'", "id2:'2'" })]
        [InlineData("'='", new[] { ":'='" })]
        [InlineData("'a=b'", new[] { ":'a=b'" })]
        public void ParseKeys(string str, IEnumerable<string> expectedKeyValues)
        {
            var result = KeyValueParser.ParseKeys(str);
            Assert.Equal(result.Select(r => r.Key + ":" + r.Value).OrderBy(r => r), expectedKeyValues.OrderBy(k => k));
        }

        [Fact]
        public void ParseKeys_ThrowsODataException_UnterminatedStringLiteral()
        {
            Assert.Throws<ODataException>(() => KeyValueParser.ParseKeys("id1='123"),
                "Unterminated string literal at 4 in segment 'id1='123'.");
        }

        [Theory]
        [InlineData("'a''bc", "The literal ''a''bc' has a bad format in segment ''a''bc'.")]
        [InlineData("'a'b'c'", "The literal ''a'b'c'' has a bad format in segment ''a'b'c''.")]
        [InlineData("id=''123''", "The literal '''123''' has a bad format in segment 'id=''123'''.")]
        [InlineData("id1=123,id2=12''3", "The literal '12''3' has a bad format in segment 'id1=123,id2=12''3'.")]
        public void ParseKeys_ThrowsODataException_HasABadFormatForSingleQuote(string segment, string expectedError)
        {
            Assert.Throws<ODataException>(() => KeyValueParser.ParseKeys(segment), expectedError);
        }

        [Fact]
        public void ParseKeys_ThrowsODataException_SegmentHasNoKeyName()
        {
            Assert.Throws<ODataException>(() => KeyValueParser.ParseKeys("id=1,'='"),
                "No key name was found at 5 in segment 'id=1,'=''.");
        }

        [Theory]
        [InlineData("a''''''", "The count of single quotes in non-string literal 'a''''''' must be 0 or 2 in segment 'a'''''''.")]
        [InlineData("guid'a''b'c", "The count of single quotes in non-string literal 'guid'a''b'c' must be 0 or 2 in segment 'guid'a''b'c'.")]
        [InlineData("id1=123,id2=123''abc''", "The count of single quotes in non-string literal '123''abc''' must be 0 or 2 in segment 'id1=123,id2=123''abc'''.")]
        public void ParseKeys_ThrowsODataException_InvalidCountOfSingleQuoteForNonStringLiteral(string segment, string expectedError)
        {
            Assert.Throws<ODataException>(() => KeyValueParser.ParseKeys(segment), expectedError);
        }

        [Fact]
        public void ParseKeys_ThrowsODataException_DuplicateKey()
        {
            Assert.Throws<ODataException>(() => KeyValueParser.ParseKeys("id=1,id=2"),
                "Duplicate key 'id' found in segment 'id=1,id=2'.");
        }
    }
}
