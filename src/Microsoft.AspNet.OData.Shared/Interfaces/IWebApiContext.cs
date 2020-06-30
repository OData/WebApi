// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Interfaces
{
    /// <summary>
    /// General context for WebApi.
    /// </summary>
    /// <remarks>
    /// This class is not intended to be exposed publicly; it used for the internal
    /// implementations of SelectControl(). Any design which makes this class public
    /// should find an alternative design.
    /// </remarks>
    internal interface IWebApiContext
    {
        /// <summary>
        /// Gets or sets the parsed OData <see cref="ApplyClause"/> of the request.
        /// </summary>
        ApplyClause ApplyClause { get; set; }

        /// <summary>
        /// Gets or sets the next link for the OData response.
        /// </summary>
        Uri NextLink { get; set; }

        /// <summary>
        /// Gets or sets the delta link for the OData response.
        /// </summary>
        Uri DeltaLink { get; set; }

        /// <summary>
        /// Page size to be used by skiptoken implementation for the top-level resource for the request. 
        /// </summary>
        int PageSize { get; set; }

        /// <summary>
        /// Gets the OData path.
        /// </summary>
        ODataPath Path { get; }

        /// <summary>
        /// Gets the route name for generating OData links.
        /// </summary>
        string RouteName { get; }

        /// <summary>
        /// Gets the data store used by <see cref="IODataRoutingConvention"/>s to store any custom route data.
        /// </summary>
        /// <value>Initially an empty <c>IDictionary&lt;string, object&gt;</c>.</value>
        IDictionary<string, object> RoutingConventionsStore { get; }

        /// <summary>
        /// Gets or sets the processed OData <see cref="SelectExpandClause"/> of the request.
        /// </summary>
        SelectExpandClause ProcessedSelectExpandClause { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IODataQueryOptions"/> of the request.
        /// </summary>
        IODataQueryOptions QueryOptions { get; set; }

        /// <summary>
        /// Gets or sets the total count for the OData response.
        /// </summary>
        /// <value><c>null</c> if no count should be sent back to the client.</value>
        long? TotalCount { get; }

        /// <summary>
        /// Gets or sets the total count function for the OData response.
        /// </summary>
        Func<long> TotalCountFunc { get; set; }
    }
}
