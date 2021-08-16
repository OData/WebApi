//-----------------------------------------------------------------------------
// <copyright file="PathTemplateSegmentTemplateTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Routing.Template
{
    public class PathTemplateSegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_PathTemplateSegment()
        {
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new PathTemplateSegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void Ctor_SetPropertiesCorrectly()
        {
            // Arrange
            PathTemplateSegment segment = new PathTemplateSegment("{pName:dynamicproperty}");

            // Act
            PathTemplateSegmentTemplate template = new PathTemplateSegmentTemplate(segment);

            // Assert
            Assert.Equal("pName", template.PropertyName);
            Assert.Equal("dynamicproperty", template.SegmentName);
        }

        [Fact]
        public void Ctor_FailedWithWrongPathTemplateString()
        {
            // Arrange
            PathTemplateSegment segment = new PathTemplateSegment("{pName:dynamic:test}");

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => new PathTemplateSegmentTemplate(segment),
                string.Format("The attribute routing template contains invalid segment '{0}'.", "{pName:dynamic:test}"));
        }

        [Fact]
        public void TryMatch_RetrunsTrue()
        {
            // Arrange
            PathTemplateSegment pathTemplateSegment = new PathTemplateSegment("{pName:dynamicproperty}");
            PathTemplateSegmentTemplate template = new PathTemplateSegmentTemplate(pathTemplateSegment);
            DynamicPathSegment segment = new DynamicPathSegment("property");

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.True(result);
            Assert.True(values.ContainsKey("pName"));
            Assert.Equal("property", values["pName"]);
        }

        [Fact]
        public void TryMatch_DifferentType()
        {
            // Arrange
            PathTemplateSegment pathTemplateSegment = new PathTemplateSegment("{pName:dynamicproperty}");
            PathTemplateSegmentTemplate template = new PathTemplateSegmentTemplate(pathTemplateSegment);

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(MetadataSegment.Instance, values);

            // Assert
            Assert.False(result);
        }
    }
}
