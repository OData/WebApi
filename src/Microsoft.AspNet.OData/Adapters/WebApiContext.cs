// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi OData properties to OData WebApi.
    /// </summary>
    internal class WebApiContext 
        : IWebApiContext
    {
        /// <summary>
        /// The inner context wrapped by this instance.
        /// </summary>
        private HttpRequestMessageProperties innerContext;

        /// <summary>
        /// Initializes a new instance of the WebApiContext class.
        /// </summary>
        /// <param name="context">The inner context.</param>
        public WebApiContext(HttpRequestMessageProperties context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            this.innerContext = context;
        }

        /// <summary>
        /// Gets or sets the parsed OData <see cref="ApplyClause"/> of the request.
        /// </summary>
        public ApplyClause ApplyClause
        {
            get { return this.innerContext.ApplyClause; }
            set { this.innerContext.ApplyClause = value; }
        }

        /// <summary>
        /// Gets or sets the next link for the OData response.
        /// </summary>
        public Uri NextLink
        {
            get { return this.innerContext.NextLink; }
            set { this.innerContext.NextLink = value; }
        }

        /// <summary>
        /// Gets or sets the delta link for the OData response.
        /// </summary>
        public Uri DeltaLink
        {
            get { return this.innerContext.DeltaLink; }
            set { this.innerContext.DeltaLink = value; }
        }

        /// <summary>
        /// Gets the OData path.
        /// </summary>
        public ODataPath Path
        {
            get { return this.innerContext.Path; }
        }

        /// <summary>
        /// Gets the route name for generating OData links.
        /// </summary>
        public string RouteName
        {
            get { return this.innerContext.RouteName; }
        }

        /// <summary>
        /// Gets the data store used by <see cref="IODataRoutingConvention"/>s to store any custom route data.
        /// </summary>
        /// <value>Initially an empty <c>IDictionary&lt;string, object&gt;</c>.</value>
        public IDictionary<string, object> RoutingConventionsStore
        {
            get { return this.innerContext.RoutingConventionsStore; }
        }

        /// <summary>
        /// Gets or sets the parsed OData <see cref="SelectExpandClause"/> of the request.
        /// </summary>
        public SelectExpandClause ProcessedSelectExpandClause
        {
            get { return this.innerContext.SelectExpandClause; }
            set { this.innerContext.SelectExpandClause = value; }
        }

        /// <summary>
        /// Gets or sets the parsed OData <see cref="IODataQueryOptions"/> of the request.
        /// </summary>
        public IODataQueryOptions QueryOptions 
        {
            get { return this.innerContext.QueryOptions; }
            set { this.innerContext.QueryOptions = value; }
        }

        /// <summary>
        /// Gets or sets the total count for the OData response.
        /// </summary>
        /// <value><c>null</c> if no count should be sent back to the client.</value>
        public long? TotalCount
        {
            get { return this.innerContext.TotalCount; }
        }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize
        {
            get { return this.innerContext.PageSize; }
            set { this.innerContext.PageSize = value; }
        }

        /// <summary>
        /// Gets or sets the total count function for the OData response.
        /// </summary>
        public Func<long> TotalCountFunc
        {
            get { return this.innerContext.TotalCountFunc; }
            set { this.innerContext.TotalCountFunc = value; }
        }

        
    }
}
