// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using Xunit;

namespace System.Web.Mvc.Test
{
    public class NoAsyncTimeoutAttributeTest
    {
        [Fact]
        public void DurationPropertyIsZero()
        {
            // Act
            AsyncTimeoutAttribute attr = new NoAsyncTimeoutAttribute();

            // Assert
            Assert.Equal(Timeout.Infinite, attr.Duration);
        }
    }
}
