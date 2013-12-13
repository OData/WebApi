// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Core;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Routing
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

        [Fact]
        public void ParseKeys_ThrowsODataException_DuplicateKey()
        {
            Assert.Throws<ODataException>(() => KeyValueParser.ParseKeys("id=1,id=2"),
                "Duplicate key 'id' found in segment 'id=1,id=2'.");
        }
    }
}
