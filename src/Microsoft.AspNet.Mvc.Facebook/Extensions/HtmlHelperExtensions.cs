// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Microsoft.AspNet.Mvc.Facebook.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlString FacebookSignedRequest(this HtmlHelper helper)
        {
            var signedRequest = helper.ViewContext.HttpContext.Request.Params["signed_request"];
            if (!string.IsNullOrEmpty(signedRequest))
            {
                return helper.Hidden("signed_request", signedRequest);
            }
            return new HtmlString("");
        }
    }
}
