// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.WebPages.Html
{
    public class SelectListItem
    {
        public SelectListItem()
        {
        }

        public SelectListItem(SelectListItem item)
        {
            Text = item.Text;
            Value = item.Value;
            Selected = item.Selected;
        }

        public string Text { get; set; }

        public string Value { get; set; }

        public bool Selected { get; set; }
    }
}
