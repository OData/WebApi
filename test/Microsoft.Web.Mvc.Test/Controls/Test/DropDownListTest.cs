// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc;
using Xunit;

namespace Microsoft.Web.Mvc.Controls.Test
{
    public class DropDownListTest
    {
        [Fact]
        public void NameProperty()
        {
            // TODO: This
        }

        [Fact]
        public void RenderWithNoNameNotInDesignModeThrows()
        {
            // TODO: This
        }

        [Fact]
        public void RenderWithNoNameInDesignModeRendersWithSampleData()
        {
            // Setup
            DropDownList c = new DropDownList();

            // Execute
            string html = MvcTestHelper.GetControlRendering(c, true);

            // Verify
            Assert.Equal(@"<select>
	<option>
		Sample Item
	</option>
</select>", html);
        }

        [Fact]
        public void RenderWithNoAttributes()
        {
            // Setup
            DropDownList c = new DropDownList();
            c.Name = "nameKey";

            ViewDataContainer vdc = new ViewDataContainer();
            vdc.Controls.Add(c);
            vdc.ViewData = new ViewDataDictionary();
            vdc.ViewData["nameKey"] = new SelectList(new[] { "aaa", "bbb", "ccc" }, "bbb");

            // Execute
            string html = MvcTestHelper.GetControlRendering(c, false);

            // Verify
            Assert.Equal(@"<select name=""nameKey"">
	<option>
		aaa
	</option><option selected=""selected"">
		bbb
	</option><option>
		ccc
	</option>
</select>", html);
        }

        [Fact]
        public void RenderWithTextsAndValues()
        {
            // Setup
            DropDownList c = new DropDownList();
            c.Name = "nameKey";

            ViewDataContainer vdc = new ViewDataContainer();
            vdc.Controls.Add(c);
            vdc.ViewData = new ViewDataDictionary();
            vdc.ViewData["nameKey"] = new SelectList(
                new[]
                {
                    new { Text = "aaa", Value = "111" },
                    new { Text = "bbb", Value = "222" },
                    new { Text = "ccc", Value = "333" }
                },
                "Value",
                "Text",
                "222");

            // Execute
            string html = MvcTestHelper.GetControlRendering(c, false);

            // Verify
            Assert.Equal(@"<select name=""nameKey"">
	<option value=""111"">
		aaa
	</option><option value=""222"" selected=""selected"">
		bbb
	</option><option value=""333"">
		ccc
	</option>
</select>", html);
        }

        [Fact]
        public void RenderWithNameAndIdRendersNameAndIdAttribute()
        {
            // Setup
            DropDownList c = new DropDownList();
            c.Name = "nameKey";
            c.ID = "someID";

            ViewDataContainer vdc = new ViewDataContainer();
            vdc.Controls.Add(c);
            vdc.ViewData = new ViewDataDictionary();
            vdc.ViewData["nameKey"] = new SelectList(new[] { "aaa", "bbb", "ccc" }, "bbb");

            // Execute
            string html = MvcTestHelper.GetControlRendering(c, false);

            // Verify
            Assert.Equal(@"<select id=""someID"" name=""nameKey"">
	<option>
		aaa
	</option><option selected=""selected"">
		bbb
	</option><option>
		ccc
	</option>
</select>", html);
        }
    }
}
