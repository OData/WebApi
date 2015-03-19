// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
