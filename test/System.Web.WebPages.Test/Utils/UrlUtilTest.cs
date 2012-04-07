// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.WebPages.TestUtils;
using Xunit;

namespace System.Web.WebPages.Test
{
    public class UrlUtilTest
    {
        [Fact]
        public void UrlTest()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                using (IDisposable _ = Utils.CreateHttpContext("default.aspx", "http://localhost/WebSite1/subfolder1/default.aspx"),
                                   __ = Utils.CreateHttpRuntime("/WebSite1/"))
                {
                    var vpath = "~/subfolder1/default.aspx";
                    var href = "~/world/test.aspx";
                    var expected = "/WebSite1/world/test.aspx";
                    Assert.Equal(expected, UrlUtil.Url(vpath, href));
                    Assert.Equal(expected, new MockPage() { VirtualPath = vpath }.Href(href));
                }
            });
        }

        [Fact]
        public void UrlTest2()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                using (IDisposable _ = Utils.CreateHttpContext("default.aspx", "http://localhost/WebSite1/default.aspx"),
                                   __ = Utils.CreateHttpRuntime("/WebSite1/"))
                {
                    var vpath = "~/default.aspx";
                    var href = "~/world/test.aspx";
                    var expected = "/WebSite1/world/test.aspx";
                    Assert.Equal(expected, UrlUtil.Url(vpath, href));
                    Assert.Equal(expected, new MockPage() { VirtualPath = vpath }.Href(href));
                }
            });
        }

        [Fact]
        public void UrlTest3()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                using (IDisposable _ = Utils.CreateHttpContext("default.aspx", "http://localhost/WebSite1/subfolder1/default.aspx"),
                                   __ = Utils.CreateHttpRuntime("/WebSite1/"))
                {
                    var vpath = "~/subfolder1/default.aspx";
                    var href = "world/test.aspx";
                    var expected = "/WebSite1/subfolder1/world/test.aspx";
                    Assert.Equal(expected, UrlUtil.Url(vpath, href));
                    Assert.Equal(expected, new MockPage() { VirtualPath = vpath }.Href(href));
                }
            });
        }

        [Fact]
        public void UrlTest4()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                using (IDisposable _ = Utils.CreateHttpContext("default.aspx", "http://localhost/WebSite1/subfolder1/default.aspx"),
                                   __ = Utils.CreateHttpRuntime("/WebSite1/"))
                {
                    var vpath = "~/subfolder2/default.aspx";
                    var href = "world/test.aspx";
                    var expected = "/WebSite1/subfolder2/world/test.aspx";
                    Assert.Equal(expected, UrlUtil.Url(vpath, href));
                    Assert.Equal(expected, new MockPage() { VirtualPath = vpath }.Href(href));
                }
            });
        }

        [Fact]
        public void BuildUrlEncodesPagePart()
        {
            // Arrange
            var page = "This is a really bad name for a page";
            var expected = "This%20is%20a%20really%20bad%20name%20for%20a%20page";

            // Act
            var actual = UrlUtil.BuildUrl(page);

            // Assert
            Assert.Equal(actual, expected);
        }

        [Fact]
        public void BuildUrlAppendsNonAnonymousTypesToPathPortion()
        {
            // Arrange
            object[] pathParts = new object[] { "part", Decimal.One, 1.25f };
            var page = "home";

            // Act
            var actual = UrlUtil.BuildUrl(page, pathParts);

            // Assert
            Assert.Equal(actual, page + "/part/1/1.25");
        }

        [Fact]
        public void BuildUrlEncodesAppendedPathPortion()
        {
            // Arrange
            object[] pathParts = new object[] { "path portion", "ζ" };
            var page = "home";

            // Act
            var actual = UrlUtil.BuildUrl(page, pathParts);

            // Assert
            Assert.Equal(actual, page + "/path%20portion/%ce%b6");
        }

        [Fact]
        public void BuildUrlAppendsAnonymousObjectsToQueryString()
        {
            // Arrange
            var page = "home";
            var queryString = new { sort = "FName", dir = "desc" };

            // Act
            var actual = UrlUtil.BuildUrl(page, queryString);

            // Assert
            Assert.Equal(actual, page + "?sort=FName&dir=desc");
        }

        [Fact]
        public void BuildUrlAppendsMultipleAnonymousObjectsToQueryString()
        {
            // Arrange
            var page = "home";
            var queryString1 = new { sort = "FName", dir = "desc" };
            var queryString2 = new { view = "Activities", page = 7 };

            // Act
            var actual = UrlUtil.BuildUrl(page, queryString1, queryString2);

            // Assert
            Assert.Equal(actual, page + "?sort=FName&dir=desc&view=Activities&page=7");
        }

        [Fact]
        public void BuildUrlEncodesQueryStringKeysAndValues()
        {
            // Arrange
            var page = "home";
            var queryString = new { ζ = "my=value&", mykey = "<π" };

            // Act
            var actual = UrlUtil.BuildUrl(page, queryString);

            // Assert
            Assert.Equal(actual, page + "?%ce%b6=my%3dvalue%26&mykey=%3c%cf%80");
        }

        [Fact]
        public void BuildUrlGeneratesPathPartsAndQueryString()
        {
            // Arrange
            var page = "home";

            // Act
            var actual = UrlUtil.BuildUrl(page, "products", new { cat = 37 }, "furniture", new { sort = "name", dir = "desc" });

            // Assert
            Assert.Equal(actual, page + "/products/furniture?cat=37&sort=name&dir=desc");
        }

        [Fact]
        public void UrlAppRootTest()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                using (IDisposable _ = Utils.CreateHttpContext("default.aspx", "http://localhost/"),
                                   __ = Utils.CreateHttpRuntime("/"))
                {
                    var vpath = "~/";
                    var href = "~/world/test.aspx";
                    var expected = "/world/test.aspx";
                    Assert.Equal(expected, UrlUtil.Url(vpath, href));
                    Assert.Equal(expected, new MockPage() { VirtualPath = vpath }.Href(href));
                }
            });
        }

        [Fact]
        public void UrlAnonymousObjectTest()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                using (IDisposable _ = Utils.CreateHttpContext("default.aspx", "http://localhost/"),
                                   __ = Utils.CreateHttpRuntime("/"))
                {
                    Assert.Equal("/world/test.cshtml?Prop1=value1",
                                 UrlUtil.Url("~/world/page.cshtml", "test.cshtml", new { Prop1 = "value1" }));
                    Assert.Equal("/world/test.cshtml?Prop1=value1&Prop2=value2",
                                 UrlUtil.Url("~/world/page.cshtml", "test.cshtml", new { Prop1 = "value1", Prop2 = "value2" }));
                }
            });
        }
    }
}
