// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.WebPages.Html;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.WebPages.Test
{
    public class HtmlHelperTest
    {
        [Fact]
        public void ValidationInputCssClassNameThrowsWhenAssignedNull()
        {
            // Act and Assert
            Assert.ThrowsArgumentNull(() => HtmlHelper.ValidationInputCssClassName = null, "value");
        }

        [Fact]
        public void ValidationSummaryClassNameThrowsWhenAssignedNull()
        {
            // Act and Assert
            Assert.ThrowsArgumentNull(() => HtmlHelper.ValidationSummaryClass = null, "value");
        }

        [Fact]
        public void EncodeObject()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();
            object text = "<br />" as object;

            // Act
            string encodedHtml = htmlHelper.Encode(text);

            // Assert
            Assert.Equal(encodedHtml, "&lt;br /&gt;");
        }

        [Fact]
        public void EncodeObjectNull()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();
            object text = null;

            // Act
            string encodedHtml = htmlHelper.Encode(text);

            // Assert
            Assert.Equal(String.Empty, encodedHtml);
        }

        [Fact]
        public void EncodeString()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();
            var text = "<br />";

            // Act
            string encodedHtml = htmlHelper.Encode(text);

            // Assert
            Assert.Equal(encodedHtml, "&lt;br /&gt;");
        }

        [Fact]
        public void EncodeStringNull()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();
            string text = null;

            // Act
            string encodedHtml = htmlHelper.Encode(text);

            // Assert
            Assert.Equal("", encodedHtml);
        }

        [Fact]
        public void RawAllowsNullValue()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();

            // Act
            IHtmlString markupHtml = htmlHelper.Raw(null);

            // Assert
            Assert.Equal(null, markupHtml.ToString());
            Assert.Equal(null, markupHtml.ToHtmlString());
        }

        [Fact]
        public void RawAllowsNullObjectValue()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();

            // Act
            IHtmlString markupHtml = htmlHelper.Raw((object)null);

            // Assert
            Assert.Equal(null, markupHtml.ToString());
            Assert.Equal(null, markupHtml.ToHtmlString());
        }

        [Fact]
        public void RawAllowsEmptyValue()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();

            // Act
            IHtmlString markupHtml = htmlHelper.Raw("");

            // Assert
            Assert.Equal("", markupHtml.ToString());
            Assert.Equal("", markupHtml.ToHtmlString());
        }

        [Fact]
        public void RawReturnsWrapperMarkup()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();
            string markup = "<b>bold</b>";

            // Act
            IHtmlString markupHtml = htmlHelper.Raw(markup);

            // Assert
            Assert.Equal("<b>bold</b>", markupHtml.ToString());
            Assert.Equal("<b>bold</b>", markupHtml.ToHtmlString());
        }

        [Fact]
        public void RawReturnsWrapperMarkupOfObject()
        {
            // Arrange
            HtmlHelper htmlHelper = HtmlHelperFactory.Create();
            ObjectWithWrapperMarkup obj = new ObjectWithWrapperMarkup();

            // Act
            IHtmlString markupHtml = htmlHelper.Raw(obj);

            // Assert
            Assert.Equal("<b>boldFromObject</b>", markupHtml.ToString());
            Assert.Equal("<b>boldFromObject</b>", markupHtml.ToHtmlString());
        }

        private class ObjectWithWrapperMarkup
        {
            public override string ToString()
            {
                return "<b>boldFromObject</b>";
            }
        }
    }
}
