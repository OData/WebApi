// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing
{
    public class OpenPropertyPathSegmentTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_PropertyName()
        {
            Assert.ThrowsArgumentNull(() => new OpenPropertyPathSegment(propertyName: null), "propertyName");
        }

        [Fact]
        public void TryMatch()
        {
            OpenPropertyPathSegment leftSegment = new OpenPropertyPathSegment("property");
            OpenPropertyPathSegment rightSegment = new OpenPropertyPathSegment("property");

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = leftSegment.TryMatch(rightSegment, values);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void TryMatch_DifferentName()
        {
            OpenPropertyPathSegment leftSegment = new OpenPropertyPathSegment("property");
            OpenPropertyPathSegment rightSegment = new OpenPropertyPathSegment("nomatch");

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = leftSegment.TryMatch(rightSegment, values);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TryMatch_DifferentType()
        {
            OpenPropertyPathSegment leftSegment = new OpenPropertyPathSegment("property");
            KeyValuePathSegment rightSegment = new KeyValuePathSegment("value");

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = leftSegment.TryMatch(rightSegment, values);

            // Assert
            Assert.False(result);
        }
    }
}
