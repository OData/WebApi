// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.OData.Extensions;
using System.Web.OData.Properties;
using Microsoft.OData.Core;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IHttpRouteConstraint"/> that only matches a specific OData protocol 
    /// version. This constraint won't match incomming requests that contain any of the previous OData version
    /// headers (for OData versions 1.0 to 3.0) regardless of the version in the current version headers.
    /// </summary>
    public class ODataVersionConstraint : IHttpRouteConstraint
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
        }

        /// <summary>
        /// The version of the OData protocol that an OData-Version or OData-MaxVersion request header must have
        /// in order to be processed by the OData service with this route constraint.
        /// </summary>
        public ODataVersion Version { get; private set; }

        /// <inheritdoc />
        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName,
            IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (routeDirection == HttpRouteDirection.UriGeneration)
            {
                return true;
            }

            if (ContainsPreviousVersionHeaders(request))
            {
                return false;
            }

            ODataVersion? requestVersion = GetVersion(request);
            return requestVersion.HasValue && requestVersion.Value == Version;
        }

        private static bool ContainsPreviousVersionHeaders(HttpRequestMessage request)
        {
            return request.Headers.Contains(PreviousODataVersionHeaderName) ||
                   request.Headers.Contains(PreviousODataMinVersionHeaderName) ||
                   request.Headers.Contains(PreviousODataMaxVersionHeaderName);
        }

        private ODataVersion? GetVersion(HttpRequestMessage request)
        {
            // The logic is as follows. We check DataServiceVersion first and if not present we check MaxDataServiceVersion.
            // In order to consider any header valid, it needs to have exactly one valid value, for example a header with
            // value 3.0 will be considered valid, but a header with two values 3.0, 4.0 won't. The user experience is 
            // better if you fail with a 404 here than if you "guess" the version. The moment we see an invalid header 
            // we return a null version. The spec says that in the case of an invalid odata version the service should
            // return 4xx.
            int versionHeaderCount = GetHeaderCount(HttpRequestMessageProperties.ODataServiceVersionHeader, request);
            int maxVersionHeaderCount = GetHeaderCount(HttpRequestMessageProperties.ODataMaxServiceVersionHeader, request);

            if ((versionHeaderCount == 1 && request.ODataProperties().ODataServiceVersion != null))
            {
                return request.ODataProperties().ODataServiceVersion;
            }
            else if ((versionHeaderCount == 0 && maxVersionHeaderCount == 1 &&
               request.ODataProperties().ODataMaxServiceVersion != null))
            {
                return request.ODataProperties().ODataMaxServiceVersion;
            }
            else if (versionHeaderCount == 0 && maxVersionHeaderCount == 0)
            {
                return Version;
            }
            else
            {
                return null;
            }
        }

        private static int GetHeaderCount(string headerName, HttpRequestMessage request)
        {
            IEnumerable<string> values;
            if (request.Headers.TryGetValues(headerName, out values))
            {
                return values.Count();
            }
            return 0;
        }
    }
}
