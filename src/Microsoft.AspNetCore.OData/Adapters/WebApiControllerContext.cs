//-----------------------------------------------------------------------------
// <copyright file="WebApiControllerContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNet.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi controller context to OData WebApi.
    /// </summary>
    internal class WebApiControllerContext : IWebApiControllerContext
    {
        /// <summary>
        /// The inner context wrapped by this instance.
        /// </summary>
        private RouteContext innerContext;

        /// <summary>
        /// Initializes a new instance of the WebApiControllerContext class.
        /// </summary>
        /// <param name="routeContext">The inner context.</param>
        /// <param name="controllerResult">The selected controller result.</param>
        public WebApiControllerContext(RouteContext routeContext, SelectControllerResult controllerResult)
        {
            if (routeContext == null)
            {
                throw Error.ArgumentNull("routeContext");
            }

            if (controllerResult == null)
            {
                throw Error.ArgumentNull("controllerResult");
            }

            this.innerContext = routeContext;
            this.ControllerResult = controllerResult;

            HttpRequest request = routeContext.HttpContext.Request;
            if (request != null)
            {
                this.Request = new WebApiRequestMessage(request);
            }
        }

        /// <summary>
        /// The selected controller result.
        /// </summary>
        public SelectControllerResult ControllerResult { get; private set; }

        /// <summary>
        /// Gets the request.
        /// </summary>
        public IWebApiRequestMessage Request { get; private set; }

        /// <summary>
        /// Gets the route data.
        /// </summary>
        public IDictionary<string, object> RouteData
        {
            get { return this.innerContext.RouteData.Values; }
        }
    }
}
