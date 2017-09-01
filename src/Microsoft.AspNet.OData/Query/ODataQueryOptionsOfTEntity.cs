// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// This defines a composite OData query options that can be used to perform query composition.
    /// Currently this only supports $filter, $orderby, $top, $skip.
    /// </summary>
    public partial class ODataQueryOptions<TEntity> : ODataQueryOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQueryOptions"/> class based on the incoming request and some metadata information from
        /// the <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        /// <param name="request">The incoming request message</param>
        public ODataQueryOptions(ODataQueryContext context, HttpRequestMessage request)
            : base(context, request)
        {
            if (Context.ElementClrType == null)
            {
                throw Error.Argument("context", SRResources.ElementClrTypeNull, typeof(ODataQueryContext).Name);
            }

            if (context.ElementClrType != typeof(TEntity))
            {
                throw Error.Argument("context", SRResources.EntityTypeMismatch, context.ElementClrType.FullName, typeof(TEntity).FullName);
            }
        }

        /// <summary>
        /// Gets the <see cref="ETag{TEntity}"/> from IfMatch header.
        /// </summary>
        public new ETag<TEntity> IfMatch
        {
            get
            {
                return base.IfMatch as ETag<TEntity>;
            }
        }

        /// <summary>
        /// Gets the <see cref="ETag{TEntity}"/> from IfNoneMatch header.
        /// </summary>
        public new ETag<TEntity> IfNoneMatch
        {
            get
            {
                return base.IfNoneMatch as ETag<TEntity>;
            }
        }

        /// <summary>
        /// Gets the EntityTagHeaderValue ETag>.
        /// </summary>
        internal override ETag GetETag(EntityTagHeaderValue etagHeaderValue)
        {
            return Request.GetETag<TEntity>(etagHeaderValue);
        }
    }
}
