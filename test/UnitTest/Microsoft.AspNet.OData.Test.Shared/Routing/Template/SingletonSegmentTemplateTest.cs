//-----------------------------------------------------------------------------
// <copyright file="SingletonSegmentTemplateTest.cs" company=".NET Foundation">
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
    public class SingletonSegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Segment()
        {
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new SingletonSegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void TryMatch_ReturnsTrue()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "entity");
            IEdmEntityContainer container = new EdmEntityContainer("NS", "default");
            IEdmSingleton singleton = new EdmSingleton(container, "singleton", entityType);

            SingletonSegmentTemplate template = new SingletonSegmentTemplate(new SingletonSegment(singleton));
            SingletonSegment segment = new SingletonSegment(singleton);

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
            IEdmEntityContainer container = new EdmEntityContainer("NS", "default");
            IEdmSingleton singleton1 = new EdmSingleton(container, "singleton1", entityType);
            IEdmSingleton singleton2 = new EdmSingleton(container, "singleton2", entityType);

            SingletonSegmentTemplate template = new SingletonSegmentTemplate(new SingletonSegment(singleton1));
            SingletonSegment segment = new SingletonSegment(singleton2);

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.False(result);
        }
    }
}
