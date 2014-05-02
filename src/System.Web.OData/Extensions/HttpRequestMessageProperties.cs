// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.OData.Formatter;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;

namespace System.Web.OData.Extensions
{
    /// <summary>
    /// Provides properties for use with the <see cref="HttpRequestMessageExtensions.ODataProperties"/> extension
    /// method.
    /// </summary>
    public class HttpRequestMessageProperties
    {
        // Maintain the System.Web.OData. prefix in any new properties to avoid conflicts with user properties
        // and those of the v3 assembly.
        private const string ModelKey = "System.Web.OData.Model";
        private const string NextLinkKey = "System.Web.OData.NextLink";
        private const string PathHandlerKey = "System.Web.OData.PathHandler";
        private const string PathKey = "System.Web.OData.Path";
        private const string RouteNameKey = "System.Web.OData.RouteName";
        private const string RoutingConventionsStoreKey = "System.Web.OData.RoutingConventionsStore";
        private const string RoutingConventionsKey = "System.Web.OData.RoutingConventions";
        private const string SelectExpandClauseKey = "System.Web.OData.SelectExpandClause";
        private const string TotalCountKey = "System.Web.OData.TotalCount";

        internal const string ODataServiceVersionHeader = "OData-Version";
        internal const string ODataMaxServiceVersionHeader = "OData-MaxVersion";
        
        internal const ODataVersion DefaultODataVersion = ODataVersion.V4;

        private HttpRequestMessage _request;

        internal HttpRequestMessageProperties(HttpRequestMessage request)
        {
            Contract.Assert(request != null);
            _request = request;
        }

        /// <summary>
        /// Gets or sets the EDM model associated with the request.
        /// </summary>
        public IEdmModel Model
        {
            get
            {
                return GetValueOrNull<IEdmModel>(ModelKey);
            }
            set
            {
                _request.Properties[ModelKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the route name for generating OData links.
        /// </summary>
        public string RouteName
        {
            get
            {
                return GetValueOrNull<string>(RouteNameKey);
            }
            set
            {
                _request.Properties[RouteNameKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the OData routing conventions for controller and action selection.
        /// </summary>
        public IEnumerable<IODataRoutingConvention> RoutingConventions
        {
            get
            {
                return GetValueOrNull<IEnumerable<IODataRoutingConvention>>(RoutingConventionsKey);
            }
            set
            {
                _request.Properties[RoutingConventionsKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IODataPathHandler"/> for generating links.
        /// </summary>
        /// <value>A default <see cref="IODataPathHandler"/> implementation if previously <c>null</c>.</value>
        public IODataPathHandler PathHandler
        {
            get
            {
                IODataPathHandler pathHandler = GetValueOrNull<IODataPathHandler>(PathHandlerKey);
                if (pathHandler == null)
                {
                    pathHandler = new DefaultODataPathHandler();
                    PathHandler = pathHandler;
                }

                return pathHandler;
            }
            set
            {
                _request.Properties[PathHandlerKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the OData path of the request.
        /// </summary>
        public Routing.ODataPath Path
        {
            get
            {
                return GetValueOrNull<Routing.ODataPath>(PathKey);
            }
            set
            {
                _request.Properties[PathKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the total count for the OData response.
        /// </summary>
        /// <value><c>null</c> if no count should be sent back to the client.</value>
        public long? TotalCount
        {
            get
            {
                object totalCount;
                if (_request.Properties.TryGetValue(TotalCountKey, out totalCount))
                {
                    // Fairly big problem if following cast fails. Indicates something else is writing properties with
                    // names we've chosen. Do not silently return null because that will hide the problem.
                    return (long)totalCount;
                }

                return null;
            }
            set
            {
                if (!value.HasValue)
                {
                    throw Error.ArgumentNull("value");
                }

                _request.Properties[TotalCountKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the next link for the OData response.
        /// </summary>
        /// <value><c>null</c> if no next link should be sent back to the client.</value>
        public Uri NextLink
        {
            get
            {
                return GetValueOrNull<Uri>(NextLinkKey);
            }
            set
            {
                _request.Properties[NextLinkKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the parsed OData <see cref="SelectExpandClause"/> of the request. The
        /// <see cref="ODataMediaTypeFormatter"/> will use this information (if any) while writing the response for
        /// this request.
        /// </summary>
        public SelectExpandClause SelectExpandClause
        {
            get
            {
                return GetValueOrNull<SelectExpandClause>(SelectExpandClauseKey);
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }

                _request.Properties[SelectExpandClauseKey] = value;
            }
        }

        /// <summary>
        /// Gets the data store used by <see cref="IODataRoutingConvention"/>s to store any custom route data.
        /// </summary>
        /// <value>Initially an empty <c>IDictionary&lt;string, object&gt;</c>.</value>
        public IDictionary<string, object> RoutingConventionsStore
        {
            get
            {
                IDictionary<string, object> store =
                    GetValueOrNull<IDictionary<string, object>>(RoutingConventionsStoreKey);
                if (store == null)
                {
                    store = new Dictionary<string, object>();
                    RoutingConventionsStore = store;
                }

                return store;
            }
            private set
            {
                _request.Properties[RoutingConventionsStoreKey] = value;
            }
        }

        internal ODataVersion? ODataServiceVersion
        {
            get
            {
                return GetODataVersionFromHeader(_request.Headers, ODataServiceVersionHeader);
            }
        }

        internal ODataVersion? ODataMaxServiceVersion
        {
            get
            {
                return GetODataVersionFromHeader(_request.Headers, ODataMaxServiceVersionHeader);
            }
        }

        private static ODataVersion? GetODataVersionFromHeader(HttpHeaders headers, string headerName)
        {
            IEnumerable<string> values;
            if (headers.TryGetValues(headerName, out values))
            {
                string value = values.FirstOrDefault();
                if (value != null)
                {
                    string trimmedValue = value.Trim(' ', ';');
                    try
                    {
                        return ODataUtils.StringToODataVersion(trimmedValue);
                    }
                    catch (ODataException)
                    {
                        // Parsing the odata version failed.
                    }
                }
            }

            return null;
        }

        private T GetValueOrNull<T>(string propertyName) where T : class
        {
            object value;
            if (_request.Properties.TryGetValue(propertyName, out value))
            {
                // Fairly big problem if following cast fails. Indicates something else is writing properties with
                // names we've chosen. Do not silently return null because that will hide the problem.
                return (T)value;
            }

            return null;
        }
    }
}
