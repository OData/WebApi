// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Mime;
using System.Web.Routing;

namespace Microsoft.Web.Mvc.Resources
{
    /// <summary>
    /// RequestContext extension methods that call directly into the registered FormatHelper
    /// </summary>
    public static class RequestContextExtensions
    {
        public static ContentType GetRequestFormat(this RequestContext requestContext)
        {
            return FormatManager.Current.FormatHelper.GetRequestFormat(requestContext);
        }

        public static IEnumerable<ContentType> GetResponseFormats(this RequestContext requestContext)
        {
            return FormatManager.Current.FormatHelper.GetResponseFormats(requestContext);
        }

        public static bool IsBrowserRequest(this RequestContext requestContext)
        {
            return FormatManager.Current.FormatHelper.IsBrowserRequest(requestContext);
        }
    }
}
