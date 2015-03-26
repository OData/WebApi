// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing
{
    public class DynamicPropertyPathSegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_DynamicPropertyPathSegment()
        {
            // Assert
            Assert.ThrowsArgumentNull(() => new DynamicPropertyPathSegmentTemplate(dynamicPropertyPathSegment: null), "dynamicPropertyPathSegment");
        }

        [Fact]
        public void TryMatch_AlwaysTrueWhenParameterName()
        {
            // Arrange
            DynamicPropertyPathSegmentTemplate template = new DynamicPropertyPathSegmentTemplate(new DynamicPropertyPathSegment("{parameter}"));
            DynamicPropertyPathSegment segment = new DynamicPropertyPathSegment("property");

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
            DynamicPropertyPathSegmentTemplate template = new DynamicPropertyPathSegmentTemplate(new DynamicPropertyPathSegment("matchingproperty"));
            DynamicPropertyPathSegment segment = new DynamicPropertyPathSegment(propertName);

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.Equal(isSamePropertyName, result);
            Assert.False(values.ContainsKey("matchingproperty"));

        }
    }
}
