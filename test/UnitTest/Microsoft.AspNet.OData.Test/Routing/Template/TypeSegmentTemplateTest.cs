// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.AspNet.OData.Common;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Routing.Template
{
    public class TypeSegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Segment()
        {
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new TypeSegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void TryMatch_ReturnsTrue()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "entity");
            TypeSegmentTemplate template = new TypeSegmentTemplate(new TypeSegment(entityType, null));
            TypeSegment segment = new TypeSegment(entityType, null);

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
            EdmEntityType entityType1 = new EdmEntityType("NS", "entity1");
            EdmEntityType entityType2 = new EdmEntityType("NS", "entity2");

            TypeSegmentTemplate template = new TypeSegmentTemplate(new TypeSegment(entityType1, null));
            TypeSegment segment = new TypeSegment(entityType2, null);

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.False(result);
        }
    }
}
