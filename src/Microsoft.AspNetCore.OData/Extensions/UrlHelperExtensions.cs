// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ODataPathSegment = Microsoft.OData.UriParser.ODataPathSegment;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Http.Extensions;

namespace Microsoft.AspNet.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="IUrlHelper"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler and given segments.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string CreateODataLink(this IUrlHelper urlHelper, params ODataPathSegment[] segments)
        {
            return CreateODataLink(urlHelper, segments as IList<ODataPathSegment>);
        }

        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler and given segments.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string CreateODataLink(this IUrlHelper urlHelper, IList<ODataPathSegment> segments)
        {
            if (urlHelper == null)
            {
                throw Error.ArgumentNull("urlHelper");
            }

            HttpRequest request = urlHelper.ActionContext.HttpContext.Request;
            Contract.Assert(request != null);

            string routeName = request.ODataFeature().RouteName;
            if (String.IsNullOrEmpty(routeName))
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveODataRouteName);
            }

            ILoggerFactory loggeFactory = request.HttpContext.RequestServices.GetService<ILoggerFactory>();
            ILogger logger = loggeFactory.CreateLogger<ODataInputFormatter>();

            string uri = request.GetDisplayUrl();
            logger.LogInformation($"[ODataInfo:] ODataInputFormatter 2, RouteName='{routeName}', request={uri} ...");

            IODataPathHandler pathHandler = request.GetPathHandler();
            string link = CreateODataLink(urlHelper, routeName, pathHandler, segments);

            logger.LogInformation($"[ODataInfo:] ODataInputFormatter 3, RouteName='{routeName}', link={link} ...");

            return link;
        }

        /// <summary>
        /// Generates an OData link using the given OData route name, path handler, and segments.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="routeName">The name of the OData route.</param>
        /// <param name="pathHandler">The path handler to use for generating the link.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string CreateODataLink(this IUrlHelper urlHelper, string routeName, IODataPathHandler pathHandler,
            IList<ODataPathSegment> segments)
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
                new RouteValueDictionary() { { ODataRouteConstants.ODataPath, odataPath } });
        }
    }
}
