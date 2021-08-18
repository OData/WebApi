//-----------------------------------------------------------------------------
// <copyright file="ApplyQueryOption.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// This defines a $apply OData query option for querying.
    /// </summary>
    public class ApplyQueryOption
    {
        private ApplyClause _applyClause;
        private ODataQueryOptionParser _queryOptionParser;

        /// <summary>
        /// Initialize a new instance of <see cref="ApplyQueryOption"/> based on the raw $apply value and
        /// an EdmModel from <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="rawValue">The raw value for $filter query. It can be null or empty.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        /// <param name="queryOptionParser">The <see cref="ODataQueryOptionParser"/> which is used to parse the query option.</param>
        public ApplyQueryOption(string rawValue, ODataQueryContext context, ODataQueryOptionParser queryOptionParser)
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
            // TODO: Implement and add validator
            //Validator = new FilterQueryValidator();
            _queryOptionParser = queryOptionParser;
            ResultClrType = Context.ElementClrType;
        }

        /// <summary>
        ///  Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; private set; }

        /// <summary>
        /// ClrType for result of transformations
        /// </summary>
        public Type ResultClrType { get; private set; }

        /// <summary>
        /// Gets the parsed <see cref="ApplyClause"/> for this query option.
        /// </summary>
        public ApplyClause ApplyClause
        {
            get
            {
                if (_applyClause == null)
                {
                    _applyClause = _queryOptionParser.ParseApply();
                }

                return _applyClause;
            }
        }

        /// <summary>
        ///  Gets the raw $apply value.
        /// </summary>
        public string RawValue { get; private set; }

        /// <summary>
        /// Apply the apply query to the given IQueryable.
        /// </summary>
        /// <remarks>
        /// The <see cref="ODataQuerySettings.HandleNullPropagation"/> property specifies
        /// how this method should handle null propagation.
        /// </remarks>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The <see cref="ODataQuerySettings"/> that contains all the query application related settings.</param>
        /// <returns>The new <see cref="IQueryable"/> after the filter query has been applied to.</returns>
        public IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings)
        {
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }

            if (querySettings == null)
            {
                throw Error.ArgumentNull("querySettings");
            }

            if (Context.ElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "ApplyTo");
            }

            // Linq to SQL not supported for $apply
            if (query.Provider.GetType().Namespace == HandleNullPropagationOptionHelper.Linq2SqlQueryProviderNamespace)
            {
                throw Error.NotSupported(SRResources.ApplyQueryOptionNotSupportedForLinq2SQL);
            }

            ApplyClause applyClause = ApplyClause;
            Contract.Assert(applyClause != null);

            ODataQuerySettings updatedSettings = Context.UpdateQuerySettings(querySettings, query);

            // The IWebApiAssembliesResolver service is internal and can only be injected by WebApi.
            // This code path may be used in cases when the service container is not available
            // and the service container is available but may not contain an instance of IWebApiAssembliesResolver.
            IWebApiAssembliesResolver assembliesResolver = WebApiAssembliesResolver.Default;
            if (Context.RequestContainer != null)
            { 
                IWebApiAssembliesResolver injectedResolver = Context.RequestContainer.GetService<IWebApiAssembliesResolver>();
                if (injectedResolver != null)
                {
                    assembliesResolver = injectedResolver;
                }
            }

            foreach (var transformation in applyClause.Transformations)
            {
                if (transformation.Kind == TransformationNodeKind.Aggregate || transformation.Kind == TransformationNodeKind.GroupBy)
                {
                    var binder = new AggregationBinder(updatedSettings, assembliesResolver, ResultClrType, Context.Model, transformation);
                    query = binder.Bind(query);
                    this.ResultClrType = binder.ResultClrType;
                }
                else if (transformation.Kind == TransformationNodeKind.Compute)
                {
                    var binder = new ComputeBinder(updatedSettings, assembliesResolver, ResultClrType, Context.Model, (ComputeTransformationNode)transformation);
                    query = binder.Bind(query);
                    this.ResultClrType = binder.ResultClrType;
                }
                else if (transformation.Kind == TransformationNodeKind.Filter)
                {
                    var filterTransformation = transformation as FilterTransformationNode;
                    Expression filter = FilterBinder.Bind(query, filterTransformation.FilterClause, ResultClrType, Context, querySettings);
                    query = ExpressionHelpers.Where(query, filter, ResultClrType);
                }
            }

            return query;
        }
    }
}
