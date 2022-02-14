//-----------------------------------------------------------------------------
// <copyright file="EntitySetSegmentTemplateTest.cs" company=".NET Foundation">
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
    public class EntitySetSegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EntitySetSegment()
        {
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EntitySetSegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void TryMatch_ReturnsTrue()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "entity");
            EdmEntityContainer container = new EdmEntityContainer("NS", "default");
            EdmEntitySet entityset = new EdmEntitySet(container, "entities", entityType);
            EntitySetSegmentTemplate template = new EntitySetSegmentTemplate(new EntitySetSegment(entityset));
            EntitySetSegment segment = new EntitySetSegment(entityset);

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
            EdmEntityContainer container = new EdmEntityContainer("NS", "default");
            EdmEntitySet entityset1 = new EdmEntitySet(container, "entities1", entityType);
            EdmEntitySet entityset2 = new EdmEntitySet(container, "entities2", entityType);
            EntitySetSegmentTemplate template = new EntitySetSegmentTemplate(new EntitySetSegment(entityset1));
            EntitySetSegment segment = new EntitySetSegment(entityset2);

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.False(result);
        }
    }
}
