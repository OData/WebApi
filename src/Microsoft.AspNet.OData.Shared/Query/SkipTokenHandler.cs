// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Represents how NextLink for paging is generated.
    /// </summary>
    public abstract class SkipTokenHandler
    {
        /// <summary>
        /// Process SkipToken Value to create string key - object value collection 
        /// </summary>
        /// <param name="rawValue"></param>
        public abstract IDictionary<string, object> ProcessSkipTokenValue(string rawValue);

        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <param name="orderByNodes">Information about the orderby query option.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skiptoken query has been applied to.</returns>
        public abstract IQueryable<T> ApplyTo<T>(IQueryable<T> query, ODataQuerySettings querySettings, IList<OrderByNode> orderByNodes);

        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <param name="orderByNodes">Information about the orderby query option.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skiptoken query has been applied to.</returns>
        public abstract IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings, IList<OrderByNode> orderByNodes);
       
        /// <summary>
        /// Returns the URI for NextPageLink
        /// </summary>
        /// <param name="lastMember"> Object based on which SkipToken value will be generated.</param>
        /// <param name="context">Serializer context</param>
        /// <returns></returns>
        public abstract Uri GenerateNextPageLink(Object lastMember, ODataSerializerContext context);

        /// <summary>
        /// Returns a function that converts an object to a skiptoken value string
        /// </summary>
        /// <param name="lastMember"> Object based on which SkipToken value will be generated.</param>
        /// <param name="model">The edm model.</param>
        /// <param name="orderByNodes">QueryOption </param>
        /// <returns>The value for skiptoken query parameter</returns>
        public abstract string GenerateSkipTokenValue(Object lastMember, IEdmModel model, IList<OrderByNode> orderByNodes);

        /// <summary>
        /// Gets and sets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; set; }
    }
}
