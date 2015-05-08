// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing
{
    public class DynamicPropertyPathSegmentTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_PropertyName()
        {
            Assert.ThrowsArgumentNull(() => new DynamicPropertyPathSegment(propertyName: null), "propertyName");
        }

        [Fact]
        public void TryMatch()
        {
            // Arrange
            DynamicPropertyPathSegment leftSegment = new DynamicPropertyPathSegment("property");
            DynamicPropertyPathSegment rightSegment = new DynamicPropertyPathSegment("property");

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = leftSegment.TryMatch(rightSegment, values);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void TryMatch_DifferentName()
        {
            // Arrange
            DynamicPropertyPathSegment leftSegment = new DynamicPropertyPathSegment("property");
            DynamicPropertyPathSegment rightSegment = new DynamicPropertyPathSegment("nomatch");

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = leftSegment.TryMatch(rightSegment, values);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TryMatch_DifferentType()
        {
            // Arrange
            DynamicPropertyPathSegment leftSegment = new DynamicPropertyPathSegment("property");
            KeyValuePathSegment rightSegment = new KeyValuePathSegment("value");

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = leftSegment.TryMatch(rightSegment, values);

            // Assert
            Assert.False(result);
        }
    }
}
