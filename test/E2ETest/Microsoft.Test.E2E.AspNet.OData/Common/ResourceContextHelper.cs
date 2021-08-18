//-----------------------------------------------------------------------------
// <copyright file="ResourceContextHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.UriParser;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public static class ResourceContextHelper
    {
        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler and given segments.
        /// </summary>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string CreateODataLink(ResourceContext context, params ODataPathSegment[] segments)
        {
#if NETCORE
            return context.Request.HttpContext.GetUrlHelper().CreateODataLink(segments);
#else
            return context.Url.CreateODataLink(segments);
#endif
        }

        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler and given segments.
        /// </summary>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string CreateODataLink(ResourceContext context, IList<ODataPathSegment> segments)
        {
#if NETCORE
            return context.Request.HttpContext.GetUrlHelper().CreateODataLink(segments);
#else
            return context.Url.CreateODataLink(segments);
#endif
        }

        /// <summary>
        /// Generates an OData link using the given OData route name, path handler, and segments.
        /// </summary>
        /// <param name="routeName">The name of the OData route.</param>
        /// <param name="pathHandler">The path handler to use for generating the link.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string CreateODataLink(ResourceContext context, string routeName, IODataPathHandler pathHandler, IList<ODataPathSegment> segments)
        {
#if NETCORE
            return context.Request.HttpContext.GetUrlHelper().CreateODataLink(routeName, pathHandler, segments);
#else
            return context.Url.CreateODataLink(routeName, pathHandler, segments);
#endif
        }
    }
}
