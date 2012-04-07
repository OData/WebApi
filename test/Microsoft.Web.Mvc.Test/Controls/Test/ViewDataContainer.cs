// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc;
using System.Web.UI;

namespace Microsoft.Web.Mvc.Controls.Test
{
    public class ViewDataContainer : Control, IViewDataContainer
    {
        public ViewDataDictionary ViewData { get; set; }
    }
}
