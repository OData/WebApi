//-----------------------------------------------------------------------------
// <copyright file="PropertySegmentTemplateTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Routing.Template
{
    public class PropertySegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Segment()
        {
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new PropertySegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void TryMatch_ReturnsTrue()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "entity");
            IEdmStructuralProperty property = entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);

            PropertySegmentTemplate template = new PropertySegmentTemplate(new PropertySegment(property));
            PropertySegment segment = new PropertySegment(property);

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.True(result);
            Assert.Empty(values);
        }

        [Fact]
        public void TryMatch_ReturnsFalse()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "entity");
            IEdmStructuralProperty property1 = entityType.AddStructuralProperty("Name1", EdmPrimitiveTypeKind.String);
            IEdmStructuralProperty property2 = entityType.AddStructuralProperty("Name2", EdmPrimitiveTypeKind.String);

            PropertySegmentTemplate template = new PropertySegmentTemplate(new PropertySegment(property1));
            PropertySegment segment = new PropertySegment(property2);

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.False(result);
        }
    }
}
