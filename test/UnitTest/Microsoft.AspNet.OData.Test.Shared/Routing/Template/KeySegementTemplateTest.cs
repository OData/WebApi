//-----------------------------------------------------------------------------
// <copyright file="KeySegementTemplateTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Routing.Template
{
    public class KeySegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_KeySegment()
        {
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new KeySegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void TryMatch_SingleKey_ReturnsTrueOnMatch()
        {
            KeySegmentTemplate template =
                new KeySegmentTemplate(new KeySegment(new[] {new KeyValuePair<string, object>("ID", "{key}")}, null,
                    null));
            KeySegment segment = new KeySegment(new[] {new KeyValuePair<string, object>("ID", 123)}, null, null);

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.True(result);
            Assert.Equal(123, values["key"]);
        }

        [Fact]
        public void TryMatch_MultiKey_ReturnsTrueOnMatch()
        {
            KeySegmentTemplate template =
                new KeySegmentTemplate(new KeySegment(new[]
                {
                    new KeyValuePair<string, object>("FirstName", "{key1}"),
                    new KeyValuePair<string, object>("LastName", "{key2}")
                }, null, null));

            KeySegment segment = new KeySegment(new[]
            {
                new KeyValuePair<string, object>("FirstName", "abc"),
                new KeyValuePair<string, object>("LastName", "xyz") 
            }, null, null);

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.True(result);
            Assert.Equal("abc", values["key1"]);
            Assert.Equal("xyz", values["key2"]);
        }
    }
}
