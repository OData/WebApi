//-----------------------------------------------------------------------------
// <copyright file="WebApiUrlHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Adapters
{
    /// <summary>
    /// Adapter class to abstract the Asp.Net Url helper.
    /// </summary>
    internal class WebApiUrlHelper : IWebApiUrlHelper
    {
        /// <summary>
        /// The inner request wrapped by this instance.
        /// </summary>
        internal IUrlHelper innerHelper;

        /// <summary>
        /// Initializes a new instance of the WebApiUrlHelper class.
        /// </summary>
        /// <param name="helper">The inner helper.</param>
        public WebApiUrlHelper(IUrlHelper helper)
        {
            if (helper == null)
            {
                throw Error.ArgumentNull("helper");
            }

            this.innerHelper = helper;
        }

        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler and given segments.
        /// </summary>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public string CreateODataLink(params ODataPathSegment[] segments)
        {
            return this.CreateODataLink(segments as IList<ODataPathSegment>);
        }

        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler and given segments.
        /// </summary>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public string CreateODataLink(IList<ODataPathSegment> segments)
        {
            string routeName = this.innerHelper.ActionContext.HttpContext.Request.ODataFeature().RouteName;
            if (String.IsNullOrEmpty(routeName))
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveODataRouteName);
            }

            IODataPathHandler pathHandler = this.innerHelper.ActionContext.HttpContext.Request.GetPathHandler();
            return CreateODataLink(routeName, pathHandler, segments);
        }

        /// <summary>
        /// Generates an OData link using the given OData route name, path handler, and segments.
        /// </summary>
        /// <param name="routeName">The name of the OData route.</param>
        /// <param name="pathHandler">The path handler to use for generating the link.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public string CreateODataLink(string routeName, IODataPathHandler pathHandler, IList<ODataPathSegment> segments)
        {
            if (String.IsNullOrEmpty(routeName))
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveODataRouteName);
            }

            if (pathHandler == null)
            {
                throw Error.ArgumentNull("pathHandler");
            }

            string odataPath = pathHandler.Link(new ODataPath(segments));

            return this.innerHelper.Link(
                routeName,
                new RouteValueDictionary() { { ODataRouteConstants.ODataPath, odataPath } });
        }
    }
}
