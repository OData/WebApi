// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData;
using Microsoft.Test.AspNet.OData.Common;
using Xunit;

namespace Microsoft.Test.AspNet.OData
{
    public class ClrPropertyInfoAnnotationTest
    {
        [Fact]
        public void Ctor_ThrowsForNullPropertyInfo()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new ClrPropertyInfoAnnotation(clrPropertyInfo: null),
                "clrPropertyInfo");
        }
    }
}
