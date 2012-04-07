// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Web.UnitTestUtil;
using Xunit;

namespace System.Web.Mvc.Html.Test
{
    public class NameExtensionsTest
    {
        [Fact]
        public void NonStronglyTypedWithNoPrefix()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());

            // Act & Assert
            Assert.Equal("", html.IdForModel().ToHtmlString());
            Assert.Equal("foo", html.Id("foo").ToHtmlString());
            Assert.Equal("foo_bar", html.Id("foo.bar").ToHtmlString());
            Assert.Equal(String.Empty, html.Id("<script>alert(\"XSS!\")</script>").ToHtmlString());

            Assert.Equal("", html.NameForModel().ToHtmlString());
            Assert.Equal("foo", html.Name("foo").ToHtmlString());
            Assert.Equal("foo.bar", html.Name("foo.bar").ToHtmlString());
            Assert.Equal("&lt;script>alert(&quot;XSS!&quot;)&lt;/script>", html.Name("<script>alert(\"XSS!\")</script>").ToHtmlString());
        }

        [Fact]
        public void NonStronglyTypedWithPrefix()
        {
            // Arrange
            HtmlHelper html = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            html.ViewData.TemplateInfo.HtmlFieldPrefix = "prefix";

            // Act & Assert
            Assert.Equal("prefix", html.IdForModel().ToHtmlString());
            Assert.Equal("prefix_foo", html.Id("foo").ToHtmlString());
            Assert.Equal("prefix_foo_bar", html.Id("foo.bar").ToHtmlString());

            Assert.Equal("prefix", html.NameForModel().ToHtmlString());
            Assert.Equal("prefix.foo", html.Name("foo").ToHtmlString());
            Assert.Equal("prefix.foo.bar", html.Name("foo.bar").ToHtmlString());
        }

        [Fact]
        public void StronglyTypedWithNoPrefix()
        {
            // Arrange
            HtmlHelper<OuterClass> html = MvcHelper.GetHtmlHelper(new ViewDataDictionary<OuterClass>());

            // Act & Assert
            Assert.Equal("IntValue", html.IdFor(m => m.IntValue).ToHtmlString());
            Assert.Equal("Inner_StringValue", html.IdFor(m => m.Inner.StringValue).ToHtmlString());

            Assert.Equal("IntValue", html.NameFor(m => m.IntValue).ToHtmlString());
            Assert.Equal("Inner.StringValue", html.NameFor(m => m.Inner.StringValue).ToHtmlString());
        }

        [Fact]
        public void StronglyTypedWithPrefix()
        {
            // Arrange
            HtmlHelper<OuterClass> html = MvcHelper.GetHtmlHelper(new ViewDataDictionary<OuterClass>());
            html.ViewData.TemplateInfo.HtmlFieldPrefix = "prefix";

            // Act & Assert
            Assert.Equal("prefix_IntValue", html.IdFor(m => m.IntValue).ToHtmlString());
            Assert.Equal("prefix_Inner_StringValue", html.IdFor(m => m.Inner.StringValue).ToHtmlString());

            Assert.Equal("prefix.IntValue", html.NameFor(m => m.IntValue).ToHtmlString());
            Assert.Equal("prefix.Inner.StringValue", html.NameFor(m => m.Inner.StringValue).ToHtmlString());
        }

        private sealed class OuterClass
        {
            public InnerClass Inner { get; set; }
            public int IntValue { get; set; }
        }

        private sealed class InnerClass
        {
            public string StringValue { get; set; }
        }
    }
}
