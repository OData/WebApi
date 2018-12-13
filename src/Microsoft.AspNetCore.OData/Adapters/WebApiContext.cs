// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;
using IODataRoutingConvention = Microsoft.AspNet.OData.Routing.Conventions.IODataRoutingConvention;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi OData properties to OData WebApi.
    /// </summary>
    internal class WebApiContext : IWebApiContext
    {
        /// <summary>
        /// The inner context wrapped by this instance.
        /// </summary>
        private IODataFeature innerFeature;

        /// <summary>
        /// Initializes a new instance of the WebApiContext class.
        /// </summary>
        /// <param name="feature">The inner feature.</param>
        public WebApiContext(IODataFeature feature)
        {
            this.innerFeature = feature;
        }

        /// <summary>
        /// Gets or sets the parsed OData <see cref="ApplyClause"/> of the request.
        /// </summary>
        public ApplyClause ApplyClause
        {
            get { return this.innerFeature.ApplyClause; }
            set { this.innerFeature.ApplyClause = value; }
        }

        /// <summary>
        /// Gets or sets the next link for the OData response.
        /// </summary>
        public Uri NextLink
        {
            get { return this.innerFeature.NextLink; }
            set { this.innerFeature.NextLink = value; }
        }

        /// <summary>
        /// Gets or sets the next link function for the OData response.
        /// </summary>
        public Func<object, ODataSerializerContext, Uri> NextLinkFunc
        {
            get { return this.innerFeature.NextLinkFunc; }
            set { this.innerFeature.NextLinkFunc = value; }
        }

        /// <summary>
        /// Gets or sets the delta link for the OData response.
        /// </summary>
        public Uri DeltaLink
        {
            get { return this.innerFeature.DeltaLink; }
            set { this.innerFeature.DeltaLink = value; }
        }

        /// <summary>
        /// Gets the OData path.
        /// </summary>
        public ODataPath Path
        {
            get { return this.innerFeature.Path; }
        }

        /// <summary>
        /// Gets the route name for generating OData links.
        /// </summary>
        public string RouteName
        {
            get { return this.innerFeature.RouteName; }
        }

        /// <summary>
        /// Gets the data store used by <see cref="IODataRoutingConvention"/>s to store any custom route data.
        /// </summary>
        /// <value>Initially an empty <c>IDictionary&lt;string, object&gt;</c>.</value>
        public IDictionary<string, object> RoutingConventionsStore
        {
            get { return this.innerFeature.RoutingConventionsStore; }
        }

        /// <summary>
        /// Gets or sets the parsed OData <see cref="SelectExpandClause"/> of the request.
        /// </summary>
        public SelectExpandClause SelectExpandClause
        {
            get { return this.innerFeature.SelectExpandClause; }
            set { this.innerFeature.SelectExpandClause = value; }
        }

        /// <summary>
        /// Gets or sets the parsed OData <see cref="SelectExpandClause"/> of the request.
        /// </summary>
        public ODataQueryOptions QueryOptions
        {
            get { return this.innerFeature.QueryOptions; }
            set { this.innerFeature.QueryOptions = value; }
        }

        /// <summary>
        /// Gets or sets the total count for the OData response.
        /// </summary>
        /// <value><c>null</c> if no count should be sent back to the client.</value>
        public long? TotalCount
        {
            get { return this.innerFeature.TotalCount; }
        }

        /// <summary>
        /// Gets or sets the total count for the OData response.
        /// </summary>
        /// <value><c>null</c> if no count should be sent back to the client.</value>
        public int PageSize
        {
            get { return this.innerFeature.PageSize; }
            set { this.innerFeature.PageSize = value; }
        }

        /// <summary>
        /// Gets or sets the total count function for the OData response.
        /// </summary>
        public Func<long> TotalCountFunc
        {
            get { return this.innerFeature.TotalCountFunc; }
            set { this.innerFeature.TotalCountFunc = value; }
        }
    }
}
