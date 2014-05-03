// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query.SemanticAst;

namespace System.Web.Http.OData.Extensions
{
    /// <summary>
    /// Provides properties for use with the <see cref="HttpRequestMessageExtensions.ODataProperties"/> extension
    /// method.
    /// </summary>
    public class HttpRequestMessageProperties
    {
        private const string ModelKey = "MS_EdmModel";
        private const string NextLinkKey = "MS_NextPageLink";
        private const string PathHandlerKey = "MS_ODataPathHandler";
        private const string PathKey = "MS_ODataPath";
        private const string RouteNameKey = "MS_ODataRouteName";
        private const string RoutingConventionsStoreKey = "MS_RoutingConventionDataStore";
        private const string RoutingConventionsKey = "MS_ODataRoutingConventions";
        private const string SelectExpandClauseKey = "MS_SelectExpandClause";
        private const string TotalCountKey = "MS_InlineCount";

        internal const string ODataServiceVersionHeader = "DataServiceVersion";
        internal const string ODataMaxServiceVersionHeader = "MaxDataServiceVersion";

        internal const ODataVersion DefaultODataVersion = ODataVersion.V3;

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
        /// Gets or sets the <see cref="IODataPathHandler"/> for generating links. Getter creates a default
        /// <see cref="IODataPathHandler"/> if value is currently <c>null</c>.
        /// </summary>
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
        /// Gets or sets the total count for the OData response. Getter returns <c>null</c> if no count should be sent
        /// back to the client.
        /// </summary>
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
        /// Gets or sets the next link for the OData response. Getter returns <c>null</c> if no next link should be
        /// sent back to the client.
        /// </summary>
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
        /// Gets the data store used by <see cref="IODataRoutingConvention"/>s to store any custom route data. Getter
        /// creates a new <c>IDictionary&lt;string, object&gt;</c> the first time it is called.
        /// </summary>
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
