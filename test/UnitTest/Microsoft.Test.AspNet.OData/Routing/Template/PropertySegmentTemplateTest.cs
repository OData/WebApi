// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.AspNet.OData.TestCommon;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Routing.Template
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
