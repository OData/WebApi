// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing
{
    public class OpenPropertyPathSegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_OpenPropertyPathSegment()
        {
            Assert.ThrowsArgumentNull(() => new OpenPropertyPathSegmentTemplate(openPropertyPathSegment: null), "openPropertyPathSegment");
        }

        [Fact]
        public void TryMatch_AlwaysTrueWhenParameterName()
        {
            OpenPropertyPathSegmentTemplate template = new OpenPropertyPathSegmentTemplate(new OpenPropertyPathSegment("{parameter}"));
            OpenPropertyPathSegment segment = new OpenPropertyPathSegment("property");

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
            OpenPropertyPathSegmentTemplate template = new OpenPropertyPathSegmentTemplate(new OpenPropertyPathSegment("matchingproperty"));

            foreach (bool b in new bool[] { true, false })
            {
                OpenPropertyPathSegment segment = new OpenPropertyPathSegment(b ? "matchingproperty" : "notmatchingproperty");

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
