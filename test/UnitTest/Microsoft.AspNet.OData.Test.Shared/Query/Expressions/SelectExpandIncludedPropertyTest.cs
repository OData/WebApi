//-----------------------------------------------------------------------------
// <copyright file="SelectExpandIncludedPropertyTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
            ExceptionAssert.ThrowsArgumentNull(() => new SelectExpandIncludedProperty(null, null),
                "propertySegment");
        }
    }
}
