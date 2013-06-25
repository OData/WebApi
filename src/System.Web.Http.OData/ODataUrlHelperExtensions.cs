// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http.OData;
using System.Web.Http.OData.Properties;
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
        /// Generates an OData link using the request's OData route name and path handler.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string ODataLink(this UrlHelper urlHelper, params ODataPathSegment[] segments)
        {
            return urlHelper.ODataLink(segments as IList<ODataPathSegment>);
        }

        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string ODataLink(this UrlHelper urlHelper, IList<ODataPathSegment> segments)
        {
            if (urlHelper == null)
            {
                throw Error.ArgumentNull("urlHelper");
            }

            HttpRequestMessage request = urlHelper.Request;
            Contract.Assert(request != null);

            string routeName = request.GetODataRouteName();

            if (routeName == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveODataRouteName);
            }

            IODataPathHandler pathHandler = request.GetODataPathHandler();
            return urlHelper.ODataLink(routeName, pathHandler, segments);
        }

        /// <summary>
        /// Generates an OData link using the given OData route name and path handler.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="routeName">The name of the OData route.</param>
        /// <param name="pathHandler">The path handler to use for generating the link.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string ODataLink(this UrlHelper urlHelper, string routeName, IODataPathHandler pathHandler, IList<ODataPathSegment> segments)
        {
            if (urlHelper == null)
            {
                throw Error.ArgumentNull("urlHelper");
            }

            if (pathHandler == null)
            {
                throw Error.ArgumentNull("pathHandler");
            }

            string odataPath = pathHandler.Link(new ODataPath(segments));
            return urlHelper.Link(
                routeName,
                new HttpRouteValueDictionary() { { ODataRouteConstants.ODataPath, odataPath } });
        }
    }
}
