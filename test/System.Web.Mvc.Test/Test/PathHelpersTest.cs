// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;
using Moq;
using Xunit;

namespace System.Web.Mvc.Test
{
    public class PathHelpersTest
    {
        [Fact]
        public void GenerateClientUrlWithAbsoluteContentPathAndRewritingDisabled()
        {
            // Arrange
            Mock<HttpContextBase> mockHttpContext = GetMockHttpContext(includeRewriterServerVar: false);

            // Act
            string returnedUrl = PathHelpers.GenerateClientUrl(mockHttpContext.Object, "should remain unchanged");

            // Assert
            Assert.Equal("should remain unchanged", returnedUrl);
        }

        [Fact]
        public void GenerateClientUrlWithAbsoluteContentPathAndRewritingEnabled()
        {
            PathHelpers.ResetUrlRewriterHelper(); // Reset the "is URL rewriting enabled?" cache

            // Arrange
            Mock<HttpContextBase> mockHttpContext = GetMockHttpContext(includeRewriterServerVar: true);
            mockHttpContext.Setup(c => c.Request.RawUrl).Returns("/quux/foo/bar/baz");
            mockHttpContext.Setup(c => c.Request.Path).Returns("/myapp/foo/bar/baz");

            // Act
            string returnedUrl = PathHelpers.GenerateClientUrl(mockHttpContext.Object, "/myapp/some/absolute/path?alpha=bravo");

            // Assert
            Assert.Equal("/quux/some/absolute/path?alpha=bravo", returnedUrl);
        }

        [Fact]
        public void GenerateClientUrlWithAppRelativeContentPathAndRewritingDisabled()
        {
            // Arrange
            Mock<HttpContextBase> mockHttpContext = GetMockHttpContext(includeRewriterServerVar: false);

            // Act
            string returnedUrl = PathHelpers.GenerateClientUrl(mockHttpContext.Object, "~/foo/bar?alpha=bravo");

            // Assert
            Assert.Equal("/myapp/(S(session))/foo/bar?alpha=bravo", returnedUrl);
        }

        [Fact]
        public void GenerateClientUrlWithAppRelativeContentPathAndRewritingEnabled()
        {
            PathHelpers.ResetUrlRewriterHelper(); // Reset the "is URL rewriting enabled?" cache

            // Arrange
            Mock<HttpContextBase> mockHttpContext = GetMockHttpContext(includeRewriterServerVar: true);
            mockHttpContext.Setup(c => c.Request.RawUrl).Returns("/quux/foo/baz");
            mockHttpContext.Setup(c => c.Request.Path).Returns("/myapp/foo/baz");

            // Act
            string returnedUrl = PathHelpers.GenerateClientUrl(mockHttpContext.Object, "~/foo/bar?alpha=bravo");

            // Assert
            Assert.Equal("/quux/foo/bar?alpha=bravo", returnedUrl);
        }

        [Fact]
        public void GenerateClientUrlWithEmptyContentPathReturnsEmptyString()
        {
            // Act
            string returnedUrl = PathHelpers.GenerateClientUrl(null, "");

            // Assert
            Assert.Equal("", returnedUrl);
        }

        [Fact]
        public void GenerateClientUrlWithNullContentPathReturnsNull()
        {
            // Act
            string returnedUrl = PathHelpers.GenerateClientUrl(null, null);

            // Assert
            Assert.Null(returnedUrl);
        }

        [Fact]
        public void GenerateClientUrlWithOnlyQueryStringForContentPathReturnsOriginalContentPath()
        {
            // Act
            string returnedUrl = PathHelpers.GenerateClientUrl(null, "?foo=bar");

            // Assert
            Assert.Equal("?foo=bar", returnedUrl);
        }

        [Fact]
        public void MakeAbsoluteFromDirectoryToParent()
        {
            // Act
            string returnedUrl = PathHelpers.MakeAbsolute("/Account/Register", "../Account");

            // Assert
            Assert.Equal("/Account", returnedUrl);
        }

