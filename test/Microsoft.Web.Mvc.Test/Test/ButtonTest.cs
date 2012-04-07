// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Web.UnitTestUtil;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace Microsoft.Web.Mvc.Test
{
    public class ButtonTest
    {
        [Fact]
        public void ButtonWithNullNameThrowsArgumentNullException()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            Assert.ThrowsArgumentNull(() => html.Button(null, "text", HtmlButtonType.Button), "name");
        }

        [Fact]
        public void ButtonRendersBaseAttributes()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MvcHtmlString result = html.Button("nameAttr", "buttonText", HtmlButtonType.Reset, "onclickAttr");
            Assert.Equal("<button name=\"nameAttr\" onclick=\"onclickAttr\" type=\"reset\">buttonText</button>", result.ToHtmlString());
        }

        [Fact]
        public void ButtonWithoutOnClickDoesNotRenderOnclickAttribute()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MvcHtmlString result = html.Button("nameAttr", "buttonText", HtmlButtonType.Reset);
            Assert.Equal("<button name=\"nameAttr\" type=\"reset\">buttonText</button>", result.ToHtmlString());
        }

        [Fact]
        public void ButtonAllowsInnerHtml()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MvcHtmlString result = html.Button("nameAttr", "<img src=\"puppy.jpg\" />", HtmlButtonType.Submit, "onclickAttr");
            Assert.Equal("<button name=\"nameAttr\" onclick=\"onclickAttr\" type=\"submit\"><img src=\"puppy.jpg\" /></button>", result.ToHtmlString());
        }

        [Fact]
        public void ButtonRendersExplicitAttributes()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MvcHtmlString result = html.Button("nameAttr", "buttonText", HtmlButtonType.Reset, "onclickAttr", new { title = "the-title" });
            Assert.Equal("<button name=\"nameAttr\" onclick=\"onclickAttr\" title=\"the-title\" type=\"reset\">buttonText</button>", result.ToHtmlString());
        }

        [Fact]
        public void ButtonRendersExplicitAttributesWithUnderscores()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MvcHtmlString result = html.Button("nameAttr", "buttonText", HtmlButtonType.Reset, "onclickAttr", new { foo_bar = "baz" });
            Assert.Equal("<button foo-bar=\"baz\" name=\"nameAttr\" onclick=\"onclickAttr\" type=\"reset\">buttonText</button>", result.ToHtmlString());
        }

        [Fact]
        public void ButtonRendersExplicitDictionaryAttributes()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MvcHtmlString result = html.Button("nameAttr", "buttonText", HtmlButtonType.Button, "onclickAttr", new RouteValueDictionary(new { title = "the-title" }));
            Assert.Equal("<button name=\"nameAttr\" onclick=\"onclickAttr\" title=\"the-title\" type=\"button\">buttonText</button>", result.ToHtmlString());
        }
    }
}
