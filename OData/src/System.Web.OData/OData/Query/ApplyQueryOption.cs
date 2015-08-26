using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Properties;
using System.Web.OData.Query.Expressions;

namespace System.Web.OData.Query
{
    /// <summary>
    /// This defines a $apply OData query option for querying.
    /// </summary>
    public class ApplyQueryOption
    {
        private static readonly IAssembliesResolver _defaultAssembliesResolver = new DefaultAssembliesResolver();
        private ApplyClause _applyClause;
        private ODataQueryOptionParser _queryOptionParser;


        /// <summary>
        /// Initialize a new instance of <see cref="ApplyQueryOption"/> based on the raw $filter value and 
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
        }

        // This constructor is intended for unit testing only.
        internal ApplyQueryOption(string rawValue, ODataQueryContext context)
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
            //Validator = new FilterQueryValidator();
            _queryOptionParser = new ODataQueryOptionParser(
                context.Model,
                context.ElementType,
                context.NavigationSource,
                new Dictionary<string, string> { { "$apply", rawValue } });
        }

        /// <summary>
        ///  Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; private set; }


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
                    // TODO: After refactoring to QueryNodes re-thingk do we need that part.
                    //SingleValueNode filterExpression = _applyClause.Expression.Accept(
                    //    new ParameterAliasNodeTranslator(_queryOptionParser.ParameterAliasNodes)) as SingleValueNode;
                    //filterExpression = filterExpression ?? new ConstantNode(null);
                    //_applyClause = new ApplyClause(filterExpression, _applyClause.RangeVariable);
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
            return ApplyTo(query, querySettings, _defaultAssembliesResolver);
        }


        /// <summary>
        /// Apply the apply query to the given IQueryable.
        /// </summary>
        /// <remarks>
        /// The <see cref="ODataQuerySettings.HandleNullPropagation"/> property specifies
        /// how this method should handle null propagation.
        /// </remarks>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The <see cref="ODataQuerySettings"/> that contains all the query application related settings.</param>
        /// <param name="assembliesResolver">The <see cref="IAssembliesResolver"/> to use.</param>
        /// <returns>The new <see cref="IQueryable"/> after the filter query has been applied to.</returns>
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
            if (Context.ElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "ApplyTo");
            }

            ApplyClause applyClause = ApplyClause;
            Contract.Assert(applyClause != null);

            // All following code is just PoC 
            // Ensure we have decided how to handle null propagation
            ODataQuerySettings updatedSettings = querySettings;
            if (querySettings.HandleNullPropagation == HandleNullPropagationOption.Default)
            {
                updatedSettings = new ODataQuerySettings(updatedSettings);
                updatedSettings.HandleNullPropagation = HandleNullPropagationOptionHelper.GetDefaultHandleNullPropagationOption(query);
            }

            // Hardcoded test support for aggregation sum.
            // It must be properly implmeneted and moved to odata.net as a binder
            if (applyClause.Transformations.Count != 1)
            {
                throw new NotImplementedException("Only one transformation is supported");
            }
            var transformation = applyClause.Transformations.First().Item2 as ApplyAggregateClause;
            if (transformation == null)
            {
                throw new NotImplementedException("Only aggregation transformation is supported");
            }

            var propertyName = transformation.AggregatableProperty;
            var aggProperty = ExpressionHelpers.GetPropertyAccessLambda(Context.ElementClrType, propertyName);

            Type elementType = Context.ElementClrType;
            ParameterExpression source = Expression.Parameter(elementType);

            Type wrapperType = typeof(AggregationWrapper<>).MakeGenericType(elementType);
            ParameterExpression accum = Expression.Parameter(wrapperType);
            MethodInfo getPropMethod = wrapperType.GetMethod("GetProperty");

            ConstructorInfo wrapperConstructor = wrapperType.GetConstructor(new Type[] { typeof(string), typeof(int) });

            NewExpression init = Expression.New(wrapperConstructor, Expression.Constant(transformation.Alias), Expression.Constant(0));

            var starter = wrapperConstructor.Invoke(new object[] { transformation.Alias, 0 });

            var valuePropAccess = Expression.Property(source, propertyName);
            var accumPropAccess = Expression.Call(accum, getPropMethod, Expression.Constant(transformation.Alias));


            //step = { (Param_0, Param_1) => new AggregationWrapper`1("Alias", (Param_1.Amount + Param_0.GetProperty("Alias")))}
            LambdaExpression step = Expression.Lambda(
                    Expression.New(wrapperConstructor, Expression.Constant(transformation.Alias), Expression.Add(valuePropAccess, accumPropAccess)),
                    accum,
                    source);

            

            var result = ExpressionHelpers.Aggregate(query, starter, step, Context.ElementClrType, wrapperType);

            return result;
        }
    }

}
