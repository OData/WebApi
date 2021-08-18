//-----------------------------------------------------------------------------
// <copyright file="ODataPathRouteConstraint.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// An implementation of route constraint that only matches OData paths.
    /// </summary>
    public partial class ODataPathRouteConstraint
    {
        // "%2F"
        private static readonly string _escapedSlash = Uri.EscapeDataString("/");

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathRouteConstraint" /> class.
        /// </summary>
        /// <param name="routeName">The name of the route this constraint is associated with.</param>
        public ODataPathRouteConstraint(string routeName)
        {
            if (routeName == null)
            {
                throw Error.ArgumentNull("routeName");
            }

            RouteName = routeName;
        }

        /// <summary>
        /// Gets the name of the route this constraint is associated with.
        /// </summary>
        public string RouteName
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the OData path from the url and query string.
        /// </summary>
        /// <param name="oDataPathString">The ODataPath from the route values.</param>
        /// <param name="uriPathString">The Uri from start to end of path, i.e. the left portion.</param>
        /// <param name="queryString">The Uri from the query string to the end, i.e. the right portion.</param>
        /// <param name="requestContainerFactory">The request container factory.</param>
        /// <returns>The OData path.</returns>
        internal static ODataPath GetODataPath(string oDataPathString, string uriPathString, string queryString, Func<IServiceProvider> requestContainerFactory)
        {
            ODataPath path = null;

            try
            {
                // Service root is the current RequestUri, less the query string and the ODataPath (always the
                // last portion of the absolute path).  ODL expects an escaped service root and other service
                // root calculations are calculated using AbsoluteUri (also escaped).  But routing exclusively
                // uses unescaped strings, determined using
                //    address.GetComponents(UriComponents.Path, UriFormat.Unescaped)
                //
                // For example if the AbsoluteUri is
                // <http://localhost/odata/FunctionCall(p0='Chinese%E8%A5%BF%E9%9B%85%E5%9B%BEChars')>, the
                // oDataPathString will contain "FunctionCall(p0='Chinese西雅图Chars')".
                //
                // Due to this decoding and the possibility of unnecessarily-escaped characters, there's no
                // reliable way to determine the original string from which oDataPathString was derived.
                // Therefore a straightforward string comparison won't always work.  See RemoveODataPath() for
                // details of chosen approach.
                string serviceRoot = uriPathString;

                if (!String.IsNullOrEmpty(oDataPathString))
                {
                    serviceRoot = RemoveODataPath(serviceRoot, oDataPathString);
                }

                // As mentioned above, we also need escaped ODataPath.
                // The requestLeftPart and request.QueryString are both escaped.
                // The ODataPath for service documents is empty.
                string oDataPathAndQuery = uriPathString.Substring(serviceRoot.Length);

                if (!String.IsNullOrEmpty(queryString))
                {
                    // Ensure path handler receives the query string as well as the path.
                    oDataPathAndQuery += queryString;
                }

                // Leave an escaped '/' out of the service route because DefaultODataPathHandler will add a
                // literal '/' to the end of this string if not already present. That would double the slash
                // in response links and potentially lead to later 404s.
                if (serviceRoot.EndsWith(_escapedSlash, StringComparison.OrdinalIgnoreCase))
                {
                    serviceRoot = serviceRoot.Substring(0, serviceRoot.Length - _escapedSlash.Length);
                }

                IServiceProvider requestContainer = requestContainerFactory();
                IODataPathHandler pathHandler = requestContainer.GetRequiredService<IODataPathHandler>();
                path = pathHandler.Parse(serviceRoot, oDataPathAndQuery, requestContainer);
            }
            catch (ODataException)
            {
                path = null;
            }

            return path;
        }

        // Find the substring of the given URI string before the given ODataPath.  Tests rely on the following:
        // 1. ODataPath comes at the end of the processed Path
        // 2. Virtual path root, if any, comes at the beginning of the Path and a '/' separates it from the rest
        // 3. OData prefix, if any, comes between the virtual path root and the ODataPath and '/' characters separate
        //    it from the rest
        // 4. Even in the case of Unicode character corrections, the only differences between the escaped Path and the
        //    unescaped string used for routing are %-escape sequences which may be present in the Path
        //
        // Therefore, look for the '/' character at which to lop off the ODataPath.  Can't just unescape the given
        // uriString because subsequent comparisons would only help to check whether a match is _possible_, not where
        // to do the lopping.
        private static string RemoveODataPath(string uriString, string oDataPathString)
        {
            // Potential index of oDataPathString within uriString.
            int endIndex = uriString.Length - oDataPathString.Length - 1;
            if (endIndex <= 0)
            {
                // Bizarre: oDataPathString is longer than uriString.  Likely the values collection passed to Match()
                // is corrupt.
                throw Error.InvalidOperation(SRResources.RequestUriTooShortForODataPath, uriString, oDataPathString);
            }

            string startString = uriString.Substring(0, endIndex + 1);  // Potential return value.
            string endString = uriString.Substring(endIndex + 1);       // Potential oDataPathString match.
            if (String.Equals(endString, oDataPathString, StringComparison.Ordinal))
            {
                // Simple case, no escaping in the ODataPathString portion of the Path.  In this case, don't do extra
                // work to look for trailing '/' in startString.
                return startString;
            }

            while (true)
            {
                // Escaped '/' is a derivative case but certainly possible.
                int slashIndex = startString.LastIndexOf('/', endIndex - 1);
                int escapedSlashIndex =
                    startString.LastIndexOf(_escapedSlash, endIndex - 1, StringComparison.OrdinalIgnoreCase);
                if (slashIndex > escapedSlashIndex)
                {
                    endIndex = slashIndex;
                }
                else if (escapedSlashIndex >= 0)
                {
                    // Include the escaped '/' (three characters) in the potential return value.
                    endIndex = escapedSlashIndex + 2;
                }
                else
                {
                    // Failure, unable to find the expected '/' or escaped '/' separator.
                    throw Error.InvalidOperation(SRResources.ODataPathNotFound, uriString, oDataPathString);
                }

                startString = uriString.Substring(0, endIndex + 1);
                endString = uriString.Substring(endIndex + 1);

                // Compare unescaped strings to avoid both arbitrary escaping and use of lowercase 'a' through 'f' in
                // %-escape sequences.
                endString = Uri.UnescapeDataString(endString);
                if (String.Equals(endString, oDataPathString, StringComparison.Ordinal))
                {
                    return startString;
                }

                if (endIndex == 0)
                {
                    // Failure, could not match oDataPathString after an initial '/' or escaped '/'.
                    throw Error.InvalidOperation(SRResources.ODataPathNotFound, uriString, oDataPathString);
                }
            }
        }
    }
}