        [Fact]
        public void MakeAbsoluteFromDirectoryToSelf()
        {
            // Act
            string returnedUrl = PathHelpers.MakeAbsolute("/foo/", "./");

            // Assert
            Assert.Equal("/foo/", returnedUrl);
        }

        [Fact]
        public void MakeAbsoluteFromFileToFile()
        {
            // Act
            string returnedUrl = PathHelpers.MakeAbsolute("/foo", "bar");

            // Assert
            Assert.Equal("/bar", returnedUrl);
        }

        [Fact]
        public void MakeAbsoluteFromFileWithQueryToFile()
        {
            // Act
            string returnedUrl = PathHelpers.MakeAbsolute("/foo/bar?alpha=bravo", "baz");

            // Assert
            Assert.Equal("/foo/baz", returnedUrl);
        }

        [Fact]
        public void MakeAbsoluteFromRootToSelf()
        {
            // Act
            string returnedUrl = PathHelpers.MakeAbsolute("/", "./");

            // Assert
            Assert.Equal("/", returnedUrl);
        }

        [Fact]
        public void MakeRelativeFromFileToDirectory()
        {
            // Act
            string returnedUrl = PathHelpers.MakeRelative("/foo/bar", "/foo/");

            // Assert
            Assert.Equal("./", returnedUrl);
        }

        [Fact]
        public void MakeRelativeFromFileToDirectoryWithQueryString()
        {
            // Act
            string returnedUrl = PathHelpers.MakeRelative("/foo/bar", "/foo/?alpha=bravo");

            // Assert
            Assert.Equal("./?alpha=bravo", returnedUrl);
        }

        [Fact]
        public void MakeRelativeFromFileToFile()
        {
            // Act
            string returnedUrl = PathHelpers.MakeRelative("/foo/bar", "/baz/quux");

            // Assert
            Assert.Equal("../baz/quux", returnedUrl);
        }

        [Fact]
        public void MakeRelativeFromFileToFileWithQuery()
        {
            // Act
            string returnedUrl = PathHelpers.MakeRelative("/foo/bar", "/baz/quux?alpha=bravo");

            // Assert
            Assert.Equal("../baz/quux?alpha=bravo", returnedUrl);
        }

        [Fact]
        public void MakeRelativeFromFileWithQueryToFileWithQuery()
        {
            // Act
            string returnedUrl = PathHelpers.MakeRelative("/foo/bar?charlie=delta", "/baz/quux?alpha=bravo");

            // Assert
            Assert.Equal("../baz/quux?alpha=bravo", returnedUrl);
        }

        [Fact]
        public void MakeRelativeFromRootToRoot()
        {
            // Act
            string returnedUrl = PathHelpers.MakeRelative("/", "/");

            // Assert
            Assert.Equal("./", returnedUrl);
        }

        [Fact]
        public void MakeRelativeFromRootToRootWithQueryString()
        {
            // Act
            string returnedUrl = PathHelpers.MakeRelative("/", "/?foo=bar");

            // Assert
            Assert.Equal("./?foo=bar", returnedUrl);
        }

        internal static Mock<HttpContextBase> GetMockHttpContext(bool includeRewriterServerVar)
        {
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();

            NameValueCollection serverVars = new NameValueCollection();
            serverVars["IIS_UrlRewriteModule"] = "I'm on!";
            mockContext.Setup(c => c.Request.ServerVariables).Returns(serverVars);
            mockContext.Setup(c => c.Request.ApplicationPath).Returns("/myapp");

            if (includeRewriterServerVar)
            {
                serverVars["IIS_WasUrlRewritten"] = "Got rewritten!";
                mockContext
                    .Setup(c => c.Response.ApplyAppPathModifier(It.IsAny<string>()))
                    .Returns(
                        delegate(string input) { return input; });
            }
            else
            {
                mockContext
                    .Setup(c => c.Response.ApplyAppPathModifier(It.IsAny<string>()))
                    .Returns(
                        delegate(string input) { return "/myapp/(S(session))" + input.Substring("/myapp".Length); });
            }

            return mockContext;
        }
    }
}
