// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.OData.Routing;

namespace System.Web.Http.Routing
{
    /// <summary>
    /// Provides extension methods for the <see cref="UrlHelper"/> class.
    /// </summary>
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Generates an OData link using the default OData route name.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="pathHandler">The path handler to use for generating the link.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string ODataLink(this UrlHelper urlHelper, IODataPathHandler pathHandler, params ODataPathSegment[] segments)
        {
            return urlHelper.ODataLink(pathHandler, segments as IList<ODataPathSegment>);
        }

        /// <summary>
        /// Generates an OData link using the default OData route name.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="pathHandler">The path handler to use for generating the link.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The geerated OData link.</returns>
        public static string ODataLink(this UrlHelper urlHelper, IODataPathHandler pathHandler, IList<ODataPathSegment> segments)
        {
            string odataPath = pathHandler.Link(new ODataPath(segments));
            return urlHelper.Link(
                ODataRouteConstants.RouteName,
                new HttpRouteValueDictionary() { { ODataRouteConstants.ODataPath, odataPath } });
        }
    }
}
