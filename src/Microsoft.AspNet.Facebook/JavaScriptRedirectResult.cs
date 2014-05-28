// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Web;
using System.Web.Mvc;

namespace Microsoft.AspNet.Facebook
{
    /// <summary>
    /// Represents an <see cref="ActionResult"/> that redirects to a permission prompt login via JavaScript.
    /// </summary>
    public class JavaScriptRedirectResult : ContentResult
    {
        /// <summary>
        /// Creates a JavaScript based redirect <see cref="ActionResult"/>.
        /// </summary>
        /// <param name="redirectUrl">The url to redirect to.</param>
        public JavaScriptRedirectResult(Uri redirectUrl)
        {
            ContentType = "text/html";
            Content = String.Format(
                CultureInfo.InvariantCulture,
                "<script>window.top.location = '{0}';</script>",
                HttpUtility.JavaScriptStringEncode(redirectUrl.AbsoluteUri));
        }
    }
}
