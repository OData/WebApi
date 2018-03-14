// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.UriParser;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing.Template
{
    public class DynamicSegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_OpenPropertySegment()
        {
            // Assert
            Assert.ThrowsArgumentNull(() => new DynamicSegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void TryMatch_AlwaysTrueWhenParameterName()
        {
            // Arrange
            DynamicSegmentTemplate template = new DynamicSegmentTemplate(new DynamicPathSegment("{parameter}"));
            DynamicPathSegment segment = new DynamicPathSegment("property");

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.True(result);
            Assert.True(values.ContainsKey("parameter"));
            Assert.Equal("property", values["parameter"]);
        }

        [Theory]
        [InlineData(false, "notmatchingproperty")]
        [InlineData(true, "matchingproperty")]
        public void TryMatch_ConditionalWhenPropertyName(bool isSamePropertyName, string propertName)
        {
            // Arrange
            DynamicSegmentTemplate template = new DynamicSegmentTemplate(new DynamicPathSegment("matchingproperty"));
            DynamicPathSegment segment = new DynamicPathSegment(propertName);

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.Equal(isSamePropertyName, result);
            Assert.False(values.ContainsKey("matchingproperty"));

        }
    }
}
