// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;
using IODataRoutingConvention = Microsoft.AspNet.OData.Routing.Conventions.IODataRoutingConvention;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Contains the details of a given OData request. These properties should all be mutable.
    /// None of these properties should ever be set to null.
    /// </summary>
    public class ODataFeature : IODataFeature, IDisposable
    {
        internal const string ODataServiceVersionHeader = "OData-Version";
        internal const ODataVersion DefaultODataVersion = ODataVersion.V4;

        /// <summary>
        /// Instantiates a new instance of the <see cref="ODataFeature"/> class.
        /// </summary>
        public ODataFeature()
        {
            Model = EdmCoreModel.Instance; // default Edm model
        }

        /// <summary>
        /// Gets or sets the EDM model.
        /// </summary>
        public IEdmModel Model { get; set; }

        /// <summary>
        /// Gets or sets the OData path.
        /// </summary>
        public ODataPath Path { get; set; }

        /// <summary>
        /// Gets or sets the route name.
        /// </summary>
        public string RouteName { get; set; }

        /// <summary>
        /// Gets or sets the request scope.
        /// </summary>
        public IServiceScope RequestScope { get; set; }

        /// <summary>
        /// Gets or sets the request container.
        /// </summary>
        public IServiceProvider RequestContainer { get; set; }

        /// <summary>
        /// Gets or sets whether the request is the valid OData request.
        /// </summary>
        public bool IsValidODataRequest
        {
            get { return this.Path != null; }
        }

        /// <summary>
        /// Gets or sets the next link for the OData response.
        /// </summary>
        public Uri NextLink { get; set; }

        /// <summary>
        /// Gets or sets the delta link for the OData response.
        /// </summary>
        public Uri DeltaLink { get; set; }

        /// <summary>
        /// Gets or sets the total count for the OData response.
        /// </summary>
        /// <value><c>null</c> if no count should be sent back to the client.</value>
        public long? TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the total count function for the OData response.
        /// </summary>
        public Func<long> TotalCountFunc { get; set; }

        /// <summary>
        /// Gets or sets the parsed OData <see cref="ApplyClause"/> of the request.
        /// </summary>
        public ApplyClause ApplyClause { get; set; }

        /// <summary>
        /// Gets or sets the parsed OData <see cref="SelectExpandClause"/> of the request.
        /// </summary>
        public SelectExpandClause SelectExpandClause { get; set; }

        /// <summary>
        /// Gets the data store used by <see cref="IODataRoutingConvention"/>s to store any custom route data.
        /// </summary>
        /// <value>Initially an empty <c>IDictionary&lt;string, object&gt;</c>.</value>
        public IDictionary<string, object> RoutingConventionsStore { get; set; } = new Dictionary<string, object>();

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc/>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                RequestScope?.Dispose();
            }
        }
    }
}
