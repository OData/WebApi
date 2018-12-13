// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Interfaces
{
    /// <summary>
    /// Allows for custom implementations of SkipToken with a custom format and application specific filtering.
    /// </summary>
    public interface ISkipTokenImplementation
    {
        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <param name="orderByNodes">Information about the orderby query option.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skip query has been applied to.</returns>
        IQueryable<T> ApplyTo<T>(IQueryable<T> query, ODataQuerySettings querySettings, IList<OrderByNode> orderByNodes);

        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <param name="orderByNodes">Information about the orderby query option.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skip query has been applied to.</returns>
        IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings, IList<OrderByNode> orderByNodes);

        /// <summary>
        /// Returns a function that converts an object to a skiptoken value string
        /// </summary>
        /// <param name="lastMember">Object based on which the value of the skiptoken is generated.</param>
        /// <param name="model">The edm model.</param>
        /// <param name="orderByNodes">QueryOption</param>
        /// <returns></returns>
        string GenerateSkipTokenValue(object lastMember, IEdmModel model, IList<OrderByNode> orderByNodes);

        /// <summary>
        /// Hook for processing skiptoken value, it gets invoked when the query option is created.
        /// </summary>
        /// <param name="rawValue"></param>
        void ProcessSkipTokenValue(string rawValue);

        /// <summary>
        /// Gets and sets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        ODataQueryContext Context { get; set; }
    }
}
