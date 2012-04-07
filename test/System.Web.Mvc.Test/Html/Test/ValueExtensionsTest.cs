// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc.Test;
using Microsoft.Web.UnitTestUtil;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Html.Test
{
    public class ValueExtensionsTest
    {
        // Value

        [Fact]
        public void ValueWithNullNameThrows()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetValueViewData());

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => helper.Value(name: null),
                "name"
                );
        }

        [Fact]
        public void ValueGetsValueFromViewData()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetValueViewData());

            // Act
            MvcHtmlString html = helper.Value("foo");

            // Assert
            Assert.Equal("ViewDataFoo", html.ToHtmlString());
        }

        // ValueFor

        [Fact]
        public void ValueForWithNullExpressionThrows()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetValueViewData());

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => helper.ValueFor<FooBarModel, object>(expression: null),
                "expression"
                );
        }

        [Fact]
        public void ValueForGetsExpressionValueFromViewDataModel()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetValueViewData());

            // Act
            MvcHtmlString html = helper.ValueFor(m => m.foo);

            // Assert
            Assert.Equal("ViewItemFoo", html.ToHtmlString());
        }

        // All Value Helpers including ValueForModel

        [Fact]
        public void ValueHelpersWithErrorsGetValueFromModelState()
        {
            // Arrange
            ViewDataDictionary<FooBarModel> viewDataWithErrors = new ViewDataDictionary<FooBarModel> { { "foo", "ViewDataFoo" } };
            viewDataWithErrors.Model = new FooBarModel() { foo = "ViewItemFoo", bar = "ViewItemBar" };
            viewDataWithErrors.TemplateInfo.HtmlFieldPrefix = "FieldPrefix";

            ModelState modelStateFoo = new ModelState();
            modelStateFoo.Value = HtmlHelperTest.GetValueProviderResult(new string[] { "AttemptedValueFoo" }, "AttemptedValueFoo");
            viewDataWithErrors.ModelState["FieldPrefix.foo"] = modelStateFoo;

            ModelState modelStateFooBar = new ModelState();
            modelStateFooBar.Value = HtmlHelperTest.GetValueProviderResult(new string[] { "AttemptedValueFooBar" }, "AttemptedValueFooBar");
            viewDataWithErrors.ModelState["FieldPrefix"] = modelStateFooBar;

            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(viewDataWithErrors);

            // Act & Assert
            Assert.Equal("AttemptedValueFoo", helper.Value("foo").ToHtmlString());
            Assert.Equal("AttemptedValueFoo", helper.ValueFor(m => m.foo).ToHtmlString());
            Assert.Equal("AttemptedValueFooBar", helper.ValueForModel().ToHtmlString());
        }

        [Fact]
        public void ValueHelpersWithEmptyNameConvertModelValueUsingCurrentCulture()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetValueViewData());
            string expectedModelValue = "{ foo = ViewItemFoo, bar = 1900/01/01 12:00:00 AM }";

            // Act & Assert
            using (HtmlHelperTest.ReplaceCulture("en-ZA", "en-US"))
            {
                Assert.Equal(expectedModelValue, helper.Value(name: String.Empty).ToHtmlString());
                Assert.Equal(expectedModelValue, helper.ValueFor(m => m).ToHtmlString());
                Assert.Equal(expectedModelValue, helper.ValueForModel().ToHtmlString());
            }
        }

        [Fact]
        public void ValueHelpersFormatValue()
        {
            // Arrange
            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(GetValueViewData());
            string expectedModelValue = "-{ foo = ViewItemFoo, bar = 1900/01/01 12:00:00 AM }-";
            string expectedBarValue = "-1900/01/01 12:00:00 AM-";

            // Act & Assert
            using (HtmlHelperTest.ReplaceCulture("en-ZA", "en-US"))
            {
                Assert.Equal(expectedModelValue, helper.ValueForModel("-{0}-").ToHtmlString());
                Assert.Equal(expectedBarValue, helper.Value("bar", "-{0}-").ToHtmlString());
                Assert.Equal(expectedBarValue, helper.ValueFor(m => m.bar, "-{0}-").ToHtmlString());
            }
        }

        [Fact]
        public void ValueHelpersEncodeValue()
        {
            // Arrange
            ViewDataDictionary<FooBarModel> viewData = new ViewDataDictionary<FooBarModel> { { "foo", @"ViewDataFoo <"">" } };
            viewData.Model = new FooBarModel { foo = @"ViewItemFoo <"">" };

            ModelState modelStateFoo = new ModelState();
            modelStateFoo.Value = HtmlHelperTest.GetValueProviderResult(new string[] { @"AttemptedValueBar <"">" }, @"AttemptedValueBar <"">");
            viewData.ModelState["bar"] = modelStateFoo;

            HtmlHelper<FooBarModel> helper = MvcHelper.GetHtmlHelper(viewData);

            // Act & Assert
            Assert.Equal("&lt;{ foo = ViewItemFoo &lt;&quot;>, bar = (null) }", helper.ValueForModel("<{0}").ToHtmlString());
            Assert.Equal("&lt;ViewDataFoo &lt;&quot;>", helper.Value("foo", "<{0}").ToHtmlString());
            Assert.Equal("&lt;ViewItemFoo &lt;&quot;>", helper.ValueFor(m => m.foo, "<{0}").ToHtmlString());
            Assert.Equal("AttemptedValueBar &lt;&quot;>", helper.ValueFor(m => m.bar).ToHtmlString());
        }

        private sealed class FooBarModel
        {
            public string foo { get; set; }
            public object bar { get; set; }

            public override string ToString()
            {
                return String.Format("{{ foo = {0}, bar = {1} }}", foo ?? "(null)", bar ?? "(null)");
            }
        }

        private static ViewDataDictionary<FooBarModel> GetValueViewData()
        {
            ViewDataDictionary<FooBarModel> viewData = new ViewDataDictionary<FooBarModel> { { "foo", "ViewDataFoo" } };
            viewData.Model = new FooBarModel { foo = "ViewItemFoo", bar = new DateTime(1900, 1, 1, 0, 0, 0) };

            return viewData;
        }
    }
}
