// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData.Query.Expressions;
using System.Web.Http.OData.Query.Validators;
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
        private static readonly IAssembliesResolver _defaultAssembliesResolver = new DefaultAssembliesResolver();
        private FilterClause _filterClause;
    
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

            if (String.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty("rawValue");
            }

            Context = context;
            RawValue = rawValue;
            Validator = new FilterQueryValidator();
        }

        /// <summary>
        ///  Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; private set; }

        /// <summary>
        /// Gets or sets the Filter Query Validator
        /// </summary>
        public FilterQueryValidator Validator { get; set; }
       
        /// <summary>
        /// Gets the parsed <see cref="FilterClause"/> for this query option.
        /// </summary>
        public FilterClause FilterClause
        {
            get
            {
                if (_filterClause == null)
                {
                    _filterClause = ODataUriParser.ParseFilter(RawValue, Context.Model, Context.ElementType);
                }

                return _filterClause;
            }
        }

        /// <summary>
        ///  Gets the raw $filter value.
        /// </summary>
        public string RawValue { get; private set; }

        /// <summary>
        /// Apply the filter query to the given IQueryable.
        /// </summary>
        /// <remarks>
        /// The <see cref="ODataQuerySettings.HandleNullPropagation"/> property specifies
        /// how this method should handle null propagation.
        /// </remarks>
        /// <param name="query">The IQueryable that we are applying filter query against.</param>
        /// <param name="querySettings">Specifies if we need to handle null propagation. Pass false if the underlying query provider handles null propagation. Otherwise pass true.</param>
        /// <returns>The query that the filter query has been applied to.</returns>
        public IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings)
        {
            return ApplyTo(query, querySettings, _defaultAssembliesResolver);
        }

        /// <summary>
        /// Apply the filter query to the given IQueryable.
        /// </summary>
        /// <remarks>
        /// The <see cref="ODataQuerySettings.HandleNullPropagation"/> property specifies
        /// how this method should handle null propagation.
        /// </remarks>
        /// <param name="query">The IQueryable that we are applying filter query against.</param>
        /// <param name="querySettings">Specifies if we need to handle null propagation. Pass false if the underlying query provider handles null propagation. Otherwise pass true.</param>
        /// <param name="assembliesResolver">The <see cref="IAssembliesResolver"/> to use.</param>
        /// <returns>The query that the filter query has been applied to.</returns>
        public IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings, IAssembliesResolver assembliesResolver)
        {
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }

            if (querySettings == null)
            {
                throw Error.ArgumentNull("querySettings");
            }

            if (assembliesResolver == null)
            {
                throw Error.ArgumentNull("assembliesResolver");
            }

            FilterClause filterClause = FilterClause;
            Contract.Assert(filterClause != null);

            Expression filter = FilterBinder.Bind(filterClause, Context.ElementClrType, Context.Model, assembliesResolver, querySettings);
            query = ExpressionHelpers.Where(query, filter, Context.ElementClrType);
            return query;
        }

        public void Validate(ODataValidationSettings validationSettings)
        {
            if (validationSettings == null)
            {
                throw Error.ArgumentNull("validationSettings");
            }

            if (Validator != null)
            {
                Validator.Validate(this, validationSettings);
            }
        }
    }
}
