// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="UrlHelper"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ODataUrlHelperExtensions
    {
        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler and given segments.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        [Obsolete("This method is obsolete; use the CreateODataLink method from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static string ODataLink(this UrlHelper urlHelper, params ODataPathSegment[] segments)
        {
            return UrlHelperExtensions.CreateODataLink(urlHelper, segments);
        }

        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler and given segments.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        [Obsolete("This method is obsolete; use the CreateODataLink method from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static string ODataLink(this UrlHelper urlHelper, IList<ODataPathSegment> segments)
        {
            return UrlHelperExtensions.CreateODataLink(urlHelper, segments);
        }

        /// <summary>
        /// Generates an OData link using the given OData route name, path handler, and segments.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="routeName">The name of the OData route.</param>
        /// <param name="pathHandler">The path handler to use for generating the link.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        [Obsolete("This method is obsolete; use the CreateODataLink method from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static string ODataLink(this UrlHelper urlHelper, string routeName, IODataPathHandler pathHandler,
            IList<ODataPathSegment> segments)
        {
            return UrlHelperExtensions.CreateODataLink(urlHelper, routeName, pathHandler, segments);
        }
    }
}
