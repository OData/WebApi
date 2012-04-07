// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Web.UnitTestUtil;
using Xunit;

namespace Microsoft.Web.Mvc.Test
{
    public class SubmitButtonExtensionsTest
    {
        [Fact]
        public void SubmitButtonRendersWithJustTypeAttribute()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MvcHtmlString button = html.SubmitButton();
            Assert.Equal("<input type=\"submit\" />", button.ToHtmlString());
        }

        [Fact]
        public void SubmitButtonWithAttributesWithUnderscores()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MvcHtmlString button = html.SubmitButton(null, "blah", new { foo_bar = "baz" });
            Assert.Equal("<input foo-bar=\"baz\" type=\"submit\" value=\"blah\" />", button.ToHtmlString());
        }

        [Fact]
        public void SubmitButtonWithNameRendersButtonWithNameAttribute()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MvcHtmlString button = html.SubmitButton("button-name");
            Assert.Equal("<input id=\"button-name\" name=\"button-name\" type=\"submit\" />", button.ToHtmlString());
        }

        [Fact]
        public void SubmitButtonWithIdDifferentFromNameRendersButtonWithId()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MvcHtmlString button = html.SubmitButton("button-name", "blah", new { id = "foo" });
            Assert.Equal("<input id=\"foo\" name=\"button-name\" type=\"submit\" value=\"blah\" />", button.ToHtmlString());
        }

        [Fact]
        public void SubmitButtonWithNameAndTextRendersAttributes()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MvcHtmlString button = html.SubmitButton("button-name", "button-text");
            Assert.Equal("<input id=\"button-name\" name=\"button-name\" type=\"submit\" value=\"button-text\" />", button.ToHtmlString());
        }

        [Fact]
        public void SubmitButtonWithNameAndValueRendersBothAttributes()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MvcHtmlString button = html.SubmitButton("button-name", "button-value", new { id = "button-id" });
            Assert.Equal("<input id=\"button-id\" name=\"button-name\" type=\"submit\" value=\"button-value\" />", button.ToHtmlString());
        }

        [Fact]
        public void SubmitButtonWithNameAndIdRendersBothAttributesCorrectly()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MvcHtmlString button = html.SubmitButton("button-name", "button-value", new { id = "button-id" });
            Assert.Equal("<input id=\"button-id\" name=\"button-name\" type=\"submit\" value=\"button-value\" />", button.ToHtmlString());
        }

        [Fact]
        public void SubmitButtonWithTypeAttributeRendersCorrectType()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MvcHtmlString button = html.SubmitButton("specified-name", "button-value", new { type = "not-submit" });
            Assert.Equal("<input id=\"specified-name\" name=\"specified-name\" type=\"not-submit\" value=\"button-value\" />", button.ToHtmlString());
        }

        [Fact]
        public void SubmitButtonWithNameAndValueSpecifiedAndPassedInAsAttributeChoosesSpecified()
        {
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            MvcHtmlString button = html.SubmitButton("specified-name", "button-value", new RouteValueDictionary(new { name = "name-attribute-value", value = "value-attribute" }));
            Assert.Equal("<input id=\"specified-name\" name=\"name-attribute-value\" type=\"submit\" value=\"value-attribute\" />", button.ToHtmlString());
        }
    }
}
