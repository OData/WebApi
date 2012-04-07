// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Moq;

namespace System.Web.Mvc.Test
{
    public static class HttpContextHelpers
    {
        public static Mock<HttpContextBase> GetMockHttpContext()
        {
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(m => m.Items).Returns(new Dictionary<object, object>());
            return mockContext;
        }
    }
}
