// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Routing;
using System.Web.WebPages;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    public class ViewStartPageTest
    {
        [Fact]
        public void Html_DelegatesToChildPage()
        {
            // Arrange
            MockViewStartPage viewStart = new MockViewStartPage();
            var viewPage = new Mock<WebViewPage>() { CallBase = true };
            var helper = new HtmlHelper<object>(new ViewContext() { ViewData = new ViewDataDictionary() }, viewPage.Object, new RouteCollection());
            viewPage.Object.Html = helper;
            viewStart.ChildPage = viewPage.Object;

            // Act
            var result = viewStart.Html;

            // Assert
            Assert.Same(helper, result);
        }

        [Fact]
        public void Url_DelegatesToChildPage()
        {
            // Arrange
            MockViewStartPage viewStart = new MockViewStartPage();
            var viewPage = new Mock<WebViewPage>() { CallBase = true };
            var helper = new UrlHelper(new RequestContext());
            viewPage.Object.Url = helper;
            viewStart.ChildPage = viewPage.Object;

            // Act
            var result = viewStart.Url;

            // Assert
            Assert.Same(helper, result);
        }

        [Fact]
        public void ViewContext_DelegatesToChildPage()
        {
            // Arrange
            MockViewStartPage viewStart = new MockViewStartPage();
            var viewPage = new Mock<WebViewPage>() { CallBase = true };
            var viewContext = new ViewContext();
            viewPage.Object.ViewContext = viewContext;
            viewStart.ChildPage = viewPage.Object;

            // Act
            var result = viewStart.ViewContext;

            // Assert
            Assert.Same(viewContext, result);
        }

        [Fact]
        public void ViewStartPageChild_ThrowsOnNonMvcChildPage()
        {
            // Arrange
            MockViewStartPage viewStart = new MockViewStartPage();
            viewStart.ChildPage = new Mock<WebPage>().Object;

            // Act + Assert
            Assert.Throws<InvalidOperationException>(delegate() { var c = viewStart.ViewStartPageChild; }, "A ViewStartPage can be used only with with a page that derives from WebViewPage or another ViewStartPage.");
        }

        [Fact]
        public void ViewStartPageChild_WorksWithWebViewPage()
        {
            // Arrange
            MockViewStartPage viewStart = new MockViewStartPage();
            var viewPage = new Mock<WebViewPage>();
            viewStart.ChildPage = viewPage.Object;

            // Act
            var result = viewStart.ViewStartPageChild;

            // Assert
            Assert.Same(viewPage.Object, result);
        }

        [Fact]
        public void ViewStartPageChild_WorksWithAnotherRazorStartPage()
        {
            // Arrange
            MockViewStartPage viewStart = new MockViewStartPage();
            var anotherViewStart = new Mock<ViewStartPage>();
            viewStart.ChildPage = anotherViewStart.Object;

            // Act
            var result = viewStart.ViewStartPageChild;

            // Assert
            Assert.Same(anotherViewStart.Object, result);
        }

        class MockViewStartPage : ViewStartPage
        {
            public override void Execute()
            {
            }
        }
    }
}
