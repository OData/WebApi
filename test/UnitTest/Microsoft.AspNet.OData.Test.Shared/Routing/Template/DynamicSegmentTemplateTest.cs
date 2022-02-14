//-----------------------------------------------------------------------------
// <copyright file="DynamicSegmentTemplateTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Routing.Template
{
    public class DynamicSegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_OpenPropertySegment()
        {
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new DynamicSegmentTemplate(segment: null), "segment");
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
