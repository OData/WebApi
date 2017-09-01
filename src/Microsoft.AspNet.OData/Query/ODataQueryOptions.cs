// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// This defines a composite OData query options that can be used to perform query composition.
    /// Currently this only supports $filter, $orderby, $top, $skip, and $count.
    /// </summary>
    ////[ODataQueryParameterBinding]
    public partial class ODataQueryOptions
    {
        private ETag _etagIfMatch;

        private bool _etagIfMatchChecked;

        private ETag _etagIfNoneMatch;

        private bool _etagIfNoneMatchChecked;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQueryOptions"/> class based on the incoming request and some metadata information from
        /// the <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information.</param>
        /// <param name="request">The incoming request message.</param>
        public ODataQueryOptions(ODataQueryContext context, HttpRequestMessage request)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            // Set the request container into context
            Contract.Assert(context.RequestContainer == null);
            context.RequestContainer = request.GetRequestContainer();

            // Remember the context and request
            Context = context;
            Request = request;
            InternalRequest = new WebApiRequestMessage(request);

            // Parse the query from request Uri, including only keys which are OData query parameters or parameter alias
            RawValues = new ODataRawQueryOptions();
            IDictionary<string, string> queryParameters = GetODataQueryParameters();

            _queryOptionParser = new ODataQueryOptionParser(
                context.Model,
                context.ElementType,
                context.NavigationSource,
                queryParameters);

            _queryOptionParser.Resolver = request.GetRequestContainer().GetRequiredService<ODataUriResolver>();

            BuildQueryOptions(queryParameters);

            Validator = ODataQueryValidator.GetODataQueryValidator(context);
        }

        /// <summary>
        /// Gets the request message associated with this instance.
        /// </summary>
        public HttpRequestMessage Request { get; private set; }

        /// <summary>
        /// Gets the <see cref="ETag"/> from IfMatch header.
        /// </summary>
        public virtual ETag IfMatch
        {
            get
            {
                if (!_etagIfMatchChecked && _etagIfMatch == null)
                {
                    EntityTagHeaderValue etagHeaderValue = Request.Headers.IfMatch.SingleOrDefault();
                    _etagIfMatch = GetETag(etagHeaderValue);
                    _etagIfMatchChecked = true;
                }

                return _etagIfMatch;
            }
        }

        /// <summary>
        /// Gets the <see cref="ETag"/> from IfNoneMatch header.
        /// </summary>
        public virtual ETag IfNoneMatch
        {
            get
            {
                if (!_etagIfNoneMatchChecked && _etagIfNoneMatch == null)
                {
                    EntityTagHeaderValue etagHeaderValue = Request.Headers.IfNoneMatch.SingleOrDefault();
                    _etagIfNoneMatch = GetETag(etagHeaderValue);
                    if (_etagIfNoneMatch != null)
                    {
                        _etagIfNoneMatch.IsIfNoneMatch = true;
                    }
                    _etagIfNoneMatchChecked = true;
                }

                return _etagIfNoneMatch;
            }
        }

        /// <summary>
        /// Gets the EntityTagHeaderValue ETag.
        /// </summary>
        internal virtual ETag GetETag(EntityTagHeaderValue etagHeaderValue)
        {
            return Request.GetETag(etagHeaderValue);
        }
    }
}
