// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.WebPages
{
    // An executor is a class that can take over the execution of a WebPage. This can be used to
    // implement features like AJAX callback methods on the page (like WebForms Page Methods)
    public interface IWebPageRequestExecutor
    {
        bool Execute(WebPage page);
    }
}
