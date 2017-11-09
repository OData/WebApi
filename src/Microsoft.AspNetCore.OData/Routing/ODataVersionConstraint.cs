// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IHttpRouteConstraint"/> that only matches a specific OData protocol 
    /// version. This constraint won't match incoming requests that contain any of the previous OData version
    /// headers (for OData versions 1.0 to 3.0) regardless of the version in the current version headers.
    /// </summary>
    public class ODataVersionConstraint : IRouteConstraint
    {
        // The header names used for versioning in the versions 1.0 to 3.0 of the OData protocol.
        private const string PreviousODataVersionHeaderName = "DataServiceVersion";
        private const string PreviousODataMaxVersionHeaderName = "MaxDataServiceVersion";
        private const string PreviousODataMinVersionHeaderName = "MinDataServiceVersion";

        /// <summary>
        /// Creates a new instance of the <see cref="ODataVersionConstraint"/> class that will have a default version
        /// of 4.0.
        /// </summary>
        public ODataVersionConstraint()
        {
            Version = ODataVersion.V4;
            IsRelaxedMatch = true;
        }

        /// <summary>
        /// The version of the OData protocol that an OData-Version or OData-MaxVersion request header must have
        /// in order to be processed by the OData service with this route constraint.
        /// </summary>
        public ODataVersion Version { get; private set; }

        /// <summary>
        /// If set to true, allow passing in both OData V4 and previous version headers.
        /// </summary>
        public bool IsRelaxedMatch { get; set; }

        /// <inheritdoc />
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            throw new NotImplementedException();
        }
    }
}
