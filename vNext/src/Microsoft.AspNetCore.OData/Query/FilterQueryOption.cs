using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Properties;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Query
{
    // TODO: Replace with full version in the future.
    /// <summary>
    /// This defines a $filter OData query option for querying.
    /// </summary>
    public class FilterQueryOption
    {
        private FilterClause _filterClause;
        private readonly ODataQueryOptionParser _queryOptionParser;

        /// <summary>
        /// Initialize a new instance of <see cref="FilterQueryOption"/> based on the raw $filter value and 
        /// an EdmModel from <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="rawValue">The raw value for $filter query. It can be null or empty.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        /// <param name="queryOptionParser">The <see cref="ODataQueryOptionParser"/> which is used to parse the query option.</param>
        public FilterQueryOption(string rawValue, ODataQueryContext context, ODataQueryOptionParser queryOptionParser)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (String.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty("rawValue");
            }

            if (queryOptionParser == null)
            {
                throw Error.ArgumentNull("queryOptionParser");
            }

            Context = context;
            RawValue = rawValue;
            _queryOptionParser = queryOptionParser;
        }

        /// <summary>
        ///  Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; private set; }

        /// <summary>
        /// Gets the parsed <see cref="FilterClause"/> for this query option.
        /// </summary>
        public FilterClause FilterClause
        {
            get
            {
                if (_filterClause == null)
                {
                    _filterClause = _queryOptionParser.ParseFilter();
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
	    /// <param name="query">The original <see cref="IQueryable"/>.</param>
	    /// <param name="querySettings">The <see cref="ODataQuerySettings"/> that contains all the query application related settings.</param>
	    /// <param name="assemblyNames">The assembly to use.</param>
	    /// <returns>The new <see cref="IQueryable"/> after the filter query has been applied to.</returns>
	    public IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings, AssemblyNames assemblyNames)
        {
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }
            
            if (assemblyNames == null)
            {
                throw Error.ArgumentNull("assemblyNames");
            }

            if (Context.ElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "ApplyTo");
            }
            
            var filter = FilterBinder.Bind(FilterClause, Context.ElementClrType, Context.Model, assemblyNames, querySettings);
            return ExpressionHelpers.Where(query, filter, Context.ElementClrType);
        }
    }
}