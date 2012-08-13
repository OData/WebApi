// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http.OData.Query.Expressions;
using Microsoft.Data.Edm;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;

namespace System.Web.Http.OData.Query
{
    /// <summary>
    /// This defines a $filter OData query option that can be used to perform query composition. 
    /// </summary>
    public class FilterQueryOption
    {
        private FilterQueryNode _queryNode;

        /// <summary>
        /// Initialize a new instance of <see cref="FilterQueryOption"/> based on the raw $filter value and 
        /// an EdmModel from <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="rawValue">The raw value for $filter query. It can be null or empty.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        public FilterQueryOption(string rawValue, ODataQueryContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (string.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty("rawValue");
            }

            Context = context;
            RawValue = rawValue;
        }

        /// <summary>
        ///  Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; private set; }

        /// <summary>
        /// Gets the <see cref="FilterQueryNode"/> for this query option.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "TODO: remove this when implement TODO below")]
        public FilterQueryNode QueryNode
        {
            get
            {
                if (_queryNode == null)
                {
                    // TODO: Bug 462293: 
                    //  -   we should be using the real uri
                    //  -   we should be parsing the whole URL once (including all query options) and using what we get from that
                    //      but the UriParser need to change from a linked list style tree (semantic ordering of operations pre-applied) to 
                    //      a flower style tree first. So for now we are rebuilding the just part of the Uri that is important for parsing $filter.
                    Uri fakeServiceRootUri = new Uri("http://server/");
                    Uri fakeQueryOptionsUri = new Uri(fakeServiceRootUri, string.Format(CultureInfo.InvariantCulture, "{0}/?$filter={1}", Context.EntitySet.Name, RawValue));
                    SemanticTree semanticTree = SemanticTree.ParseUri(fakeQueryOptionsUri, fakeServiceRootUri, Context.Model);
                    _queryNode = semanticTree.Query as FilterQueryNode;
                }
                return _queryNode;
            }
        }

        /// <summary>
        ///  Gets the raw $filter value.
        /// </summary>
        public string RawValue { get; private set; }

        /// <summary>
        /// Apply the filter query to the given IQueryable.
        /// </summary>
        /// <param name="query">The IQueryable that we are applying filter query against.</param>
        /// <param name="handleNullPropagation">Specifies if we need to handle null propagation. Pass false if the underlying query provider handles null propagation. Otherwise pass true.</param>
        /// <returns>The query that the filter query has been applied to.</returns>
        public IQueryable ApplyTo(IQueryable query, bool handleNullPropagation)
        {
            FilterQueryNode node = QueryNode;
            Contract.Assert(node != null);

            Expression filter = FilterBinder.Bind(node, Context.EntityClrType, Context.Model, handleNullPropagation);
            query = ExpressionHelpers.Where(query, filter, Context.EntityClrType);
            return query;
        }
    }
}
