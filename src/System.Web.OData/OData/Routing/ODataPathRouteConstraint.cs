// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.OData.Extensions;
using System.Web.OData.Properties;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IHttpRouteConstraint"/> that only matches OData paths.
    /// </summary>
    public class ODataPathRouteConstraint : IHttpRouteConstraint
    {
        // "%2F"
        private static readonly string _escapedSlash = Uri.HexEscape('/');

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathRouteConstraint" /> class.
        /// </summary>
        /// <param name="pathHandler">The OData path handler to use for parsing.</param>
        /// <param name="model">The EDM model to use for parsing the path.</param>
        /// <param name="routeName">The name of the route this constraint is associated with.</param>
        /// <param name="routingConventions">The OData routing conventions to use for selecting the controller name.</param>
        public ODataPathRouteConstraint(IODataPathHandler pathHandler, IEdmModel model, string routeName, IEnumerable<IODataRoutingConvention> routingConventions)
        {
            if (pathHandler == null)
            {
                throw Error.ArgumentNull("pathHandler");
            }

            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (routeName == null)
            {
                throw Error.ArgumentNull("routeName");
            }

            if (routingConventions == null)
            {
                throw Error.ArgumentNull("routingConventions");
            }

            PathHandler = pathHandler;
            EdmModel = model;
            RouteName = routeName;
            RoutingConventions = new Collection<IODataRoutingConvention>(routingConventions.ToList());
        }

        /// <summary>
        /// Gets the OData path handler to use for parsing.
        /// </summary>
        public IODataPathHandler PathHandler
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the EDM model to use for parsing the path.
        /// </summary>
        public IEdmModel EdmModel
        {
            get;
            private set;
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
        /// Gets the OData routing conventions to use for selecting the controller name.
        /// </summary>
        public Collection<IODataRoutingConvention> RoutingConventions
        {
            get;
            private set;
        }

        /// <summary>
        /// Determines whether this instance equals a specified route.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="route">The route to compare.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="values">A list of parameter values.</param>
        /// <param name="routeDirection">The route direction.</param>
        /// <returns>
        /// True if this instance equals a specified route; otherwise, false.
        /// </returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed after being sent.")]
        public virtual bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (values == null)
            {
                throw Error.ArgumentNull("values");
            }

            if (routeDirection == HttpRouteDirection.UriResolution)
            {
                object oDataPathValue;
                if (values.TryGetValue(ODataRouteConstants.ODataPath, out oDataPathValue))
                {
                    string oDataPathString = oDataPathValue as string;
                    ODataPath path;

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
                        // Due to this decoding and the possibility of unecessarily-escaped characters, there's no
                        // reliable way to determine the original string from which oDataPathString was derived.
                        // Therefore a straightforward string comparison won't always work.  See RemoveODataPath() for
                        // details of chosen approach.
                        string requestLeftPart = request.RequestUri.GetLeftPart(UriPartial.Path);
                        string serviceRoot = requestLeftPart;
                        if (!String.IsNullOrEmpty(oDataPathString))
                        {
                            serviceRoot = RemoveODataPath(serviceRoot, oDataPathString);
                        }

                        // As mentioned above, we also need escaped ODataPath.
                        // The requestLeftPart and request.RequestUri.Query are both escaped.
                        // The ODataPath for service documents is empty.
                        string oDataPathAndQuery = requestLeftPart.Substring(serviceRoot.Length);
                        if (!String.IsNullOrEmpty(request.RequestUri.Query))
                        {
                            // Ensure path handler receives the query string as well as the path.
                            oDataPathAndQuery += request.RequestUri.Query;
                        }

                        // Leave an escaped '/' out of the service route because DefaultODataPathHandler will add a
                        // literal '/' to the end of this string if not already present. That would double the slash
                        // in response links and potentially lead to later 404s.
                        if (serviceRoot.EndsWith(_escapedSlash, StringComparison.OrdinalIgnoreCase))
                        {
                            serviceRoot = serviceRoot.Substring(0, serviceRoot.Length - 3);
                        }

                        path = PathHandler.Parse(EdmModel, serviceRoot, oDataPathAndQuery);
                    }
                    catch (ODataException)
                    {
                        path = null;
                    }

                    if (path != null)
                    {
                        // Set all the properties we need for routing, querying, formatting
                        request.ODataProperties().Model = EdmModel;
                        request.ODataProperties().PathHandler = PathHandler;
                        request.ODataProperties().Path = path;
                        request.ODataProperties().RouteName = RouteName;
                        request.ODataProperties().RoutingConventions = RoutingConventions;

                        if (!values.ContainsKey(ODataRouteConstants.Controller))
                        {
                            // Select controller name using the routing conventions
                            string controllerName = SelectControllerName(path, request);
                            if (controllerName != null)
                            {
                                values[ODataRouteConstants.Controller] = controllerName;
                            }
                        }

                        return true;
                    }
                }

                return false;
            }
            else
            {
                // This constraint only applies to URI resolution
                return true;
            }
        }

        /// <summary>
        /// Selects the name of the controller to dispatch the request to.
        /// </summary>
        /// <param name="path">The OData path of the request.</param>
        /// <param name="request">The request.</param>
        /// <returns>The name of the controller to dispatch to, or <c>null</c> if one cannot be resolved.</returns>
        protected virtual string SelectControllerName(ODataPath path, HttpRequestMessage request)
        {
            foreach (IODataRoutingConvention routingConvention in RoutingConventions)
            {
                string controllerName = routingConvention.SelectController(path, request);
                if (controllerName != null)
                {
                    return controllerName;
                }
            }

            return null;
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
        // uriString because subsequent comparisons would only help to check wehther a match is _possible_, not where
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
