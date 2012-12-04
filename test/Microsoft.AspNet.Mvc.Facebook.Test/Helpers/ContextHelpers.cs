// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Mvc;
using Moq;

namespace Microsoft.AspNet.Mvc.Facebook.Test.Helpers
{
    public class ContextHelpers
    {
        public static ControllerContext CreateControllerContext(NameValueCollection requestFormData = null, NameValueCollection requestQueryData = null)
        {
            Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
            controllerContext.Setup(c => c.HttpContext.Items).Returns(new Dictionary<object, object>());
            controllerContext.Setup(c => c.HttpContext.Request.Form).Returns(requestFormData ?? new NameValueCollection());
            controllerContext.Setup(c => c.HttpContext.Request.QueryString).Returns(requestQueryData ?? new NameValueCollection());
            return controllerContext.Object;
        }
    }
}