// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Helpers.Test;
using System.Web.TestUtil;
using System.Web.WebPages.Scope;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace Microsoft.Web.Helpers.Test
{
    public class BingTest
    {
        private static readonly IDictionary<object, object> _emptyStateStorage = new Dictionary<object, object>();

        private const string BasicBingSearchTemplate = @"<form action=""http://www.bing.com/search"" class=""BingSearch"" method=""get"" target=""_blank"">"
                                                       + @"<input name=""FORM"" type=""hidden"" value=""FREESS"" /><input name=""cp"" type=""hidden"" value=""{0}"" />"
                                                       + @"<table cellpadding=""0"" cellspacing=""0"" style=""width:{1}px;""><tr style=""height: 32px"">"
                                                       + @"<td style=""width: 100%; border:solid 1px #ccc; border-right-style:none; padding-left:10px; padding-right:10px; vertical-align:middle;"">"
                                                       + @"<input name=""q"" style=""background-image:url(http://www.bing.com/siteowner/s/siteowner/searchbox_background_k.png); background-position:right; background-repeat:no-repeat; font-family:Arial; font-size:14px; color:#000; width:100%; border:none 0 transparent;"" title=""Search Bing"" type=""text"" />"
                                                       + @"</td><td style=""border:solid 1px #ccc; border-left-style:none; padding-left:0px; padding-right:3px;"">"
                                                       + @"<input alt=""Search"" src=""http://www.bing.com/siteowner/s/siteowner/searchbutton_normal_k.gif"" style=""border:none 0 transparent; height:24px; width:24px; vertical-align:top;"" type=""image"" />"
                                                       + @"</td></tr>";

        private const string BasicBingSearchFooter = "</table></form>";

        private const string BasicBingSearchLocalSiteSearch = @"<tr><td colspan=""2"" style=""font-size: small""><label><input checked=""checked"" name=""q1"" type=""radio"" value=""site:{0}"" />{1}</label>&nbsp;<label><input name=""q1"" type=""radio"" value="""" />Search Web</label></td></tr>";

        [Fact]
        public void SiteTitleThrowsWhenSetToNull()
        {
            Assert.ThrowsArgumentNull(() => Bing.SiteTitle = null, "SiteTitle");
        }

        [Fact]
        public void SiteTitleUsesScopeStorage()
        {
            // Arrange
            var value = "value";

            // Act
            Bing.SiteTitle = value;

            // Assert
            Assert.Equal(Bing.SiteTitle, value);
            Assert.Equal(ScopeStorage.CurrentScope[Bing._siteTitleKey], value);
        }

        [Fact]
        public void SiteUrlThrowsWhenSetToNull()
        {
            Assert.ThrowsArgumentNull(() => Bing.SiteUrl = null, "SiteUrl");
        }

        [Fact]
        public void SiteUrlUsesScopeStorage()
        {
            // Arrange
            var value = "value";

            // Act
            Bing.SiteUrl = value;

            // Assert
            Assert.Equal(Bing.SiteUrl, value);
            Assert.Equal(ScopeStorage.CurrentScope[Bing._siteUrlKey], value);
        }

        [Fact]
        public void SearchBoxGeneratesValidHtml()
        {
            // Act & Assert
            XhtmlAssert.Validate1_0(
                Bing._SearchBox("322px", null, null, GetContextForSearchBox(), _emptyStateStorage), true
                );
        }

        [Fact]
        public void SearchBoxDoesNotContainSearchLocalWhenSiteUrlIsNull()
        {
            // Arrange
            var encoding = Encoding.UTF8;
            var expectedHtml = String.Format(CultureInfo.InvariantCulture, BasicBingSearchTemplate, encoding.CodePage, 322) + BasicBingSearchFooter;

            // Act
            var actualHtml = Bing._SearchBox("322px", null, null, GetContextForSearchBox(encoding), _emptyStateStorage).ToString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedHtml, actualHtml);
        }

        [Fact]
        public void SearchBoxDoesNotContainSearchLocalWhenSiteUrlIsEmpty()
        {
            // Arrange
            var encoding = Encoding.UTF8;
            var expectedHtml = String.Format(CultureInfo.InvariantCulture, BasicBingSearchTemplate, encoding.CodePage, 322) + BasicBingSearchFooter;

            // Act
            var actualHtml = Bing._SearchBox("322px", String.Empty, String.Empty, GetContextForSearchBox(encoding), _emptyStateStorage).ToString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedHtml, actualHtml);
        }

        [Fact]
        public void SearchBoxUsesResponseEncodingToDetermineCodePage()
        {
            // Arrange
            var encoding = Encoding.GetEncoding(51932); //euc-jp
            var expectedHtml = String.Format(CultureInfo.InvariantCulture, BasicBingSearchTemplate, encoding.CodePage, 322) + BasicBingSearchFooter;

            // Act
            var actualHtml = Bing._SearchBox("322px", String.Empty, String.Empty, GetContextForSearchBox(encoding), _emptyStateStorage).ToString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedHtml, actualHtml);
        }

        [Fact]
        public void SearchBoxUsesWidthToSetBingSearchTableSize()
        {
            // Arrange
            var encoding = Encoding.UTF8;
            var expectedHtml = String.Format(CultureInfo.InvariantCulture, BasicBingSearchTemplate, encoding.CodePage, 609) + BasicBingSearchFooter;

            // Act
            var actualHtml = Bing._SearchBox("609px", String.Empty, String.Empty, GetContextForSearchBox(encoding), _emptyStateStorage).ToString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedHtml, actualHtml);
        }

        [Fact]
        public void SearchBoxUsesWithSiteUrlProducesLocalSearchOptions()
        {
            // Arrange
            var encoding = Encoding.Default;
            var expectedHtml = String.Format(CultureInfo.InvariantCulture, BasicBingSearchTemplate, encoding.CodePage, 322) +
                               String.Format(CultureInfo.InvariantCulture, BasicBingSearchLocalSiteSearch, "www.asp.net", "Search Site") + BasicBingSearchFooter;

            // Act
            var actualHtml = Bing._SearchBox("322px", "www.asp.net", String.Empty, GetContextForSearchBox(encoding), _emptyStateStorage).ToString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedHtml, actualHtml);
        }

        [Fact]
        public void SearchBoxUsesWithSiteUrlAndSiteTitleProducesLocalSearchOptions()
        {
            // Arrange
            var encoding = Encoding.Default;
            var expectedHtml = String.Format(CultureInfo.InvariantCulture, BasicBingSearchTemplate, encoding.CodePage, 322) +
                               String.Format(CultureInfo.InvariantCulture, BasicBingSearchLocalSiteSearch, "www.microsoft.com", "Custom Search") + BasicBingSearchFooter;

            // Act
            var actualHtml = Bing._SearchBox("322px", "www.microsoft.com", "Custom Search", GetContextForSearchBox(encoding), _emptyStateStorage).ToString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedHtml, actualHtml);
        }

        [Fact]
        public void SearchBoxWithLocalSiteOptionUsesResponseEncoding()
        {
            // Arrange
            var encoding = Encoding.GetEncoding(1258); //windows-1258
            var expectedHtml = String.Format(CultureInfo.InvariantCulture, BasicBingSearchTemplate, encoding.CodePage, 322) +
                               String.Format(CultureInfo.InvariantCulture, BasicBingSearchLocalSiteSearch, "www.asp.net", "Search Site") + BasicBingSearchFooter;

            // Act
            var actualHtml = Bing._SearchBox("322px", "www.asp.net", String.Empty, GetContextForSearchBox(encoding), _emptyStateStorage).ToString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedHtml, actualHtml);
        }

        [Fact]
        public void SearchBoxUsesScopeStorageIfSiteTitleParameterIsNull()
        {
            // Arrange
            var encoding = Encoding.GetEncoding(1258); //windows-1258
            var expectedHtml = String.Format(CultureInfo.InvariantCulture, BasicBingSearchTemplate, encoding.CodePage, 322) +
                               String.Format(CultureInfo.InvariantCulture, BasicBingSearchLocalSiteSearch, "www.asp.net", "foobar") + BasicBingSearchFooter;

            // Act
            var actualHtml = Bing._SearchBox("322px", "www.asp.net", null, GetContextForSearchBox(encoding), new Dictionary<object, object> { { Bing._siteTitleKey, "foobar" } }).ToString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedHtml, actualHtml);
        }

        [Fact]
        public void SearchBoxUsesScopeStorageIfSiteTitleParameterIsEmpty()
        {
            // Arrange
            var encoding = Encoding.GetEncoding(1258); //windows-1258
            var expectedHtml = String.Format(CultureInfo.InvariantCulture, BasicBingSearchTemplate, encoding.CodePage, 322) +
                               String.Format(CultureInfo.InvariantCulture, BasicBingSearchLocalSiteSearch, "www.asptest.net", "bazbiz") + BasicBingSearchFooter;

            // Act
            var actualHtml = Bing._SearchBox("322px", "www.asptest.net", String.Empty, GetContextForSearchBox(encoding), new Dictionary<object, object> { { Bing._siteTitleKey, "bazbiz" } }).ToString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedHtml, actualHtml);
        }

        [Fact]
        public void SearchBoxUsesScopeStorageIfSiteUrlParameterIsNull()
        {
            // Arrange
            var encoding = Encoding.GetEncoding(1258); //windows-1258
            var expectedHtml = String.Format(CultureInfo.InvariantCulture, BasicBingSearchTemplate, encoding.CodePage, 322) +
                               String.Format(CultureInfo.InvariantCulture, BasicBingSearchLocalSiteSearch, "www.myawesomesite.net", "my-test-string") + BasicBingSearchFooter;

            // Act
            var actualHtml = Bing._SearchBox("322px", null, "my-test-string", GetContextForSearchBox(encoding), new Dictionary<object, object> { { Bing._siteUrlKey, "www.myawesomesite.net" } }).ToString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedHtml, actualHtml);
        }

        [Fact]
        public void SearchBoxUsesScopeStorageIfSiteUrlParameterIsEmpty()
        {
            // Arrange
            var encoding = Encoding.GetEncoding(1258); //windows-1258
            var expectedHtml = String.Format(CultureInfo.InvariantCulture, BasicBingSearchTemplate, encoding.CodePage, 322) +
                               String.Format(CultureInfo.InvariantCulture, BasicBingSearchLocalSiteSearch, "www.myawesomesite.net", "my-test-string") + BasicBingSearchFooter;

            // Act
            var actualHtml = Bing._SearchBox("322px", String.Empty, "my-test-string", GetContextForSearchBox(encoding), new Dictionary<object, object> { { Bing._siteUrlKey, "www.myawesomesite.net" } }).ToString();

            // Assert
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expectedHtml, actualHtml);
        }

        private HttpContextBase GetContextForSearchBox(Encoding contentEncoding = null)
        {
            Mock<HttpContextBase> context = new Mock<HttpContextBase>();
            Mock<HttpResponseBase> response = new Mock<HttpResponseBase>();
            response.Setup(c => c.ContentEncoding).Returns(contentEncoding ?? Encoding.Default);
            context.Setup(c => c.Response).Returns(response.Object);

            return context.Object;
        }
    }
}
