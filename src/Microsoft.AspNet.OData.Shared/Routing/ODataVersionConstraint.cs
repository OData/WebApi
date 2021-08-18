//-----------------------------------------------------------------------------
// <copyright file="ODataVersionConstraint.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// An implementation of route constraint that only matches a specific OData protocol 
    /// version. This constraint won't match incoming requests that contain any of the previous OData version
    /// headers (for OData versions 1.0 to 3.0) regardless of the version in the current version headers.
    /// </summary>
    public partial class ODataVersionConstraint
    {
        // The header names used for versioning in the versions 4.0+ of the OData protocol.
        internal const string ODataServiceVersionHeader = "OData-Version";
        internal const string ODataMaxServiceVersionHeader = "OData-MaxVersion";
        internal const string ODataMinServiceVersionHeader = "OData-MinVersion";
        internal const ODataVersion DefaultODataVersion = ODataVersion.V4;

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
            Version = DefaultODataVersion;
            IsRelaxedMatch = true;
        }

        /// <summary>
        /// The (minimum) version of the OData protocol that an OData-Version or OData-MaxVersion request header must have
        /// in order to be processed by the OData service with this route constraint.
        /// </summary>
        public ODataVersion Version { get; private set; }

        /// <summary>
        /// If set to true, allow passing in both OData V4 and previous version headers.
        /// </summary>
        public bool IsRelaxedMatch { get; set; }

        /// <summary>
        /// Determine if there is a version match.
        /// </summary>
        /// <param name="headers">The request headers.</param>
        /// <param name="serviceVersion">The supported service version.</param>
        /// <param name="maxServiceVersion">The max supported service version.</param>
        /// <returns>True if there is a match; false otherwise.</returns>
        private bool IsVersionMatch(IDictionary<string, IEnumerable<string>> headers, ODataVersion? serviceVersion, ODataVersion? maxServiceVersion)
        {
            // The match behavior depends on value of IsRelaxedMatch.
            // If users select using relaxed match logic, the header contains both V3 (or before) and V4 style version
            // will be regarded as valid. While under non-relaxed match logic, both version headers presented will be
            // regarded as invalid. The behavior for other situations are the same. When non version headers present,
            // assume using V4 version.
            if (!ValidateVersionHeaders(headers))
            {
                return false;
            }

            ODataVersion? requestVersion = GetVersion(headers, serviceVersion, maxServiceVersion);
            return requestVersion.HasValue && requestVersion.Value >= Version;
        }

        private bool ValidateVersionHeaders(IDictionary<string, IEnumerable<string>> headers)
        {
            bool containPreviousVersionHeaders =
                headers.ContainsKey(PreviousODataVersionHeaderName) ||
                headers.ContainsKey(PreviousODataMinVersionHeaderName) ||
                headers.ContainsKey(PreviousODataMaxVersionHeaderName);

            bool containPreviousMaxVersionHeaderOnly =
                headers.ContainsKey(PreviousODataMaxVersionHeaderName) &&
                !headers.ContainsKey(PreviousODataVersionHeaderName) &&
                !headers.ContainsKey(PreviousODataMinVersionHeaderName);

            bool containCurrentMaxVersionHeader = headers.ContainsKey(ODataMaxServiceVersionHeader);

            return IsRelaxedMatch
                ? !containPreviousVersionHeaders || (containCurrentMaxVersionHeader && containPreviousMaxVersionHeaderOnly)
                : !containPreviousVersionHeaders;
        }

        private ODataVersion? GetVersion(IDictionary<string, IEnumerable<string>> headers, ODataVersion? serviceVersion, ODataVersion? maxServiceVersion)
        {
            // The logic is as follows. We check OData-Version first and if not present we check OData-MaxVersion.
            // If both OData-Version and OData-MaxVersion are not present, we assume the version is the service version

            int versionHeaderCount = GetHeaderCount(ODataServiceVersionHeader, headers);
            int maxVersionHeaderCount = GetHeaderCount(ODataMaxServiceVersionHeader, headers);

            if ((versionHeaderCount == 1 && serviceVersion != null))
            {
                return serviceVersion;
            }
            else if ((versionHeaderCount == 0 && maxVersionHeaderCount == 1 && maxServiceVersion != null))
            {
                return maxServiceVersion;
            }
            else if (versionHeaderCount == 0 && maxVersionHeaderCount == 0)
            {
                return this.Version;
            }
            else
            {
                return null;
            }
        }

        private static int GetHeaderCount(string headerName, IDictionary<string, IEnumerable<string>> headers)
        {
            IEnumerable<string> values;
            if (headers.TryGetValue(headerName, out values))
            {
                return values.Count();
            }
            return 0;
        }
    }
}
