// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing
{
    public class KeyValuePathSegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_KeyValueSegment()
        {
            Assert.ThrowsArgumentNull(() => new KeyValuePathSegmentTemplate(keyValueSegment: null), "keyValueSegment");
        }

        [Fact]
        public void Ctor_InitializesPropertyMappings_Properly()
        {
            // Arrange && Act
            KeyValuePathSegmentTemplate template = new KeyValuePathSegmentTemplate(
                new KeyValuePathSegment("Key1={ID1},Key2={ID2}"));

            // Assert
            Assert.NotNull(template.ParameterMappings);
            Assert.Equal(2, template.ParameterMappings.Count);
            Assert.Equal("ID1", template.ParameterMappings["Key1"]);
            Assert.Equal("ID2", template.ParameterMappings["Key2"]);
        }

        [Fact]
        public void TryMatch_SingleKey_ReturnsTrueOnMatch()
        {
            KeyValuePathSegmentTemplate template = new KeyValuePathSegmentTemplate(new KeyValuePathSegment("{ID}"));
            KeyValuePathSegment segment = new KeyValuePathSegment("123");

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.True(result);
            Assert.Equal("123", values["ID"]);
        }

        [Fact]
        public void TryMatch_MultiKey_ReturnsTrueOnMatch()
        {
            KeyValuePathSegmentTemplate template = new KeyValuePathSegmentTemplate(new KeyValuePathSegment("FirstName={key1},LastName={key2}"));
            KeyValuePathSegment segment = new KeyValuePathSegment("FirstName=abc,LastName=xyz");

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.True(result);
            Assert.Equal("abc", values["key1"]);
            Assert.Equal("xyz", values["key2"]);
        }
    }
}
