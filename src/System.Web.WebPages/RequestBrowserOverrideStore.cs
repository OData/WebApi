// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.WebPages
{
    /// <summary>
    /// RequestBrowserOverrideStore simply returns the user agent of the current request.
    /// </summary>
    internal sealed class RequestBrowserOverrideStore : BrowserOverrideStore
    {
        public override string GetOverriddenUserAgent(HttpContextBase httpContext)
        {
            return httpContext.Request.UserAgent;
        }

        public override void SetOverriddenUserAgent(HttpContextBase httpContext, string userAgent)
        {
            return;
        }
    }
}
