//-----------------------------------------------------------------------------
// <copyright file="ResourceContextExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Http.Routing;
using Microsoft.AspNet.OData;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Common.Extensions
{
    /// <summary>
    /// Extensions for IWebApiUrlHelper.
    /// </summary>
    public static class ResourceContextExtensions
    {
#if NETCORE
        public static IUrlHelper GetUrlHelper(this ResourceContext context)
        {
            return context.Request.HttpContext.GetUrlHelper();
        }
#else
        public static UrlHelper GetUrlHelper(this ResourceContext context)
        {
            return context.Url;
        }
#endif
    }
}
