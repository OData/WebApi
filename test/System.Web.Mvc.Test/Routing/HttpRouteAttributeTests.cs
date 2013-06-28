// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc;
using Microsoft.TestCommon;

namespace System.Web.Routing
{
    public class HttpRouteAttributeTests
    {
        [Fact]
        public void Template_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpRouteAttribute(null), "Value cannot be null.\r\nParameter name: routeTemplate");
        }

        [Fact]
        public void Template_StartsWithForwardSlash_Throws()
        {
            Assert.Throws<ArgumentException>(() => new HttpRouteAttribute("/whatever"), "The route template '/whatever' cannot begin or end with a forward slash.\r\nParameter name: routeTemplate");
        }

        [Fact]
        public void Template_EndsWithForwardSlashAndIsNotPrefixBypass_Throws()
        {
            Assert.Throws<ArgumentException>(() => new HttpRouteAttribute("whatever/"), "The route template 'whatever/' cannot begin or end with a forward slash.\r\nParameter name: routeTemplate");
        }

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