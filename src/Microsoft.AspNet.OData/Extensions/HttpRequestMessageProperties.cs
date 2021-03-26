// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Extensions
{
    /// <summary>
    /// Provides properties for use with the <see cref="HttpRequestMessageExtensions.ODataProperties"/> extension
    /// method.
    /// </summary>
    public class HttpRequestMessageProperties
    {
        // Maintain the Microsoft.AspNet.OData. prefix in any new properties to avoid conflicts with user properties
        // and those of the v3 assembly.
        private const string DeltaLinkKey = "Microsoft.AspNet.OData.DeltaLink";
        private const string NextLinkKey = "Microsoft.AspNet.OData.NextLink";
        private const string PathKey = "Microsoft.AspNet.OData.Path";
        private const string RouteNameKey = "Microsoft.AspNet.OData.RouteName";
        private const string RoutingConventionsStoreKey = "Microsoft.AspNet.OData.RoutingConventionsStore";
        private const string SelectExpandClauseKey = "Microsoft.AspNet.OData.SelectExpandClause";
        private const string ApplyClauseKey = "Microsoft.AspNet.OData.ApplyClause";
        private const string TotalCountKey = "Microsoft.AspNet.OData.TotalCount";
        private const string TotalCountFuncKey = "Microsoft.AspNet.OData.TotalCountFunc";
        private const string PageSizeKey = "Microsoft.AspNet.OData.PageSize";
        private const string QueryOptionsKey = "Microsoft.AspNet.OData.QueryOptions";

        private HttpRequestMessage _request;

        internal HttpRequestMessageProperties(HttpRequestMessage request)
        {
            Contract.Assert(request != null);
            _request = request;
        }

        internal Func<long> TotalCountFunc
        {
            get
            {
                object totalCountFunc;
                if (_request.Properties.TryGetValue(TotalCountFuncKey, out totalCountFunc))
                {
                    return (Func<long>)totalCountFunc;
                }

                return null;
            }
            set
            {
                _request.Properties[TotalCountFuncKey] = value;
            }
        }

        /// <summary>
        /// Page size to be used by skiptoken implementation for the top-level resource for the request. 
        /// </summary>
        internal int PageSize
        {
            get
            {
                object pageSize;
                if (_request.Properties.TryGetValue(PageSizeKey, out pageSize))
                {
                    return (int)pageSize;
                }
                return -1;
            }
            set
            {
                _request.Properties[PageSizeKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the parsed OData <see cref="ODataQueryOptions"/> of the request. The
        /// <see cref="ODataResourceSet "/> will use this information (if any) while writing the response for
        /// this request.
        /// </summary>
        internal IODataQueryOptions QueryOptions
        {
            get
            {
                return GetValueOrNull<ODataQueryOptions>(QueryOptionsKey);
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }

                _request.Properties[QueryOptionsKey] = value;
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
        /// Gets or sets the OData path of the request.
        /// </summary>
        public ODataPath Path
        {
            get
            {
                return GetValueOrNull<ODataPath>(PathKey);
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
                    // TotalCount is defined as null-able long, allowed to be set as null, so it should be
                    // safe to cast to null-able type of long as result.
                    return (long?)totalCount;
                }

                if (this.TotalCountFunc != null)
                {
                    long count = this.TotalCountFunc();
                    _request.Properties[TotalCountKey] = count;
                    return count;
                }

                return null;
            }
            set
            {
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
        /// Gets or sets the delta link for the OData response.
        /// </summary>
        /// <value><c>null</c> if no delta link should be sent back to the client.</value>
        public Uri DeltaLink
        {
            get
            {
                return GetValueOrNull<Uri>(DeltaLinkKey);
            }
            set
            {
                _request.Properties[DeltaLinkKey] = value;
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
        /// Gets or sets the parsed OData <see cref="ApplyClause"/> of the request. The
        /// <see cref="ODataMediaTypeFormatter"/> will use this information (if any) while writing the response for
        /// this request.
        /// </summary>
        public ApplyClause ApplyClause
        {
            get
            {
                return GetValueOrNull<ApplyClause>(ApplyClauseKey);
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }

                _request.Properties[ApplyClauseKey] = value;
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
                return GetODataVersionFromHeader(_request.Headers, ODataVersionConstraint.ODataServiceVersionHeader);
            }
        }

        internal ODataVersion? ODataMaxServiceVersion
        {
            get
            {
                return GetODataVersionFromHeader(_request.Headers, ODataVersionConstraint.ODataMaxServiceVersionHeader);
            }
        }

        internal ODataVersion? ODataMinServiceVersion
        {
            get
            {
                return GetODataVersionFromHeader(_request.Headers, ODataVersionConstraint.ODataMinServiceVersionHeader);
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
