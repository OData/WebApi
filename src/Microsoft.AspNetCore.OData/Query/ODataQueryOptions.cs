//-----------------------------------------------------------------------------
// <copyright file="ODataQueryOptions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.Contracts;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// This defines a composite OData query options that can be used to perform query composition.
    /// Currently this only supports $filter, $orderby, $top, $skip, and $count.
    /// </summary>
    [NonValidatingParameterBinding]
    public partial class ODataQueryOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQueryOptions"/> class based on the incoming request and some metadata information from
        /// the <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information.</param>
        /// <param name="request">The incoming request message.</param>
        /// <remarks>
        /// This signature uses types that are AspNetCore-specific.
        /// While the AspNet version of this class makes the HttpRequest available, AspNetCore
        /// is unhappy when it sees the HttpRequest during validation so HttpRequest is not part
        /// of the public Api for ODataQueryOptions.
        /// </remarks>
        public ODataQueryOptions(ODataQueryContext context, HttpRequest request)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            Contract.Assert(context.RequestContainer == null);
            context.RequestContainer = request.GetRequestContainer();

            Context = context;
            Request = request;
            InternalRequest = new WebApiRequestMessage(request);
            InternalHeaders = new WebApiRequestHeaders(request.Headers);

            Initialize(context);
        }

        /// <summary>
        /// Gets the request message associated with this instance.
        /// </summary>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public HttpRequest Request { get; private set; }
    }
}
