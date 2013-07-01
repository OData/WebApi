// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc;
using Microsoft.TestCommon;

namespace System.Web.Routing
{
    public class HttpRouteAttributeTests
    {
        [Fact]
        public void Template_EmptyString_Ok()
        {
            new HttpRouteAttribute("");
        }

        [Fact]
        public void AllowedMethodsShouldBeNull()
        {
            Assert.Null(new HttpRouteAttribute("").Verbs);
        }
    }
}