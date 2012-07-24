// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Web.UI;

namespace System.Web.Mvc
{
    [ControlBuilder(typeof(ViewTypeControlBuilder))]
    [NonVisualControl]
    public class ViewType : Control
    {
        private string _typeName;

        [DefaultValue("")]
        public string TypeName
        {
            get { return _typeName ?? String.Empty; }
            set { _typeName = value; }
        }
    }
}
