﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using Microsoft.OData.Edm;

namespace System.Web.OData.Query
{
    /// <summary>
    /// This defines a composite OData query options that can be used to perform query composition.
    /// Currently this only supports $filter, $orderby, $top, $skip.
    /// </summary>
    [ODataQueryParameterBinding]
    public class ODataQueryOptions<TEntity> : ODataQueryOptions
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
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <returns>The new <see cref="IQueryable"/> after the query has been applied to.</returns>
        public override IQueryable ApplyTo(IQueryable query)
        {
            ValidateQuery(query);
            return base.ApplyTo(query);
        }

        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The settings to use in query composition.</param>
        /// <returns>The new <see cref="IQueryable"/> after the query has been applied to.</returns>
        public override IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings)
        {
            ValidateQuery(query);
            return base.ApplyTo(query, querySettings);
        }

        internal override ETag GetETag(EntityTagHeaderValue etagHeaderValue)
        {
            return Request.GetETag<TEntity>(etagHeaderValue);
        }

        private static void ValidateQuery(IQueryable query)
        {
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }

            if (!typeof(TEntity).IsAssignableFrom(query.ElementType))
            {
                throw Error.Argument("query", SRResources.CannotApplyODataQueryOptionsOfT, typeof(ODataQueryOptions).Name, typeof(TEntity).FullName, typeof(IQueryable).Name, query.ElementType.FullName);
            }
        }
    }
}
