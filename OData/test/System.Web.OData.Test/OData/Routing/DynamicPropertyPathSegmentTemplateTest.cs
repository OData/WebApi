// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing
{
    public class DynamicPropertyPathSegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_DynamicPropertyPathSegment()
        {
            Assert.ThrowsArgumentNull(() => new DynamicPropertyPathSegmentTemplate(dynamicPropertyPathSegment: null), "dynamicPropertyPathSegment");
        }

        [Fact]
        public void TryMatch_AlwaysTrueWhenParameterName()
        {
            DynamicPropertyPathSegmentTemplate template = new DynamicPropertyPathSegmentTemplate(new DynamicPropertyPathSegment("{parameter}"));
            DynamicPropertyPathSegment segment = new DynamicPropertyPathSegment("property");

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.True(result);
            Assert.True(values.ContainsKey("parameter"));
            Assert.Equal("property", values["parameter"]);
        }

        [Fact]
        public void TryMatch_ConditionalWhenPropertyName()
        {
            DynamicPropertyPathSegmentTemplate template = new DynamicPropertyPathSegmentTemplate(new DynamicPropertyPathSegment("matchingproperty"));

            foreach (bool b in new bool[] { true, false })
            {
                DynamicPropertyPathSegment segment = new DynamicPropertyPathSegment(b ? "matchingproperty" : "notmatchingproperty");

                // Act
                Dictionary<string, object> values = new Dictionary<string, object>();
                bool result = template.TryMatch(segment, values);

                // Assert
                Assert.Equal(b, result);
                Assert.False(values.ContainsKey("matchingproperty"));
            }
        }
    }
}
