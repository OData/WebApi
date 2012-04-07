// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    internal interface IViewStartPageChild
    {
        HtmlHelper<object> Html { get; }
        UrlHelper Url { get; }
        ViewContext ViewContext { get; }
    }
}
