// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Net.Http
{
    public class InvalidByteRangeExceptionTest
    {
        [Fact]
        public void Ctor_ThrowsOnNullRange()
        {
            Assert.ThrowsArgumentNull(() => new InvalidByteRangeException(contentRange: null), "contentRange");
        }

        [Fact]
        public void Ctor_SetsContentRange()
        {
            ContentRangeHeaderValue contentRange = new ContentRangeHeaderValue(0, 20, 100);
            InvalidByteRangeException invalidByteRangeException = new InvalidByteRangeException(contentRange);
            Assert.Same(contentRange, invalidByteRangeException.ContentRange);
        }
    }
}
