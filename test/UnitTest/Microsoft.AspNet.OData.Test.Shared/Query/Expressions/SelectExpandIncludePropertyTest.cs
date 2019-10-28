// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.AspNet.OData.Test.Common;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query.Expressions
{
    public class SelectExpandIncludePropertyTest
    {
        [Fact]
        public void Constructor_ThrowsPropertySegmentArgumentNull_IfMissPropertySegment()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new SelectExpandIncludeProperty(null, null),
                "propertySegment");
        }
    }
}
